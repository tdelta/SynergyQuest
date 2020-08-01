using System;
using System.Collections.Generic;
using UnityEngine;

/**
 * This component marks a game object to be able to be transported by a platform, see also `PlatformController`.
 * 
 * It also keeps track of all platforms the object currently touches so that this information can be used by the `Chasm`
 * class to determine whether a player can not fall down a chasm because they are standing on a platform.
 */
public class PlatformTransportable : MonoBehaviour
{
    /**
     * Platforms this game object is currently in contact with
     */
    public HashSet<PlatformController> PlatformsInContact { get; } = new HashSet<PlatformController>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        // If the object we touched is a PlatformController, register it in the list of platforms we are currently in
        // contact with.
        if (other.GetComponent<PlatformController>() is PlatformController platform)
        {
            Debug.Log($"Added platform for {this.name}");
            PlatformsInContact.Add(platform);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        // If the object we stopped touching is a PlatformController, unregister it from the list of platforms we are
        // currently in contact with.
        if (other.GetComponent<PlatformController>() is PlatformController platform)
        {
            PlatformsInContact.Remove(platform);
        }
    }
}
