// This file is part of the "Synergy Quest" game
// (github.com/tdelta/SynergyQuest).
// 
// Copyright (c) 2020
//   Marc Arnold     (m_o_arnold@gmx.de)
//   Martin Kerscher (martin_x@live.de)
//   Jonas Belouadi  (jonas.belouadi@posteo.net)
//   Anton W Haubner (anton.haubner@outlook.de)
// 
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the Free
// Software Foundation; either version 3 of the License, or (at your option) any
// later version.
// 
// This program is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
// FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
// more details.
// 
// You should have received a copy of the GNU General Public License along with
// this program; if not, see <https://www.gnu.org/licenses>.
// 
// Additional permission under GNU GPL version 3 section 7 apply,
// see `LICENSE.md` at the root of this source code repository.

using UnityEngine;

namespace CameraUtils
{
    /**
     * <summary>
     * Creates a render texture on which it renders all objects on the <c>WaterReflection</c> layer.
     * The camera always aligns with the settings and positions of the main camera and also automatically updates the
     * resolution of the render texture accordingly.
     * </summary>
     * <remarks>
     * To render the reflection of a sprite into the texture created by this camera, add the <see cref="WaterReflectable"/>
     * behavior to it.
     * The created render texture is for example used by <see cref="PuddleShaderController"/>.
     * </remarks>
     */
    public class WaterReflectionCamera : MonoBehaviour
    {
        public RenderTexture RenderTexture { get; private set; } = default;

        /**
         * <summary>
         * Invoked whenever the render texure is replaced because the resolution of the camera changed.
         * </summary>
         */
        public event RenderTextureUpdatedAction OnRenderTextureUpdated;
        public delegate void RenderTextureUpdatedAction();

        /**
         * Main camera of the scene
         */
        private Camera _mainCamera = default;
        /**
         * Camera of this object which renders the texture
         */
        private Camera _baseCamera = default;

        /**
         * <summary>
         * Retrieves a game object which contains an instance of this behavior.
         * If no such object exists, one is created.
         * </summary>
         * <remarks>
         * The prefab to create the instance is set in <see cref="PrefabSettings"/>.
         * </remarks>
         */
        public static WaterReflectionCamera Instance
        {
            get
            {
                var instance = FindObjectOfType<WaterReflectionCamera>();

                if (instance == null)
                {
                    instance = Instantiate(PrefabSettings.Instance.WaterReflectionCameraPrefab);
                }

                return instance;
            }
        }

        private void Awake()
        {
            _baseCamera = GetComponent<Camera>();

            // Only render the "WaterReflection" layer
            _baseCamera.cullingMask = LayerMask.GetMask("WaterReflection");
        }

        // Start is called before the first frame update
        void Start()
        {
            _mainCamera = Camera.main;
        
            UpdateRenderTexture();
            UpdateCameraSettings();
        }

        /**
         * <summary>
         * Creates a new render texture with the resolution of the main camera.
         * </summary>
         * <remarks>
         * Invokes the <see cref="OnRenderTextureUpdated"/> event.
         * </remarks>
         */
        private void UpdateRenderTexture()
        {
            RenderTexture = new RenderTexture(_mainCamera.pixelWidth, _mainCamera.pixelHeight, 24);
            if (_baseCamera.targetTexture != null)
            {
                _baseCamera.targetTexture.Release();
            }
            _baseCamera.targetTexture = RenderTexture;
        
            OnRenderTextureUpdated?.Invoke();
        }

        /**
         * <summary>
         * Applies the settings of the main camera to this camera (orthographic size etc.).
         * </summary>
         */
        void UpdateCameraSettings()
        {
            _baseCamera.orthographicSize = _mainCamera.orthographicSize;
            _baseCamera.nearClipPlane = _mainCamera.nearClipPlane;
            _baseCamera.farClipPlane = _mainCamera.farClipPlane;
        }

        // Update is called once per frame
        void LateUpdate()
        {
            this.transform.position = _mainCamera.transform.position;

            if (
                _baseCamera.pixelHeight != _mainCamera.pixelHeight ||
                _baseCamera.pixelWidth != _mainCamera.pixelWidth
            )
            {
                UpdateRenderTexture();
            }
        
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (_baseCamera.orthographicSize != _mainCamera.orthographicSize)
            {
                UpdateCameraSettings();
            }
        }
    }
}