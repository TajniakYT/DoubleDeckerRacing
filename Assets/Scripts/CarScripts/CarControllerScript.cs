using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class CarControllerScript : MonoBehaviour, CarInputs.ICarInputsMapActions
{
    private CarInputs inputs;

    public enum DriveType { FWD, RWD, AWD }

    [Header("Wheel Colliders")]
    public WheelCollider frontLeftCollider;
    public WheelCollider frontRightCollider;
    public WheelCollider rearLeftCollider;
    public WheelCollider rearRightCollider;

    [Header("Wheel Meshes")]
    public Transform frontLeftWheel;
    public Transform frontRightWheel;
    public Transform rearLeftWheel;
    public Transform rearRightWheel;

    [Header("Car Settings")]
    public float motorForce = 1500f;
    public float brakeForce = 3000f;
    public float steerAngle = 30f;
    public float handbrakeForce = 5000f;

    [Header("Drivetrain")]
    public DriveType driveType;
    public float finalDrive = 3.46f;
    public float[] gearRatios;

    [Header("Engine")]
    public AnimationCurve torqueCurve;
    public float idleRPM = 900f;
    public float maxRPM = 7000f;
    public float engineRPM;

    [Header("Wheels")]
    public float wheelRadius = 0.34f;

    [Header("Smoothing")]
    public float inputSmooth = 6f;
    public float torqueSmooth = 8f;

    float smoothThrottle;
    float smoothSteer;
    float smoothBrake;
    float currentTorque;

    float throttleInput;
    float brakeInput;
    float steerInput;
    bool handbrake;

    int currentGear = 3;
    public int maxGear = 6;

    Rigidbody rb;

    void Awake()
    {
        inputs = new CarInputs();
        inputs.CarInputsMap.SetCallbacks(this);
        rb = GetComponent<Rigidbody>();

        rb.centerOfMass = new Vector3(0, -0.5f, 0);
    }

    void OnEnable() => inputs.Enable();
    void OnDisable() => inputs.Disable();

    void FixedUpdate()
    {
        SmoothInputs();
        CalculateEngineRPM();
        HandleMotor();
        HandleSteering();
        UpdateWheels();
    }

    #region INPUT CALLBACKS

    public void OnThrottle(InputAction.CallbackContext context)
    {
        throttleInput = context.ReadValue<float>();
    }

    public void OnBrake(InputAction.CallbackContext context)
    {
        brakeInput = context.ReadValue<float>();
    }

    public void OnSteer(InputAction.CallbackContext context)
    {
        steerInput = context.ReadValue<float>();
    }

    public void OnHandBrake(InputAction.CallbackContext context)
    {
        handbrake = context.ReadValueAsButton();
    }

    public void OnGearUp(InputAction.CallbackContext context)
    {
        if (context.performed)
            currentGear = Mathf.Clamp(currentGear + 1, 0, gearRatios.Length);
    }

    public void OnGearDown(InputAction.CallbackContext context)
    {
        if (context.performed)
            currentGear = Mathf.Clamp(currentGear - 1, 0, gearRatios.Length);
    }

    #endregion

    void SmoothInputs()
    {
        smoothThrottle = Mathf.Lerp(smoothThrottle, throttleInput, Time.fixedDeltaTime * inputSmooth);
        smoothSteer = Mathf.Lerp(smoothSteer, steerInput, Time.fixedDeltaTime * inputSmooth);
        smoothBrake = Mathf.Lerp(smoothBrake, brakeInput, Time.fixedDeltaTime * inputSmooth);
    }

    void CalculateEngineRPM()
    {
        float wheelRPM = Mathf.Abs(rearLeftCollider.rpm);

        float gearRatio = gearRatios[Mathf.Clamp(currentGear - 1, 0, gearRatios.Length - 1)];

        engineRPM = wheelRPM * gearRatio * finalDrive;

        engineRPM = Mathf.Clamp(engineRPM, idleRPM, maxRPM);
    }

    void HandleMotor()
    {
        float gearRatio = gearRatios[Mathf.Clamp(currentGear - 1, 0, gearRatios.Length - 1)];

        float engineTorque = torqueCurve.Evaluate(engineRPM);

        float targetTorque = engineTorque * gearRatio * finalDrive * smoothThrottle;

        currentTorque = Mathf.Lerp(currentTorque, targetTorque, Time.fixedDeltaTime * torqueSmooth);

        ApplyDriveTorque(currentTorque);

        float totalBrake = smoothBrake * brakeForce;

        if (handbrake)
            totalBrake = handbrakeForce;

        ApplyBrake(totalBrake);
    }

    void ApplyDriveTorque(float torque)
    {
        switch (driveType)
        {
            case DriveType.FWD:
                frontLeftCollider.motorTorque = torque;
                frontRightCollider.motorTorque = torque;
                break;

            case DriveType.RWD:
                rearLeftCollider.motorTorque = torque;
                rearRightCollider.motorTorque = torque;
                break;

            case DriveType.AWD:
                float splitTorque = torque * 0.5f;

                frontLeftCollider.motorTorque = splitTorque;
                frontRightCollider.motorTorque = splitTorque;
                rearLeftCollider.motorTorque = splitTorque;
                rearRightCollider.motorTorque = splitTorque;
                break;
        }
    }

    void ApplyBrake(float brake)
    {
        frontLeftCollider.brakeTorque = brake;
        frontRightCollider.brakeTorque = brake;
        rearLeftCollider.brakeTorque = brake;
        rearRightCollider.brakeTorque = brake;
    }

    void HandleSteering()
    {
        float steer = smoothSteer * steerAngle;

        frontLeftCollider.steerAngle = steer;
        frontRightCollider.steerAngle = steer;
    }

    void UpdateWheels()
    {
        UpdateWheel(frontLeftCollider, frontLeftWheel);
        UpdateWheel(frontRightCollider, frontRightWheel);
        UpdateWheel(rearLeftCollider, rearLeftWheel);
        UpdateWheel(rearRightCollider, rearRightWheel);
    }

    void UpdateWheel(WheelCollider col, Transform wheel)
    {
        Vector3 pos;
        Quaternion rot;
        col.GetWorldPose(out pos, out rot);
        wheel.position = pos;
        wheel.rotation = rot;
    }

    public int ReturnCurrentGear()
    {
        return currentGear;
    }

    public float ReturnCurrentSpeed()
    {
        return rb.linearVelocity.magnitude;
    }

    public float ReturnCurrenRPM()
    {
        return engineRPM;
    }
}