using UnityEngine;
using System.Collections;

public class OtherCharacter : MonoBehaviour {

    public string playerName;
    public string playerIdentification;
    PlayerState playerState;
    Client client;

	// Use this for initialization
	void Start () {
        //Get client
        client = GameObject.FindGameObjectWithTag("NetworkController").GetComponent<Client>();
    }
	
	// Update is called once per frame
	void Update () {
        //Get position/rotation from server
        ClientInfo c = client.clients.Find(cc => cc.name.Equals(playerName) && cc.identification.Equals(playerIdentification));
        playerState = c.userState;
        gameObject.transform.position = c.userPosition;
        gameObject.transform.rotation = c.userRotation;
	}
}
