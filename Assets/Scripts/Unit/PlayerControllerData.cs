using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerData : EventListener
{

    public int maxLoad = 2;
    public int statStar = 20202030;

    [Space(20)]
    //[Header("CAR SETUP")]
    [Space(10)]
    [Range(20, 190)]
    public int maxSpeed = 80; //The maximum speed that the car can reach in km/h.
    public int MaxSpeed => ResearchManager.Instance.globalEffect.maxSpeed + maxSpeed;

    public float damageReduction = 0f;
    public float DamageReduction => 1f - ResearchManager.Instance.globalEffect.damageReduce - damageReduction;

    [Range(10, 120)]
    public int maxReverseSpeed = 80; //The maximum speed that the car can reach while going on reverse in km/h.

    [Range(1, 20)]
    public int accelerationMultiplier = 5; // How fast the car can accelerate. 1 is a slow acceleration and 10 is the fastest.
    public float Accel => (ResearchManager.Instance.globalEffect.acceleration + 1f) * 50f * accelerationMultiplier;
    [Space(10)]
    [Range(10, 45)]
    public int maxSteeringAngle = 35; // The maximum angle that the tires can reach while rotating the steering wheel.
    [Range(0.1f, 1f)]
    public float steeringSpeed = 0.5f; // How fast the steering wheel turns.
    [Space(10)]
    [Range(100, 600)]
    public int brakeForce = 400; // The strength of the wheel brakes.
    [Range(1, 10)]
    public int decelerationMultiplier = 2; // How fast the car decelerates when the user is not using the throttle.
    [Range(1, 10)]
    public int handbrakeDriftMultiplier = 5; // How much grip the car loses when the user hit the handbrake.

    public GameObject frontLeftMesh;
    public GameObject frontRightMesh;
    public GameObject rearLeftMesh;
    public GameObject rearRightMesh;

    public CapsuleCollider coll;

    public MeshRenderer[] meshRenderers;
    public GameObject[] pizzaBoxes;

    [Space(20)]
    public AudioSource carEngineSound; // This variable stores the sound of the car engine.
    public AudioSource tireScreechSound; // This variable stores the sound of the tire screech (when the car is drifting).
    public AudioSource backupSound; // 후진 사운드

    //PARTICLE SYSTEMS
    [Space(20)]
    // The following particle systems are used as tire smoke when the car drifts.
    [SerializeField] protected Vector3 RLWParticleSystemPos = new Vector3(-0.8851747f, 0.2f, -1.558439f);
    [SerializeField] protected Vector3 RRWParticleSystemPos = new Vector3(0.8851747f, 0.2f, -1.558439f);
    [Space(10)]
    // The following trail renderers are used as tire skids when the car loses traction.
    [SerializeField] protected Vector3 RLWTireSkidPos = new Vector3(-0.8851747f, 0.3f, -1.558439f);
    [SerializeField] protected Vector3 RRWTireSkidPos = new Vector3(0.8851747f, 0.3f, -1.558439f);

    [Space(10)]
    public Vector3 bodyMassCenter; // This is a vector that contains the center of mass of the car. I recommend to set this value
                                   // in the points x = 0 and z = 0 of your car. You can select the value that you want in the y axis,
                                   // however, you must notice that the higher this value is, the more unstable the car becomes.
                                   // Usually the y value goes from 0 to 1.5.

    //WHEELS
    /*
    The following variables are used to store the wheels' data of the car. We need both the mesh-only game objects and wheel
    collider components of the wheels. The wheel collider components and 3D meshes of the wheels cannot come from the same
    game object; they must be separate game objects.
    */
    public WheelCollider frontLeftCollider;
    public WheelCollider frontRightCollider;
    public WheelCollider rearLeftCollider;
    public WheelCollider rearRightCollider;


    protected override void AddListeners()
    {
        
    }

    protected override void RemoveListeners()
    {
        
    }

    public void SetData(PlayerController player)
    {
        gameObject.SetActive(true);

        player.maxLoad = maxLoad;

        player.maxSpeed = maxSpeed;
        player.maxReverseSpeed = maxReverseSpeed;

        player.damageReduction = damageReduction;

        player.accelerationMultiplier = accelerationMultiplier;

        player.maxSteeringAngle = maxSteeringAngle;
        player.steeringSpeed = steeringSpeed;

        player.brakeForce = brakeForce;
        player.decelerationMultiplier = decelerationMultiplier;
        player.handbrakeDriftMultiplier = handbrakeDriftMultiplier;

        player.frontLeftMesh = frontLeftMesh;
        player.frontRightMesh = frontRightMesh;
        player.rearLeftMesh = rearLeftMesh;
        player.rearRightMesh = rearRightMesh;

        player.coll = coll;

        player.meshRenderers = meshRenderers;
        player.pizzaBoxes = pizzaBoxes;

        player.carEngineSound = carEngineSound;
        player.tireScreechSound = tireScreechSound;
        player.backupSound = backupSound;

        player.RLWParticleSystem.transform.localPosition = RLWParticleSystemPos;
        player.RRWParticleSystem.transform.localPosition = RRWParticleSystemPos;
        player.RLWTireSkid.transform.localPosition = RLWTireSkidPos;
        player.RRWTireSkid.transform.localPosition = RRWTireSkidPos;

        player.bodyMassCenter = bodyMassCenter;

        player.frontLeftCollider = frontLeftCollider;
        player.frontRightCollider = frontRightCollider;
        player.rearLeftCollider = rearLeftCollider;
        player.rearRightCollider = rearRightCollider;

        player.SetTierPhysics();
        player.SetSound();
        player.SetEtc();
    }
}
