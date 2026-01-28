using UnityEngine;

/// <summary>
/// 손이 Element에 닿고 일정 시간 유지하면 Grab으로 판정
/// OVRInput 없이 작동 (시뮬레이터 호환)
/// </summary>
[RequireComponent(typeof(Collider))]
public class HandGrabDetector : MonoBehaviour
{
    private AtomPullDispenser dispenser;
    private float lastGrabTime;
    private const float GRAB_COOLDOWN = 0.5f;
    private const float GRAB_HOLD_TIME = 0.3f; // 0.3초 유지하면 Grab

    private float touchStartTime;
    private bool isHandTouching;
    private bool hasTriggeredGrab;

    private void Awake()
    {
        dispenser = GetComponent<AtomPullDispenser>();

        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void Start()
    {
        Debug.Log($"[HandGrabDetector] {gameObject.name} 준비됨");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsHandCollider(other) && !isHandTouching)
        {
            isHandTouching = true;
            touchStartTime = Time.time;
            hasTriggeredGrab = false;
            Debug.Log($"[HandGrabDetector] {gameObject.name} 손 접촉 시작: {other.name}");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isHandTouching || hasTriggeredGrab) return;
        if (!IsHandCollider(other)) return;

        // 일정 시간 유지하면 Grab
        if (Time.time - touchStartTime >= GRAB_HOLD_TIME)
        {
            if (Time.time - lastGrabTime >= GRAB_COOLDOWN)
            {
                lastGrabTime = Time.time;
                hasTriggeredGrab = true;

                Debug.Log($"[HandGrabDetector] {gameObject.name} GRAB 감지!");

                if (dispenser != null)
                {
                    dispenser.OnMetaSelect();
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsHandCollider(other))
        {
            isHandTouching = false;
            hasTriggeredGrab = false;
        }
    }

    private bool IsHandCollider(Collider other)
    {
        string name = other.name.ToLower();
        string parentName = other.transform.parent != null ?
                           other.transform.parent.name.ToLower() : "";

        return name.Contains("hand") ||
               name.Contains("grab") ||
               name.Contains("palm") ||
               name.Contains("finger") ||
               name.Contains("poke") ||
               name.Contains("pinch") ||
               parentName.Contains("hand") ||
               parentName.Contains("grab");
    }
}
