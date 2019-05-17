using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NetworkSettings {

    //public const string serverHostAddr = "10.240.224.54";
    public const string serverHostAddr = "127.0.0.1";
    public const int serverPort = 6666;

    // MSG ID
    public const short MSG_CS_LOGIN = 0x1001;

    public const short MSG_CS_REGISTER = 0x1002;

    public const short SERVER_MESSAGE = 0x3001;

    public const short CLIENT_MESSAGE = 0x4001;

    // msg type
    public const short SERVER_FEEDBACK = 0x3100;
    public const short SERVER_CLIENTID_DATA = 0x3200;
    public const short SERVER_CREATE_MONSTER = 0x3300;
    public const short SERVER_SYNC_MONSTER = 0x3400;
    public const short SERVER_KILL_MONSTER = 0x3500;
    public const short SERVER_SYNC_PLAYER = 0x3600;
    public const short SERVER_PLAYER_DIE = 0x3700;
    public const short SERVER_CREATE_MISSILE = 0x3800;
    public const short SERVER_SYNC_MISSILE = 0x3900;
    public const short SERVER_DESTORY_MISSILE = 0x3A00;
    public const short SERVER_MISSILE_EXPLOSION = 0x3B00;
    public const short SERVER_CREATE_OTHER_PLAYER = 0x3C00;
    public const short SERVER_ALL_ENTITIES = 0x3D00;
    public const short SERVER_SYNC_OTHER_PLAYER = 0x3E00;
    public const short SERVER_SYNC_OTHER_PLAYER_SHOOT = 0x3F00;
    public const short SERVER_CREATE_TRAP = 0x4100;
    public const short SERVER_ELEPHANT_ATTACK = 0x4200;
    public const short SERVER_PLAYER_DISCONNECT = 0x4300;
    public const short SERVER_NEXT_LEVEL_TIME = 0x4400;
    public const short SERVER_HIGHEST_SCORE = 0x4500;

    // command code
    public const short COMMAND_LOGIN_SUCCESSFUL = 0x3101;
    public const short COMMAND_REGISTER_SUCCESSFUL = 0x3102;

    public const short COMMAND_WRONG_PASSWORD = 0x3103;
    public const short COMMAND_NAME_ALREADY_EXISTS = 0x3104;
    public const short COMMAND_DATABASE_ERROR = 0x3105;
    public const short COMMAND_LOGIN_ALREADY = 0x3106;

    // server data code
    public const short COMMAND_SEND_CLIENTID = 0x3201;


    // service & command ID
    public const short LOGIN_SERVICE_ID = 0x0100;
    public const short LOGIN_SERVICE_LOGIN_CMD = 0x0101;
    public const short LOGIN_SERVICE_REGISTER_CMD = 0x0102;
    public const short GAME_RESTART_CMD = 0x0103;

    public const short PLAYER_ENTITY_SERVICE_ID = 0x0200;
    public const short PLAYER_ENTITY_REGISTER_CMD = 0x0201;
    public const short PLAYER_ENTITY_SYNC_CMD = 0x0202;
    public const short PLAYER_ENTITY_ATTACK_CMD = 0x0203;
    public const short PLAYER_SKILL_LEVEL_UP_CMD = 0x0204;

    public const short MONSTER_ENTITY_SERVICE_ID = 0x0300;
    public const short MONSTER_ENTITY_ATTACK_CMD = 0x0301;
    public const short PUMPKIN_HIT_CMD = 0x0302;

    public const short MISSILE_ENTITY_SERVICE_ID = 0x0400;
    public const short MISSILE_ENTITY_SHOOT_CMD = 0x0401;
    public const short MISSILE_ENTITY_HIT_CMD = 0x0402;

    public const short TRAP_ENTITY_SERVICE_ID = 0x0500;
    public const short TRAP_ENTITY_CREATE_CMD = 0x0501;
    public const short MONSTER_ENTER_ICE_TRAP_CMD = 0x0502;
    public const short MONSTER_OUTER_ICE_TRAP_CMD = 0x0503;
    public const short NEEDLE_TRAP_HURT_MONSTER_CMD = 0x0504;

    public const int NET_HEAD_LENGTH_SIZE = 4;

    public const float SERVER_SYNC_INTERVAL = 0.1f;


}
