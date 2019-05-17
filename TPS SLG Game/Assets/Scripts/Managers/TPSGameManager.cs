using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TPSGameManager : MonoBehaviour {

    public GameObject player;   //to get player transfrom

    // instance to generate
    public GameObject Zombunny;
    public GameObject Zombear;
    public GameObject Hellephant;
    public GameObject Missile;
    public GameObject ExplosionAir;
    public GameObject OtherPlayer;
    public GameObject NeedleTrap;
    public GameObject IceTrap;
    public GameObject Pumpkin;

    private NetworkHost networkHost;
    private bool playerEneityRegistered;

    private float timer = 0f;

    // strage the gameobjects which need sync by server
    private Dictionary<int, Dictionary<int, GameObject>> serverGameObjects;
    // storage the server entities' movement, change their position each update
    private Dictionary<int, Dictionary<int, Vector3>> serverGameObjectsMovement;


    // Use this for initialization
    void Awake()
    {
        networkHost = NetworkHost.GetInstance();
        playerEneityRegistered = false;

        serverGameObjects = new Dictionary<int, Dictionary<int, GameObject>>
        {
            { GameSettings.OTHER_PLAYER_TYPE, new Dictionary<int, GameObject>()},
            { GameSettings.MONSTER_TYPE, new Dictionary<int, GameObject>()},
            { GameSettings.MISSILE_TYPE, new Dictionary<int, GameObject>()},
        };

        serverGameObjectsMovement = new Dictionary<int, Dictionary<int, Vector3>>
        {
            { GameSettings.OTHER_PLAYER_TYPE, new Dictionary<int, Vector3>()},
            { GameSettings.MONSTER_TYPE, new Dictionary<int, Vector3>()},
            { GameSettings.MISSILE_TYPE, new Dictionary<int, Vector3>()},
        };

    }

	// Update is called once per frame
	void Update () {
        // fisrt register a player entity at server
        if(!playerEneityRegistered && networkHost.connected)
        {
            SendPlayerEntityRegisterMsg();
            playerEneityRegistered = true;
        }
        
        // sync the player location to server
        if (networkHost.connected && timer >= NetworkSettings.SERVER_SYNC_INTERVAL)
        {
            if(!GameSettings.gameOver)
            {
                SendPlayerEntitySyncMsg();
            }
            timer -= NetworkSettings.SERVER_SYNC_INTERVAL;
        }
        else
        {
            timer += Time.deltaTime;
        }

        // receive data to message queue
        networkHost.ReceiveData();
        while (networkHost.receiveMessages.Count > 0)
        {
            ServerMsg serverMessage = networkHost.receiveMessages.Dequeue();
            HandleServerMsg(serverMessage);
        }

        // change gameobject's position
        ChangeServerGameObjectPosotion(Time.deltaTime);

        if(GameSettings.gameOver &&  PlayerInput.GetR())
        {
            SendRestartMsg();
            InitPlayerData();
            SceneManager.LoadScene("Login Scene");
        }
    }

    void SendRestartMsg()
    {
        ClientGameRestartMsg gameRestaratMsg = new ClientGameRestartMsg
        {
            clientID = GameSettings.clientID
        };

        string gameRestaratMsgJson = JsonConvert.SerializeObject(gameRestaratMsg);

        byte[] msg = MessageHandler.SetClientMsg(
            NetworkSettings.LOGIN_SERVICE_ID,
            NetworkSettings.GAME_RESTART_CMD,
            gameRestaratMsgJson);

        StartCoroutine(networkHost.SendBytesMessage(msg));
    }

    void InitPlayerData()
    {
        GameSettings.clientID = -1;
        GameSettings.playerID = -1;
        GameSettings.playerMoney = 0;
        GameSettings.playerExp = 0;
        GameSettings.playerLevel = 1;
        GameSettings.iceTrapLevel = 1;
        GameSettings.needleTrapLevel = 1;
        GameSettings.missileLevel = 1;
        GameSettings.highestScore = 0;
        GameSettings.score = 0;
        GameSettings.gameOver = false;
    }

    // user movement get from server to change each entity's position
    void ChangeServerGameObjectPosotion(float delaTime)
    {
        float factor = delaTime / NetworkSettings.SERVER_SYNC_INTERVAL;
        foreach(var gameObjectTypeDict in serverGameObjects)
        {
            int type = gameObjectTypeDict.Key;
            foreach (var gameObjectDict in gameObjectTypeDict.Value)
            {
                int entityID = gameObjectDict.Key;
                GameObject gameObject = gameObjectDict.Value;
                //Debug.Log("type " + type + " entity ID " + entityID);
                if(serverGameObjectsMovement[type].ContainsKey(entityID))
                    gameObject.transform.position += factor * serverGameObjectsMovement[type][entityID];
            }
        } 
    }

    void AddServerGameObjects(int type, int entityID, GameObject gameObject)
    {
        if(!serverGameObjectsMovement[type].ContainsKey(entityID))
            serverGameObjects[type].Add(entityID, gameObject);
    }

    void UpdateServerGameObjectsMovement(int type, int entityID, Vector3 movement)
    {
        if (serverGameObjectsMovement[type].ContainsKey(entityID))
            serverGameObjectsMovement[type][entityID] = movement;
        else
            serverGameObjectsMovement[type].Add(entityID, movement);
    }

    void DeleteServerGameobjects(int type, int entityID)
    {
        serverGameObjects[type].Remove(entityID);
        serverGameObjectsMovement[type].Remove(entityID);
    }

    void HandleServerMsg(ServerMsg serverMsg)
    {
        switch (serverMsg.msgType)
        {
            case NetworkSettings.SERVER_CLIENTID_DATA:
                Debug.Log("client id: " + Convert.ToString(((ServerClientIDMsg)serverMsg).clientID));
                GameSettings.clientID = ((ServerClientIDMsg)serverMsg).clientID;
                break;
            case NetworkSettings.SERVER_CREATE_MONSTER:
                Debug.Log("Create Monster of id " + ((ServerMonsterMsg)serverMsg).monsterEntity.entityID);
                CreateMonster(((ServerMonsterMsg)serverMsg).monsterEntity);
                break;
            case NetworkSettings.SERVER_SYNC_MONSTER:
                //Debug.Log("Sync the monste location of id " + ((ServerMonsterMsg)serverMsg).monsterEntity.entityID);
                SyncMonster(((ServerMonsterMsg)serverMsg).monsterEntity);
                break;
            case NetworkSettings.SERVER_KILL_MONSTER:
                int monsterID = ((ServerPlayerKillMsg)serverMsg).monsterID;
                Debug.Log("monser is killed " + monsterID);
                KillMonster(monsterID);
                break;
            case NetworkSettings.SERVER_SYNC_PLAYER:
                SyncPlayer((ServerSyncPlayerMsg)serverMsg);
                break;
            case NetworkSettings.SERVER_PLAYER_DIE:
                Debug.Log("Player Die");
                PlayerDie((ServerPlayerDieMsg)serverMsg);
                break;
            case NetworkSettings.SERVER_CREATE_MISSILE:
                Debug.Log("Create Missile of id " + ((ServerMissileMsg)serverMsg).missileEntity.entityID);
                CreateMissile(((ServerMissileMsg)serverMsg).missileEntity);
                break;
            case NetworkSettings.SERVER_SYNC_MISSILE:
                SyncMissile((ServerSyncMissileMsg)serverMsg);
                break;
            case NetworkSettings.SERVER_DESTORY_MISSILE:
                DestoryMissile((ServerDestoryMissileMsg)serverMsg);
                break;
            case NetworkSettings.SERVER_MISSILE_EXPLOSION:
                MissileExplosion((ServerMissileExplosionMsg)serverMsg);
                break;
            case NetworkSettings.SERVER_CREATE_OTHER_PLAYER:
                CreateOtherPlayer((ServerOtherPlayerMsg)serverMsg);
                break;
            case NetworkSettings.SERVER_ALL_ENTITIES:
                HandleAllEntityMsg((ServerAllEntityDataMsg)serverMsg);
                break;
            case NetworkSettings.SERVER_SYNC_OTHER_PLAYER:
                SyncOtherPlayer((ServerOtherPlayerMsg)serverMsg);
                break;
            case NetworkSettings.SERVER_SYNC_OTHER_PLAYER_SHOOT:
                SyncOtherPlayerShoot((ServerOtherPlayerShootMsg)serverMsg);
                break;
            case NetworkSettings.SERVER_CREATE_TRAP:
                CreateTrap(((ServerTrapMsg)serverMsg).trapEntity);
                break;
            case NetworkSettings.SERVER_ELEPHANT_ATTACK:
                ElephantAttack((ServerElephantAttackMsg)serverMsg);
                break;
            case NetworkSettings.SERVER_PLAYER_DISCONNECT:
                PlayerDisconnect((ServerPlayerDisconnectMsg)serverMsg);
                break;
            case NetworkSettings.SERVER_NEXT_LEVEL_TIME:
                SetNextLevelTime((ServerNextLevelTimeMsg)serverMsg);
                break;
            case NetworkSettings.SERVER_HIGHEST_SCORE:
                SetHighestScore((ServerHighestScoreMsg)serverMsg);
                break;
        }
    }

    void SetHighestScore(ServerHighestScoreMsg serverHighestScoreMsg)
    {
        Debug.Log("Set Score: " + serverHighestScoreMsg.highestScore);
        GameSettings.highestScore = serverHighestScoreMsg.highestScore;
    }

    void SetNextLevelTime(ServerNextLevelTimeMsg serverNextLevelTimeMsg )
    {
        NextLevelTimeControl.nextLevelTime = serverNextLevelTimeMsg.nextLevelTime;
    }

    void PlayerDisconnect(ServerPlayerDisconnectMsg playerDisconnectMsg)
    {
        int playerID = playerDisconnectMsg.playerID;
        GameObject otherPlayer = FindOtherPlayerWithID(playerID);

        DeleteServerGameobjects(GameSettings.OTHER_PLAYER_TYPE, playerID);

        Destroy(otherPlayer);
    }

    void ElephantAttack(ServerElephantAttackMsg elephantAttackMsg)
    {
        int monsterID = elephantAttackMsg.monsterID;
        int playerID = elephantAttackMsg.playerID;
        GameObject monster = FindMonsterWithID(monsterID);
        Vector3 endPoint = new Vector3();
        if(playerID == GameSettings.playerID)
        {
            endPoint = this.player.transform.position;
        }
        else
        {
            endPoint = FindOtherPlayerWithID(playerID).transform.position;
        }
        if(monster != null)
        {
            ThrowPumpkin(monster.transform.position, endPoint, monsterID);

            Debug.Log("Hellephant Attack");
        }
    }

    void ThrowPumpkin(Vector3 beginPoint, Vector3 endPoint, int monsterID)
    {
        beginPoint.y += 3f;
        Vector3 throwVector = endPoint - beginPoint;
        throwVector.y = (float)(Math.Sqrt(Math.Pow(throwVector.x, 2) + Math.Pow(throwVector.z, 2)) * 0.2f);

        GameObject pumpkin = Instantiate(Pumpkin, beginPoint, Quaternion.Euler(-90, 0, 0));

        pumpkin.GetComponent<PumpkinControl>().monsterID = monsterID;

        pumpkin.GetComponent<Rigidbody>().AddForce(throwVector, ForceMode.VelocityChange);
    }

    void CreateTrap(TrapEntity trapEntity)
    {
        Vector3 trapPosotion = new Vector3
        {
            x = trapEntity.location.x,
            y = trapEntity.location.y,
            z = trapEntity.location.z
        };

        GameObject newTrap = null;

        if(trapEntity.trapType == GameSettings.ICE_TRAP_TYPE)
        {
            newTrap = Instantiate(IceTrap, trapPosotion, new Quaternion(0, 0, 0, 1f));
        }
        else if(trapEntity.trapType == GameSettings.NEEDLE_TRAP_TYPE)
        {
            newTrap = Instantiate(NeedleTrap, trapPosotion, new Quaternion(0, 0, 0, 1f));
        }

        TrapControl trapControl = newTrap.GetComponent<TrapControl>();

        trapControl.trapID = trapEntity.entityID;
        trapControl.trapType = trapEntity.trapType;
        trapControl.playerID = trapEntity.playerID;

        // if trap is not put by local player, then close the trigger
        if (trapEntity.playerID != GameSettings.playerID)
        {
            BoxCollider boxCollider = newTrap.GetComponent<BoxCollider>();
            boxCollider.enabled = false;
        }

        // add collider to the trap from judge collider
        BoxCollider newCollider = newTrap.AddComponent<BoxCollider>();
        newCollider.size = new Vector3(0.8f, 0.05f, 0.8f);
        newCollider.center = new Vector3(0, 0, 0);
    }

    void SyncOtherPlayerShoot(ServerOtherPlayerShootMsg otherPlayerShootMsg)
    {
        int entityID = otherPlayerShootMsg.playerID;

        Vector3 shootPoint = new Vector3
        {
            x = otherPlayerShootMsg.shootPoint.x,
            y = otherPlayerShootMsg.shootPoint.y,
            z = otherPlayerShootMsg.shootPoint.z
        };

        GameObject targetPlayer = FindOtherPlayerWithID(entityID);
        if (targetPlayer != null)
        {
            OtherPlayerControl otherPlayerControl = targetPlayer.GetComponent<OtherPlayerControl>();

            otherPlayerControl.Shoot(shootPoint);
        }
        
    }
    
    GameObject FindOtherPlayerWithID(int entityID)
    {
        if (serverGameObjects[GameSettings.OTHER_PLAYER_TYPE].ContainsKey(entityID))
            return serverGameObjects[GameSettings.OTHER_PLAYER_TYPE][entityID];

        GameObject[] otherPlayers = GameObject.FindGameObjectsWithTag("OtherPlayer");
        for (int i = 0; i < otherPlayers.Length; i++)
        {
            OtherPlayerControl otherPlayerControl = otherPlayers[i].GetComponent<OtherPlayerControl>();
            if (otherPlayerControl != null && otherPlayerControl.otherPlayerID == entityID)
            {
                return otherPlayers[i];
            }
        }
        return null;
    }

    void SyncOtherPlayer(ServerOtherPlayerMsg serverSyncOtherPlayerMsg)
    {
        int entityID = serverSyncOtherPlayerMsg.playerID;
        int playerHealth = serverSyncOtherPlayerMsg.playerHealth;

        Vector3 otherPlayerPosition = new Vector3
        {
            x = serverSyncOtherPlayerMsg.location.x,
            y = serverSyncOtherPlayerMsg.location.y,
            z = serverSyncOtherPlayerMsg.location.z
        };

        Quaternion otherPlayerRotation = Quaternion.Euler(
            serverSyncOtherPlayerMsg.rotation.x,
            serverSyncOtherPlayerMsg.rotation.y,
            serverSyncOtherPlayerMsg.rotation.z
        );

        GameObject targetPlayer = FindOtherPlayerWithID(entityID);
        if (targetPlayer != null)
        {
            OtherPlayerControl otherPlayerControl = targetPlayer.GetComponent<OtherPlayerControl>();

            otherPlayerControl.SetWalking(Location.IsDifference(targetPlayer.transform.position, otherPlayerPosition));

            //targetPlayer.transform.position = otherPlayerPosition;
            targetPlayer.transform.rotation = otherPlayerRotation;

            UpdateServerGameObjectsMovement(
                GameSettings.OTHER_PLAYER_TYPE,
                entityID,
                otherPlayerPosition - targetPlayer.transform.position
            );

            if (playerHealth < otherPlayerControl.otherPlayerHealth)
            {
                otherPlayerControl.otherPlayerHurtAduio.Play();
                otherPlayerControl.otherPlayerHealth = playerHealth;
            }
        }
    }
    // create trap and missile
    void HandleAllEntityMsg(ServerAllEntityDataMsg serverAllEntityDataMsg)
    {
        CreateOtherPlayersByList(serverAllEntityDataMsg.playerEntities);
        CreateMonstersByList(serverAllEntityDataMsg.monsterEntities);
        CreateTrapByList(serverAllEntityDataMsg.trapEntities);
        CreateMissileByList(serverAllEntityDataMsg.missileEntities);
    }

    void CreateMissileByList(List<MissileEntity> missileEntities)
    {
        foreach(MissileEntity missileEntity in missileEntities)
        {
            CreateMissile(missileEntity);
        }
    }

    void CreateTrapByList(List<TrapEntity> trapEntities)
    {
        foreach(TrapEntity trapEntity in trapEntities)
        {
            CreateTrap(trapEntity);
        }
    }

    void CreateMonstersByList(List<MonsterEntity> monsterEntities)
    {
        foreach(MonsterEntity monster in monsterEntities)
        {
            CreateMonster(monster);
        }
    }

    void CreateOtherPlayersByList(List<PlayerEntity> otherPlayerEntities)
    {
        foreach(PlayerEntity otherPlayerEntity in otherPlayerEntities)
        {
            int playerID = otherPlayerEntity.entityID;
            int playerHealth = otherPlayerEntity.playerHealth;

            Vector3 otherPlayerPosition = new Vector3
            {
                x = otherPlayerEntity.location.x,
                y = otherPlayerEntity.location.y,
                z = otherPlayerEntity.location.z
            };

            Quaternion otherPlayerRotation = Quaternion.Euler(
                otherPlayerEntity.rotation.x,
                otherPlayerEntity.rotation.y,
                otherPlayerEntity.rotation.z
            );


            GameObject newOtherPlayer = null;

            newOtherPlayer = Instantiate(OtherPlayer, otherPlayerPosition, otherPlayerRotation);

            OtherPlayerControl otherPlayerControl = newOtherPlayer.GetComponent<OtherPlayerControl>();
            otherPlayerControl.otherPlayerID = playerID;
            otherPlayerControl.otherPlayerHealth = playerHealth;

            AddServerGameObjects(GameSettings.OTHER_PLAYER_TYPE, playerID, newOtherPlayer);
        }
    }


    void CreateOtherPlayer(ServerOtherPlayerMsg serverOtherPlayerCreateMsg)
    {
        int playerID = serverOtherPlayerCreateMsg.playerID;
        int playerHealth = serverOtherPlayerCreateMsg.playerHealth;

        Vector3 otherPlayerPosition = new Vector3
        {
            x = serverOtherPlayerCreateMsg.location.x,
            y = serverOtherPlayerCreateMsg.location.y,
            z = serverOtherPlayerCreateMsg.location.z
        };

        Quaternion otherPlayerRotation = Quaternion.Euler(
            serverOtherPlayerCreateMsg.rotation.x,
            serverOtherPlayerCreateMsg.rotation.y,
            serverOtherPlayerCreateMsg.rotation.z
        );


        GameObject newOtherPlayer = null;

        newOtherPlayer = Instantiate(OtherPlayer, otherPlayerPosition, otherPlayerRotation);

        OtherPlayerControl otherPlayerControl = newOtherPlayer.GetComponent<OtherPlayerControl>();
        otherPlayerControl.otherPlayerID = playerID;
        otherPlayerControl.otherPlayerHealth = playerHealth;

        AddServerGameObjects(GameSettings.OTHER_PLAYER_TYPE, playerID, newOtherPlayer);
    }

    GameObject FindMissileWithID(int entityID)
    {
        if (serverGameObjects[GameSettings.MISSILE_TYPE].ContainsKey(entityID))
            return serverGameObjects[GameSettings.MISSILE_TYPE][entityID];

        GameObject[] missiles = GameObject.FindGameObjectsWithTag("Missile");
        for (int i = 0; i < missiles.Length; i++)
        {
            MissileControl missileControl = missiles[i].GetComponent<MissileControl>();
            if (missileControl != null && missileControl.missileID == entityID)
            {
                return missiles[i];
            }
        }
        return null;
    }

    void MissileExplosion(ServerMissileExplosionMsg serverMissileExplosionMsg)
    {
        int entityID = serverMissileExplosionMsg.entityID;

        GameObject missile = FindMissileWithID(entityID);

        DeleteServerGameobjects(GameSettings.MISSILE_TYPE, entityID);

        if (missile != null)
        {
            MissileControl missileControl = missile.GetComponent<MissileControl>();
            if (missileControl != null && missileControl.missileID == entityID)
            {
                Instantiate(ExplosionAir, missile.transform.position, missile.transform.rotation);
                Destroy(missile);
            }
        }

    }

    void DestoryMissile(ServerDestoryMissileMsg serverDestoryMissileMsg)
    {
        int entityID = serverDestoryMissileMsg.entityID;

        GameObject missile = FindMissileWithID(entityID);

        DeleteServerGameobjects(GameSettings.MISSILE_TYPE, entityID);

        if (missile != null)
        {
            MissileControl missileControl = missile.GetComponent<MissileControl>();
            if (missileControl != null && missileControl.missileID == entityID)
            {
                Destroy(missile);
                return;
            }
        }
    }

    void SyncMissile(ServerSyncMissileMsg serverSyncMissileMsg)
    {
        int entityID = serverSyncMissileMsg.entityID;

        Vector3 missilePosition = new Vector3
        {
            x = serverSyncMissileMsg.location.x,
            y = serverSyncMissileMsg.location.y,
            z = serverSyncMissileMsg.location.z
        };

        GameObject missile = FindMissileWithID(entityID);

        if (missile != null)
        {
            MissileControl missileControl = missile.GetComponent<MissileControl>();
            if (missileControl != null)
            {
                UpdateServerGameObjectsMovement(
                    GameSettings.MISSILE_TYPE, 
                    entityID, 
                    missilePosition - missile.transform.position
                );
            }
        }
    }


    void CreateMissile(MissileEntity missileEntity)
    {

        Vector3 missilePosition = new Vector3
        {
            x = missileEntity.location.x,
            y = missileEntity.location.y,
            z = missileEntity.location.z
        };

        Quaternion missileRotation = Quaternion.Euler(
            missileEntity.rotation.x,
            missileEntity.rotation.y,
            missileEntity.rotation.z
        );

        int eneityID = missileEntity.entityID;

        GameObject newMissile = null;

        if (missileEntity.playerID == GameSettings.playerID)
        {


            newMissile = Instantiate(Missile, missilePosition, missileRotation);
            newMissile.transform.Rotate(GameSettings.MISSILE_LOCAL_ROTATE);

            MissileControl missileControl = newMissile.GetComponent<MissileControl>();
            missileControl.missileID = missileEntity.entityID;
            missileControl.playerID = missileEntity.playerID;
        }
        // is missile is from other clients, then disable the trigger
        else
        {

            newMissile = Instantiate(Missile, missilePosition, missileRotation);

            CapsuleCollider capsuleCollider = newMissile.GetComponent<CapsuleCollider>();
            capsuleCollider.enabled = false;

            newMissile.transform.Rotate(GameSettings.MISSILE_LOCAL_ROTATE);

            MissileControl missileControl = newMissile.GetComponent<MissileControl>();
            missileControl.missileID = missileEntity.entityID;
            missileControl.playerID = missileEntity.playerID;
        }

        AddServerGameObjects(GameSettings.MISSILE_TYPE, eneityID, newMissile);
    }

    void PlayerDie(ServerPlayerDieMsg serverPlayerDieMsg)
    {
        int playerID = serverPlayerDieMsg.playerID;
        if (playerID == GameSettings.playerID)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            playerHealth.currentHealth = 0;
            playerHealth.Death();
            GameSettings.gameOver = true;
        }
        // other player die
        else
        {
            GameObject otherDiePlayer = FindOtherPlayerWithID(playerID);
            if (otherDiePlayer != null) {
                OtherPlayerControl otherPlayerControl = otherDiePlayer.GetComponent<OtherPlayerControl>();
                otherPlayerControl.otherPlayerHealth = 0;
                otherPlayerControl.Die();
            }

            DeleteServerGameobjects(GameSettings.OTHER_PLAYER_TYPE, playerID);
        }
    }

    void SyncPlayer(ServerSyncPlayerMsg serverSyncPlayerMsg)
    {
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        GameSettings.playerID = serverSyncPlayerMsg.playerID;
        GameSettings.playerMoney = serverSyncPlayerMsg.money;
        GameSettings.playerExp = serverSyncPlayerMsg.exp;
        GameSettings.playerLevel = serverSyncPlayerMsg.userLevel;
        GameSettings.iceTrapLevel = serverSyncPlayerMsg.iceTrapLevel;
        GameSettings.needleTrapLevel = serverSyncPlayerMsg.needleTrapLevel;
        GameSettings.missileLevel = serverSyncPlayerMsg.missileLevel;
        GameSettings.score = serverSyncPlayerMsg.score;

        ScoreManager.score = serverSyncPlayerMsg.score;
        ScoreManager.money = serverSyncPlayerMsg.money;

        int healthNum = serverSyncPlayerMsg.playerHealth;
        if(healthNum < playerHealth.currentHealth)
        {
            playerHealth.SetHealth(healthNum);
        }

    }

    GameObject FindMonsterWithID(int entityID)
    {
        if (serverGameObjects[GameSettings.MONSTER_TYPE].ContainsKey(entityID))
            return serverGameObjects[GameSettings.MONSTER_TYPE][entityID];

        GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");
        for (int i = 0; i < monsters.Length; i++)
        {
            EnemyHealth enemyHealth = monsters[i].GetComponent<EnemyHealth>();
            if (enemyHealth != null && enemyHealth.monsterID == entityID)
            {
                return monsters[i];
            }
        }
        return null;
    }

    void KillMonster(int monsterID)
    {
        GameObject monster = FindMonsterWithID(monsterID);

        DeleteServerGameobjects(GameSettings.MONSTER_TYPE, monsterID);

        if (monster != null)
        {
            EnemyHealth enemyHealth = monster.GetComponent<EnemyHealth>();
            enemyHealth.currentHealth = 0;
            enemyHealth.SetHealthImage();
            enemyHealth.isDead = true;
            enemyHealth.Death();
        }
    }

    void SyncMonster(MonsterEntity monsterEntity)
    {
        int entityID = monsterEntity.entityID;
        int monsterHealth = monsterEntity.monsterHealth;

        Vector3 monsterPosition = new Vector3
        {
            x = monsterEntity.location.x,
            y = monsterEntity.location.y,
            z = monsterEntity.location.z
        };

        Quaternion monsterRotation = Quaternion.Euler(
            monsterEntity.rotation.x,
            monsterEntity.rotation.y,
            monsterEntity.rotation.z
        );

        GameObject monster = FindMonsterWithID(entityID);
        if (monster != null)
        {
            EnemyHealth enemyHealth = monster.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                monsterPosition.y = monster.transform.position.y;
                //monster.transform.position = monsterPosition;
                monster.transform.rotation = monsterRotation;

                Vector3 movement = monsterPosition - monster.transform.position;
                UpdateServerGameObjectsMovement(GameSettings.MONSTER_TYPE, entityID, movement);

                if (monsterHealth < enemyHealth.currentHealth)
                {
                    enemyHealth.enemyAudio.Play();
                    enemyHealth.currentHealth = monsterHealth;
                    enemyHealth.SetHealthImage();
                    if(enemyHealth.currentHealth <= 0)
                    {
                        DeleteServerGameobjects(GameSettings.MONSTER_TYPE, entityID);
                    }
                }
            }
        }
    }

    void CreateMonster(MonsterEntity monsterEntity)
    {
        int monsterStyle = monsterEntity.monsterType;
        Vector3 monsterPosition = new Vector3
        {
            x = monsterEntity.location.x,
            y = monsterEntity.location.y,
            z = monsterEntity.location.z
        };

        Quaternion monsterRotation = Quaternion.Euler(
            monsterEntity.rotation.x,
            monsterEntity.rotation.y,
            monsterEntity.rotation.z
        );

        int eneityID = monsterEntity.entityID;

        GameObject newEnemy = null;

        if (monsterStyle == 0)
            newEnemy = Instantiate(Zombunny, monsterPosition, monsterRotation);
        else if (monsterStyle == 1)
            newEnemy = Instantiate(Zombear, monsterPosition, monsterRotation);
        else if (monsterStyle == 2)
            newEnemy = Instantiate(Hellephant, monsterPosition, monsterRotation);

        EnemyHealth enemyHealth = newEnemy.GetComponent<EnemyHealth>();
        enemyHealth.monsterID = eneityID;
        enemyHealth.monsterStyle = monsterStyle;
        enemyHealth.currentHealth = monsterEntity.monsterHealth;

        AddServerGameObjects(GameSettings.MONSTER_TYPE, eneityID, newEnemy);
    }

    // register a player entity in server
    void SendPlayerEntityRegisterMsg()
    {
        ClientPlayerRegisterMsg playerRegisterMsg = new ClientPlayerRegisterMsg
        {
            location = new Location(player.transform.position),
            rotation = new Rotation(player.transform.rotation.eulerAngles),
            playerName = GameSettings.username
        };

        string playerRegisterMsgJson = JsonConvert.SerializeObject(playerRegisterMsg);

        byte[] msg = MessageHandler.SetClientMsg(
            NetworkSettings.PLAYER_ENTITY_SERVICE_ID,
            NetworkSettings.PLAYER_ENTITY_REGISTER_CMD,
            playerRegisterMsgJson);

        StartCoroutine(networkHost.SendBytesMessage(msg));
    }

    // sync player location
    void SendPlayerEntitySyncMsg()
    {

        ClientPlayerSyncMsg clientPlayerSyncMsg = new ClientPlayerSyncMsg();
        clientPlayerSyncMsg.SetTransFrom(player.transform.position, player.transform.rotation.eulerAngles);

        string clientPlayerSyncMsgJson = JsonConvert.SerializeObject(clientPlayerSyncMsg);

        byte[] msg = MessageHandler.SetClientMsg(
            NetworkSettings.PLAYER_ENTITY_SERVICE_ID,
            NetworkSettings.PLAYER_ENTITY_SYNC_CMD,
            clientPlayerSyncMsgJson);

        StartCoroutine(networkHost.SendBytesMessage(msg));

    }
}
