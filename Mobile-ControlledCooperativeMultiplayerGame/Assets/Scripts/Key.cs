using UnityEngine;

/**
 * A key can be collected by players and be used to open doors. See also `KeyLock`.
 *
 * Usually this class should be combined with a `ContactTrigger` which invokes the `CollectKey` method.
 */
[RequireComponent(typeof(Guid))]
public class Key : MonoBehaviour
{
    private SpriteRenderer _renderer;
    public Guid Guid { get; private set; }

    /**
     * Since this object will invisibly persist for a short while before being destroyed to play a sound,
     * we use this flag to indicate this state and prevent the key from being collected.
     *
     * This prevents multiple players from collecting a key at the same time.
     */
    private bool _isBeingDestroyed = false; 

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        Guid = GetComponent<Guid>();
        
        // Self-destruct, if this key has already been collected
        if (DungeonDataKeeper.Instance.HasKeyBeenCollected(this))
        {
            Destroy(this.gameObject);
        }
    }

    /**
     * Call this method to let a player collect this key.
     */
    public void CollectKey(PlayerController collector)
    {
        if (!_isBeingDestroyed)
        {
            // Remember, that this key has been collected
            DungeonDataKeeper.Instance.MarkKeyAsCollected(this);
            // Increase the counter of collected keys for the players
            PlayerDataKeeper.Instance.NumKeys += 1;
            // Play a sound and destroy this object afterwards
            this.PlaySoundAndDestroy();
            _isBeingDestroyed = true;
            // Display an animation on the player collecting this key.
            collector.PresentItem(_renderer.sprite);
        }
    }
}
