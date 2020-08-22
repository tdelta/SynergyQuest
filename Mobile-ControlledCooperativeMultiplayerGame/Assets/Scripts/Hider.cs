using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Switchable))]
public class Hider : MonoBehaviour
{
    // List of objects which will be made invisible / visible
    [SerializeField] private List<GameObject> objects = default;
    
    private Switchable _switchable;

    // Start is called before the first frame update
    void Awake()
    {
        _switchable = this.GetComponent<Switchable>();    
    }

    private void OnEnable()
    {
        _switchable.OnActivationChanged += OnActivationChanged;
    }
    
    private void OnDisable()
    {
        _switchable.OnActivationChanged -= OnActivationChanged;
    }

    private void Start()
    {
        // Apply the initial activation to all managed objects
        foreach (var o in objects.Where(o => o != null))
        {
            o.SetVisibility(_switchable.Activation);
        }
    }

    private void OnActivationChanged(bool activation)
    {
        foreach (var o in objects.Where(o => o != null))
        {
            if (Teleport.SupportsTeleportEffect(o))
            {
                if (activation)
                {
                    Teleport.TeleportIn(o, Color.cyan);
                }

                else
                {
                    Teleport.TeleportOut(o, Color.cyan);
                }
            }

            else
            {
                o.SetVisibility(activation);
            }
        }
    }
}
