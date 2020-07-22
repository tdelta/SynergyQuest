using System;
using System.Runtime.InteropServices;
using Sokoban;
using UnityEngine;

/**
 * Assigns a sokoban box the correct sprite, depending on the color of its `Pushable` component.
 */
[RequireComponent(typeof(Interactive))]
public class Box : MonoBehaviour
{
    /**
     * Sprites to use, depending on the color
     */
    [SerializeField] private SokobanSprites sprites;

    private Interactive _interactive;
    public PlayerColor Color => _interactive.Color;

    private void Awake()
    {
        _interactive = GetComponent<Interactive>();
    }

    /**
     * Only called in editor, e.g. when changing a property
     */
    public void OnValidate()
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        var interactive = GetComponent<Interactive>();

        // Change the sprite according to the assigned color
        spriteRenderer.sprite = sprites.GetBoxSprite(interactive.Color);
    }
}
