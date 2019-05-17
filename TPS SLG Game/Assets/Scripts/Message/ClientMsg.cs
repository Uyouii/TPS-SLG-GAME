using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientMsg{
    
}

public class ClientPlayerRegisterMsg: ClientMsg
{
    public Location location;
    public Rotation rotation;
    public string playerName;
}

public class ClientMonsterAttackMsg: ClientMsg
{
    public int monsterID;
    public int playerID;
    public int monsterDamage;
}

public class ClientPlayerAttackMsg: ClientMsg{
    public int playerDamage;
    public int monsterID;
    public Location shootPoint;
}

public class ClientMissileShootMsg: ClientMsg
{
    public int playerID;
    public Location location;
    public Rotation rotation;
    public Location towardVector;

    public ClientMissileShootMsg()
    {
        location = new Location();
        rotation = new Rotation();
        towardVector = new Location();
    }

    public void SetTransFrom(Vector3 loc, Quaternion rot)
    {
        location.SetLocation(loc);
        rotation.SetRotation(rot);
    }
}

public class ClientMissileHitMsg: ClientMsg
{
    public int missileID;
    public int playerID;
}

public class ClientPumpkinHitMsg: ClientMsg
{
    public int monsterID;
    public int playerID;
}

public class ClientPlayerSyncMsg: ClientMsg
{
    public Location location;
    public Rotation rotation;

    public ClientPlayerSyncMsg()
    {
        location = new Location();
        rotation = new Rotation();
    }

    public void SetTransFrom(Vector3 loc, Vector3 rot)
    {
        location.SetLocation(loc);
        rotation.SetRotation(rot);
    }
}

public class ClientPutTrapMsg: ClientMsg
{
    public int trapType;
    public int playerID;
    public Location location;

    public ClientPutTrapMsg()
    {
        location = new Location();
    }
}

public class ClientTrapImpactMonsterMsg: ClientMsg
{
    public int trapID;
    public int playerID;
    public int monsterID;
    public int trapType;
}

public class ClientSkillLevelUpMsg: ClientMsg
{
    public int playerID;
    public int skillType;
}

public class ClientGameRestartMsg: ClientMsg
{
    public int clientID;
}