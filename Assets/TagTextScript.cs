using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TagTextScript : MonoBehaviour {

    Transform dad;
    bool initialized = false;
	// Use this for initialization
	void Start () {
        dad = this.gameObject.transform.parent;

    }
	
	// Update is called once per frame
	void Update () {
		if(!initialized && dad.childCount > 1)
        {
            UpdateText();
        }
	}

    void UpdateText()
    {
        this.GetComponent<TextMesh>().text = dad.GetComponentInChildren<UnityBodyImplement>().gameObject.tag;
    }
}
