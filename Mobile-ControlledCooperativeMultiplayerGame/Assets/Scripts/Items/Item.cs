using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Item : MonoBehaviour
{
    // must be implemented to realize cooldown
    public abstract bool Ready();

    // TODO: currently an Item instantiates itself, which is a bit confusing
    // a better solution would be to introduce an additional class of indirection
    public abstract Item Instantiate(Vector2 coords);
}
