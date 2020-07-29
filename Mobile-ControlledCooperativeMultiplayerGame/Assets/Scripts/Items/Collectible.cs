using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    [SerializeField] Item item;

    public void Collect(ref Item item)
    {
        if (item?.GetType() != this.item.GetType())
        {
          Destroy(gameObject);
          item = Instantiate(this.item);
          item.gameObject.SetActive(false);
        }
    }
}
