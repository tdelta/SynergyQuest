using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchController : MonoBehaviour
{
    [SerializeField] private PlayerColor color;
    
    private bool _pressed;

    public delegate void SwitchChangedAction();
    public event SwitchChangedAction OnSwitchChanged;

    public void Start()
    {
        _pressed = false;
    }

    private void SetPressed(bool pressed)
    {
        _pressed = pressed;
        OnSwitchChanged?.Invoke();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Box")){
            BoxController box = other.gameObject.GetComponent<BoxController>();
            
            if(box.getColor() == this.GetColor()) {
                SetPressed(true);
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Box")){
            BoxController box = other.gameObject.GetComponent<BoxController>();
            
            if(box.getColor() == this.GetColor()) {
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
