using UnityEngine;

/**
 * Utility component which represents a switch which can be activated or deactivated by players.
 * It only provides the logic to propagate the state of the switch to other components which use the `Switchable`
 * component:
 *
 * The change is propagated by providing an event `OnValueChange`, which other objects can subscribe to.
 *
 * The logic which triggers a change of the state of the switch must be provided by another component, see for example
 * the `DeadManSwitch` class.
 * The change can be triggered by setting the `Value` property.
 */
public class Switch : MonoBehaviour
{
    /**
     * Current value of the switch (activated / not activated)
     */
    [SerializeField] private bool value = false;
    
    /**
     * Event which is triggered when the state of this switch changes
     */
    public event ValueChangedAction OnValueChanged;
    public delegate void ValueChangedAction(bool value);

    public bool Value
    {
        get => value;
        set
        {
            var oldValue = this.value;
            this.value = value;

            // Only trigger the `OnValueChange` event, if the value actually changed.
            // Hence it does not trigger if the same value is set as before.
            if (oldValue != this.value)
            {
                OnValueChanged?.Invoke(value);
            }
        }
    }

    void Start()
    {
        OnValueChanged?.Invoke(Value);
    }
}
