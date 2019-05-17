using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings{

    public static string username = "USERNAME";

    public const int SHOOTABLE_LAYER = 9;

    public const int PLAYER_INIT_HEALTH = 100;

    public const float MISSILE_SHOOT_INTERVAL = 3f;

    static public int clientID = -1;
    static public int playerID = -1;
    static public int playerMoney = 0;
    static public int playerExp = 0;
    static public int playerLevel = 1;
    static public int iceTrapLevel = 1;
    static public int needleTrapLevel = 1;
    static public int missileLevel = 1;
    static public int highestScore = 0;
    static public int score = 0;
    static public bool gameOver = false;

    static public Vector3 MISSILE_LOCAL_ROTATE = new Vector3(0, 180, 0);

    static public float ENVIRONMENT_FLOOR_HEIGHT = -7.67f;
    static public float TRAP_WIDTH = 2.0f;

    public const int NEEDLE_TRAP_TYPE = 0;
    public const int ICE_TRAP_TYPE = 1;

    public const float NEEDLE_TRAP_HURT_MONSTER_INTERVAL = 0.5f;

    public const int TRAP_MONEY = 30;

    public const int MISSILE_IMAGE_HEIGHT = 80;

    public const int OTHER_PLAYER_TYPE = 0;
    public const int MONSTER_TYPE = 1;
    public const int MISSILE_TYPE = 2;

    public const int ICE_TRAP_LEVEL_UP = 1;
    public const int NEEDLE_TRAP_LEVEL_UP = 2;
    public const int MISSILE_LEVEL_UP = 3;

    public const float HEALTH_INIT_WIDTH = 2.0f;
    public const float  HEALTH_INIT_HEIGHT = 0.1f;

}
