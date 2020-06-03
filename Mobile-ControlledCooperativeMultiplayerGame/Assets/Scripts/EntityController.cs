using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class EntityController : MonoBehaviour {
    public float knockbackFactor = 1;

    public abstract void putDamage(int amount, Vector2 knockbackDirection);
}
