using System;
using UnityEngine;

/**
 * Ensures an object is not spawning again when reloading a scene, if it has been destroyed manually before.
 * ("Manually" means, it has not been destroyed by unloading the scene)
 *
 * This is for example important for destructible walls which should still be destroyed if a player revisits a room.
 */
[RequireComponent(typeof(Guid))]
public class PersistentlyDestructible : MonoBehaviour
{
    public Guid guid { get; private set; }
    private bool _applicationHasQuit = false;

    private void Awake()
    {
        guid = GetComponent<Guid>();

        // If this object has been destroyed previously, prevent respawning by self-destructing on initialization
        if (DungeonDataKeeper.Instance.HasObjectBeenDestroyed(this))
        {
            Destroy(this.gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        _applicationHasQuit = true;
    }

    private void OnDestroy()
    {
        // Mark object as destroyed, but only if it is not being destroyed due to loading another scene (or closing the application)
        if (!_applicationHasQuit && !SceneController.Instance.IsLoadingScene)
        {
            DungeonDataKeeper.Instance.MarkAsDestroyed(this);
        }
    }
}
