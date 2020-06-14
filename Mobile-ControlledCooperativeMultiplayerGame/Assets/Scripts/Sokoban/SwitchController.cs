using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchController : MonoBehaviour
{
    [SerializeField] private PlayerColor color = PlayerColor.Any;
    
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
