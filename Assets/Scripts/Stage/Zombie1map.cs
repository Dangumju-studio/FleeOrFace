using UnityEngine;
using System.Collections;

public class Zombie1map : MonoBehaviour {

	// Use this for initialization
	void Start () {

        RenderSettings.fog = true;
        RenderSettings.fogColor = Color.gray;
        RenderSettings.fogDensity = 0.005f;
        RenderSettings.fogMode = FogMode.ExponentialSquared;

    }

    // Update is called once per frame
    void Update () {
	
	}
}
