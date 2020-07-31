using UnityEngine;

public class ItemInstance<I> : MonoBehaviour where I : Item, new()
{
    public I GetItem()
    {
        return new I();
    }
}
