using UnityEngine;

public class WaiterWalking : MonoBehaviour
{
    public Transform[] targets;          // Same as before
    public float stopDistance = 0.25f;   // Same name (Inspector: Stop Distance)
    public float turnSpeed = 8f;         // Same name (Inspector: Turn Speed)

    private Animator anim;
    private bool walking;
    private int currentTargetIndex = 0;
    private int direction = 1; // 1 = forward, -1 = backward

    void Awake()
    {
        anim = GetComponent<Animator>();
        if (anim != null)
        {
            anim.applyRootMotion = true;
            anim.SetBool("IsWalking", false);
        }
    }

    void Update()
    {
        if (!walking || targets == null || targets.Length == 0) return;

        Transform target = targets[currentTargetIndex];
        if (target == null) { StopWalking(); return; }

        Vector3 to = target.position - transform.position;
        Vector3 flat = new Vector3(to.x, 0f, to.z);
        float dist = flat.magnitude;

        // rotate toward target
        if (flat.sqrMagnitude > 0.0001f)
        {
            Quaternion look = Quaternion.LookRotation(flat.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * turnSpeed);
        }

        // arrived?
        if (dist <= Mathf.Max(0.001f, stopDistance))
        {
            currentTargetIndex += direction;

            // end of path?
            if (currentTargetIndex >= targets.Length || currentTargetIndex < 0)
            {
                StopWalking();
            }
        }
    }

    void OnAnimatorMove()
    {
        if (anim == null || !walking) return;

        float moveSpeed = anim.deltaPosition.magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
        Vector3 forward = transform.forward * moveSpeed * Time.deltaTime;
        forward.y = 0f;
        transform.position += forward;

        transform.rotation *= anim.deltaRotation;
    }

    /// <summary>
    /// Start walking along the targets. reverse=true walks backwards.
    /// Starts from the NEAREST waypoint to the current position so it never gets stuck.
    /// </summary>
    public void StartWalking(bool reverse = false)
    {
        if (targets == null || targets.Length == 0) return;

        direction = reverse ? -1 : 1;

        // Pick nearest waypoint to start from (works even if array order differs between scenes)
        int nearest = 0;
        float best = float.PositiveInfinity;
        Vector3 pos = transform.position;
        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] == null) continue;
            float d = (targets[i].position - pos).sqrMagnitude;
            if (d < best) { best = d; nearest = i; }
        }

        currentTargetIndex = nearest;

        // Edge cases: if we're already at the end in the intended direction, just stop.
        if (!reverse && currentTargetIndex >= targets.Length - 1) { StopWalking(); return; }
        if (reverse && currentTargetIndex <= 0) { StopWalking(); return; }

        walking = true;
        if (anim != null) anim.SetBool("IsWalking", true);

        Debug.Log($"[WaiterWalking] StartWalking(reverse={reverse}) from waypoint {currentTargetIndex} of {targets.Length}");
    }

    public bool IsWalking() => walking;

    private void StopWalking()
    {
        walking = false;
        if (anim != null) anim.SetBool("IsWalking", false);
        Debug.Log("[WaiterWalking] Stopped.");
    }
}
