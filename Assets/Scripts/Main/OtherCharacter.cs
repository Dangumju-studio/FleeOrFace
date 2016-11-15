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
    public PlayerState playerState;

    Client client;
    ClientInfo clientInfo;

    Animator m_animator;
    [SerializeField] GameObject zombie, human;
    [SerializeField] GameObject FlashObj;
    [SerializeField] TextMesh txtName;

    #region AudioClip resources
    [SerializeField] AudioClip[] audioFoot;
    [SerializeField] AudioClip audioJump;
    [SerializeField] AudioClip audioLand;
    [SerializeField] AudioClip audioFlash;
    [SerializeField] AudioClip audioWaterIn;
    [SerializeField] AudioClip audioWaterOut;
    [SerializeField] AudioClip audioAttack;
    #endregion
    AudioSource audioSource;

    Vector3 oldPos;
    Quaternion oldRot;
    float oldVelX, oldVelZ;

    bool isFlashOn = false;
    bool isUnderwater = false;

	// Use this for initialization
	void Start () {
        //Get client
        client = GameObject.FindGameObjectWithTag("NetworkController").GetComponent<Client>();
        m_animator = zombie.GetComponent<Animator>();
        clientInfo = ClientInfo.findClientInfo(ref client.clientLists, playerIdentification);

        //disable ragdoll
        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>())
            rb.isKinematic = true;
        foreach (Collider cd in GetComponentsInChildren<Collider>())
            cd.enabled = false;

        //Player name
        txtName.text = playerName;
        print("Created: " + playerName + " - " + clientInfo.userState);

        //AudioSource
        audioSource = GetComponent<AudioSource>();

        //WalkingSound
        StartCoroutine(WalkingSound());
    }

    // Update is called once per frame
    void Update () {
        //Get position/rotation from server
        if(clientInfo == null || !clientInfo.isConnected)
        {
            print(playerName + " disconnected");
            Destroy(gameObject);
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
                        human.SetActive(true);
                        zombie.SetActive(false);
                        m_animator = human.GetComponent<Animator>();
                        m_animator.enabled = false;
                        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>())
                            rb.isKinematic = false;
                        foreach (Collider cd in GetComponentsInChildren<Collider>())
                            cd.enabled = true;
                        return;
                    case PlayerState.None:
                        //Destroy(gameObject);
                        return;
                }
            }

            if (playerState != PlayerState.Dead)
            {
                oldPos = gameObject.transform.position;
                oldRot = gameObject.transform.rotation;

                gameObject.transform.position = Vector3.Lerp(oldPos, clientInfo.userPosition, Time.deltaTime * 5);
                gameObject.transform.rotation = Quaternion.Slerp(oldRot, clientInfo.userRotation, Time.deltaTime * 5);

                //jumping/landing
                bool oldOnGround = m_animator.GetBool("OnGround");
                m_animator.SetBool("OnGround", clientInfo.userIsOnGround);
                if (oldOnGround != clientInfo.userIsOnGround)
                {
                    if (clientInfo.userIsOnGround) audioSource.PlayOneShot(audioLand);
                    else audioSource.PlayOneShot(audioJump);
                }

                //Set animation's variables by player's velocity, etc.
                float velX, velZ;
                velX = Mathf.Lerp(oldVelX, -(oldPos.x - gameObject.transform.position.x), 0.8f);
                velZ = Mathf.Lerp(oldVelZ, -(oldPos.z - gameObject.transform.position.z), 0.8f);
                oldVelX = velX; oldVelZ = velZ;
                m_animator.SetFloat("Side", velX / Time.deltaTime);
                m_animator.SetFloat("Forward", velZ / Time.deltaTime);

                if (clientInfo.userIsAttack)
                {
                    if (playerState == PlayerState.Zombie)
                    {
                        m_animator.SetTrigger("OnAttack");
                        audioSource.PlayOneShot(audioAttack);
                    }
                    clientInfo.userIsAttack = false;
                }

                //Flash on/off
                if (clientInfo.userIsFlashOn != isFlashOn)
                {
                    isFlashOn = clientInfo.userIsFlashOn;
                    FlashObj.SetActive(isFlashOn);
                    audioSource.PlayOneShot(audioFlash);
                }
            }
            //Player name rotation
            txtName.gameObject.transform.LookAt(Camera.main.transform);

        } catch (System.Exception e)
        {
            print(e.Message);
        }
    }

    /// <summary>
    /// Play walking footstep sound periodically while walking.
    /// </summary>
    /// <returns></returns>
    IEnumerator WalkingSound()
    {
        while(playerState == PlayerState.Zombie || playerState == PlayerState.Human)
            if (clientInfo.userIsOnGround && !isUnderwater)
            {
                float vX = Mathf.Abs(oldVelX), vZ = Mathf.Abs(oldVelZ);
                if (vX > 8 || vZ > 8)
                {
                    int audioNum = Random.Range(0, audioFoot.Length);
                    audioSource.PlayOneShot(audioFoot[audioNum]);
                    yield return new WaitForSeconds(0.2f);
                }
                else if (vX > 1 || vZ > 1)
                {
                    int audioNum = Random.Range(0, audioFoot.Length);
                    audioSource.PlayOneShot(audioFoot[audioNum]);
                    yield return new WaitForSeconds(0.4f);
                }
                else yield return null;
            }
            else yield return null;
    }

    void OnDestroy()
    {
        StopCoroutine(WalkingSound());
    }

    /// <summary>
    /// Water enter sound play method. Called from map water manager.
    /// </summary>
    public void OnWaterEnter()
    {
        isUnderwater = true;
        audioSource.PlayOneShot(audioWaterIn);
    }
    /// <summary>
    /// Water out sound play method. Called from map water manager.
    /// </summary>
    public void OnWaterLeave()
    {
        isUnderwater = false;
        audioSource.PlayOneShot(audioWaterOut);
    }
}
