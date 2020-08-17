using System;
using System.Linq;
using UnityEngine;

/**
 * Utility class which observes the state of switches (see the `Switch` class).
 * It triggers an event, if all registered switches are activated at once.
 *
 * See example the `DeadManSwitch` for an usage example.
 */
public class Switchable : MonoBehaviour
{
    /**
     * Switches which shall be observed.
     */
    [SerializeField] private Switch[] switches = new Switch[0];

    /**
     * Automatically find all switches in the scene who have a tag of the given name and bind to them
     */
    [SerializeField] private string[] autoDiscoveredSwitchTags = new string[0];
    
    /**
     * Event which is invoked if all registered switches are active at the same time.
     */
    public event ActivationChangedAction OnActivationChanged;
    public delegate void ActivationChangedAction(bool activation);

    /**
     * Stores, whether all observed switches are currently active
     */
    private bool _activation = true;
    public bool Activation => _activation;
    
    /**
     * Caches the current value of every switch
     */
    private bool[] _switchValues;
    /**
     * Caches the event handlers we registered on every switch, so that we can unregister them in `OnDisable`.
     */
    private Switch.ValueChangedAction[] _switchChangeHandlers;

    void Awake()
    {
        BuildSwitches();
    }

    private void BuildSwitches()
    {
        // Auto-discover switches by tags and add them to the manually configured ones
        switches = autoDiscoveredSwitchTags
            .SelectMany(GameObject.FindGameObjectsWithTag)
            .Select(gameObject => gameObject.GetComponent<Switch>())
            .Concat(switches)
            .Distinct() // Have no duplicate switches
            .ToArray();

        // Allocate memory in our caches for every switch
        _switchValues = new bool[switches.Length];
        _switchChangeHandlers = new Switch.ValueChangedAction[switches.Length];
    }

    public void AddSwitches(params Switch[] additionalSwitches)
    {
        OnDisable();
        switches = switches.Concat(additionalSwitches).ToArray();
        BuildSwitches();
        OnEnable();
    }

    private void Start()
    {
        ComputeActivation();
    }

    private void OnEnable()
    {
        // For every switch which shall be observed, register an event handler
        for (int i = 0; i < switches.Length; ++i)
        {
            var switchObj = switches[i];
            if ( switchObj != null )
            {
                _switchValues[i] = false;
                
                var localI = i;
                _switchChangeHandlers[i] = value =>
                {
                    OnSwitchValueChanged(localI, value);
                };

                switchObj.OnValueChanged += _switchChangeHandlers[i];
            }
        }
    }

    private void OnDisable()
    {
        // For every switch which shall is observed, unregister our event handler
        for (int i = 0; i < switches.Length; ++i)
        {
            var switchObj = switches[i];
            if (i < _switchChangeHandlers.Length && switchObj != null)
            {
                switchObj.OnValueChanged -= _switchChangeHandlers[i];
            }
        }
    }

    /**
     * Determine, whether all observed switches are currently activated at once
     */
    void ComputeActivation()
    {
        var oldActivation = _activation;
        _activation = _switchValues.All(v => v);

        // If the activation state of this component changed, inform all subscribers
        if (oldActivation != _activation)
        {
            OnActivationChanged?.Invoke(_activation);
        }
    }

    /**
     * Called, if an observed switch changes its value.
     */
    void OnSwitchValueChanged(int switchIdx, bool value)
    {
        // Store the new switch value
        _switchValues[switchIdx] = value;
        // Determine, if now all switches are pressed.
        ComputeActivation();
    }
}
