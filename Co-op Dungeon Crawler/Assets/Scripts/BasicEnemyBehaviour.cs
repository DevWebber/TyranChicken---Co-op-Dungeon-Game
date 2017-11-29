using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

public class BasicEnemyBehaviour : NetworkBehaviour {
    /// <summary>
    /// Handles all the basic enemey behaviour, which just assumes all this guy wants to do is run at the player and touch them
    /// </summary>
    [SerializeField][SyncVar]
    private int enemyHealth;
    [SerializeField][SyncVar]
    private int enemyDamage;

    private bool invulnerable;
    private bool hasCollided;

    private Transform[] PlayerLocations;

    private Transform target;
    private Material[] enemyMaterial;
    private Color enemyColor;
    private PlayerBehaviour tempBehaviour;
    private bool isColliding = false;

    NavMeshAgent enemyAgent;
    Animator enemyAnimator;
    private bool enemyAttacking;

    public int EnemyDamage
    {
        get
        {
            return enemyDamage;
        }
    }
    
    public bool EnemyAttacking
    {
        get
        {
            return enemyAttacking;
        }
    }
    
	// Use this for initialization
	void Start()
    {
        enemyMaterial = new Material[transform.childCount];
        enemyAgent = GetComponent<NavMeshAgent>();
        enemyAnimator = GetComponent<Animator>();
        enemyAgent.speed = 5f;

        //This is supposed to find all the players on the server and give every enemy a reference to each of them

        int clientLength = FindObjectOfType<NetworkManager>().client.connection.playerControllers.Count;
        PlayerLocations = new Transform[clientLength];

        for (int i = 0; i < clientLength;  i++)
        {
            PlayerLocations[i] = FindObjectOfType<NetworkManager>().client.connection.playerControllers[i].gameObject.transform;
        }

        //Handles the visual feedback of enemies being hit
        for (int i = 0; i < transform.GetChild(0).childCount; i++)
        {
            enemyMaterial[i] = transform.GetChild(0).GetChild(i).GetComponent<Renderer>().material;
        }
        enemyColor = enemyMaterial[0].color;

        invulnerable = false;
        hasCollided = false;
        enemyAttacking = false;
	}
	
	//Ensures the enemy has a constant target
	void FixedUpdate()
    {
        if (PlayerLocations != null)
        {
            target = FindClosestPlayer();
        }

        if (target != null)
        {
            enemyAgent.SetDestination(target.position);

            if (enemyAgent.remainingDistance <= enemyAgent.stoppingDistance && !enemyAttacking)
            {
                CmdAttack();
            }
        }

        if (enemyAttacking)
        {
            transform.LookAt(target);
        }
    }

    private Transform FindClosestPlayer()
    {
        Transform closestPlayer;
        float tempDistance;
        float closestDistance;

        closestDistance = Vector3.Distance(transform.position, PlayerLocations[0].transform.position);
        closestPlayer = PlayerLocations[0];

        for (int i = 0; i < PlayerLocations.Length; i++)
        {
            tempDistance = Vector3.Distance(transform.position, PlayerLocations[i].transform.position);

            if (tempDistance > closestDistance)
            {
                closestPlayer = PlayerLocations[i];
            }
        }

        return closestPlayer;
    }

    [Command]
    private void CmdAttack()
    {
        enemyAttacking = true;
        enemyAgent.angularSpeed = 360f;
        enemyAgent.speed = 0f;
        enemyAnimator.SetBool("canAttack", true);
        Invoke("CmdResetAttack", 2f);
    }

    [Command]
    private void CmdResetAttack()
    {
        enemyAttacking = false;
        enemyAgent.angularSpeed = 120f;
        enemyAgent.speed = 5f;
        enemyAnimator.SetBool("canAttack", false);
    }


    //Handles the enemy touching the player. Later on this will be done by an attacking animation

    //NOTE - CHANGE TO LAYERS TO STOP CONSTANT COLLISIONS
    /*   private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.CompareTag("Player"))
            {
                tempBehaviour = collision.gameObject.GetComponentInParent<PlayerBehaviour>();

                collision.gameObject.GetComponentInParent<PlayerBehaviour>().TakeDamage(enemyDamage);
                hasCollided = true;
            }
        }
    */

    //Hurts the enemy if they are touched with a weapon that is attacking.
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Damage"))
        {
            tempBehaviour = collision.gameObject.GetComponentInParent<PlayerBehaviour>();

            Debug.Log(tempBehaviour.IsAnimationPlaying);
            if (tempBehaviour.IsAnimationPlaying)
            {
                TakeDamage(tempBehaviour.PlayerDamage);
            }

            isColliding = true;
        }
    }

    //If the weapon is still stuck inside the enemy and the player trys to swing again, take damage. Another collider issue.
/*    private void OnTriggerStay(Collider collision)
    {
        if (isColliding && tempBehaviour != null)
        {
            if (tempBehaviour.IsAnimationPlaying)
            {
                TakeDamage(tempBehaviour.PlayerDamage);
            }
        }
    }
*/

    private void OnTriggerExit()
    {
        isColliding = false;
    }

    //Takes damage method, same as the player.
    public void TakeDamage(int damage)
    {
        if (!invulnerable)
        {
            enemyHealth -= damage;

            if (enemyHealth <= 0)
            {
                CmdDeath();
            }
            else
            {
                invulnerable = true;
                ChangeColor(Color.red);

                Vector3 forceVector = (transform.position - target.position).normalized;
                forceVector.y = 0;
                transform.GetComponent<Rigidbody>().AddForce(forceVector * 300f);

                Invoke("ResetInvulnerable", 1f);
            }
        }

    }

    private void ResetInvulnerable()
    {
        invulnerable = false;
        ChangeColor(enemyColor);
        transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
        transform.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }

    //A very crude way of doing it, this will improve later on.
    [Command]
    private void CmdDeath()
    {
        Destroy(gameObject);
    }

    private void ChangeColor(Color colorToChange)
    {
        for (int i = 0; i < enemyMaterial.Length; i++)
        {
            enemyMaterial[i].color = colorToChange;
        }
    }
}
