using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchController : MonoBehaviour
{

    bool pressed;

    public Transform collisionPointCenter;
    public float collisionPointRadius;

    private Collider2D[] colliders;

    public objectColor color;

    public void Start()
    {
        pressed = false;

    }

    public void Update()
    {
        pressed = false;
        // FIXME: Physics calculation in every graphics frame??? Why not instead us onCollisionEnter?
        colliders = Physics2D.OverlapCircleAll(collisionPointCenter.position, collisionPointRadius);

        foreach (var collider in colliders)
        {
            if (collider.gameObject.tag == "Box"){
                BoxController box = collider.gameObject.GetComponent<BoxController>();
                if(box.getColor() == this.GetColor()) {
                    pressed = true;
                }
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

    public objectColor GetColor(){
        return color;
    }
}
