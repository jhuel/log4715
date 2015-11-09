using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarController : MonoBehaviour
{

    // This car component is designed to be used on a gameobject which has wheels attached.
    // The wheels must be child objects, and each have a Wheel script attached, and a WheelCollider component.

    // Even though wheelcolliders have their own settings for grip loss, this car script (and its accompanying
    // wheel scripts) modify the settings on the wheelcolliders at runtime, to give a more exaggerated and fun
    // experience, allowing burnouts and drifting behavior in a way that is not readily achievable using
    // constant values on wheelcolliders alone.

    // The code priorities fun over realism, and although a gears system is included, it is not used to 
    // 'drive' the engine. Instead, the current revs and gear are calculated retrospectively based
    // on the car's current speed. These gear and rev values can then be read and used by a GUI or Sound component.


    const float MAX_SPEED = 75;
    const float MAX_TORQUE = 50;
    const float MAX_STEER_ANGLE = 28;
    const int MAX_CAR_HP = 100;
    const int MAX_CAR_NITRO = 100;


    [SerializeField]
    private float rotationHorizontale = 5;
    [SerializeField]
    private float rotationVerticale = 5;
    [SerializeField]
    private float maxSteerAngle = MAX_STEER_ANGLE;                              // The maximum angle the car can steer
    [SerializeField]
    private float steeringResponseSpeed = 200;                     // how fast the steering responds
    [SerializeField]
    [Range(0, 1)]
    private float maxSpeedSteerAngle = 0.23f;        // the reduction in steering angle at max speed
    [SerializeField]
    [Range(0, .5f)]
    private float maxSpeedSteerResponse = 0.5f;    // the reduction in steer response at max speed
    [SerializeField]
    private float maxSpeed = MAX_SPEED;                                   // the maximum speed (in meters per second!)
    [SerializeField]
    private float maxTorque = MAX_TORQUE;                                  // the maximum torque of the engine
    [SerializeField]
    private float minTorque = 10;                                  // the minimum torque of the engine
    [SerializeField]
    private float brakePower = 40;                                 // how powerful the brakes are at stopping the car
    [SerializeField]
    private float adjustCentreOfMass = 0.25f;                      // vertical offset for the centre of mass
    [SerializeField]
    private Advanced advanced;                                     // container for the advanced setting which will expose as a foldout in the inspector
    [SerializeField]
    bool preserveDirectionWhileInAir = false;                      // flag for if the direction of travel to be preserved in the air (helps cars land in the right direction if doing huge jumps!)

    [System.Serializable]
    public class Advanced                                                           // the advanced settings for the car controller
    {
        [Range(0, 1)]
        public float burnoutSlipEffect = 0.4f;                        // how much the car wheels will slide when burning out
        [Range(0, 1)]
        public float burnoutTendency = 0.2f;                          // how likely the car is to burnout 
        [Range(0, 1)]
        public float spinoutSlipEffect = 0.5f;                        // how easily the car spins out when turning
        [Range(0, 1)]
        public float sideSlideEffect = 0.5f;                          // how easily the car loses sideways grip 

        public float downForce = 30;                                                // the amount of downforce applied (speed is factored in)
        public int numGears = 5;                                                    // the number of gears
        [Range(0, 1)]
        public float gearDistributionBias = 0.2f;                     // Controls whether the gears are bunched together towards the lower or higher end of the car's range of speed.
        public float steeringCorrection = 2f;                                       // How fast the steering returns to centre with no steering input
        public float oppositeLockSteeringCorrection = 4f;                           // How fast the steering responds when steer input is in the opposite direction to the current wheel angle
        public float reversingSpeedFactor = 0.3f;                                   // The car's maximum reverse speed, as a proportion of its max forward speed.
        public float skidGearLockFactor = 0.1f;                                     // The car will not automatically change gear if the current skid factor is higher than this value.
        public float accelChangeSmoothing = 2f;                                     // Used to smooth out changes in acceleration input.
        public float gearFactorSmoothing = 5f;                                      // Controls the speed at which revs drop or raise to match new gear, after a gear change.
        [Range(0, 1)]
        public float revRangeBoundary = 0.8f;                           // The amount of the full rev range used in each gear.
    }

    [SerializeField]
    public GUIText mphDisplay;


    private float[] gearDistribution;                                               // Stores the caluclated change point for each gear (0-1 as a normalised amount relative to car's max speed)
    private Wheel[] wheels;                                                         // Stores a reference to each wheel attached to this car.
    private float accelBrake;                                                       // The acceleration or braking input (1 to -1 range)
    private float smallSpeed;                                                       // A small proportion of max speed, used to decide when to start accelerating/braking when transitioning between fwd and reverse motion
    private float maxReversingSpeed;                                                // The maximum reversing speed
    private bool immobilized;                                                       // Whether the car is accepting inputs.
    private float healthSpeedMultip;

    public int currentCheckpoint;
    public int currentLap;
    public Transform lastCheckpoint;

    private static int CHECKPOINT_VALUE = 100;
    private static int LAP_VALUE = 10000;


    [Range(0.01f, 0.2f)]
    [SerializeField]
    float jumpTime = 0.05f;

    [SerializeField]
    float jumpForce = 1000f;	
                                    // the average skid factor from all wheels
    // publicly read-only props, useful for GUI, Sound effects, etc.
    public int GearNum { get; private set; }                                        // the current gear we're in.
    public float CurrentSpeed { get; private set; }                                 // the current speed of the car
    public float CurrentSteerAngle { get; private set; }                             // The current steering angle for steerable wheels.
    public float AccelInput { get; private set; }                                   // the current acceleration input
    public float BrakeInput { get; private set; }                                   // the current brake input
    public float GearFactor { get; private set; }                                  // value between 0-1 indicating where the current revs fall within the current range of revs for this gear
    public float AvgPowerWheelRpmFactor { get; private set; }                       // the average RPM of all wheels marked as 'powered'
    public float AvgSkid { get; private set; }                                      // the average skid factor from all wheels
    public float RevsFactor { get; private set; }                                   // value between 0-1 indicating where the current revs fall between 0 and max revs
    public float SpeedFactor { get;  private set; }                                 // value between 0-1 of the car's current speed relative to max speed


    // Use this for initialization
    public void Initialize()
    {
        currentCheckpoint = 0;
        currentLap = 0;
    }

    private enum CollectibleTypes
    {
        CollectibleNone,
        CollectibleHeal,
        CollectibleNitro,
        CollectibleSpeed
    }
    private CollectibleTypes currentCollectible;
    private int SpeedBonus; // number of iteration of accel bonus
    public void OnTriggerEnter(Collider other) {

        string otherTag = other.gameObject.tag;

        if (otherTag.CompareTo("SpeedBonus") == 0)
        {
            SpeedBonus = 3;
        }
        if (otherTag.CompareTo("CollectibleSpeed") == 0)
        {
            currentCollectible = CollectibleTypes.CollectibleSpeed;
        }
        if (otherTag.CompareTo("CollectibleHeal") == 0)
        {
            currentCollectible = CollectibleTypes.CollectibleHeal;
        }
        if (otherTag.CompareTo("CollectibleNitro") == 0)
        {
            currentCollectible = CollectibleTypes.CollectibleNitro;
        }
        else if (otherTag.CompareTo("checkpoint1") == 0)
        {
            currentLap++;

            lastCheckpoint = other.transform;
        }
        else if (otherTag.CompareTo("checkpoint2") == 0 || otherTag.CompareTo("checkpoint3") == 0)
        {
            lastCheckpoint = other.transform;
        }
     }
 
    public void Update()
    {
        if(SpeedBonus > 0)
        {
            rigidbody.AddForce(transform.rotation * (new Vector3(0f, 0f, 200f)));
            SpeedBonus--;
        }

    }
     public float GetDistance() {
         if (lastCheckpoint == null)
             return -10000;
         return (transform.position - lastCheckpoint.position).magnitude + currentCheckpoint * CHECKPOINT_VALUE + currentLap * LAP_VALUE;
     }
 
     public int GetCarPosition(CarController[] allCars) {
         float distance = GetDistance();
         int position = 1;
         foreach (CarController car in allCars) {
             if (car.GetDistance() > distance && name != car.name)
                 position++;
         }
         return position;
     }

    public int NumGears
    {					// the number of gears set up on the car
        get { return advanced.numGears; }
    }

    private int carHP;
    private int carNitro;
    public void getHit(int damage = 10)
    {
        carHP -= damage;

        dealWithHP();
    }
    private void dealWithHP()
    {
        if(carHP < 0)
        {
            carHP = 0;
        }

        
        if (carHP > 2 * MAX_CAR_HP / 3)
        {
            // todo maxspeed est descendu
            healthSpeedMultip = 1.0f;
        }
        if (carHP <= 2*MAX_CAR_HP / 3)
        {
            // todo maxspeed est descendu
            healthSpeedMultip = 0.8f;
        }
        else if(carHP <= MAX_CAR_HP/3)
        {
            // todo maxspeed est descendu
            healthSpeedMultip = 0.5f;
        }
        else if (carHP <= MAX_CAR_HP / 10)
        {
            // todo maxspeed est descendu
            healthSpeedMultip = 0.1f;
        } 
        else if (carHP == 0)
        {
            healthSpeedMultip = 0;
        }
    }

    private int playerPoints;

    public Text playerPointText;
    public Text playerCollectibleText;
    public Text playerHPText;
    public Slider playerHPSlider;
    public Slider playerNitroSlider;

    public int PlayerPoints
    {
        get { return playerPoints; }
        set
        {
            playerPoints = value;
        }
    }

    public void ApplyRubberBand(float multiplier)
    {
        float maxSpeedAfterHP = MAX_SPEED *healthSpeedMultip;
        maxSpeed = maxSpeedAfterHP + maxSpeedAfterHP * multiplier;
        maxTorque = MAX_TORQUE + MAX_TORQUE * multiplier;
        maxSteerAngle = MAX_STEER_ANGLE - MAX_STEER_ANGLE * multiplier;
    }

    void Start()
    {
        currentCollectible = CollectibleTypes.CollectibleNone;
        SpeedBonus = 0;
        PlayerPoints = 0;
        carHP = MAX_CAR_HP;
        carNitro = MAX_CAR_NITRO;
        healthSpeedMultip = 1.0f;
        if (playerNitroSlider != null)
            playerNitroSlider.gameObject.SetActive(false);

        if (playerHPSlider != null)
            playerHPSlider.gameObject.SetActive(false);

    }

    private int airPoints = 0;
    void FixedUpdate()
    {
        if (!anyOnGround && CurrentSpeed != 0)
        {
            airPoints++;
            if (airPoints == 3)
            {
                PlayerPoints += 3;
                airPoints = 0;
            }
        }
    }



    // the following values are provided as read-only properties,
    // and are required by the Wheel script to compute grip, burnout, skidding, etc
    public float MaxSpeed
    {
        get { return maxSpeed; }
    }

    public float MaxTorque
    {
        get { return maxTorque; }
    }


    public float BurnoutSlipEffect
    {
        get { return advanced.burnoutSlipEffect; }
    }


    public float BurnoutTendency
    {
        get { return advanced.burnoutTendency; }
    }


    public float SpinoutSlipEffect
    {
        get { return advanced.spinoutSlipEffect; }
    }


    public float SideSlideEffect
    {
        get { return advanced.sideSlideEffect; }
    }


    public float MaxSteerAngle
    {
        get { return maxSteerAngle; }
    }


    // variables added due to separating out things into functions!
    bool anyOnGround;
    float curvedSpeedFactor;
    bool reversing;
    float targetAccelInput; // target accel input is our desired acceleration input. We smooth towards it later

    UnityEngine.Object projectile;
    float speed = 20; 

    void Awake()
    {
        // get a reference to all wheel attached to the car.
        wheels = GetComponentsInChildren<Wheel>();

        SetUpGears();

        // deactivate and reactivate the gameobject - this is a workaround
        // to a bug where changes to wheelcolliders at runtime are not 'taken'
        // by the rigidbody unless this step is performed :(
        gameObject.SetActive(false);
        gameObject.SetActive(true);

        // a few useful speeds are calculated for use later:
        smallSpeed = maxSpeed * 0.05f;
        maxReversingSpeed = maxSpeed * advanced.reversingSpeedFactor;
    }


    void OnEnable()
    {
        // set adjusted centre of mass.
        rigidbody.centerOfMass = Vector3.up * adjustCentreOfMass;
    }


    public void Move(float steerInput, float accelBrakeInput)
    {
        // lose control of engine if immobilized
        if (immobilized) accelBrakeInput = 0;

        ConvertInputToAccelerationAndBraking(accelBrakeInput);
        CalculateSpeedValues();
        HandleGearChanging();
        CalculateGearFactor();
        ProcessWheels(steerInput);
        ApplyDownforce();
        CalculateRevs();
        PreserveDirectionInAir(accelBrakeInput, steerInput);

    }

    private string jump;

    public void Jump(string jumpButton)
    {
        if (anyOnGround)
        {
            jump = jumpButton;
            StartCoroutine(JumpRoutine());
        }
    }

    // http://gamasutra.com/blogs/DanielFineberg/20150825/244650/Designing_a_Jump_in_Unity.php
    IEnumerator JumpRoutine()
    {
        float jumpTimer = 0;
        // Check if jump button is still pressed
        while (CrossPlatformInput.GetButton(jump) && jumpTimer < jumpTime )
        {
            // Jump time proportion
            float proportionCompleted = jumpTimer / jumpTime;

            Vector3 thisFrameJumpVector = Vector3.Lerp(new Vector3(0f, jumpForce, 0f), Vector3.zero, proportionCompleted);

            // Increment the force relative to the time spent with the jump button pressed
            rigidbody.AddForce(thisFrameJumpVector);

            jumpTimer += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

    }

    void OnGUI()
    {
        if (playerPointText != null)
        {
            playerPointText.text = ("Styling points : " + playerPoints.ToString());
        }

        if(playerCollectibleText != null)
        {
            string collectibleString;
            switch(currentCollectible)
            {
                case CollectibleTypes.CollectibleHeal:
                    collectibleString = "Heal";
                    break;
                case CollectibleTypes.CollectibleSpeed:
                    collectibleString = "Speed";
                    break;
                case CollectibleTypes.CollectibleNone:
                default:
                    collectibleString = "No collectibles";
                    break;
            }

            playerCollectibleText.text = "Current Collectible : " + collectibleString;
        }

        if(playerHPSlider != null)
        {
            playerHPSlider.gameObject.SetActive(true);
            playerHPSlider.value = carHP;

        }
        if (playerNitroSlider != null)
        {
            playerNitroSlider.gameObject.SetActive(true);
            playerNitroSlider.value = carNitro;
        }
    }

    public void ShootGreen()
    {

        // http://answers.unity3d.com/questions/19710/shooting-a-bullet-projectile-properly.html
        // http://answers.unity3d.com/questions/12003/instantiate-a-prefab-through-code-in-c.html
        GameObject clone;

        projectile = Resources.LoadAssetAtPath("Assets/Sample Assets/Utility/Prefabs/BouncyProjectile.prefab", typeof(GameObject));
        clone = Instantiate(projectile, transform.position + transform.rotation * (new Vector3(0f, 0.5f, 5f)), transform.rotation) as GameObject;


        //clone.rigidbody.AddRelativeForce(transform.rotation * (new Vector3(0f, 0f, 5000f)));
        clone.rigidbody.velocity = transform.rigidbody.velocity;

        clone.GetComponent<BouncyProjectileBehavior>().Shoot();
    }

    public void shootRed()
    {
        GameObject clone;

        projectile = Resources.LoadAssetAtPath("Assets/Sample Assets/Utility/Prefabs/SeekingProjectile.prefab", typeof(GameObject));
        clone = Instantiate(projectile, transform.position + transform.rotation * (new Vector3(0f, 0.5f, 10f)), transform.rotation) as GameObject;

        GameObject[] cars = GameObject.FindGameObjectsWithTag("Player");

        Transform closestCar = null;

        float shortestDistance = float.MaxValue;
        /* Find closest car */
        foreach (GameObject car in cars)
        {
            /* Find magnitude (distance between the two objects) */
            float distance =
                (car.transform.position - clone.transform.position).magnitude;
            if (distance < shortestDistance && car != null && car.transform != transform)
            {
                closestCar = car.gameObject.transform;
                shortestDistance = distance;
            }
        }

        //clone.AddComponent<SeekingProjectileBehaviour>();
            
        

        clone.GetComponent<SeekingProjectileBehaviour>().isTracking = true;
        clone.GetComponent<SeekingProjectileBehaviour>().trackedObject = closestCar;

    }
    public void useNitro()
    {
        if(carNitro >= 10)
        {
            SpeedBonus += 2;
            carNitro -= 10;
        }
    }

    public void useCollectible()
    { 
        switch(currentCollectible)
        {
            case CollectibleTypes.CollectibleSpeed:
                SpeedBonus = 10;
                break;

            case CollectibleTypes.CollectibleHeal:
                carHP = MAX_CAR_HP;
                dealWithHP();
                break;
            case CollectibleTypes.CollectibleNitro:
                carNitro = MAX_CAR_NITRO;
                break;
            case CollectibleTypes.CollectibleNone:
            default:
                break;

        }

        currentCollectible = CollectibleTypes.CollectibleNone;
    }

    void ConvertInputToAccelerationAndBraking(float accelBrakeInput)
    {
        // move.Z is the user's fwd/back input. We need to convert it into acceleration and braking.
        // this differs based on if the car is currently moving forward or backward.
        // change is based slightly away from the zero value (by "smallspeed") so that for example when
        // the car transitions from reversing to moving forwards, the car does not need to come to a complete
        // rest before starting to accelerate.

        reversing = false;
        if (accelBrakeInput > 0)
        {
            if (CurrentSpeed > -smallSpeed)
            {
                // pressing forward while moving forward : accelerate!
                targetAccelInput = accelBrakeInput;
                BrakeInput = 0;
            }
            else
            {
                // pressing forward while movnig backward : brake!
                BrakeInput = accelBrakeInput;
                targetAccelInput = 0;
            }
        }
        else
        {
            if (CurrentSpeed > smallSpeed)
            {
                // pressing backward while moving forward : brake!
                BrakeInput = -accelBrakeInput;
                targetAccelInput = 0;
            }
            else
            {
                // pressing backward while moving backward : accelerate (in reverse direction)
                BrakeInput = 0;
                targetAccelInput = accelBrakeInput;
                reversing = true;
            }
        }
        // smoothly move the current accel towards the target accel value.
        AccelInput = Mathf.MoveTowards(AccelInput, targetAccelInput, Time.deltaTime * advanced.accelChangeSmoothing);
    }

    void CalculateSpeedValues()
    {
        // current speed is measured in the forward direction of the car (sliding sideways doesn't count!)
        CurrentSpeed = transform.InverseTransformDirection(rigidbody.velocity).z;
        // speedfactor is a normalized representation of speed in relation to max speed:
        SpeedFactor = Mathf.InverseLerp(0, reversing ? maxReversingSpeed : maxSpeed, Mathf.Abs(CurrentSpeed));
        curvedSpeedFactor = reversing ? 0 : CurveFactor(SpeedFactor);
    }

    void HandleGearChanging()
    {
        // change gear, when appropriate (if speed has risen above or below the current gear's range, as stored in the gearDistribution array)
        if (!reversing)
        {
            if (SpeedFactor < gearDistribution[GearNum] && GearNum > 0)
                GearNum--;
            if (SpeedFactor > gearDistribution[GearNum + 1] && AvgSkid < advanced.skidGearLockFactor && GearNum < advanced.numGears - 1)
                GearNum++;
        }
    }

    void CalculateGearFactor()
    {
        // gear factor is a normalised representation of the current speed within the current gear's range of speeds.
        // We smooth towards the 'target' gear factor, so that revs don't instantly snap up or down when changing gear.
        var targetGearFactor = Mathf.InverseLerp(gearDistribution[GearNum], gearDistribution[GearNum + 1], Mathf.Abs(AvgPowerWheelRpmFactor));
        GearFactor = Mathf.Lerp(GearFactor, targetGearFactor, Time.deltaTime * advanced.gearFactorSmoothing);
    }

    void ProcessWheels(float steerInput)
    {
        // Process each wheel:
        // we accumulate some averages of all wheels into these vars:
        AvgPowerWheelRpmFactor = 0;
        AvgSkid = 0;
        var numPowerWheels = 0;
        anyOnGround = false;
        foreach (var wheel in wheels)
        {
            var wheelCollider = wheel.wheelCollider;
            if (wheel.steerable)
            {
                // apply steering to this wheel. The actual steering change applied is based on the steering range, current speed, 
                // and whether the wheel is currently pointing in the direction that steering is being applied
                var currentSteerSpeed = Mathf.Lerp(steeringResponseSpeed, steeringResponseSpeed * maxSpeedSteerResponse, curvedSpeedFactor);
                var currentMaxAngle = Mathf.Lerp(maxSteerAngle, maxSteerAngle * maxSpeedSteerAngle, curvedSpeedFactor);
                // auto-correct steering to centre if no steering input:
                if (steerInput == 0)
                {
                    currentSteerSpeed *= advanced.steeringCorrection;
                }
                // increase steering speed if steering input is in opposite direction to current wheel direction (for faster response)
                if (Mathf.Sign(steerInput) != Mathf.Sign(CurrentSteerAngle))
                {
                    currentSteerSpeed *= advanced.oppositeLockSteeringCorrection;
                }
                // modify the actual steer angle of the wheel by these calculated values:
                CurrentSteerAngle = Mathf.MoveTowards(CurrentSteerAngle, steerInput * currentMaxAngle, Time.deltaTime * currentSteerSpeed);
                wheelCollider.steerAngle = CurrentSteerAngle;
            }
            // acumulate skid amount from this wheel, for averaging later
            AvgSkid += wheel.SkidFactor;
            if (wheel.powered)
            {
                // apply power to wheels marked as powered:
                // available torque drops off as we approach max speed
                var currentMaxTorque = Mathf.Lerp(maxTorque, (SpeedFactor < 1) ? minTorque : 0, reversing ? SpeedFactor : curvedSpeedFactor);
                wheelCollider.motorTorque = AccelInput * currentMaxTorque;
                // accumulate RPM from this wheel, for averaging later
                AvgPowerWheelRpmFactor += wheel.Rpm / wheel.MaxRpm;
                numPowerWheels++;
            }
            // apply curent brake torque to wheel
            wheelCollider.brakeTorque = BrakeInput * brakePower;
            // if any wheel is on the ground, the car is considered grounded
            if (wheel.OnGround)
            {
                anyOnGround = true;
            }
        }
        // average the accumulated wheel values
        AvgPowerWheelRpmFactor /= numPowerWheels;
        AvgSkid /= wheels.Length;
    }

    void ApplyDownforce()
    {
        // apply downforce
        if (anyOnGround)
        {
            rigidbody.AddForce(-transform.up * curvedSpeedFactor * advanced.downForce);
        }
    }

    void CalculateRevs()
    {
        // calculate engine revs (for display / sound)
        // (this is done in retrospect - revs are not used in force/power calculations)
        var gearNumFactor = GearNum / (float)NumGears;
        var revsRangeMin = ULerp(0f, advanced.revRangeBoundary, CurveFactor(gearNumFactor));
        var revsRangeMax = ULerp(advanced.revRangeBoundary, 1f, gearNumFactor);
        RevsFactor = ULerp(revsRangeMin, revsRangeMax, GearFactor);
    }

    void PreserveDirectionInAir(float accelBrakeInput, float steerInput)
    {
        // special feature which allows cars to remain roughly pointing in the direction of travel
        if (!anyOnGround && preserveDirectionWhileInAir && rigidbody.velocity.magnitude > smallSpeed)
        {
            rigidbody.MoveRotation(Quaternion.Slerp(rigidbody.rotation, Quaternion.LookRotation(rigidbody.velocity), Time.deltaTime));
            rigidbody.angularVelocity = Vector3.Lerp(rigidbody.angularVelocity, Vector3.zero, Time.deltaTime);
        }
        if (!anyOnGround)
        {
            steerInput /= 2;
            rigidbody.AddTorque(transform.up * steerInput * rotationHorizontale);
            rigidbody.AddTorque(transform.right * accelBrakeInput * rotationVerticale);
        }
    }

    // simple function to add a curved bias towards 1 for a value in the 0-1 range
    float CurveFactor(float factor)
    {
        return 1 - (1 - factor) * (1 - factor);
    }


    // unclamped version of Lerp, to allow value to exceed the from-to range
    float ULerp(float from, float to, float value)
    {
        return (1.0f - value) * from + value * to;
    }


    void SetUpGears()
    {
        // the gear distribution is a range of normalized values marking out where the gear changes should occur
        // over the normalized range of speeds for the car.
        // eg, if the bias is centred, 5 gears would be evenly distributed as 0-0.2, 0.2-0.4, 0.4-0.6, 0.6-0.8, 0.8-1
        // with a low bias, the gears are clumped towards the lower end of the speed range, and vice-versa for high bias.

        gearDistribution = new float[advanced.numGears + 1];
        for (int g = 0; g <= advanced.numGears; ++g)
        {
            float gearPos = g / (float)advanced.numGears;

            float lowBias = gearPos * gearPos * gearPos;
            float highBias = 1 - (1 - gearPos) * (1 - gearPos) * (1 - gearPos);

            if (advanced.gearDistributionBias < 0.5f)
            {
                gearPos = Mathf.Lerp(gearPos, lowBias, 1 - (advanced.gearDistributionBias * 2));
            }
            else
            {
                gearPos = Mathf.Lerp(gearPos, highBias, (advanced.gearDistributionBias - 0.5f) * 2);
            }

            gearDistribution[g] = gearPos;
        }
    }


    void OnDrawGizmosSelected()
    {
        // visualise the adjusted centre of mass in the editor
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(rigidbody.position + Vector3.up * adjustCentreOfMass, 0.2f);
    }

    // Immobilize can be called from other objects, if the car needs to be made uncontrollable
    // (eg, from asplosion!)
    public void Immobilize()
    {
        immobilized = true;
    }

    // Reset is called via the ObjectResetter script, if present.
    public void Reset()
    {
        immobilized = false;
    }
}
