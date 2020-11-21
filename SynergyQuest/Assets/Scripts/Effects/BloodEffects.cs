using Effects;
using UnityEngine;

[RequireComponent(typeof(Attackable))]
public class BloodEffects : MonoBehaviour
{
    [SerializeField] private Color baseColor = new Color(1, 0.149f, 0, 1);
    
    [SerializeField] private BloodParticles particlesPrefab;
    [SerializeField] private BloodStain bloodStainPrefab;
    
    class ParticlesPool: ObjectPool<BloodParticles> {}
    private ParticlesPool particlesPool;
    // FIXME: ^Use a global pool

    private Attackable _attackable;

    private void Awake()
    {
        _attackable = GetComponent<Attackable>();
        
        particlesPool = ObjectPool.Make<ParticlesPool, BloodParticles>(this.transform, particlesPrefab);
    }

    private void OnEnable()
    {
        _attackable.OnAttacked += OnAttacked;
    }
    
    private void OnDisable()
    {
        _attackable.OnAttacked -= OnAttacked;
    }

    private void OnAttacked(GameObject attacker, Optional<Vector2> attackDirection)
    {
        attackDirection.Match(
            some: direction =>
            {
                var particles = particlesPool.GetInstance();
                particles.onDoneCallback = () => particlesPool.ReturnInstance(particles);
                particles.transform.position = VectorExtensions.Assign2D(particles.transform.position, this.transform.position);
                particles.darkColor = baseColor * 0.7f;
                particles.lightColor = baseColor;
                
                particles.Trigger(direction);
                
                var stain = Instantiate(bloodStainPrefab);
                stain.color = baseColor;
                stain.transform.position = VectorExtensions.Assign2D(stain.transform.position, this.transform.position);
            }
        );
    }
}
