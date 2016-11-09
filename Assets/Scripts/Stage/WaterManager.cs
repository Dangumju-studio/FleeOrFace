using UnityEngine;
using System.Collections;

public class WaterManager : MonoBehaviour {
    public GameObject water;
    // Use this for initialization
    public bool playerwaterin = false;
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTiriggerEnter(Collider col)
    {
        if (col.tag == "Player")
        {
            water.GetComponent<AudioSource>().Play();
        }
    }
}
