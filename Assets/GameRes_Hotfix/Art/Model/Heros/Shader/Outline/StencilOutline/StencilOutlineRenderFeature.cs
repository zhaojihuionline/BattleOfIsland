using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class StencilOutlineRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class StencilOutlineSettings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        public LayerMask outlineLayer = -1;
        public Material outlineMaterial;
        public int outlineRenderingLayerMask = 0;
        public float outlineWidth = 1.0f;
        public Color outlineColor = Color.yellow;
        public bool enableFog = true;
        public int stencilRef = 1;
    }

    public StencilOutlineSettings settings = new StencilOutlineSettings();
    private CombinedOutlinePass outlinePass;

    public override void Create()
    {
        outlinePass = new CombinedOutlinePass("Stencil Outline", settings);
        outlinePass.renderPassEvent = settings.renderPassEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.outlineMaterial == null)
        {
            Debug.LogWarning("Outline material is missing");
            return;
        }
        
        // 设置描边材质的属性
        settings.outlineMaterial.SetFloat("_OutlineWidth", settings.outlineWidth);
        settings.outlineMaterial.SetColor("_OutlineColor", settings.outlineColor);
        settings.outlineMaterial.SetFloat("_EnableFog", settings.enableFog ? 1.0f : 0.0f);
        settings.outlineMaterial.SetInt("_StencilRef", settings.stencilRef);
        
        // 添加合并后的Pass
        renderer.EnqueuePass(outlinePass);
    }

    protected override void Dispose(bool disposing)
    {
        outlinePass?.Dispose();
    }
}


public class CombinedOutlinePass : ScriptableRenderPass
{
    private static readonly ShaderTagId k_ShaderTagId = new ShaderTagId("UniversalForward");
    private static readonly string k_ProfilerTag = "Stencil Outline";
    
    private string profilerTag;
    private StencilOutlineRenderFeature.StencilOutlineSettings settings;
    private FilteringSettings filteringSettings;
    private RenderStateBlock stencilWriteStateBlock;
    private RenderStateBlock stencilTestStateBlock;
    private ProfilingSampler profilingSampler;
    
    public CombinedOutlinePass(string profilerTag, StencilOutlineRenderFeature.StencilOutlineSettings settings)
    {
        this.profilerTag = profilerTag;
        this.settings = settings;
        
        filteringSettings = new FilteringSettings(RenderQueueRange.opaque, settings.outlineLayer);
        
        // 设置写入模板缓冲区的状态
        stencilWriteStateBlock = new RenderStateBlock(RenderStateMask.Stencil);
        stencilWriteStateBlock.stencilReference = settings.stencilRef;
        stencilWriteStateBlock.stencilState = new StencilState(
            true,
            (byte)settings.stencilRef,
            (byte)255,
            CompareFunction.Always,
            StencilOp.Replace,
            StencilOp.Keep,
            StencilOp.Keep
        );
        
        // 设置测试模板缓冲区的状态
        stencilTestStateBlock = new RenderStateBlock(RenderStateMask.Stencil);
        stencilTestStateBlock.stencilReference = settings.stencilRef;
        stencilTestStateBlock.stencilState = new StencilState(
            true,
            (byte)settings.stencilRef,
            (byte)255,
            CompareFunction.NotEqual,
            StencilOp.Keep,
            StencilOp.Keep,
            StencilOp.Keep
        );
        
        profilingSampler = new ProfilingSampler(k_ProfilerTag);
    }
    
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (settings.outlineMaterial == null)
            return;
            
        CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
        
        using (new ProfilingScope(cmd, profilingSampler))
        {
            // 设置渲染层掩码
            uint renderingLayerMask = (uint)(1 << settings.outlineRenderingLayerMask);
            filteringSettings.renderingLayerMask = renderingLayerMask;
            
            // 第一步 写入模板缓冲区
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            
            DrawingSettings normalDrawingSettings = CreateDrawingSettings(
                k_ShaderTagId, 
                ref renderingData, 
                SortingCriteria.CommonOpaque
            );
            
            // 绘制到模板缓冲区
            context.DrawRenderers(renderingData.cullResults, ref normalDrawingSettings, 
                                 ref filteringSettings, ref stencilWriteStateBlock);
            
            // 第二步 使用模板测试绘制描边
            DrawingSettings outlineDrawingSettings = CreateDrawingSettings(
                k_ShaderTagId, 
                ref renderingData, 
                SortingCriteria.CommonOpaque
            );
            outlineDrawingSettings.overrideMaterial = settings.outlineMaterial;
            outlineDrawingSettings.overrideMaterialPassIndex = 0;
            
            // 绘制描边
            context.DrawRenderers(renderingData.cullResults, ref outlineDrawingSettings, 
                                 ref filteringSettings, ref stencilTestStateBlock);
        }
        
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public void Dispose() { }
}