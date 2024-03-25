using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class CarController : MonoBehaviour {

	public float idealRPM = 500f;
	public float maxRPM = 1000f;

	public Transform centerOfGravity;
	protected Rigidbody rigid;

	public WheelCollider wheelFR;
	public WheelCollider wheelFL;
	public WheelCollider wheelRR;
	public WheelCollider wheelRL;

	public float turnRadius = 6f;
	public float torque = 25f;
	public float brakeTorque = 100f;

	public float AntiRoll = 20000.0f;

	public enum DriveMode { Front, Rear, All };
	public DriveMode driveMode = DriveMode.Rear;

	public TextMeshProUGUI speedText;

	void Start() 
	{
		rigid = GetComponent<Rigidbody>();
		rigid.centerOfMass = centerOfGravity.localPosition;
	}

	public float Speed() 
	{
		return wheelRR.radius * Mathf.PI * wheelRR.rpm * 60f / 1000f;
	}

	public float Rpm() 
	{
		return wheelRL.rpm;
	}

	void FixedUpdate () 
	{
		if (speedText != null)
		{
			//speedText.text = "Speed: " + Speed().ToString("f0") + " km/h" + "\nRPM: " + wheelRL.rpm.ToString("f1");
			speedText.text = "Speed: " + rigid.velocity.magnitude + " km/h" + "\nRPM: " + wheelRL.rpm.ToString("f1");
		}

		float scaledTorque = Input.GetAxis("Vertical") * torque;

		if (wheelRL.rpm < idealRPM)
			scaledTorque = Mathf.Lerp(scaledTorque / 10f, scaledTorque, wheelRL.rpm / idealRPM);
		else
			scaledTorque = Mathf.Lerp(scaledTorque, 0, (wheelRL.rpm - idealRPM) / (maxRPM - idealRPM));

		DoRollBar(wheelFR, wheelFL);
		DoRollBar(wheelRR, wheelRL);

		wheelFR.steerAngle = Input.GetAxis("Horizontal") * turnRadius;
		wheelFL.steerAngle = Input.GetAxis("Horizontal") * turnRadius;

		wheelFR.motorTorque = driveMode == DriveMode.Rear ? 0 : scaledTorque;
		wheelFL.motorTorque = driveMode == DriveMode.Rear ? 0 : scaledTorque;
		wheelRR.motorTorque = driveMode == DriveMode.Front ? 0 : scaledTorque;
		wheelRL.motorTorque = driveMode == DriveMode.Front ? 0 : scaledTorque;

		if(Input.GetKey(KeyCode.LeftShift)) 
		{
			wheelFR.brakeTorque = brakeTorque;
			wheelFL.brakeTorque = brakeTorque;
			wheelRR.brakeTorque = brakeTorque;
			wheelRL.brakeTorque = brakeTorque;
		}
		else 
		{
			wheelFR.brakeTorque = 0;
			wheelFL.brakeTorque = 0;
			wheelRR.brakeTorque = 0;
			wheelRL.brakeTorque = 0;
		}
	}


	void DoRollBar(WheelCollider WheelL, WheelCollider WheelR) 
	{
		WheelHit hit;
		float travelL = 1.0f;
		float travelR = 1.0f;
		
		bool groundedL = WheelL.GetGroundHit(out hit);
		if (groundedL)
			travelL = (-WheelL.transform.InverseTransformPoint(hit.point).y - WheelL.radius) / WheelL.suspensionDistance;
		
		bool groundedR = WheelR.GetGroundHit(out hit);
		if (groundedR)
			travelR = (-WheelR.transform.InverseTransformPoint(hit.point).y - WheelR.radius) / WheelR.suspensionDistance;
		
		float antiRollForce = (travelL - travelR) * AntiRoll;
		
		if (groundedL)
			rigid.AddForceAtPosition(WheelL.transform.up * -antiRollForce, WheelL.transform.position);
		if (groundedR)
			rigid.AddForceAtPosition(WheelR.transform.up * antiRollForce, WheelR.transform.position); 
	}

}
