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

	NavMeshAgent playerAgent;

	void Start()
	{
        playerAgent = GetComponent<NavMeshAgent>();
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
}
