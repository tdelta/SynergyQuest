using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Cinemachine.Utility;
using JetBrains.Annotations;
using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
#endif

namespace CameraUtils
{
    /**
     * <summary>
     * Modified version of the standard <see cref="CinemachineTargetGroup"/>.
     * Behaves the same as <see cref="CinemachineTargetGroup"/> with the following changes:
     *
     * * 3D is not supported. Only use with 2D Framing Transposer
     * * New objects are smoothly blended in by slowly expanding the view frame, instead of immediately resetting it like the default <see cref="CinemachineTargetGroup"/> does it
     *   Set the <see cref="blendDuration"/> field to influence the speed of this expansion.
     * * It is no longer supported to set weights for members manually
     * * Adding objects is now possible only at runtime, not in the editor. Use <see cref="CameraTracked"/> behaviour for this
     * </summary>
     */
    public partial class BlendingCameraTargetGroup2D : MonoBehaviour, ICinemachineTargetGroup
    {

        /**
         * Objects of the target group together with some metainformation. See <see cref="Target"/>.
         */
        private List<Target> _targets = new List<Target>();

        /**
         * List of objects which are currently being blended-in / blended-out together with information on how far the
         * blending has progressed. 
         */
        private List<BlendingProcess> _blendingProcesses = new List<BlendingProcess>();
        
        [Tooltip("Maximum duration for blending in a new object or blending out a removed object")]
        [SerializeField] private float blendDuration = 2.0f;

        /**
         * Transform of the game object which contains this behaviour
         */
        public Transform Transform => this.transform;

        /**
         * Axis-aligned bounding box of all objects in this group with their radii.
         * (Though the box might be in the process of growing / shrinking for new objects or objects which are currently being removed)
         *
         * At least the contents of this box must be shown by the camera.
         */
        public Bounds BoundingBox { get; private set; }

        /**
         * Same as <see cref="BoundingBox"/>, but a bounding sphere instead of an AABB.
         */
        public BoundingSphere Sphere
        {
            get
            {
                var b = BoundingBox;
                return new BoundingSphere(b.center, ((b.max - b.min) / 2).magnitude);
            }
        }

        /**
         * Are there no objects in the target group?
         * (Objects in the process of being blended-out are still being counted)
         */
        public bool IsEmpty => _targets.IsEmpty();

        /**
         * <summary>
         * Add an object to this target group, so that it is shown by the camera.
         * The framing of the camera will be smoothly expanded, until it is visible.
         * </summary>
         * <param name="targetTransform">transform of the object which shall be added</param>
         * <param name="radius">radius around the object which must be included in the camera view</param>
         */
        public void AddMember(Transform targetTransform, float radius)
        {
            // First check, if there is already a blending process for this object
            // (ie. it is already part of the group and currently being removed or being added)
            var blendingProcessIdx = FindBlendingProcess(targetTransform);
            
            // If there is no blending process...
            if (blendingProcessIdx < 0)
            {
                // Check, whether the object is already part of this group, even if there is no blending process
                if (_targets.Any(target => target.Transform == targetTransform))
                {
                    Debug.LogError("An object which is already part of the target group is being added twice!");
                    return;
                }
                
                // Otherwise, we can add it as a new member
                var newTarget = new Target
                {
                    Transform = targetTransform,
                    LastPosition = targetTransform.position,
                    Weight = ComputeInitialWeight(targetTransform, radius),
                    Radius = radius
                };

                _targets.Add(newTarget);
                
                // And start blending it in...
                _blendingProcesses.Add(new BlendingProcess
                {
                    Target = newTarget,
                    StartTimeStamp = Time.time,
                    Direction = 1
                });
            }

            // otherwise, if the new object was being blended-out, reverse the blending direction and blend it in
            else
            {
                var blendingProcess = _blendingProcesses[blendingProcessIdx];

                if (blendingProcess.Direction > 0)
                {
                    Debug.LogError("An object which is already part of the target group is being added twice!");
                    return;
                }

                blendingProcess.Direction = 1;
                blendingProcess.Target.IsBeingRemoved = false;
            }
        }
        
