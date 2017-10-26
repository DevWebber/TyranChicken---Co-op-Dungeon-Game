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
    private float v_movementSpeed;

	NavMeshAgent playerAgent;

	void Start()
	{
        playerAgent = GetComponent<NavMeshAgent>();
	}

	void Update()
    {	
        v_xAxis = Input.GetAxis("Horizontal") * Time.deltaTime * v_movementSpeed;
        v_zAxis = Input.GetAxis("Vertical") * Time.deltaTime * v_movementSpeed;

        playerAgent.Move(new Vector3(v_xAxis, 0, v_zAxis));
	}
}
