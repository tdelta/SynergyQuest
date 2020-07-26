using System;
using UnityEngine;

/**
 * Can be assigned an object which it will then move along with it at relative distance while disabling normal physics.
 * It is used by `Pushable` to move the player character along with a box when it is being pulled.
 *
 * Comment by Anton:
 *   Maybe we should use a `FixedJoint2D` instead, which Marc did in the original implementation of
 *   Sokoban boxes.
 *   However, for now it seems to work and I haven't gotten around yet to adapt the Pushable class to use a FixedJoint2D.
 *   If something goes horribly wrong here we probably should try using a Joint instead first.
 */
public class MovementBinder : MonoBehaviour
{
    /**
     * Object which is moved along this one. It is null as long as no object has been bound
     */
    private GameObject _boundObject;
    /**
     * The relative offset of the bound object from the binder.
     */
    private Vector3 _objectOffset = Vector3.zero;

    private void Update()
    {
        // If an object has been bound
        if (!ReferenceEquals(_boundObject, null)) // `ReferenceEquals` is supposedly faster than a != null check
        {
            // Move it along every frame
            _boundObject.transform.position = transform.position + _objectOffset;
        }
    }

    /**
     * Binds an object to this binder, that is, if this binder moves, the object does too at a relative distance.
     * If the object has a rigidbody, we disable that.
     */
    public void Bind(GameObject obj)
    {
        _boundObject = obj;
        // remember the relative offset of the object
        _objectOffset = _boundObject.transform.position - transform.position;

        // If there is a rigidbody on the object, temporarily disable it
        var body = _boundObject.GetComponent<Rigidbody2D>();
        if (!ReferenceEquals(body, null))
        {
            body.simulated = false;
        }
    }

    /**
     * Unbinds an object from this binder, see also `Bind`.
     * If the object has a rigidbody, we reenable that.
     */
    public void Unbind()
    {
        if (ReferenceEquals(_boundObject, null)) return;
        
        // If there is a rigidbody on the object, enable it
        var body = _boundObject.GetComponent<Rigidbody2D>();
        if (!ReferenceEquals(body, null))
        {
            body.simulated = true;
        }
        
        _boundObject = null;
    }

    /**
     * Returns true if the binder is currently manipulating an object
     */
    public bool IsActive()
    {
        return !ReferenceEquals(_boundObject, null);
    }
}
