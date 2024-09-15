using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;

public class CameraFollow2 : MonoBehaviour {

	public Transform carTransform;
	public Transform shakeTransform;
	[Range(1, 10)]
	public float followSpeed = 2;
	[Range(1, 10)]
	public float lookSpeed = 5;
	//Vector3 initialCameraPosition;
	//Vector3 initialCarPosition;
	private Vector3 initCP;
	public Vector3 absoluteInitCameraPosition;

	public Transform camTransform;

	public Camera mainCam;
	public Camera uiCam;

	int layerMask;

	public bool checkBlocklay = true;

	[Space(20f)]
	public bool secondMode = false;
	//public Transform secondModeTarget;
	//public float secondSpeed = 1.5f;

	[Space(20f)]
	public float lerpTime = 3.5f;
	[Range(0.5f, 3.5f)] public float forwardDistance = 3;
	private float accelerationEffect;

	public ParticleSystem speedlineEffect;
	public PlayerController playerController;

	private Vector3 newPos;
	public Transform focusPoint;

	public float distance = 2f;
	public Vector3 cameraPos;

    private void Awake()
    {
		layerMask = 1 << LayerMask.NameToLayer("EnvironmentObject") | 1 << LayerMask.NameToLayer("EnvironmentRocket");
		initCP = absoluteInitCameraPosition;
		uiCam.gameObject.SetActive(false);
		ChangeMode(true);
	}

    void Start()
	{
		uiCam.gameObject.SetActive(true);

		//initialCameraPosition = gameObject.transform.position;
		//initialCarPosition = carTransform.position;
		//absoluteInitCameraPosition = initialCameraPosition - initialCarPosition;
	}

	public void ChangeMode()
    {
		ChangeMode(!secondMode);
    }

	public void ChangeMode(bool on)
    {
		if (on)
        {
			secondMode = true;
			camTransform.localPosition = new Vector3(0f, cameraPos.z, 0f);
		}
		else
        {
			secondMode = false;
			camTransform.localPosition = new Vector3(0f, 0f, 0f);
			speedlineEffect.gameObject.SetActive(false);
        }
		ResetShake();
	}

	void FixedUpdate()
	{
		if (!secondMode)
		{
			//Look at car
			Vector3 _lookDirection = (new Vector3(carTransform.position.x, carTransform.position.y, carTransform.position.z)) - transform.position;
			Quaternion _rot = Quaternion.LookRotation(_lookDirection, Vector3.up);
			transform.rotation = Quaternion.Lerp(transform.rotation, _rot, lookSpeed * Time.deltaTime);

			//Move to car
			Vector3 _targetPos = absoluteInitCameraPosition + carTransform.transform.position;
			transform.position = Vector3.Lerp(transform.position, _targetPos, followSpeed * Time.deltaTime);
		}
		else
        {
			//transform.LookAt(carTransform);
			//float car_move = Mathf.Abs(Vector3.Distance(transform.position, secondModeTarget.position) * secondSpeed);
			//transform.position = Vector3.MoveTowards(transform.position, secondModeTarget.position, car_move * Time.deltaTime);

			//float gForce = playerController.gForce;

			newPos = carTransform.position - (carTransform.forward * cameraPos.x) + (carTransform.up * cameraPos.y);
			// smotherened G force value
			//accelerationEffect = Mathf.Lerp(accelerationEffect, gForce * 3.5f, 2f * Time.deltaTime);
			accelerationEffect = 0f;
			transform.position = Vector3.Lerp(transform.position, focusPoint.position, lerpTime * Time.deltaTime);

			distance = Mathf.Pow(Vector3.Distance(transform.position, newPos), forwardDistance);
			transform.position = Vector3.MoveTowards(transform.position, newPos, distance * Time.deltaTime);

			camTransform.localRotation = Quaternion.Lerp(camTransform.localRotation, Quaternion.Euler(-accelerationEffect, 0, 0), 5f * Time.deltaTime);
			transform.LookAt(carTransform);

			float speed = playerController.carSpeed;
			if (speed > 60f)
            {
				if (!speedlineEffect.gameObject.activeSelf)
					speedlineEffect.gameObject.SetActive(true);

				var emission = speedlineEffect.emission;
				emission.rateOverTime = 2f * speed - 80f;
			}
			else
            {
				if (speedlineEffect.gameObject.activeSelf)
					speedlineEffect.gameObject.SetActive(false);
			}
        }
	}

