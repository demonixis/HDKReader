/// OSVR-Unity Connection
///
/// http://sensics.com/osvr
///
/// <copyright>
/// Copyright 2014 Sensics, Inc.
///
/// Licensed under the Apache License, Version 2.0 (the "License");
/// you may not use this file except in compliance with the License.
/// You may obtain a copy of the License at
///
///     http://www.apache.org/licenses/LICENSE-2.0
///
/// Unless required by applicable law or agreed to in writing, software
/// distributed under the License is distributed on an "AS IS" BASIS,
/// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
/// See the License for the specific language governing permissions and
/// limitations under the License.
/// </copyright>
/// <summary>
/// Author: Greg Aring, Ryan Pavlik
/// Email: greg@sensics.com
/// </summary>
using UnityEngine;

namespace OSVR.Unity
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	public class K1RadialDistortion : MonoBehaviour
	{
		public float k1Red = 0.15f;
		public float k1Green = 0.15f;
		public float k1Blue = 0.15f;
		public Vector2 center = new Vector2(0.5f, 0.5f);
		private Material DistortionMaterial;

        private void Start()
        {
            DistortionMaterial = new Material(Shader.Find("Osvr/OsvrDistortion"));
        }

		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
#if UNITY_EDITOR
            if (DistortionMaterial == null)
                Start();

            if (DistortionMaterial == null)
                Graphics.Blit(source, destination);
#endif

            DistortionMaterial.SetFloat("_K1_Red", k1Red);
			DistortionMaterial.SetFloat("_K1_Green", k1Green);
			DistortionMaterial.SetFloat("_K1_Blue", k1Blue);
			DistortionMaterial.SetVector("_Center", center);
			Graphics.Blit(source, destination, DistortionMaterial);
		}
	}
}
