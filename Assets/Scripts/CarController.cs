using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class CarController : MonoBehaviour {
    [SerializeField] private WheelCollider[] wheelColliders;
    [SerializeField] private GameObject[] wheelMeshes;
    [SerializeField] private WheelSkid[] wheelSkids;
    [SerializeField] private float torque = 200;
    [SerializeField] private float torqueSlipMultiplier;
    [SerializeField] private int maxSpeed = 75;
    [SerializeField] private float curTorque = 0;
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
    [SerializeField] public bool hasStarted = false;
    [SerializeField] private ParticleSystem[] dust;
    [SerializeField] public int numDestroyableObjects;
    [SerializeField] public int numDestroyedObjects;
    [SerializeField] private TextMeshProUGUI destroyedText;
    [SerializeField] private TextMeshProUGUI destroyedTextColored;
    [SerializeField] private List<String> prints = new List<String>();
    [SerializeField] private float messageDuration = 3f;
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
        binsCollectedText.text = $"{binsCollected}/{bins.Length}";
        for (int i = 0; i < 4; i++) {
            dust[i].Stop();
        }
    }

    public void UpdateCompost() {
        binsCollected += 1;
        print($"A bin was collected! The user now has {binsCollected} bins collected.");
        binsCollectedText.text = $"{binsCollected}/{bins.Length}";
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
            timer += Time.deltaTime;
            TimeSpan time = TimeSpan.FromSeconds(timer);
            timerText.text = time.ToString(@"mm\:ss\.ff");
        }
        UpdateSpeedometer();
    }

    private void FixedUpdate() { 
        AnimateWheelColliders();
        SteerVehicle();
        MoveVehicle();
        AddDownForce();
    }

    private void MoveVehicle() { 
        if (!inputManager.breaking && !lockActions) {
            if (IsSkidding()) { curTorque = torque * torqueSlipMultiplier; }
            else { curTorque = torque; }
            if (speed > maxSpeed ) { curTorque = 0; }
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
                wheelColliders[0].motorTorque = inputManager.vertical * curTorque;
                wheelColliders[1].motorTorque = inputManager.vertical * curTorque;
            } else if (driveType == DriveType.Rear) {
                wheelColliders[2].motorTorque = inputManager.vertical * curTorque;
                wheelColliders[3].motorTorque = inputManager.vertical * curTorque;
            } else {
                wheelColliders[0].motorTorque = inputManager.vertical * curTorque;
                wheelColliders[1].motorTorque = inputManager.vertical * curTorque;
                wheelColliders[2].motorTorque = inputManager.vertical * curTorque;
                wheelColliders[3].motorTorque = inputManager.vertical * curTorque;
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
    }

    private void UpdateSpeedometer() {
        // at 0 mph: z angle is 105
        // at 70 mph: z angle is -105
        // rotate needle to match speed
        float angle = 105 - (speed / 70) * 105 * 2;
        // interpolate the rotation towards the new angle
        needle.transform.rotation = Quaternion.Lerp(needle.transform.rotation, Quaternion.Euler(0, 0, angle), Time.deltaTime * 3);
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

    private bool IsSkidding() {
        foreach (WheelSkid skid in wheelSkids) {
            if (skid.isSkidding) {
                return true;
            }
        }
        return false;
    }

    public void PrintText(String destroyed, Color color) {
        StartCoroutine(PrintTextCoro(destroyed, color));
        StartCoroutine(RemoveTextCoro());
    }

    private IEnumerator PrintTextCoro(String destroyed, Color color) {
        prints.Add(destroyed);
        yield return new WaitForSeconds(0.01f);
        destroyedTextColored.color = color;
        string normalString = "";
        string coloredString = "";
        foreach (string toPrint in prints) {
            normalString += $"Destroyed {toPrint} ({numDestroyedObjects / numDestroyableObjects * 100}%)\n";
            coloredString += $"     {toPrint}\n";
        }
        destroyedText.text = normalString;
        destroyedTextColored.text = coloredString;
    }

    private IEnumerator RemoveTextCoro() {
        yield return new WaitForSeconds(messageDuration);
        prints.RemoveAt(0);
    }
}
