using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Astrobee : MonoBehaviour
{
    // geometric parameters (m)
    public float a = 0.1f;
    public float b = 0.05f;
    public float c = 0.15f;
    public float d = 0.08f;
    public float F0 = 1; // Force magnitude (N)

    public bool ideal = true;

    private Vector3[] r; // Positions of force application
    private Vector3[] F; // Force vectors

    private Rigidbody rb;

    public GameObject thrusterEffectPrefab;

    public AudioSource thruster;
    public float thrusterVolume = 0.7f;

    // Dictionary mapping key inputs to vent activations
    private Dictionary<KeyCode, int[]> keyToControl = new Dictionary<KeyCode, int[]>
    {
        { KeyCode.A, new int[]{2, 9} },   // +x
        { KeyCode.D, new int[]{4, 7} },   // -x
        { KeyCode.W, new int[]{11, 12} }, // -y
        { KeyCode.S, new int[]{5, 6} },   // +y
        { KeyCode.Q, new int[]{3, 10} },  // +z
        { KeyCode.E, new int[]{1, 8} },   // -z
        { KeyCode.I, new int[]{3, 8} },   // +Rx1
        { KeyCode.K, new int[]{1, 10} },  // -Rx1
        { KeyCode.J, new int[]{2, 4} },   // +Ry1
        { KeyCode.L, new int[]{1, 3} },   // -Ry1
        { KeyCode.U, new int[]{4, 9} },   // +Rz1
        { KeyCode.O, new int[]{2, 7} },    // -Rz1
        { KeyCode.Alpha1, new int[]{1} },  // vent 1
        { KeyCode.Alpha2, new int[]{2} },  // vent 2
        { KeyCode.Alpha3, new int[]{3} },  // vent 3
        { KeyCode.Alpha4, new int[]{4} },  // vent 4
        { KeyCode.Alpha5, new int[]{5} },  // vent 5
        { KeyCode.Alpha6, new int[]{6} },  // vent 6
        { KeyCode.Alpha7, new int[]{7} },  // vent 7
        { KeyCode.Alpha8, new int[]{8} },  // vent 8
        { KeyCode.Alpha9, new int[]{9} },  // vent 9
        { KeyCode.Alpha0, new int[]{10} }, // vent 10
        { KeyCode.Minus, new int[]{11} },  // vent 11
        { KeyCode.Equals, new int[]{12} }  // vent 12
    };

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Initialize positions (r) corresponding to each force application point
        r = new Vector3[]
        {
            new Vector3(-d, a, c),  // 1
            new Vector3(-c, a, d),  // 2
            new Vector3( d, a, -c), // 3
            new Vector3( c, a, -d), // 4
            new Vector3( b, a, b),  // 5
            new Vector3(-b, a, -b), // 6
            new Vector3( c, -a, d), // 7
            new Vector3( d, -a, c), // 8
            new Vector3(-c, -a, -d),// 9
            new Vector3(-d, -a, -c), // 10
            new Vector3(-b, -a, b), // 11
            new Vector3( b, -a, -b) // 12
        };

        // Initialize force vectors
        F = new Vector3[]
        {
            new Vector3( 0,  0, -F0), // 1
            new Vector3( F0,  0,  0), // 2
            new Vector3( 0,  0,  F0), // 3
            new Vector3(-F0,  0,  0), // 4
            new Vector3( 0, -F0,  0), // 5
            new Vector3( 0, -F0,  0), // 6
            new Vector3(-F0,  0,  0), // 7
            new Vector3( 0,  0, -F0), // 8
            new Vector3( F0,  0,  0), // 9
            new Vector3( 0,  0,  F0), // 10
            new Vector3( 0,  F0,  0), // 11
            new Vector3( 0,  F0,  0)  // 12
        };

        if (!ideal)
        {
            AddGaussianError(0.01f, 0.0001f, 0.001f, 0.005f, 0.01f, 0.01f);
        }

    }

    void Update()
    {
        bool anyKeyActive = false;

        foreach (var entry in keyToControl)
        {
            if (Input.GetKey(entry.Key))
            {
                anyKeyActive = true;
                if (thruster != null && !thruster.isPlaying)
                {
                    thruster.volume = thrusterVolume;
                    thruster.Play();
                }
                ApplyForces(entry.Value);
            }
        }

        if (!anyKeyActive && thruster != null && thruster.isPlaying)
        {
            StartCoroutine(FadeOutThrusterSound());
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResetObject();
        }
    }

    IEnumerator FadeOutThrusterSound()
    {

        while (thruster.volume > 0)
        {
            thruster.volume -= thrusterVolume * Time.deltaTime / 2.0f; // 1.0f is the fade out duration
            yield return null;
        }

        thruster.Stop();
    }

    void ResetObject()
    {
        // Reset position to origin
        rb.position = Vector3.zero;

        // Reset velocity
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Reset rotation to identity (no rotation)
        rb.rotation = Quaternion.identity;
        thruster.Stop();
    }
    void ApplyForces(int[] vents)
    {
        foreach (int ventIndex in vents)
        {
            int i = ventIndex - 1; // Convert from 1-based to 0-based indexing
            Vector3 worldForce = transform.TransformDirection(F[i]);  // Convert force from local to world
            Vector3 worldPosition = transform.TransformPoint(r[i]);   // Convert position from local to world
            rb.AddForceAtPosition(worldForce, worldPosition, ForceMode.Force);
            ActivateThrusterEffect(worldForce, worldPosition);
        }
    }

    void ActivateThrusterEffect(Vector3 force, Vector3 position)
    {
        // Create thruster effect at vent position
        GameObject thruster = Instantiate(thrusterEffectPrefab, position, Quaternion.identity);
        thruster.transform.rotation = Quaternion.LookRotation(-force);

        // Attach thruster to the object so it moves with it
        thruster.transform.parent = transform;

        // Destroy effect after 0.5 seconds
        Destroy(thruster, 0.5f);
    }

    public void ActivateThruster(KeyCode key, float duration)
    {
        StartCoroutine(ActivateThrusterCoroutine(key, duration));
    }

    private IEnumerator ActivateThrusterCoroutine(KeyCode key, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            if (keyToControl.ContainsKey(key))
            {
                ApplyForces(keyToControl[key]);
            }
            elapsedTime += Time.deltaTime;
            yield return null;
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
