/*
MESSAGE FROM CREATOR: This script was coded by Mena. You can use it in your games either these are commercial or
personal projects. You can even add or remove functions as you wish. However, you cannot sell copies of this
script by itself, since it is originally distributed as a free product.
I wish you the best for your project. Good luck!

P.S: If you need more cars, you can check my other vehicle assets on the Unity Asset Store, perhaps you could find
something useful for your game. Best regards, Mena.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.InputSystem;

public class PlayerController : PlayerControllerData
{

    protected float initialCarEngineSoundPitch; // Used to store the initial pitch of the car engine sound.

    //PARTICLE SYSTEMS
    [Space(20)]
    // The following particle systems are used as tire smoke when the car drifts.
    public ParticleSystem RLWParticleSystem;
    public ParticleSystem RRWParticleSystem;
    [Space(10)]
    // The following trail renderers are used as tire skids when the car loses traction.
    public TrailRenderer RLWTireSkid;
    public TrailRenderer RRWTireSkid;

    // 기본 10f
    public float steeringSpeedModify = 10f;
    // 기본 1
    public int brakeForceModify = 1;

    //CAR DATA
    [HideInInspector] public float carSpeed; // Used to store the speed of the car.
    [HideInInspector] public bool isDrifting; // Used to know whether the car is drifting or not.
    [HideInInspector] public bool isTractionLocked; // Used to know whether the traction of the car is locked or not.
    [HideInInspector] public bool isGoBack; // 후진중?

    //PRIVATE VARIABLES
    /*
    IMPORTANT: The following variables should not be modified manually since their values are automatically given via script.
    */
    [HideInInspector] public Rigidbody carRigidbody; // Stores the car's rigidbody.
    float steeringAxis; // Used to know whether the steering wheel has reached the maximum value. It goes from -1 to 1.
    float throttleAxis; // Used to know whether the throttle has reached the maximum value. It goes from -1 to 1.
    float driftingAxis;
    float localVelocityZ;
    public float localVelocityX { get; private set; }
    bool deceleratingCar;
    /*
    The following variables are used to store information about sideways friction of the wheels (such as
    extremumSlip,extremumValue, asymptoteSlip, asymptoteValue and stiffness). We change this values to
    make the car to start drifting.
    */
    WheelFrictionCurve FLwheelFriction;
    float FLWextremumSlip;
    WheelFrictionCurve FRwheelFriction;
    float FRWextremumSlip;
    WheelFrictionCurve RLwheelFriction;
    float RLWextremumSlip;
    WheelFrictionCurve RRwheelFriction;
    float RRWextremumSlip;

    [Header("Custom")]
    public float drift_X_value = 2f;
    public float crashDrag = 1000f;
    public List<ZombieBase> contactingZombies = new List<ZombieBase>();
    private float beforeCollisionSpeed;
    //private Vector3 beforeCollisionVec;
    private bool isCollision;

    public CameraFollow2 cam;

    public static EventHandler<float> DamageEvent;

    private Coroutine hitCoroutine;
    private Coroutine soundCoroutine;
    private Coroutine decelerateCoroutine;
    private Coroutine recoverTractionCoroutine;
    private Coroutine iceCoroutine;

    public float dirftContactBlockTimer; // 드리프트로 좀비들 떨쳐낸 후 몇초간 좀비 붙기 면역

    public bool manMode; // 인간 조종 모드
    public float manSpeed;
    public float manRotSpeed;
    public GameObject manObj;
    public Animator manAnim;
    public AudioSource stepSound;

    public int waterCount;
    private float waterTimer;
    private int currentPizza;
    public AudioSource waterSplashSound;

    public GameObject installDeco;

    private void Start()
    {
        carRigidbody = gameObject.GetComponent<Rigidbody>();
    }

    public void SetTierPhysics()
    {
        if (decelerateCoroutine != null)
        {
            StopCoroutine(decelerateCoroutine);
            decelerateCoroutine = null;
        }
        if (recoverTractionCoroutine != null)
        {
            StopCoroutine(recoverTractionCoroutine);
            recoverTractionCoroutine = null;
        }

        //In this part, we set the 'carRigidbody' value with the Rigidbody attached to this
        //gameObject. Also, we define the center of mass of the car with the Vector3 given
        //in the inspector.
        carRigidbody.centerOfMass = bodyMassCenter;

        //Initial setup to calculate the drift value of the car. This part could look a bit
        //complicated, but do not be afraid, the only thing we're doing here is to save the default
        //friction values of the car wheels so we can set an appropiate drifting value later.
        FLWextremumSlip = frontLeftCollider.sidewaysFriction.extremumSlip;
        FLwheelFriction = new WheelFrictionCurve
        {
            extremumSlip = frontLeftCollider.sidewaysFriction.extremumSlip,
            extremumValue = frontLeftCollider.sidewaysFriction.extremumValue,
            asymptoteSlip = frontLeftCollider.sidewaysFriction.asymptoteSlip,
            asymptoteValue = frontLeftCollider.sidewaysFriction.asymptoteValue,
            stiffness = frontLeftCollider.sidewaysFriction.stiffness
        };

        FRWextremumSlip = frontRightCollider.sidewaysFriction.extremumSlip;
        FRwheelFriction = new WheelFrictionCurve
        {
            extremumSlip = frontRightCollider.sidewaysFriction.extremumSlip,
            extremumValue = frontRightCollider.sidewaysFriction.extremumValue,
            asymptoteSlip = frontRightCollider.sidewaysFriction.asymptoteSlip,
            asymptoteValue = frontRightCollider.sidewaysFriction.asymptoteValue,
            stiffness = frontRightCollider.sidewaysFriction.stiffness
        };

        RLWextremumSlip = rearLeftCollider.sidewaysFriction.extremumSlip;
        RLwheelFriction = new WheelFrictionCurve
        {
            extremumSlip = rearLeftCollider.sidewaysFriction.extremumSlip,
            extremumValue = rearLeftCollider.sidewaysFriction.extremumValue,
            asymptoteSlip = rearLeftCollider.sidewaysFriction.asymptoteSlip,
            asymptoteValue = rearLeftCollider.sidewaysFriction.asymptoteValue,
            stiffness = rearLeftCollider.sidewaysFriction.stiffness
        };

        RRWextremumSlip = rearRightCollider.sidewaysFriction.extremumSlip;
        RRwheelFriction = new WheelFrictionCurve
        {
            extremumSlip = rearRightCollider.sidewaysFriction.extremumSlip,
            extremumValue = rearRightCollider.sidewaysFriction.extremumValue,
            asymptoteSlip = rearRightCollider.sidewaysFriction.asymptoteSlip,
            asymptoteValue = rearRightCollider.sidewaysFriction.asymptoteValue,
            stiffness = rearRightCollider.sidewaysFriction.stiffness
        };

        InstantRecoverIce();
    }
    public void SetSound()
    {
        if (carEngineSound != null)
        {
            carEngineSound.pitch = 0.75f;
            initialCarEngineSoundPitch = 0.75f;
        }
        if (soundCoroutine != null)
        {
            StopCoroutine(soundCoroutine);
        }
        soundCoroutine = StartCoroutine(CarSounds());
    }
    public void SetEtc()
    {
        ShakeOffAllZombies();

        if (hitCoroutine != null)
            StopCoroutine(hitCoroutine);

        for (int i = 0; i < meshRenderers.Length; i++)
        {
            meshRenderers[i].material = DataManager.Instance.materialLib.baseMaterial;
        }
        ToggleBackLight(false);

        UpdateBox(OrderManager.Instance.GetCurrentPizzaBox());

        UIManager.Instance.shopUI.OrderLoadCountTextUpdate();
    }
    public void ToggleBackLight(bool on)
    {
        if (backLights != null)
        {
            if (on)
            {
                for (int i = 0; i < backLights.Length; i++)
                {
                    if (!backLights[i].activeSelf) backLights[i].SetActive(true);
                }
            }
            else
            {
                for (int i = 0; i < backLights.Length; i++)
                {
                    if (backLights[i].activeSelf) backLights[i].SetActive(false);
                }
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Zombie"))
        {
            float speedPercent = Mathf.Abs(carSpeed) / MaxSpeed;
            ContactPoint cp = collision.GetContact(0);
            ZombieBase zombie = collision.gameObject.GetComponent<ZombieBase>();

            if (speedPercent >= 0.15f)
            {
                Vector3 targetDirection = (collision.transform.position - transform.position).normalized;
                //float dotProduct = Vector2.Dot(transform.forward.normalized, targetDirection);
                Vector3 carVel = carRigidbody.velocity.normalized;
                float dotProduct = Vector3.Dot(carVel, targetDirection);
                // 내적의 값이 > 0 이면 플레이어 앞에있고, < 0이면 뒤에있다.
                // Target과 Player사이의 각도
                float theta = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;
                //Debug.Log(dotProduct + " >> " + theta);

                if (dotProduct > 0)
                {
                    // 시야각 안에있는지 여부
                    if (theta <= 120f / 2f)
                    {
                        //Debug.Log("속도 방향과 충돌 방향 일치 -> 힘 전달 " + carRigidbody.velocity.magnitude);

                        int currentVehicle = GM.Instance.currentVehicle;
                        if (currentVehicle != 5 && currentVehicle != 6) // 전투트럭과 경찰차는 좀비 충돌에 따른 패널티 없음
                        {
                            float playerDot = Vector3.Dot(carVel, transform.forward); // 차앞 방향과 속도 내적
                            float crashDrag = this.crashDrag;
                            if (zombie.isHeavy && !zombie.dead)
                            {
                                if (playerDot >= 0) // 전진일 경우만 헤비 좀비에게 튕겨남
                                    crashDrag *= 200f * 2f * speedPercent;
                                beforeCollisionSpeed = MaxSpeed; // 헤비와 부딪히면 무조건 충돌 판정
                                isCollision = true;
                            }

                            if (playerDot >= 0)
                            {
                                carRigidbody.AddForce(-1f * crashDrag * transform.forward, ForceMode.Impulse);
                            }
                            else
                            {
                                carRigidbody.AddForce(crashDrag * transform.forward, ForceMode.Impulse);
                            }
                        }

                        AudioManager.Instance.PlaySFX(Sfx.zombieCrash);
                        zombie.Hit(cp.point, speedPercent, targetDirection);
                        StatManager.Instance.AddHitZombies(1);
                        cam.Shake(2f);

                        return;
                    }
                }
            }

            if (dirftContactBlockTimer <= 0f)
            {
                if (contactingZombies.Count < 10 && zombie.CloseContact(cp.point)) // 최대 10명 부착
                {
                    contactingZombies.Add(zombie);
                    if (GM.Instance.day <= 1) TutorialManager.Instance.ToggleDriftGuide(true);
                }
            }
        }
        //else if (collision.gameObject.layer == 6) // 패스파인딩 블럭 (일반적인 장애물)
        else if (collision.gameObject.layer == 6)
        {
            //dfffexcollisionFromSky = collision.gameObject.CompareTag("Plane");

            beforeCollisionSpeed = carSpeed;
            isCollision = true;
        }
    }

    #region 빙판 & 물
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 4) // 물
        {
            waterCount++;
        }
        else if (other.gameObject.layer == 12) // 빙판
        {
            if (iceCoroutine != null)
                StopCoroutine(iceCoroutine);
            iceCoroutine = StartCoroutine(TouchIce());
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 4) // 물
        {
            waterCount--;
            if (waterCount <= 0)
            {
                waterCount = 0;
                waterTimer = 0f;
            }
        }
    }
    private IEnumerator TouchIce()
    {
        FLwheelFriction.stiffness = 0f;
        frontLeftCollider.sidewaysFriction = FLwheelFriction;

        FRwheelFriction.stiffness = 0f;
        frontRightCollider.sidewaysFriction = FRwheelFriction;

        RLwheelFriction.stiffness = 0f;
        rearLeftCollider.sidewaysFriction = RLwheelFriction;

        RRwheelFriction.stiffness = 0f;
        rearRightCollider.sidewaysFriction = RRwheelFriction;

        yield return CoroutineHelper.WaitForSeconds(1f);

        RecoverIce();

        iceCoroutine = null;
    }
    private void RecoverIce()
    {
        FLwheelFriction.stiffness = sideStiffness;
        frontLeftCollider.sidewaysFriction = FLwheelFriction;

        FRwheelFriction.stiffness = sideStiffness;
        frontRightCollider.sidewaysFriction = FRwheelFriction;

        RLwheelFriction.stiffness = sideStiffness;
        rearLeftCollider.sidewaysFriction = RLwheelFriction;

        RRwheelFriction.stiffness = sideStiffness;
        rearRightCollider.sidewaysFriction = RRwheelFriction;
    }
    private void InstantRecoverIce()
    {
        if (iceCoroutine != null)
        {
            StopCoroutine(iceCoroutine);
            RecoverIce();
            iceCoroutine = null;
        }
    }
    #endregion
    public void HitBlink()
    {
        if (hitCoroutine != null)
            StopCoroutine(hitCoroutine);
        hitCoroutine = StartCoroutine(HitBlinkSequence(meshRenderers));
    }
    private IEnumerator HitBlinkSequence(MeshRenderer[] meshes)
    {
        for (int i = 0; i < meshes.Length; i++)
        {
            meshes[i].material = DataManager.Instance.materialLib.hitMaterial;
        }
        yield return CoroutineHelper.WaitForSeconds(0.1f);
        for (int i = 0; i < meshes.Length; i++)
        {
            meshes[i].material = DataManager.Instance.materialLib.baseMaterial;
        }
        hitCoroutine = null;
    }

    public void UpdateBox(int box)
    {
        for (int i = 0; i < pizzaBoxes.Length; i++)
        {
            pizzaBoxes[i].SetActive(false);
        }
        int max = Mathf.Min(box, pizzaBoxes.Length);
        for (int i = 0; i < max; i++)
        {
            pizzaBoxes[i].SetActive(true);
        }
        currentPizza = box;
    }

    public void StopPlayer(bool forceRotate = true, bool instance = false)
    {
        if (decelerateCoroutine != null)
        {
            StopCoroutine(decelerateCoroutine);
            decelerateCoroutine = null;
        }
        if (recoverTractionCoroutine != null)
        {
            StopCoroutine(recoverTractionCoroutine);
            recoverTractionCoroutine = null;
        }

        //this.transform.eulerAngles = Vector3.zero;

        if (forceRotate)
        {
            if (instance)
                this.transform.localEulerAngles = Vector3.zero;
            else
                this.transform.DORotate(Vector3.zero, 1f).SetUpdate(true);

        }
        else
        {
            // 긴급 탈출의 경우 z 회전만 초기화
            Vector3 angle = transform.localEulerAngles;
            angle.z = 0f;
            this.transform.DORotate(angle, 0.1f).SetUpdate(true);
        }

        carRigidbody.velocity = Vector3.zero;
        carRigidbody.angularVelocity = Vector3.zero;

        frontLeftCollider.motorTorque = 0;
        frontRightCollider.motorTorque = 0;
        rearLeftCollider.motorTorque = 0;
        rearRightCollider.motorTorque = 0;

        frontLeftCollider.brakeTorque = Mathf.Infinity;
        frontRightCollider.brakeTorque = Mathf.Infinity;
        rearLeftCollider.brakeTorque = Mathf.Infinity;
        rearRightCollider.brakeTorque = Mathf.Infinity;

        carSpeed = 0;
        localVelocityX = 0;
        localVelocityZ = 0;

        isDrifting = false;
        RecoverTraction_Instant();
        DriftCarPS();

        InstantRecoverIce();

        ClearCarPS();
    }


    bool forward;
    bool backward;
    bool left;
    bool right;
    bool pressBreak;
    bool upBreak;
    Vector2 input;

    protected override void AddListeners()
    {
        InputHelper.MoveEvent += OnMove;
        InputHelper.SideBreakEvent += OnSideBreak;
    }
    protected override void RemoveListeners()
    {
        InputHelper.MoveEvent -= OnMove;
        InputHelper.SideBreakEvent -= OnSideBreak;
    }

    public void OnMove(object sender, InputAction.CallbackContext e)
    {
        input = Vector2.zero;
        if (GM.Instance.stop_control)
        {
            forward = false;
            backward = false;
            left = false;
            right = false;
            return;
        }

        input = e.ReadValue<Vector2>();
        if (input != null)
        {
            forward = input.y > 0;
            backward = input.y < 0;
            left = input.x < 0;
            right = input.x > 0;
        }
        else
            input = Vector2.zero;
    }
    public void OnSideBreak(object sender, InputAction.CallbackContext e)
    {
        if (GM.Instance.stop_control)
        {
            pressBreak = false;
            upBreak = true;
            return;
        }

        pressBreak = e.performed;
        upBreak = !pressBreak;
    }

    //public float gForce;
    //private float lastFrameVel;

    private void CalcSpeed()
    {
        // We determine the speed of the car.
        carSpeed = (2 * Mathf.PI * frontLeftCollider.radius * frontLeftCollider.rpm * 60) / 1000;
        // Save the local velocity of the car in the x axis. Used to know if the car is drifting.
        localVelocityX = transform.InverseTransformDirection(carRigidbody.velocity).x;
        // Save the local velocity of the car in the z axis. Used to know if the car is going forward or backwards.
        localVelocityZ = transform.InverseTransformDirection(carRigidbody.velocity).z;

        //float currentVel = carRigidbody.velocity.magnitude;
        //gForce = (currentVel - lastFrameVel) / (Time.deltaTime * Physics.gravity.magnitude);
        //lastFrameVel = currentVel;
    }

    void Update()
    {
        //CAR DATA
        CalcSpeed();
        carEngineSound.enabled = !GM.Instance.stop_control;
        tireScreechSound.enabled = !GM.Instance.stop_control;
        backupSound.enabled = !GM.Instance.stop_control;
        //Physics.Raycast(transform.position, Vector3.down, 10f, 1 << LayerMask.NameToLayer("UI"));

        if (isCollision)
        {
            isCollision = false;
            //float speedPercent = Mathf.Abs(beforeCollisionSpeed) / MaxSpeed;

            //if (speedPercent > 0.15f)
            if (Mathf.Abs(beforeCollisionSpeed) > 20f)
            {
                float speedDiff = carSpeed - beforeCollisionSpeed;
                //Debug.Log(speedDiff);
                if (Mathf.Abs(speedDiff) > 0.5f)
                {
                    AudioManager.Instance.PlaySFX(Sfx.crash);

                    if (DamageEvent != null)
                        DamageEvent(null, Constant.crash_damage * UnityEngine.Random.Range(0.75f, 1.25f));

                    ShakeOffAllZombies();

                    StatManager.Instance.carCrash++;

                    cam.Shake(5f);
                }
            }
        }

        if (GM.Instance.stop_control) return;
        if (manObj.activeSelf)
        {
            //if (recoverTractionCoroutine == null)
            //    recoverTractionCoroutine = StartCoroutine(RecoverTraction());

            //if (decelerateCoroutine == null)
            //    decelerateCoroutine = StartCoroutine(DecelerateCar());
            //deceleratingCar = true;

            //ResetSteeringAngle();

            return;
        }

        //CAR PHYSICS

        /*
        The next part is regarding to the car controller. First, it checks if the user wants to use touch controls (for
        mobile devices) or analog input controls (WASD + Space).

        The following methods are called whenever a certain key is pressed. For example, in the first 'if' we call the
        method GoForward() if the user has pressed W.

        In this part of the code we specify what the car needs to do if the user presses W (throttle), S (reverse),
        A (turn left), D (turn right) or Space bar (handbrake).
        */

        {
            //bool forward = HotKey[KeyMap.carForward].Getkey();
            //bool backward = HotKey[KeyMap.carBackward].Getkey();
            //bool left = HotKey[KeyMap.carLeft].Getkey();
            //bool right = HotKey[KeyMap.carRight].Getkey();
            //bool pressBreak = HotKey[KeyMap.carBreak].Getkey();
            //bool upBreak = HotKey[KeyMap.carBreak].GetkeyUp();

            //if (!connected && controllers.Length > 0)
            //{
            //    connected = true;
            //    Debug.Log("Connected");

            //}
            //else if (connected && controllers.Length == 0)
            //{
            //    connected = false;
            //    Debug.Log("Disconnected");
            //}

            // 듀얼센스 테스트
            //bool forward = Input.GetAxisRaw("Forward") > 0;
            //bool backward = Input.GetAxisRaw("Backward") > 0;
            //bool left = Input.GetAxis("Horizontal") < 0;
            //bool right = Input.GetAxis("Horizontal") > 0;
            //bool pressBreak = Input.GetAxis("SideBreak") > 0;
            //bool upBreak = !pressBreak;

            //Debug.Log(Input.GetAxis("SideBreak"));

            ////Debug.Log(Input.GetAxis("Vertical"));
            ////Debug.Log(Input.GetAxis("Mouse X"));
            ///

            if (forward)
            {
                StopDecelerateCoroutine();
                deceleratingCar = false;
                GoForward();
            }
            if (backward)
            {
                StopDecelerateCoroutine();
                deceleratingCar = false;
                GoReverse();
                isGoBack = true;
            }

            if (left)
            {
                TurnLeft();
            }
            if (right)
            {
                TurnRight();
            }
            if (pressBreak)
            {
                StopDecelerateCoroutine();
                deceleratingCar = false;
                Handbrake();
            }
            if (upBreak)
            {
                if (recoverTractionCoroutine == null)
                    recoverTractionCoroutine = StartCoroutine(RecoverTraction());
            }
            if (!backward && !forward)
            {
                ThrottleOff();
            }
            if (!backward && !forward && !pressBreak && !deceleratingCar)
            {
                if (decelerateCoroutine == null)
                    decelerateCoroutine = StartCoroutine(DecelerateCar());
                deceleratingCar = true;
            }
            if (!left && !right && steeringAxis != 0f)
            {
                ResetSteeringAngle();
            }

            if (backward)
            {
                ToggleBackLight(true);
            }
            else
            {
                if (right && !left)
                {
                    if (backLights != null && backLights.Length == 2)
                    {
                        if (!backLights[0].activeSelf) backLights[0].SetActive(true);
                        if (backLights[1].activeSelf) backLights[1].SetActive(false);
                    }
                }
                else if (left && !right)
                {
                    if (backLights != null && backLights.Length == 2)
                    {
                        if (backLights[0].activeSelf) backLights[0].SetActive(false);
                        if (!backLights[1].activeSelf) backLights[1].SetActive(true);
                    }
                }
                else
                {
                    ToggleBackLight(false);
                }
            }
        }

        if (isDrifting) // 드리프트할 경우 떼어내기 발동
        {
            ShakeOffAllZombies();
        }

        if (dirftContactBlockTimer > 0f)
        {
            dirftContactBlockTimer -= Time.deltaTime;
        }

        // We call the method AnimateWheelMeshes() in order to match the wheel collider movements with the 3D meshes of the wheels.
        AnimateWheelMeshes();

        if (waterCount > 0 && currentPizza > 0) // 물속이면 초당 피자가 데미지 받음
        {
            waterTimer += Time.deltaTime;
            if (waterTimer >= 2f)
            {
                waterTimer = 0f;
                if (DamageEvent != null)
                    DamageEvent(null, Constant.water_damage * UnityEngine.Random.Range(0.75f, 1.25f));

                if (!waterSplashSound.isPlaying)
                    waterSplashSound.Play();
            }
        }

        StatManager.Instance.carMileage += Mathf.Abs(carSpeed) * 0.00001f;
        if (SteamHelper.Instance != null) SteamHelper.Instance.AchieveMileage(StatManager.Instance.carMileage);
    }

    private void FixedUpdate()
    {
        if (GM.Instance.stop_control) return;
        if (!manObj.activeSelf) return;

        bool walk = false;
        bool run = pressBreak;
        if (manMode)
        {
            if (input.y < 0)
            {
                //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.forward), 0.1f);
                //transform.Translate(manSpeed * Time.fixedDeltaTime * Vector3.forward);
                //walk = true;

                //transform.rotation = Quaternion.Euler(-transform.rotation.x, -transform.rotation.y, -transform.rotation.z);

                //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, -transform.rotation.y, 0), 0.1f);

                //Vector3 targetAngles = transform.eulerAngles + 180f * Vector3.up;
                //transform.eulerAngles = targetAngles;
                //transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, targetAngles, Time.fixedDeltaTime);
                //walk = true;
                //run = false;
            }
            if (input.x > 0)
            {
                //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.left), 0.1f);

                transform.Rotate(manRotSpeed * Time.fixedDeltaTime * new Vector3(0, 1, 0));

                //transform.Translate(manSpeed * Time.fixedDeltaTime * Vector3.forward);
                walk = true;
                run = false;
            }
            if (input.x < 0)
            {
                //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.right), 0.1f);

                transform.Rotate(manRotSpeed * Time.fixedDeltaTime * new Vector3(0, -1, 0));

                //transform.Translate(manSpeed * Time.fixedDeltaTime * Vector3.forward);
                walk = true;
                run = false;
            }

            if (input.y > 0)
            {
                //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.back), 0.1f);
                float speed = (pressBreak ? 1.5f * manSpeed : manSpeed) * Time.fixedDeltaTime;
                transform.Translate(speed * Vector3.forward);
                walk = true;
                run = pressBreak;
            }
        }
        if (walk)
        {
            stepSound.pitch = run ? 1.9f : 1f; 

            if (!stepSound.isPlaying)
                stepSound.Play();
        }
        manAnim.SetBool(TextManager.WalkId, walk);
        manAnim.SetBool(TextManager.RunId, run);
    }

    public void ShakeOffAllZombies()
    {
        dirftContactBlockTimer = 0.5f;
        StatManager.Instance.AddHitZombies(contactingZombies.Count);
        for (int i = contactingZombies.Count - 1; i >= 0; i--)
        {
            float speedPercent = Mathf.Abs(carSpeed) / MaxSpeed;
            contactingZombies[i].DriftOffContact(localVelocityX, speedPercent);
            contactingZombies.RemoveAt(i);
        }
        TutorialManager.Instance.ToggleDriftGuide(false);
    }

    // This method controls the car sounds. For example, the car engine will sound slow when the car speed is low because the
    // pitch of the sound will be at its lowest point. On the other hand, it will sound fast when the car speed is high because
    // the pitch of the sound will be the sum of the initial pitch + the car speed divided by 100f.
    // Apart from that, the tireScreechSound will play whenever the car starts drifting or losing traction.

    private IEnumerator CarSounds()
    {
        yield return CoroutineHelper.WaitForSeconds(0.1f);

        while (GM.Instance.stop_control)
        {
            yield return null;
        }

        if (carEngineSound != null)
        {
            float engineSoundPitch = initialCarEngineSoundPitch + (Mathf.Abs(carRigidbody.velocity.magnitude) / 25f);
            carEngineSound.pitch = engineSoundPitch;
        }
        if ((isDrifting) || (isTractionLocked && Mathf.Abs(carSpeed) > 12f))
        {
            if (!tireScreechSound.isPlaying && tireScreechSound.enabled)
            {
                tireScreechSound.Play();
            }
        }
        else if ((!isDrifting) && (!isTractionLocked || Mathf.Abs(carSpeed) < 12f))
        {
            tireScreechSound.Stop();
        }
        if (isGoBack && carSpeed < -0.1f)
        {
            if (!backupSound.isPlaying && backupSound.enabled)
                backupSound.Play();
        }
        else
        {
            backupSound.Stop();
        }
        isGoBack = false;

        soundCoroutine = StartCoroutine(CarSounds());
    }

    //
    //STEERING METHODS
    //
    //The following method turns the front car wheels to the left. The speed of this movement will depend on the steeringSpeed variable.
    public void TurnLeft()
    {
        steeringAxis -= (Time.deltaTime * steeringSpeedModify * steeringSpeed);
        if (steeringAxis < -1f)
        {
            steeringAxis = -1f;
        }
        var steeringAngle = steeringAxis * maxSteeringAngle;
        frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, steeringAngle, steeringSpeed);
        frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, steeringAngle, steeringSpeed);
    }

    //The following method turns the front car wheels to the right. The speed of this movement will depend on the steeringSpeed variable.
    public void TurnRight()
    {
        steeringAxis += (Time.deltaTime * steeringSpeedModify * steeringSpeed);
        if (steeringAxis > 1f)
        {
            steeringAxis = 1f;
        }
        var steeringAngle = steeringAxis * maxSteeringAngle;
        frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, steeringAngle, steeringSpeed);
        frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, steeringAngle, steeringSpeed);
    }

    //The following method takes the front car wheels to their default position (rotation = 0). The speed of this movement will depend
    // on the steeringSpeed variable.
    public void ResetSteeringAngle()
    {
        if (steeringAxis < 0f)
        {
            steeringAxis += (Time.deltaTime * steeringSpeedModify * steeringSpeed);
        }
        else if (steeringAxis > 0f)
        {
            steeringAxis -= (Time.deltaTime * steeringSpeedModify * steeringSpeed);
        }
        if (Mathf.Abs(frontLeftCollider.steerAngle) < 1f)
        {
            steeringAxis = 0f;
        }
        var steeringAngle = steeringAxis * maxSteeringAngle;
        frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, steeringAngle, steeringSpeed);
        frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, steeringAngle, steeringSpeed);
    }

    // This method matches both the position and rotation of the WheelColliders with the WheelMeshes.
    void AnimateWheelMeshes()
    {
        float visual_steer_modify = 0.5f;

        frontLeftCollider.GetWorldPose(out Vector3 FLWPosition, out Quaternion FLWRotation);
        frontLeftMesh.transform.position = FLWPosition;
        //frontLeftMesh.transform.rotation = FLWRotation;
        Vector3 angle = new Vector3(FLWRotation.eulerAngles.x, frontLeftCollider.steerAngle * visual_steer_modify, 0);
        frontLeftMesh.transform.localEulerAngles = angle;

        frontRightCollider.GetWorldPose(out Vector3 FRWPosition, out Quaternion FRWRotation);
        frontRightMesh.transform.position = FRWPosition;
        //frontRightMesh.transform.rotation = FRWRotation;
        Vector3 angle2 = new Vector3(FRWRotation.eulerAngles.x, frontRightCollider.steerAngle * visual_steer_modify, 0);
        frontRightMesh.transform.localEulerAngles = angle2;

        rearLeftCollider.GetWorldPose(out Vector3 RLWPosition, out Quaternion RLWRotation);
        rearLeftMesh.transform.position = RLWPosition;
        //rearLeftMesh.transform.rotation = RLWRotation;
        Vector3 angle3 = new Vector3(RLWRotation.eulerAngles.x, rearLeftCollider.steerAngle * visual_steer_modify, 0);
        rearLeftMesh.transform.localEulerAngles = angle3;

        rearRightCollider.GetWorldPose(out Vector3 RRWPosition, out Quaternion RRWRotation);
        rearRightMesh.transform.position = RRWPosition;
        //rearRightMesh.transform.rotation = RRWRotation;
        Vector3 angle4 = new Vector3(RRWRotation.eulerAngles.x, rearRightCollider.steerAngle * visual_steer_modify, 0);
        rearRightMesh.transform.localEulerAngles = angle4;
    }

    //
    //ENGINE AND BRAKING METHODS
    //

    // This method apply positive torque to the wheels in order to go forward.
    public void GoForward()
    {
        //If the forces aplied to the rigidbody in the 'x' asis are greater than
        //3f, it means that the car is losing traction, then the car will start emitting particle systems.
        if (Mathf.Abs(localVelocityX) > drift_X_value)
        {
            isDrifting = true;
            DriftCarPS();
        }
        else
        {
            isDrifting = false;
            DriftCarPS();
        }
        // The following part sets the throttle power to 1 smoothly.
        throttleAxis += (Time.deltaTime * 3f);
        if (throttleAxis > 1f)
        {
            throttleAxis = 1f;
        }
        //If the car is going backwards, then apply brakes in order to avoid strange
        //behaviours. If the local velocity in the 'z' axis is less than -1f, then it
        //is safe to apply positive torque to go forward.
        if (localVelocityZ < -1f)
        {
            Brakes();
        }
        else
        {
            if (Mathf.RoundToInt(carSpeed) < MaxSpeed)
            {
                //Apply positive torque in all wheels to go forward if maxSpeed has not been reached.
                frontLeftCollider.brakeTorque = 0;
                frontLeftCollider.motorTorque = Accel * throttleAxis;
                frontRightCollider.brakeTorque = 0;
                frontRightCollider.motorTorque = Accel * throttleAxis;
                rearLeftCollider.brakeTorque = 0;
                rearLeftCollider.motorTorque = Accel * throttleAxis;
                rearRightCollider.brakeTorque = 0;
                rearRightCollider.motorTorque = Accel * throttleAxis;
            }
            else
            {
                // If the maxSpeed has been reached, then stop applying torque to the wheels.
                // IMPORTANT: The maxSpeed variable should be considered as an approximation; the speed of the car
                // could be a bit higher than expected.
                frontLeftCollider.motorTorque = 0;
                frontRightCollider.motorTorque = 0;
                rearLeftCollider.motorTorque = 0;
                rearRightCollider.motorTorque = 0;
            }
        }
    }

    // This method apply negative torque to the wheels in order to go backwards.
    public void GoReverse()
    {
        //If the forces aplied to the rigidbody in the 'x' asis are greater than
        //3f, it means that the car is losing traction, then the car will start emitting particle systems.
        if (Mathf.Abs(localVelocityX) > drift_X_value)
        {
            isDrifting = true;
            DriftCarPS();
        }
        else
        {
            isDrifting = false;
            DriftCarPS();
        }
        // The following part sets the throttle power to -1 smoothly.
        throttleAxis -= (Time.deltaTime * 6f);
        if (throttleAxis < -1f)
        {
            throttleAxis = -1f;
        }
        //If the car is still going forward, then apply brakes in order to avoid strange
        //behaviours. If the local velocity in the 'z' axis is greater than 1f, then it
        //is safe to apply negative torque to go reverse.
        if (localVelocityZ > 1f)
        {
            Brakes();
        }
        else
        {
            if (Mathf.Abs(Mathf.RoundToInt(carSpeed)) < maxReverseSpeed)
            {
                //Apply negative torque in all wheels to go in reverse if maxReverseSpeed has not been reached.
                frontLeftCollider.brakeTorque = 0;
                frontLeftCollider.motorTorque = Accel * throttleAxis * 1.1f; // 후진 가속도는 10% 더 빠름
                frontRightCollider.brakeTorque = 0;
                frontRightCollider.motorTorque = Accel * throttleAxis * 1.1f;
                rearLeftCollider.brakeTorque = 0;
                rearLeftCollider.motorTorque = Accel * throttleAxis * 1.1f;
                rearRightCollider.brakeTorque = 0;
                rearRightCollider.motorTorque = Accel * throttleAxis * 1.1f;
            }
            else
            {
                //If the maxReverseSpeed has been reached, then stop applying torque to the wheels.
                // IMPORTANT: The maxReverseSpeed variable should be considered as an approximation; the speed of the car
                // could be a bit higher than expected.
                frontLeftCollider.motorTorque = 0;
                frontRightCollider.motorTorque = 0;
                rearLeftCollider.motorTorque = 0;
                rearRightCollider.motorTorque = 0;
            }
        }
    }

    //The following function set the motor torque to 0 (in case the user is not pressing either W or S).
    public void ThrottleOff()
    {
        frontLeftCollider.motorTorque = 0;
        frontRightCollider.motorTorque = 0;
        rearLeftCollider.motorTorque = 0;
        rearRightCollider.motorTorque = 0;
    }

    // The following method decelerates the speed of the car according to the decelerationMultiplier variable, where
    // 1 is the slowest and 10 is the fastest deceleration. This method is called by the function InvokeRepeating,
    // usually every 0.1f when the user is not pressing W (throttle), S (reverse) or Space bar (handbrake).
    private IEnumerator DecelerateCar()
    {
        yield return CoroutineHelper.WaitForSeconds(0.1f);

        if (Mathf.Abs(localVelocityX) > drift_X_value)
        {
            isDrifting = true;
            DriftCarPS();
        }
        else
        {
            isDrifting = false;
            DriftCarPS();
        }
        // The following part resets the throttle power to 0 smoothly.
        if (throttleAxis != 0f)
        {
            if (throttleAxis > 0f)
            {
                throttleAxis -= (Time.deltaTime * 10f);
            }
            else if (throttleAxis < 0f)
            {
                throttleAxis += (Time.deltaTime * 10f);
            }
            if (Mathf.Abs(throttleAxis) < 0.15f)
            {
                throttleAxis = 0f;
            }
        }
        carRigidbody.velocity *= 1f / (1f + (0.025f * decelerationMultiplier));
        // Since we want to decelerate the car, we are going to remove the torque from the wheels of the car.
        frontLeftCollider.motorTorque = 0;
        frontRightCollider.motorTorque = 0;
        rearLeftCollider.motorTorque = 0;
        rearRightCollider.motorTorque = 0;
        // If the magnitude of the car's velocity is less than 0.25f (very slow velocity), then stop the car completely and
        // also cancel the invoke of this method.
        if (carRigidbody.velocity.magnitude < 0.25f)
        {
            carRigidbody.velocity = Vector3.zero;
            StopDecelerateCoroutine();
        }
        else
        {
            decelerateCoroutine = StartCoroutine(DecelerateCar());
        }
    }
    private void StopDecelerateCoroutine()
    {
        if (decelerateCoroutine != null)
        {
            StopCoroutine(decelerateCoroutine);
            decelerateCoroutine = null;
        }
    }

    // This function applies brake torque to the wheels according to the brake force given by the user.
    public void Brakes()
    {
        int force = brakeForce * brakeForceModify;

        //carRigidbody.velocity = Vector3.zero;
        //carRigidbody.angularVelocity = Vector3.zero;

        frontLeftCollider.brakeTorque = force;
        frontRightCollider.brakeTorque = force;
        rearLeftCollider.brakeTorque = force;
        rearRightCollider.brakeTorque = force;

        //carRigidbody.velocity *= 1f / (1f + (0.025f * decelerationMultiplier));
        carRigidbody.velocity *= 0.99f;
    }

    // This function is used to make the car lose traction. By using this, the car will start drifting. The amount of traction lost
    // will depend on the handbrakeDriftMultiplier variable. If this value is small, then the car will not drift too much, but if
    // it is high, then you could make the car to feel like going on ice.
    public void Handbrake()
    {
        if (recoverTractionCoroutine != null)
        {
            StopCoroutine(recoverTractionCoroutine);
            recoverTractionCoroutine = null;
        }
        // We are going to start losing traction smoothly, there is were our 'driftingAxis' variable takes
        // place. This variable will start from 0 and will reach a top value of 1, which means that the maximum
        // drifting value has been reached. It will increase smoothly by using the variable Time.deltaTime.
        driftingAxis += 2f * Time.deltaTime;
        float secureStartingPoint = driftingAxis * FLWextremumSlip * handbrakeDriftMultiplier;

        if (secureStartingPoint < FLWextremumSlip)
        {
            driftingAxis = FLWextremumSlip / (FLWextremumSlip * handbrakeDriftMultiplier);
        }
        if (driftingAxis > 1f)
        {
            driftingAxis = 1f;
        }
        //If the forces aplied to the rigidbody in the 'x' asis are greater than
        //3f, it means that the car lost its traction, then the car will start emitting particle systems.
        if (Mathf.Abs(localVelocityX) >= drift_X_value * 0.6f) // 드리프트 키를 사용할 경우, 드리프트 효과 (좀비 떼내기), 비쥬얼 이펙트 역치가 낮아짐
        {
            isDrifting = true;
        }
        else
        {
            isDrifting = false;
        }
        //If the 'driftingAxis' value is not 1f, it means that the wheels have not reach their maximum drifting
        //value, so, we are going to continue increasing the sideways friction of the wheels until driftingAxis
        // = 1f.
        if (driftingAxis < 1f)
        {
            FLwheelFriction.extremumSlip = FLWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            frontLeftCollider.sidewaysFriction = FLwheelFriction;

            FRwheelFriction.extremumSlip = FRWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            frontRightCollider.sidewaysFriction = FRwheelFriction;

            RLwheelFriction.extremumSlip = RLWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            rearLeftCollider.sidewaysFriction = RLwheelFriction;

            RRwheelFriction.extremumSlip = RRWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            rearRightCollider.sidewaysFriction = RRwheelFriction;
        }

        // Whenever the player uses the handbrake, it means that the wheels are locked, so we set 'isTractionLocked = true'
        // and, as a consequense, the car starts to emit trails to simulate the wheel skids.
        isTractionLocked = true;
        DriftCarPS();

    }

    // This function is used to emit both the particle systems of the tires' smoke and the trail renderers of the tire skids
    // depending on the value of the bool variables 'isDrifting' and 'isTractionLocked'.
    public void DriftCarPS()
    {
        if (isDrifting)
        {
            RLWParticleSystem.Play();
            RRWParticleSystem.Play();
        }
        else
        {
            RLWParticleSystem.Stop();
            RRWParticleSystem.Stop();
        }

        if ((isTractionLocked || Mathf.Abs(localVelocityX) > 5f) && Mathf.Abs(carSpeed) > 12f && transform.position.y < 0.5f)
        {
            RLWTireSkid.emitting = true;
            RRWTireSkid.emitting = true;
        }
        else
        {
            RLWTireSkid.emitting = false;
            RRWTireSkid.emitting = false;
        }

        cam.DriftCameraMove(isDrifting, localVelocityX);
    }
    public void ClearCarPS()
    {
        RLWParticleSystem.Clear();
        RRWParticleSystem.Clear();
        RLWTireSkid.Clear();
        RRWTireSkid.Clear();
    }

    // This function is used to recover the traction of the car when the user has stopped using the car's handbrake.
    private IEnumerator RecoverTraction()
    {
        yield return null;

        isTractionLocked = false;
        driftingAxis -= (Time.deltaTime / 1.5f);
        if (driftingAxis < 0f)
        {
            driftingAxis = 0f;
        }

        //If the 'driftingAxis' value is not 0f, it means that the wheels have not recovered their traction.
        //We are going to continue decreasing the sideways friction of the wheels until we reach the initial
        // car's grip.
        if (FLwheelFriction.extremumSlip > FLWextremumSlip)
        {
            FLwheelFriction.extremumSlip = FLWextremumSlip * handbrakeDriftMultiplier * driftingAxis;            
            frontLeftCollider.sidewaysFriction = FLwheelFriction;

            FRwheelFriction.extremumSlip = FRWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            frontRightCollider.sidewaysFriction = FRwheelFriction;

            RLwheelFriction.extremumSlip = RLWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            rearLeftCollider.sidewaysFriction = RLwheelFriction;

            RRwheelFriction.extremumSlip = RRWextremumSlip * handbrakeDriftMultiplier * driftingAxis;
            rearRightCollider.sidewaysFriction = RRwheelFriction;

            recoverTractionCoroutine = StartCoroutine(RecoverTraction());
        }
        else if (FLwheelFriction.extremumSlip < FLWextremumSlip)
        {
            FLwheelFriction.extremumSlip = FLWextremumSlip;
            frontLeftCollider.sidewaysFriction = FLwheelFriction;

            FRwheelFriction.extremumSlip = FRWextremumSlip;
            frontRightCollider.sidewaysFriction = FRwheelFriction;

            RLwheelFriction.extremumSlip = RLWextremumSlip;
            rearLeftCollider.sidewaysFriction = RLwheelFriction;

            RRwheelFriction.extremumSlip = RRWextremumSlip;
            rearRightCollider.sidewaysFriction = RRwheelFriction;

            driftingAxis = 0f;

            recoverTractionCoroutine = null;
        }
    }
    private void RecoverTraction_Instant()
    {
        isTractionLocked = false;

        FLwheelFriction.extremumSlip = FLWextremumSlip;
        frontLeftCollider.sidewaysFriction = FLwheelFriction;

        FRwheelFriction.extremumSlip = FRWextremumSlip;
        frontRightCollider.sidewaysFriction = FRwheelFriction;

        RLwheelFriction.extremumSlip = RLWextremumSlip;
        rearLeftCollider.sidewaysFriction = RLwheelFriction;

        RRwheelFriction.extremumSlip = RRWextremumSlip;
        rearRightCollider.sidewaysFriction = RRwheelFriction;

        driftingAxis = 0f;
    }
}
