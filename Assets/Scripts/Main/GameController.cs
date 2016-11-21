using UnityEngine;
using System.Collections;
using System.Text;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.UI;
using System;

public class GameController : MonoBehaviour {

    public bool DEBUGGING_MODE = false;

    public bool isThirdPersonCamera = false;

    [SerializeField] private GameObject m_PlayerCamera;
    [SerializeField] private UnityStandardAssets.Characters.FirstPerson.FirstPersonController m_fpsCtrl;

    #region player modeling objects
    [SerializeField] GameObject zombieObj, zombieFPSObj;
    [SerializeField] SkinnedMeshRenderer zombieObjRenderer, zombieFPSObjRenderer;
    [SerializeField] GameObject humanObj, humanFPSObj;
    [SerializeField] SkinnedMeshRenderer humanObjRenderer, humanFPSObjRenderer;
    [SerializeField] GameObject playerRagdollObj;
    #endregion

    #region Canvas Gameobjects
    [SerializeField] private Image m_imgLoading;
    [SerializeField] private GameObject m_pnLoadingBG;
    [SerializeField] private GameObject m_pnIngameUI;
    [SerializeField] private GameObject m_pnESCMenu;
    [SerializeField] private GameObject m_pnBackToMain;
    [SerializeField] private GameObject m_pnExitToDesktop;
    [SerializeField] private GameObject m_pnGameOver;

    [SerializeField] private Text txtPlayerStatus;
    [SerializeField] private Text txtRotateLeftTime;
    [SerializeField] private Text txtWinner;

    //VR Canvas
    [SerializeField] private GameObject m_pnVRCanvas;
    [SerializeField] private Image m_imgVRLoading;
    [SerializeField] private GameObject m_pnVRLoadingBG;
    [SerializeField] private GameObject m_pnVRIngameUI;
    [SerializeField] private GameObject m_pnVRESCMenu;
    [SerializeField] private GameObject m_pnVRGameOver;
    [SerializeField] private Text txtVRRotateLeftTime;
    [SerializeField] private Text txtVRPlayerStatus;
    [SerializeField] private Text txtVRWinner;
    #endregion

    Client client;
    Server server;
    IngameManager gameManager;
    public PlayerState playerState;

    #region audio clip/sources
    [SerializeField] AudioClip audioThunder;
    [SerializeField] AudioClip audioFlash;
    [SerializeField] AudioClip audioAttack;
    AudioSource sceneAudio;
    #endregion

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

    //Game end?
    bool isGameOver = false;

    //is Paused?
    public bool isPause = false;

    // Use this for initialization
    void Start () {
        //Hide all menu ui
        m_pnESCMenu.SetActive(false);
        m_pnVRESCMenu.SetActive(false);
        m_pnGameOver.SetActive(false);
        m_pnVRGameOver.SetActive(false);
        m_pnBackToMain.SetActive(false);
        m_pnExitToDesktop.SetActive(false);

        sceneAudio = GetComponent<AudioSource>();

        //Get client
        if (!DEBUGGING_MODE)
        {
            client = GameObject.FindGameObjectWithTag("NetworkController").GetComponent<Client>();
            server = GameObject.FindGameObjectWithTag("NetworkController").GetComponent<Server>();
            client.userState = PlayerState.None;
            gameManager = GameObject.FindGameObjectWithTag("NetworkController").GetComponent<IngameManager>();
            isThirdPersonCamera = gameManager.is3rdCam;
        }
        //Set camera mode(FPS or TPS)
        if (isThirdPersonCamera)
        {
            m_fpsCtrl.m_UseHeadBob = false;
            m_PlayerCamera.transform.localPosition = new Vector3(0, 2, -2.5f);
            m_PlayerCamera.transform.localRotation = Quaternion.Euler(30, 0, 0);
            //TPS -> Not using HMD
            OVRManager.instance.gameObject.GetComponent<OVRCameraRig>().enabled = false;
            OVRManager.instance.enabled = false;
            m_pnVRCanvas.SetActive(false);
            //TPS modeling
            zombieFPSObjRenderer.enabled = false;
            humanFPSObjRenderer.enabled = false;
            zombieObjRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            humanObjRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }
        else {
            //Not using HMD
            if (!OVRManager.isHmdPresent)
            {
                OVRManager.instance.gameObject.GetComponent<OVRCameraRig>().enabled = false;
                OVRManager.instance.enabled = false;
                m_pnVRCanvas.SetActive(false);
            }
        }

        m_imgLoading.fillAmount = 0;
        m_imgVRLoading.fillAmount = 0;

        //Start check message sending corouting
        if (!DEBUGGING_MODE) StartCoroutine(client.SendCheck());

        //Load Map (Game Scene)
        if (DEBUGGING_MODE)
            StartCoroutine(LoadGameScene("Zombie1"));
        else
            StartCoroutine(LoadGameScene(gameManager.mapList[gameManager.mapNumber]));

        //Randomly located
        float x = UnityEngine.Random.Range(10, 90);
        float y = 20;
        float z = UnityEngine.Random.Range(10, 90);
        m_fpsCtrl.gameObject.transform.position = new Vector3(x, y, z);
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
            m_imgVRLoading.fillAmount = loadState.progress;
            print("Map loading.." + loadState.progress);
            yield return null;
        }
        print("Map loaded");

