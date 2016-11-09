using UnityEngine;
using System.Collections;

public class Zombie1map : MonoBehaviour {

    void Awake()
    {
        UnityEngine.SceneManagement.Scene mainScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName("main");
        if (!mainScene.isLoaded)
            UnityEngine.SceneManagement.SceneManager.LoadScene("main");
    }

	// Use this for initialization
	void Start () {

     
    }

    // Update is called once per frame
    void Update () {
	
	}
}
