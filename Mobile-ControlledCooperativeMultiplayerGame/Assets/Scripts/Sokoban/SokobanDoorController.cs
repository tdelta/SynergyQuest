using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SokobanDoorController : DoorController
{
    [SerializeField] private SwitchController[] switches;

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
