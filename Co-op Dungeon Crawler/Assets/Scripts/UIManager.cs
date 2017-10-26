using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    private int player1Health;

    [SerializeField]
    private Text healthText;


    void OnEnable()
    {
        PlayerBehaviour.OnSendHealthInfo += HandleOnUpdateHealth;
    }
	void OnDisable()
    {
        PlayerBehaviour.OnSendHealthInfo -= HandleOnUpdateHealth;
    }

    void Start()
    {
        
    }

    void Update()
    {
		
	}

    private void HandleOnUpdateHealth(int health)
    {
        player1Health = health;
        healthText.text = "Health: " + player1Health;
    }
}
