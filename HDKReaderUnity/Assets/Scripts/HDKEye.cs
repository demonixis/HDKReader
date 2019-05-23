using OSVR.Unity;
using UnityEngine;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif

namespace HDKReader.Unity
{
    public sealed class HDKEye : MonoBehaviour
    {
        private const float EyeOffset = 0.0031f;
        private Camera m_Camera;
        private RenderTexture m_RenderTexture;

        public RenderTexture RenderTexture => m_RenderTexture;

        public void Setup(Camera parent, bool left, bool hdk2, bool disablePostProcessStack)
        {
            gameObject.layer = parent.gameObject.layer;

            m_Camera = CopyComponent<Camera>(parent, gameObject);
            m_Camera.rect = new Rect(left ? 0.0f : 0.5f, 0.0f, 0.5f, 1.0f);
            m_Camera.depth = 2;

            m_RenderTexture = new RenderTexture(hdk2 ? 1080 : 960, hdk2 ? 1200 : 1080, 24);
            m_RenderTexture.Create();

            m_Camera.stereoTargetEye = left ? StereoTargetEyeMask.Left : StereoTargetEyeMask.Right;
            //m_Camera.targetTexture = m_RenderTexture;
            m_Camera.fieldOfView = 100.0f;

            var eyeTransform = transform;
            eyeTransform.parent = parent.transform;
            eyeTransform.localPosition = new Vector3(EyeOffset * (left ? -1.0f : 1.0f), 0.0f, 0.0f);
            eyeTransform.localRotation = Quaternion.identity;

            gameObject.AddComponent<K1RadialDistortion>();
        }

        private void TryCopyPostProcessLayer(Camera parent)
        {
#if UNITY_POST_PROCESSING_STACK_V2
            var layer = parent.GetComponent<PostProcessLayer>();
            if (layer == null)
                return;

            var eyeLayer = gameObject.AddComponent<PostProcessLayer>();
            eyeLayer.antialiasingMode = layer.antialiasingMode;
            eyeLayer.fastApproximateAntialiasing = layer.fastApproximateAntialiasing;
            eyeLayer.stopNaNPropagation = layer.stopNaNPropagation;
            eyeLayer.volumeLayer = layer.volumeLayer;
            eyeLayer.volumeTrigger = layer.volumeTrigger;
            eyeLayer.enabled = false;// PostProcessResources is not copied, we've to use relection for that.
#endif
        }

        private static T CopyComponent<T>(T original, GameObject destination) where T : Component
        {
            var type = original.GetType();
            var copy = destination.AddComponent(type);
            var fields = type.GetFields();

            foreach (var field in fields)
                field.SetValue(copy, field.GetValue(original));

            return copy as T;
        }
    }
}
