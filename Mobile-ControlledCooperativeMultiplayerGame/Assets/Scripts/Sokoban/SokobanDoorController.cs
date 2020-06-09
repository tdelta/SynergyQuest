using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SokobanDoorController : MonoBehaviour
{

    public Sprite openedDoor;
    public Sprite closedDoor;

    public SwitchController[] switches;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (sokobanSolved()){
            this.GetComponent<SpriteRenderer>().sprite = openedDoor;
        } else {
            this.GetComponent<SpriteRenderer>().sprite = closedDoor;
        }
    }

    bool sokobanSolved(){
        foreach (var s in switches)
        {
            if(!s.isPressed()) {
                return false;
            }
        }
        return true;
    }

}
