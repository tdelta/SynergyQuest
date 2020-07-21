using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockSwitch : MonoBehaviour
{

    [SerializeField] uint timeout = 0;
    Switch switcher;
    Animator animator;

    readonly int hitTrigger = Animator.StringToHash("Hit");
    readonly int timeoutTrigger = Animator.StringToHash("Timeout");

    void Awake()
    {
        switcher = GetComponent<Switch>();
        animator = GetComponent<Animator>();
    }

    public void Activate()
    {
        animator.SetTrigger(hitTrigger);
        switcher.Value = true;

        if (timeout > 0)
        {
            StopCoroutine("StartTimer");
            StartCoroutine("StartTimer");
        }
    }

    IEnumerator StartTimer()
    {
        yield return new WaitForSeconds(timeout);
        switcher.Value = false;
        animator.SetTrigger(timeoutTrigger);
        Debug.Log("Deactivated");
    }

}
