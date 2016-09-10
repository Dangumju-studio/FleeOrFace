using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System;

public class Server : MonoBehaviour {

    UdpClient udpClient;
    Socket udpServer;


    List<ClientInfo> clients = new List<ClientInfo>();
    byte[] bData = new byte[1024];

	public void startServer(int port = 9210)
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

    private void OnReceive(IAsyncResult ar)
    {
        try
        {
            EndPoint epSender = new IPEndPoint(IPAddress.Any, 0);
            udpServer.EndReceiveFrom(ar, ref epSender);

            //receive
            Network.Data dataReceived = new Network.Data(bData);

            //response
            Network.Data dataToSend = new Network.Data();

            //set response data
            dataToSend.name = dataReceived.name;

            //send code :
            //serverSocket.BeginSendTo(message, 0, message.Length, SocketFlags.None, clientInfo.endpoint, new AsyncCallback(OnSend), clientInfo.endpoint);
        }
        catch(Exception ex)
        {
            print(ex.Message);
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
            print(e.Message);
        }
    }

    class ClientInfo
    {
        IPEndPoint ep;
        string name;
    }
}
