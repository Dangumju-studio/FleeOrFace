﻿using UnityEngine;
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
        /// Server connection check message (ACK)
        /// </summary>
        Check,      
        /// <summary>
        /// Chat message
        /// </summary>
        Chat,
        /// <summary>
        /// Ready message in player waiting room
        /// </summary>
        Ready,
        /// <summary>
        /// Every player's position data
        /// </summary>
        Position,
        /// <summary>
        /// Some player's attack flag data
        /// </summary>
        Attack
    }

public class Network : MonoBehaviour {
    
    /// <summary>
    /// Data class
    /// </summary>
    public class Data
    {
        public NetCommand cmd;
        public string msg;
        public string name;

        public Data()
        {
            cmd = NetCommand.Null;
            msg = string.Empty;
            name = string.Empty;
        }
        public Data(byte[] data)
        {
            cmd = (NetCommand)BitConverter.ToInt32(data, 0); //
            int nameLen = BitConverter.ToInt32(data, 4);    //
            int msgLen = BitConverter.ToInt32(data, 8);     //

            //get name
            if (nameLen > 0)
                name = Encoding.UTF8.GetString(data, 12, nameLen);
            else
                name = string.Empty;

            //get message
            if (msgLen > 0)
                msg = Encoding.UTF8.GetString(data, 12 + nameLen, msgLen);
            else
                msg = string.Empty;
        }

        /// <summary>
        /// Convert Data Object to bytes array.
        /// </summary>
        /// <returns></returns>
        public byte[] ConvertToByte()
        {
            List<byte> res = new List<byte>();

            //0 : command
            res.AddRange(BitConverter.GetBytes((short)cmd));

            //1 : name length
            res.AddRange(BitConverter.GetBytes((short)name.Length));

            //2, 3 : msg length
            res.AddRange(BitConverter.GetBytes((short)msg.Length));

            //4~ : name, message
            res.AddRange(Encoding.UTF8.GetBytes(name));
            res.AddRange(Encoding.UTF8.GetBytes(msg));

            //return bytes array
            return res.ToArray();
        }
    }

    public void Start()
    {

        //Make this object immortal
        DontDestroyOnLoad(this);
    }

}
