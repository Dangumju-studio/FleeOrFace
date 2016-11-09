using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Current Game Manager.
/// </summary>
public class IngameManager : MonoBehaviour {

    bool _isGamePlaying;
    bool isGamePlaying
    {
        get { return _isGamePlaying; }
        set { _isGamePlaying = value; }
    }

    Server server;

    /// <summary>
    /// Players' spawning point
    /// </summary>
    List<Vector3> spawnPoint;

    /// <summary>
    /// Map number.
    /// </summary>
    public int mapNumber = 0;
    public string[] mapList;

    /// <summary>
    /// 3rd cam mode
    /// </summary>
    public bool is3rdCam = false;

    /// <summary>
    /// Zombie - Human Switching rotation left time (seconds)
    /// </summary>
    public int rotateTimeLeft = 30;
    /// <summary>
    /// Zombie - Human Switching rotation time term (seconds).
    /// Decided randomly(10s~35s) when role rotated.
    /// </summary>
    public int rotateTimeTerm = 30;
    /// <summary>
    /// Last rotated(switched) datetime.
    /// </summary>
    private System.DateTime lastRotatedTime;

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
    /// Set role to players zombie or human.
    /// Only called from Server.
    /// </summary>
    public void DistributeRole()
    {
        if (!server.isServerOpened) return;
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

        lastRotatedTime = System.DateTime.Now;
        rotateTimeTerm = Random.Range(10, 35);
    }

    IEnumerator rotatePlayerRole()
    {
        while(isGamePlaying)
        {
            yield return new WaitForSeconds(0.5f);

            int between = (lastRotatedTime - System.DateTime.Now).Seconds;
            rotateTimeLeft = rotateTimeTerm - between;

            //Current role time out. Rotate role again.
            if (rotateTimeTerm < 0) DistributeRole();
        }
    }
}
