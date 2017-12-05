using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationController : StateMachineBehaviour {

    private PlayerBehaviour localPlayerBehaviour;
    private float animationLength;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        localPlayerBehaviour = animator.GetComponentInParent<PlayerBehaviour>();

        if (stateInfo.IsName("SecondSwing") && animator.GetBool("IsThirdSwing") == false)
        {
            animator.SetBool("IsThirdSwing", false);
        }
    }

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {/*
        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !animator.IsInTransition(0))
        {
            if (stateInfo.IsName("FirstSwing") && animator.GetBool("IsSecondSwing") == false)
            {
                animator.SetBool("IsFirstSwing", false);
            }
            if (stateInfo.IsName("SecondSwing") && animator.GetBool("IsThirdSwing") == false)
            {
                animator.SetBool("IsSecondSwing", false);
            }
        }
        */
    }

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
/*	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.IsName("FirstSwing") || stateInfo.IsName("SecondSwing") || stateInfo.IsName("ThirdSwing"))
        {
            ChangeState(false);
        }
    }
*/

    private void ChangeState(bool isPlaying)
    {
        localPlayerBehaviour.IsAnimationPlaying = isPlaying;
    }

	// OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
	//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
	//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}
}
