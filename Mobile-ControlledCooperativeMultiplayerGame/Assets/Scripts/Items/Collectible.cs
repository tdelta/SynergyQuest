using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    [SerializeField] Item item;

    public void Collect(PlayerController player)
    {
        Destroy(gameObject);
        item.Collect(player);
    }
}
