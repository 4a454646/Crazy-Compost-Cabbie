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
    private enum SpecialBehavior { 
        Normal,
        Streetlight,
        Trashcan,
        Hydrant,
        TelephonePole,
        Tree0,
    }
    [SerializeField] private SpecialBehavior behaviorType = SpecialBehavior.Normal;
    private Rigidbody rb;
    private void Start() {
        rb = GetComponent<Rigidbody>();
        if (behaviorType == SpecialBehavior.Streetlight) {
            specialObject.GetComponent<Light>().enabled = true;
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
        if (collision.gameObject.tag != "Ground") {
            // switch for the different SpecialBehaviors
            if (!specialTriggered) { 
                specialTriggered = true;
                switch (behaviorType) {
                    case SpecialBehavior.Streetlight:
                        StartCoroutine(FlickerLight());
                        break;
                    case SpecialBehavior.Trashcan:
                        break;
                    case SpecialBehavior.Hydrant:
                        break;
                    case SpecialBehavior.TelephonePole:
                        break;
                    case SpecialBehavior.Tree0:
                        for (int i = 0; i < specialObject.transform.childCount; i++) {
                            GameObject child = specialObject.transform.GetChild(i).gameObject;
                            child.AddComponent<Rigidbody>();
                            child.GetComponent<Rigidbody>().mass = 100;
                            child.AddComponent<ObjectLauncher>();
                            child.GetComponent<ObjectLauncher>().vertForceMultiplier = 500;
                        }
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
