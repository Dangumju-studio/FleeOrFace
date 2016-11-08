using UnityEngine;
using System.Collections;
using System.Text;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

    public bool DEBUGGING_MODE = false;

    public bool isThirdPersonCamera = false;

    [SerializeField] private Camera m_PlayerCamera;
    [SerializeField] private UnityStandardAssets.Characters.FirstPerson.FirstPersonController m_fpsCtrl;

    #region Canvas Gameobjects
    [SerializeField] private Image m_imgLoading;
    [SerializeField] private GameObject m_pnLoadingBG;
    [SerializeField] private GameObject m_pnIngameUI;
    #endregion

    Client client;
    IngameManager gameManager;
    
    [SerializeField] GameObject playerCharPrefab;
    bool isFlashOn = false;
    [SerializeField] GameObject FlashObj;

    //StringBuilder for Position and Rotation
    StringBuilder sbPosRot = new StringBuilder();

    bool isGameStart = false;

    // Use this for initialization
    void Start () {

        //Get client
        if (!DEBUGGING_MODE)
        {
            client = GameObject.FindGameObjectWithTag("NetworkController").GetComponent<Client>();
            gameManager = GameObject.FindGameObjectWithTag("NetworkController").GetComponent<IngameManager>();
            isThirdPersonCamera = gameManager.is3rdCam;
        }
        //Set camera mode(FPS or TPS)
        if (isThirdPersonCamera)
        {
            m_fpsCtrl.m_UseHeadBob = false;
            m_PlayerCamera.transform.localPosition = new Vector3(0, 2, -2.5f);
            m_PlayerCamera.transform.localRotation = Quaternion.Euler(30, 0, 0);

        }
        else {
            if(!OVRManager.isHmdPresent)
            {
                OVRManager.instance.gameObject.GetComponent<OVRCameraRig>().enabled = false;
                OVRManager.instance.enabled = false;
            }
        }

        //Start check message sending corouting
        if(!DEBUGGING_MODE) StartCoroutine(client.SendCheck());

        //Load Map (Game Scene)
        if (DEBUGGING_MODE)
            StartCoroutine(LoadGameScene("Zombie1"));
        else
            StartCoroutine(LoadGameScene(gameManager.mapList[gameManager.mapNumber]));

        //Load other players
        if(!DEBUGGING_MODE)
            foreach (ClientInfo ci in client.clients)
            {
                if (ci.name == client.playerName) continue;
                GameObject newPlayer = Instantiate(playerCharPrefab);
                OtherCharacter oc = newPlayer.GetComponent<OtherCharacter>();
                oc.playerName = ci.name;
                oc.playerIdentification = ci.identification;
            }
    }
    /// <summary>
    /// Game scene(map) load method (Coroutine)
    /// </summary>
    /// <returns></returns>
    IEnumerator LoadGameScene(string mapName)
    {
        print("Map load start");
        var loadState = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(mapName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
        while (!loadState.isDone)
        {
            m_imgLoading.fillAmount = loadState.progress;
            print("Map loading.." + loadState.progress);
            yield return null;
        }
        print("Map loaded");

        m_imgLoading.fillAmount = 1;

        yield return new WaitForSeconds(2f);

        //Send 'Loading done' message to server
        if(DEBUGGING_MODE)
        {
            isGameStart = true;
            m_pnLoadingBG.SetActive(false);
            m_pnIngameUI.SetActive(true);
            m_fpsCtrl.enabled = true;
            m_fpsCtrl.gameObject.GetComponent<Rigidbody>().useGravity = true;
            m_fpsCtrl.transform.position = new Vector3(m_fpsCtrl.transform.position.x, 10, m_fpsCtrl.transform.position.z);
            print("Game start");
        }
        else client.SendData(NetCommand.LoadGame, "True");
    }


    bool attackAvailable = true;
	// Update is called once per frame
	void Update () {

        //Update position and rotation value
        //Clear stringbuilder
        sbPosRot.Length = 0;
        //Append player's position and rotation value to stringbuilder
        Transform playerT = m_fpsCtrl.gameObject.transform;
        sbPosRot.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8}",    //Position(x, y, z), Rotation(x, y, z, w -> Quaternion), OnGround, Flash
            playerT.position.x, playerT.position.y, playerT.position.z,
            playerT.rotation.x, playerT.rotation.y, playerT.rotation.z, playerT.rotation.w,
            m_fpsCtrl.m_Animator.GetBool("OnGround"), isFlashOn);
        //Push position and rotation value
        if(client != null) client.positionRotation = sbPosRot.ToString();

        //Update attack value
        float attack = CrossPlatformInputManager.GetAxis("Fire1");
        if(attack > 0) {
            if (attackAvailable)
            {
                if(client != null) client.attack = true;
                attackAvailable = false;
            }
        }
        if (attack <= 0) { attackAvailable = true; if (client != null) client.attack = false; }

        }

    //FixedUpdate
    void FixedUpdate()
    {
        if(!DEBUGGING_MODE)
            if (client.isGamePlaying)
            {
                //Start Game!!
                if (!isGameStart)
                {
                    isGameStart = true;
                    m_pnLoadingBG.SetActive(false);
                    m_pnIngameUI.SetActive(true);
                    m_fpsCtrl.enabled = true;
                    m_fpsCtrl.gameObject.GetComponent<Rigidbody>().useGravity = true;
                    m_fpsCtrl.transform.position = new Vector3(m_fpsCtrl.transform.position.x, 10, m_fpsCtrl.transform.position.z);
                    print("Game start");
                }

                //Send player's control
                if (client != null) client.SendPlayerControl();
                //Get informations from client instance
                ///////////
            }
    }
}
