using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerInfo : MonoBehaviour {

    public string playerName;
    public string identification;
    [SerializeField] RawImage imgIsReady;
    [SerializeField] Text txtPlayerName;
    [SerializeField] Texture imgReady, imgUnready;
    public bool isReady
    {
        get { return _isReady; }
        set
        {
            _isReady = value;
            if (value) imgIsReady.texture = imgReady;
            else imgIsReady.texture = imgUnready;
        }
    }
    bool _isReady;

	// Use this for initialization
	void Start () {
        isReady = false;
        txtPlayerName.text = playerName;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
