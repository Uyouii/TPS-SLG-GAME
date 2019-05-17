using UnityEngine;
using System.Collections;
using Newtonsoft.Json;

public class EnemyAttack : MonoBehaviour
{
    public float timeBetweenAttacks = 0.5f;
    public int attackDamage = 10;


    private NetworkHost networkHost;
    Animator anim;
    ArrayList players;
    EnemyHealth enemyHealth;
    bool playerInRange;
    float timer;


    void Awake ()
    {
        players = new ArrayList();
        enemyHealth = GetComponent<EnemyHealth>();
        anim = GetComponent <Animator> ();
        networkHost = NetworkHost.GetInstance();
    }


    void OnTriggerEnter (Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            players.Add(other.gameObject);
        }
    }


    void OnTriggerExit (Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            if(players.Contains(other.gameObject))
            {
                players.Remove(other.gameObject);
            }
        }      
    }


    void Update ()
    {
        timer += Time.deltaTime;

        if(timer >= timeBetweenAttacks && players.Count > 0  && !enemyHealth.isDead)
        {
            Attack ();
        }

        //if(playerHealth.currentHealth <= 0)
        //{
        //    anim.SetTrigger ("PlayerDead");
        //}
    }


    void Attack ()
    {
        timer = 0f;
        foreach( GameObject player in players)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth.currentHealth > 0)
            {
                // send enemy attack msg
                SendMonsterAttackMsg(enemyHealth.monsterID, GameSettings.playerID, attackDamage);
            }
        }

    }
    void SendMonsterAttackMsg(int monsterID, int playerID, int monsterDamage)
    {
        ClientMonsterAttackMsg clientMonsterAttackMsg = new ClientMonsterAttackMsg();
        clientMonsterAttackMsg.monsterID = monsterID;
        clientMonsterAttackMsg.playerID = playerID;
        clientMonsterAttackMsg.monsterDamage = monsterDamage;

        string monsterAttackJson = JsonConvert.SerializeObject(clientMonsterAttackMsg);

        byte[] msg = MessageHandler.SetClientMsg(
           NetworkSettings.MONSTER_ENTITY_SERVICE_ID,
           NetworkSettings.MONSTER_ENTITY_ATTACK_CMD,
           monsterAttackJson);

        StartCoroutine(networkHost.SendBytesMessage(msg));
    }

}
