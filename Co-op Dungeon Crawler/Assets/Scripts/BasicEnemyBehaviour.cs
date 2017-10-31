using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BasicEnemyBehaviour : MonoBehaviour {
    /// <summary>
    /// Handles all the basic enemey behaviour, which just assumes all this guy wants to do is run at the player and touch them
    /// </summary>
    [SerializeField]
    private int enemyHealth;
    [SerializeField]
    private int enemyDamage;

    private bool invulnerable;
    private bool hasCollided;

    private Transform target;
    private Material[] enemyMaterial;
    private Color enemyColor;
    private PlayerBehaviour tempBehaviour;
    private bool isColliding = false;

    NavMeshAgent enemyAgent;
    
	// Use this for initialization
	void Start()
    {
        enemyMaterial = new Material[transform.childCount];
        enemyAgent = GetComponent<NavMeshAgent>();
        target = GameObject.FindGameObjectWithTag("Player").transform;

        for (int i = 0; i < transform.childCount; i++)
        {
            enemyMaterial[i] = transform.GetChild(i).GetComponent<Renderer>().material;
        }
        enemyColor = enemyMaterial[0].color;

        invulnerable = false;
        hasCollided = false;
	}
	
	//Ensures the enemy has a constant target
	void FixedUpdate()
    {
		if (target != null)
        {
            enemyAgent.SetDestination(target.position);

            //If the player is right next to the enemy, keep taking damage. Change this if we move away from colliders
            if (enemyAgent.remainingDistance < 2f && hasCollided)
            {
                target.gameObject.GetComponent<PlayerBehaviour>().TakeDamage(enemyDamage);
            }
        }
    }

    //Handles the enemy touching the player. Later on this will be done by an attacking animation
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            tempBehaviour = collision.gameObject.GetComponentInParent<PlayerBehaviour>();

            collision.gameObject.GetComponentInParent<PlayerBehaviour>().TakeDamage(enemyDamage);
            hasCollided = true;
        }
    }

    //Hurts the enemy if they are touched with a weapon that is attacking.
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Damage"))
        {
            tempBehaviour = collision.gameObject.GetComponentInParent<PlayerBehaviour>();

            if (tempBehaviour.IsAnimationPlaying)
            {
                TakeDamage(1);
            }

            isColliding = true;
        }
    }

    //If the weapon is still stuck inside the enemy and the player trys to swing again, take damage. Another collider issue.
    private void OnTriggerStay(Collider collision)
    {
        if (isColliding && tempBehaviour != null)
        {
            if (tempBehaviour.IsAnimationPlaying)
            {
                TakeDamage(1);
            }
        }
    }

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
                Death();
            }
            else
            {
                invulnerable = true;
                ChangeColor(Color.red);

                Invoke("ResetInvulnerable", 1f);
            }
        }

    }

    private void ResetInvulnerable()
    {
        invulnerable = false;
        ChangeColor(enemyColor);
    }

    //A very crude way of doing it, this will improve later on.
    private void Death()
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
