using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CarController : MonoBehaviour {
    [SerializeField] private WheelCollider[] wheelColliders;
    [SerializeField] private GameObject[] wheelMeshes;
    [SerializeField] private float torque = 200;
    [SerializeField] private float steeringMax = 4;

    private void Start() {
        
    }

    private void FixedUpdate() { 
        AnimatewheelColliders();
        if (Input.GetKey(KeyCode.W)) {
            foreach (WheelCollider wheel in wheelColliders) {
                wheel.motorTorque = torque;
            }
        } 

        if (Input.GetAxis("Horizontal") != 0) {
            wheelColliders[0].steerAngle = Input.GetAxis("Horizontal") * steeringMax;
            wheelColliders[1].steerAngle = Input.GetAxis("Horizontal") * steeringMax;
        }
        else {
            for (int i = 0; i < wheelColliders.Length; i++) {
                wheelColliders[i].steerAngle = 0;
            }
        }
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
}
