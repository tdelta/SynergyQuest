using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DamageSystem
{
    /**
     * Objects with this behaviour receive damage when covered / stuck within other colliders.
     * E.g. a player pushes a box onto an enemy which is then crushed between the box and a wall.
     */
    [RequireComponent(typeof(Attackable))]
    public class Crushable : MonoBehaviour
    {
        private Collider2D _collider;

        private List<ContactPoint2D> _contactBuffer = new List<ContactPoint2D>();

        [Tooltip("Percentage by which another collider must penetrate this collider to trigger crushing damage. (Percentage of the smallest side length of the bounding box of the collider of this object.)")]
        [SerializeField] private float depthLimit = 0.35f;
        [Tooltip("How often this behaviour shall check whether this object is being crushed (Hz)")]
        [SerializeField] private float samplingRate = 4.0f;
        [Tooltip("Minimum amount of time this object needs to be covered by other colliders before crushing damage is applied.")]
        [SerializeField] private float timeoutBeforeDamage = 0.7f;
        [SerializeField] private int damage = 1;

        // Inverse of sampling rate
        private float _secondsPerSample = float.NaN;
        // Timestamp of the last time we checked for other colliders
        private float _lastSamplingTimestamp = float.NaN;
        // Timestamp of the moment where we last refistered a violation of the depthLimit.
        // Once the time difference to this limit exceeds timeoutBeforeDamage and the violation is still present,
        // damage is applied
        private float _exceededLimitTimestamp = float.NaN;

        private Attackable _attackable;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            _attackable = GetComponent<Attackable>();

            _secondsPerSample = 1 / samplingRate;
        }

        private void OnCollisionStay2D(Collision2D _)
        {
            CheckIfBeingCrushed(true);
        }

        private void OnCollisionExit2D(Collision2D other)
        {
            // Dont respect sampling rate when the collisions stop, so that
            // _exceededLimitTimestamp can be reset
            CheckIfBeingCrushed(false);
        }

        /**
         * Check if another collider is in contact with this one and the contact point is within this object while the following
         * conditions hold:
         *
         * * the contact point is within the collider of this object
         * * its depth exceeds <see cref="depthLimit"/> percent of the minimum side length of the bounding box of this object
         * * the above conditions hold for at least <see cref="timeoutBeforeDamage"/> seconds
         * These conditions are checked at a rate of <see cref="samplingRate"/> Hz though this rate limiting is bypassed if <see cref="respectSamplingRate"/> is false.
         *
         * If the conditions hold, apply <see cref="damage"/> to the <see cref="Attackable"/> behaviour of this object.
         */
        private void CheckIfBeingCrushed(bool respectSamplingRate)
        {
            var now = Time.time;

            // Is it time to check the conditions according to the sampling rate, or are we ignoring the sampling rate anyway?
            if (!respectSamplingRate || float.IsNaN(_lastSamplingTimestamp) || now - _lastSamplingTimestamp >= _secondsPerSample)
            {
                // Get all objects currently colliding with this one
                var numContacts = _collider.GetContacts(_contactBuffer);
                
                // If there is at least one collision...
                if (numContacts > 0)
                {
                    // Determine the farthest length a collision contact has travelled into the collider of this object
                    // (in this case, the separation value is negative, so we compute the minimum)
                    var minimumSeparation = Enumerable.Range(0, numContacts)
                        .Min(i => _contactBuffer[i].separation);

                    // If there is a contact point within the collider of this object...
                    if (minimumSeparation < 0)
                    {
                        // Compute the smallest side length of the axis-aligned bounding box of this object
                        var size = _collider.bounds.size;
                        var smallestSideLength = Mathf.Min(size.x, size.y);

                        // If the depth of the deepest contact point exceeds depthLimit percent of the smallest side length...
                        if (Mathf.Abs(minimumSeparation) / smallestSideLength >= depthLimit)
                        {
                            // Apply damage, if this state has persistet for at least timeoutBeforeDamage seconds.
                            
                            if (float.IsNaN(_exceededLimitTimestamp))
                            {
                                _exceededLimitTimestamp = now;
                            }

                            if (now - _exceededLimitTimestamp >= timeoutBeforeDamage)
                            {
                                ApplyDamage();
                                _exceededLimitTimestamp = now;
                            }
                        }

                        else
                        {
                            // If one of the conditions is violated, reset the timer which counts the time since the depth limit has last been exceeded
                            _exceededLimitTimestamp = float.NaN;
                        }
                    }

                    else
                    {
                        // If one of the conditions is violated, reset the timer which counts the time since the depth limit has last been exceeded
                        _exceededLimitTimestamp = float.NaN;
                    }
                }

                else
                {
                    // If one of the conditions is violated, reset the timer which counts the time since the depth limit has last been exceeded
                    _exceededLimitTimestamp = float.NaN;
                }
                
                _lastSamplingTimestamp = now;
            }
        }
        
        private void ApplyDamage() {
            _attackable.Attack(new WritableAttackData
            {
                Damage = damage
            });
        }
    }
}