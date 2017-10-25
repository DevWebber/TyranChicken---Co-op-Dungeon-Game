using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour {
    /// <summary>
    /// Controls the players behaviour, consisting of their health, attacking and interacting.
    /// </summary>
    /// 

    public delegate void SendHealthInfo(int health);
    public static event SendHealthInfo OnSendHealthInfo;

    [SerializeField]
    private int playerHealth;

    private bool invulnerable;

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    void Start()
    {
        SendHealthData();
	}

    public void TakeDamage(int damage)
    {
        if (!invulnerable)
        {
            playerHealth -= damage;

            invulnerable = true;
            transform.GetComponent<Material>().color = Color.red;

            Invoke("ResetInvulnerable", 1.5f);

            SendHealthData();
        }
    }

    private void ResetInvulnerable()
    {
        invulnerable = false;
        transform.GetComponent<Material>().color = Color.blue;
    }

	void Update()
    {
		
	}

    void SendHealthData()
    {
        if (OnSendHealthInfo != null)
        {
            OnSendHealthInfo(playerHealth);
        }
    }
}
