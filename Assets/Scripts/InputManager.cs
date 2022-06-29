using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {
    
    public float vertical;
    public float horizontal;
    public bool breaking;
    
    private void Start() {
        
    }

    private void Update() {
        vertical = Input.GetAxis("Vertical");
        horizontal = Input.GetAxis("Horizontal");
        breaking = Input.GetAxis("Jump") == 0 ? false : true;
    }
}
