using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMovement : MonoBehaviour {
    /// <summary>
    /// Moves the player in 4 directions using the Unity Navigation Mesh
    /// </summary>
    float v_xAxis;
    float v_zAxis;
    [SerializeField]
    private float movementSpeed;
    [SerializeField]
    private float rotationSpeed;

    Vector3 nextDirection;
    private Animation playerAnim;
    private bool playerWalking;

	NavMeshAgent playerAgent;

	void Start()
	{
        playerAgent = GetComponent<NavMeshAgent>();
        playerAnim = GetComponent<Animation>();

        playerWalking = false;
	}

	void Update()
    {	
        v_xAxis = Input.GetAxis("Horizontal") * Time.deltaTime * movementSpeed;
        v_zAxis = Input.GetAxis("Vertical") * Time.deltaTime * movementSpeed;

        playerAgent.Move(new Vector3(v_xAxis, 0, v_zAxis));

        nextDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

        if (nextDirection != Vector3.zero)
        {
            Quaternion newRotation = Quaternion.LookRotation(-nextDirection);

            transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.deltaTime * rotationSpeed);

            //transform.rotation = Quaternion.FromToRotation(transform.position, nextDirection);
            
        }
	}

    void LateUpdate()
    {
        //This starts and stops the animation when the player is moving.
        if (Vector3.Distance(nextDirection, Vector3.zero) > 0.1f && !playerWalking)
        {
            playerAnim.Blend("Player Walking");
            playerWalking = true;
        }
        else if (Vector3.Distance(nextDirection, Vector3.zero) < 0.1f && playerWalking)
        {
            playerAnim.Stop("Player Walking");
            playerWalking = false;
            //This doesn't work yet, the idea is when the player stops moving the animation goes back to the beginning.
            playerAnim.Rewind("Player Walking");
        }
    }
}


