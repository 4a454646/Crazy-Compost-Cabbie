using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorGenerator : MonoBehaviour {
    [SerializeField] private Color[] colors;
    [SerializeField] private List<GameObject> cars;
    private void Start() {
        foreach (GameObject car in cars) {
            Color randColor = colors[Random.Range(0, colors.Length)];
            for (int i = 0; i < car.transform.childCount; i++) {
                if (car.transform.GetChild(i).GetComponent<Renderer>().materials.Length > 1) {
                    for (int j = 0; j < car.transform.GetChild(i).GetComponent<Renderer>().materials.Length; j++) {
                        if (car.transform.GetChild(i).GetComponent<Renderer>().materials[j].name.Contains("001")) {
                            car.transform.GetChild(i).GetComponent<Renderer>().materials[j].color = randColor;
                        }
                    }
                }
            }
        }
        // car is weird, need to look through all the materials and select the '001' material to change the red
    }

    private void Update() {
        
    }
}
