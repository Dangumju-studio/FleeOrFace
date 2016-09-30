using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using UnityEngine.UI;
using System.Text;

public class Client : MonoBehaviour {
    public Socket client;
    public EndPoint epServer;
    public string playerName = "Player";
    public string identification = "";
    public bool isConnected = false;

    /// <summary>
    /// Every client's information list. Except this client.
    /// </summary>
    List<ClientInfo> clients = new List<ClientInfo>();

    /// <summary>
    /// Chatting text component. Defferent component between ingame and waiting room.
    /// </summary>
    public Text txtChat;
    StringBuilder sbChat = new StringBuilder();

    byte[] bData = new byte[1024];

    public void ConnectToServer(string ip, int port = 9210)
    {
        //try
        //{
            //udp socket initialize
            client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            //get server's ip address
            IPAddress serverIP = IPAddress.Parse(ip);
            epServer = new IPEndPoint(serverIP, port);

            NetworkData msgToSend = new NetworkData();
            msgToSend.cmd = NetCommand.Connect;
            msgToSend.name = playerName;
            msgToSend.identify = identification;

            bData = msgToSend.ConvertToByte();

            //Connect to server
            client.BeginSendTo(bData, 0, bData.Length, SocketFlags.None, epServer, new AsyncCallback(OnSend), null);

            //Receive data from server asynchronously
            bData = new byte[1024];
            client.BeginReceiveFrom(bData, 0, bData.Length, SocketFlags.None, ref epServer, new AsyncCallback(OnReceive), null);
        //}
        //catch (Exception ex)
        //{
        //    print(ex.Message);
        //}
    }

    private void OnReceive(IAsyncResult ar)
    {
        try
        {
            client.EndReceive(ar);

            //Create Data refer to received byte array data
            NetworkData msgReceived = new NetworkData(bData);
            ClientInfo cInfo;
            switch(msgReceived.cmd)
            {
                case NetCommand.Connect:
                    //Connect success
                    if(msgReceived.name == playerName && msgReceived.identify == identification)
                    {
                        isConnected = true;
                    } else {
                        cInfo = new ClientInfo();
                        cInfo.name = msgReceived.name;
                        clients.Add(cInfo);
                        txtChat.text += string.Format("{0} Entered!\n", msgReceived.name);
                    }
                    break;

                case NetCommand.Disconnect:
                    if (msgReceived.name == playerName && msgReceived.identify == identification)
                        isConnected = false;
                    else
                    {
                        //Disconnect someone
                        clients.Remove(clients.Find(c => c.name == msgReceived.name && c.identification == msgReceived.identify));
                        txtChat.text += string.Format("{0} Leave..\n", msgReceived.name);
                    }
                    break;

                case NetCommand.Check:
                    //Check success, Get ClientList
                    string[] players = msgReceived.msg.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    break;

                case NetCommand.Chat:
                    sbChat.AppendLine(msgReceived.msg);
                    //find newline character
                    int cntNewline = 0;
                    foreach (char c in sbChat.ToString())
                        if (c == '\n') cntNewline += 1;
                    if(cntNewline > 50) //Max chat line = 50
                        //cut off old message
                        sbChat.Remove(0, sbChat.ToString().IndexOf('\n'));
                    txtChat.text = sbChat.ToString();
                    break;

                case NetCommand.Ready:
                    cInfo = clients.Find(c => c.name == msgReceived.name && c.identification == msgReceived.identify);
                    cInfo.isReady = bool.Parse(msgReceived.msg);
                    break;

                case NetCommand.PositionRotation:

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
        NetworkData msgToSend = new NetworkData();
        msgToSend.cmd = NetCommand.Disconnect;
        msgToSend.name = playerName;
        msgToSend.msg = null;

        byte[] bData = msgToSend.ConvertToByte();
        client.SendTo(bData, 0, bData.Length, SocketFlags.None, epServer);
        client.Close();
    }

    /// <summary>
    /// Message send method.
    /// </summary>
    /// <param name="msg"></param>
    public void SendData(NetCommand cmd, string msg)
    {
        NetworkData newMessage = new NetworkData();
        newMessage.name = playerName;
        newMessage.cmd = cmd;
        newMessage.msg = msg;
        byte[] bData = newMessage.ConvertToByte();
        client.BeginSendTo(bData, 0, bData.Length, SocketFlags.None, epServer, new AsyncCallback(OnReceive), epServer);
    }
}
