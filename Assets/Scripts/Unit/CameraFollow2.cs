using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;

public class CameraFollow2 : MonoBehaviour {

	public Transform carTransform;
	[Range(1, 10)]
	public float followSpeed = 2;
	[Range(1, 10)]
	public float lookSpeed = 5;
	//Vector3 initialCameraPosition;
	//Vector3 initialCarPosition;
	public Vector3 absoluteInitCameraPosition;

	public Transform camTransform;

	public Camera mainCam;
	public Camera uiCam;

	int layerMask;

	public bool checkBlocklay = true;

    private void Awake()
    {
		layerMask = 1 << LayerMask.NameToLayer("EnvironmentObject");
		uiCam.gameObject.SetActive(false);
	}

    void Start()
	{
		uiCam.gameObject.SetActive(true);

		//initialCameraPosition = gameObject.transform.position;
		//initialCarPosition = carTransform.position;
		//absoluteInitCameraPosition = initialCameraPosition - initialCarPosition;
	}

	void FixedUpdate()
	{
		//Look at car
		Vector3 _lookDirection = (new Vector3(carTransform.position.x, carTransform.position.y, carTransform.position.z)) - transform.position;
		Quaternion _rot = Quaternion.LookRotation(_lookDirection, Vector3.up);
		transform.rotation = Quaternion.Lerp(transform.rotation, _rot, lookSpeed * Time.deltaTime);

		//Move to car
		Vector3 _targetPos = absoluteInitCameraPosition + carTransform.transform.position;
		transform.position = Vector3.Lerp(transform.position, _targetPos, followSpeed * Time.deltaTime);
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
			var hitsTransform = hitInfo.transform;
            var check = hitsTransform.gameObject.GetComponent<TransparentObject>();
            if (check == null)
            {
                var obj = hitsTransform.gameObject.AddComponent<TransparentObject>();
                obj.Init(1f);
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
	}

	public void ForceUpdate_WhenMoving() // 이동할 때 사용 - 운전 연습장 => 피자 가게 (회전 다음에 위치 이동 시키면 카메라가 이동하는 듯한 느낌을 줌)
	{
		Vector3 _lookDirection = (new Vector3(carTransform.position.x, carTransform.position.y, carTransform.position.z)) - transform.position;
		Quaternion _rot = Quaternion.LookRotation(_lookDirection, Vector3.up);
		transform.rotation = _rot;

		Vector3 _targetPos = absoluteInitCameraPosition + carTransform.transform.position;
		transform.position = _targetPos;
	}


	public void Shake(float strength)
    {
		camTransform.DOShakePosition(strength * 0.02f, strength * 0.1f);

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
}
