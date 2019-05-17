using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerMsg
{
    public int msgType { get; set; }
}

public class ServerFeedbackMsg: ServerMsg
{
    public int code { get; set; }
}

public class ServerClientIDMsg : ServerMsg
{
    public int code { get; set; }
    public int clientID { get; set; }
}

public class ServerMonsterMsg : ServerMsg
{
    public MonsterEntity monsterEntity { get; set; }
}

public class ServerPlayerAttackMsg: ServerMsg
{
    public int playerID;
    public int monsterID;
    public int playerDamage;
    public Location shootPoint;

    public ServerPlayerAttackMsg()
    {
        shootPoint = new Location();
    }
}

public class ServerPlayerKillMsg: ServerMsg
{
    public int monsterID;
}

public class ServerPlayerDieMsg: ServerMsg
{
    public int playerID;
}

public class ServerSyncPlayerMsg: ServerMsg
{
    public int playerID;
    public int score;
    public int money;
    public int playerHealth;
    public int exp;
    public int userLevel;
    public int iceTrapLevel;
    public int needleTrapLevel;
    public int missileLevel;
}

public class ServerMissileMsg: ServerMsg
{
    public MissileEntity missileEntity { get; set; }
}

public class ServerSyncMissileMsg: ServerMsg
{
    public Location location { get; set; }
    public int entityID;
    
    public ServerSyncMissileMsg()
    {
        entityID = -1;
        location = new Location();
    }
}

public class ServerDestoryMissileMsg: ServerMsg
{
    public int entityID;
}

public class ServerMissileExplosionMsg: ServerMsg
{
    public int entityID;
}

public class ServerOtherPlayerMsg: ServerMsg
{
    public int playerID;
    public int playerHealth;
    public Location location;
    public Rotation rotation;

    public ServerOtherPlayerMsg()
    {
        location = new Location();
        rotation = new Rotation();
        playerHealth = 100;
        playerID = -1;
    }
}

public class ServerOtherPlayerShootMsg: ServerMsg{
    public int playerID;
    public Location shootPoint;

    public ServerOtherPlayerShootMsg()
    {
        shootPoint = new Location();
    }
}

public class ServerAllEntityDataMsg: ServerMsg
{
    public List<PlayerEntity> playerEntities;
    public List<MonsterEntity> monsterEntities;
    public List<MissileEntity> missileEntities;
    public List<TrapEntity> trapEntities;

    public ServerAllEntityDataMsg()
    {
        playerEntities = new List<PlayerEntity>();
        monsterEntities = new List<MonsterEntity>();
        missileEntities = new List<MissileEntity>();
        trapEntities = new List<TrapEntity>();
    }

}

public class ServerTrapMsg: ServerMsg
{
    public TrapEntity trapEntity { get; set; }
}

public class ServerElephantAttackMsg: ServerMsg
{
    public int monsterID;
    public int playerID;
}

public class ServerPlayerDisconnectMsg: ServerMsg
{
    public int playerID;
}

public class ServerNextLevelTimeMsg : ServerMsg
{
    public int nextLevelTime;
}

public class ServerHighestScoreMsg: ServerMsg
{
    public int highestScore;
}