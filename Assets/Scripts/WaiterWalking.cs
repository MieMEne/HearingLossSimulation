using UnityEngine;

public class WaiterWalking : MonoBehaviour
{
    public Transform[] targets;       
    public float stopDistance = 0.25f;
    public float turnSpeed = 8f;

    private Animator anim;
    private bool walking;
    private int currentTargetIndex = 0;
    private int direction = 1; // 1 = forward, -1 = backward

    void Awake()
    {
        anim = GetComponent<Animator>();
        anim.applyRootMotion = true;
        anim.SetBool("IsWalking", false);
    }

    void Update()
    {
        if (!walking || targets.Length == 0) return;

        Transform target = targets[currentTargetIndex];

        Vector3 to = target.position - transform.position;
        Vector3 flat = new Vector3(to.x, 0f, to.z);
        float dist = flat.magnitude;

        if (dist > stopDistance)
        {
            if (flat.sqrMagnitude > 0.0001f)
            {
                Quaternion look = Quaternion.LookRotation(flat.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * turnSpeed);
            }
        }
        else
        {
            currentTargetIndex += direction;

            if (currentTargetIndex >= targets.Length || currentTargetIndex < 0)
            {
                walking = false;
                anim.SetBool("IsWalking", false);
            }
        }
    }

    void OnAnimatorMove()
    {
        if (anim == null || !walking) return;

        float moveSpeed = anim.deltaPosition.magnitude / Time.deltaTime;
        Vector3 forward = transform.forward * moveSpeed * Time.deltaTime;
        forward.y = 0f;
        transform.position += forward;
        transform.rotation *= anim.deltaRotation;
    }

    /// <summary>
    /// Start walking along the targets. Reverse = true will walk backward.
    /// </summary>
    public void StartWalking(bool reverse = false)
    {
        if (targets.Length == 0) return;

        walking = true;
        anim.SetBool("IsWalking", true);

        direction = reverse ? -1 : 1;
        currentTargetIndex = reverse ? targets.Length - 1 : 0;
    }

    public bool IsWalking()
    {
        return walking;
    }
}

//Reference
// This script was created with inspiration from Copolit and Chatgpt