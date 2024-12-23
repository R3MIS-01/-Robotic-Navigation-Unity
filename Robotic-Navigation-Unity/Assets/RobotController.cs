using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class RobotController : MonoBehaviour
{
    // naming constraints do not change
    [SerializeField] private WheelCollider FLC;
    [SerializeField] private WheelCollider FRC;
    [SerializeField] private WheelCollider RLC;
    [SerializeField] private WheelCollider RRC;

    [SerializeField] private Transform FLT;
    [SerializeField] private Transform FRT;
    [SerializeField] private Transform RLT;
    [SerializeField] private Transform RRT;

    [SerializeField] private Transform FRS;
    [SerializeField] private Transform L1S;
    [SerializeField] private Transform L2S;
    [SerializeField] private Transform L3S;
    [SerializeField] private Transform R1S;
    [SerializeField] private Transform R2S;
    [SerializeField] private Transform R3S;
    [SerializeField] private Transform ORS;


    [SerializeField] private float maxSteeringAngle = 30;
    [SerializeField] private float motorForce = 50;
    [SerializeField] private float brakeForce;

    private Rigidbody rb;

    [SerializeField] private float angle_x;
    [SerializeField] private float angle_z;
    [SerializeField] private float velocity;

    private float steerAngle;
    private bool isBreaking;

    private float s1dist = 5;
    private float s2dist = 6;
    private float s3dist = 6;

    [SerializeField] private LayerMask roadLayer;
    [SerializeField] private LayerMask obsLayer;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        float s1x = 0; float s1y = 10; float s1z = 0;
        float s2x = 8; float s2y = 30; float s2z = 0;
        float s3x = 16; float s3y = 60; float s3z = 0;

        AdjustSensors(FRS, 20, 0, 0);
        AdjustSensors(L1S, s1x, -s1y, s1z);
       AdjustSensors(L2S, s2x, -s2y, s2z);
        AdjustSensors(L3S, s3x, -s3y, s3z);
        AdjustSensors(R1S, s1x, s1y, s1z);
        AdjustSensors(R2S, s2x, s2y, s2z);
        AdjustSensors(R3S, s3x, s3y, s3z);
        AdjustSensors(ORS, 50, 180, 0);

        // Initialize layer masks
        obsLayer = LayerMask.GetMask("Obs");
        roadLayer = LayerMask.GetMask("Road");
    }
    

    private void FixedUpdate()
    {
        StayOnRoad();
        AvoidObstacles();
        AdjustSpeed();
        HandleMotor();
        UpdateWheels();
        brake();


        angle_x = ORS.eulerAngles.x;
        angle_z = ORS.eulerAngles.z;

        velocity = rb.velocity.magnitude;
    }

    private void AdjustSensors(Transform sensor, float x_angle, float y_angle, float z_angle)
    {
        sensor.transform.Rotate(x_angle, y_angle, z_angle);
    }

    private void HandleMotor()
    {
        float CurrentAcceleration;

        CurrentAcceleration = isBreaking ? 0 : motorForce;
        FLC.motorTorque = CurrentAcceleration;
        FRC.motorTorque = CurrentAcceleration;
        RLC.motorTorque = CurrentAcceleration;
        RRC.motorTorque = CurrentAcceleration;

        brakeForce = isBreaking ? 3000f : 0f;
        FLC.brakeTorque = brakeForce;
        FRC.brakeTorque = brakeForce;
        RLC.brakeTorque = brakeForce;
        RRC.brakeTorque = brakeForce;
    }

    private void UpdateWheels()
    {
        UpdateWheelPos(FLC, FLT);
        UpdateWheelPos(FRC, FRT);
        UpdateWheelPos(RLC, RLT);
        UpdateWheelPos(RRC, RRT);
    }

    private void UpdateWheelPos(WheelCollider wheelCollider, Transform trans)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        trans.rotation = rot;
        trans.position = pos;
    }

    private void HandleSteering(float direction)
    {
        steerAngle = maxSteeringAngle * direction;
        FLC.steerAngle = steerAngle;
        FRC.steerAngle = steerAngle;
    }

    private bool sense(Transform sensor, float dist, string layer)
    {
        LayerMask mask = LayerMask.GetMask(layer);  //the layer mask enables the sensor to detect the road
        if (Physics.Raycast(sensor.position, sensor.TransformDirection(Vector3.forward), dist, mask))
        {
            Debug.DrawRay(sensor.position, sensor.TransformDirection(Vector3.forward) * dist, Color.yellow);
            return true;

        }
        else
        {
            Debug.DrawRay(sensor.position, sensor.TransformDirection(Vector3.forward) * dist, Color.white);
            return false;

        }
    }

    

    private void StayOnRoad()
    {
        if (!sense(L3S, s3dist, "Road") || !sense(R3S, s3dist, "Road"))
        {

            if (!sense(L3S, s3dist, "Road"))
            {
                HandleSteering(1);
            }
            if (!sense(R3S, s3dist, "Road"))
            {
                HandleSteering(-1);
            }
        }
        else
        {
            HandleSteering(0);
        }
    }

    private void AdjustSpeed()
    {

        if (velocity < 2 && motorForce <= 50)
        {
            motorForce = motorForce + 340f;

        }

        else if (velocity > 6 && motorForce > 0)
        {
            motorForce = motorForce - 30f;
        }

        else if (velocity > 12 && motorForce < 0)
        {
            motorForce = motorForce - 80f;
        }

    }

    private void AvoidObstacles()
    {
        if (sense(L1S, s1dist, "Obs"))
        {
            HandleSteering(1);
        }
        if (sense(R1S, s1dist, "Obs"))
        {
            HandleSteering(-1);
        }
        //
        if (sense(L2S, s2dist, "Obs"))
        {
            HandleSteering(1);
        }
        if (sense(R2S, s2dist, "Obs"))
        {
            HandleSteering(-1);
        }

    }

    private void brake()
    {
        if (Input.GetKey(KeyCode.Space))

            brakeForce = 3000f;


        else
            brakeForce = 0f;


    }
}
