using UnityEngine;

public class WaiterWalking : MonoBehaviour
{
    public Transform[] targets;       // Array of points to walk to
    public float stopDistance = 0.25f;
    public float turnSpeed = 8f;
    public float startDelay = 0f;

    Animator anim;
    bool walking;
    int currentTargetIndex = 0;

    void Awake()
    {
        anim = GetComponent<Animator>();
        anim.applyRootMotion = true;
        anim.SetBool("IsWalking", false);
    }

    void Start()
    {
        if (startDelay <= 0f) BeginWalk();
        else Invoke(nameof(BeginWalk), startDelay);
    }

    void BeginWalk()
    {
        if (targets.Length == 0) return;
        walking = true;
        anim.SetBool("IsWalking", true);
    }

    void Update()
    {
        if (!walking || targets.Length == 0) return;

        Transform target = targets[currentTargetIndex];

        // Drej mod mål i horisontal plan
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
            // Stop ved mål → gå til næste punkt
            currentTargetIndex++;

            if (currentTargetIndex >= targets.Length)
            {
                // Ingen flere punkter → stå stille
                walking = false;
                anim.SetBool("IsWalking", false);
            }
        }
    }

    void OnAnimatorMove()
    {
        if (anim == null || !walking) return;

        // Calculate forward movement manually
        float moveSpeed = anim.deltaPosition.magnitude / Time.deltaTime; // use animation’s speed
        Vector3 forward = transform.forward * moveSpeed * Time.deltaTime;

        // Move forward only in XZ plane
        forward.y = 0f;
        transform.position += forward;

        // Keep rotation from animation
        transform.rotation *= anim.deltaRotation;
    }
    public void StartWalking()
    {
        if (targets.Length == 0) return;
        walking = true;
        anim.SetBool("IsWalking", true);
    }
}
