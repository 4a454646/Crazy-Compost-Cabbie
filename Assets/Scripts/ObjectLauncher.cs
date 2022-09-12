using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectLauncher : MonoBehaviour {
    [SerializeField] private int vertForceMultiplier = 50;
    [SerializeField] private int horizForceMultiplier = 10;
    [SerializeField] private int vertRandomness = 20;
    [SerializeField] private int horizRandomness = 5;
    [SerializeField] private bool hasCollided = false;
    [SerializeField] private bool specialTriggered = false;
    [SerializeField] private GameObject specialObject;
    [SerializeField] private CarController carController;
    private Colors colors;
    private enum SpecialBehavior { 
        Normal,
        Streetlight,
        Trashcan,
        Hydrant,
        TelephonePole,
        Tree0,
        Tree1,
        Cone,
        Bin, 
        Car,
        StopSign,
    }
    [SerializeField] private SpecialBehavior behaviorType = SpecialBehavior.Normal;
    private Rigidbody rb;
    private void Start() {
        colors = FindObjectOfType<Colors>();
        rb = GetComponent<Rigidbody>();
        if (behaviorType == SpecialBehavior.Streetlight) {
            specialObject.GetComponent<Light>().enabled = true;
        }
        carController = FindObjectOfType<CarController>();
        if (Time.time < 1) {
            carController.numDestroyableObjects++;
        }
    }

    private void Update() {
        
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.tag == "Player" && !hasCollided) {
            hasCollided = true;
            float sqrtMagnitude = Mathf.Sqrt(collision.impulse.magnitude);
            // prevents collisions from getting too crazy at higher speeds
            rb.AddForce(new Vector3(
                Random.Range(
                    -sqrtMagnitude * (horizForceMultiplier + horizRandomness), 
                    sqrtMagnitude * (horizForceMultiplier + horizRandomness)
                ),
                Random.Range(
                    sqrtMagnitude * (vertForceMultiplier - vertRandomness), 
                    sqrtMagnitude * (vertForceMultiplier + vertRandomness)
                ),
                Random.Range(
                    -sqrtMagnitude * (horizForceMultiplier + horizRandomness), 
                    sqrtMagnitude * (horizForceMultiplier + horizRandomness)
                )
            ));
        }
        if (collision.gameObject.tag == "Player") {
            // switch for the different SpecialBehaviors
            if (!specialTriggered) { 
                print($"should say destroyed {gameObject.name}");
                specialTriggered = true;
                carController.numDestroyedObjects++;
                switch (behaviorType) {
                    case SpecialBehavior.Streetlight:
                        StartCoroutine(FlickerLight());
                        carController.PrintText("Streetlight", colors.white);
                        break;
                    case SpecialBehavior.Trashcan:
                        carController.PrintText("Trash Can", colors.green);
                        break;
                    case SpecialBehavior.Hydrant:
                        carController.PrintText("Fire Hydrant", colors.red);
                        break;
                    case SpecialBehavior.TelephonePole:
                        carController.PrintText("Telephone Pole", colors.brown);
                        break;
                    case SpecialBehavior.Tree0:
                        for (int i = 0; i < specialObject.transform.childCount; i++) {
                            GameObject child = specialObject.transform.GetChild(i).gameObject;
                            child.AddComponent<Rigidbody>();
                            child.GetComponent<Rigidbody>().mass = 100;
                            child.AddComponent<ObjectLauncher>();
                            child.GetComponent<ObjectLauncher>().vertForceMultiplier = 1000;
                            child.AddComponent<ObjectLauncher>();
                        }
                        carController.PrintText("Tree", colors.green);
                        break;
                    case SpecialBehavior.Tree1:
                        carController.PrintText("Tree", colors.green);
                        break;
                    case SpecialBehavior.Cone:
                        carController.PrintText("Traffic Cone", colors.orange);
                        break;
                    case SpecialBehavior.Bin:
                        carController.PrintText("Compost Bin", colors.yellow);
                        break;
                    case SpecialBehavior.Car:
                        carController.PrintText("Car", colors.blue);
                        break;
                    case SpecialBehavior.StopSign:
                        carController.PrintText("Stop Sign", colors.red);
                        break;
                    default:
                        break;
                }
            }
        }
        else {
            hasCollided = false;
        }
    }

    private IEnumerator FlickerLight() { 
        Light light = specialObject.GetComponent<Light>();
        light.enabled = false;
        yield return new WaitForSeconds(0.01f);
        light.enabled = true;
        yield return new WaitForSeconds(0.01f);
        light.enabled = false;
        yield return new WaitForSeconds(0.03f);
        light.enabled = true;
        yield return new WaitForSeconds(0.01f);
        light.enabled = false;
    }
}
