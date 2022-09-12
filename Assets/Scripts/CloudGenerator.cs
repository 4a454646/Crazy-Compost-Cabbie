using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;

public class CloudData{
	public Vector3 pos;
    public Vector3 scale;
    public Quaternion rot;
    public bool isActive;

	public int x;
    public int y;
    public float distFromCam;
    public float randomMaxSize;
    public float randomGrowthSpeed;
    public float randomOffset;

	//Returns the Matrix4x4 of our cloud
    public Matrix4x4 matrix {
        get {
            return Matrix4x4.TRS(pos, rot, scale);
        }
    }

	//Used to instanciate our cloud
    public CloudData(Vector3 pos, Vector3 scale, Quaternion rot, int x, int y, float distFromCam, float randomMinSize, float randomMaxSize, float minGrowthMultiplier, float maxGrowthMultiplier, float initialGrowthMultiplier){
		this.pos = pos;
        this.scale = scale;
        this.rot = rot;
        this.isActive = true;
        this.x = x;
        this.y = y;
        this.randomMaxSize = Random.Range(randomMinSize, randomMaxSize);
        this.randomGrowthSpeed = Random.Range(minGrowthMultiplier, maxGrowthMultiplier) * initialGrowthMultiplier;
	}
}

public class CloudGenerator : MonoBehaviour {
	//Meshes
    [SerializeField] private Mesh cloudMesh;
    [SerializeField] private Material cloudMat;

	//Cloud Data
    [SerializeField] private float cloudSize = 5;

	//Noise Generation
    [SerializeField] private float timeScale = 1;
    [SerializeField] private float texScale = 1;

	//Cloud Scaling Info
    [SerializeField] private float minNoiseSize = 0.5f;

	//Culling Data
    [SerializeField] private Camera cam;
    [SerializeField] private int maxDist;

	//Number of batches
    [SerializeField] private int batchesToCreate;
    [SerializeField] private int cloudCount = 7;

	private Vector3 prevCamPos;
    private float offsetX = 1;
    private float offsetY = 1;
    [SerializeField] private float randomMinSize;
    [SerializeField] private float randomMaxSize;
    [SerializeField] private float randomOffset;
    [SerializeField] private float shrinkSpeed;
    [SerializeField] private float minGrowthMultiplier;
    [SerializeField] private float maxGrowthMultiplier;
    [SerializeField] private float cloudHeightVariation;
    [SerializeField] private float initialGrowthRate;

    private List<List<CloudData>> batches = new List<List<CloudData>>();
    private List<List<CloudData>> batchesToUpdate = new List<List<CloudData>>();

    // TODO: CHECK IF THE CLOUDS ARE IN FRONT OR BEHIND OF THE PLAYER (VERSUS DISTANCE)
    
	private void Start() {
		for(int batchesX = 0; batchesX < batchesToCreate; batchesX++) {
			for(int batchesY = 0; batchesY < batchesToCreate; batchesY++) {
				BuildCloudBatch(batchesX, batchesY);
			}
		}
        offsetX = Random.Range(0, 1000);
        offsetY = Random.Range(0, 1000);
        StartCoroutine(UpdateCloudGrowthRates());
	}

    private IEnumerator UpdateCloudGrowthRates() {
        yield return new WaitForSeconds(1f);
        foreach (var batch in batchesToUpdate) {
            foreach (var cloud in batch) {
                cloud.randomGrowthSpeed /= initialGrowthRate;
            }
        }
    }

	//We start by looping though our X and Y values to generate a batch that's 7x7 clouds
    //Limited due to 1024 max of Graphics.DrawMeshInstanciated
    private void BuildCloudBatch(int xLoop, int yLoop) {
		//Mark a batch if it's within rage of our camera
        bool markBatch = false;
		//This is our current cloud batch that we're brewing
        List<CloudData> currBatch = new List<CloudData>();

		for (int x = 0; x < cloudCount; x++) {
            for (int y = 0; y < cloudCount; y++) {
                //Add a cloud for each loop
                AddCloud(currBatch, x + xLoop * cloudCount, y + yLoop * cloudCount);
            }
        }

		//Check if the batch should be marked
        markBatch = CheckForActiveBatch(currBatch);

		//Add the newest batch to the batches list
        batches.Add(currBatch);

		//If the batch is marked add it to the batchesToUpdate list
        if (markBatch) batchesToUpdate.Add(currBatch);
	}

	//This method checks to see if the current batch has a cloud that is witin our cameras range
    //Return true if a cloud is within range
    //Return false if no clouds are within range
    private bool CheckForActiveBatch(List<CloudData> batch) {
		foreach (var cloud in batch) {
            cloud.distFromCam = Vector3.Distance(cloud.pos, cam.transform.position);
            if (cloud.distFromCam < maxDist) return true;
        }
        return false;
	}

