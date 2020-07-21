using JetBrains.Annotations;
using UnityEngine;

/**
 * A key can be collected by players and be used to open doors. See also `KeyLock`.
 *
 * Usually this class should be combined with a `ContactTrigger` which invokes the `CollectKey` method.
 */
public class Key : MonoBehaviour
{
    /**
     * Every key can optionally have an ID.
     * This ID must be set if you want the game to remember, whether a key has already been collected across scenes.
     * Then, an already collected key will not appear again when revisiting a scene.
     */
    [SerializeField] [CanBeNull] private string keyId;
    [CanBeNull] public string KeyId => keyId;
    
    private SpriteRenderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        
        // Self-destruct, if this key has already been collected
        if (PlayerDataKeeper.Instance.HasKeyBeenCollected(this))
        {
            Destroy(this.gameObject);
        }
    }

    /**
     * Call this method to let a player collect this key.
     */
    public void CollectKey(PlayerController collector)
    {
        // Remember, that this key has been collected
        PlayerDataKeeper.Instance.MarkKeyAsCollected(this);
        // Increase the counter of collected keys for the players
        PlayerDataKeeper.Instance.NumKeys += 1;
        // Play a sound and destroy this object afterwards
        this.PlaySoundAndDestroy();
        // Display an animation on the player collecting this key.
        collector.PresentItem(_renderer.sprite);
    }
}
