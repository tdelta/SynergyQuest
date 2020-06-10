using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SokobanDoorController : MonoBehaviour
{
    [SerializeField] private Sprite openedDoor;
    [SerializeField] private Sprite closedDoor;

    [SerializeField] private SwitchController[] switches;

    private SpriteRenderer _renderer;
    private AudioSource _audioSource;

    // Start is called before the first frame update
    void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        foreach (var switch_controller in switches)
        {
            switch_controller.OnSwitchChanged += OnSwitchChanged;
        }
    }

    private void OnDisable()
    {
        foreach (var switch_controller in switches)
        {
            switch_controller.OnSwitchChanged -= OnSwitchChanged;
        }
    }

    private void OnSwitchChanged()
    {
        if (SokobanSolved()) {
            _renderer.sprite = openedDoor;
            _audioSource.Play();
        } else {
            _renderer.sprite = closedDoor;
        }
    }

    private bool SokobanSolved(){
        foreach (var s in switches)
        {
            if(!s.IsPressed()) {
                return false;
            }
        }
        return true;
    }

}
