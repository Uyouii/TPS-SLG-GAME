using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class MissileControl : MonoBehaviour {

    [HideInInspector]
    public int missileID;

    [HideInInspector]
    public int playerID;

    private bool hasSendExplosionMsg;

    NetworkHost networkHost;


    void Awake () {
        networkHost = NetworkHost.GetInstance();
        hasSendExplosionMsg = false;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == GameSettings.SHOOTABLE_LAYER)
        {

            if(!hasSendExplosionMsg)
            {
                SendMissileExpolisionMsg();
                hasSendExplosionMsg = true;
            }

        }
    }

    void SendMissileExpolisionMsg()
    {
        ClientMissileHitMsg clientMissileHitMsg = new ClientMissileHitMsg();
        clientMissileHitMsg.missileID = missileID;
        clientMissileHitMsg.playerID = playerID;

        string missileHitMsg = JsonConvert.SerializeObject(clientMissileHitMsg);

        byte[] msg = MessageHandler.SetClientMsg(
           NetworkSettings.MISSILE_ENTITY_SERVICE_ID,
           NetworkSettings.MISSILE_ENTITY_HIT_CMD,
           missileHitMsg);

        StartCoroutine(networkHost.SendBytesMessage(msg));
    }

}
