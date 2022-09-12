using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Copyright 2017 Nition, BSD licence (see LICENCE file). http://nition.co
[RequireComponent(typeof(WheelCollider))]
public class WheelSkid : MonoBehaviour {


	[SerializeField] Rigidbody rb;
	[SerializeField] private Skidmarks skidmarksController;


	[SerializeField] private WheelCollider wheelCollider;
	[SerializeField] private WheelHit wheelHitInfo;
	[SerializeField] private float sidewaysSlipDivider = 500f; 
	[SerializeField] private float minSidewaysSlip = 0.2f; 
	[SerializeField] private int minSkidSpeed = 20;
	[SerializeField] public bool isSkidding = false;
	[SerializeField] private float offset = 0.1f;
	private int lastSkid = -1; 
	// Array index for the skidmarks controller. Index of last skidmark piece this wheel used
	private float lastFixedUpdateTime;

	protected void Awake() {
		wheelCollider = GetComponent<WheelCollider>();
		lastFixedUpdateTime = Time.time;
	}

	protected void FixedUpdate() {
		lastFixedUpdateTime = Time.time;
	}

	protected void LateUpdate() {
		if (wheelCollider.GetGroundHit(out wheelHitInfo)) {
			minSidewaysSlip = rb.velocity.magnitude * 2.24f / sidewaysSlipDivider;
			if ((wheelHitInfo.sidewaysSlip >= minSidewaysSlip || wheelHitInfo.sidewaysSlip <= -minSidewaysSlip) && rb.velocity.magnitude > minSkidSpeed) {
				isSkidding = true;
				Vector3 skidPoint = wheelHitInfo.point + rb.velocity * offset;
				lastSkid = skidmarksController.AddSkidMark(skidPoint, wheelHitInfo.normal, 1, lastSkid);
			}
			else {
				isSkidding = false;
				lastSkid = -1;
			}
		}
		else {
			isSkidding = false;
			lastSkid = -1;
		}
	}
}
