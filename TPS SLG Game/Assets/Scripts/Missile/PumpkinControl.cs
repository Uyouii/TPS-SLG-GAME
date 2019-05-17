using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PumpkinControl : MonoBehaviour {

    [HideInInspector]
    public int monsterID;

    private bool hasSendHitMsg;
    private NetworkHost networkHost;

    private void Awake()
    {
        hasSendHitMsg = false;
        networkHost = NetworkHost.GetInstance();
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 eularAngles = transform.rotation.eulerAngles;
        eularAngles.x += 3;
        eularAngles.y += 2;
        eularAngles.z += 1;
        transform.rotation = Quaternion.Euler(eularAngles);

		if(transform.position.y < GameSettings.ENVIRONMENT_FLOOR_HEIGHT - 2.0f)
        {
            Destroy(gameObject, 1);
        }
	}

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == GameSettings.SHOOTABLE_LAYER)
        {
            if (!hasSendHitMsg && other.gameObject.tag == "Player")
            {
                SendPumpkinHitPlayerMsg();
                hasSendHitMsg = true;
            }
        }
    }

    void SendPumpkinHitPlayerMsg()
    {
        ClientPumpkinHitMsg clienPumkinHitMsg = new ClientPumpkinHitMsg();
        clienPumkinHitMsg.monsterID = this.monsterID;
        clienPumkinHitMsg.playerID = GameSettings.playerID;

        string pumpkinHitMsg = JsonConvert.SerializeObject(clienPumkinHitMsg);

        byte[] msg = MessageHandler.SetClientMsg(
           NetworkSettings.MONSTER_ENTITY_SERVICE_ID,
           NetworkSettings.PUMPKIN_HIT_CMD,
           pumpkinHitMsg);

        StartCoroutine(networkHost.SendBytesMessage(msg));
    }
}
