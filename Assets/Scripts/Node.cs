using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour {

    public List<Node> connectedWaypoints;
    bool hasBeenChecked = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(!hasBeenChecked)
        {
            Debug.Log("My list of nodes is: " + connectedWaypoints.Count);
            hasBeenChecked = true;
        }
	}
}
