using UnityEngine;

public class Camera : MonoBehaviour
{
    public Transform target; // Object to focus on
    public Vector3 fixedPosition = new Vector3(0, 10, -10); // Default position
    public Quaternion fixedRotation = Quaternion.Euler(30, 0, 0); // Default rotation

    public Vector3 focusOffset = new Vector3(0, 2, -5); // Offset when focusing
    public float rotationSpeed = 5f; // Mouse rotation speed
    public float zoomSpeed = 5f; // Zoom speed
    public float minZoom = 2f, maxZoom = 10f; // Min & max zoom distance
    public float transitionSpeed = 5f; // Smooth transition speed

    private float currentZoom;
    private float yaw = 0f, pitch = 10f; // Rotation angles
    private bool isFocusing = false; // Focus mode

    private Vector3 velocity = Vector3.zero; // For SmoothDamp

    void Start()
    {
        currentZoom = focusOffset.magnitude;
        transform.position = fixedPosition;
        transform.rotation = fixedRotation;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left Click to focus
        {
            isFocusing = true;
        }
        else if (Input.GetMouseButtonUp(0)) // Release Click to return to fixed position
        {
            isFocusing = false;
        }

        if (isFocusing)
        {
            // Rotate camera with mouse
            yaw += Input.GetAxis("Mouse X") * rotationSpeed;
            pitch -= Input.GetAxis("Mouse Y") * rotationSpeed;
            pitch = Mathf.Clamp(pitch, -20f, 80f); // Limit rotation

            // Zoom with scroll wheel
            currentZoom -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
        }
    }

    void FixedUpdate() // 🔥 Use FixedUpdate for fast-moving objects
    {
        if (isFocusing && target != null)
        {
            // Calculate new position
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
            Vector3 desiredPosition = target.position - (rotation * Vector3.forward * currentZoom) + Vector3.up * focusOffset.y;

            // 🚀 Use SmoothDamp to prevent shaking
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, 0.1f);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * transitionSpeed);
            transform.LookAt(target.position);
        }
        else
        {
            // Move to fixed position
            transform.position = Vector3.SmoothDamp(transform.position, fixedPosition, ref velocity, 0.2f);
            transform.rotation = Quaternion.Slerp(transform.rotation, fixedRotation, Time.deltaTime * transitionSpeed);
        }
    }
}
