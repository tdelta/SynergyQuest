using UnityEngine;

/**
 * Given a source collider, this behavior will clone it at runtime and assign it to the own game object.
 * Some properties of the cloned collider can be overridden.
 *
 * This behavior is for example used for chasms, which need a separate child object with the same collider.
 * This child is set up such that only monsters collide with it.
 * This way, players can fall down a chasm, while monsters wont enter it.
 */
public class ColliderCloner : MonoBehaviour
{
    [SerializeField] private Collider2D source;

    /**
     * If true, the `IsTrigger` property of the cloned collider will be overriden
     */
    [SerializeField] private bool overrideIsTrigger = false;
    /**
     * The `IsTrigger` property of the cloned collider is overridden with this value,
     * if `overrideIsTrigger` is set.
     */
    [SerializeField] private bool isTrigger = false;

    private void Awake()
    {
        // Clone collider
        var copiedComponent = source.CopyComponent(gameObject);

        // If enabled, override properties
        if (overrideIsTrigger)
        {
            copiedComponent.isTrigger = isTrigger;
        }
    }
}
