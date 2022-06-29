using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarFollow : MonoBehaviour {
    [SerializeField] private GameObject car;
    private void Start() {
        
    }

    private void FixedUpdate() {
        transform.position = new Vector3(car.transform.position.x, car.transform.position.y, car.transform.position.z + 3.137f);
    }
}
