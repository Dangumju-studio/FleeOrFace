using UnityEngine;
using System.Collections;
using System.Text;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

    public bool DEBUGGING_MODE = false;

    public bool isThirdPersonCamera = false;

    [SerializeField] private GameObject m_PlayerCamera;
    [SerializeField] private UnityStandardAssets.Characters.FirstPerson.FirstPersonController m_fpsCtrl;

    #region player modeling objects
    [SerializeField] GameObject zombieObj, zombieFPSObj;
    [SerializeField] GameObject humanObj, humanFPSObj;
    #endregion

    #region Canvas Gameobjects
    [SerializeField] private Image m_imgLoading;
    [SerializeField] private GameObject m_pnLoadingBG;
    [SerializeField] private GameObject m_pnIngameUI;
    [SerializeField] private GameObject m_pnESCMenu;
    [SerializeField] private GameObject m_pnBackToMain;
    [SerializeField] private GameObject m_pnExitToDesktop;

    [SerializeField] private Text txtPlayerStatus;
    [SerializeField] private Text txtRotateLeftTime;
    #endregion

    Client client;
    IngameManager gameManager;
    PlayerState playerState;
    
    /// <summary>
    /// Other player's object prefab
    /// </summary>
    [SerializeField] GameObject playerCharPrefab;

    /// <summary>
    /// Flash on/off
    /// </summary>
    bool isFlashOn = false;
    [SerializeField] GameObject FlashObj;

    //StringBuilder for Position and Rotation
    StringBuilder sbPosRot = new StringBuilder();

    //Game start?
    bool isGameStart = false;

    //is Pause?
    public bool isPause = false;

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
            OVRManager.instance.gameObject.GetComponent<OVRCameraRig>().enabled = false;
            OVRManager.instance.enabled = false;

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

        //Hide all menu
        m_pnESCMenu.SetActive(false);
        m_pnBackToMain.SetActive(false);
        m_pnExitToDesktop.SetActive(false);

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

    /// <summary>
    /// Wait while attack motion ends
    /// </summary>
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

        //Update role rotation time, player's role
        if(client != null) { 
            txtRotateLeftTime.text = client.rotateTimeLeft.ToString();
            if (client.userState != playerState)
            {
                playerState = client.userState;
                switch (playerState)
                {
                    case PlayerState.Human:
                        humanObj.SetActive(true);
                        humanFPSObj.SetActive(true);
                        zombieObj.SetActive(false);
                        zombieFPSObj.SetActive(false);
                        m_fpsCtrl.m_Animator = humanObj.GetComponent<Animator>();
                        m_fpsCtrl.m_Animator_fps = humanFPSObj.GetComponent<Animator>();
                        txtPlayerStatus.text = "You are the human..";
                        break;
                    case PlayerState.Zombie:
                        zombieObj.SetActive(true);
                        zombieFPSObj.SetActive(true);
                        humanObj.SetActive(false);
                        humanFPSObj.SetActive(false);
                        m_fpsCtrl.m_Animator = zombieObj.GetComponent<Animator>();
                        m_fpsCtrl.m_Animator_fps = zombieFPSObj.GetComponent<Animator>();
                        txtPlayerStatus.text = "You are the zombie..";
                        break;
                    case PlayerState.Dead:
                        //DEATH EFFECT

                        //FREE CAM MODE
                        break;
                }
            }
        }

        //Update attack value
        if(CrossPlatformInputManager.GetButtonDown("Attack")) {
            
            if (attackAvailable)
            {
                if(client != null) client.attack = true;
                attackAvailable = false;
                StartCoroutine(AttackEnd());
            }
        }

        //Flash Control
        if(CrossPlatformInputManager.GetButtonDown("Flash"))
        {
            isFlashOn = !isFlashOn;
            FlashObj.SetActive(isFlashOn);
        }

        //ESC Menu Control
        if(CrossPlatformInputManager.GetButtonDown("Cancel"))
        {
            if(!isPause)
            {
                isPause = true;
                m_fpsCtrl.m_isPause = isPause;
                m_pnESCMenu.SetActive(true);
                m_pnBackToMain.SetActive(false);
                m_pnExitToDesktop.SetActive(false);
                Cursor.visible = true;
            }
        }
    }
    /// <summary>
    /// Wait until Attack animation end
    /// </summary>
    IEnumerator AttackEnd()
    {
        yield return new WaitForSeconds(0.3f);
        attackAvailable = true;
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

                    //role rotation timer start!!
                    
                    StartCoroutine(gameManager.RotatePlayerRoleTimer());
                    print("Game start");
                }

                //Send player's control
                if (client != null) client.SendPlayerControl();
                
                //Get informations from client instance -> processed in "OtherCharacter" class component of each players' gameobject.
            }
    }

    #region ESC Menu buttons
    public void btnResume_Clicked()
    {
        m_pnESCMenu.SetActive(false);
        isPause = false;
        m_fpsCtrl.m_isPause = isPause;
        Cursor.visible = false;
    }
    public void btnBackToMain_Clicked()
    {
        m_pnESCMenu.SetActive(false);
        m_pnBackToMain.SetActive(true);
    }
    public void btnExitToDesktop_Clicked()
    {
        m_pnESCMenu.SetActive(false);
        m_pnExitToDesktop.SetActive(true);
    }
    public void btnBackToMainYes_Clicked()
    {
        client.SendData(NetCommand.Disconnect, "");
        UnityEngine.SceneManagement.SceneManager.LoadScene("menu");
    }
    public void btnExitToDesktopYes_Clicked()
    {
        client.SendData(NetCommand.Disconnect, "");
        Application.Quit();
    }
    public void btnCancel_Clicked()
    {
        m_pnBackToMain.SetActive(false);
        m_pnExitToDesktop.SetActive(false);
        m_pnESCMenu.SetActive(true);
    }
    #endregion

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            if (!isPause)
            {
                isPause = true;
                m_fpsCtrl.m_isPause = isPause;
                m_pnESCMenu.SetActive(true);
                m_pnBackToMain.SetActive(false);
                m_pnExitToDesktop.SetActive(false);
                Cursor.visible = true;
            }
        }
    }
}