        m_imgLoading.fillAmount = 1;
        m_imgVRLoading.fillAmount = 1;

        yield return new WaitForSeconds(1f);

        //Send 'Loading done' message to server
        if (DEBUGGING_MODE)
        {
            isGameStart = true;
            m_pnLoadingBG.SetActive(false);
            m_pnIngameUI.SetActive(true);
            m_pnVRLoadingBG.SetActive(false);
            m_pnVRIngameUI.SetActive(true);
            m_fpsCtrl.enabled = true;
            m_fpsCtrl.gameObject.GetComponent<Rigidbody>().useGravity = true;
            m_fpsCtrl.transform.position = new Vector3(m_fpsCtrl.transform.position.x, 10, m_fpsCtrl.transform.position.z);
            print("Game start");
        }
        else
        {        
            //Load other players
            foreach (ClientInfo ci in client.clientLists)
            {
                if (ci.identification == client.identification) continue;
                GameObject newPlayer = Instantiate(playerCharPrefab);
                OtherCharacter oc = newPlayer.GetComponent<OtherCharacter>();
                oc.playerName = ci.name;
                oc.playerIdentification = ci.identification;
            }
            client.SendData(NetCommand.LoadGame, "True");
        }
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
        if(playerState != PlayerState.Dead)
            sbPosRot.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8}",    //Position(x, y, z), Rotation(x, y, z, w -> Quaternion), OnGround, Flash
                Math.Round(playerT.position.x,2), Math.Round(playerT.position.y,2), Math.Round(playerT.position.z,2),
                Math.Round(playerT.rotation.x,1), Math.Round(playerT.rotation.y,1), Math.Round(playerT.rotation.z,1), Math.Round(playerT.rotation.w,2),
                m_fpsCtrl.m_Animator.GetBool("OnGround"), isFlashOn);

        //Push position and rotation value
        if(client != null) client.positionRotation = sbPosRot.ToString();

        //Update role rotation time, player's role
        if(client != null) {
            int rotateTime = client.rotateTimeLeft;
            txtRotateLeftTime.text = Mathf.Clamp(client.rotateTimeLeft, 0, 100).ToString();
            txtVRRotateLeftTime.text = Mathf.Clamp(client.rotateTimeLeft, 0, 100).ToString();
            if(playerState == PlayerState.Human)
                m_fpsCtrl.isHuman = true;
            else
                m_fpsCtrl.isHuman = false;

            SwitchPlayerCharacter();
            if (isGameStart && client.isRoleRotated)
            {
                sceneAudio.PlayOneShot(audioThunder);
                StartCoroutine(SwitchPlayerThunder());
                client.isRoleRotated = false;
            }

            if (client.userState != playerState)
            {
                playerState = client.userState;
                //################################### ON DEATH #####################################
                if(playerState == PlayerState.Dead)
                {
                    //put ragdoll.
                    GameObject ragdoll = Instantiate(playerRagdollObj, m_fpsCtrl.gameObject.transform.position, m_fpsCtrl.gameObject.transform.rotation) as GameObject;
                    //remove player object
                    zombieFPSObj.SetActive(false);
                    zombieObj.SetActive(false);
                    humanFPSObj.SetActive(false);
                    humanObj.SetActive(false);
                    m_fpsCtrl.gameObject.GetComponent<DeathCamController>().enabled = true;
                    m_fpsCtrl.gameObject.GetComponent<CharacterController>().enabled = false;
                    m_fpsCtrl.enabled = false;

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
            sceneAudio.PlayOneShot(audioFlash);
            FlashObj.SetActive(isFlashOn);
        }

        //ESC Menu Control
        if(CrossPlatformInputManager.GetButtonDown("Cancel"))
        {
            //if(!isPause)
            {
                isPause = !isPause;
                m_fpsCtrl.m_isPause = isPause;
                m_pnESCMenu.SetActive(isPause);
                m_pnBackToMain.SetActive(false);
                m_pnExitToDesktop.SetActive(false);
                m_pnVRESCMenu.SetActive(isPause);
                Cursor.visible = isPause;
            }
        }
    }
    /// <summary>
    /// Wait until Attack animation end
    /// </summary>
    IEnumerator AttackEnd()
    {
        yield return new WaitForSeconds(0.1f);
        attackAvailable = true;
    }

    //FixedUpdate
    void FixedUpdate()
    {
        if (!DEBUGGING_MODE)
        {
            if (client.isGamePlaying)
            {
                //Start Game!!
                if (!isGameStart)
                {
                    isGameStart = true;
                    m_pnLoadingBG.SetActive(false);
                    m_pnIngameUI.SetActive(true);
                    m_pnVRLoadingBG.SetActive(false);
                    m_pnVRIngameUI.SetActive(true);
                    m_fpsCtrl.enabled = true;
                    m_fpsCtrl.gameObject.GetComponent<Rigidbody>().useGravity = true;
                    m_fpsCtrl.transform.position = new Vector3(m_fpsCtrl.transform.position.x, 10, m_fpsCtrl.transform.position.z);
                    //role rotation timer start!!
                    StartCoroutine(gameManager.ReceiveRotatePlayerRoleTimer());
                    SwitchPlayerCharacter();
                    sceneAudio.PlayOneShot(audioThunder);
                    StartCoroutine(SwitchPlayerThunder());
                    print("Game start");
                }

                //Send player's control
                if (client != null)
                    if(client.userState != PlayerState.Dead)
                        client.SendPlayerControl();

                //Get informations from client instance -> processed in "OtherCharacter" class component of each players' gameobject.
            }

            //gameover?
            if(client.isGameOver)
                if(!isGameOver)
                {
                    isGameOver = true;
                    StartCoroutine(GameOver());
                }
        }
    }

    #region ESC Menu buttons
    public void btnResume_Clicked()
    {
        m_pnESCMenu.SetActive(false);
        m_pnVRESCMenu.SetActive(false);
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
        isGameStart = false;
        client.isConnected = false;
        client.isGamePlaying = false;
        client.isLoadingStarting = false;
        client.Disconnect();
        server.CloseServer();
        UnityEngine.SceneManagement.SceneManager.LoadScene("menu");
    }
    public void btnExitToDesktopYes_Clicked()
    {
        client.SendData(NetCommand.Disconnect, "");
        client.Disconnect();
        server.CloseServer();
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
        if (!hasFocus && isGameStart)
        {
            isPause = true;
            m_fpsCtrl.m_isPause = isPause;
            m_pnESCMenu.SetActive(true);
            m_pnBackToMain.SetActive(false);
            m_pnExitToDesktop.SetActive(false);
            Cursor.visible = true;
        }
    }

    /// <summary>
    /// Switch player character refer userstate
    /// </summary>
    void SwitchPlayerCharacter()
    {
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
                txtVRPlayerStatus.text = "You are the human..";
                break;
            case PlayerState.Zombie:
                zombieObj.SetActive(true);
                zombieFPSObj.SetActive(true);
                humanObj.SetActive(false);
                humanFPSObj.SetActive(false);
                m_fpsCtrl.m_Animator = zombieObj.GetComponent<Animator>();
                m_fpsCtrl.m_Animator_fps = zombieFPSObj.GetComponent<Animator>();
                txtPlayerStatus.text = "You are the zombie..";
                txtVRPlayerStatus.text = "You are the zombie..";
                break;
        }
    }
    /// <summary>
    /// Thunder sound playing coroutine
    /// </summary>
    /// <returns></returns>
    IEnumerator SwitchPlayerThunder()
    {
        if (isGameStart)
        {
            Light light = GameObject.FindGameObjectWithTag("Sunlight").GetComponent<Light>();
            light.intensity = 6f;
            yield return new WaitForSeconds(0.1f);
            light.intensity = 0.5f;
            yield return new WaitForSeconds(0.1f);
            light.intensity = 5f;

            while (light.intensity > 0.8f)
            {
                light.intensity -= 0.2f;
                yield return new WaitForSeconds(0.05f);
            }
            light.intensity = 0.8f;
        }
    }

    /// <summary>
    /// Game over coroutine
    /// </summary>
    /// <returns></returns>
    IEnumerator GameOver()
    {
        string winner;
        try
        {
            winner = client.clientLists.Find(c => c.identification == client.strWinner).name;
        }
        catch
        {
            winner = "[UNKNOWN]";
        }

        txtWinner.text += winner;
        txtVRWinner.text += winner;
        if (client.strWinner == client.identification)
        {
            txtWinner.text += "(YOU)";
            txtVRWinner.text += "(YOU)";
        }
        m_pnGameOver.SetActive(true);
        m_pnVRGameOver.SetActive(true);
        yield return new WaitForSeconds(5f);
        isGameStart = false;
        isGameOver = false;
        client.isGameOver = false;
        client.isGamePlaying = false;
        client.isLoadingStarting = false;
        client.strWinner = "";
        StopCoroutine(client.SendCheck());

        foreach (ClientInfo ci in client.clientLists)
        {
            ci.isReady = false;
            ci.isLoadingDone = false;
            ci.userState = PlayerState.None;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        UnityEngine.SceneManagement.SceneManager.LoadScene("room");
    }
}