using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using UnityEngine.UI;
using System.Text;

public class Client : MonoBehaviour {
    private Socket client;
    public EndPoint epServer;
    public string playerName = "Player";
    public string identification = "";  //assigned in MenuController (after Player name input)
    public string hostIP = "";
    public bool isConnected = false;
    public bool isLoadingStarting = false;
    public bool isGamePlaying = false;

    IngameManager gameManager;

    /// <summary>
    /// Every client's information list. Except this client.
    /// </summary>
    public List<ClientInfo> clients = new List<ClientInfo>();

    /// <summary>
    /// Chatting text queue.
    /// Every chatting message is pushed into txtChatQueue. Textbox in game or room refers this object to display chatting message.
    /// </summary>
    public Queue<string> txtChatQueue = new Queue<string>();

    /// <summary>
    /// Player's position and rotation
    /// </summary
    public string positionRotation = "0,0,0,0,0,0,0,True,False";  //(position),(rotation),(onground),(isFlashOn)
    /// <summary>
    /// When player push attack button, 'attack' variable turn to 'True'.
    /// </summary>
    public bool attack = false;

    byte[] bData = new byte[1024];

    void Start()
    { gameManager = GameObject.FindGameObjectWithTag("NetworkController").GetComponent<IngameManager>(); }

    /// <summary>
    /// Connect server method
    /// </summary>
    /// <param name="ip">Host's IP address</param>
    /// <param name="port">Host's port number. default = 9210</param>
    public void ConnectToServer(string ip, int port = 9210)
    {
        try
        {
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

            //Receive data from server asynchronously, and save the received data into Client.bData
            bData = new byte[1024];
            client.BeginReceiveFrom(bData, 0, bData.Length, SocketFlags.None, ref epServer, new AsyncCallback(OnReceive), null);
        }
        catch (Exception ex)
        {
            print(ex.Message);
        }
    }

    /// <summary>
    /// Called when any data has arrived.
    /// Received data is in 'Client.bData'(Global variable).
    /// </summary>
    /// <param name="ar"></param>
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
                        SendData(NetCommand.Check, "");
                    } else {
                        cInfo = new ClientInfo();
                        cInfo.name = msgReceived.name;
                        cInfo.identification = msgReceived.identify;
                        cInfo.lastCheck = DateTime.Now;
                        clients.Add(cInfo);
                    }
                    print(msgReceived.name + " Entered");
                    txtChatQueue.Enqueue(string.Format("{0} Entered!", msgReceived.name));

                    break;

                case NetCommand.Disconnect:
                    if (msgReceived.name == playerName && msgReceived.identify == identification)
                        isConnected = false;
                    else
                    {
                        //Disconnect someone
                        clients.Remove(clients.Find(c => c.name == msgReceived.name && c.identification == msgReceived.identify));
                        txtChatQueue.Enqueue(string.Format("{0} Leave..", msgReceived.name));
                    }
                    break;

                case NetCommand.Check:
                    //Check success, Modify ClientList(clients)
                    string[] players = msgReceived.msg.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach(string s in players)
                    {
                        string[] pStr = s.Split(new char[] { ':' });
                        ClientInfo ci = clients.Find(c => c.name.Equals(pStr[0]) && c.identification.Equals(pStr[1]));
                        if (ci != null)
                        {
                            //modify
                            ci.isReady = bool.Parse(pStr[2]);
                            ci.lastCheck = DateTime.Now;
                        }
                        else
                        {
                            //add
                            ci = new ClientInfo(pStr[0], pStr[1], bool.Parse(pStr[2]));
                            clients.Add(ci);
                        }
                    }

                    //remove ClientInfo in ClientList(clients) if it had removed from server.
                    for(int i=0; i < clients.Count; i++)
                    {
                        if (Array.Find<string>(players, s => s.Substring(0, s.LastIndexOf(':')).Equals(string.Format("{0}:{1}", clients[i].name, clients[i].identification))) == null)
                            clients.RemoveAt(i--);
                    }
                    break;

                case NetCommand.Chat:
                    //print(msgReceived.msg);
                    //push chatting message to queue
                    txtChatQueue.Enqueue(msgReceived.name + ":" + msgReceived.msg);
                    break;

                case NetCommand.MapSetting:
                    string[] settingvalues = msgReceived.msg.Split(new char[] { ',' });
                    gameManager.mapNumber = int.Parse(settingvalues[0]);
                    gameManager.is3rdCam = bool.Parse(settingvalues[1]);
                    break;

                case NetCommand.Ready:
                    cInfo = clients.Find(c => c.name == msgReceived.name && c.identification == msgReceived.identify);
                    if (cInfo != null) cInfo.isReady = bool.Parse(msgReceived.msg);
                    break;
                case NetCommand.LoadGame:
                    isLoadingStarting = true;
                    break;

                case NetCommand.StartGame:
                    isGamePlaying = true;
                    break;

                case NetCommand.PositionRotation:
                    cInfo = clients.Find(c => c.name == msgReceived.name && c.identification == msgReceived.identify);
                    string[] values = msgReceived.msg.Split(new char[] { ',' });
                    if(cInfo != null)
                    {
                        cInfo.userPosition = new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
                        cInfo.userRotation = new Quaternion(float.Parse(values[3]), float.Parse(values[4]), float.Parse(values[5]), float.Parse(values[6]));
                        cInfo.userIsOnGround = bool.Parse(values[7]);
                        cInfo.userIsFlashOn = bool.Parse(values[8]);
                    }
                    break;

                case NetCommand.Attack:

                    break;
            }

            //Continue receiving
            //bData = new byte[1024];
            //client.BeginReceiveFrom(bData, 0, bData.Length, SocketFlags.None, ref epServer, new AsyncCallback(OnReceive), null);

        }
        catch (Exception ex)
        {
            print(ex.Message);
        }
        finally {
            bData = new byte[1024];
            client.BeginReceiveFrom(bData, 0, bData.Length, SocketFlags.None, ref epServer, new AsyncCallback(OnReceive), null);
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

    /// <summary>
    /// Disconnect
    /// </summary>
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
    /// Send message method.
    /// </summary>
    /// <param name="msg"></param>
    public void SendData(NetCommand cmd, string msg)
    {
        NetworkData newMessage = new NetworkData();
        newMessage.name = playerName;
        newMessage.identify = identification;
        newMessage.cmd = cmd;
        newMessage.msg = msg;
        byte[] bData = newMessage.ConvertToByte();
        client.BeginSendTo(bData, 0, bData.Length, SocketFlags.None, epServer, new AsyncCallback(OnSend), null);
    }

    /// <summary>
    /// Send check message every 5 seconds to confirm connection.
    /// </summary>
    /// <returns></returns>
    public IEnumerator SendCheck()
    {
        while(isConnected)
        {
            print("SendCheck:" + playerName);
            yield return new WaitForSeconds(5f);
            SendData(NetCommand.Check, "");
        }
        print("Disconnected. No more check");
    }

    /// <summary>
    /// Send Player's control status.
    /// Player's position, Rotation, and Attack status.
    /// Called from main game scene - SceneController - FixedUpdate function
    /// </summary>
    /// <returns></returns>
    public void SendPlayerControl()
    {
        SendData(NetCommand.PositionRotation, positionRotation);
        if (attack) { SendData(NetCommand.Attack, "True"); attack = false; }
    }
}
