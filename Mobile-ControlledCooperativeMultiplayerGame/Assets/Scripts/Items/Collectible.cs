using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    [SerializeField] Item item;

    public Item Collect()
    {
        Destroy(gameObject);
        var clone = Instantiate(item);
        clone.gameObject.SetActive(false);
        return clone;
    }
}
