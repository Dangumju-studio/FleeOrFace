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
    bool isServerOpened = false;

    List<ClientInfo> clients = new List<ClientInfo>();
    byte[] bData = new byte[1024];

    const int CLIENT_TIMEOUT = 30;

    /// <summary>
    /// Start the server.
    /// </summary>
    /// <param name="port"></param>
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
            isServerOpened = true;
            StartCoroutine(CheckClientsConnection());
        }
        catch(Exception ex)
        {
            print(ex.Message);
        }

    }

    /// <summary>
    /// Close current server.
    /// </summary>
    public void CloseServer()
    {
        udpServer.Disconnect(false);
        udpServer.Close();
        udpServer.Dispose();
        udpServer = null;
        isServerOpened = false;
    }

    /// <summary>
    /// OnReceive some data from any client
    /// </summary>
    /// <param name="ar"></param>
    private void OnReceive(IAsyncResult ar)
    {
        //epSender = Sender
        EndPoint epSender = new IPEndPoint(IPAddress.Any, 0);
        udpServer.EndReceiveFrom(ar, ref epSender);
        try
        {
            //receive data
            NetworkData dataReceived = new NetworkData(bData);

            //response data
            NetworkData dataToSend = new NetworkData();

            //set response data
            dataToSend.name = dataReceived.name;
            dataToSend.cmd = dataReceived.cmd;
            dataToSend.identify = dataReceived.identify;
            dataToSend.msg = dataReceived.msg;
            byte[] msg = dataToSend.ConvertToByte();
            ClientInfo cInfo;

            switch (dataReceived.cmd)
            {
                case NetCommand.Connect:
                    //ignore player (clients size = 8)
                    if (clients.Count >= 8)
                    {
                        dataToSend.cmd = NetCommand.Disconnect;
                        dataToSend.msg = "No more empty slots.";
                        udpServer.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, epSender, new AsyncCallback(OnSend), epSender);
                        break;
                    }
                    //add player (clients size < 8)
                    cInfo = new ClientInfo();
                    cInfo.ep = epSender;
                    cInfo.name = dataReceived.name;
                    cInfo.identification = dataReceived.identify;
                    clients.Add(cInfo);
                    //send every player to this player's connection state
                    foreach (ClientInfo c in clients)
                        udpServer.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, c.ep, new AsyncCallback(OnSend), c.ep);
                    break;

                case NetCommand.Disconnect:
                    //remove player
                        clients.Remove(clients.Find(c => c.ep.Equals(epSender)));
                    //send every player to this player's connection state
                    foreach (ClientInfo c in clients)
                        udpServer.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, c.ep, new AsyncCallback(OnSend), c.ep);
                    break;

                case NetCommand.Check:
                    //update lastcheck time
                    cInfo = clients.Find(c => c.ep.Equals(epSender));
                    cInfo.lastCheck = DateTime.Now;
                    dataToSend.msg = "";
                    //Ready to send clients list.
                    StringBuilder sb = new StringBuilder();
                    foreach (ClientInfo c in clients)
                        sb.AppendFormat("{0}:{1}:{2},", c.name, c.identification, c.isReady.ToString());
                    dataToSend.msg = sb.ToString();
                    //convert data to byte
                    msg = dataToSend.ConvertToByte();
                    udpServer.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, epSender, new AsyncCallback(OnSend), epSender);
                    break;

                case NetCommand.Chat:
                    //send every player to chat message
                    foreach (ClientInfo c in clients)
                    {
                       if (c.ep == epSender) continue;
                        udpServer.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, c.ep, new AsyncCallback(OnSend), c.ep);
                    }
                    break;

                case NetCommand.Ready:
                    //update ready state
                    cInfo = clients.Find(c => c.ep.Equals(epSender));
                    cInfo.isReady = bool.Parse(dataReceived.msg);
                    //send every player to this player's ready state
                    foreach (ClientInfo c in clients)
                        udpServer.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, c.ep, new AsyncCallback(OnSend), c.ep);

                    //if all players are ready, start the game.
                    cInfo = clients.Find(c => c.isReady == false);
                    if(cInfo != null)
                    {
                        dataToSend.cmd = NetCommand.StartGame;
                        dataToSend.msg = "";
                        msg = dataToSend.ConvertToByte();
                        foreach (ClientInfo c in clients)
                            udpServer.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, c.ep, new AsyncCallback(OnSend), c.ep);
                    }
                    break;

                case NetCommand.Attack:
                    foreach (ClientInfo c in clients)
                    {
                        if (c.ep == epSender) continue;
                        udpServer.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, c.ep, new AsyncCallback(OnSend), c.ep);
                    }
                    break;

                case NetCommand.PositionRotation:
                    foreach (ClientInfo c in clients)
                    {
                        if (c.ep == epSender) continue;
                        udpServer.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, c.ep, new AsyncCallback(OnSend), c.ep);
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            print(ex.Message);
        }
        finally
        {
            //Continue receiving
            bData = new byte[1024];
            udpServer.BeginReceiveFrom(bData, 0, bData.Length, SocketFlags.None, ref epSender, new AsyncCallback(OnReceive), epSender);
        }
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

    /// <summary>
    /// Check clients' connection with ClientInfo lastcheck variable.
    /// </summary>
    /// <returns></returns>
    IEnumerator CheckClientsConnection()
    {
        while(isServerOpened)
        {
            yield return new WaitForSeconds(5f);
            NetworkData data = new NetworkData();
            for(int i=0; i<clients.Count; i++)
            {
                //if a client doesn't send check packet until 30(CLIENT_TIMEOUT) seconds from last check time, 
                //judge by that client is disconnected from server.
                if ((DateTime.Now - clients[i].lastCheck).Seconds > CLIENT_TIMEOUT)
                {
                    data.name = clients[i].name;
                    data.identify = clients[i].identification;
                    data.cmd = NetCommand.Disconnect;
                    data.msg = "";
                    clients.RemoveAt(i--);
                    byte[] msg = data.ConvertToByte();
                    foreach (ClientInfo c in clients)
                        udpServer.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, c.ep, new AsyncCallback(OnSend), c.ep);
                }
            }
        }
    }
}
