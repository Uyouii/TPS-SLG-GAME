using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;
using Newtonsoft.Json;

public class MessageHandler{

    public static byte[] SetLoginMsg(string name, string password)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        // add service ID and command ID
        writer.Write(NetworkSettings.LOGIN_SERVICE_ID);
        writer.Write(NetworkSettings.LOGIN_SERVICE_LOGIN_CMD);

        // add login msg
        writer.Write(NetworkSettings.MSG_CS_LOGIN);
        writer.Write(name.Length);
        writer.Write(System.Text.Encoding.Default.GetBytes(name));
        writer.Write(password.Length);
        writer.Write(System.Text.Encoding.Default.GetBytes(password));

        // add header
        return AddHeaderToMsg(stream.ToArray());
    }

    public static byte[] SetRegisterMsg(string name, string password)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        // add service ID and command ID
        writer.Write(NetworkSettings.LOGIN_SERVICE_ID);
        writer.Write(NetworkSettings.LOGIN_SERVICE_REGISTER_CMD);

        // add register msg
        writer.Write(NetworkSettings.MSG_CS_REGISTER);
        writer.Write(name.Length);
        writer.Write(System.Text.Encoding.Default.GetBytes(name));
        writer.Write(password.Length);
        writer.Write(System.Text.Encoding.Default.GetBytes(password));

        // add header
        return AddHeaderToMsg(stream.ToArray());
    }

    public static byte[] AddHeaderToMsg(byte[] data)
    {
        int len = data.Length + NetworkSettings.NET_HEAD_LENGTH_SIZE;
        byte[] lenByte = System.BitConverter.GetBytes(len);
        byte[] sendData = new byte[data.Length + lenByte.Length];

        lenByte.CopyTo(sendData, 0);
        data.CopyTo(sendData, lenByte.Length);

        //Debug.Log(sendData.Length);

        return sendData;
    }

    public static byte[] SetClientMsg(short sid, short cid, string msgData)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        // add service ID and command ID
        writer.Write(sid);
        writer.Write(cid);

        
        writer.Write(NetworkSettings.CLIENT_MESSAGE);

        if(msgData != null && msgData.Length > 0)
        {
            writer.Write(msgData.Length);
            writer.Write(System.Text.Encoding.Default.GetBytes(msgData));
        }

        // add header
        return AddHeaderToMsg(stream.ToArray());
    }



    static public ServerMsg ParseFrom(byte[] buffer, int begin, int length)
    {
        if (begin < 0 || length <= 0 || begin + length > buffer.Length)
            return null;

        short msgId = System.BitConverter.ToInt16(buffer, begin);
        begin += sizeof(short);
        short msgType = System.BitConverter.ToInt16(buffer, begin);
        begin += sizeof(short);

        int dataLength = System.BitConverter.ToInt32(buffer, begin);
        begin += sizeof(int);

        string dataStr = System.Text.Encoding.Default.GetString(buffer, begin, dataLength);

        ServerMsg serverMsg = null;
        //Debug.Log("receive server msg type: " + Convert.ToString(msgType, 16));
        //Debug.Log(dataStr);
        switch (msgType)
        {
            case NetworkSettings.SERVER_FEEDBACK:
                serverMsg = JsonConvert.DeserializeObject<ServerFeedbackMsg>(dataStr);
                break;
            case NetworkSettings.SERVER_CLIENTID_DATA:
                serverMsg = JsonConvert.DeserializeObject<ServerClientIDMsg>(dataStr);
                break;
            case NetworkSettings.SERVER_CREATE_MONSTER:
            case NetworkSettings.SERVER_SYNC_MONSTER:
                serverMsg = JsonConvert.DeserializeObject<ServerMonsterMsg>(dataStr);
                break;
            case NetworkSettings.SERVER_KILL_MONSTER:
                serverMsg = JsonConvert.DeserializeObject<ServerPlayerKillMsg>(dataStr);
                break;
            case NetworkSettings.SERVER_SYNC_PLAYER:
                serverMsg = JsonConvert.DeserializeObject<ServerSyncPlayerMsg>(dataStr);
                break;
            case NetworkSettings.SERVER_PLAYER_DIE:
                serverMsg = JsonConvert.DeserializeObject<ServerPlayerDieMsg>(dataStr);
                break;
            case NetworkSettings.SERVER_CREATE_MISSILE:
                serverMsg = JsonConvert.DeserializeObject<ServerMissileMsg>(dataStr);
                break;
            case NetworkSettings.SERVER_SYNC_MISSILE:
                serverMsg = JsonConvert.DeserializeObject<ServerSyncMissileMsg>(dataStr);
                break;
            case NetworkSettings.SERVER_DESTORY_MISSILE:
                serverMsg = JsonConvert.DeserializeObject<ServerDestoryMissileMsg>(dataStr);
                break;
            case NetworkSettings.SERVER_MISSILE_EXPLOSION:
                serverMsg = JsonConvert.DeserializeObject<ServerMissileExplosionMsg>(dataStr);
                break;
            case NetworkSettings.SERVER_CREATE_OTHER_PLAYER:
            case NetworkSettings.SERVER_SYNC_OTHER_PLAYER:
                serverMsg = JsonConvert.DeserializeObject<ServerOtherPlayerMsg>(dataStr);
                break;
            case NetworkSettings.SERVER_ALL_ENTITIES:
                Debug.Log(dataStr);
                serverMsg = JsonConvert.DeserializeObject<ServerAllEntityDataMsg>(dataStr);
                break;
            case NetworkSettings.SERVER_SYNC_OTHER_PLAYER_SHOOT:
                serverMsg = JsonConvert.DeserializeObject<ServerOtherPlayerShootMsg>(dataStr);
                break;
            case NetworkSettings.SERVER_CREATE_TRAP:
                serverMsg = JsonConvert.DeserializeObject<ServerTrapMsg>(dataStr);
                break;
            case NetworkSettings.SERVER_ELEPHANT_ATTACK:
                serverMsg = JsonConvert.DeserializeObject<ServerElephantAttackMsg>(dataStr);
                break;
            case NetworkSettings.SERVER_PLAYER_DISCONNECT:
                serverMsg = JsonConvert.DeserializeObject<ServerPlayerDisconnectMsg>(dataStr);
                break;
            case NetworkSettings.SERVER_NEXT_LEVEL_TIME:
                serverMsg = JsonConvert.DeserializeObject<ServerNextLevelTimeMsg>(dataStr);
                break;
            case NetworkSettings.SERVER_HIGHEST_SCORE:
                serverMsg = JsonConvert.DeserializeObject<ServerHighestScoreMsg>(dataStr);
                break;
        }
        if(serverMsg != null)
            serverMsg.msgType = msgType;

        return serverMsg;

    }
}
