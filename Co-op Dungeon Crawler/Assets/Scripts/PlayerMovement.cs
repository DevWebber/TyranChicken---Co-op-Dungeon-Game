using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class PlayerMovement : NetworkBehaviour {
    /// <summary>
    /// Moves the player in 4 directions using the Unity Navigation Mesh
    /// </summary>
    [SerializeField]
    private float movementSpeed;
    [SerializeField]
    private float rotationSpeed;
    private float xAxis;
    private float zAxis;

    //Vector3 nextDirection;
    private Animation playerAnim;
    private bool playerWalking;

	NavMeshAgent playerAgent;

    private Vector3 mousePosition;
    private Vector3 screenPosition;
    private float playerRotationAngle;


    void Start()
	{
        playerAgent = GetComponent<NavMeshAgent>();
        playerAnim = GetComponent<Animation>();

        playerWalking = false;
	}

	void Update()
    {	
        if (!isLocalPlayer)
        {
            return;
        }

        //Sets our X and Y axis for movement input.
        xAxis = Input.GetAxis("Horizontal") * Time.deltaTime * movementSpeed;
        zAxis= Input.GetAxis("Vertical") * Time.deltaTime * movementSpeed;

        //This uses the navigation mesh to move so the player is aware of walls and obstacles.
        playerAgent.Move(new Vector3(xAxis, 0, zAxis));

        //Gets the input of the mouse and the position of the player to screen space
        mousePosition = Input.mousePosition;
        screenPosition = Camera.main.WorldToScreenPoint(transform.position);

        mousePosition.x -= screenPosition.x;
        mousePosition.y -= screenPosition.y;

        //Some math to get the angle between these two points
        playerRotationAngle = Mathf.Atan2(mousePosition.y, mousePosition.x) * Mathf.Rad2Deg;

        //How does this even work I don't know, please don't question it is just does.
        transform.rotation = Quaternion.Euler(new Vector3(0, -playerRotationAngle - 90f, 0));



        /*Legacy code in case the above code breaks. This code rotates the player based on the direction they are facing
        nextDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

        if (nextDirection != Vector3.zero)
        {
            Quaternion newRotation = Quaternion.LookRotation(-nextDirection)

            transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.deltaTime * rotationSpeed);        
        }
        */
    }

    //ToFix - player walking animation.
    void LateUpdate()
    {
        //This starts and stops the animation when the player is moving.
        if (xAxis >= 0.01f && xAxis <= -0.01f && zAxis >= 0.01f && zAxis <= -0.01f && !playerWalking)
        {
            playerAnim.Blend("Player Walking");
            playerWalking = true;
        }
        else if (xAxis < 0.01f && xAxis > -0.01f && zAxis < 0.01f && zAxis > -0.01f && playerWalking)
        {
            playerAnim.Stop("Player Walking");
            playerWalking = false;
            //This doesn't work yet, the idea is when the player stops moving the animation goes back to the beginning.
            playerAnim.Rewind("Player Walking");
        }
    }

    public override void OnStartLocalPlayer()
    {
        transform.Find("Hat").gameObject.SetActive(true);

        Camera.main.GetComponent<CameraFollow>().AssignTarget(transform);
    }

}


