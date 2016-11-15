using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;
public class DeathCamController : MonoBehaviour {

    public float moveSpeed = 50;
    public float rotSpeed = 10;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        float hor = CrossPlatformInputManager.GetAxis("Horizontal") * moveSpeed;
        float ver = CrossPlatformInputManager.GetAxis("Vertical") * moveSpeed;

        if(CrossPlatformInputManager.GetButtonDown("Dash"))
        {
            hor *= 2;
            ver *= 2;
        }

        float xRot = CrossPlatformInputManager.GetAxis("Mouse X") * rotSpeed;
        float yRot = CrossPlatformInputManager.GetAxis("Mouse Y") * rotSpeed;

        transform.Rotate(xRot, yRot, 0);
        transform.Translate(hor, 0, ver);
    }
}
