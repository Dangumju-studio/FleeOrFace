using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Singleton Game Manager.
/// </summary>
public class IngameManager : MonoBehaviour {

    bool _isGamePlaying;
    bool isGamePlaying
    {
        get { return _isGamePlaying; }
        set { _isGamePlaying = value; }
    }

    Server server;

    List<Vector3> spawnPoint;

    /// <summary>
    /// Map number.
    /// </summary>
    public int mapNumber = 0;
    /// <summary>
    /// 3rd cam mode
    /// </summary>
    public bool is3rdCam = false;
    public string[] mapList;

    /// <summary>
    /// Current game's manager.
    /// </summary>
    /// <param name="server">Current server</param>
    /// <param name="clients">Current clientinfo list</param>
    void Start()
    {
        server = GameObject.FindGameObjectWithTag("NetworkController").GetComponent<Server>();
    }

    /// <summary>
    /// Initialize game
    /// </summary>
    /// <returns>Result of initializing. True = success</returns>
    public bool Initialize()
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
        int half = server.clients.Count / 2;
        for (int i = 0; i < server.clients.Count; i++)
        {
            if (i <= half) server.clients[i].userState = PlayerState.Zombie;
            else server.clients[i].userState = PlayerState.Human;
        }
        //mix
        for (int i = 0; i < server.clients.Count; i++)
        {
            int changeTarget = Random.Range(0, server.clients.Count);
            PlayerState tmp = server.clients[i].userState;
            server.clients[i].userState = server.clients[changeTarget].userState;
            server.clients[changeTarget].userState = tmp;
        }
    }
}
