using UnityEngine;

public class Graph : MonoBehaviour
{
    public Node[] Waypoints;
    public int MaximumDistance = 8;

    public void InitializeListOfNodes()
    {
        if (Waypoints.Length != 0) return;
        Waypoints = GetComponentsInChildren<Node>();

        foreach (var nodeA in Waypoints)
        {
            foreach (var nodeB in Waypoints)
            {
                if (!nodeA.transform.position.Equals(nodeB.transform.position) && IsCloseEnough(nodeA, nodeB) &&
                    !nodeA.ConnectedWaypoints.Contains(nodeB))
                {
                    nodeA.ConnectedWaypoints.Add(nodeB);
                }
            }
        }
    }

    private bool IsCloseEnough(Component nodeA, Component nodeB)
    {
        var positionA = nodeA.transform.position;
        var positionB = nodeB.transform.position;

        double distance =
            Vector2.Distance(new Vector2(positionA.x, positionA.y), new Vector2(positionB.x, positionB.y));
        return distance <= MaximumDistance;
    }
}