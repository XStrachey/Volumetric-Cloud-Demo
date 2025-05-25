using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AtmospherePass : ScriptableRenderPass
{
    private Material _atmosphereMaterial;
    private RenderTargetHandle _atmosphereTex;
    private string _profilerTag = "Render Atmosphere Background";

    private RenderTargetIdentifier _colorAttachment;

    public RenderTargetIdentifier AtmosphereTexture => _atmosphereTex.Identifier();

    public AtmospherePass(RenderPassEvent evt, Material mat)
    {
        renderPassEvent = evt;
        _atmosphereMaterial = mat;
        _atmosphereTex.Init("_AtmosphereBackgroundTex");
    }

    public void Setup(RenderTargetIdentifier colorAttachment)
    {
        _colorAttachment = colorAttachment;
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        var desc = cameraTextureDescriptor;
        desc.colorFormat = RenderTextureFormat.ARGBHalf;
        desc.depthBufferBits = 0;

        cmd.GetTemporaryRT(_atmosphereTex.id, desc, FilterMode.Bilinear);

        ConfigureTarget(_atmosphereTex.Identifier());
        ConfigureClear(ClearFlag.None, Color.black);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get(_profilerTag);

        // 渲染到 RT
        Blit(cmd, BuiltinRenderTextureType.None, _atmosphereTex.Identifier(), _atmosphereMaterial, 0);

        // 混合进 color attachment
        Blit(cmd, _atmosphereTex.Identifier(), _colorAttachment, _atmosphereMaterial, 1);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void FrameCleanup(CommandBuffer cmd)
    {
        if (_atmosphereTex != RenderTargetHandle.CameraTarget)
            cmd.ReleaseTemporaryRT(_atmosphereTex.id);
    }
}
