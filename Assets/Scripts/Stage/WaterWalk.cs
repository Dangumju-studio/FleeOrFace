using UnityEngine;
using System.Collections;

public class WaterWalk : MonoBehaviour {
    public GameObject player;

    void Start()
    {
        player = GameObject.Find("FPScontroller");
    }

    public void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Player")
        {
            //player.GetComponent<AudioSource>().clip = Resources.Load("Audio/waterwalk");
        }
    }
}
