using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectLauncher : MonoBehaviour {
    [SerializeField] private int vertForceMultiplier = 50;
    [SerializeField] private int horizForceMultiplier = 10;
    [SerializeField] private int vertRandomness = 20;
    [SerializeField] private int horizRandomness = 5;
    [SerializeField] private bool hasCollided = false;
    private Rigidbody rb;
    private void Start() {
        rb = GetComponent<Rigidbody>();
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
        else if (collision.gameObject.tag == "Ground") {
            hasCollided = false;
        }
    }
}
