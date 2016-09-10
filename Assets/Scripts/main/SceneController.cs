using UnityEngine;
using System.Collections;

public class SceneController : MonoBehaviour {

    public bool isThirdPersonCamera = false;

    [SerializeField] private Camera m_PlayerCamera;
    [SerializeField] private UnityStandardAssets.Characters.FirstPerson.FirstPersonController m_fpsCtrl;

	// Use this for initialization
	void Start () {
        if(isThirdPersonCamera)
        {
            m_fpsCtrl.m_UseHeadBob = false;
            m_PlayerCamera.transform.localPosition = new Vector3(0, 2, -2.5f);
            m_PlayerCamera.transform.localRotation = Quaternion.Euler(30, 0, 0);
            
        }

    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
