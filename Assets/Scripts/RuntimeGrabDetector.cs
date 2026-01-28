using UnityEngine;
using System;
using System.Reflection;

/// <summary>
/// 런타임에서 Meta SDK HandGrabInteractor의 Grab 이벤트를 감지
/// 씬 초기화 문제를 피하기 위해 에디터 설정 없이 런타임에서만 작동
/// </summary>
public class RuntimeGrabDetector : MonoBehaviour
{
    private AtomPullDispenser dispenser;
    private object grabbable;
    private Type grabbableType;
    private bool isSetup = false;
    private float lastGrabTime;
    private const float GRAB_COOLDOWN = 0.5f;

    // Grab 상태 추적
    private bool wasSelected = false;

    private void Start()
    {
        dispenser = GetComponent<AtomPullDispenser>();

        // Grabbable 타입 찾기
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            grabbableType = assembly.GetType("Oculus.Interaction.Grabbable");
            if (grabbableType != null) break;
        }

        if (grabbableType != null)
        {
            grabbable = GetComponent(grabbableType);
            if (grabbable != null)
            {
                isSetup = true;
                Debug.Log($"[RuntimeGrabDetector] {gameObject.name} - Grabbable 감지됨");
            }
        }

        if (!isSetup)
        {
            Debug.LogWarning($"[RuntimeGrabDetector] {gameObject.name} - Grabbable 없음");
        }
    }

    private void Update()
    {
        if (!isSetup || grabbable == null || dispenser == null) return;

        try
        {
            // IPointable.State 프로퍼티를 통해 선택 상태 확인
            // Grabbable은 IPointable을 구현함
            var stateProperty = grabbableType.GetProperty("State");
            if (stateProperty != null)
            {
                var state = stateProperty.GetValue(grabbable);
                // InteractableState enum: Normal=0, Hover=1, Select=2
                int stateValue = Convert.ToInt32(state);
                bool isSelected = stateValue == 2; // Select 상태

                // Select 상태로 전환되면 원자 생성
                if (isSelected && !wasSelected)
                {
                    if (Time.time - lastGrabTime >= GRAB_COOLDOWN)
                    {
                        lastGrabTime = Time.time;
                        Debug.Log($"[RuntimeGrabDetector] {gameObject.name} GRAB 감지!");
                        dispenser.OnMetaSelect();
                    }
                }
                wasSelected = isSelected;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[RuntimeGrabDetector] 에러: {e.Message}");
            isSetup = false;
        }
    }
}
