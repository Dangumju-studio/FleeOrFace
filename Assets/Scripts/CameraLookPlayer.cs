using UnityEngine;
using System.Collections;

public class CameraLookPlayer : MonoBehaviour {
    public Transform player;
	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
        transform.LookAt(player);
	}
}
