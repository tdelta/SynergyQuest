using System;
using System.Collections.Generic;
using System.Linq;
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
    private Collider2D _collider;
    
    /**
     * Platforms this game object is currently in contact with
     */
    public HashSet<Platform> PlatformsInContact { get; } = new HashSet<Platform>();

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // If the object we touched is a Platform, register it in the list of platforms we are currently in
        // contact with.
        if (other.GetComponent<Platform>() is Platform platform)
        {
            PlatformsInContact.Add(platform);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        // If the object we stopped touching is a Platform, unregister it from the list of platforms we are
        // currently in contact with.
        if (other.GetComponent<Platform>() is Platform platform)
        {
            PlatformsInContact.Remove(platform);
        }
    }
}
