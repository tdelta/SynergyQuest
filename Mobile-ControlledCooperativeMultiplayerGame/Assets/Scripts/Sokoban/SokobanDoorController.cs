using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// DEPRECATED! Use Switchable Door!
/*
public class SokobanDoorController : DoorController
{
    [SerializeField] private SwitchController[] switches;

    private void Awake()
    {
        // If no switches have been configured manually, try to find them all at runtime
        if (!switches.Any())
        {
            switches = FindObjectsOfType<SwitchController>();
        }
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
            OpenDoor();
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
*/