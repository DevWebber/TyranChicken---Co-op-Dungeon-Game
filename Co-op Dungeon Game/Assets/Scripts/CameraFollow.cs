using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

    [SerializeField]
    private Transform target;

    [SerializeField]
    private float yOffset;
    [SerializeField]
    private float zOffset;
	
	void Update()
    {
        transform.position = new Vector3(target.position.x, yOffset, target.position.z - zOffset);

	}
}
