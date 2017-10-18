using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    float v_xAxis;
    float v_zAxis;
    [SerializeField]
    private float v_movementSpeed;
	
	void Update()
    {
        v_xAxis = Input.GetAxis("Horizontal") * Time.deltaTime * v_movementSpeed;
        v_zAxis = Input.GetAxis("Vertical") * Time.deltaTime * v_movementSpeed;

        transform.Translate(v_xAxis, 0, v_zAxis);
	}
}
