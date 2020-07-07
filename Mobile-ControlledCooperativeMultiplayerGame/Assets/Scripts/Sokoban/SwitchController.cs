﻿using System;
using System.Collections;
using System.Collections.Generic;
using Sokoban;
using UnityEngine;

public class SwitchController : MonoBehaviour
{
    [SerializeField] private PlayerColor color = PlayerColor.Any;
    /**
     * Sprites to use, depending on the color
     */
    [SerializeField] private SokobanSprites sprites;
    
    private bool _pressed;

    public delegate void SwitchChangedAction();
    public event SwitchChangedAction OnSwitchChanged;

    public void Start()
    {
        _pressed = false;
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
        _pressed = pressed;
        OnSwitchChanged?.Invoke();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Box")){
            Pushable box = other.gameObject.GetComponent<Pushable>();
            
            if(box.Color.IsCompatibleWith(this.GetColor())) {
                SetPressed(true);
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Box")){
            Pushable box = other.gameObject.GetComponent<Pushable>();
            
            if(box.Color.IsCompatibleWith(this.GetColor())) {
                SetPressed(false);
            }
        }
    }

    public bool IsPressed(){
        return _pressed;
    }

    public PlayerColor GetColor(){
        return color;
    }
}
