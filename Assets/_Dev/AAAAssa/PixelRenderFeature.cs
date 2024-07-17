using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PixelRenderFeature : ScriptableRendererFeature
{
    [SerializeField] private Material material;
    PostRenderPixelPass scriptablePass;

    public override void Create()
    {
        scriptablePass = new PostRenderPixelPass(material);

        // Configures where the render pass should be injected
        scriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera (every frame!)
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        scriptablePass.ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal);
        renderer.EnqueuePass(scriptablePass);
    }
}