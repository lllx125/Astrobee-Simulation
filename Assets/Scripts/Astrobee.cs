using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Astrobee : MonoBehaviour
{
    Rigidbody rb;

    public bool ideal = true;
    public GameObject thrusterEffectPrefab;

    public AudioSource thrusterSound;

    GameObject[] thrusterEffect;

    Vector3[] r; // Positions of force application
    Vector3[] F; // Force vectors

    // it avoid repeating opening of vents, the integer counts the number of vent that is opened
    // 0 means closed vent, >0 means open vent
    // the index range from 1~12
    int[] openVents;


    // the angle of the motor controlling the vent
    // 0 is fully closed, 1 is fully opened
    float[] servoAngle;
    // the velocity of the motor controlling the vent
    float[] servoVelocity;
    // the rotational velocity of the motor controlling the vent, units in rpm
    float motorVelocity_right;
    float motorAcceleration_right;
    float motorVelocity_left;
    float motorAcceleration_left;
    float[] forceMagnitude = new float[13];
    void Start()
    {
        // Look for the Rigidbody component in this GameObject
        rb = GetComponent<Rigidbody>();
        // Check if the component was found
        if (rb == null)
        {
            Debug.LogError("Rigidbody component not found on this GameObject.");
        }
        ResetObject();
        // initialize rigid body
        rb = GetComponent<Rigidbody>();
        // Set center of mass, mass, and moment of inertia from parameters
        rb.centerOfMass = Parameters.R;
        rb.mass = Parameters.M;
        rb.inertiaTensor = Parameters.I_value;
        rb.inertiaTensorRotation = Parameters.I_rotation;
        // Initialize positions (r) and force vectors (F)
        r = (Vector3[])Parameters.r.Clone();
        F = (Vector3[])Parameters.F.Clone();
        if (!ideal)
        {
            AddGaussianError(0.01f, 0.0001f, 0.001f, 0.005f, 0.01f, 0.01f);
        }
        InitializeThrusterEffects();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResetObject();
        }
        UpdateMotor();
        UpdateThrusterEffects();
        UpdateThrusterSound();
        ApplyForce();
    }

    void ResetObject()
    {
        // close all vents
        openVents = new int[13];
        // set initial angle to 0
        servoAngle = new float[13];
        // set all servo velocity to 0
        servoVelocity = new float[13];
        // set all motors to 0
        motorVelocity_right = 0f;
        motorVelocity_left = 0f;
        motorAcceleration_right = 0f;
        motorAcceleration_left = 0f;



        // Reset position to origin
        rb.position = Vector3.zero;

        // Reset velocity
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Reset rotation to identity (no rotation)
        rb.rotation = Quaternion.identity;
        //stop the engine sound 
        thrusterSound.Stop();
    }

    public void OpenVent(int vent)
    {
        openVents[vent]++;
        if (openVents[vent] != 1) return;
        servoVelocity[vent] = Parameters.servoVelocity[vent];
    }

    public void CloseVent(int vent)
    {
        openVents[vent]--;
        if (openVents[vent] != 0) return;
        servoVelocity[vent] = -Parameters.servoVelocity[vent];
    }

    /// Updates the motor and servo angles for each vent.
    /// Ensures servo angles remain within the range [0, 1] and adjusts velocities accordingly.
    /// Updates the left and right motor velocities based on their accelerations.
    void UpdateMotor()
    {
        // Update servo angles and velocities for each vent
        for (int vent = 1; vent <= 12; vent++)
        {
            servoAngle[vent] += servoVelocity[vent] * Time.deltaTime;

            // Clamp servo angles to the range [0, 1] and stop velocity if limits are reached
            if (servoAngle[vent] > 1)
            {
                servoAngle[vent] = 1;
                servoVelocity[vent] = 0;
            }
            if (servoAngle[vent] < 0)
            {
                servoAngle[vent] = 0;
                servoVelocity[vent] = 0;
            }
        }

        // Update left motor velocity
        motorVelocity_left += motorAcceleration_left * Time.deltaTime;
        if (motorVelocity_left > Parameters.targetMotorVelocity)
        {
            motorVelocity_left = Parameters.targetMotorVelocity;
            motorAcceleration_left = 0;
        }
        if (motorVelocity_left < 0)
        {
            motorVelocity_left = 0;
            motorAcceleration_left = 0;
        }

        // Update right motor velocity
        motorVelocity_right += motorAcceleration_right * Time.deltaTime;
        if (motorVelocity_right > Parameters.targetMotorVelocity)
        {
            motorVelocity_right = Parameters.targetMotorVelocity;
            motorAcceleration_right = 0;
        }
        if (motorVelocity_right < 0)
        {
            motorVelocity_right = 0;
            motorAcceleration_right = 0;
        }
    }

    void ApplyForce()
    {
        // compute the total area of left and right side.
        float A_left = 0f;
        float A_right = 0f;
        for (int vent = 1; vent <= 6; vent++)
        {
            A_left += Parameters.AngleToArea(servoAngle[vent], vent);
        }
        for (int vent = 7; vent <= 12; vent++)
        {
            A_right += Parameters.AngleToArea(servoAngle[vent], vent);
        }
        // turn on the motor if the area is not 0, else turn off the motor
        if (A_left > 0.001f && motorVelocity_left < Parameters.targetMotorVelocity)
        {
            motorAcceleration_left = Parameters.motorAcceleration;
        }
        else if (A_left < 0.001f && motorVelocity_left > 0.001f)
        {
            motorAcceleration_left = -Parameters.motorAcceleration;
        }
        if (A_right > 0.001f && motorVelocity_right < Parameters.targetMotorVelocity)
        {
            motorAcceleration_right = Parameters.motorAcceleration;
        }
        else if (A_right < 0.001f && motorVelocity_right > 0.001f)
        {
            motorAcceleration_right = -Parameters.motorAcceleration;
        }
        for (int vent = 1; vent <= 6; vent++)
        {
            if (A_left > 0.001f)
            {
                forceMagnitude[vent] = Parameters.K * motorVelocity_left * motorVelocity_left * Parameters.AngleToArea(servoAngle[vent], vent) / (A_left * A_left);
            }
            else
            {
                forceMagnitude[vent] = 0f; // No force if area is zero
            }
        }
        for (int vent = 7; vent <= 12; vent++)
        {
            if (A_right > 0.001f)
            {
                forceMagnitude[vent] = Parameters.K * motorVelocity_right * motorVelocity_right * Parameters.AngleToArea(servoAngle[vent], vent) / (A_right * A_right);
            }
            else
            {
                forceMagnitude[vent] = 0f; // No force if area is zero
            }
        }

        // Apply forces
        for (int vent = 1; vent <= 12; vent++)
        {
            Vector3 worldForce = transform.TransformDirection(F[vent]) * forceMagnitude[vent];  // Scale and convert force from local to world
            Vector3 worldPosition = transform.TransformPoint(r[vent]);   // Convert position from local to world
            rb.AddForceAtPosition(worldForce, worldPosition, ForceMode.Force);
        }

    }

    void InitializeThrusterEffects()
    {
        thrusterEffect = new GameObject[13];
        for (int i = 1; i <= 12; i++)
        {
            // Instantiate the thruster effect prefab for each vent
            thrusterEffect[i] = Instantiate(thrusterEffectPrefab, transform);
            thrusterEffect[i].name = $"ThrusterEffect_{i}";

            // Set the initial position and orientation of the thruster effect
            thrusterEffect[i].transform.localPosition = r[i];
            thrusterEffect[i].transform.localRotation = Quaternion.LookRotation(-F[i]);

            // Disable the thruster effect initially
            thrusterEffect[i].SetActive(false);
        }
    }

    void UpdateThrusterEffects()
    {
        for (int i = 1; i <= 12; i++)
        {
            // Calculate the force scale
            float forceScale = forceMagnitude[i];

            if (forceScale > 0.001f)
            {
                // Enable the thruster effect and scale it based on the force
                thrusterEffect[i].SetActive(true);
                thrusterEffect[i].transform.localScale = Vector3.one * forceScale * 1f;
            }
            else
            {
                // Disable the thruster effect if forceScale is 0
                thrusterEffect[i].SetActive(false);
            }
        }
    }

    void UpdateThrusterSound()
    {
        float volumeScale = 0f;

        // Calculate the volumescale by summing up the force scales of all vents
        for (int i = 1; i <= 12; i++)
        {
            volumeScale += forceMagnitude[i];
            if (volumeScale > 1f)
            {
                volumeScale = 1f;
                break;
            }
        }

        // If the volumescale is greater than a small threshold, update the sound
        if (volumeScale > 0.001f)
        {
            thrusterSound.volume = volumeScale * 5; // Set volume proportional to total force scale
            if (!thrusterSound.isPlaying)
            {
                thrusterSound.Play(); // Start playing the sound if not already playing
            }
        }
        else
        {
            thrusterSound.Stop(); // Stop the sound if total force scale is zero
        }
    }

    void AddGaussianError(float comMagnitude, float inertiaMagnitude, float rotationMagnitude, float positionMagnitude, float forceMagnitude, float massMagnitude)
    {
        // Add Gaussian error to the mass
        float massError = GaussianRandom(massMagnitude);
        rb.mass += massError;

        // Add Gaussian error to the center of mass
        Vector3 comError = new Vector3(
            GaussianRandom(comMagnitude),
            GaussianRandom(comMagnitude),
            GaussianRandom(comMagnitude)
        );
        rb.centerOfMass += comError;

        // Add Gaussian error to the moment of inertia tensor
        Vector3 inertiaError = new Vector3(
            GaussianRandom(inertiaMagnitude),
            GaussianRandom(inertiaMagnitude),
            GaussianRandom(inertiaMagnitude)
        );
        rb.inertiaTensor += inertiaError;

        // Add Gaussian error to the rotation
        Quaternion rotationError = new Quaternion(
            GaussianRandom(rotationMagnitude),
            GaussianRandom(rotationMagnitude),
            GaussianRandom(rotationMagnitude),
            GaussianRandom(rotationMagnitude)
        );
        rb.inertiaTensorRotation *= rotationError;

        // Add Gaussian error to each element of r
        for (int i = 0; i < r.Length; i++)
        {
            r[i] += new Vector3(
            GaussianRandom(positionMagnitude),
            GaussianRandom(positionMagnitude),
            GaussianRandom(positionMagnitude)
            );
        }

        // Add Gaussian error to each element of F
        for (int i = 0; i < F.Length; i++)
        {
            F[i] += new Vector3(
            GaussianRandom(forceMagnitude),
            GaussianRandom(forceMagnitude),
            GaussianRandom(forceMagnitude)
            );
        }
    }

    float GaussianRandom(float magnitude)
    {
        // Generate a Gaussian random number with mean 0 and standard deviation 1
        float u1 = 1.0f - Random.Range(0.0f, 1.0f);
        float u2 = 1.0f - Random.Range(0.0f, 1.0f);
        float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);

        // Scale by the magnitude
        return randStdNormal * magnitude;
    }

}
