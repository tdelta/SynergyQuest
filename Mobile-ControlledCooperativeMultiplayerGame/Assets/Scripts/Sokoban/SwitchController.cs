using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchController : MonoBehaviour
{

    bool pressed;

    public Transform collisionPointCenter;
    public float collisionPointRadius;

    private Collider2D[] colliders;

    public void Start()
    {
        pressed = false;

    }

    public void Update()
    {
        pressed = false;
        colliders = Physics2D.OverlapCircleAll(collisionPointCenter.position, collisionPointRadius);

        foreach (var collider in colliders)
        {
            if (collider.gameObject.tag == "Box"){
                pressed = true;
            }
        }
    }
    void OnDrawGizmosSelected()
    {
        //Gizmos.DrawSphere(collisionPointCenter.position, collisionPointRadius);
    }

    public bool isPressed(){
        return pressed;
    }
}
