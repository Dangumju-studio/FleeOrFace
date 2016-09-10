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
        Connect,    //Similar to SYN
        Disconnect,
        Check,      //Similar to ACK
        Chat,
        Ready,
        Position,
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
            int nameLen = BitConverter.ToInt32(data, 2);    //
            int msgLen = BitConverter.ToInt32(data, 4);     //

            //get name
            if (nameLen > 0)
                name = Encoding.UTF8.GetString(data, 6, nameLen);
            else
                name = string.Empty;

            //get message
            if (msgLen > 0)
                msg = Encoding.UTF8.GetString(data, 3 + nameLen, msgLen);
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

}
