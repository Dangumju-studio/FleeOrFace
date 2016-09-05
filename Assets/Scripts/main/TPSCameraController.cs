using UnityEngine;
using System.Collections;

public class TPSCameraController : MonoBehaviour {

	#region private field
	[SerializeField]
	GameObject player;
	[SerializeField]
	Camera camera;
	[SerializeField]
	Vector3 camDistance;
	[SerializeField]
	Transform camViewTarget;
	[SerializeField]
	float camFollowSpeed;

	Vector3 nextCamPos;
	#endregion

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		camera.transform.localPosition = camDistance;
		transform.position = Vector3.Lerp (transform.position, player.transform.position, Time.deltaTime * camFollowSpeed);

		transform.Rotate (Input.GetAxisRaw ("Mouse Y"), Input.GetAxisRaw ("Mouse X"), 0);
		transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
	}
}
