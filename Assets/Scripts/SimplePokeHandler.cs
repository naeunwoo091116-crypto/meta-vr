using UnityEngine;

/// <summary>
/// Meta SDK HandPokeInteractor와 호환되는 간단한 Poke 핸들러
/// Collider Trigger를 사용하여 손가락이 닿으면 원자 생성
/// </summary>
[RequireComponent(typeof(Collider))]
public class SimplePokeHandler : MonoBehaviour
{
    private AtomPullDispenser dispenser;
    private float lastPokeTime;
    private const float POKE_COOLDOWN = 0.5f;

    private void Awake()
    {
        dispenser = GetComponent<AtomPullDispenser>();

        // Collider를 Trigger로 설정
        var collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
    }

    private void Start()
    {
        if (dispenser != null)
        {
            Debug.Log($"[SimplePokeHandler] {gameObject.name} 준비됨");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 쿨다운 체크
        if (Time.time - lastPokeTime < POKE_COOLDOWN) return;

        // 손가락/손/Poke Interactor인지 확인
        if (IsPokeSource(other))
        {
            lastPokeTime = Time.time;
            Debug.Log($"[SimplePokeHandler] {gameObject.name} Poke 감지! by {other.name}");

            if (dispenser != null)
            {
                dispenser.OnMetaSelect();
            }
        }
    }

    private bool IsPokeSource(Collider other)
    {
        string name = other.name.ToLower();
        string parentName = other.transform.parent != null ? other.transform.parent.name.ToLower() : "";

        // Meta SDK의 HandPokeInteractor 또는 손가락 관련 오브젝트 확인
        return name.Contains("poke") ||
               name.Contains("finger") ||
               name.Contains("index") ||
               name.Contains("hand") ||
               name.Contains("tip") ||
               parentName.Contains("poke") ||
               parentName.Contains("hand");
    }
}
