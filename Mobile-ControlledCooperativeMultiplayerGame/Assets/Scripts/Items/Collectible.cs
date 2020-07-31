using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    [SerializeField] private Item item;
    public Item Item => item;

    public Item Collect()
    {
        Destroy(gameObject);
        return item;
    }
}
