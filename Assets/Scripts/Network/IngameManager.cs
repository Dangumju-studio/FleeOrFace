using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Current Game Manager.
/// </summary>
public class IngameManager : MonoBehaviour {

    bool _isGamePlaying;
    public bool isGamePlaying
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

            //Role rotate timer start => in GameController.cs FixedUpdate() gamestart part. (cross thread..)
        }
        catch (System.Exception e)
        {
            print(e.Message);
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
        print("role distribute start");
        if (!server.isServerOpened) return;
        int half = server.clients.Count / 2;
        for (int i = 0; i < server.clients.Count; i++)
        {
            if (i < half) server.clients[i].userState = PlayerState.Human;
            else server.clients[i].userState = PlayerState.Zombie;
        }
        //mix(shuffle)
        System.Random rnd = new System.Random();
        int n = server.clients.Count;
        while(n>1)
        {
            n--;
            int k = rnd.Next(n + 1);
            int ps = (int)server.clients[k].userState;
            server.clients[k].userState = (PlayerState)((int)server.clients[n].userState);
            server.clients[n].userState = (PlayerState)ps;
        }

        //Send role information to clients
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach(ClientInfo ci in server.clients)
            sb.AppendFormat("{0}:{1},", ci.identification, (int)ci.userState);
        server.NoticeData(NetCommand.RoleRotate, sb.ToString());

        lastRotatedTime = System.DateTime.Now;
        rotateTimeTerm = rnd.Next(10, 35);
    }

    /// <summary>
    /// Players role rotation timer / DistributeRole caller.
    /// </summary>
    /// <returns></returns>
    public IEnumerator RotatePlayerRoleTimer()
    {
        if(server.isServerOpened)
            while(isGamePlaying)
            {
                yield return new WaitForSeconds(0.5f);

                int between = (System.DateTime.Now - lastRotatedTime).Seconds;
                rotateTimeLeft = rotateTimeTerm - between;

                //Send time information to clients
                server.NoticeData(NetCommand.TimeCount, rotateTimeLeft.ToString());

                //Current role time out. Rotate role again.
                if (rotateTimeLeft < 0) DistributeRole();
            }
    }
}
