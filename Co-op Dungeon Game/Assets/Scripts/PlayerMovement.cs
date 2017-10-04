using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    float xAxis;
    float zAxis;
    [SerializeField]
    private float movementSpeed;
	
	void Update()
    {
        xAxis = Input.GetAxis("Horizontal") * Time.deltaTime * movementSpeed;
        zAxis = Input.GetAxis("Vertical") * Time.deltaTime * movementSpeed;

        transform.Translate(xAxis, 0, zAxis);
	}
}
