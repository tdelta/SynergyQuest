using UnityEngine;

/**
 * <summary>
 * An entity which periodically fires projectiles into a certain direction.
 * </summary>
 */
public class Turret : MonoBehaviour
{
    /**
     * How often a projectile shall be fired (every n seconds)
     */
    [SerializeField] private float fireRate = 2.0f;
    /**
     * Where the projectile shall be fired from
     */
    [SerializeField] private Transform launchPoint = null;
    /**
     * In what direction the projectile shall be fired
     */
    [SerializeField] private Vector2 direction = Vector2.zero;
    /**
     * The projectile to fire
     */
    [SerializeField] FireballProjectile fireballPrefab = default;

    private Collider2D _collider;

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        // Call the given method repeatedly to launch a projectile 
        InvokeRepeating(nameof(LaunchProjectile), 0.0f, fireRate);
    }

    void LaunchProjectile()
    {
        var instance = FireballProjectile.Launch(fireballPrefab, this.launchPoint.position, direction);

        // If this object has a collider, the projectile shall not collide with it
        if (!ReferenceEquals(_collider, null))
        {
            Physics2D.IgnoreCollision(instance.Collider, _collider);
        }
    }
}
