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

    bool wallSliding;
    int wallDirX;

    public Vector2 velocity;
    float hspeedSmoothing;

    Vector2 directionalInput;

    Controller2D controller;
    Animator animator;

	void Start() 
    {
        controller = GetComponent<Controller2D>();
        animator = GetComponent<Animator>();

        Gravity = -(2 * JumpHeight) / Mathf.Pow(TimeToApex, 2);
        JumpSpeed = Mathf.Abs(Gravity) * TimeToApex;
        // MinJumpSpeed = Mathf.Sqrt(2 * Mathf.Abs(Gravity) * MinJumpHeight);
        print("G: " + Gravity + "; speed: " + JumpSpeed);
    }

    void Update()
    {
        CalculateVelocity();
        HandleWallSliding();

        // Jumping was here
        // Short jump was here

        controller.Move(velocity * Time.deltaTime, directionalInput);

        if (velocity.y > 0)
        {
            animator.Play("Jump");
        }
        else if (velocity.y < 0 && !controller.collisions.below)
        {
            animator.Play("Fall");
        }
        else if (Mathf.Abs(velocity.x) > float.Epsilon)
        {
            animator.Play("BunWalk");
        }
        else
        {
            animator.Play("Idle");
        }

        // Flip when moving left
        if (velocity.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (velocity.x > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }

        if (controller.collisions.above || controller.collisions.below)
        {
            velocity.y = 0;
        }
    }

    public void SetDirectionalInput(Vector2 input)
    {
        directionalInput = input;
    }

    public void OnJumpButtonDown()
    {
        if (wallSliding)
        {
            // Jump when moving towards the wall
            if (wallDirX == directionalInput.x)
            {
                velocity.x = -wallDirX * WallJumpClimb.x;
                velocity.y = WallJumpClimb.y;
            }
            else if (directionalInput.x == 0)
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

    public void OnJumpButtonUp()
    {
        if (!controller.collisions.below && velocity.y > 0)
        {
            velocity.y *= 0.5f;
            // if (velocity.y > MinJumpSpeed)
            //     velocity.y = MinJumpSpeed;
        }
    }

    void CalculateVelocity()
    {
        float targetHspeed = directionalInput.x * MoveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetHspeed, ref hspeedSmoothing, (controller.collisions.below ? accelerationTimeGrounded : accelerationTimeAirborne));
        velocity.y += Gravity * Time.deltaTime;

        // Avoid really small floats
        if (Mathf.Abs(velocity.x) < 0.3) {
            velocity.x = 0;
        }
    }

    void HandleWallSliding()
    {
        wallDirX = (controller.collisions.left ? -1 : 1);

        /* Wall Sliding / Jumping section */
        wallSliding = false;
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

                if (directionalInput.x != wallDirX && directionalInput.x != 0)
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
    }
}
