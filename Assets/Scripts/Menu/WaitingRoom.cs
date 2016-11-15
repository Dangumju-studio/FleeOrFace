using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Text;
using System.Collections.Generic;

public class WaitingRoom : MonoBehaviour {
    Client client;
    //Server server;
    IngameManager gameManager;
    [SerializeField] Text myPlayerName;
    [SerializeField] Text txtChatMessage;
    [SerializeField] ScrollRect scrChat;
    [SerializeField] InputField txtChatInput;
    [SerializeField] Text btnReady;
    bool isReady = false;
    bool isLoadingGame = false; //if true, main scene is loading now.

    //Player Object panel list
    List<PlayerInfo> playerInfoList;
    [SerializeField] GameObject playerInfoPrefab;
    [SerializeField] Transform playerInfoWrapper;

    //Map setting objects
    [SerializeField] Dropdown lstMap;
    [SerializeField] Toggle chk3rdcam;

    // Use this for initialization
    void Start () {
        client = GameObject.FindGameObjectWithTag("NetworkController").GetComponent<Client>();
        gameManager = GameObject.FindGameObjectWithTag("NetworkController").GetComponent<IngameManager>();

        //load maplist
        foreach (string s in gameManager.mapList)
        lstMap.options.Add(new Dropdown.OptionData(s));
        lstMap.value = 0;
        lstMap.RefreshShownValue();

        if(GameObject.FindGameObjectWithTag("NetworkController").GetComponent<Server>().isServerOpened)
        {
            lstMap.interactable = true;
            chk3rdcam.interactable = true;
        }


        playerInfoList = new List<PlayerInfo>();
        myPlayerName.text = client.playerName;

        client.ConnectToServer(client.hostIP);
        
        //load room
        StartCoroutine(DelayLoadRoom());
    }
	
	// Update is called once per frame
	void Update () {

        //Apply text(chat) changed from queue
        while (client.txtChatQueue.Count > 0)
        {
            bool isBottom = false;
            if (scrChat.verticalNormalizedPosition == 0) isBottom = true;
            StringBuilder sbChat = new StringBuilder();
            sbChat.Append(txtChatMessage.text);
            string nextMsg = client.txtChatQueue.Dequeue();
            sbChat.AppendLine(nextMsg);
            //find newline character
            int cntNewline = 0;
            foreach (char c in sbChat.ToString())
                if (c == '\n') cntNewline += 1;
            if (cntNewline > 50) //Max chat line = 50
                                 //cut off old message
                sbChat.Remove(0, sbChat.ToString().IndexOf('\n'));
            txtChatMessage.text = sbChat.ToString();
            txtChatMessage.gameObject.transform.parent.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(0, (cntNewline+1) * 17);
            if (isBottom) { scrChat.verticalNormalizedPosition = 0; }
        }

        //update player list ref client list
        //Add player into playerlist
        foreach(ClientInfo ci in client.clientLists)
        {
            if (ci.name == client.playerName && ci.identification == client.identification) continue;
            PlayerInfo pi = playerInfoList.Find(p => p.identification.Equals(ci.identification));
            if(pi == null)
            {
                GameObject piObj = Instantiate(playerInfoPrefab, playerInfoWrapper) as GameObject;
                pi = piObj.GetComponent<PlayerInfo>();
                pi.playerName = ci.name;
                pi.identification = ci.identification;
                playerInfoList.Add(pi);
            }
        }
        //Remove player or Modify player status from playerlist
        for(int i=0; i<playerInfoList.Count; i++)
        {
            ClientInfo ci = client.clientLists.Find(c => c.identification.Equals(playerInfoList[i].identification));
            //remove
            if(ci == null)
            {
                Destroy(playerInfoList[i].gameObject);
                playerInfoList.Remove(playerInfoList[i]);
                i -= 1; continue;
            }
            //modify
            //playerInfoList[i].gameObject.GetComponent<RectTransform>(). = new Vector2(0, i * 50);
            playerInfoList[i].gameObject.GetComponent<RectTransform>().localPosition = new Vector2(0, -i* 50);
            playerInfoList[i].gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 50);

            playerInfoList[i].isReady = ci.isReady;
        }
        playerInfoWrapper.GetComponent<RectTransform>().offsetMax = new Vector2(0, playerInfoList.Count);

        //Map setting update
        lstMap.value = gameManager.mapNumber;
        chk3rdcam.isOn = gameManager.is3rdCam;
	}

    void FixedUpdate()
    {
        //Start game
        if (client.isLoadingStarting && !isLoadingGame)
        {
            StopCoroutine(client.SendCheck());  //While scene switching, coroutine will not work. (Unity system)
            isLoadingGame = true;
            
            UnityEngine.SceneManagement.SceneManager.LoadScene("main");
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
        StartCoroutine(client.SendCheck());
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
            scrChat.verticalNormalizedPosition = 0;
            txtChatInput.ActivateInputField();
        }
    }

    public void MapSetting_Changed()
    {
        gameManager.mapNumber = lstMap.value;
        gameManager.is3rdCam = chk3rdcam.isOn;
        int mapNum = lstMap.value;
        bool is3rd = chk3rdcam.isOn;
        client.SendData(NetCommand.MapSetting, string.Format("{0},{1}", mapNum, is3rd));
    }
}
