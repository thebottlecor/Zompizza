using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feel
{
	public class FeelSquashAndStretchCarController : MonoBehaviour
	{
		[Header("Car Settings")]
		public float Speed = 2f;
		public float RotationSpeed = 2f;
		public float Accel = 1f;
		private float currentSpeed;
		public float RotationLimitPercent = 0.8f;
		public float SpeedStopDown = 3f;

		[Header("Bindings")] 
		public Collider BoundaryCollider;

		public List<TrailRenderer> Trails;
		public MMFeedbacks TeleportFeedbacks;

		protected float playerRealDir;
		protected Vector2 _input;
		protected Vector3 _rotationAxis = Vector3.up;

		protected const string _horizontalAxis = "Horizontal";
		protected const string _verticalAxis = "Vertical";
		protected Bounds _bounds;
		protected Vector3 _thisPosition;
		protected Vector3 _newPosition;
		protected float _trailTime = 0f;

		public Transform cameraTransform;

		public Transform backward;
		public Caravan[] caravans;
        
		protected virtual void Start()
		{
			if (BoundaryCollider != null)
				_bounds = BoundaryCollider.bounds;

			TeleportFeedbacks?.Initialization();
			_trailTime = Trails[0].time;
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
			HandleBounds();

			if (cameraTransform != null)
			{
				// 임의로 카메라 각도 조절
				Vector3 tempAngle = cameraTransform.localEulerAngles;
				tempAngle.y = this.transform.localEulerAngles.y;
				cameraTransform.localEulerAngles = tempAngle;
			}
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

		protected virtual void HandleBounds()
		{
			_newPosition = _thisPosition = this.transform.position;

			if (BoundaryCollider != null)
			{
				if (_thisPosition.x < _bounds.min.x)
				{
					_newPosition.x = _bounds.max.x;
				}
				else if (_thisPosition.x > _bounds.max.x)
				{
					_newPosition.x = _bounds.min.x;
				}

				if (_thisPosition.z < _bounds.min.z)
				{
					_newPosition.z = _bounds.max.z;
				}
				else if (_thisPosition.z > _bounds.max.z)
				{
					_newPosition.z = _bounds.min.z;
				}
			}

			if (_newPosition != _thisPosition)
			{
				StartCoroutine(TeleportSequence());
			}
		}

		protected virtual IEnumerator TeleportSequence()
		{
			TeleportFeedbacks?.PlayFeedbacks();
			SetTrails(false);
			yield return MMCoroutine.WaitForFrames(1);
			this.transform.position = _newPosition;
			TeleportFeedbacks?.PlayFeedbacks();
			SetTrails(true);
		}

		protected virtual void SetTrails(bool status)
		{
			foreach (TrailRenderer trail in Trails)
			{
				trail.Clear();
			}
		}
	}    
}