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
    float[] motorAngle;
    // the velocity of the motor controlling the vent
    float[] motorVelocity;
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
        // Iterate through each vent to update motor angle and velocity
        for (int i = 1; i <= 12; i++)
        {
            // Update the motor angle based on its velocity
            motorAngle[i] += motorVelocity[i] * Time.deltaTime;

            // If the motor angle reaches or exceeds 1 (fully open)
            if (motorAngle[i] >= 1f)
            {
                motorAngle[i] = 1f; // Clamp the angle to 1
                motorVelocity[i] = 0f; // Stop the motor by setting velocity to 0
            }
            // If the motor angle reaches or goes below 0 (fully closed)
            else if (motorAngle[i] <= 0f)
            {
                motorAngle[i] = 0f; // Clamp the angle to 0
                motorVelocity[i] = 0f; // Stop the motor by setting velocity to 0
            }
        }
        // Iterate through each vent to apply force
        for (int i = 1; i <= 12; i++)
        {
            // Apply force only if the vent is open (motor angle > 0.001)
            if (motorAngle[i] > 0.001f)
            {
                ApplyForce(i);
            }
        }
        UpdateThrusterEffects();
        UpdateThrusterSound();

    }

    void ResetObject()
    {
        // close all vents
        openVents = new int[13];
        // set initial angle to 0
        motorAngle = new float[13];
        // set all motor velocity to 0
        motorVelocity = new float[13];

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
        motorVelocity[vent] = Parameters.motorVelocity[vent];
    }

    public void CloseVent(int vent)
    {
        openVents[vent]--;
        if (openVents[vent] != 0) return;
        motorVelocity[vent] = -Parameters.motorVelocity[vent];
    }


    void ApplyForce(int vent)
    {
        float forceScale = Parameters.AngleToForce(motorAngle[vent]);
        Vector3 worldForce = transform.TransformDirection(F[vent]) * forceScale;  // Scale and convert force from local to world
        Vector3 worldPosition = transform.TransformPoint(r[vent]);   // Convert position from local to world
        rb.AddForceAtPosition(worldForce, worldPosition, ForceMode.Force);
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
            float forceScale = Parameters.AngleToForce(motorAngle[i]);

            if (forceScale > 0.001f)
            {
                // Enable the thruster effect and scale it based on the force
                thrusterEffect[i].SetActive(true);
                thrusterEffect[i].transform.localScale = Vector3.one * forceScale * 0.1f;
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
            volumeScale += Parameters.AngleToForce(motorAngle[i]);
            if (volumeScale > 1f)
            {
                volumeScale = 1f;
                break;
            }
        }

        // If the volumescale is greater than a small threshold, update the sound
        if (volumeScale > 0.001f)
        {
            thrusterSound.volume = volumeScale; // Set volume proportional to total force scale
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
