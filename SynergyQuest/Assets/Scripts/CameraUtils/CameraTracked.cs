// This file is part of the "Synergy Quest" game
// (github.com/tdelta/SynergyQuest).
// 
// Copyright (c) 2020
//   Marc Arnold     (m_o_arnold@gmx.de)
//   Martin Kerscher (martin_x@live.de)
//   Jonas Belouadi  (jonas.belouadi@posteo.net)
//   Anton W Haubner (anton.haubner@outlook.de)
//   David Heck (david@heck.info)
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


using Cinemachine;
using UnityEngine;
using Utils;

namespace CameraUtils
{
    /**
     * <summary>
     * Objects with this behaviour will be automatically tracked by the camera
     * </summary>
     * <remarks>
     * * This works by adding this object automatically to first found instance of <see cref="CinemachineTargetGroup"/>.
     *   So make sure there is an instance in your scene.
     * * Camera-tracking can be turned off and on by enabling / disabling this behaviour
     * </remarks>
     */
    public class CameraTracked : MonoBehaviour
    {
        /**
         * Reference to the first found cinemachine target group. This is initialized when this behaviour is first enabled.
         */
        private BlendingCameraTargetGroup2D _cameraTargetGroup = null;
        
        /**
         * Indicates, whether we have tried yet to search for a camera target group
         */
        private bool searchedForGroup = false;

        [Tooltip("What radius around the center of this object should the camera at least capture? If NaN, an apropriate value will be guessed based on the bounding box of this object.")]
        [SerializeField] private float radius = float.NaN;

        /**
         * Radius around this object which should always be captured by the camera.
         * If set to NaN, an appropriate value will be guessed which is at least large enough to cover the bounding box of
         * this object.
         */
        public float Radius
        {
            get
            {
                if (float.IsNaN(radius))
                {
                    radius = GuessRadius();
                }

                return radius;
            }
        
            set
            {
                radius = value;
                if (enabled && _cameraTargetGroup.IsNotNull())
                {
                    _cameraTargetGroup.RemoveMember(this.transform);
                    _cameraTargetGroup.AddMember(this.transform, radius);
                }
            }
        }

        [SerializeField] private bool tracking = true;

        public bool Tracking
        {
            get => tracking;
            set
            {
                if (value)
                {
                    StartTracking();
                }

                else
                {
                    StopTracking();
                }
            
                tracking = value;
            }
        }

        private void StartTracking()
        {
            // If we have not yet acquired a reference to a CinemachineTargetGroup, try finding one..
            if (_cameraTargetGroup.IsNull() && !searchedForGroup)
            {
                _cameraTargetGroup = FindObjectOfType<BlendingCameraTargetGroup2D>();
                if (_cameraTargetGroup == null)
                {
                    // Set to true null value (not the Unity null emulation object) to make subsequent null checks faster
                    _cameraTargetGroup = null;
                    searchedForGroup = true;
                }
            }
        
            // If we have one, add this object to it 
            if (_cameraTargetGroup.IsNotNull())
            {
                // ReSharper disable once PossibleNullReferenceException
                _cameraTargetGroup.AddMember(this.transform, Radius);
            }

            else
            {
                Debug.LogError($"Could not track object with camera, since there is no {nameof(CinemachineTargetGroup)} instance.");
            }
        }
    
    

        private void StopTracking()
        {
            // Dont track this object with the camera anymore
            if (_cameraTargetGroup.IsNotNull())
            {
                _cameraTargetGroup.RemoveMember(this.transform);
            }
        }

        private void OnEnable()
        {
            if (tracking)
            {
                StartTracking();
            }
        }

        private void OnDisable()
        {
            StopTracking();
        }

        private void OnDestroy()
        {
            // If still enabled, stop tracking. Otherwise, we have already been removed from the target group
            if (enabled)
            {
                StopTracking();
            }
        }

        /**
         * <summary>
         * Guess best radius around object which should be captured by the camera at all times.
         * </summary>
         * <remarks>
         * A value will be guessed which is at least large enough to cover the bounding box of this object.
         * </remarks>
         */
        private float GuessRadius()
        {
            var bounds = gameObject.DetermineAABB();
            var maxSideLength = Mathf.Max(bounds.size.x, bounds.size.y);

            // Radius should be at least as wide as the smallest side length of the bounding box + another unit length
            return maxSideLength / 2 + 1.0f;
        }
    }
}