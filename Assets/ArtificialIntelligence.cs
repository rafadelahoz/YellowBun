using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArtificialIntelligence : MonoBehaviour
{
    public GameObject Target;
    public Graph Graph;
    public Node ClosestNode;
    public Node TargetClosestNode;

    // TODO: Clean, all copied from player
    private const float Gravity = -20;

    public float MoveSpeed = 6;
    public float AccelerationTimeAirborne = 0.2f;
    public float AccelerationTimeGrounded = 0.1f;

    private Vector2 _velocity;
    private float _hspeedSmoothing;

    private Vector2 _directionalInput;
    public Controller2D AiController;

    private List<Node> _pathToFollow = new List<Node>();

    // Use this for initialization
    private void Start()
    {
        AiController = GetComponent<Controller2D>();

        Graph = GameObject.Find("Graph").GetComponent<Graph>();
        Graph.InitializeListOfNodes();

        ClosestNode = GetClosestNode(transform.position);
        TargetClosestNode = GetClosestNode(Target.transform.position);
        _pathToFollow = AStar(ClosestNode, Graph, TargetClosestNode);
    }

    // Update is called once per frame
    private void Update()
    {
        ClosestNode = GetClosestNode(transform.position);
        var newTargetClosestNode = GetClosestNode(Target.transform.position);

        if (!TargetClosestNode.Equals(newTargetClosestNode))
        {
            TargetClosestNode = newTargetClosestNode;
            _pathToFollow = AStar(ClosestNode, Graph, TargetClosestNode);
        }

        if (_pathToFollow.Count > 1 && ClosestNode.Equals(_pathToFollow[0]))
        {
            _pathToFollow.Remove(ClosestNode);
        }

        // TODO if no new closest node go to target
        _directionalInput = DistanceToPosition(transform.position, Target.transform.position) <=
                            DistanceToNode(transform.position, TargetClosestNode)
            ? CalculateDirectionalInput(Target.transform.position)
            : CalculateDirectionalInput(_pathToFollow[0]);

        CalculateVelocity();

        AiController.Move(_velocity * Time.deltaTime, _directionalInput, true);
    }

    // aStar implementation to calculate the path this object will follow to get to a certain goal
    private static List<Node> AStar(Node start, Graph graph, Component goal)
    {
        // Set of nodes currently evaluated
        var closedSet = new HashSet<Node>();

        // The set of currently discovered nodes that are not evaluated yet
        var openSet = new HashSet<Node> {start};
        //Initially, only the start node is known.

        var scoreMap = InitializeAStarScores(graph);

        scoreMap[start].CostScore = 0;
        scoreMap[start].HeuristicScore = HeuristicCostEstimate(start, goal);

        // While there are items in the open set
        while (openSet.Count > 0)
        {
            var current = RetrieveNodeWithLowestHeuristicScore(openSet, scoreMap);

            if (current.Equals(goal))
            {
                return ReconstructPath(start, scoreMap, current);
            }

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (var neighbor in current.ConnectedWaypoints)
            {
                if (closedSet.Contains(neighbor)) continue; // Ignore the neighbor which is already evaluated.

                if (!openSet.Contains(neighbor)) // Discover a new node
                {
                    openSet.Add(neighbor);
                }

                // TODO : Handle situations that require jumping
                // The distance from current to a neighbor
                var tentativeCostScore = scoreMap[current].CostScore +
                                         (int) Math.Ceiling(DistanceToNode(current.transform.position,
                                             neighbor));

                if (tentativeCostScore >= scoreMap[neighbor].CostScore) continue; // This is not a better path.

                scoreMap[neighbor].CameFrom = current;
                scoreMap[neighbor].CostScore = tentativeCostScore;
                scoreMap[neighbor].HeuristicScore =
                    scoreMap[neighbor].CostScore + HeuristicCostEstimate(neighbor, goal);
            }
        }

        return null;
    }

    // Returns the path from the start node to the goal node. (start -> node -> node -> ... -> goal)
    private static List<Node> ReconstructPath(Node start, IDictionary<Node, ScoreMap> scoreMap, Node current)
    {
        var path = new List<Node>();
        var aux = current;

        path.Add(aux);
        while (!aux.Equals(start))
        {
            aux = scoreMap[aux].CameFrom;
            path.Add(aux);
        }

        path.Add(start);
        path.Reverse();
        return path;
    }

    // Retrieves the node with the lowest heuristic score, it ignores the cost score.
    private static Node RetrieveNodeWithLowestHeuristicScore(IEnumerable<Node> openSet,
        IDictionary<Node, ScoreMap> scoreMap)
    {
        var smallestValue = int.MaxValue;
        Node bestNode = null;

        foreach (var node in openSet)
        {
            ScoreMap nodeValues;
            scoreMap.TryGetValue(node, out nodeValues);

            if (nodeValues != null && nodeValues.HeuristicScore >= smallestValue) continue;
            if (nodeValues != null) smallestValue = nodeValues.HeuristicScore;
            bestNode = node;
        }

        return bestNode;
    }

    // Initializes the scores for the aStar algorithm graph nodes
    private static Dictionary<Node, ScoreMap> InitializeAStarScores(Graph graph)
    {
        return graph.Waypoints.ToDictionary(node => node, node => new ScoreMap(int.MaxValue, int.MaxValue, null));
    }

    // Estimates a heuristic cost to get to a certain node according to the manhattan distance
    // TODO: For now we calculated it using manhattan distance
    private static int HeuristicCostEstimate(Component start, Component goal)
    {
        return (int) Math.Ceiling(Math.Abs(goal.transform.position.x - start.transform.position.x) +
                                  Math.Abs(goal.transform.position.y - start.transform.position.y));
    }

    // Calculates the velocity of this object
    // TODO: Clean up, this is copied from player
    // TODO: Something here blocks the velocity on y, so now our AI cannot go upwards
    private void CalculateVelocity()
    {
        var targetHspeed = _directionalInput.x * MoveSpeed;
        _velocity.x = Mathf.SmoothDamp(_velocity.x, targetHspeed, ref _hspeedSmoothing,
            (AiController.collisions.below ? AccelerationTimeGrounded : AccelerationTimeAirborne));
        _velocity.y += Gravity * Time.deltaTime;

        // Avoid really small floats
        if (Mathf.Abs(_velocity.x) < 0.3)
        {
            _velocity.x = 0;
        }
    }

    // Calculates the directional input to get from this object currents position to the given node in a 2d plane identified by x and y
    private Vector2 CalculateDirectionalInput(Component target)
    {
        return CalculateDirectionalInput(target.transform.position);
    }

    // Calculates the directional input to get from this object currents position to the given position in a 2d plane identified by x and y
    private Vector2 CalculateDirectionalInput(Vector3 position)
    {
        return (GetV2FromV3(position) - GetV2FromV3(transform.position)).normalized;
    }

    // Returns the closest node in a 2d plane identified by x and y
    private Node GetClosestNode(Vector3 position)
    {
        // Initialization
        var shortestDistance = double.MaxValue;
        Node closestNode = null;

        // Find closest node
        foreach (var node in Graph.Waypoints)
        {
            var newDistance = DistanceToNode(position, node);
            if (!(shortestDistance > newDistance)) continue;
            shortestDistance = newDistance;
            closestNode = node;
        }

        return closestNode;
    }

    // Calculates the distance between a position and a node in a 2d plane identified by x and y
    private static double DistanceToNode(Vector3 position, Component node)
    {
        return DistanceToPosition(position, node.transform.position);
    }

    // Calculates the distance between to positions in a 2d plane identified by x and y
    private static double DistanceToPosition(Vector3 positionA, Vector3 positionB)
    {
        return Vector2.Distance(GetV2FromV3(positionA), GetV2FromV3(positionB));
    }

    // Transforms a Vector3 to a Vector2
    private static Vector2 GetV2FromV3(Vector3 v3)
    {
        return new Vector2(v3.x, v3.y);
    }

    // Represents the values that are stored by the aStar algorithm to calculate the best path
    private class ScoreMap
    {
        public int HeuristicScore;
        public int CostScore;
        public Node CameFrom;

        public ScoreMap(int heuristicScore, int costScore, Node cameFrom)
        {
            HeuristicScore = heuristicScore;
            CostScore = costScore;
            CameFrom = cameFrom;
        }
    }
}