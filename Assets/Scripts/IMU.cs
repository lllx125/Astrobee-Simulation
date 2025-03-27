using UnityEngine;

public class IMU : MonoBehaviour
{
    // Reference to the Rigidbody to compute linear acceleration
    private Rigidbody rb;

    // Store the previous frame's velocity for acceleration calculation
    private Vector3 lastVelocity;

    // Store the previous frame's angular velocity for angular acceleration calculation
    private Vector3 lastAngularVelocity;

    // Variable to hold the computed linear acceleration value
    public Vector3 acceleration;

    // Variable to hold the computed angular acceleration value
    public Vector3 angularAcceleration;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get the Rigidbody component attached to this gameobject.
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Initialize lastVelocity and lastAngularVelocity with the current values of the Rigidbody.
            lastVelocity = rb.linearVelocity;
            lastAngularVelocity = rb.angularVelocity;
        }

    }

    //Unity’s physics engine updates on its own fixed timestep
    void FixedUpdate()
    {
        // Calculate linear acceleration as change in velocity over fixed time step.
        acceleration = (rb.linearVelocity - lastVelocity) / Time.fixedDeltaTime;

        // Calculate angular acceleration similarly.
        angularAcceleration = (rb.angularVelocity - lastAngularVelocity) / Time.fixedDeltaTime;

        // Update for next frame
        lastVelocity = rb.linearVelocity;
        lastAngularVelocity = rb.angularVelocity;
    }
    /// <summary>
    /// Returns the IMU sensor readings as an array of floats.
    /// The first three elements (indices 0-2) represent the computed linear acceleration (X, Y, Z).
    /// The next three elements (indices 3-5) represent the computed angular acceleration (X, Y, Z).
    /// 
    /// Notes:
    /// - Linear acceleration is calculated from the change in Rigidbody's velocity.
    /// - Angular acceleration is calculated from the change in Rigidbody's angular velocity.
    /// - Ensure the GameObject has a Rigidbody component to get valid data.
    /// </summary>
    public float[] GetIMUReadings()
    {
        // Construct and return an array with both linear and angular acceleration data.
        return new float[] {
            acceleration.x, acceleration.y, acceleration.z,
            angularAcceleration.x, angularAcceleration.y, angularAcceleration.z
        };
    }
}
