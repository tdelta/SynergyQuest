using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hider : MonoBehaviour
{
    // List of objects which will be made invisible / visible
    public List<MonoBehaviour> objects;
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

    private void OnActivationChanged(bool activation)
    {
        if (activation) {
            foreach (var o in objects) {
                if (o != null) {
                    Debug.Log("Aktiviert!");
                    o.MakeVisible();
                }
            }
        }
        else {
            foreach (var o in objects) {
                if (o != null) {
                    Debug.Log("Deaktiviert!");
                    o.MakeInvisible();
                }
            }
        }
    }
}
