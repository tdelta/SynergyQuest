﻿using Sokoban;
using UnityEngine;

public class SokobanSwitch : MonoBehaviour
{
    [SerializeField] private PlayerColor color = PlayerColor.Any;
    /**
     * Sprites to use, depending on the color
     */
    [SerializeField] private SokobanSprites sprites = default;
    

    private Switch _switch;

    public delegate void SwitchChangedAction();
    public event SwitchChangedAction OnSwitchChanged;

    public void Start()
    {
        _switch = this.GetComponent<Switch>();
    }

    /**
     * Only called in editor, e.g. when changing a property
     */
    private void OnValidate()
    {
        // Change the sprite according to the assigned color
        GetComponent<SpriteRenderer>().sprite = sprites.GetSwitchSprite(this.color);
    }

    private void SetPressed(bool pressed)
    {
        _switch.Value = pressed;

        OnSwitchChanged?.Invoke();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!_switch.Value && other.gameObject.CompareTag("Box")){
            var box = other.GetComponent<Box>();
            var pushable = other.GetComponent<Pushable>();
            
            if(pushable.state == State.Resting && box.Color.IsCompatibleWith(this.GetColor())) {
                SetPressed(true);
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Box")){
            var box = other.gameObject.GetComponent<Box>();
            
            if(box.Color.IsCompatibleWith(this.GetColor())) {
                SetPressed(false);
            }
        }
    }
    
    public PlayerColor GetColor(){
        return color;
    }
}