using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VolumetricCloudFeature : ScriptableRendererFeature
{
    public Material rayMarchingCloudMat;
    public Material atmosphereMaterial;

    private VolumetricCloudPass m_volumetricCloudPass;
    private AtmospherePass m_atmospherePass;

    private RenderTargetIdentifier m_colorAttachment;

    private bool EnsureMaterials()
    {
        return (rayMarchingCloudMat != null && atmosphereMaterial != null);
    }

    public override void Create()
    {
        if (!EnsureMaterials())
            return;

        m_atmospherePass = new AtmospherePass(RenderPassEvent.AfterRenderingSkybox, atmosphereMaterial);
        m_volumetricCloudPass = new VolumetricCloudPass(RenderPassEvent.AfterRenderingTransparents, rayMarchingCloudMat);

        m_colorAttachment = new RenderTargetIdentifier("_CameraColorAttachmentA");

        atmosphereMaterial.SetFloat("_AtmosphereExposure", 0.1f);
        atmosphereMaterial.SetFloat("_AtmosphereHeight", 100000.0f);
        atmosphereMaterial.SetFloat("_MieG", 0.85f);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (!EnsureMaterials())
            return;

        m_atmospherePass.Setup(m_colorAttachment);
        m_volumetricCloudPass.Setup();

        renderer.EnqueuePass(m_atmospherePass);
        renderer.EnqueuePass(m_volumetricCloudPass);
    }
}
