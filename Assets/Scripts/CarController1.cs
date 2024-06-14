using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController1 : MonoBehaviour
{
    public GameObject frontLeftMesh;
    public GameObject frontRightMesh;
    public GameObject rearLeftMesh;
    public GameObject rearRightMesh;
    public WheelCollider frontLeftCollider;
    public WheelCollider frontRightCollider;
    public WheelCollider rearLeftCollider;
    public WheelCollider rearRightCollider;

    public float maxSpeed = 30f;
    public float motorForce = 1000f;
    public float brakeForce = 3000f;
    public float maxSteerAngle = 30f;
    public float currentSpeed;

    private Rigidbody rb;

    public float horizontalInput=0;
    public float verticalInput=0;
    public bool isBraking=false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // GetInput();
        HandleMotor();
        HandleSteering();
        UpdateWheels();
    }

    // private void GetInput()
    // {
    //     horizontalInput = Input.GetAxis("Horizontal");
    //     verticalInput = Input.GetAxis("Vertical");
    //     isBraking = Input.GetKey(KeyCode.Space);
    // }

    private void HandleMotor()
    {
        currentSpeed = rb.velocity.magnitude;

        bool isChangingDirection = (verticalInput < 0 && rb.velocity.z > 0) || (verticalInput > 0 && rb.velocity.z < 0);

        float appliedMotorForce = isChangingDirection ? motorForce * 25 : motorForce;

        frontLeftCollider.motorTorque = verticalInput * appliedMotorForce;
        frontRightCollider.motorTorque = verticalInput * appliedMotorForce;

        if (currentSpeed > maxSpeed)
        {
            frontLeftCollider.motorTorque = 0;
            frontRightCollider.motorTorque = 0;
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }

        if (isBraking)
        {
            frontLeftCollider.brakeTorque = brakeForce;
            frontRightCollider.brakeTorque = brakeForce;
            rearLeftCollider.brakeTorque = brakeForce;
            rearRightCollider.brakeTorque = brakeForce;
        }
        else
        {
            frontLeftCollider.brakeTorque = 0;
            frontRightCollider.brakeTorque = 0;
            rearLeftCollider.brakeTorque = 0;
            rearRightCollider.brakeTorque = 0;
        }
    }

    private void HandleSteering()
    {
        float steerAngle = maxSteerAngle * horizontalInput;
        frontLeftCollider.steerAngle = steerAngle;
        frontRightCollider.steerAngle = steerAngle;
    }

    private void UpdateWheels()
    {
        UpdateWheelPose(frontLeftCollider, frontLeftMesh);
        UpdateWheelPose(frontRightCollider, frontRightMesh);
        UpdateWheelPose(rearLeftCollider, rearLeftMesh);
        UpdateWheelPose(rearRightCollider, rearRightMesh);
    }

    private void UpdateWheelPose(WheelCollider collider, GameObject mesh)
    {
        Vector3 pos;
        Quaternion quat;
        collider.GetWorldPose(out pos, out quat);

        mesh.transform.position = pos;
        mesh.transform.rotation = quat;
    }
    public void StopCar()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        frontLeftCollider.brakeTorque = brakeForce;
        frontRightCollider.brakeTorque = brakeForce;
        rearLeftCollider.brakeTorque = brakeForce;
        rearRightCollider.brakeTorque = brakeForce;

        frontLeftCollider.motorTorque = 0;
        frontRightCollider.motorTorque = 0;
        rearLeftCollider.motorTorque = 0;
        rearRightCollider.motorTorque = 0;

        frontLeftCollider.rotationSpeed = 0;
        frontRightCollider.rotationSpeed = 0;
        rearLeftCollider.rotationSpeed = 0;
        rearRightCollider.rotationSpeed = 0;
    }
}