	//This method created our clouds as a CloudData object
    private void AddCloud(List<CloudData> currBatch, int x, int y) {
		//First we set our new clouds position
        Vector3 randVector = new Vector3(
            Random.Range(-randomOffset, randomOffset),
            Random.Range(-randomOffset, randomOffset),
            Random.Range(-randomOffset, randomOffset)
        );
        Vector3 position = new Vector3(
            transform.position.x + x * cloudSize, 
            transform.position.y,
            transform.position.z + y * cloudSize
        );
        
        Vector3 alteredPosition = position + randVector;

		//We set our new clouds distance to the camera so we can use it later
        float distToCam = Vector3.Distance(new Vector3(x, transform.position.y, y), cam.transform.position);

		//Finally we add our new CloudData cloud to the current batch
        currBatch.Add(new CloudData(alteredPosition, Vector3.zero, Random.rotation, x, y, distToCam, randomMinSize, randomMaxSize, minGrowthMultiplier, maxGrowthMultiplier, initialGrowthRate));
	}

	//We need to generate our noise
    //We update our offsets to the noise 'rolls' through the cloud objects
	private void Update() {
        MakeNoise();
        offsetX += Time.deltaTime * timeScale;
        offsetY += Time.deltaTime * timeScale;
        // clear all lists and redraw all clouds if the period button is pressed
        if (Input.GetKeyDown(KeyCode.Period)) {
            print("resetting!");
            batches.Clear();
            batchesToUpdate.Clear();
            for(int batchesX = 0; batchesX < batchesToCreate; batchesX++) {
                for(int batchesY = 0; batchesY < batchesToCreate; batchesY++) {
                    BuildCloudBatch(batchesX, batchesY);
                }
            }
            StartCoroutine(UpdateCloudGrowthRates());
        }
    }

	//This method updates our noise/clouds
    //First we check to see if the camera has moved
    //If it hasn't we update batches
    //If it has moved we need to reset the prevCamPos along with updating our batch list before updating our batches
    //TODO: Set allowed movement range to camera so the player can move a small amount without causing a full batch list reset
    private void MakeNoise() {
		if (cam.transform.position == prevCamPos) {
            UpdateBatches();
        }
		else {
            prevCamPos = cam.transform.position;
            UpdateBatchList();
            UpdateBatches();
        }
		RenderBatches();
        prevCamPos = cam.transform.position;
	}

	//This method updates our clouds
    //First we loop through all of our batches in the batchesToUpdate list
    //For each batch we need to get each cloud with another loop
    private void UpdateBatches() {
		foreach (var batch in batchesToUpdate) {
            foreach (var cloud in batch) {
                //Get noise size based on clouds pos, noise texture scale, and our offset amount
                float size = Mathf.PerlinNoise(cloud.x * texScale + offsetX, cloud.y * texScale + offsetY);

                //If our cloud has a size that's above our visible cloud threashold we need to show it
                if (size > minNoiseSize) {
                    //Get the current scale of the cloud
                    float localScaleX = cloud.scale.x;

                    //Activate any clouds
                    if (!cloud.isActive) {
                        cloud.isActive = true;
                        cloud.scale = Vector3.zero;
                    }
                    //If not max size, scale up
                    if (localScaleX < cloud.randomMaxSize) {
                        ScaleCloud(cloud, size * cloud.randomGrowthSpeed);
                    }
                }
                //Active and it shouldn't be, let's scale down
                else {
                    float localScaleX = cloud.scale.x;
                    ScaleCloud(cloud, cloud.randomMaxSize / shrinkSpeed);

                    //When the cloud is reallllly small we can just set it to 0 and hide it
                    if (localScaleX <= 0.1) {
                        cloud.isActive = false;
                        cloud.scale = Vector3.zero;
                    }
                }
            }
        }
	}

	//This method sets our cloud to a new size
    private void ScaleCloud(CloudData cloud, float direction) {
        cloud.scale += new Vector3(direction, direction, direction);
    }

	//This method clears our batchesToUpdate list because we only want visible batches within this list
    private void UpdateBatchList() {
		//Clears our list
        batchesToUpdate.Clear();

		//Loop through all the generated batches
        foreach (var batch in batches) {
			//If a single cloud is within range we need to add the batch to the update list
            if (CheckForActiveBatch(batch)) {
                batchesToUpdate.Add(batch);
            }
		}
	}

	//This method loops through all the batches to update and draws their meshes to the screen
    private void RenderBatches() {
		foreach (var batch in batchesToUpdate)
        {
            Graphics.DrawMeshInstanced(
                mesh: cloudMesh, 
                submeshIndex: 0,
                material: cloudMat, 
                matrices: batch.Select((a) => a.matrix).ToArray(), 
                count: batch.Count,
                properties: null,
                castShadows: ShadowCastingMode.Off, 
                receiveShadows: true);
        }
	}
}