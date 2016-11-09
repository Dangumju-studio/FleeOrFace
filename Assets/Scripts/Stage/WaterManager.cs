using UnityEngine;
using System.Collections;

public class WaterManager : MonoBehaviour {
    public GameObject water;
    public GameObject wind;
    public GameObject[] wateroutsound;
    public GameObject waterUp;
    public GameObject waterDown;

    // Use this for initialization
    public bool playerwaterin = false;
	void Start () {
        waterDown.SetActive(false);
        waterUp.SetActive(true);
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Player")
        {
            waterDown.SetActive(true);
            waterUp.SetActive(false);
            water.GetComponent<AudioSource>().Play();
            wind.GetComponent<AudioSource>().Stop();                
            GameObject.FindWithTag("Player").GetComponent<AudioSource>().volume = 0;
            for(int i = 0;  i < wateroutsound.Length; i++)
            {
                wateroutsound[i].GetComponent<AudioSource>().Stop();
            }
        }
        
    }
    public void OnTriggerExit(Collider col)
    {
        if (col.tag == "Player")
        {
            waterDown.SetActive(false);
            waterUp.SetActive(true);
            water.GetComponent<AudioSource>().Stop();
            wind.GetComponent<AudioSource>().Play();
            
            GameObject.FindWithTag("Player").GetComponent<AudioSource>().volume = 1;
            for (int i = 0; i < wateroutsound.Length; i++)
            {
                wateroutsound[i].GetComponent<AudioSource>().Play();
            }
        }

    }
}
