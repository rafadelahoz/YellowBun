using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph : MonoBehaviour {

    public Node[] graph =  null;
    public int maximumDistance = 8;

	// Use this for initialization
	void Start () {

	}
	
    public void InitializeListOfNodes()
    {
        if (this.graph.Length == 0)
        {
            graph = this.GetComponentsInChildren<Node>();

            foreach (Node nodeA in graph)
            {
                Vector3 position = nodeA.GetComponent<Transform>().position;
                foreach (Node nodeB in graph)
                {
                    if (!nodeA.transform.position.Equals(nodeB.transform.position) && IsCloseEnough(nodeA, nodeB) && !nodeA.connectedWaypoints.Contains(nodeB))
                    {
                        nodeA.connectedWaypoints.Add(nodeB);
                    }
                }
            }
        }
    }

    bool IsCloseEnough(Node nodeA, Node nodeB)
    {
        Vector3 positionA = nodeA.transform.position;
        Vector3 positionB = nodeB.transform.position;

        double distance = Vector2.Distance(new Vector2(positionA.x, positionA.y), new Vector2(positionB.x, positionB.y));
        return distance <= maximumDistance;
    }

	// Update is called once per frame
	void Update () {
		
	}
}
