using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Client Information class
/// </summary>
public class ClientInfo
{
	/// <summary>
    /// End Point
    /// </summary>
    public System.Net.EndPoint ep;
    /// <summary>
    /// Client Name
    /// </summary>
    public string name;
    /// <summary>
    /// Identification string
    /// </summary>
    public string identification;
    /// <summary>
    /// Last client connection checked time
    /// </summary>
    public DateTime lastCheck;

    /// <summary>
    /// Ready state
    /// </summary>
    public bool isReady;

    /// <summary>
    /// User character position
    /// </summary>
    public Vector3 userPosition;
    /// <summary>
    /// User character rotation
    /// </summary>
    public Quaternion userRotation;
    /// <summary>
    /// Player state (dead, zombie, human, none)
    /// </summary>
    public PlayerState userState;

    /// <summary>
    /// Client Information Class
    /// </summary>
    public ClientInfo()
    {
        //initialize client information
        ep = null;
        name = "";
        lastCheck = DateTime.Now;
        isReady = false;
        userPosition = Vector3.zero;
        userRotation = Quaternion.identity;
        userState = PlayerState.None;
    }
    public ClientInfo(string name, string identification, bool isReady = false)
    {
        //initialize client information
        ep = null;
        this.name = name;
        this.identification = identification;
        lastCheck = DateTime.Now;
        this.isReady = isReady;
        userPosition = Vector3.zero;
        userRotation = Quaternion.identity;
        userState = PlayerState.None;
    }
}
