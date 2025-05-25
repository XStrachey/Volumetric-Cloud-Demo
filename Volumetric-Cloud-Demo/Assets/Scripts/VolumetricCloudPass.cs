using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System;

public class VolumetricCloudPass : ScriptableRenderPass
{
    private readonly string k_RayMarchLightTag = "VolumetricLight Ray March Cloud RenderPass";

    internal Material rayMarchingCloudMat;

    private RenderTargetIdentifier m_colorAttachment;

    private RenderTargetHandle m_tmpBuffer = new RenderTargetHandle();

    // 上一次纹理分辨率
    private int m_gameViewPreWidth;
    private int m_gameViewPreHeight;
    private int m_sceneViewPreWidth;
    private int m_sceneViewPreHeight;

    // 当前帧数
    private int m_gameViewFrameCount;
    private int m_SceneViewFrameCount;

    private int m_gameViewBufferFlag;
    private int m_sceneViewBufferFlag;
    
    public VolumetricCloudPass(RenderPassEvent renderPassEvent, Material rayMarchingMaterial)
    {
        this.renderPassEvent = renderPassEvent;
        this.rayMarchingCloudMat = rayMarchingMaterial;
    }

    internal void Setup()
    {
        m_colorAttachment = new RenderTargetIdentifier("_CameraColorAttachmentA");
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        base.Configure(cmd, cameraTextureDescriptor);

        m_tmpBuffer.Init("_VolumetricCloudTex");

        RenderTextureDescriptor descriptor = cameraTextureDescriptor;
        descriptor.colorFormat = RenderTextureFormat.ARGBHalf;
        // descriptor.width >>= 1;
        // descriptor.height >>= 1;

        cmd.GetTemporaryRT(m_tmpBuffer.id, descriptor, FilterMode.Bilinear);

        // 明确配置 color attachment，防止意外写入错误 buffer
        ConfigureTarget(m_colorAttachment);
        ConfigureClear(ClearFlag.None, Color.black); // 保留已有内容
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Preview || 
            renderingData.cameraData.renderType == CameraRenderType.Overlay)
            return;

        CommandBuffer cmd = CommandBufferPool.Get(k_RayMarchLightTag);
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();

        // 设置 frame count / resolution 参数
        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            rayMarchingCloudMat.SetInt(ShaderVariables._FrameCount, m_gameViewFrameCount);
            rayMarchingCloudMat.SetVector(ShaderVariables._RenderTargetSize, new Vector2(m_gameViewPreWidth - 1, m_gameViewPreHeight - 1));
        }
        else if (renderingData.cameraData.cameraType == CameraType.SceneView)
        {
            rayMarchingCloudMat.SetInt(ShaderVariables._FrameCount, m_SceneViewFrameCount);
            rayMarchingCloudMat.SetVector(ShaderVariables._RenderTargetSize, new Vector2(m_sceneViewPreWidth - 1, m_sceneViewPreHeight - 1));
        }

        // Blit 到临时 buffer，然后再写入 color attachment
        cmd.Blit(m_colorAttachment, m_tmpBuffer.id, rayMarchingCloudMat, 0);
        cmd.Blit(m_tmpBuffer.id, m_colorAttachment, rayMarchingCloudMat, 1);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void FrameCleanup(CommandBuffer cmd)
    {
        base.FrameCleanup(cmd);
        cmd.ReleaseTemporaryRT(m_tmpBuffer.id);
    }
}
