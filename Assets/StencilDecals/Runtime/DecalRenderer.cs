using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Yunchang
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public class DecalRenderer : MonoBehaviour
    {
        /// <summary>
        /// 是否支持法线
        /// </summary>
        public bool supportNormal = false;

        Camera m_Camera;

        RendererBuffer rendererBuffer;

        void OnValidate()
        {
            OnDisable();
            OnEnable();
        }

        void OnEnable()
        {
            m_Camera = this.GetComponent<Camera>();

            if (supportNormal)
            {
                Shader.EnableKeyword("_SUPPORT_NORMAL");
                m_Camera.depthTextureMode = DepthTextureMode.DepthNormals;
            }
            else
            {
                Shader.DisableKeyword("_SUPPORT_NORMAL");
#if UNITY_EDITOR
                if(!Application.isPlaying)
                    m_Camera.depthTextureMode = DepthTextureMode.Depth;
#endif
                rendererBuffer = new RendererBuffer(m_Camera);
                rendererBuffer.Create();
            }
        }

        void OnDisable()
        {
            if (rendererBuffer != null)
            {
                rendererBuffer.Dispose();
                rendererBuffer = null;
            }
                
        }
    }
}
