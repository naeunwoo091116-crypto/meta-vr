using UnityEngine;
using TMPro;

/// <summary>
/// 주기율표 Element 버튼 - Meta SDK Grabbable과 함께 사용
/// 손으로 잡으면(WhenSelect) 원자가 생성됨
/// </summary>
public class AtomPullDispenser : MonoBehaviour
{
    [Header("설정: 뽑아낼 원소")]
    public string elementSymbol = "H";
    public Color atomColor = Color.white;
    public GameObject atomPrefab;

    private float lastSpawnTime;
    private const float SPAWN_COOLDOWN = 0.5f;

    private void Start()
    {
        Debug.Log($"AtomPullDispenser 시작: {elementSymbol}, Prefab: {(atomPrefab != null ? atomPrefab.name : "없음")}");
    }

    /// <summary>
    /// Meta SDK InteractableUnityEventWrapper의 WhenSelect 이벤트에 연결
    /// 손으로 잡으면 호출됨
    /// </summary>
    public void OnMetaSelect()
    {
        if (Time.time - lastSpawnTime < SPAWN_COOLDOWN) return;
        lastSpawnTime = Time.time;

        Debug.Log($"OnMetaSelect 호출: {elementSymbol}");
        Vector3 spawnPos = transform.position + transform.forward * 0.15f;
        SpawnAtomAtPosition(spawnPos);
    }

    /// <summary>
    /// 지정된 위치에 원자 생성
    /// </summary>
    public void SpawnAtomAtPosition(Vector3 position)
    {
        if (atomPrefab == null)
        {
            Debug.LogError($"AtomPullDispenser: atomPrefab이 없습니다! ({elementSymbol})");
            return;
        }

        GameObject newAtom = Instantiate(atomPrefab, position, Quaternion.identity);

        // 색상 설정
        var meshRenderer = newAtom.GetComponentInChildren<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.material.color = atomColor;
        }

        // 원소 기호 설정
        AtomConnector connector = newAtom.GetComponent<AtomConnector>();
        if (connector != null)
        {
            connector.elementSymbol = elementSymbol;
        }

        // 텍스트 표시
        var textDisplay = newAtom.GetComponentInChildren<TextMeshPro>();
        if (textDisplay != null)
        {
            textDisplay.text = elementSymbol;
        }

        Debug.Log($"원자 생성: {elementSymbol}");
    }

    /// <summary>
    /// 기본 위치에 원자 생성
    /// </summary>
    public void SpawnAtom()
    {
        Vector3 spawnPos = transform.position + transform.forward * 0.15f;
        SpawnAtomAtPosition(spawnPos);
    }
}
