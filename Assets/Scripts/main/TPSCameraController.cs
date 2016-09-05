using UnityEngine;
using System.Collections;

public class TPSCameraController : MonoBehaviour {

	#region private field
	[SerializeField]
	GameObject player;
	[SerializeField]
	Camera mainCam;
	[SerializeField]
	Vector3 camDistance;
	//[SerializeField]
	//Transform camViewTarget;
	[SerializeField]
	float camFollowSpeed;

	Vector3 nextCamPos;
    Vector3 nextCamRot;
	#endregion

	// Use this for initialization
	void Start () {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
	}
	
	// Update is called once per frame
	void LateUpdate () {
        //Camera Following
        mainCam.transform.localPosition = camDistance;
        transform.position = player.transform.position;// Vector3.Lerp (transform.position, player.transform.position, Time.deltaTime * camFollowSpeed);

		transform.Rotate (Input.GetAxisRaw ("Mouse Y"), Input.GetAxisRaw ("Mouse X"), 0);
        nextCamRot = new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
        if (nextCamRot.x > 25 && nextCamRot.x < 100) nextCamRot.x = 25;
        if (nextCamRot.x < 310 && nextCamRot.x > 250) nextCamRot.x = 310;
        transform.rotation = Quaternion.Euler(nextCamRot);
        
	}

    void Update()
    {
        //Toggle cursor state
        if (Input.GetKey(KeyCode.Escape))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        if (Input.GetMouseButtonDown(0))
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
