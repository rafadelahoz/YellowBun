using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtificialIntelligence : MonoBehaviour {

    public GameObject target;
    public Graph graph;
    public Node closestNode;
    public Node targetClosestNode;

    // TODO: Clean, all copied from player
    float Gravity = -20;

    public float MoveSpeed = 6;
    public float accelerationTimeAirborne = 0.2f;
    public float accelerationTimeGrounded = 0.1f;

    Vector2 velocity;
    float hspeedSmoothing;

    Vector2 directionalInput;
    Controller2D aiController;

    List<Node> pathToFollow = new List<Node>();

    // Use this for initialization
    void Start () {
        aiController = this.GetComponent<Controller2D>();

        graph = GameObject.Find("Graph").GetComponent<Graph>();
        graph.InitializeListOfNodes();

        this.closestNode = GetClosestNode(this.transform.position);
        targetClosestNode = GetClosestNode(target.transform.position);
        pathToFollow = aStar(closestNode, graph, targetClosestNode);

    }

    // Update is called once per frame
    void Update () {
        this.closestNode = GetClosestNode(this.transform.position);
        Node newTargetClosestNode = GetClosestNode(target.transform.position);

        if (!targetClosestNode.Equals(newTargetClosestNode))
        {
            targetClosestNode = newTargetClosestNode;
            pathToFollow = aStar(closestNode, graph, targetClosestNode);
        }

        if(pathToFollow.Count > 1 && closestNode.Equals(pathToFollow[0]))
        {
            pathToFollow.Remove(closestNode);
        }

        // TODO if no new closest node go to target
        if (DistanceToPosition(this.transform.position, target.transform.position) <= DistanceToNode(this.transform.position, targetClosestNode))
        {
            directionalInput = CalculateDirectionalInput(target.transform.position);
        } else {
            directionalInput = CalculateDirectionalInput(pathToFollow[0]);
        }

        CalculateVelocity();

        aiController.Move(velocity * Time.deltaTime, directionalInput, true);
	}

    // aStar implementation to calculate the path this object will follow to get to a certain goal
    List<Node> aStar(Node start, Graph graph, Node goal)
    {
        // Set of nodes currently evaluated
        HashSet<Node> closedSet = new HashSet<Node>();

        // The set of currently discovered nodes that are not evaluated yet
        HashSet<Node> openSet = new HashSet<Node>();
        //Initially, only the start node is known.
        openSet.Add(start);

        Dictionary<Node, ScoreMap> scoreMap = InitializeAStarScores(graph);

        scoreMap[start].costScore = 0;
        scoreMap[start].heuristicScore = HeuristicCostEstimate(start, goal);

        // While there are items in the open set
        while (openSet.Count > 0)
        {
            Node current = RetrieveNodeWithLowestHeuristicScore(openSet, scoreMap);

            if (current.Equals(goal))
            {
                return ReconstructPath(start, scoreMap, current);
            }

            openSet.Remove(current);
            closedSet.Add(current);

            foreach(Node neighbor in current.connectedWaypoints)
            {
                if(closedSet.Contains(neighbor)) continue; // Ignore the neighbor which is already evaluated.

                if (!openSet.Contains(neighbor))// Discover a new node
                {
                    openSet.Add(neighbor);
                }

                // TODO : Handle situations that require jumping
                // The distance from current to a neighbor
                int tentativeCostScore = scoreMap[current].costScore + (int) System.Math.Ceiling(DistanceToNode(current.transform.position,neighbor));

                if (tentativeCostScore >= scoreMap[neighbor].costScore) continue; // This is not a better path.

                scoreMap[neighbor].cameFrom = current;
                scoreMap[neighbor].costScore = tentativeCostScore;
                scoreMap[neighbor].heuristicScore = scoreMap[neighbor].costScore + HeuristicCostEstimate(neighbor, goal);
            }
        }

        return null;
    }

    // Returns the path from the start node to the goal node. (start -> node -> node -> ... -> goal)
    private List<Node> ReconstructPath(Node start, Dictionary<Node, ScoreMap> scoreMap, Node current)
    {
        List<Node> path = new List<Node>();
        Node aux = current;

        path.Add(aux);
        while (!aux.Equals(start))
        {
            aux = scoreMap[aux].cameFrom;
            path.Add(aux);
        }

        path.Add(start);
        path.Reverse();
        return path;
    }

    // Retrieves the node with the lowest heuristic score, it ignores the cost score.
    private Node RetrieveNodeWithLowestHeuristicScore(HashSet<Node> openSet, Dictionary<Node, ScoreMap> scoreMap)
    {
        int smallestValue = int.MaxValue;
        Node bestNode = null;

        foreach (Node node in openSet)
        {
            ScoreMap nodeValues;
            scoreMap.TryGetValue(node, out nodeValues);

            if (nodeValues.heuristicScore < smallestValue)
            {
                smallestValue = nodeValues.heuristicScore;
                bestNode = node;
            }
        }

        return bestNode;
    }

    // Initializes the scores for the aStar algorithm graph nodes
    Dictionary<Node, ScoreMap> InitializeAStarScores(Graph graph)
    {
        Dictionary<Node, ScoreMap> nodeMap = new Dictionary<Node, ScoreMap>();

        foreach(Node node in graph.graph)
        {
            nodeMap.Add(node, new ScoreMap(int.MaxValue, int.MaxValue, null));
        }

        return nodeMap;
    }

    // Estimates a heuristic cost to get to a certain node according to the manhattan distance
    // TODO: For now we calculated it using manhattan distance
    int HeuristicCostEstimate(Node start, Node goal)
    {
        return (int) System.Math.Ceiling(System.Math.Abs(goal.transform.position.x - start.transform.position.x) + System.Math.Abs(goal.transform.position.y - start.transform.position.y));
    }

    // Calculates the velocity of this object
    // TODO: Clean up, this is copied from player
    // TODO: Something here blocks the velocity on y, so now our AI cannot go upwards
    void CalculateVelocity()
    {
        float targetHspeed = directionalInput.x * MoveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetHspeed, ref hspeedSmoothing, (aiController.collisions.below ? accelerationTimeGrounded : accelerationTimeAirborne));
        velocity.y += Gravity * Time.deltaTime;

        // Avoid really small floats
        if (Mathf.Abs(velocity.x) < 0.3)
        {
            velocity.x = 0;
        }
    }

    // Calculates the directional input to get from this object currents position to the given node in a 2d plane identified by x and y
    Vector2 CalculateDirectionalInput(Node target)
    {
        return CalculateDirectionalInput(target.transform.position);
    }

    // Calculates the directional input to get from this object currents position to the given position in a 2d plane identified by x and y
    private Vector2 CalculateDirectionalInput(Vector3 position)
    {
        return (GetV2FromV3(position) - GetV2FromV3(this.transform.position)).normalized;
    }

    // Returns the closest node in a 2d plane identified by x and y
    Node GetClosestNode(Vector3 position)
    {
        // Initialization
        double shortest_distance = double.MaxValue;
        Node closestNode = null;

        // Find closest node
        foreach (Node node in graph.graph)
        {
            double new_distance = DistanceToNode(position, node);
            if (shortest_distance > new_distance)
            {
                shortest_distance = new_distance;
                closestNode = node;
            }
        }

        return closestNode;
    }

    // Calculates the distance between a position and a node in a 2d plane identified by x and y
    double DistanceToNode(Vector3 position, Node node)
    {
        return DistanceToPosition(position, node.transform.position);
    }

    // Calculates the distance between to positions in a 2d plane identified by x and y
    double DistanceToPosition(Vector3 positionA, Vector3 positionB)
    {
        return Vector2.Distance(GetV2FromV3(positionA), GetV2FromV3(positionB));
    }

    // Transforms a Vector3 to a Vector2
    Vector2 GetV2FromV3(Vector3 v3)
    {
        return new Vector2(v3.x, v3.y);
    }

    // Represents the values that are stored by the aStar algorithm to calculate the best path
    private class ScoreMap
    {
        public int heuristicScore;
        public int costScore;
        public Node cameFrom;

        public ScoreMap(int heuristicScore, int costScore, Node cameFrom)
        {
            this.heuristicScore = heuristicScore;
            this.costScore = costScore;
            this.cameFrom = cameFrom;
        }
    };
}
