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
    public IngameManager(Server server, List<ClientInfo> clients)
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
            int half = clients.Count / 2;
            //////////////////////
            //Set each player's position and rotation
        }
        catch
        {
            return false;
        }
        return true;
    }

}
