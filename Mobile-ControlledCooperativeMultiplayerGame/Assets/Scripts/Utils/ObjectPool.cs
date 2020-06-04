using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/**
 * Object pools are used when lots of instances of a prefab must be drawn and the cost of frequent instantiations
 * and object destructions shall be avoided. For example in games where there are lots of projectiles.
 *
 * Instances can be retrieved from the pool by `GetInstance`.
 * As soon as you don't need the instance anymore, dont destroy it, but return it using `ReturnInstance`.
 *
 * This way, instances can be reused.
 */
public class ObjectPool : MonoBehaviour
{
    // Prefab that is instantiated by this pool
    [SerializeField] private GameObject prefab;

    // We store returned instances here until they are needed again
    private Stack<GameObject> _unusedInstances = new Stack<GameObject>();

    public GameObject Prefab => prefab;

    /**
     * Returns an instance of `prefab`.
     * If no pre-allocated instances are stored, a new one will be created.
     * Always return the instance with `ReturnInstance` when you don't need it anymore.
     *
     * @param parent   set a parent for the instance (optional)
     * @param activate whether the instance shall be activated (SetActive)
     */
    public GameObject GetInstance(Transform parent = null, bool activate = true)
    {
        GameObject instance;
        if (_unusedInstances.Any())
        {
            instance = _unusedInstances.Pop();
            instance.transform.SetParent(parent);
        }

        else if (parent != null)
        {
            instance = Instantiate(prefab, parent);
        }

        else
        {
            instance = Instantiate(prefab);
        }

        if (activate)
        {
            instance.SetActive(true);
        }

        return instance;
    }

    /**
     * Stores prefab instances for reuse. See also `GetInstance`.
     */
    public void ReturnInstance(GameObject instance, bool deactivate = true)
    {
        if (deactivate)
        {
            instance.SetActive(false);
        }

        _unusedInstances.Push(instance);
    }
}


