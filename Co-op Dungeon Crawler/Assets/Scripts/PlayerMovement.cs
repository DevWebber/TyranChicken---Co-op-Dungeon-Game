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

    //The animator and a boolean for whether the player is walking or not
    //private Animator playerAnim;
    private bool playerWalking;

	NavMeshAgent playerAgent;

    private Vector3 mousePosition;
    private Vector3 screenPosition;
    private float playerRotationAngle;

    private PlayerBehaviour behaviourScript;


    void Start()
	{
        //Assigns the navmesh and the animator
        playerAgent = GetComponent<NavMeshAgent>();
        //playerAnim = GetComponent<Animator>();
        behaviourScript = GetComponent<PlayerBehaviour>();

        //Start off not walking
        playerWalking = false;
	}

	void Update()
    {	
        //if it isn't the local player, don't do anything. We only want this for the local player
        if (!isLocalPlayer)
        {
            return;
        }

        if (!behaviourScript.IsAnimationPlaying)
        {

            //Sets our X and Y axis for movement input.
            xAxis = Input.GetAxis("Horizontal") * Time.deltaTime * movementSpeed;
            zAxis = Input.GetAxis("Vertical") * Time.deltaTime * movementSpeed;

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
            transform.rotation = Quaternion.Euler(new Vector3(0, -playerRotationAngle - 270f, 0));
        }


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
        //This starts and stops the animation when the player is moving. It's very crude
        if (xAxis >= 0.01f && xAxis <= -0.01f && zAxis >= 0.01f && zAxis <= -0.01f && !playerWalking)
        {
            //playerAnim.Blend("Player Walking");
            playerWalking = true;
        }
        else if (xAxis < 0.01f && xAxis > -0.01f && zAxis < 0.01f && zAxis > -0.01f && playerWalking)
        {
            //playerAnim.Stop("Player Walking");
            playerWalking = false;
            //This doesn't work yet, the idea is when the player stops moving the animation goes back to the beginning.
            //playerAnim.Rewind("Player Walking");
        }
    }

    public override void OnStartLocalPlayer()
    {
       //Rip the hat
       // if (transform.Find("Hat").gameObject != null)
       // {
       //    transform.Find("Hat").gameObject.SetActive(true);
       // }

        //Assigns the cameras target to be this transform
        Camera.main.GetComponent<CameraFollow>().AssignTarget(transform);
    }

}


