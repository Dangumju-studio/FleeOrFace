﻿using UnityEngine;
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
    /// Game scene loading state
    /// </summary>
    public bool isLoadingDone;

    /// <summary>
    /// User character position
    /// </summary>
    public Vector3 userPosition;
    /// <summary>
    /// User character rotation
    /// </summary>
    public Quaternion userRotation;
    /// <summary>
    /// User character onGround
    /// </summary>
    public bool userIsOnGround;
    /// <summary>
    /// User flash state
    /// </summary>
    public bool userIsFlashOn;
    /// <summary>
    /// User Attack. (When this true, this will change to false immediately and Character will attack.)
    /// </summary>
    public bool userIsAttack;
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
        isLoadingDone = false;
        userPosition = Vector3.zero;
        userRotation = Quaternion.identity;
        userIsOnGround = true;
        userIsFlashOn = false;
        userIsAttack = false;
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
        isLoadingDone = false;
        userPosition = Vector3.zero;
        userRotation = Quaternion.identity;
        userIsOnGround = true;
        userIsFlashOn = false;
        userIsAttack = false;
        userState = PlayerState.None;
    }
}
