using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Yunchang
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public class RendererFeatures : MonoBehaviour
    {
        Camera m_Camera;

        CommandBuffer cb;
        RenderTexture m_colorTex;
        RenderTexture m_depthTex;

        void OnEnable()
        {
            m_Camera = this.GetComponent<Camera>();

            cb = new CommandBuffer();
            m_Camera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, cb);

            m_colorTex = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
            m_colorTex.Create();
            m_depthTex = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.Depth);
            m_depthTex.Create();
            m_Camera.SetTargetBuffers(m_colorTex.colorBuffer, m_depthTex.depthBuffer);
        }

        void OnDisable()
        {
            m_Camera.targetTexture = null;
            m_Camera.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, cb);
            cb.Dispose();
            m_colorTex.Release();
            m_depthTex.Release();
        }

        void Update()
        {
            cb.Clear();
            cb.SetGlobalTexture("_CameraDepthTexture", m_depthTex);
        }
    }
}
