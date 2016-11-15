using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System;
using System.Text;
using System.Diagnostics;

public class Server : MonoBehaviour {

    UdpClient udpClient;
    Socket udpServer;
    public bool isServerOpened = false;

    public List<ClientInfo> clientLists = new List<ClientInfo>();
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
                    if (clientLists.Count >= 8)
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
                    clientLists.Add(cInfo);
                    //send every player to this player's connection state
                    foreach (ClientInfo c in clientLists)
                        udpServer.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, c.ep, new AsyncCallback(OnSend), c.ep);
                    break;

                //When player disconnected
                case NetCommand.Disconnect:
                    //remove player
                    int removedIdx = clientLists.FindIndex(c => c.ep.Equals(epSender));
                    clientLists[removedIdx].isConnected = false;
                    clientLists.RemoveAt(removedIdx);
                    //send every player to this player's connection state
                    foreach (ClientInfo c in clientLists)
                        udpServer.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, c.ep, new AsyncCallback(OnSend), c.ep);
                    break;

                //Check player's connection state
                case NetCommand.Check:
                    //update lastcheck time
                    cInfo = ClientInfo.findClientInfo(ref clientLists, epSender);
                    //print("Checked: " + cInfo.name);
                    cInfo.lastCheck = DateTime.Now;
                    dataToSend.msg = "";
                    //Ready to send clients list.
                    StringBuilder sb = new StringBuilder();
                    foreach (ClientInfo c in clientLists)
                        sb.AppendFormat("{0}:{1}:{2},", c.name, c.identification, c.isReady.ToString());
                    dataToSend.msg = sb.ToString();
                    //convert data to byte
                    msg = dataToSend.ConvertToByte();
                    udpServer.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, epSender, new AsyncCallback(OnSend), epSender);
                    break;

                //Chatting message
                case NetCommand.Chat:
                    //send every player to chat message
                    foreach (ClientInfo c in clientLists)
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
                    foreach (ClientInfo c in clientLists)
                    {
                        if (c.ep.Equals(epSender)) continue;
                        udpServer.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, c.ep, new AsyncCallback(OnSend), c.ep);
                    }
                    break;

                //Ready in lobby
                case NetCommand.Ready:
                    //update ready state
                    cInfo = ClientInfo.findClientInfo(ref clientLists, epSender);
                    cInfo.isReady = bool.Parse(dataReceived.msg);
                    //send every player to this player's ready state
                    foreach (ClientInfo c in clientLists)
                        udpServer.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, c.ep, new AsyncCallback(OnSend), c.ep);

                    //if all players are ready, Start LOADING GAME SCENE
                    cInfo = null;
                    if (clientLists.Count >= 2)
                    {
                        cInfo = clientLists.Find(c => c.isReady == false);
                        if (cInfo == null)
                        {
                            dataToSend.cmd = NetCommand.LoadGame;
                            dataToSend.msg = "";
                            msg = dataToSend.ConvertToByte();
                            foreach (ClientInfo c in clientLists)
                                udpServer.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, c.ep, new AsyncCallback(OnSend), c.ep);
                        }
                    }
                    break;
                //When one's Loading done
                case NetCommand.LoadGame:
                    cInfo = ClientInfo.findClientInfo(ref clientLists, epSender);
                    cInfo.isLoadingDone = bool.Parse(dataReceived.msg);
                    //if all players' loading is done, START THE GAME.
                    cInfo = null;
                    cInfo = clientLists.Find(c => c.isLoadingDone == false);
                    if (cInfo == null)
                    {
                        gameManager.isGamePlaying = true;
                        gameManager.Initialize();
                        dataToSend.cmd = NetCommand.StartGame;
                        dataToSend.msg = "";
                        msg = dataToSend.ConvertToByte();
                        foreach (ClientInfo c in clientLists)
                            udpServer.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, c.ep, new AsyncCallback(OnSend), c.ep);
                    }
                    break;
                
                //Attack
                case NetCommand.Attack:
                    //Attack - Kill check
                    cInfo = ClientInfo.findClientInfo(ref clientLists, epSender);
                    string attacked = "";
                    if (cInfo.userState == PlayerState.Zombie)
                    {
                        //Get attacked player's identification string.
                        if (cInfo != null) attacked = gameManager.AttackCheck(ref cInfo);
                        if (attacked.Length > 0)
                        {
                            //Get attacked player via attacked identification string.
                            ClientInfo cInfo2 = ClientInfo.findClientInfo(ref clientLists, attacked);
                            if (cInfo2 != null)
                            {
                                if (cInfo2.userState == PlayerState.Human)
                                {
                                    print(cInfo2.name + " has attacked by " + cInfo.name);
                                    cInfo2.userState = PlayerState.Dead;

                                    //########################### GAMEOVER CHECK ###################
                                    //Is the game end?
                                    int zombies = clientLists.FindAll(c => c.userState == PlayerState.Zombie).Count;
                                    int humans = clientLists.FindAll(c => c.userState == PlayerState.Human).Count;
                                    //There's only one survival => GAME OVER
                                    if(zombies + humans <= 1 && gameManager.isGamePlaying)
                                    {
                                        gameManager.isGamePlaying = false;
                                        dataToSend = new NetworkData();
                                        dataToSend.cmd = NetCommand.Gameover;
                                        dataToSend.identify = cInfo.identification; //the winner
                                        byte[] endmsg = dataToSend.ConvertToByte();
                                        //SEND GAMEOVER MESSAGE
                                        //################ GAME OVER ########################
                                        foreach (ClientInfo c in clientLists)
                                        {
                                            //Reset every clientinfo
                                            c.isReady = false;
                                            c.isLoadingDone = false;
                                            udpServer.BeginSendTo(endmsg, 0, endmsg.Length, SocketFlags.None, c.ep, new AsyncCallback(OnSend), c.ep);
                                        }
                                    }
                                    //########################### END GAMEOVER CHECK ####################
                                }
                                else attacked = "";
                            }
                            else attacked = "";
                        }
                    }
                    dataToSend = new NetworkData(msg);
                    dataToSend.msg = attacked;
                    msg = dataToSend.ConvertToByte();
                    foreach (ClientInfo c in clientLists)
                    {
                        //if (c.ep.Equals(epSender)) continue;
                        udpServer.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, c.ep, new AsyncCallback(OnSend), c.ep);
                    }
                    break;
                //Moving/Turning
                case NetCommand.PositionRotation:
                    cInfo = ClientInfo.findClientInfo(ref clientLists, epSender);
                    values = dataReceived.msg.Split(new char[] { ',' });
                    if (cInfo != null)
                    {
                        cInfo.userPosition = new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
                        cInfo.userRotation = new Quaternion(float.Parse(values[3]), float.Parse(values[4]), float.Parse(values[5]), float.Parse(values[6]));
                        cInfo.userIsOnGround = bool.Parse(values[7]);
                        cInfo.userIsFlashOn = bool.Parse(values[8]);
                    }
                    foreach (ClientInfo c in clientLists)
                    {
                        if (c.ep.Equals(epSender)) continue;
                        udpServer.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, c.ep, new AsyncCallback(OnSend), c.ep);
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            StackTrace st = new StackTrace(ex, true);
            StackFrame frame = st.GetFrame(0);
            print(ex.Message);
            print(string.Format("{0} - {1} occured error ({2}:{3})", frame.GetFileName(), frame.GetMethod().Name, frame.GetFileLineNumber(), frame.GetFileColumnNumber()));
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
            for(int i=0; i<clientLists.Count; i++)
            {
                //if a client doesn't send check packet until 30(CLIENT_TIMEOUT) seconds from last check time, 
                //judge by that client is disconnected from server.
                if ((DateTime.Now - clientLists[i].lastCheck).Seconds > CLIENT_TIMEOUT)
                {
                    data.name = clientLists[i].name;
                    data.identify = clientLists[i].identification;
                    data.cmd = NetCommand.Disconnect;
                    data.msg = "";
                    clientLists.RemoveAt(i--);
                    byte[] msg = data.ConvertToByte();
                    foreach (ClientInfo c in clientLists)
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
        foreach (ClientInfo c in clientLists)
            udpServer.BeginSendTo(bData, 0, bData.Length, SocketFlags.None, c.ep, new AsyncCallback(OnSend), null);
    }
}
