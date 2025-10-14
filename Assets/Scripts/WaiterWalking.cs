using UnityEngine;

public class WaiterWalking : MonoBehaviour
{
    public Transform target;
    public float stopDistance = 0.25f;
    public float turnSpeed = 8f;          // hvor hurtigt vi drejer mod målet
    public float startDelay = 0f;         // fx 2f hvis han skal stå først

    Animator anim;
    bool walking;

    void Awake()
    {
        anim = GetComponent<Animator>();
        anim.applyRootMotion = true;          // vigtigt!
        anim.SetBool("IsWalking", false);     // start i idle
    }

    void Start()
    {
        if (startDelay <= 0f) BeginWalk();
        else Invoke(nameof(BeginWalk), startDelay);
    }

    void BeginWalk()
    {
        if (target == null) return;
        walking = true;
        anim.SetBool("IsWalking", true);
    }

    void Update()
    {
        if (!walking || target == null) return;

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
            // Stop ved mål → tilbage til idle
            walking = false;
            anim.SetBool("IsWalking", false);
        }
    }

    // Her sker selve root-motion flytningen
    void OnAnimatorMove()
    {
        if (anim == null) return;

        // Kun lad animationen flytte når vi faktisk går
        if (walking)
        {
            // Brug delta fra anim til position/rotation
            transform.position += anim.deltaPosition;   // fremdrift fra walk-anim
            transform.rotation *= anim.deltaRotation;   // hvis din anim også roterer
        }
        // Når walking = false, gør vi intet → står stille i Idle
    }
}
