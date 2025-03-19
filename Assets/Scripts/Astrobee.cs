using UnityEngine;
using System.Collections.Generic;

public class Astrobee : MonoBehaviour
{
    // geometric parameters (m)
    public float a = 0.1f;
    public float b = 0.05f;
    public float c = 0.15f;
    public float d = 0.08f;
    public float F0 = 1; // Force magnitude (N)

    private Vector3[] r; // Positions of force application
    private Vector3[] F; // Force vectors

    private Rigidbody rb;

    public GameObject thrusterEffectPrefab;

    // Dictionary mapping key inputs to vent activations
    private Dictionary<KeyCode, string> keyToControl = new Dictionary<KeyCode, string>
    {
        { KeyCode.A, "+x" },  { KeyCode.D, "-x" },
        { KeyCode.W, "-y" },  { KeyCode.S, "+y" },
        { KeyCode.Q, "+z" },  { KeyCode.E, "-z" },
        { KeyCode.I, "+Rx1" }, { KeyCode.K, "-Rx1" },
        { KeyCode.J, "+Ry1" }, { KeyCode.L, "-Ry1" },
        { KeyCode.U, "+Rz1" }, { KeyCode.O, "-Rz1" }
    };

    // Mapping control commands to active vent indices
    private Dictionary<string, int[]> CONTROL = new Dictionary<string, int[]>
    {
        { "+x", new int[]{2, 9} },   { "-x", new int[]{4, 7} },
        { "+y", new int[]{11, 12} }, { "-y", new int[]{5, 6} },
        { "+z", new int[]{3, 10} },  { "-z", new int[]{1, 8} },
        { "+Rx1", new int[]{3, 8} }, { "+Rx2", new int[]{5, 12} },
        { "-Rx1", new int[]{1, 10} }, { "-Rx2", new int[]{6, 11} },
        { "+Ry1", new int[]{2, 4} }, { "+Ry2", new int[]{8, 10} },
        { "-Ry1", new int[]{1, 3} }, { "-Ry2", new int[]{7, 9} },
        { "+Rz1", new int[]{4, 9} }, { "+Rz2", new int[]{6, 12} },
        { "-Rz1", new int[]{2, 7} }, { "-Rz2", new int[]{5, 11} }
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
            new Vector3(-d, -a, c), // 10
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

    }

    void Update()
    {
        foreach (var entry in keyToControl)
        {
            if (Input.GetKey(entry.Key))
            {
                ApplyForces(entry.Value);
            }
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResetObject();
        }
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
    }
    void ApplyForces(string controlKey)
    {
        if (!CONTROL.ContainsKey(controlKey)) return;

        foreach (int ventIndex in CONTROL[controlKey])
        {
            int i = ventIndex - 1; // Convert from 1-based to 0-based indexing
            rb.AddForceAtPosition(F[i], transform.position + r[i], ForceMode.Force);
            ActivateThrusterEffect(i);
        }
    }

    void ActivateThrusterEffect(int ventIndex)
    {
        // Create thruster effect at vent position
        GameObject thruster = Instantiate(thrusterEffectPrefab, transform.position + r[ventIndex], Quaternion.identity);
        thruster.transform.rotation = Quaternion.LookRotation(-F[ventIndex].normalized);

        // Attach thruster to the object so it moves with it
        thruster.transform.parent = transform;

        // Destroy effect after 0.5 seconds
        Destroy(thruster, 0.5f);
    }
}
