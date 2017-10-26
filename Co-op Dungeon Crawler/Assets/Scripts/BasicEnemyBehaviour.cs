using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BasicEnemyBehaviour : MonoBehaviour {

    [SerializeField]
    private int enemyHealth;
    [SerializeField]
    private int enemyDamage;

    private bool invulnerable;
    private bool hasCollided;

    private Transform target;
    private Material[] enemyMaterial;
    private Color enemyColor;

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
	
	// Update is called once per frame
	void FixedUpdate()
    {
		if (target != null)
        {
            enemyAgent.SetDestination(target.position);

            if (enemyAgent.remainingDistance < 2f && hasCollided)
            {
                target.gameObject.GetComponent<PlayerBehaviour>().TakeDamage(enemyDamage);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerBehaviour>().TakeDamage(enemyDamage);
            hasCollided = true;
        }
    }

    public void TakeDamage(int damage)
    {
        if (!invulnerable)
        {
            enemyHealth -= damage;

            invulnerable = true;
            ChangeColor(Color.red);

            Invoke("ResetInvulnerable", 0.5f);
        }
    }

    private void ResetInvulnerable()
    {
        invulnerable = false;
        ChangeColor(enemyColor);
    }

    private void ChangeColor(Color colorToChange)
    {
        for (int i = 0; i < enemyMaterial.Length; i++)
        {
            enemyMaterial[i].color = colorToChange;
        }
    }
}
