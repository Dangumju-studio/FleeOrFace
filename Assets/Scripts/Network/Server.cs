using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System;
using System.Text;

public class Server : MonoBehaviour {

    UdpClient udpClient;
    Socket udpServer;


    List<ClientInfo> clients = new List<ClientInfo>();
    byte[] bData = new byte[1024];

	public void StartServer(int port = 9210)
    {
        try
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, port);
            udpServer = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpServer.Bind(ep);

            EndPoint epSender = new IPEndPoint(IPAddress.Any, 0);

            //start receiving data
            udpServer.BeginReceiveFrom(bData, 0, bData.Length, SocketFlags.None, ref epSender, new AsyncCallback(OnReceive), epSender);
        }
        catch(Exception ex)
        {
            print(ex.Message);
        }

    }

    public void CloseServer()
    {
        udpServer.Disconnect(false);
        udpServer.Close();
        udpServer.Dispose();
        udpServer = null;
    }

    private void OnReceive(IAsyncResult ar)
    {
        //try
        //{
            EndPoint epSender = new IPEndPoint(IPAddress.Any, 0);
            udpServer.EndReceiveFrom(ar, ref epSender);

            //receive
            NetworkData dataReceived = new NetworkData(bData);

            //response
            NetworkData dataToSend = new NetworkData();

            //set response data
            dataToSend.name = dataReceived.name;
            dataToSend.cmd = dataReceived.cmd;
            dataToSend.identify = dataReceived.identify;
            byte[] msg = dataToSend.ConvertToByte();
            ClientInfo cInfo;

            switch (dataReceived.cmd)
            {
                case NetCommand.Connect:
                    //add player
                    cInfo = new ClientInfo();
                    cInfo.ep = epSender;
                    cInfo.name = dataReceived.name;
                    cInfo.identification = dataReceived.identify;
                    clients.Add(cInfo);
                    udpServer.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, epSender, new AsyncCallback(OnSend), epSender);
                    //send every player to this player's connection state
                    foreach (ClientInfo c in clients)
                        udpServer.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, c.ep, new AsyncCallback(OnSend), c.ep);
                    break;

                case NetCommand.Disconnect:
                    //remove player
                    clients.Remove(clients.Find(c => c.ep == epSender));
                    //send every player to this player's connection state
                    foreach (ClientInfo c in clients)
                        udpServer.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, c.ep, new AsyncCallback(OnSend), c.ep);
                    break;

                case NetCommand.Check:
                    //update lastcheck time
                    cInfo = clients.Find(c => c.ep == epSender);
                    cInfo.lastCheck = DateTime.Now;
                    dataToSend.msg = "";
                    //Ready to send clients list.
                    StringBuilder sb = new StringBuilder();
                    foreach (ClientInfo c in clients)
                        sb.AppendFormat("{0},", c.name);
                    dataToSend.msg = sb.ToString();
                    //convert data to byte
                    msg = dataToSend.ConvertToByte();
                    udpServer.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, epSender, new AsyncCallback(OnSend), epSender);
                    break;

                case NetCommand.Chat:
                    //send every player to chat message
                    foreach(ClientInfo c in clients)
                        udpServer.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, c.ep, new AsyncCallback(OnSend), c.ep);
                    break;

                case NetCommand.Ready:
                    //update ready state
                    cInfo = clients.Find(c => c.ep == epSender);
                    cInfo.isReady = bool.Parse(dataReceived.msg);
                    //send every player to this player's ready state
                    foreach (ClientInfo c in clients)
                        udpServer.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, c.ep, new AsyncCallback(OnSend), c.ep);
                    break;

                case NetCommand.Attack:
                    foreach (ClientInfo c in clients)
                    {
                        udpServer.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, c.ep, new AsyncCallback(OnSend), c.ep);
                    }
                    break;

                case NetCommand.PositionRotation:
                    foreach (ClientInfo c in clients)
                    {
                        udpServer.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, c.ep, new AsyncCallback(OnSend), c.ep);
                    }
                    break;
            }

            udpServer.BeginReceiveFrom(bData, 0, bData.Length, SocketFlags.None, ref epSender, new AsyncCallback(OnReceive), epSender);

        //send code :
        //serverSocket.BeginSendTo(message, 0, message.Length, SocketFlags.None, clientInfo.endpoint, new AsyncCallback(OnSend), clientInfo.endpoint);
        //}
        //catch(Exception ex)
        //{
        //    print(ex.Message);
        //}
    }

    private void OnSend(IAsyncResult ar)
    {
        try
        {
            udpServer.EndSend(ar);
        }
        catch(Exception ex)
        {
            print(ex.Message);
        }
    }

}
