using UnityEngine;

/// <summary>
/// Meta SDK HandGrabInteractor와 호환되는 간단한 Grab 핸들러
/// 손으로 잡으면 원자 생성
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class SimpleGrabHandler : MonoBehaviour
{
    private AtomPullDispenser dispenser;
    private float lastGrabTime;
    private const float GRAB_COOLDOWN = 0.5f;

    // Grab 상태 추적
    private bool isBeingGrabbed = false;

    private void Awake()
    {
        dispenser = GetComponent<AtomPullDispenser>();
    }

    private void Start()
    {
        if (dispenser != null)
        {
            Debug.Log($"[SimpleGrabHandler] {gameObject.name} 준비됨");
        }
    }

    // Meta SDK의 HandGrabInteractor가 이 오브젝트를 잡을 때 호출됨
    // Grabbable 컴포넌트의 WhenSelect 이벤트 또는 직접 연결
    public void OnGrabbed()
    {
        if (Time.time - lastGrabTime < GRAB_COOLDOWN) return;

        lastGrabTime = Time.time;
        isBeingGrabbed = true;

        Debug.Log($"[SimpleGrabHandler] {gameObject.name} Grab 감지!");

        if (dispenser != null)
        {
            dispenser.OnMetaSelect();
        }
    }

    public void OnReleased()
    {
        isBeingGrabbed = false;
        Debug.Log($"[SimpleGrabHandler] {gameObject.name} Release");
    }

    // Trigger 방식 백업 (HandGrabInteractor의 Collider가 닿을 때)
    private void OnTriggerEnter(Collider other)
    {
        if (Time.time - lastGrabTime < GRAB_COOLDOWN) return;

        // HandGrabInteractor 또는 Hand 관련 오브젝트 확인
        if (IsGrabSource(other))
        {
            lastGrabTime = Time.time;
            Debug.Log($"[SimpleGrabHandler] {gameObject.name} Trigger Grab by {other.name}");

            if (dispenser != null)
            {
                dispenser.OnMetaSelect();
            }
        }
    }

    private bool IsGrabSource(Collider other)
    {
        string name = other.name.ToLower();
        string parentName = other.transform.parent != null ? other.transform.parent.name.ToLower() : "";

        // HandGrabInteractor 관련 오브젝트 확인
        return name.Contains("grab") ||
               name.Contains("hand") ||
               name.Contains("palm") ||
               name.Contains("pinch") ||
               parentName.Contains("grab") ||
               parentName.Contains("handgrab");
    }
}
