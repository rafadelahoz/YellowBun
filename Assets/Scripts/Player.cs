using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Controller2D))]
public class Player : MonoBehaviour {

    float Gravity = -20;

    public float JumpHeight = 4;
    // public float MinJumpHeight = 1;
    public float TimeToApex = 0.4f;

    public float MoveSpeed = 6;
    public float accelerationTimeAirborne = 0.2f;
    public float accelerationTimeGrounded = 0.1f;

    public float WallSlideSpeedMax = 3;
    public Vector2 WallJumpClimb;
    public Vector2 WallJumpOff;
    public Vector2 WallJumpLeap;
    public float WallStickTime = 0.25f;
    float wallTimeToUnstick;

    float JumpSpeed; // = 8;
    // float MinJumpSpeed;

    Vector3 velocity;
    float hspeedSmoothing;

    Controller2D controller;

	void Start() 
    {
        controller = GetComponent<Controller2D>();

        Gravity = -(2 * JumpHeight) / Mathf.Pow(TimeToApex, 2);
        JumpSpeed = Mathf.Abs(Gravity) * TimeToApex;
        // MinJumpSpeed = Mathf.Sqrt(2 * Mathf.Abs(Gravity) * MinJumpHeight);
        print("G: " + Gravity + "; speed: " + JumpSpeed);
    }

    void Update()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        int wallDirX = (controller.collisions.left ? -1 : 1);

        float targetHspeed = input.x * MoveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetHspeed, ref hspeedSmoothing, (controller.collisions.below ? accelerationTimeGrounded : accelerationTimeAirborne));

        /* Wall Sliding / Jumping section */
        bool wallSliding = false;
        if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0)
        {
            wallSliding = true;
            if (velocity.y < -WallSlideSpeedMax)
            {
                velocity.y = -WallSlideSpeedMax;
            }

            // Allow for wall sticking
            if (wallTimeToUnstick > 0)
            {
                // Reset the horizontal velocity
                velocity.x = 0;
                hspeedSmoothing = 0;

                if (input.x != wallDirX && input.x != 0)
                {
                    wallTimeToUnstick -= Time.deltaTime;
                }
                else
                {
                    wallTimeToUnstick = WallStickTime;
                }
            }
            else
            {
                wallTimeToUnstick = WallStickTime;
            }
        }

        /* Jumping */
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (wallSliding)
            {
                // Jump when moving towards the wall
                if (wallDirX == input.x)
                {
                    velocity.x = -wallDirX * WallJumpClimb.x;
                    velocity.y = WallJumpClimb.y;
                }
                else if (input.x == 0)
                {
                    velocity.x = -wallDirX * WallJumpOff.x;
                    velocity.y = WallJumpOff.y;
                } 
                else 
                {
                    velocity.x = -wallDirX * WallJumpLeap.x;
                    velocity.y = WallJumpLeap.y;
                }
            }
            else if (controller.collisions.below)
            {
                velocity.y = JumpSpeed;
            }
        }

        // Short jump
        if (Input.GetKeyUp(KeyCode.Space) && !controller.collisions.below && velocity.y > 0)
        {
            velocity.y *= 0.5f;
            // if (velocity.y > MinJumpSpeed)
            //     velocity.y = MinJumpSpeed;
        }

        velocity.y += Gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime, input);

        if (controller.collisions.above || controller.collisions.below)
        {
            velocity.y = 0;
        }
    }
}
