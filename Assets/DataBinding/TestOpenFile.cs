using UnityEngine;
using System.Collections;
using DataBinding;
using System.Text.RegularExpressions;

public class TestOpenFile : MonoBehaviour {

    public TextAsset tasset;

	// Use this for initialization
	void Start () {       
        DataObject dobjs = new DataObject(tasset.text);
        
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