        /**
         * Removes an object from the target group.
         * The framing of the camera will be smoothly shrunk until it is no longer visible.
         */
        public void RemoveMember(Transform t)
        {
            // First, check if the object is in the process of being blended in or blended out...
            var blendingProcessIdx = FindBlendingProcess(t);
            
            // If there is no blending process...
            if (blendingProcessIdx < 0)
            {
                // Find it in the list of targets
                var targetIdx = _targets.FindIndex(otherTaget => otherTaget.Transform == t);
                if (targetIdx < 0)
                {
                    // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                    Debug.LogError("Can not remove object from camera target group, because it has not been a member in the first place.");
                    return;
                }

                var target = _targets[targetIdx];
                
                // And start a process which blends it out
                var blendingProcess = new BlendingProcess
                {
                    Target = target,
                    Direction = -1,
                    StartTimeStamp = Time.time
                };

                _blendingProcesses.Add(blendingProcess);
                target.IsBeingRemoved = true;
            }

            // If there is already a blending-process...
            else
            {
                var blendingProcess = _blendingProcesses[blendingProcessIdx];

                if (blendingProcess.Direction < 0)
                {
                    return;
                }
                
                // If it is being blended in, we just reverse the blend direction to blend it out
                blendingProcess.Direction = -1;
                blendingProcess.Target.IsBeingRemoved = true;
            }
        }

        /**
         * The weight indicates, how visible a member object is. For all members it is gradually increased to 1.
         * Hence, for new member objects, we have to set the initial weight to the percentage of their radius that is
         * already covered by the current group bounding box.
         * This is computed here.
         *
         * (If we didnt compute the initial weight, then new members which are already visible might become temporarily
         * invisible again if they move faster out of view than the blending process catches up.)
         */
        private float ComputeInitialWeight(Transform t, float radius)
        {
            var position = t.position;

            // We want to compute the minimum length of the radius thar is already covered by the current bounding box
            float coveredRadius;
            
            // Is the new object within the current bounding box?
            if (BoundingBox.Contains(position))
            {
                
                var minXDist = Math.Min(position.x - BoundingBox.min.x, BoundingBox.max.x - position.x);
                var minYDist = Math.Min(position.y - BoundingBox.min.y, BoundingBox.max.y - position.y);
                
                // then the covered radius is the minimum distance to one of the sides of the AABB
                coveredRadius = Math.Min(minXDist, minYDist);
            }

            // Otherwise, it is the distance to the closest point on the AABB
            else
            {
                var closestPoint = BoundingBox.ClosestPoint(position);
                var distanceToClosestPoint = (position - closestPoint).magnitude;
                coveredRadius = radius - distanceToClosestPoint;
            }
            
            // Now we convert it into a percentage of the target radius

            if (coveredRadius < 0)
            {
                return 0.0f;
            }
            
            else if (coveredRadius < radius)
            {
                return coveredRadius / radius;
            }

            else
            {
                return 1.0f;
            }
        }

        /**
         * If there is a blending process for the object with the given transform, return the index of that process.
         * If there is none, return a value < 0.
         */
        private int FindBlendingProcess(Transform transform)
        {
            return _blendingProcesses.FindIndex(process => process.Target.Transform == transform);
        }

        /**
         * <summary>
         *     The axis-aligned bounding box of the group, in a specific reference frame.
         *
         *     This method implementation is largely the same as <see cref="CinemachineTargetGroup.GetViewSpaceBoundingBox"/>.
         * </summary>
         * <param name="observer">The frame of reference in which to compute the bounding box</param>
         * <returns>The axis-aligned bounding box of the group, in the desired frame of reference</returns>
         */
        public Bounds GetViewSpaceBoundingBox(Matrix4x4 observer)
        {
            var inverseView = observer.inverse;
            
            // We will compute a bounding box that encompasses all members of this group
            // (though we will take their weights into account)
            // First, we transfer the average position of all members into the coordinate system of the observer:
            var bounds = new Bounds(inverseView.MultiplyPoint3x4(_averagePos), Vector3.zero);
        
            foreach (var target in _targets)
            {
                // Compute the bounding sphere of each group member
                var boundingSphere = WeightedMemberBounds(target);
                
                // Transfer the sphere position from world coordinates into the coordinate system of the observer
                boundingSphere.position = inverseView.MultiplyPoint3x4(boundingSphere.position);
                
                // Expand the bounds for the whole group to capture this member
                bounds.Encapsulate(new Bounds(boundingSphere.position, boundingSphere.radius * 2 * Vector3.one));
            }
        
            return bounds;
        }