	// Camera script
	void LateUpdate()
	{
		if (!checkBlocklay) return;

		Vector3 direction = (carTransform.position - transform.position).normalized;
		//RaycastHit[] hits = Physics.RaycastAll(transform.position, direction, Mathf.Infinity, 1 << LayerMask.NameToLayer("EnvironmentObject"));

		//for (int i = 0; i < hits.Length; i++)
		//{
		//	var hitsTransform = hits[i].transform;
		//	var check = hitsTransform.gameObject.GetComponent<TransparentObject>();
		//	if (check == null)
		//          {
		//		var obj = hitsTransform.gameObject.AddComponent<TransparentObject>();
		//		obj.Init(1f);
		//          }
		//	else
		//          {
		//		check.timer = 1f;
		//          }
		//}

		RaycastHit hitInfo;
		Ray ray = new Ray(transform.position, direction);
		if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, layerMask, QueryTriggerInteraction.UseGlobal))
		{
			bool specialObj = hitInfo.collider.gameObject.layer == 14;

			var hitsTransform = hitInfo.transform;
            var check = hitsTransform.gameObject.GetComponent<TransparentObject>();
            if (check == null)
            {
                var obj = hitsTransform.gameObject.AddComponent<TransparentObject>();
                obj.Init(1f, specialObj);
            }
            else
            {
                check.timer = 1f;
            }
        }
	}

	public void ForceUpdate() // 초기화할 때 사용 - 운전 연습장에서 시작
    {
		Vector3 _targetPos = absoluteInitCameraPosition + carTransform.transform.position;
		transform.position = _targetPos;

		Vector3 _lookDirection = (new Vector3(carTransform.position.x, carTransform.position.y, carTransform.position.z)) - transform.position;
		Quaternion _rot = Quaternion.LookRotation(_lookDirection, Vector3.up);
		transform.rotation = _rot;

		ResetShake();
	}

	public void ForceUpdate_WhenMoving() // 이동할 때 사용 - 운전 연습장 => 피자 가게 (회전 다음에 위치 이동 시키면 카메라가 이동하는 듯한 느낌을 줌)
	{
		Vector3 _lookDirection = (new Vector3(carTransform.position.x, carTransform.position.y, carTransform.position.z)) - transform.position;
		Quaternion _rot = Quaternion.LookRotation(_lookDirection, Vector3.up);
		transform.rotation = _rot;

		Vector3 _targetPos = absoluteInitCameraPosition + carTransform.transform.position;
		transform.position = _targetPos;
	}

	public void ResetShake()
    {
		shakeTransform.localPosition = Vector3.zero;
    }


	public void Shake(float strength)
    {
		shakeTransform.DOShakePosition(strength * 0.02f, strength * 0.1f);

		GamePadRumble(strength * 0.05f, strength * 0.2f, strength * 0.03f);
	}
	public void GamePadRumble(float strength)
    {
		GamePadRumble(strength * 0.05f, strength * 0.2f, strength * 0.03f);
	}

	private Coroutine motorCoroutine;
	public void GamePadRumble(float low, float high, float duration)
    {
		var pad = Gamepad.current;

		if (pad != null)
		{
			pad.SetMotorSpeeds(low, high);

			if (motorCoroutine != null)
            {
				StopCoroutine(motorCoroutine);
				motorCoroutine = null;
            }
			motorCoroutine = StartCoroutine(StopMoter(pad, duration));
		}
	}
	private IEnumerator StopMoter(Gamepad pad, float duration)
    {
		yield return CoroutineHelper.WaitForSecondsRealtime(duration);
		pad.SetMotorSpeeds(0f, 0f);
    }

	public void DriftCameraMove(bool drifting, float xVel)
    {
		if (drifting)
		{
			Vector3 newCP = absoluteInitCameraPosition;
			float newY = Mathf.Lerp(absoluteInitCameraPosition.y, absoluteInitCameraPosition.y - Mathf.Abs(xVel), Time.deltaTime);
			newY = Mathf.Max(newY, initCP.y * 0.9f);
			newCP.y = newY;
			absoluteInitCameraPosition = newCP;
		}
		else
        {
			Vector3 newCP = absoluteInitCameraPosition;
			newCP.y = Mathf.Lerp(newCP.y, initCP.y, Time.deltaTime * 8f);
			absoluteInitCameraPosition = newCP;
        }
    }
}
