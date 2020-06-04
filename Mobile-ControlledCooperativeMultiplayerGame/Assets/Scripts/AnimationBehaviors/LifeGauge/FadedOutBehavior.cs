using UnityEngine;

/**
 * Logic of the last state of the fade-out animation of the LifeGauge / Heart bar.
 */
public class FadedOutBehavior : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Inform LifeGauge logic, that the Fade-Out animation completed
        animator.gameObject.GetComponent<LifeGauge>().OnFadedOut();
    }
}
