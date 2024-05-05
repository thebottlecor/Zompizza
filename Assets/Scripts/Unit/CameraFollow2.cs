using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CameraFollow2 : MonoBehaviour {

	public Transform carTransform;
	[Range(1, 10)]
	public float followSpeed = 2;
	[Range(1, 10)]
	public float lookSpeed = 5;
	Vector3 initialCameraPosition;
	Vector3 initialCarPosition;
	Vector3 absoluteInitCameraPosition;

	public Transform camTransform;

	public Camera uiCam;

	int layerMask;

    private void Awake()
    {
		layerMask = 1 << LayerMask.NameToLayer("EnvironmentObject");
		uiCam.gameObject.SetActive(false);
	}

    void Start()
	{
		uiCam.gameObject.SetActive(true);

		initialCameraPosition = gameObject.transform.position;
		initialCarPosition = carTransform.position;
		absoluteInitCameraPosition = initialCameraPosition - initialCarPosition;
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

	public void Shake(float strength)
    {
		camTransform.DOShakePosition(strength * 0.02f, strength * 0.1f);
    }
}
