using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Orbit + Zoom camera around a target, without jumps or drift.
/// Works with PlayerInput (Invoke Unity Events).
/// </summary>
[RequireComponent(typeof(PlayerInput))]
public class StableOrbitZoomCamera : MonoBehaviour
{
    [Header("🎯 Target")]
    public Transform target;

    [Header("⚙️ Settings")]
    public float orbitSpeed = 60f;
    public float zoomSpeed = 10f;
    public float minDistance = 0.5f;
    public float maxDistance = 200f;
    public bool lockY = true; // optional: keep Y constant

    private float currentDistance;
    private Vector3 direction; // current direction from target → camera
    private Vector2 orbitInput;
    private float zoomInput;

    void Start()
    {
        if (!target)
        {
            target = new GameObject("TempTarget").transform;
            target.position = Vector3.zero;
        }

        // Take scene placement as truth
        direction = (transform.position - target.position).normalized;
        currentDistance = Vector3.Distance(transform.position, target.position);

        // Normalize direction if it's zero-length
        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector3.forward;

        transform.LookAt(target.position);
    }

    void Update()
    {
        if (orbitInput.sqrMagnitude > 0.001f)
            ApplyOrbit(orbitInput);

        if (Mathf.Abs(zoomInput) > 0.001f)
            ApplyZoom(zoomInput);
    }

    // ── Input Events ────────────────────────────────
    public void OnOrbit(InputAction.CallbackContext ctx)
        => orbitInput = ctx.performed ? ctx.ReadValue<Vector2>() : Vector2.zero;

    public void OnZoom(InputAction.CallbackContext ctx)
        => zoomInput = ctx.performed ? ctx.ReadValue<float>() : 0f;

    // ── Core Logic ─────────────────────────────────
    private void ApplyOrbit(Vector2 delta)
    {
        // Convert to local spherical coordinates relative to target
        Vector3 pivot = target.position;
        Vector3 camPos = transform.position;

        Vector3 offset = camPos - pivot;
        float radius = offset.magnitude;

        // Get current spherical angles
        float yaw = Mathf.Atan2(offset.x, offset.z);
        float pitch = Mathf.Asin(offset.y / radius);

        // Apply input deltas
        yaw += delta.x * orbitSpeed * Mathf.Deg2Rad * Time.deltaTime;
        pitch -= delta.y * orbitSpeed * Mathf.Deg2Rad * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, -Mathf.PI / 2f + 0.01f, Mathf.PI / 2f - 0.01f);

        // Recalculate offset
        offset.x = radius * Mathf.Sin(yaw) * Mathf.Cos(pitch);
        offset.z = radius * Mathf.Cos(yaw) * Mathf.Cos(pitch);
        offset.y = radius * Mathf.Sin(pitch);

        // Move camera
        transform.position = pivot + offset;
        transform.LookAt(pivot);

        // Update shared state
        direction = (transform.position - pivot).normalized;
        currentDistance = radius;
    }

    private void ApplyZoom(float input)
    {
        currentDistance -= input * zoomSpeed * Time.deltaTime;
        currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);

        Vector3 pivot = target.position;

        // Move strictly along current direction (no Y drift)
        Vector3 moveDir = direction;
        if (lockY) moveDir.y = 0f;
        moveDir.Normalize();

        transform.position = pivot + moveDir * currentDistance;
        transform.LookAt(pivot);
    }
}
