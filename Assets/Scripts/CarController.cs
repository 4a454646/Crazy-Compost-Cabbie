using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour {

    private const string HORIZONTAL = "Horizontal";
    private const string VERTICAL = "Vertical";
    [SerializeField] private float horizontalInput;
    [SerializeField] private float verticalInput;
    [SerializeField] private float motorForce = 3000f;
    [SerializeField] private bool isBreaking = false;
    [SerializeField] private float breakForce = 1000f;
    [SerializeField] private float curBreakForce = 0f;
    [SerializeField] private float curSteerAngle;
    [SerializeField] private float maxSteerAngle = 30f;
    [SerializeField] private WheelCollider frontLeftCollider;
    [SerializeField] private WheelCollider frontRightCollider;
    [SerializeField] private WheelCollider backLeftCollider;
    [SerializeField] private WheelCollider backRightCollider;
    [SerializeField] private Transform frontLeftTransform;
    [SerializeField] private Transform frontRightTransform;
    [SerializeField] private Transform backLeftTransform;
    [SerializeField] private Transform backRightTransform;

    private void Start() {
        
    }

    private void FixedUpdate() {
        GetInput();
        HandleMotor();
        HandleSteering();
        UpdateWheels();
    }

    private void HandleMotor() {
        frontLeftCollider.motorTorque = verticalInput * motorForce;
        frontRightCollider.motorTorque = verticalInput * motorForce;
        curBreakForce = isBreaking ? breakForce : 0f;
        if (isBreaking) { ApplyBreaking(); }
        else {
            frontLeftCollider.brakeTorque = 0f;
            frontRightCollider.brakeTorque = 0f;
            backLeftCollider.brakeTorque = 0f;
            backRightCollider.brakeTorque = 0f;
        }
    }

    private void ApplyBreaking() {
        frontLeftCollider.brakeTorque = curBreakForce;
        frontRightCollider.brakeTorque = curBreakForce;
        backLeftCollider.brakeTorque = curBreakForce;
        backRightCollider.brakeTorque = curBreakForce;
    }

    private void GetInput() {
        horizontalInput = Input.GetAxis(HORIZONTAL);
        verticalInput = Input.GetAxis(VERTICAL);
        isBreaking = Input.GetKey(KeyCode.Space);
    }

    private void HandleSteering() { 
        curSteerAngle = horizontalInput * maxSteerAngle;
        frontLeftCollider.steerAngle = curSteerAngle;
        frontRightCollider.steerAngle = curSteerAngle;
    }

    private void UpdateWheels() { 
        UpdateSingleWheel(frontLeftCollider, frontLeftTransform);
        UpdateSingleWheel(frontRightCollider, frontRightTransform);
        UpdateSingleWheel(backLeftCollider, backLeftTransform);
        UpdateSingleWheel(backRightCollider, backRightTransform);
    }

    private void UpdateSingleWheel(WheelCollider collider, Transform transform) {
        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);
        transform.position = position;
        transform.rotation = rotation;
    }
}
