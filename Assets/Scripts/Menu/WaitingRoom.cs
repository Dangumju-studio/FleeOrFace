using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Text;

public class WaitingRoom : MonoBehaviour {
    Client client;
    Server server;
    [SerializeField] Text txtChatMessage;
    [SerializeField] InputField txtChatInput;
    [SerializeField] Text btnReady;
    bool isReady = false;

    System.Collections.Generic.Queue<string> netChatQueue;

    // Use this for initialization
    void Start () {
        client = GameObject.FindGameObjectWithTag("NetworkController").GetComponent<Client>();
        server = GameObject.FindGameObjectWithTag("NetworkController").GetComponent<Server>();
        netChatQueue = new System.Collections.Generic.Queue<string>();
        client.ConnectToServer(client.hostIP);
        //set chat component in client.
        client.txtChatQueue = netChatQueue;
        StartCoroutine(DelayLoadRoom());
    }
	
	// Update is called once per frame
	void Update () {

        //Apply changed from queue
        while (netChatQueue.Count > 0)
        {
            StringBuilder sbChat = new StringBuilder();
            sbChat.Append(txtChatMessage.text);
            string nextMsg = netChatQueue.Dequeue();
            sbChat.AppendLine(nextMsg);
            //find newline character
            int cntNewline = 0;
            foreach (char c in sbChat.ToString())
                if (c == '\n') cntNewline += 1;
            if (cntNewline > 50) //Max chat line = 50
                                 //cut off old message
                sbChat.Remove(0, sbChat.ToString().IndexOf('\n'));
            txtChatMessage.text = sbChat.ToString();
        }

    
	}

    /// <summary>
    /// Delay to load room until server is available.
    /// </summary>
    /// <returns></returns>
    IEnumerator DelayLoadRoom()
    {
        while (!client.isConnected)
            yield return false;
        //Connection success. Load waiting lobby.
        txtChatInput.readOnly = false;
        txtChatMessage.text += "Connected!\n";
        StartCoroutine(client.sendCheck());
    }

    public void btnReady_Clicked()
    {
        isReady = !isReady;
        client.SendData(NetCommand.Ready, isReady.ToString());
        btnReady.text = isReady ? "Cancel" : "Ready";
    }

    public void txtChat_Edited()
    {
        if (txtChatInput.text.Length > 0)
        {
            client.SendData(NetCommand.Chat, txtChatInput.text);
            txtChatInput.text = "";
            txtChatInput.ActivateInputField();
        }
    }
}
