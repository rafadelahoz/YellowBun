using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetCamera : MonoBehaviour {

    public Transform target;
    public float LerpDelta = 0.6f;

	void Update () {
        if (target != null)
        {
            Vector3 targetPos = new Vector3();
            targetPos.x = Mathf.Lerp(transform.position.x, target.position.x, LerpDelta);
            targetPos.y = Mathf.Lerp(transform.position.y, target.position.y, LerpDelta);
            targetPos.z = transform.position.z;
            transform.position = targetPos;
        }
	}
}
