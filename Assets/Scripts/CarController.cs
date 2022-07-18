using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CarController : MonoBehaviour {
    [SerializeField] private WheelCollider[] wheelColliders;
    [SerializeField] private GameObject[] wheelMeshes;
    [SerializeField] private float torque = 200;
    private enum DriveType {
        Front,
        Rear,
        All
    }
    [SerializeField] private DriveType driveType = DriveType.Front;
    [SerializeField] private float radius = 6f;
    [SerializeField] private float downForce = 50;
    [SerializeField] private GameObject centerOfMass;
    [SerializeField] public float speed;
    [SerializeField] private float brakeTorque;
    [SerializeField] private float[] slip = new float[4];
    private Rigidbody rb;
    private InputManager inputManager;

    private void Start() {
        inputManager = FindObjectOfType<InputManager>();
        rb = GetComponent<Rigidbody>(); 
        rb.centerOfMass = centerOfMass.transform.localPosition;
    }

    private void FixedUpdate() { 
        AnimatewheelColliders();
        SteerVehicle();
        MoveVehicle();
        AddDownForce();
    }

    private void MoveVehicle() { 
        if (!inputManager.breaking) {
            if (driveType == DriveType.Front) {
                wheelColliders[0].motorTorque = inputManager.vertical * torque;
                wheelColliders[1].motorTorque = inputManager.vertical * torque;
            } else if (driveType == DriveType.Rear) {
                wheelColliders[2].motorTorque = inputManager.vertical * torque;
                wheelColliders[3].motorTorque = inputManager.vertical * torque;
            } else {
                wheelColliders[0].motorTorque = inputManager.vertical * torque;
                wheelColliders[1].motorTorque = inputManager.vertical * torque;
                wheelColliders[2].motorTorque = inputManager.vertical * torque;
                wheelColliders[3].motorTorque = inputManager.vertical * torque;
            }
            wheelColliders[2].brakeTorque = 0;
            wheelColliders[3].brakeTorque = 0;
        }
        else { 
            wheelColliders[2].brakeTorque = brakeTorque;
            wheelColliders[3].brakeTorque = brakeTorque;
        }
        speed = rb.velocity.magnitude * 2.24f;
    }

    private void SteerVehicle() {
        // ackerman steering
        if (inputManager.horizontal > 0) {
            wheelColliders[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius + (1.5f / 2))) * inputManager.horizontal;
            wheelColliders[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius - (1.5f / 2))) * inputManager.horizontal;
        }
        else if (inputManager.horizontal < 0) {
            wheelColliders[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius - (1.5f / 2))) * inputManager.horizontal;
            wheelColliders[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius + (1.5f / 2))) * inputManager.horizontal;
        }
        else {
            wheelColliders[0].steerAngle = 0;
            wheelColliders[1].steerAngle = 0;
        }
    }

    private void AddDownForce() { 
        rb.AddForce(-transform.up * downForce * rb.velocity.magnitude);
    }

    private void AnimatewheelColliders() {
        Vector3 wheelPosition = Vector3.zero;
        Quaternion wheelRotation = Quaternion.identity;

        for (int i = 0; i < wheelColliders.Length; i++) {
            wheelColliders[i].GetWorldPose(out wheelPosition, out wheelRotation);
            wheelMeshes[i].transform.position = wheelPosition;
            wheelMeshes[i].transform.rotation = wheelRotation;
        }
    }

    private void GetFriction() {
        for (int i = 0; i < wheelColliders.Length; i++) {
            WheelHit hit;
            wheelColliders[i].GetGroundHit(out hit);
            slip[i] = hit.sidewaysSlip;
        }
    }
}
