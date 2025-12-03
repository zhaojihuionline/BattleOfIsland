using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OutlineRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class OutlineSettings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        public LayerMask outlineLayer = -1;
        public Material outlineMaterial;
        public int outlineRenderingLayerMask = 0; 
        public float outlineWidth = 1.0f;
        public Color outlineColor = Color.yellow;
        public bool enableFog = true;
    }

    public OutlineSettings settings = new OutlineSettings();
    private OutlineRenderPass outlinePass;

    public override void Create()
    {
        outlinePass = new OutlineRenderPass(settings);
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
        
        outlinePass.ConfigureInput(ScriptableRenderPassInput.Depth);
        outlinePass.renderPassEvent = settings.renderPassEvent;
        renderer.EnqueuePass(outlinePass);
    }
}