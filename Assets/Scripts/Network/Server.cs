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
    public bool isServerOpened = false;

    public List<ClientInfo> clients = new List<ClientInfo>();
    byte[] bData = new byte[1024];

    const int CLIENT_TIMEOUT = 15;

    IngameManager gameManager;

    void Start()
    { gameManager = GameObject.FindGameObjectWithTag("NetworkController").GetComponent<IngameManager>(); }

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
            string[] values;
            switch (dataReceived.cmd)
            {
                // When player connected
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

                //When player disconnected
                case NetCommand.Disconnect:
                    //remove player
                    int removedIdx = clients.FindIndex(c => c.ep.Equals(epSender));
                    clients[removedIdx].isConnected = false;
                    clients.RemoveAt(removedIdx);
                    //send every player to this player's connection state
                    foreach (ClientInfo c in clients)
                        udpServer.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, c.ep, new AsyncCallback(OnSend), c.ep);
                    break;

                //Check player's connection state
                case NetCommand.Check:
                    //update lastcheck time
                    cInfo = clients.Find(c => c.ep.Equals(epSender));
                    //print("Checked: " + cInfo.name);
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

                //Chatting message
                case NetCommand.Chat:
                    //send every player to chat message
                    foreach (ClientInfo c in clients)
                    {
                       //if (c.ep.Equals(epSender)) continue;
                        udpServer.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, c.ep, new AsyncCallback(OnSend), c.ep);
                    }
                    break;

                //Map setting changed
                case NetCommand.MapSetting:
                    values = dataReceived.msg.Split(new char[] { ',' });
                    gameManager.mapNumber = int.Parse(values[0]);
                    gameManager.is3rdCam = bool.Parse(values[1]);
                    foreach (ClientInfo c in clients)
                    {
                        if (c.ep.Equals(epSender)) continue;
                        udpServer.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, c.ep, new AsyncCallback(OnSend), c.ep);
                    }
                    break;

                //Ready in lobby
                case NetCommand.Ready:
                    //update ready state
                    cInfo = clients.Find(c => c.ep.Equals(epSender));
                    cInfo.isReady = bool.Parse(dataReceived.msg);
                    //send every player to this player's ready state
                    foreach (ClientInfo c in clients)
                        udpServer.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, c.ep, new AsyncCallback(OnSend), c.ep);

                    //if all players are ready, Start LOADING GAME SCENE
                    cInfo = null;
                    if (clients.Count >= 2)
                    {
                        cInfo = clients.Find(c => c.isReady == false);
                        if (cInfo == null)
                        {
                            dataToSend.cmd = NetCommand.LoadGame;
                            dataToSend.msg = "";
                            msg = dataToSend.ConvertToByte();
                            foreach (ClientInfo c in clients)
                                udpServer.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, c.ep, new AsyncCallback(OnSend), c.ep);
                        }
                    }
                    break;
                //When one's Loading done
                case NetCommand.LoadGame:
                    cInfo = clients.Find(c => c.ep.Equals(epSender));
                    cInfo.isLoadingDone = bool.Parse(dataReceived.msg);
                    //if all players' loading is done, START THE GAME.
                    cInfo = null;
                    cInfo = clients.Find(c => c.isLoadingDone == false);
                    if (cInfo == null)
                    {
                        gameManager.isGamePlaying = true;
                        gameManager.Initialize();
                        dataToSend.cmd = NetCommand.StartGame;
                        dataToSend.msg = "";
                        msg = dataToSend.ConvertToByte();
                        foreach (ClientInfo c in clients)
                            udpServer.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, c.ep, new AsyncCallback(OnSend), c.ep);
                    }
                    break;
                
                //Attack
                case NetCommand.Attack:
                    foreach (ClientInfo c in clients)
                    {
                        if (c.ep.Equals(epSender)) continue;
                        udpServer.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, c.ep, new AsyncCallback(OnSend), c.ep);
                    }

                    //Attack - Kill check
                    cInfo = clients.Find(c => c.ep.Equals(epSender));
                    if (cInfo.userState == PlayerState.Zombie)
                    {
                        string attacked = "";
                        if (cInfo != null) attacked = gameManager.AttackCheck(ref cInfo);
                        if (attacked.Length > 0)
                        {
                            ClientInfo cInfo2 = clients.Find(c => c.identification == attacked);
                            if (cInfo2 != null)
                            {
                                print(cInfo2.name + " has attacked by " + cInfo.name);
                            }
                        }
                    }
                    break;
                //Moving/Turning
                case NetCommand.PositionRotation:
                    cInfo = clients.Find(c => c.ep.Equals(epSender));
                    values = dataReceived.msg.Split(new char[] { ',' });
                    if (cInfo != null)
                    {
                        cInfo.userPosition = new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
                        cInfo.userRotation = new Quaternion(float.Parse(values[3]), float.Parse(values[4]), float.Parse(values[5]), float.Parse(values[6]));
                        cInfo.userIsOnGround = bool.Parse(values[7]);
                        cInfo.userIsFlashOn = bool.Parse(values[8]);
                    }
                    foreach (ClientInfo c in clients)
                    {
                        if (c.ep.Equals(epSender)) continue;
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

    /// <summary>
    /// Notice message
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="msg"></param>
    public void NoticeData(NetCommand cmd, string msg)
    {
        NetworkData newMessage = new NetworkData();
        newMessage.cmd = cmd;
        newMessage.msg = msg;
        byte[] bData = newMessage.ConvertToByte();
        foreach (ClientInfo c in clients)
            udpServer.BeginSendTo(bData, 0, bData.Length, SocketFlags.None, c.ep, new AsyncCallback(OnSend), null);
    }
}
