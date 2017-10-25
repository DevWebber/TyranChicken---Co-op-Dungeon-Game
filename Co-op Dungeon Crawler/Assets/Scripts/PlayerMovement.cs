using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMovement : MonoBehaviour {

    float v_xAxis;
    float v_zAxis;
    [SerializeField]
    private float v_movementSpeed;

	NavMeshAgent agent;

	void Start()
	{
		agent = GetComponent<NavMeshAgent> ();
	}

	void Update()
    {

		
        v_xAxis = Input.GetAxis("Horizontal") * Time.deltaTime * v_movementSpeed;
        v_zAxis = Input.GetAxis("Vertical") * Time.deltaTime * v_movementSpeed;

        agent.Move(new Vector3(v_xAxis, 0, v_zAxis));

        /*
        transform.Translate(v_xAxis, 0, v_zAxis);
        */
	}
}
