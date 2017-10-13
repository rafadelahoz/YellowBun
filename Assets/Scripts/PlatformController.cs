using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformController : RaycastController {

    public LayerMask passengerMask;

    public Vector3 move;

    HashSet<Transform> movedPassengers;

	override public void Start () {
        base.Start();

        movedPassengers = new HashSet<Transform>();
	}
	
	void Update () {
        UpdateRaycastOrigins(); 

        Vector3 velocity = move * Time.deltaTime;
        MovePassengers(velocity);
        transform.Translate(velocity); 
	}

    void MovePassengers(Vector3 velocity)
    {
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

                        hit.transform.Translate(new Vector3(pushX, pushY));
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
                        float pushY = 0;

                        hit.transform.Translate(new Vector3(pushX, pushY));
                        movedPassengers.Add(hit.transform);
                    }
                }
            }
        }

        // Passenger riding platform
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

                        hit.transform.Translate(new Vector3(pushX, pushY));
                        movedPassengers.Add(hit.transform);
                    }
                }
            } 
        }
    }
}
