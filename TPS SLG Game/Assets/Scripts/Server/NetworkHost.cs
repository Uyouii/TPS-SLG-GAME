using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using System.Threading;
using System.Text;
using System.IO;

public class NetworkHost {

    private TcpClient client;
    private IPAddress serverAddr;
    private int serverPort;

    private NetworkStream networkStream;

    StreamWriter streamWriter;
    StreamReader streamReader;

    public bool connected;

    private Byte[] receiveBuffer;
    private Byte[] dataBuffer;
    private int dataBufferLength;

    // receive msg queues
    public Queue<ServerMsg> receiveMessages;

    private static NetworkHost networkHostInstance = null;

    public static NetworkHost GetInstance()
    {
        if(networkHostInstance == null)
        {
            networkHostInstance = new NetworkHost();
        }
        return networkHostInstance;
    }

    private NetworkHost()
    {

        dataBufferLength = 0;
        receiveBuffer = new Byte[8192 * 4];
        dataBuffer = new Byte[8192 * 8];

        receiveMessages = new Queue<ServerMsg>();

        connected = false;
        HostInit();

        //use a thread to connect server so that game won't block on the connect process
        Thread connectThread = new Thread(new ThreadStart(ConnectServer));
        connectThread.IsBackground = true;
        connectThread.Start();

    }

    private void HostInit()
    {
        serverAddr = IPAddress.Parse(NetworkSettings.serverHostAddr);
        serverPort = NetworkSettings.serverPort;
        client = new TcpClient();
    }

    private void ConnectServer()
    {
        try
        {
            client.Connect(serverAddr, serverPort);
            Debug.Log("Connect Server Successfully to server: " + NetworkSettings.serverHostAddr);
        }

        catch (Exception e)
        {
            Debug.Log("Connect Server Failed with addr: " + NetworkSettings.serverHostAddr);
            Debug.Log(e);
            return;
        }

        connected = true;

    }

    public IEnumerator SendBytesMessage(byte[] bytes)
    {
        bool sendSucc = false;

        if(connected)
        {
            networkStream = client.GetStream();
            if (networkStream.CanWrite)
            {
                try
                {
                    networkStream.Write(bytes, 0, bytes.Length);
                    sendSucc = true;
                }
                catch (System.IO.IOException e)
                {
                    Debug.Log(e);
                    sendSucc = false;
                    connected = false;
                }
            }
        }

        yield return sendSucc;
    }

    public void ReceiveData()
    {
        if (connected)
        {
            try
            {
                networkStream = client.GetStream();
            }
            catch (InvalidOperationException e)
            {
                Debug.Log(e);
                connected = false;
            }
            finally
            {
                int length;

                // Read incomming stream into byte arrary. 					
                while (networkStream.DataAvailable)
                {
                    length = networkStream.Read(receiveBuffer, 0, receiveBuffer.Length);

                    Array.Copy(receiveBuffer, 0, dataBuffer, dataBufferLength, length);
                    dataBufferLength += length;
                    HandleReceiveMsg(dataBuffer, dataBufferLength);
                }
            }
        }
    }

    public void HandleReceiveMsg(byte[] dataBuffer, int dataLength)
    {
        int begin = 0;
        //Debug.Log(dataLength);
        while (begin < dataLength)
        {
            int msgLen = System.BitConverter.ToInt32(dataBuffer, begin);
            if (begin + msgLen > dataLength)
            {
                break;
            }
            begin += NetworkSettings.NET_HEAD_LENGTH_SIZE;
            int dataLen = msgLen - NetworkSettings.NET_HEAD_LENGTH_SIZE;

            ServerMsg serverMsg = MessageHandler.ParseFrom(dataBuffer, begin, msgLen);
            receiveMessages.Enqueue(serverMsg);

            begin += dataLen;
        }

        if (begin < dataLength)
        {
            Array.Copy(dataBuffer, begin, dataBuffer, 0, dataLength - begin);
        }
        dataBufferLength = dataLength - begin;
    }

    public void Close()
    {
        client.Close();
    }
}
