using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feel
{
	public class CarController2 : MonoBehaviour
	{
		[Header("Car Settings")]
		public float Speed = 2f;
		public float RotationSpeed = 2f;
		public float Accel = 1f;
		private float currentSpeed;
		public float RotationLimitPercent = 0.8f;
		public float SpeedStopDown = 3f;

		[Header("Bindings")] 
		protected float playerRealDir;
		protected Vector2 _input;
		protected Vector3 _rotationAxis = Vector3.up;

		public Transform backward;
		public Caravan[] caravans;
        
		protected virtual void Start()
		{

		}

		protected virtual void HandleInput()
		{
			_input = FeelDemosInputHelper.GetDirectionAxis(ref _input);

			//_input = new Vector2(0, 0);
			//if (Input.GetKey(KeyCode.W))
			//         {
			//	_input.y = 1f;
			//         }
			//else if (Input.GetKey(KeyCode.S))
			//         {
			//	_input.y = -1f;
			//         }

			//if (Input.GetKey(KeyCode.A))
			//         {
			//	_input.x = -1f;
			//}
			//else if (Input.GetKey(KeyCode.D))
			//         {
			//	_input.x = 1f;
			//}

			float y = 0f;
			if (Input.GetKey(KeyCode.W))
			{
				y = 1f;
			}
			else if (Input.GetKey(KeyCode.S))
			{
				y = -1f;
			}
			_input.y = y;
		}
        
		protected virtual void Update()
		{
			HandleInput();
			MoveCar();
		}

		protected virtual void MoveCar()
		{
			if (_input.y != 0)
			{
				currentSpeed += Accel * Time.deltaTime;

				playerRealDir += _input.y * Time.deltaTime;
			}
			else
			{
				currentSpeed += SpeedStopDown * Time.deltaTime;
			}

			if (_input.x != 0) // 회전 중 최대속도 감소 (최대 80%)
			{
				float rotateMax = ((RotationLimitPercent - 1f) * Mathf.Abs(_input.x) + 1f) * Speed;
				if (currentSpeed > rotateMax)
					currentSpeed = rotateMax;
			}

			if (playerRealDir > 1f)
				playerRealDir = 1f;
			else if (playerRealDir < -1f)
				playerRealDir = -1f;

			if (currentSpeed > Speed)
				currentSpeed = Speed;
			else if (currentSpeed < 0f)
				currentSpeed = 0f;

			if (currentSpeed == 0f)
            {
				if (Mathf.Abs(playerRealDir) > 0.1f)
					playerRealDir += -playerRealDir * 2f * Time.deltaTime;
				else
					playerRealDir = 0f;
            }

			//this.transform.Rotate(_rotationAxis, _input.x * Time.deltaTime * RotationSpeed);
			//this.transform.Translate(this.transform.forward * _input.y * Speed * Time.deltaTime, Space.World);

			this.transform.Rotate(_rotationAxis, _input.x * Time.deltaTime * RotationSpeed);
			//this.transform.Translate(this.transform.forward * _input.y * currentSpeed * Time.deltaTime, Space.World);

			this.transform.Translate(this.transform.forward * playerRealDir * currentSpeed * Time.deltaTime, Space.World);

			for (int i = 0; i < caravans.Length; i++)
            {
				if (i == 0)
				{
					//caravans[i].transform.LookAt(backward);

					Vector3 direction = backward.position - caravans[i].transform.position;
					var rotation = Quaternion.LookRotation(direction);
					rotation.eulerAngles = new Vector3(rotation.eulerAngles.x, Mathf.Clamp(rotation.eulerAngles.y, -30f, 30f), rotation.eulerAngles.z);
					caravans[i].transform.rotation = rotation;

					//Vector3 direction = backward.position - caravans[i].transform.position;
					//Quaternion toRotation = Quaternion.LookRotation(direction, caravans[i].transform.up);
					//caravans[i].transform.rotation = Quaternion.Lerp(caravans[i].transform.rotation, toRotation, 10f * Time.deltaTime);

					//caravans[i].transform.Translate(caravans[i].transform.forward * playerRealDir * currentSpeed * Time.deltaTime, Space.World);
					caravans[i].transform.position = Vector3.Lerp(caravans[i].transform.position, backward.position, Time.time);
				}
				else
                {
					caravans[i].transform.LookAt(caravans[i - 1].backward);
					//caravans[i].transform.Translate(caravans[i].transform.forward * playerRealDir * currentSpeed * Time.deltaTime, Space.World);
				}
			}
		}
	}    
}