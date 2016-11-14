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
    [SerializeField] TextMesh txtName;

    #region AudioClip resources
    [SerializeField] AudioClip[] audioFoot;
    [SerializeField] AudioClip audioJump;
    [SerializeField] AudioClip audioLand;
    [SerializeField] AudioClip audioFlash;
    [SerializeField] AudioClip audioWater;
    #endregion
    AudioSource audioSource;

    Vector3 oldPos;
    Quaternion oldRot;
    float oldVelX, oldVelZ;

    bool isFlashOn = false;

	// Use this for initialization
	void Start () {
        //Get client
        client = GameObject.FindGameObjectWithTag("NetworkController").GetComponent<Client>();
        m_animator = zombie.GetComponent<Animator>();
        clientInfo = client.clients.Find(cc => cc.name.Equals(playerName) && cc.identification.Equals(playerIdentification));

        //Player name
        txtName.text = playerName;

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
            oldRot = gameObject.transform.rotation;
            gameObject.transform.position = Vector3.Lerp(oldPos, clientInfo.userPosition, Time.deltaTime*5);
            gameObject.transform.rotation = Quaternion.Slerp(oldRot, clientInfo.userRotation, Time.deltaTime*5);

            //jumping/landing
            bool oldOnGround = m_animator.GetBool("OnGround");
            m_animator.SetBool("OnGround", clientInfo.userIsOnGround);
            if(oldOnGround != clientInfo.userIsOnGround)
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
                m_animator.SetTrigger("OnAttack");
                clientInfo.userIsAttack = false;
            }

            //Flash on/off
            if(clientInfo.userIsFlashOn != isFlashOn)
            {
                isFlashOn = clientInfo.userIsFlashOn;
                FlashObj.SetActive(isFlashOn);
                audioSource.PlayOneShot(audioFlash);
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
        while(true)
            if (clientInfo.userIsOnGround)
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
}
