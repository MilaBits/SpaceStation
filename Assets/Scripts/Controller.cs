using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour {
    private Rigidbody rigidbody;
    private Camera viewCamera;

    // Movement
    public float MoveSpeed = 5f;
    private Vector3 velocity;

    // Camera
    public float RotationSpeed = .5f;
    private Vector3 desiredDirection = Vector3.forward;
    private readonly CameraOrientation orientation = new CameraOrientation();

    void Start() {
        rigidbody = GetComponent<Rigidbody>();
        viewCamera = GetComponentInChildren<Camera>();
    }

    void Update() {
        if (Input.GetButtonDown("Rotate Camera Left"))
            desiredDirection = orientation.PreviousOrientation();
        if (Input.GetButtonDown("Rotate Camera Right"))
            desiredDirection = orientation.NextOrientation();

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(desiredDirection),
            Time.deltaTime * RotationSpeed);

        Vector3 direction = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        velocity = direction.normalized * Time.deltaTime * MoveSpeed;
    }

    private void FixedUpdate() {
        transform.Translate(velocity);
    }

    private class CameraOrientation {
        private int i;

        private readonly List<Vector3> Orientations = new List<Vector3>() {
            new Vector3(0, 0, 0),
            new Vector3(0, 90, 0),
            new Vector3(0, 180, 0),
            new Vector3(0, 270, 0)
        };

        public Vector3 NextOrientation() {
            if (i < Orientations.Count-1) i++;
            else i = 0;
            return Orientations[i];
        }

        public Vector3 PreviousOrientation() {
            if (i > 0) i--;
            else i = Orientations.Count-1;
            return Orientations[i];
        }
    }
}