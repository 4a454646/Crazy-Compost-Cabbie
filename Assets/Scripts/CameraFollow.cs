using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {
    [SerializeField] private Vector3 offset;
    [SerializeField] private Transform target;
    [SerializeField] private float translateSpeed;
    [SerializeField] private float rotationSpeed;
    private CarController carController;

    private void Start() {
        carController = FindObjectOfType<CarController>();
    }

    private void FixedUpdate() {
        if (carController.speed > 50) {
            translateSpeed = 5;
        }
        else if (carController.speed > 20) {
            translateSpeed = (80 - carController.speed)/6;
        }
        else {
            translateSpeed = 10;
        }
        HandleTranslation();
        HandleRotation();
    }
   
    private void HandleTranslation() {
        var targetPosition = target.TransformPoint(offset);
        transform.position = Vector3.Lerp(transform.position, targetPosition, translateSpeed * Time.deltaTime);
    }
    
    private void HandleRotation() {
        var direction = target.position - transform.position;
        var rotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
    }
}