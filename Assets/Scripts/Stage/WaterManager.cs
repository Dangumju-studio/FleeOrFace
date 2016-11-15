using UnityEngine;
using System.Collections;

public class WaterManager : MonoBehaviour {
    public GameObject water;
    public GameObject wind;
    public GameObject[] wateroutsound;
    public GameObject waterUp;
    public GameObject diving;
    public GameObject waterout;
    

    // Use this for initialization
    public bool playerwaterin = false;
	void Start () {
    
    }
	
	// Update is called once per frame
	void Update () {
       
	}

    public void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Player")
        {      
            waterUp.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
            waterUp.GetComponent<UnityStandardAssets.Water.Water>().reflectLayers.value = -257;
            water.GetComponent<AudioSource>().Play();
            diving.GetComponent<AudioSource>().PlayOneShot(diving.GetComponent<AudioSource>().clip);
            wind.GetComponent<AudioSource>().Stop();
            GameObject.FindWithTag("Player").GetComponent<AudioSource>().volume = 0;
            for (int i = 0; i < wateroutsound.Length; i++)
            {
                wateroutsound[i].GetComponent<AudioSource>().Stop();
            }
        }
        else if (col.tag == "OtherPlayer") { GetComponent<OtherCharacter>().OnWaterEnter(); }

        
    }
    public void OnTriggerExit(Collider col)
    {
        if (col.tag == "Player")
        {
            waterout.GetComponent<AudioSource>().PlayOneShot(waterout.GetComponent<AudioSource>().clip);
            waterUp.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            waterUp.GetComponent<UnityStandardAssets.Water.Water>().reflectLayers.value = -1;
            water.GetComponent<AudioSource>().Stop();
            wind.GetComponent<AudioSource>().Play();
            GameObject.FindWithTag("Player").GetComponent<AudioSource>().volume = 1;
            for (int i = 0; i < wateroutsound.Length; i++)
            {
                wateroutsound[i].GetComponent<AudioSource>().Play();
            }
        }
        else if(col.tag == "OtherPlayer")
        {
            GetComponent<OtherCharacter>().OnWaterLeave();
        }

    }
}
