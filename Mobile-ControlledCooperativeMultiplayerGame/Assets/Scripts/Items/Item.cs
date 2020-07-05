using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Item : MonoBehaviour
{
    // must be implemented to realize cooldown
    public abstract bool Ready();
}
