using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Throwable
{
    IEnumerator PickUpCoroutine(Vector2 ontop, HingeJoint2D joint, BoxCollider2D collider);
    IEnumerator ThrowCoroutine(Vector2 direction, BoxCollider2D collider);
}
