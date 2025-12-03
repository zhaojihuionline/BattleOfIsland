using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OutlineRenderPass : ScriptableRenderPass
{
    private static readonly string k_RenderTag = "Render Outline Objects";
    private static readonly int s_RenderingLayerMaskId = Shader.PropertyToID("_RenderingLayerMask");
    
    private OutlineRenderFeature.OutlineSettings settings;
    private FilteringSettings filteringSettings;
    private RenderStateBlock renderStateBlock;
    private ProfilingSampler profilingSampler;
    
    public OutlineRenderPass(OutlineRenderFeature.OutlineSettings settings)
    {
        this.settings = settings;
        
        // 设置过滤条件
        filteringSettings = new FilteringSettings(RenderQueueRange.opaque, settings.outlineLayer);
        
        // 设置渲染状态
        renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
        
        // 设置性能分析采样器
        profilingSampler = new ProfilingSampler(k_RenderTag);
    }
    
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (settings.outlineMaterial == null)
            return;
            
        // 获取命令缓冲区
        CommandBuffer cmd = CommandBufferPool.Get(k_RenderTag);
        
        using (new ProfilingScope(cmd, profilingSampler))
        {
            // 设置渲染层掩码 - 0代表第一层，1代表第二层
            uint renderingLayerMask = 0;
            
            // 将 outlineRenderingLayerMask 转换为实际的位掩码
            renderingLayerMask = (uint)(1 << settings.outlineRenderingLayerMask);
            
            // 设置全局渲染层掩码
            cmd.SetGlobalInt(s_RenderingLayerMaskId, (int)renderingLayerMask);
            
            // 更新过滤设置以包含渲染层掩码
            filteringSettings.layerMask = settings.outlineLayer;
            filteringSettings.renderingLayerMask = renderingLayerMask;
            
            // 绘制对象
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            
            // 设置排序标准
            SortingCriteria sortingCriteria = SortingCriteria.CommonOpaque;
            
            // 绘制设置
            DrawingSettings drawingSettings = CreateDrawingSettings(
                new ShaderTagId("UniversalForward"), 
                ref renderingData, 
                sortingCriteria
            );
            
            // 使用原始材质填充模板缓冲区
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            
            // 使用描边材质绘制
            DrawingSettings outlineSettings = CreateDrawingSettings(
                new ShaderTagId("UniversalForward"), 
                ref renderingData, 
                sortingCriteria
            );
            outlineSettings.overrideMaterial = settings.outlineMaterial;
            outlineSettings.overrideMaterialPassIndex = 0;
            
            // 执行绘制
            context.DrawRenderers(renderingData.cullResults, ref outlineSettings, ref filteringSettings, ref renderStateBlock);
        }
        
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}