using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;
public class DeathCamController : MonoBehaviour {

    public float moveSpeed = 10;
    public float rotSpeed = 80;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        float hor = CrossPlatformInputManager.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        float ver = CrossPlatformInputManager.GetAxis("Vertical") * moveSpeed * Time.deltaTime;

        if(CrossPlatformInputManager.GetButton("Dash"))
        {
            hor *= 2;
            ver *= 2;
        }

        float xRot = CrossPlatformInputManager.GetAxis("Mouse X") * rotSpeed * Time.deltaTime;
        float yRot = CrossPlatformInputManager.GetAxis("Mouse Y") * rotSpeed * Time.deltaTime;

        transform.Rotate(yRot, xRot, 0,Space.World);
        transform.Translate(hor, 0, ver);
    }
}
