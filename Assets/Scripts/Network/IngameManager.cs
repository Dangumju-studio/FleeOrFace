using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IngameManager {

    bool _isGamePlaying;
    bool isGamePlaying
    {
        get { return _isGamePlaying; }
        set { _isGamePlaying = value; }
    }

    Server server;
    List<ClientInfo> clients;

    /// <summary>
    /// Current game's manager.
    /// </summary>
    /// <param name="server">Current server</param>
    /// <param name="clients">Current clientinfo list</param>
    public IngameManager(ref Server server, ref List<ClientInfo> clients)
    {
        this.server = server;
        this.clients = clients;
        Initialize();
    }

    /// <summary>
    /// Initialize game
    /// </summary>
    /// <returns>Result of initializing. True = success</returns>
    bool Initialize()
    {
        try
        {
            //Zombie or Human
            DistributeRole();
            //Set each player's position and rotation

        }
        catch
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Set role to players zombie or human
    /// </summary>
    void DistributeRole()
    {
        int half = clients.Count / 2;
        for (int i = 0; i < clients.Count; i++)
        {
            if (i <= half) clients[i].userState = PlayerState.Zombie;
            else clients[i].userState = PlayerState.Human;
        }
        //mix
        for (int i = 0; i < clients.Count; i++)
        {
            int changeTarget = Random.Range(0, clients.Count);
            PlayerState tmp = clients[i].userState;
            clients[i].userState = clients[changeTarget].userState;
            clients[changeTarget].userState = tmp;
        }
    }
}
