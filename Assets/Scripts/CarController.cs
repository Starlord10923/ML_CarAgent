using System;
using UnityEngine;
using UnityEngine.UI;

public class CarController : MonoBehaviour
{
    [Range(20, 400)]
    public int maxSpeed = 90;
    [Range(10, 120)]
    public int maxReverseSpeed = 45;
    [Range(1, 50)]
    public int accelerationMultiplier = 2;
    [Range(10, 45)]
    public int maxSteeringAngle = 27;
    [Range(0.1f, 1f)]
    public float steeringSpeed = 0.5f;
    [Range(100, 600)]
    public int brakeForce = 350;
    [Range(1, 50)]
    public int decelerationMultiplier = 2;
    [Range(1, 50)]
    public int handbrakeDriftMultiplier = 5;
    public Vector3 bodyMassCenter;


    [Header("WHEELS")]
    public GameObject frontLeftMesh;
    public WheelCollider frontLeftCollider;
    [Space(10)]
    public GameObject frontRightMesh;
    public WheelCollider frontRightCollider;
    [Space(10)]
    public GameObject rearLeftMesh;
    public WheelCollider rearLeftCollider;
    [Space(10)]
    public GameObject rearRightMesh;
    public WheelCollider rearRightCollider;

    //CAR DATA

    [HideInInspector]
    public float carSpeed; // Used to store the speed of the car.
    [HideInInspector]
    public bool isDrifting; // Used to know whether the car is drifting or not.
    [HideInInspector]
    public bool isTractionLocked; // Used to know whether the traction of the car is locked or not.

    //PRIVATE VARIABLES
    Rigidbody carRigidbody; // Stores the car's rigidbody.
    float steeringAxis; // Used to know whether the steering wheel has reached the maximum value. It goes from -1 to 1.
    float throttleAxis; // Used to know whether the throttle has reached the maximum value. It goes from -1 to 1.
    float driftingAxis;
    float localVelocityZ;
    float localVelocityX;
    bool deceleratingCar;

    WheelFrictionCurve FLwheelFriction;
    float FLWextremumSlip;
    WheelFrictionCurve FRwheelFriction;
    float FRWextremumSlip;
    WheelFrictionCurve RLwheelFriction;
    float RLWextremumSlip;
    WheelFrictionCurve RRwheelFriction;
    float RRWextremumSlip;

    public float horizontalInput=0;
    public float verticalInput=0;
    public bool space=false;

    void Start()
    {
        carRigidbody = gameObject.GetComponent<Rigidbody>();
        carRigidbody.centerOfMass = bodyMassCenter;

        FLwheelFriction = new WheelFrictionCurve();
        FLwheelFriction.extremumSlip = frontLeftCollider.sidewaysFriction.extremumSlip;
        FLWextremumSlip = frontLeftCollider.sidewaysFriction.extremumSlip;
        FLwheelFriction.extremumValue = frontLeftCollider.sidewaysFriction.extremumValue;
        FLwheelFriction.asymptoteSlip = frontLeftCollider.sidewaysFriction.asymptoteSlip;
        FLwheelFriction.asymptoteValue = frontLeftCollider.sidewaysFriction.asymptoteValue;
        FLwheelFriction.stiffness = frontLeftCollider.sidewaysFriction.stiffness;
        FRwheelFriction = new WheelFrictionCurve();
        FRwheelFriction.extremumSlip = frontRightCollider.sidewaysFriction.extremumSlip;
        FRWextremumSlip = frontRightCollider.sidewaysFriction.extremumSlip;
        FRwheelFriction.extremumValue = frontRightCollider.sidewaysFriction.extremumValue;
        FRwheelFriction.asymptoteSlip = frontRightCollider.sidewaysFriction.asymptoteSlip;
        FRwheelFriction.asymptoteValue = frontRightCollider.sidewaysFriction.asymptoteValue;
        FRwheelFriction.stiffness = frontRightCollider.sidewaysFriction.stiffness;
        RLwheelFriction = new WheelFrictionCurve();
        RLwheelFriction.extremumSlip = rearLeftCollider.sidewaysFriction.extremumSlip;
        RLWextremumSlip = rearLeftCollider.sidewaysFriction.extremumSlip;
        RLwheelFriction.extremumValue = rearLeftCollider.sidewaysFriction.extremumValue;
        RLwheelFriction.asymptoteSlip = rearLeftCollider.sidewaysFriction.asymptoteSlip;
        RLwheelFriction.asymptoteValue = rearLeftCollider.sidewaysFriction.asymptoteValue;
        RLwheelFriction.stiffness = rearLeftCollider.sidewaysFriction.stiffness;
        RRwheelFriction = new WheelFrictionCurve();
        RRwheelFriction.extremumSlip = rearRightCollider.sidewaysFriction.extremumSlip;
        RRWextremumSlip = rearRightCollider.sidewaysFriction.extremumSlip;
        RRwheelFriction.extremumValue = rearRightCollider.sidewaysFriction.extremumValue;
        RRwheelFriction.asymptoteSlip = rearRightCollider.sidewaysFriction.asymptoteSlip;
        RRwheelFriction.asymptoteValue = rearRightCollider.sidewaysFriction.asymptoteValue;
        RRwheelFriction.stiffness = rearRightCollider.sidewaysFriction.stiffness;
    }

