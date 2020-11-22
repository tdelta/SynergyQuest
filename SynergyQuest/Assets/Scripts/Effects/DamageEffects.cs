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
    private Color damageBaseColor = new Color(1, 0.149f, 0, 1);

    [SerializeField] private Color healingBaseColor = Color.green;
    
    [FormerlySerializedAs("particlesPrefab")]
    [SerializeField, CanBeNull, Tooltip("Set to enable blood particles.")] private BloodParticles bloodParticlesPrefab = null;
    [SerializeField, CanBeNull, Tooltip("Set to enable blood stains on floor.")] private BloodStain bloodStainPrefab = null;
    [SerializeField, CanBeNull, Tooltip("Set to enable sprite tint flash on damage.")] private TintFlashController tintFlashController = null;
    [SerializeField, CanBeNull, Tooltip("Set to enable damage sounds.")] private MultiSound damageSounds = null;
    [SerializeField, CanBeNull, Tooltip("Set to enable knockback")] private PhysicsEffects physicsEffects = null;

    [SerializeField, Tooltip("Knockback will be multiplied by this, if there is no damage")] private float knockbackReductionWhenNoDamage = 0.5f;
    
    class ParticlesPool: ObjectPool<BloodParticles> {}
    [CanBeNull] private ParticlesPool particlesPool = null;
    // FIXME: ^Use a global pool

    private Attackable _attackable;
    [CanBeNull] private Renderer _renderer;

    private void Awake()
    {
        // make sure our serialized parameters are real null values and not some weird Unity thing
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
        Vector3 entityCenter;
        if (ReferenceEquals(_renderer, null))
        {
            entityCenter = this.transform.position;
        }
        else
        {
            entityCenter = _renderer.bounds.center;
        }
        
        if (attack.damage > 0)
        {
            // Blood stains on the floor
            if (!ReferenceEquals(bloodStainPrefab, null))
            {
                var stain = Instantiate(bloodStainPrefab);
                stain.color = damageBaseColor;
                stain.transform.position = VectorExtensions.Assign2D(stain.transform.position, entityCenter);
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

        if (attack.damage < 0)
        {
            // Positive tint flash if being healed
            // ReSharper disable once Unity.NoNullPropagation
            tintFlashController?.FlashTint(
                healingBaseColor, 0.5f
            );
        }
        
        attack.attackDirection.Match(
            some: direction =>
            {
                // knockback
                // ReSharper disable once Unity.NoNullPropagation
                physicsEffects?.ApplyImpulse(direction * (attack.knockback * (attack.damage == 0 ? knockbackReductionWhenNoDamage : 1)));

                if (attack.damage > 0)
                {
                    // blood particles in attack direction
                    if (!ReferenceEquals(particlesPool, null))
                    {
                        var particles = particlesPool.GetInstance();
                        particles.onDoneCallback = () =>
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
