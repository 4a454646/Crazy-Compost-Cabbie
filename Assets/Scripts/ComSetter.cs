using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComSetter : MonoBehaviour {
    [SerializeField] Transform com;
    private void Start() {
        GetComponent<Rigidbody>().centerOfMass = com.localPosition;
    }
}
