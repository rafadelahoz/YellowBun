using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Controller2D))]
public class Player : MonoBehaviour {

    float Gravity = -20;
    float JumpSpeed = 8;

    public float JumpHeight = 4;
    public float TimeToApex = 0.4f;

    public float MoveSpeed = 6;
    public float accelerationTimeAirborne = 0.2f;
    public float accelerationTimeGrounded = 0.1f;

    Vector3 velocity;
    float hspeedSmoothing;

    Controller2D controller;

	void Start() 
    {
        controller = GetComponent<Controller2D>();

        Gravity = -(2 * JumpHeight) / Mathf.Pow(TimeToApex, 2);
        JumpSpeed = Mathf.Abs(Gravity) * TimeToApex;

        print("G: " + Gravity + "; speed: " + JumpSpeed);
    }

    void Update()
    {
        if (controller.collisions.above || controller.collisions.below)
        {
            velocity.y = 0;
        }

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (Input.GetKeyDown(KeyCode.Space) && controller.collisions.below)
        {
            velocity.y = JumpSpeed;
        }

        if (Input.GetKeyUp(KeyCode.Space) && !controller.collisions.below)
        {
            velocity.y *= 0.5f;
        }

        float targetHspeed  = input.x * MoveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetHspeed, ref hspeedSmoothing, (controller.collisions.below ? accelerationTimeGrounded : accelerationTimeAirborne));
        velocity.y += Gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }
}
