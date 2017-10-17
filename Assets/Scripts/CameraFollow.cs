using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

    public Controller2D target;
    public Vector2 focusAreaSize;
    public float LookAheadDistanceX;
    public float HorizontalSmoothTime;
    public float VerticalSmoothTime;
    public float VerticalOffset;

    FocusArea focusArea;

    float currentLookAhead;
    float targetLookAhead;
    float lookAheadDirectionX;
    float smoothLookVelocityX;
    float smoothVelocityY;

    bool lookAheadStopped;

    void Start()
    {
        focusArea = new FocusArea(target.collider2d.bounds, focusAreaSize);
    }

    private void LateUpdate()
    {
        focusArea.Update(target.collider2d.bounds);
        Vector2 focusPosition = focusArea.center + Vector2.up * VerticalOffset;

        if (focusArea.velocity.x != 0)
        {
            lookAheadDirectionX = Mathf.Sign(focusArea.velocity.x);
            if (target.playerInput.x != 0 && Mathf.Sign(target.playerInput.x) == Mathf.Sign(focusArea.velocity.x)) {
                lookAheadStopped = false;
                targetLookAhead = lookAheadDirectionX * LookAheadDistanceX;
            } else {
                if (!lookAheadStopped)
                {
                    lookAheadStopped = true;
                    targetLookAhead = currentLookAhead + (lookAheadDirectionX * LookAheadDistanceX - currentLookAhead) / 4f;
                }
            }
        }

        focusPosition.y = Mathf.SmoothDamp(transform.position.y, focusPosition.y, ref smoothVelocityY, VerticalSmoothTime);
        currentLookAhead = Mathf.SmoothDamp(currentLookAhead, targetLookAhead, ref smoothLookVelocityX, HorizontalSmoothTime);
        focusPosition += Vector2.right * currentLookAhead;

        transform.position = (Vector3)focusPosition + Vector3.forward * -10;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, .3f);
        Gizmos.DrawCube(focusArea.center, focusAreaSize);
    }

    struct FocusArea
    {
        public Vector2 center;
        public Vector2 velocity;

        float left, right;
        float top, bottom;

        public FocusArea(Bounds targetBounds, Vector2 size)
        {
            left = targetBounds.center.x - size.x / 2;
            right = targetBounds.center.x + size.x / 2;
            bottom = targetBounds.min.y;
            top = targetBounds.min.y + size.y;

            center = new Vector2((left + right) / 2, (top + bottom) / 2);
            velocity = Vector2.zero;
        }

        public void Update(Bounds targetBounds)
        {
            float shiftX = 0;
            if (targetBounds.min.x < left)
                shiftX = targetBounds.min.x - left;
            else if (targetBounds.max.x > right)
                shiftX = targetBounds.max.x - right;

            left += shiftX;
            right += shiftX;

            float shiftY = 0;
            if (targetBounds.min.y < bottom)
                shiftY = targetBounds.min.y - bottom;
            else if (targetBounds.max.y > top)
                shiftY = targetBounds.max.y - top;

            top += shiftY;
            bottom += shiftY;

            center = new Vector2((left + right) / 2, (top + bottom) / 2);
            velocity = new Vector2(shiftX, shiftY);
        }
    }
}