        /**
         * Computes a bounding sphere for the group member with the given transform.
         * We will take blending processes into account here, that is, the position of the sphere is moved towards the
         * average position of all members and the radius is shrunk depending on the current weight of the member.
         */
        private BoundingSphere WeightedMemberBounds(Target target)
        {
            return new BoundingSphere(Vector3.Lerp(_averagePos, target.LastPosition, target.Weight), target.Radius * target.Weight);
        }

        private Vector3 _averagePos;

        /**
         * Compute the average of the positions of all members, while taking blending processes (weights) into account.
         * It also saves the current transform position of each member in the <see cref="Target.LastPosition"/> field.
         * Also checks if a member object has been destroyed. If so, it will start to blend out the position of that object.
         */
        void CalculateAveragePosition()
        {
            _averagePos = Vector3.zero;
            float weight = 0;
        
            // For every member...
            foreach (var target in _targets)
            {
                // Register its weight
                weight += target.Weight;

                // If the member has not been destroyed, save its last position
                if (target.Transform != null)
                {
                    target.LastPosition = target.Transform.position;
                }
                
                // Otherwise, start to blend out the member, if a blend-out process has not already been started
                else if (!target.IsBeingRemoved)
                {
                    _blendingProcesses.Add(new BlendingProcess
                    {
                        Target = target,
                        StartTimeStamp = Time.time,
                        Direction = -1
                    });

                    target.IsBeingRemoved = true;
                }
                
                // Compute a weighted average position by first summing up all weighted positions
                _averagePos += target.LastPosition * target.Weight;
            }
        
            // Normalize the position over the sum of the weights
            if (weight > UnityVectorExtensions.Epsilon)
            {
                _averagePos /= weight;
            }

            else
            {
                // If we have no targets (at least none with non-zero weight), fallback to the position of the target group object
                _averagePos = transform.position;
            }
        }

        /**
         * Compute the bounding box of the group by combining the bounding boxes of all members.
         * (Blending processes are being taken into account)
         */
        private void CalculateBoundingBox()
        {
            var newBounds = new Bounds(_averagePos, Vector3.zero);
        
            foreach (var target in _targets)
            {
                var boundingSphere = WeightedMemberBounds(target);
            
                newBounds.Encapsulate(new Bounds(boundingSphere.position, boundingSphere.radius * 2 * Vector3.one));
            }

            BoundingBox = newBounds;
        }

        private void LateUpdate()
        {
            // Keep old position if we have no more targets
            if (IsEmpty)
            {
                return;
            }

            // Otherwise, continue blending processes (objects being blended in / blended out)
            UpdateBlending();
            
            // And update the average position and the bounding box of the group
            CalculateAveragePosition();
            CalculateBoundingBox();

            transform.position = _averagePos;
        }

