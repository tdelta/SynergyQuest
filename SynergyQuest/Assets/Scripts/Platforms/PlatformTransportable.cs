using System.Collections.Generic;
using UnityEngine;

/**
 * <summary>
 * This component marks a game object to be able to be transported by a platform, see also <see cref="Platform"/>.
 * 
 * It also keeps track of all platforms the object currently touches so that this information can be used by the
 * <see cref="Chasm"/> class to determine whether a player can not fall down a chasm because they are standing on a
 * platform.
 * </summary>
 */
public class PlatformTransportable : MonoBehaviour
{
    private Spawnable _spawnable;
    private Collider2D _collider;
    
    /**
     * Platforms this game object is currently in contact with
     */
    public HashSet<Platform> PlatformsInContact { get; } = new HashSet<Platform>();

    private void Awake()
    {
        _spawnable = GetComponent<Spawnable>();
        _collider = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        // If this object has a <see cref="Spawnable"/>, we register to its <c>OnRespawn</c> event.
        if (!ReferenceEquals(_spawnable, null))
        {
            _spawnable.OnRespawn += OnRespawn;
        }
    }
    
    private void OnDisable()
    {
        // If this object has a <see cref="Spawnable"/>, we unregister from its <c>OnRespawn</c> event.
        if (!ReferenceEquals(_spawnable, null))
        {
            _spawnable.OnRespawn -= OnRespawn;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // If the object we touched is a Platform, register it in the list of platforms we are currently in
        // contact with.
        if (other.TryGetComponent<Platform>(out var platform))
        {
            PlatformsInContact.Add(platform);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        // If the object we stopped touching is a Platform, unregister it from the list of platforms we are
        // currently in contact with.
        if (other.TryGetComponent<Platform>(out var platform))
        {
            PlatformsInContact.Remove(platform);
        }
    }
    
    /**
     * <summary>
     * This method is called, if this object also has a <see cref="Spawnable"/> component and its
     * <see cref="Spawnable.OnRespawn"/> event is triggered.
     *
     * We use this event to manually check for platform contacts on respawn, since the physics event functions of
     * <see cref="Chasm"/> may be called before the event functions of this behavior.
     * However, we have to register contacts with <see cref="Platform"/> before <see cref="Chasm"/> does, so we directly
     * do it on respawn using this method.
     * </summary>
     */
    private void OnRespawn(Vector3 respawnPosition)
    {
        var colliderList = new List<Collider2D>();
        var contactFilter = new ContactFilter2D();
        contactFilter.NoFilter();
        
        _collider.OverlapCollider(contactFilter, colliderList);

        foreach (var collider in colliderList)
        {
            OnTriggerEnter2D(collider);
        }
    }
}