    void Update()
    {
        carSpeed = (2 * Mathf.PI * frontLeftCollider.radius * frontLeftCollider.rpm * 60) / 1000;
        localVelocityX = transform.InverseTransformDirection(carRigidbody.velocity).x;
        localVelocityZ = transform.InverseTransformDirection(carRigidbody.velocity).z;

        Turn(horizontalInput);
        if (verticalInput != 0)
            MoveCar(verticalInput);
        else
            ThrottleOff();

        if (space)
        {
            CancelInvoke("DecelerateCar");
            deceleratingCar = false;
            Handbrake();
        }
        if (!space && isTractionLocked)
        {
            RecoverTraction();
        }
        if (verticalInput == 0 && !space && !deceleratingCar)
        {
            InvokeRepeating("DecelerateCar", 0f, 0.1f);
            deceleratingCar = true;
        }
        if (horizontalInput == 0 && steeringAxis != 0f)
        {
            ResetSteeringAngle();
        }

        AnimateWheelMeshes();
    }
    //
    //STEERING METHODS
    //

    public void Turn(float horizontalInput)
    {
        steeringAxis += horizontalInput * Time.deltaTime * 10f * steeringSpeed;
        steeringAxis = Mathf.Clamp(steeringAxis, -1f, 1f);

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
            steeringAxis += Time.deltaTime * 10f * steeringSpeed;
        }
        else if (steeringAxis > 0f)
        {
            steeringAxis -= Time.deltaTime * 10f * steeringSpeed;
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
        try
        {
            frontLeftCollider.GetWorldPose(out Vector3 FLWPosition, out Quaternion FLWRotation);
            frontLeftMesh.transform.position = FLWPosition;
            frontLeftMesh.transform.rotation = FLWRotation;

            frontRightCollider.GetWorldPose(out Vector3 FRWPosition, out Quaternion FRWRotation);
            frontRightMesh.transform.position = FRWPosition;
            frontRightMesh.transform.rotation = FRWRotation;

            rearLeftCollider.GetWorldPose(out Vector3 RLWPosition, out Quaternion RLWRotation);
            rearLeftMesh.transform.position = RLWPosition;
            rearLeftMesh.transform.rotation = RLWRotation;

            rearRightCollider.GetWorldPose(out Vector3 RRWPosition, out Quaternion RRWRotation);
            rearRightMesh.transform.position = RRWPosition;
            rearRightMesh.transform.rotation = RRWRotation;
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex);
        }
    }

    public void MoveCar(float verticalInput)
    {
        isDrifting = Mathf.Abs(localVelocityX) > 2.5f;

        throttleAxis += Mathf.Sign(verticalInput) * Time.deltaTime * 3f;
        throttleAxis = Mathf.Clamp(throttleAxis, -1f, 1f);

        if (verticalInput * localVelocityZ < 0)
            Brakes();

        if (Mathf.Abs(Mathf.RoundToInt(carSpeed)) < maxSpeed && Mathf.Abs(Mathf.RoundToInt(carSpeed)) < maxReverseSpeed)
        {
            frontLeftCollider.brakeTorque = 0;
            frontLeftCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
            frontRightCollider.brakeTorque = 0;
            frontRightCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
            rearLeftCollider.brakeTorque = 0;
            rearLeftCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
            rearRightCollider.brakeTorque = 0;
            rearRightCollider.motorTorque = (accelerationMultiplier * 50f) * throttleAxis;
        }
        else
        {
            // If already at max speed, stop applying motor torque
            frontLeftCollider.motorTorque = 0;
            frontRightCollider.motorTorque = 0;
            rearLeftCollider.motorTorque = 0;
            rearRightCollider.motorTorque = 0;
        }
    }

    public void ThrottleOff()
    {
        frontLeftCollider.motorTorque = 0;
        frontRightCollider.motorTorque = 0;
        rearLeftCollider.motorTorque = 0;
        rearRightCollider.motorTorque = 0;
    }

    public void DecelerateCar()
    {
        isDrifting = Mathf.Abs(localVelocityX) > 2.5f;

        // The following part resets the throttle power to 0 smoothly.
        if (throttleAxis != 0f)
        {
            throttleAxis += -Mathf.Sign(throttleAxis) * Time.deltaTime * 10f;
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
            CancelInvoke("DecelerateCar");
        }
    }

    public void Brakes()
    {
        frontLeftCollider.brakeTorque = brakeForce;
        frontRightCollider.brakeTorque = brakeForce;
        rearLeftCollider.brakeTorque = brakeForce;
        rearRightCollider.brakeTorque = brakeForce;
    }

    public void Handbrake()
    {
        CancelInvoke("RecoverTraction");
        driftingAxis += Time.deltaTime;
        float secureStartingPoint = driftingAxis * FLWextremumSlip * handbrakeDriftMultiplier;

        if (secureStartingPoint < FLWextremumSlip)
            driftingAxis = FLWextremumSlip / (FLWextremumSlip * handbrakeDriftMultiplier);
        if (driftingAxis > 1f)
            driftingAxis = 1f;

        isDrifting = Mathf.Abs(localVelocityX) > 2.5f;

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

        isTractionLocked = true;
    }

    public void RecoverTraction()
    {
        isTractionLocked = false;
        driftingAxis -= (Time.deltaTime / 1.5f);
        if (driftingAxis < 0f)
            driftingAxis = 0f;

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

            Invoke("RecoverTraction", Time.deltaTime);

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
        }
    }
}
