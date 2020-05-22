using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Yunchang
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public class RendererFeatures : MonoBehaviour
    {
        Camera m_Camera;

        RenderTexture m_ColorTex;
        RenderTexture m_DepthTex;
        RenderTexture m_DepthCopyTex;

        Material m_BlitCopyDepthMat;

        CommandBuffer m_CopyDepthPass;
        CommandBuffer m_FinelPass;
        
        void OnEnable()
        {
            m_Camera = this.GetComponent<Camera>();
            //m_Camera.depthTextureMode = DepthTextureMode.Depth;

            m_ColorTex = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
            m_ColorTex.Create();
            m_DepthTex = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.Depth);
            m_DepthTex.Create();
            m_Camera.SetTargetBuffers(m_ColorTex.colorBuffer, m_DepthTex.depthBuffer);

            Shader.SetGlobalTexture("_CameraDepthTexture", m_DepthTex);

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

        void OnDisable()
        {
            m_Camera.targetTexture = null;
            m_Camera.RemoveCommandBuffer(CameraEvent.AfterImageEffects, m_FinelPass);
            m_ColorTex.Release();
            m_DepthTex.Release();
            m_FinelPass.Dispose();

            if(m_DepthCopyTex != null)
            {
                m_Camera.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, m_CopyDepthPass);
                m_DepthCopyTex.Release();
                m_CopyDepthPass.Dispose();
                Object.DestroyImmediate(m_BlitCopyDepthMat);
            }
        }

        void Update()
        {
            //decal.GetComponent<Renderer>().material.SetTexture("_DepthTex",m_DepthTex);
        }

        void OnPreRender()
        {
            //m_Camera.AddCommandBuffer(cameraEvent, m_FinelPass);
        }

        void OnPostRender()
        {
            //m_Camera.RemoveCommandBuffer(cameraEvent, m_FinelPass);
        }
    }
}
