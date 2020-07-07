using Sokoban;
using UnityEngine;

/**
 * Assigns a sokoban box the correct sprite, depending on the color of its `Pushable` component.
 */
public class Box : MonoBehaviour
{
    /**
     * Sprites to use, depending on the color
     */
    [SerializeField] private SokobanSprites sprites;
    
    /**
     * Only called in editor, e.g. when changing a property
     */
    public void OnValidate()
    {
        var pushable = GetComponent<Pushable>();
        var spriteRenderer = GetComponent<SpriteRenderer>();

        // Change the sprite according to the assigned color
        spriteRenderer.sprite = sprites.GetBoxSprite(pushable.Color);
    }
}
