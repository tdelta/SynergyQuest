using System;
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
     * If set, this switch will remember its value when switching scenes at runtime.
     * Use this property to keep doors opened, if a puzzle has already been solved
     */
    [SerializeField] private bool isPersistentAcrossScenes = false;

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
                if (isPersistentAcrossScenes)
                {
                    DungeonDataKeeper.Instance.SaveSwitchActivation(this);
                }
                OnValueChanged?.Invoke(value);
            }
        }
    }

    public Guid Guid { get; private set; }

    private void Awake()
    {
        Guid = GetComponent<Guid>();
        isPersistentAcrossScenes = !(Guid is null) && isPersistentAcrossScenes;
    }

    private void Start()
    {
        if (isPersistentAcrossScenes)
        {
            Value = DungeonDataKeeper.Instance.HasSwitchBeenActivated(this, Value);
        }
    }

    private void OnValidate()
    {
        if (isPersistentAcrossScenes && GetComponent<Guid>() is null)
        {
            isPersistentAcrossScenes = false;
            Debug.LogError("Can not make switch persistent, if there is no Guid component.");
        }
    }
}
