using System.Runtime.Remoting;
using UnityEngine;

abstract public class Item : MonoBehaviour
{
    protected ItemDescription ItemDescription { get; private set; }
    
    public void Activate(PlayerController player, ItemDescription itemDescription)
    {
        ItemDescription = itemDescription;
        OnActivate(player);
    }
    
    public abstract void OnActivate(PlayerController player);
}
