using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Location
{
    public float x;
    public float y;
    public float z;

    public Location(float x, float y, float z)
    {
        this.x = (float)System.Math.Round(x, 3);
        this.y = (float)System.Math.Round(y, 3);
        this.z = (float)System.Math.Round(z, 3);
    }
    public Location()
    {
        this.x = 0;
        this.y = 0;
        this.z = 0;
    }


    public Location(Location loc)
    {
        x = (float)System.Math.Round(loc.x, 3);
        y = (float)System.Math.Round(loc.y, 3);
        z = (float)System.Math.Round(loc.z, 3);
    }

    public Location(Vector3 vector)
    {
        SetLocation(vector);
    }

    public void SetLocation(Vector3 vector)
    {
        x = (float)System.Math.Round(vector.x, 3);
        y = (float)System.Math.Round(vector.y, 3);
        z = (float)System.Math.Round(vector.z, 3);
    }

    public static bool IsDifference(Vector3 loc1, Vector3 loc2)
    {
        return (
            (int)(loc1.x * 10) != (int)(loc2.x * 10) ||
            (int)(loc1.y * 10) != (int)(loc2.y * 10) ||
            (int)(loc1.z * 10) != (int)(loc2.z * 10)
        );
    }
}

public class Rotation
{
    public float x;
    public float y;
    public float z;

    public Rotation(float x, float y, float z)
    {
        this.x = (float)System.Math.Round(x, 3);
        this.y = (float)System.Math.Round(y, 3);
        this.z = (float)System.Math.Round(z, 3);
    }

    public Rotation()
    {
        this.x = 0;
        this.y = 0;
        this.z = 0;
    }

    public Rotation(Vector3 angles)
    {
        x = (float)System.Math.Round(angles.x, 3);
        y = (float)System.Math.Round(angles.y, 3);
        z = (float)System.Math.Round(angles.z, 3);
    }

    public Rotation(Rotation rot)
    {
        x = (float)System.Math.Round(rot.x, 3);
        y = (float)System.Math.Round(rot.y, 3);
        z = (float)System.Math.Round(rot.z, 3);
    }

    public void SetRotation(Quaternion quaternion)
    {
        x = (float)System.Math.Round(quaternion.x, 3);
        y = (float)System.Math.Round(quaternion.y, 3);
        z = (float)System.Math.Round(quaternion.z, 3);
    }

    public void SetRotation(Vector3 angles)
    {
        x = (float)System.Math.Round(angles.x, 3);
        y = (float)System.Math.Round(angles.y, 3);
        z = (float)System.Math.Round(angles.z, 3);
    }

    public Rotation(Quaternion quaternion)
    {
        SetRotation(quaternion);
    }
}


public class Entity {

    public Location location;
    public Rotation rotation;
    public int entityID;

    public Entity()
    {
        location = new Location();
        rotation = new Rotation();
        entityID = -1;
    }

    public Entity(Location loc , Rotation rot)
    {
        location = new Location(loc);
        rotation = new Rotation(rot);
        entityID = -1;
    }

    public void SetTransFrom(Vector3 loc, Quaternion rot)
    {
        location.SetLocation(loc);
        rotation.SetRotation(rot);
    }
}

public class MissileEntity : Entity
{

    public int playerID;

    public MissileEntity(Location loc, Rotation rot) : base(loc, rot)
    {

    }

    public MissileEntity() : base()
    {

    }
}

public class MonsterEntity : Entity
{
    public int monsterHealth;
    public int monsterType;

    public MonsterEntity(Location loc, Rotation rot) : base(loc, rot)
    {

    }

    public MonsterEntity() : base()
    {

    }

}

public class PlayerEntity : Entity
{

    public int playerHealth { get; set; }
    public int score { get; set; }
    public int money { get; set; }
    public string playerName { get; set; }
    public int exp { get; set; }

    public PlayerEntity(Location loc, Rotation rot) : base(loc, rot)
    {
        playerHealth = GameSettings.PLAYER_INIT_HEALTH;
        score = 0;
        money = 0;
        exp = 0;
    }

    public PlayerEntity() : base()
    {
        playerHealth = GameSettings.PLAYER_INIT_HEALTH;
        score = 0;
        money = 0;
        exp = 0;
    }

}

public class TrapEntity : Entity
{

    public int playerID;
    public int trapType;
    public int trapLevel;

    public TrapEntity() : base()
    {

    }
}