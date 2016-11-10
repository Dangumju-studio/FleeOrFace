using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System;
using System.Text;
using System.Collections.Generic;

public enum NetCommand
    {
        Null,
        /// <summary>
        /// Connect request message (SYN)
        /// </summary>
        Connect,
        /// <summary>
        /// Disconnect request message
        /// </summary>
        Disconnect,
        /// <summary>
        /// Server connection check message (ACK), Get ClientList
        /// </summary>
        Check,      
        /// <summary>
        /// Chat message
        /// </summary>
        Chat,
        /// <summary>
        /// Map setting. Received every 1s in lobby(waiting room)
        /// Format: MapNumber,3rdCam
        /// </summary>
        MapSetting,
        /// <summary>
        /// Ready message in player waiting room
        /// </summary>
        Ready,
        /// <summary>
        /// Load game message. Occured when all players are ready.
        /// </summary>
        LoadGame,
        /// <summary>
        /// Start Game message. Occured when all players' game loading is done.
        /// </summary>
        StartGame,
        /// <summary>
        /// Player's position/rotation data
        /// </summary>
        PositionRotation,
        /// <summary>
        /// Some player's attack flag data
        /// </summary>
        Attack,
        /// <summary>
        /// Role rotation time left count changed message
        /// </summary>
        TimeCount,
        /// <summary>
        /// When role rotated.
        /// </summary>
        RoleRotate,
        /// <summary>
        /// When game over.
        /// </summary>
        Gameover
    }

public enum PlayerState { Dead, Zombie, Human, None}

/// <summary>
/// Data class
/// </summary>
public class NetworkData
{
    public NetCommand cmd;
    public string msg;
    public string name;
    /// <summary>
    /// Endpoint for identify
    /// </summary>
    public string identify;

    public NetworkData()
    {
        cmd = NetCommand.Null;
        msg = string.Empty;
        name = string.Empty;
        identify = string.Empty;
    }
    /// <summary>
    /// Initialize NetworkData from byte array
    /// </summary>
    /// <param name="data">Byte array data</param>
    public NetworkData(byte[] data)
    {
        cmd = (NetCommand)BitConverter.ToInt32(data, 0); 
        int nameLen = BitConverter.ToInt32(data, 4);
        int identifyLen = BitConverter.ToInt32(data, 8);
        int msgLen = BitConverter.ToInt32(data, 12);
        
        //get name (convert from base64 to restore utf8 string)
        name = Encoding.UTF8.GetString(Convert.FromBase64String(Encoding.UTF8.GetString(data, 16, nameLen)));

        //get identification
        identify = Encoding.UTF8.GetString(data, 16 + nameLen, identifyLen);

        //get message
        if (msgLen > 0)
            msg = Encoding.UTF8.GetString(Convert.FromBase64String(Encoding.UTF8.GetString(data, 16 + nameLen + identifyLen, msgLen)));
        else
            msg = string.Empty;
    }

    /// <summary>
    /// Convert Data Object to bytes array.
    /// </summary>
    /// <returns>Converted Data</returns>
    public byte[] ConvertToByte()
    {
        List<byte> res = new List<byte>();
        //Convert utf8 string(name, msg) to base64.
        name = Convert.ToBase64String(Encoding.UTF8.GetBytes(name));
        msg = Convert.ToBase64String(Encoding.UTF8.GetBytes(msg));

        //0 : command
        res.AddRange(BitConverter.GetBytes((int)cmd));

        //1 : name length
        res.AddRange(BitConverter.GetBytes(name.Length));

        //2 : identification length
        res.AddRange(BitConverter.GetBytes(identify.Length));

        //3, 4 : msg length
        res.AddRange(BitConverter.GetBytes(msg.Length));

        //5~ : name, identification, message
        res.AddRange(Encoding.UTF8.GetBytes(name));
        res.AddRange(Encoding.UTF8.GetBytes(identify));
        if(msg != null) res.AddRange(Encoding.UTF8.GetBytes(msg));

        //return bytes array
        return res.ToArray();
    }
} 