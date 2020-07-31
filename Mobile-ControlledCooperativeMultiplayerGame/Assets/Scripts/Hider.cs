using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Switchable))]
public class Hider : MonoBehaviour
{
    // List of objects which will be made invisible / visible
    [SerializeField] private List<GameObject> objects;
    
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
        foreach (var o in objects.Where(o => o != null))
        {
            o.SetVisibility(activation);
        }
    }
}
