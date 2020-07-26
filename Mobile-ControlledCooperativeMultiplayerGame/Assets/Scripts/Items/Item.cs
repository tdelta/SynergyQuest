using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Item : MonoBehaviour
{
    // must be implemented to realize cooldown
    public abstract bool Ready();

    // Each item has to specify a button to activate it
    public abstract Button GetButton();
}
