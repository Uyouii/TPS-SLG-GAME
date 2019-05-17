using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapControl : MonoBehaviour {

    [HideInInspector]
    public int trapID;

    [HideInInspector]
    public int playerID;

    [HideInInspector]
    public int trapType;

    ArrayList inTrapGameObjects;
    NetworkHost networkHost;
    float timer;

    private void Awake()
    {
        inTrapGameObjects = new ArrayList();
        networkHost = NetworkHost.GetInstance();
        timer = 0f;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		if(this.trapType == GameSettings.NEEDLE_TRAP_TYPE)
        {
            timer += Time.deltaTime;

            if(timer >= GameSettings.NEEDLE_TRAP_HURT_MONSTER_INTERVAL)
            {
                timer = 0f;

                foreach (GameObject gameObject in this.inTrapGameObjects)
                {
                    if (gameObject.tag == "Monster")
                    {
                        EnemyHealth enemyHealth = gameObject.GetComponent<EnemyHealth>();
                        // send monster hurt msg
                        SendNeedleTrapHurtMonsterMsg(enemyHealth.monsterID, GameSettings.playerID);
                    }
                }
            }
            
        }
	}

    private void OnTriggerEnter(Collider other)
    {
        // if object is at shootable layer
        if (other.gameObject.layer == GameSettings.SHOOTABLE_LAYER)
        {
            // on trigger enter and exit function will trigger serval times
            if (!inTrapGameObjects.Contains(other.gameObject))
            {
                inTrapGameObjects.Add(other.gameObject);

                if (other.gameObject.tag == "Monster")
                {
                    // send monster slow msg
                    EnemyHealth enemyHealth = other.gameObject.GetComponent<EnemyHealth>();
                    if (trapType == GameSettings.ICE_TRAP_TYPE)
                    {
                        SendMonsterEnterIceTrapMsg(enemyHealth.monsterID, GameSettings.playerID);
                    }
                }
                else if (other.gameObject.tag == "Player")
                {
                    // for add trap hit self
                }
                else if (other.gameObject.tag == "OtherPlayer")
                {
                    // for add trap hit mate
                }
            } 
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == GameSettings.SHOOTABLE_LAYER)
        {
            // on trigger enter and exit function will trigger serval times
            if (inTrapGameObjects.Contains(other.gameObject))
            {
                inTrapGameObjects.Remove(other.gameObject);

                if (other.gameObject.tag == "Monster")
                {
                    EnemyHealth enemyHealth = other.gameObject.GetComponent<EnemyHealth>();
                    if (trapType == GameSettings.ICE_TRAP_TYPE)
                    {
                        SendMonsterOutIceTrapMsg(enemyHealth.monsterID, GameSettings.playerID);
                    }
                }
                else if (other.gameObject.tag == "Player")
                {
                    // for add trap hit self
                }
                else if (other.gameObject.tag == "OtherPlayer")
                {
                    // for add trap hit mate
                }
            }
        }
    }

    string GenTrapImpactMonsterJson(int monsterID, int playerID)
    {
        ClientTrapImpactMonsterMsg clientTrapImpactMonsterMsg = new ClientTrapImpactMonsterMsg
        {
            monsterID = monsterID,
            playerID = playerID,
            trapID = this.trapID,
            trapType = this.trapType
        };

        string clientTrapImpactMonsterMsgJson = JsonConvert.SerializeObject(clientTrapImpactMonsterMsg);

        return clientTrapImpactMonsterMsgJson;
    }

    void SendMonsterEnterIceTrapMsg(int monsterID, int playerID)
    {

        byte[] msg = MessageHandler.SetClientMsg(
           NetworkSettings.TRAP_ENTITY_SERVICE_ID,
           NetworkSettings.MONSTER_ENTER_ICE_TRAP_CMD,
           GenTrapImpactMonsterJson(monsterID, playerID));

        StartCoroutine(networkHost.SendBytesMessage(msg));
    }

    void SendMonsterOutIceTrapMsg(int monsterID, int playerID)
    {
        byte[] msg = MessageHandler.SetClientMsg(
           NetworkSettings.TRAP_ENTITY_SERVICE_ID,
           NetworkSettings.MONSTER_OUTER_ICE_TRAP_CMD,
           GenTrapImpactMonsterJson(monsterID, playerID));

        StartCoroutine(networkHost.SendBytesMessage(msg));
    }

    void SendNeedleTrapHurtMonsterMsg(int monsterID, int playerID)
    {
        byte[] msg = MessageHandler.SetClientMsg(
          NetworkSettings.TRAP_ENTITY_SERVICE_ID,
          NetworkSettings.NEEDLE_TRAP_HURT_MONSTER_CMD,
          GenTrapImpactMonsterJson(monsterID, playerID));

        StartCoroutine(networkHost.SendBytesMessage(msg));
    }

}
