using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class RendererBuffer : IDisposable
{
    Camera m_Camera;

    Material m_BlitCopyDepthMat;

    RenderTexture m_ColorTex;
    RenderTexture m_DepthTex;
    RenderTexture m_DepthCopyTex;

    CommandBuffer m_CopyDepthPass;
    CommandBuffer m_FinelPass;

    public RendererBuffer(Camera camera)
    {
        m_Camera = camera;
    }

    public void Create()
    {
        m_ColorTex = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
        m_ColorTex.Create();
        m_DepthTex = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.Depth);
        
        m_DepthTex.Create();
        m_Camera.SetTargetBuffers(m_ColorTex.colorBuffer, m_DepthTex.depthBuffer);
        
        m_FinelPass = new CommandBuffer { name = "Finel Pass" };
        m_FinelPass.Blit(m_ColorTex, BuiltinRenderTextureType.None);
        m_Camera.AddCommandBuffer(CameraEvent.AfterImageEffects, m_FinelPass);
        
        m_DepthCopyTex = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.RFloat);
        m_DepthCopyTex.Create();
        m_BlitCopyDepthMat = new Material(Shader.Find("Hidden/BlitCopyDepth"));
        m_CopyDepthPass = new CommandBuffer { name = "Copy Depth Pass"};
        m_CopyDepthPass.Blit(m_DepthTex, m_DepthCopyTex, m_BlitCopyDepthMat);
        m_CopyDepthPass.SetGlobalTexture("_CameraDepthTexture", m_DepthCopyTex);
        m_Camera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, m_CopyDepthPass);
    }

    public void Dispose()
    {
        m_Camera.targetTexture = null;
        if(m_FinelPass != null)
        {
            m_Camera.RemoveCommandBuffer(CameraEvent.AfterImageEffects, m_FinelPass);
            m_FinelPass.Dispose();
            m_FinelPass = null;
        }
        if(m_CopyDepthPass != null)
        {
            m_Camera.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, m_CopyDepthPass);
            m_CopyDepthPass.Dispose();
            m_CopyDepthPass = null;
        }
        if(m_ColorTex != null)
        {
            m_ColorTex.Release();
            m_ColorTex = null;
        }
        if (m_DepthTex != null)
        {
            m_DepthTex.Release();
            m_DepthTex = null;
        }
        if (m_DepthCopyTex != null)
        {
            m_DepthCopyTex.Release();
            m_DepthCopyTex = null;
        }
        if(m_BlitCopyDepthMat != null)
        {
            UnityEngine.Object.DestroyImmediate(m_BlitCopyDepthMat);
            m_BlitCopyDepthMat = null;
        }
    }
}
