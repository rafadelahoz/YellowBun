using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformController : RaycastController {

    public LayerMask passengerMask;

    public Vector3 move;

    List<PassengerMovement> passengerMovement;
    Dictionary<Transform, Controller2D> passengerDictionary;

    HashSet<Transform> movedPassengers;

	override public void Start () {
        base.Start();

        passengerMovement = new List<PassengerMovement>();
        passengerDictionary = new Dictionary<Transform, Controller2D>();

        movedPassengers = new HashSet<Transform>();
	}
	
	void Update () {
        UpdateRaycastOrigins(); 

        Vector3 velocity = move * Time.deltaTime;
        CalculatePassengerMotion(velocity);

        MovePassengers(true);
        transform.Translate(velocity); 
        MovePassengers(false);
	}

    void MovePassengers(bool movingBeforePlatform)
    {
        foreach (PassengerMovement pm in passengerMovement)
        {
            if (!passengerDictionary.ContainsKey(pm.transform))
            {
                passengerDictionary.Add(pm.transform, pm.transform.GetComponent<Controller2D>());
            }

            if (pm.moveBeforePlatform == movingBeforePlatform)
            {
                if (passengerDictionary.ContainsKey(pm.transform) && passengerDictionary[pm.transform] != null)
                {
                    passengerDictionary[pm.transform].Move(pm.velocity, pm.standingOnPlatform);
                }
            }
        }
    }

    void CalculatePassengerMotion(Vector3 velocity)
    {
        passengerMovement.Clear();

        movedPassengers.Clear();

        float directionX = Mathf.Sign(velocity.x);
        float directionY = Mathf.Sign(velocity.y);

        // Vertically moving platform
        if (velocity.y != 0)
        {
            float rayLength = Mathf.Abs(velocity.y) + skinWidth;

            for (int i = 0; i < verticalRayCount; i++)
            {
                Vector2 rayOrigin = (directionY < 0 ? raycastOrigins.bottomLeft : raycastOrigins.topLeft);
                rayOrigin += Vector2.right * (verticalRaySpacing * i);
                // Find a passenger
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);
                // If found, stick platform and passenger together, then move the passenger
                if (hit)
                {
                    if (!movedPassengers.Contains(hit.transform))
                    {
                        float pushX = (directionY > 0 ? velocity.x : 0);
                        float pushY = velocity.y - (hit.distance - skinWidth) * directionY;

                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), (directionY > 0), true));
                        movedPassengers.Add(hit.transform); 
                    }
                }
            }
        }

        // Horizontally moving platform
        if (velocity.x != 0)
        {
            float rayLength = Mathf.Abs(velocity.y) + skinWidth;

            Vector2 firstOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            Vector2 rayOrigin = Vector2.zero;
            for (int i = 0; i < horizontalRayCount; i++)
            {
                rayOrigin = firstOrigin + Vector2.up * (horizontalRaySpacing * i);
                // Find a passenger
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, passengerMask);

                // If found, stick platform and passenger together, then move the passenger
                if (hit)
                {
                    if (!movedPassengers.Contains(hit.transform))
                    {
                        float pushX = velocity.x - (hit.distance - skinWidth) * directionX;
                        float pushY = -skinWidth; // So the passenger still checks for bottom platforms

                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), false, true));
                        movedPassengers.Add(hit.transform);
                    }
                }
            }
        }

        // Passenger riding platform (horizontally or downward)
        if (directionY < 0 || velocity.y == 0 && velocity.x != 0)
        {
            float rayLength = skinWidth * 2;

            for (int i = 0; i < verticalRayCount; i++)
            {
                Vector2 rayOrigin = raycastOrigins.topLeft + Vector2.right * (verticalRaySpacing * i);
                // Find a passenger riding the platform
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);
                // If found, stick platform and passenger together, then move the passenger
                if (hit)
                {
                    if (!movedPassengers.Contains(hit.transform))
                    {
                        float pushX = velocity.x;
                        float pushY = velocity.y;

                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
                        movedPassengers.Add(hit.transform);
                    }
                }
            } 
        }
    }

    struct PassengerMovement
    {
        public Transform transform;
        public Vector3 velocity;
        public bool standingOnPlatform;
        public bool moveBeforePlatform;

        public PassengerMovement(Transform _transform, Vector3 _velocity, bool _standingOnPlatform, bool _moveBeforePlatform)
        {
            transform = _transform;
            velocity = _velocity;
            standingOnPlatform = _standingOnPlatform;
            moveBeforePlatform = _moveBeforePlatform;
        }
    }
}
