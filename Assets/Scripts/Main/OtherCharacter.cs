using UnityEngine;
using System.Collections;

public class OtherCharacter : MonoBehaviour {

    /// <summary>
    /// This character's player name
    /// </summary>
    public string playerName;

    /// <summary>
    /// This character's player identification string
    /// </summary>
    public string playerIdentification;

    /// <summary>
    /// This player's state
    /// </summary>
    PlayerState playerState;

    Client client;
    ClientInfo clientInfo;

    Animator m_animator;
    [SerializeField] GameObject zombie, human;

    [SerializeField] GameObject FlashObj;

    Vector3 oldPos;
    float oldVelX, oldVelZ;

	// Use this for initialization
	void Start () {
        //Get client
        client = GameObject.FindGameObjectWithTag("NetworkController").GetComponent<Client>();
        m_animator = zombie.GetComponent<Animator>();
        clientInfo = client.clients.Find(cc => cc.name.Equals(playerName) && cc.identification.Equals(playerIdentification));

    }

    // Update is called once per frame
    void Update () {
        //Get position/rotation from server
        if(clientInfo == null || !clientInfo.isConnected)
        {
            print(playerName + " disconnected");
            Destroy(this.gameObject);
            return;
        }
        try
        {
            //playerState
            if(playerState != clientInfo.userState)
            {
                playerState = clientInfo.userState;
                switch (playerState)
                {
                    case PlayerState.Human:
                        human.SetActive(true);
                        zombie.SetActive(false);
                        m_animator = human.GetComponent<Animator>();
                        break;
                    case PlayerState.Zombie:
                        zombie.SetActive(true);
                        human.SetActive(false);
                        m_animator = zombie.GetComponent<Animator>();
                        break;
                    case PlayerState.Dead:
                        //DEATH EFFECT
                        Destroy(this.gameObject);
                        return;
                        break;
                }
            }

            oldPos = gameObject.transform.position;
            gameObject.transform.position = clientInfo.userPosition;
            gameObject.transform.rotation = clientInfo.userRotation;

            //Set animation's variables by player's velocity, etc.
            m_animator.SetBool("OnGround", clientInfo.userIsOnGround);
            float velX, velZ;
            velX = Mathf.Lerp(oldVelX, -(oldPos.x - gameObject.transform.position.x), 0.8f);
            velZ = Mathf.Lerp(oldVelZ, -(oldPos.z - gameObject.transform.position.z), 0.8f);
            oldVelX = velX; oldVelZ = velZ;
            m_animator.SetFloat("Side", velX / Time.deltaTime);
            m_animator.SetFloat("Forward", velZ / Time.deltaTime);

            if (clientInfo.userIsAttack)
            {
                m_animator.SetTrigger("OnAttack");
                clientInfo.userIsAttack = false;
            }

            FlashObj.SetActive(clientInfo.userIsFlashOn);
        } catch (System.Exception e)
        {
            print(e.Message);
        }
    }
}
