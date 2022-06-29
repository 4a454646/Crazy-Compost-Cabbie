using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectLauncher : MonoBehaviour {
    [SerializeField] private float vertForceMultiplier = 10;
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
            rb.AddForce(Vector3.up * collision.impulse.magnitude * vertForceMultiplier);
        }
        else if (collision.gameObject.tag == "Ground") {
            hasCollided = false;
        }
    }
}
