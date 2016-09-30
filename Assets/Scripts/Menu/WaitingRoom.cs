using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class WaitingRoom : MonoBehaviour {
    Client client;
    Server server;
    [SerializeField] Text txtChatMessage;
    [SerializeField] InputField txtChatInput;
    [SerializeField] Text btnReady;
    bool isReady = false;

    // Use this for initialization
    void Start () {
        client = GameObject.FindGameObjectWithTag("NetworkController").GetComponent<Client>();
        server = GameObject.FindGameObjectWithTag("NetworkController").GetComponent<Server>();

        //set chat component in client.
        client.txtChat = txtChatMessage;
        StartCoroutine(DelayLoadRoom());
    }
	
	// Update is called once per frame
	void Update () {
	
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
    }

    public void btnReady_Clicked()
    {
        isReady = !isReady;
        client.SendData(NetCommand.Ready, isReady.ToString());
        btnReady.text = isReady ? "Cancel" : "Ready";
    }
}
