using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

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
    [SerializeField] private float binCollectionDist = 4f;
    [SerializeField] private float collectionSpeed = 4f;
    [SerializeField] private Transform collectionPoint;
    [SerializeField] private int binsCollected = 0;
    [SerializeField] private CompostBin[] bins;
    [SerializeField] public bool lockActions;
    [SerializeField] private List<Light> breakLights;
    [SerializeField] private List<Light> reverseLights;
    [SerializeField] private GameObject needle;
    [SerializeField] private TextMeshProUGUI binsCollectedText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private float timer;
    [SerializeField] private bool hasStarted = false;
    
    private Rigidbody rb;
    private InputManager inputManager;

    private void Start() {
        inputManager = FindObjectOfType<InputManager>();
        rb = GetComponent<Rigidbody>(); 
        rb.centerOfMass = centerOfMass.transform.localPosition;
        GameObject[] gameObjectBins = GameObject.FindGameObjectsWithTag("Bin");
        for (int i = 0; i < bins.Length; i++) {
            bins[i] = gameObjectBins[i].GetComponent<CompostBin>();
            bins[i].TrySetUnavailable();
        }
        binsCollectedText.text = $"{binsCollected} / {bins.Length}";
        StartCoroutine(AnimateStart());
    }

    private IEnumerator AnimateStart() {
        
    }

    public void UpdateCompost() {
        binsCollected += 1;
        print($"A bin was collected! The user now has {binsCollected} bins collected.");
        binsCollectedText.text = $"{binsCollected} / {bins.Length}";
    }

    private void Update() {
        if (speed < collectionSpeed) { 
            for (int i = 0; i < bins.Length; i++) {
                if (bins[i] != null) {
                    float dist = Vector3.Distance(transform.position, bins[i].transform.position);
                    if (dist < binCollectionDist) {
                        if (Input.GetMouseButtonDown(0)) {
                            bins[i].CollectBin(collectionPoint.position);
                        }
                        bins[i].TrySetAvailable();
                    }
                    else {
                        bins[i].TrySetUnavailable();
                    }
                }
            }
        }
        else {
            for (int i = 0; i < bins.Length; i++) {
                if (bins[i] != null) {
                    bins[i].TrySetUnavailable();
                }
            }
        }
        // format timer text to be 00:00.00
        if (hasStarted) {
            timer = Time.timeSinceLevelLoad;
            timerText.text = $"{Mathf.Floor(timer/60):00}:{Mathf.Floor(timer%60):00}.{Mathf.Floor((timer*100)%100):00}";
        }
    }


    private void FixedUpdate() { 
        AnimateWheelColliders();
        SteerVehicle();
        MoveVehicle();
        AddDownForce();
    }

    private void MoveVehicle() { 
        if (!inputManager.breaking && !lockActions) {
            foreach (Light light in breakLights) {
                light.enabled = false;
            }
            if (inputManager.vertical >= 0) {
                foreach (Light light in reverseLights) {
                    light.enabled = false;
                }
            }
            else {
                foreach (Light light in reverseLights) {
                    light.enabled = true;
                }
            }
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
            foreach (Light light in breakLights) {
                light.enabled = true;
            }
            wheelColliders[2].brakeTorque = brakeTorque;
            wheelColliders[3].brakeTorque = brakeTorque;
        }
        speed = rb.velocity.magnitude * 2.24f;
        UpdateSpeedometer();
    }

    private void UpdateSpeedometer() {
        // at 0 mph: z angle is 105
        // at 70 mph: z angle is 0105
        // rotate needle to match speed
        float angle = 105 - (speed / 70) * 105;
        needle.transform.localEulerAngles = new Vector3(0, 0, angle);
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

    private void AnimateWheelColliders() {
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
