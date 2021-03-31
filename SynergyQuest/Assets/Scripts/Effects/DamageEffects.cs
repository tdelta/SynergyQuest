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

using System.Linq;
using DamageSystem;
using Effects;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

/**
 * <summary>
 * Plays some effects to give feedback that the enemy has taken damage.
 * </summary>
 * <remarks>
 * The following effects are currently supported:
 *
 * blood particles
 * blood stains on floor
 * flash the enemy red for the duration of its invincibility after being hit.
 * plays a hit sound
 * knockback
 * </remarks>
 */
[RequireComponent(typeof(Attackable))]
public class DamageEffects : MonoBehaviour
{
    [SerializeField, FormerlySerializedAs("baseColor")]
    [Tooltip("Color of all effects played for damaging attacks.")]
    private Color damageBaseColor = new Color(1, 0.149f, 0, 1);

    [SerializeField]
    [Tooltip("Color of all effects played for attacks with healing effect (negative damage).")]
    private Color healingBaseColor = Color.green;
    
    [FormerlySerializedAs("particlesPrefab")]
    [SerializeField, CanBeNull, Tooltip("Set to enable blood particles.")]
    private BloodParticles bloodParticlesPrefab = null;
    
    [SerializeField, CanBeNull, Tooltip("Set to enable blood stains on floor.")]
    private BloodStain bloodStainPrefab = null;

    [SerializeField,
     Tooltip(
         "If objects of these layers are in the way, dont spawn blood stains. (Eg. you dont want blood stains on chasms)"
     )]
    private LayerMask unstainableGroundLayers = new LayerMask();
    // To check more efficiently for objects of the above layers, we use this buffer to store collisions
    private Collider2D[] _collisionBuffer = new Collider2D[1];

    [SerializeField, CanBeNull, Tooltip("Set to enable sprite tint flash on damage.")]
    private TintFlashController tintFlashController = null;
    
    [SerializeField, CanBeNull, Tooltip("Set to enable damage sounds.")]
    private MultiSound damageSounds = null;
    
    [SerializeField, CanBeNull, Tooltip("Set to enable knockback")]
    private PhysicsEffects physicsEffects = null;

    [SerializeField, Tooltip("Knockback will be multiplied by this, if there is no damage")]
    private float knockbackReductionWhenNoDamage = 0.5f;
    
    class ParticlesPool: ObjectPool<BloodParticles> {}
    [CanBeNull] private ParticlesPool particlesPool = null;
    // FIXME: ^Use a global pool

    private Attackable _attackable;
    // If a renderer is present, we will use it to get a better estimate on the visual center point of this object
    [CanBeNull] private Renderer _renderer;

    private void Awake()
    {
        // make sure our serialized parameters, if not set, are real null values and not some weird Unity thing
        bloodParticlesPrefab = bloodParticlesPrefab == null ? null : bloodParticlesPrefab;
        bloodStainPrefab = bloodStainPrefab == null ? null : bloodStainPrefab;
        tintFlashController = tintFlashController == null ? null : tintFlashController;
        damageSounds = damageSounds == null ? null : damageSounds;
        physicsEffects = physicsEffects == null ? null : physicsEffects;
        
        _attackable = GetComponent<Attackable>();
        _renderer = GetComponent<Renderer>();
        if (_renderer == null)
        {
            _renderer = null;
        }

        if (bloodParticlesPrefab != null)
        {
            particlesPool = ObjectPool.Make<ParticlesPool, BloodParticles>(this.transform, bloodParticlesPrefab);
        }
    }

    private void OnEnable()
    {
        _attackable.OnAttack += OnAttack;
    }
    
    private void OnDisable()
    {
        _attackable.OnAttack -= OnAttack;
    }

    private void OnAttack(AttackData attack)
    {
        // Compute the center of this object
        Vector3 entityCenter;
        if (!ReferenceEquals(_renderer, null))
        {
            // if there is a renderer, we can use the visual center
            entityCenter = _renderer.bounds.center;
        }
        else
        {
            // otherwise, we will use the transform
            entityCenter = this.transform.position;
        }
        
        if (attack.Damage > 0)
        {
            // Blood stains on the floor
            if (!ReferenceEquals(bloodStainPrefab, null))
            {
                // Check if the ground does support blood stains.
                // For example, there should be no stains on Chasms.
                // 
                // For this, we check the bounds of the blood stain for collisions with any object that is on the
                // unstainableGroundTags list.
                var size = Physics2D.OverlapBoxNonAlloc(entityCenter, bloodStainPrefab.gameObject.DetermineAABB().size, 0.0f, _collisionBuffer, unstainableGroundLayers);

                // If no collisions have been found, we can safely place the stain
                if (size == 0)
                {
                    var stain = Instantiate(bloodStainPrefab);
                    stain.color = damageBaseColor;
                    stain.transform.position = VectorExtensions.Assign2D(stain.transform.position, entityCenter);
                }
            }
            
            // Flash tint for the duration of the temporary invincibility
            // ReSharper disable once Unity.NoNullPropagation
            tintFlashController?.FlashTint(
                damageBaseColor, _attackable.InvincibleTime
            );
            
            // Play damage sound
            // ReSharper disable once Unity.NoNullPropagation
            damageSounds?.PlayOneShot();
        }

        if (attack.Damage < 0)
        {
            // Positive tint flash if being healed
            // ReSharper disable once Unity.NoNullPropagation
            tintFlashController?.FlashTint(
                healingBaseColor, 0.5f
            );
        }
        
        attack.AttackDirection.Match(
            some: direction =>
            {
                // knockback
                // ReSharper disable once Unity.NoNullPropagation
                physicsEffects?.ApplyImpulse(direction * (attack.Knockback * (attack.Damage == 0 ? knockbackReductionWhenNoDamage : 1)));

                if (attack.Damage > 0)
                {
                    // blood particles in attack direction
                    if (!ReferenceEquals(particlesPool, null))
                    {
                        var particles = particlesPool.GetInstance();
                        particles.DoneCallback = () =>
                        {
                            if (particlesPool != null)
                            {
                                particlesPool.ReturnInstance(particles);
                            }

                            else
                            {
                                Destroy(particles.gameObject);
                            }
                        };
                        particles.transform.position = VectorExtensions.Assign2D(particles.transform.position, entityCenter);
                        particles.darkColor = damageBaseColor * 0.7f;
                        particles.lightColor = damageBaseColor;

                        particles.Trigger(direction);
                    }
                }
            }
        );
    }
}
