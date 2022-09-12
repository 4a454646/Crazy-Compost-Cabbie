using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {
    
    public float vertical;
    public float horizontal;
    public bool breaking;
    private bool preventActions = true;
    // todo: lock actions while start animation is playing
    
    
    private void Start() {
        StartCoroutine(LockStart());
    }

    private IEnumerator LockStart() {
        yield return new WaitForSeconds(3f);
        FindObjectOfType<CarController>().hasStarted = true;
        preventActions = false;
    }

    private void Update() {
        if (!preventActions) {
            vertical = Input.GetAxis("Vertical");
            horizontal = Input.GetAxis("Horizontal");
            breaking = Input.GetAxis("Jump") == 0 ? false : true;
        }
    }
}