        /**
         * Continue to blend-out / blend-in removed and newly added objects until they either reach a weight of 0.0 or
         * 1.0.
         */
        void UpdateBlending()
        {
            var now = Time.time;
            
            // For every blending process...
            for (int i = 0; i < _blendingProcesses.Count; )
            {
                var blendingProcess = _blendingProcesses[i];
                
                var timeDiff = now - blendingProcess.StartTimeStamp;
                var remainingTime = Mathf.Max(0, blendDuration - timeDiff);

                // If we are blending a member out, then...
                if (blendingProcess.Direction < 0)
                {
                    // Compute the speed by which we blend out:
                    // We select the speed such that the blending completes in the time specified by blendDuration
                    var speed = blendingProcess.Target.Weight / remainingTime;
                    
                    // Adjust the weight depending on this speed and the time difference since the last frame
                    blendingProcess.Target.Weight -= speed * Time.deltaTime;

                    // Once we reach a weight of about 0.0, the member has been completely blended out, so we can remove
                    // it from the group and also delete the blending process
                    if (blendingProcess.Target.Weight < UnityVectorExtensions.Epsilon)
                    {
                        _targets.Remove(blendingProcess.Target);
                        _blendingProcesses.RemoveAt(i);

                        continue;
                    }
                }

                else
                {
                    // Compute the speed by which we blend in:
                    // We select the speed such that the blending completes in the time specified by blendDuration
                    var speed = (1.0f - blendingProcess.Target.Weight) / remainingTime;
                    
                    // Adjust the weight depending on this speed and the time difference since the last frame
                    blendingProcess.Target.Weight = blendingProcess.Target.Weight + speed * Time.deltaTime;

                    // Once we reach a weight of at least 1.0, the member has been completely blended in, so the
                    // blending process has completed and we can delete it.
                    if (blendingProcess.Target.Weight >= 1.0f)
                    {
                        blendingProcess.Target.Weight = 1.0f;
                        _blendingProcesses.RemoveAt(i);
                        
                        continue;
                    }
                }

                ++i;
            }
        }

        /**
         * <summary>
         * This method is supposed to get the local-space angular bounds of the group, from a spoecific point of view.
         * 
         * We are operating in 2D only and apparently dont need this stuff for the Framing Transposer.
         * Thus, for now, this implementation is just zero-ing the out parameters.
         *
         * Should a need for this come up, see the implementation CinemachineTargetGroup.GetViewSpaceAngularBounds.
         * </summary>
         */
        public void GetViewSpaceAngularBounds(
            Matrix4x4 observer, out Vector2 minAngles, out Vector2 maxAngles, out Vector2 zRange)
        {
            minAngles = Vector2.zero;
            maxAngles = Vector2.zero;
            zRange = Vector2.zero;
        }
        
        /**
         * Meta-information used by <see cref="BlendingCameraTargetGroup2D"/> to keep track of the members of the target
         * group.
         * Similar to <see cref="CinemachineTargetGroup.Target"/>.
         */
        public class Target
        {
            // Transform of the object being tracked. May be null if the tracked object has been destroyed.
            // In this case, LastPosition will be used until the position has been blended out.
            [CanBeNull] public Transform Transform;
            
            // Last position of the tracked game object
            public Vector3 LastPosition;
            
            // Percentage of the object radius which must be shown in the view frame. 
            public float Weight;
            
            // Radius of the target which determines the bounding box
            public float Radius;

            // Indicates whether this object is already in the process of being removed (being blended out)
            public bool IsBeingRemoved = false;
        }
        
        /**
         * Describes how much the blending-in / blending-out of an object which is being added / removed from the group
         * has progressed.
         */
        private class BlendingProcess
        {
            // Member that is being blended-in / blended-out
            public Target Target;
            // When did we start this blending process? (in seconds)
            public float StartTimeStamp;
            // Are we blending-in or blending-out?
            // This field equals 1 in the first case and -1 in the latter case.
            public int Direction;
        }
    }
    
    #if UNITY_EDITOR
    /**
     * Custom editor script which displays the current members of a <see cref="BlendingCameraTargetGroup2D"/> in the
     * inspector (but does not allow editing them).
     */
    public partial class BlendingCameraTargetGroup2D
    {
        [CustomEditor(typeof(BlendingCameraTargetGroup2D))]
        private class BlendingCameraTargetGroup2DEditor: Editor
        {
            private BlendingCameraTargetGroup2D _typedTarget;
            
            private void OnEnable()
            {
                _typedTarget = (BlendingCameraTargetGroup2D) target;
            }

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.BeginVertical();
                foreach (var target in _typedTarget._targets)
                {
                    if (target.Transform != null)
                    {
                        EditorGUILayout.ObjectField(GUIContent.none, target.Transform, typeof(Transform), true);
                    }
                }
                EditorGUILayout.EndVertical();
                EditorGUI.EndDisabledGroup();
            }
        }
    }
    #endif
}

