using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CameraFollow : NetworkBehaviour {
    /// <summary>
    /// This class controls all the camera based mechancis. This includes the following of the player
    /// and also the hiding of objects that are in the cameras way of the view to the player (mainly walls and corridors)
    /// </summary>
    
    [SerializeField]
    private Transform playerTarget;

    [SerializeField]
    private float yOffset;
    [SerializeField]
    private float zOffset;
    [SerializeField]
    private float cameraAngle;

    //Direction and Distance from camera to player. Used for the raycast
    private Vector3 playerDirection;
    private float playerDistance;
    //Layermask to only count walls (so we don't start making invisible floors)
    private int layerMask = 1 << 8;
    private Transform tempTransform;
    private RaycastHit[] rayHits;

    //Stores all the objects that we hide so we can re-enable them once they're not longer obstructing the view
    private List<Transform> hiddenObjects;

    void Start()
    {
        hiddenObjects = new List<Transform>();
    }

    void Update()
    {
        if (playerTarget != null)
        {
            //Updates the positon of the camera to always be above the player at an offset.
            transform.position = new Vector3(playerTarget.position.x, yOffset, playerTarget.position.z - zOffset);
            transform.rotation = Quaternion.Euler(cameraAngle, 0, 0);

            playerDirection = playerTarget.position - transform.position;
            playerDistance = playerDirection.magnitude;

            //Casts a raycast to hit from the camera to the player.
            rayHits = Physics.RaycastAll(transform.position, playerDirection, playerDistance, layerMask);
            Debug.DrawRay(transform.position, playerDirection, Color.red);

            //Checks all the raycast hits on walls
            for (int i = 0; i < rayHits.Length; i++)
            {
                tempTransform = rayHits[i].transform;
                //If the hit wall is not part of the hidden list
                if (!hiddenObjects.Contains(tempTransform))
                {
                    //Add it to the list and make it invisible
                    hiddenObjects.Add(tempTransform);
                    tempTransform.GetComponent<Renderer>().enabled = false;
                }
            }

            //Check the list of hidden objects that include some that aren't hit but still in the list
            for (int i = 0; i < hiddenObjects.Count; i++)
            {
                bool isInvisible = false;

                //Check all the current hits
                for (int j = 0; j < rayHits.Length; j++)
                {
                    //See if any hits are still invisible
                    if (rayHits[j].transform == hiddenObjects[i])
                    {
                        //If it is, then ignore it. Still needs to be invisible
                        isInvisible = true;
                        break;
                    }
                }

                //If a hidden object that isn't being hit is found, enable it again and remove it from the list
                if (!isInvisible)
                {
                    tempTransform = hiddenObjects[i];
                    tempTransform.GetComponent<Renderer>().enabled = true;
                    hiddenObjects.RemoveAt(i);
                    i--;
                }
            }
        }
    }

    //When this is called, set the target of the camera to whatever is sent through.
    public void AssignTarget(Transform target)
    {
        playerTarget = target;
    }

}
