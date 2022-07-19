using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompostBin : MonoBehaviour {
    [SerializeField] public bool isAvailable = true;
    [SerializeField] private Outline outline;
    [SerializeField] private Color baseBinColor;
    [SerializeField] private Color availableBinColor;
    [SerializeField] private int baseOutlineWidth;
    [SerializeField] private int availableOutlineWidth;

    private void Start() {
        outline = GetComponent<Outline>();
    }

    private void Update() {
        
    }

    public void CollectBin(Vector3 collectionPoint) {
        StartCoroutine(CollectBinCoro(collectionPoint));
    }

    private IEnumerator CollectBinCoro(Vector3 collectionPoint) {
        Destroy(GetComponent<Rigidbody>());
        Destroy(GetComponent<MeshCollider>());
        Vector3 vertCollectionPoint = new Vector3(transform.position.x, collectionPoint.y, transform.position.z);
        for (int i = 0; i < 20; i++) {
            yield return new WaitForSeconds(0.0125f);
            transform.position = Vector3.Lerp(transform.position, vertCollectionPoint, i/20f);
        }
        for (int i = 0; i < 20; i++) {
            transform.position = Vector3.Lerp(transform.position, collectionPoint, i/20f);
            yield return new WaitForSeconds(0.025f);
        }
        for (int i = 0; i < 20; i++) {
            transform.localScale = new Vector3((2 * (19-i)/20f), (2 * (19-i)/20f), (2 * (19-i)/20f));
            yield return new WaitForSeconds(0.007f);
        }
        Destroy(gameObject);
        FindObjectOfType<CarController>().UpdateCompost();
    }

    public void TrySetAvailable() {
        if (!isAvailable) {
            isAvailable = true;
            outline.OutlineColor = availableBinColor;
            outline.OutlineWidth = availableOutlineWidth;
            // outline.OutlineMode = Outline.Mode.OutlineAll;
        }
    }

    public void TrySetUnavailable() {
        if (isAvailable) {
            isAvailable = false;
            outline.OutlineColor = baseBinColor;
            outline.OutlineWidth = baseOutlineWidth;
            // outline.OutlineMode = Outline.Mode.OutlineVisible;
        }
    }
}
