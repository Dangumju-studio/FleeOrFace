using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Collections;

public class Client : MonoBehaviour {
    public Socket client;
    public EndPoint epServer;
    public string strName = "Player";

    byte[] bData = new byte[1024];

    public void ConnectToServer(string ip, int port = 9210)
    {
        try
        {
            //udp socket initialize
            client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            //get server's ip address
            IPAddress serverIP = IPAddress.Parse(ip);
            epServer = new IPEndPoint(serverIP, port);

            Network.Data msgToSend = new Network.Data();
            msgToSend.cmd = NetCommand.Connect;
            msgToSend.msg = null;
            msgToSend.name = strName;

            byte[] bData = msgToSend.ConvertToByte();

            //Connect to server
            client.BeginSendTo(bData, 0, bData.Length, SocketFlags.None, epServer, new AsyncCallback(OnSend), null);

            //Receive data from server asynchronously
            bData = new byte[1024];
            client.BeginReceiveFrom(bData, 0, bData.Length, SocketFlags.None, ref epServer, new AsyncCallback(OnReceive), null);
        }
        catch (Exception ex)
        {
            print(ex.Message);
        }
    }

    private void OnReceive(IAsyncResult ar)
    {
        try
        {
            client.EndReceive(ar);

            //Create Data refer to received byte array data
            Network.Data msgReceived = new Network.Data(bData);

            switch(msgReceived.cmd)
            {
                case NetCommand.Connect:

                    break;
                case NetCommand.Disconnect:

                    break;
                case NetCommand.Check:

                    break;
                case NetCommand.Chat:
                    
                    break;
                case NetCommand.Ready:

                    break;
                case NetCommand.Position:

                    break;
                case NetCommand.Attack:

                    break;
            }

        }
        catch (Exception ex)
        {
            print(ex.Message);
        }
    }

    private void OnSend(IAsyncResult ar)
    {
        try
        {
            client.EndSend(ar);
        }
        catch (Exception ex)
        {
            print(ex.Message);
        }
    }

    public void Disconnect()
    {
        Network.Data msgToSend = new Network.Data();
        msgToSend.cmd = NetCommand.Disconnect;
        msgToSend.name = strName;
        msgToSend.msg = null;

        byte[] bData = msgToSend.ConvertToByte();
        client.SendTo(bData, 0, bData.Length, SocketFlags.None, epServer);
        client.Close();
    }
}
