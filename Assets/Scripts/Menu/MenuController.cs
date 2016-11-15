using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour {

    [SerializeField] GameObject pnTypeName;
    [SerializeField] Text txtName;

    [SerializeField] GameObject pnSelectHostMode;
    [SerializeField] GameObject pnTypeHostIP;
    [SerializeField] Text txtHostIP;
    Client client;
    Server server;

    // Use this for initialization
    void Start () {
        //Assign client and server
        client = GameObject.FindGameObjectWithTag("NetworkController").GetComponent<Client>();
        server = GameObject.FindGameObjectWithTag("NetworkController").GetComponent<Server>();

        Application.runInBackground = true;
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    //Type name & Generate identification string randomly
    public void btnName() {
        if (txtName.text.Length > 0)
        {
            client.playerName = txtName.text;
            //remove non-alphanumeric characters
            System.Text.RegularExpressions.Regex rgx = new System.Text.RegularExpressions.Regex("[,:\"'/\\()]");
            client.playerName = rgx.Replace(client.playerName, "");
            //Generate identification string
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for(int i=0; i<8; i++)
            {
                char[] cRandom = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
                sb.Append(cRandom[Random.Range(0, cRandom.Length)]);    
            }
            client.identification = sb.ToString();
            //Move next
            pnTypeName.SetActive(false);
            pnSelectHostMode.SetActive(true);
        }
    }

    //Select host mode
    public void btnBeHost()
    {
        server.StartServer();
        client.hostIP = "127.0.0.1";
        SceneManager.LoadScene("room");
    }
    public void btnConnectHost()
    {
        pnSelectHostMode.SetActive(false);
        pnTypeHostIP.SetActive(true);
    }
    public void btnHelp()
    {
       
    }

    //Type IP
    public void btnIP()
    {
        if (txtHostIP.text.Length > 0)
        {
            System.Net.IPAddress tmp;
            if (System.Net.IPAddress.TryParse(txtHostIP.text, out tmp))
            {
                client.hostIP = txtHostIP.text;
                SceneManager.LoadScene("room");
            }
        }
    }
    
}
