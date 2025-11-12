using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Unity.XR.CoreUtils; // XROrigin

public class VRSpawnAlign : MonoBehaviour
{
    public XROrigin xrOrigin;
    public Transform spawnPoint;

    [Header("Tracking origin")]
    public bool useFloorTracking = true;           // Floor = room-scale, Device = seated
    [Range(0.8f, 1.8f)] public float deviceEyeHeight = 1.6f;

    bool alignedOnce;

    void Awake()
    {
        if (!xrOrigin) xrOrigin = FindObjectOfType<XROrigin>();
    }

    void OnEnable()
    {
        // Re-align if tracking origin changes after we start
        var subs = new List<XRInputSubsystem>();
        SubsystemManager.GetSubsystems(subs);
        foreach (var s in subs)
        {
            s.trackingOriginUpdated += OnTrackingOriginUpdated;
            s.boundaryChanged += OnBoundaryChanged; // optional extra hook
        }
    }

    void OnDisable()
    {
        var subs = new List<XRInputSubsystem>();
        SubsystemManager.GetSubsystems(subs);
        foreach (var s in subs)
        {
            s.trackingOriginUpdated -= OnTrackingOriginUpdated;
            s.boundaryChanged -= OnBoundaryChanged;
        }
    }

    void Start()
    {
        if (!spawnPoint || !xrOrigin) return;
        StartCoroutine(InitAndAlignWhenReady());
    }

    IEnumerator InitAndAlignWhenReady()
    {
        // 1) Set tracking origin on all XR subsystems
        var subs = new List<XRInputSubsystem>();
        SubsystemManager.GetSubsystems(subs);
        foreach (var s in subs)
        {
            var mode = useFloorTracking ? TrackingOriginModeFlags.Floor : TrackingOriginModeFlags.Device;
            s.TrySetTrackingOriginMode(mode);
            s.TryRecenter(); // harmless if unsupported
        }

        // 2) Configure camera offset for Device (ignored for Floor)
        xrOrigin.CameraYOffset = useFloorTracking ? 0f : deviceEyeHeight;

        // 3) Wait for HMD to be valid and tracked
        yield return StartCoroutine(WaitForHMDTracking());

        // 4) Give runtime an extra frame, then align twice for safety
        yield return null;
        AlignToSpawn();
        alignedOnce = true;

        yield return null;
        AlignToSpawn();
    }

    IEnumerator WaitForHMDTracking()
    {
        float timeout = Time.time + 5f; // avoid infinite wait in Editor/Play
        InputDevice hmd = InputDevices.GetDeviceAtXRNode(XRNode.CenterEye);

        while (Time.time < timeout)
        {
            hmd = InputDevices.GetDeviceAtXRNode(XRNode.CenterEye);
            if (hmd.isValid &&
                hmd.TryGetFeatureValue(CommonUsages.isTracked, out bool tracked) &&
                tracked)
                break;

            yield return null;
        }
    }

    void OnTrackingOriginUpdated(XRInputSubsystem s)
    {
        if (alignedOnce) AlignToSpawn();
    }

    void OnBoundaryChanged(XRInputSubsystem s)
    {
        // Guardian/room change can nudge origin; re-align
        if (alignedOnce) AlignToSpawn();
    }

    public void AlignToSpawn()
    {
        if (!xrOrigin || !spawnPoint || !xrOrigin.Camera) return;

        var cam = xrOrigin.Camera.transform;

        // Yaw match
        Vector3 camFwd = cam.forward; camFwd.y = 0f; camFwd.Normalize();
        Vector3 tgtFwd = spawnPoint.forward; tgtFwd.y = 0f; tgtFwd.Normalize();
        if (camFwd.sqrMagnitude > 1e-6f && tgtFwd.sqrMagnitude > 1e-6f)
        {
            Quaternion yawDelta = Quaternion.FromToRotation(camFwd, tgtFwd);
            xrOrigin.transform.rotation = yawDelta * xrOrigin.transform.rotation;
        }

        // Position match (move rig so camera equals spawn)
        Vector3 toTarget = spawnPoint.position - cam.position;
        xrOrigin.transform.position += toTarget;
    }
}
