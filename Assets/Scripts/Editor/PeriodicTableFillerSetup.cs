#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEditor;
using TMPro;
using System.Reflection;

public class PeriodicTableFillerSetup
{
    private static readonly string[] symbols = {
        "H", "He",
        "Li", "Be", "B", "C", "N", "O", "F", "Ne",
        "Na", "Mg", "Al", "Si", "P", "S", "Cl", "Ar",
        "K", "Ca", "Sc", "Ti", "V", "Cr", "Mn", "Fe", "Co", "Ni", "Cu", "Zn", "Ga", "Ge", "As", "Se", "Br", "Kr"
    };

    // Atom.prefab에서 사용하는 GUID들
    private const string GRABBABLE_GUID = "43f86b14a27b52f4f9298c33015b5c26";  // Grabbable
    private const string HAND_GRAB_INTERACTABLE_GUID = "e9a7676b01585ce43908639a27765dfc";  // HandGrabInteractable

    private static readonly Color[] colors = {
        new Color(0.4f, 0.6f, 1f), new Color(0.9f, 0.5f, 0.9f),
        new Color(1f, 0.4f, 0.4f), new Color(1f, 0.6f, 0.3f), new Color(0.4f, 0.8f, 0.8f),
        new Color(0.4f, 0.6f, 1f), new Color(0.4f, 0.6f, 1f), new Color(0.4f, 0.6f, 1f),
        new Color(0.8f, 0.4f, 1f), new Color(0.9f, 0.5f, 0.9f),
        new Color(1f, 0.4f, 0.4f), new Color(1f, 0.6f, 0.3f), new Color(0.6f, 0.8f, 0.6f),
        new Color(0.4f, 0.8f, 0.8f), new Color(0.4f, 0.6f, 1f), new Color(0.4f, 0.6f, 1f),
        new Color(0.8f, 0.4f, 1f), new Color(0.9f, 0.5f, 0.9f),
        new Color(1f, 0.4f, 0.4f), new Color(1f, 0.6f, 0.3f),
        new Color(1f, 0.8f, 0.4f), new Color(1f, 0.8f, 0.4f), new Color(1f, 0.8f, 0.4f),
        new Color(1f, 0.8f, 0.4f), new Color(1f, 0.8f, 0.4f), new Color(1f, 0.8f, 0.4f),
        new Color(1f, 0.8f, 0.4f), new Color(1f, 0.8f, 0.4f), new Color(1f, 0.8f, 0.4f),
        new Color(1f, 0.8f, 0.4f), new Color(0.6f, 0.8f, 0.6f), new Color(0.4f, 0.8f, 0.8f),
        new Color(0.4f, 0.8f, 0.8f), new Color(0.4f, 0.6f, 1f), new Color(0.8f, 0.4f, 1f),
        new Color(0.9f, 0.5f, 0.9f),
    };

    [MenuItem("Atoms/Remove InteractableUnityEventWrapper (오류 수정)")]
    public static void RemoveEventWrappers()
    {
        // 모든 Element_ 오브젝트에서 InteractableUnityEventWrapper 제거
        System.Type wrapperType = null;
        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            wrapperType = assembly.GetType("Oculus.Interaction.InteractableUnityEventWrapper");
            if (wrapperType != null) break;
        }

        if (wrapperType == null)
        {
            EditorUtility.DisplayDialog("정보", "InteractableUnityEventWrapper 타입을 찾을 수 없습니다.", "확인");
            return;
        }

        int removedCount = 0;
        var allObjects = Object.FindObjectsOfType<GameObject>(true);
        foreach (var obj in allObjects)
        {
            if (obj.name.StartsWith("Element_"))
            {
                var wrapper = obj.GetComponent(wrapperType);
                if (wrapper != null)
                {
                    Object.DestroyImmediate(wrapper);
                    EditorUtility.SetDirty(obj);
                    removedCount++;
                    Debug.Log($"{obj.name}: InteractableUnityEventWrapper 제거됨");
                }
            }
        }

        if (removedCount > 0)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            EditorUtility.DisplayDialog("완료", $"{removedCount}개 오브젝트에서 InteractableUnityEventWrapper를 제거했습니다.\n\n씬을 저장하고 다시 플레이하세요.", "확인");
        }
        else
        {
            EditorUtility.DisplayDialog("정보", "제거할 InteractableUnityEventWrapper가 없습니다.", "확인");
        }
    }

    [MenuItem("Atoms/Fill Periodic Table (기존 Image 복제)")]
    public static void Setup()
    {
        // PeriodicTablePanel 찾기
        var allTransforms = Object.FindObjectsOfType<RectTransform>(true);
        RectTransform panel = null;

        foreach (var t in allTransforms)
        {
            if (t.name == "PeriodicTablePanel")
            {
                panel = t;
                break;
            }
        }

        if (panel == null)
        {
            EditorUtility.DisplayDialog("오류", "PeriodicTablePanel을 찾을 수 없습니다!", "확인");
            return;
        }

        // Image 자식 찾기
        Transform imageChild = panel.Find("Image");
        if (imageChild == null)
        {
            EditorUtility.DisplayDialog("오류", "PeriodicTablePanel 안에 Image가 없습니다!", "확인");
            return;
        }

        GameObject templateImage = imageChild.gameObject;

        // atomPrefab 찾기
        GameObject atomPrefab = null;
        var dispenser = templateImage.GetComponent<AtomPullDispenser>();
        if (dispenser != null)
        {
            atomPrefab = dispenser.atomPrefab;
        }

        if (atomPrefab == null)
        {
            atomPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Atom.prefab");
        }

        // 기존 자식들 삭제 (원본 Image 제외)
        for (int i = panel.childCount - 1; i >= 0; i--)
        {
            var child = panel.GetChild(i);
            if (child.gameObject != templateImage)
            {
                Object.DestroyImmediate(child.gameObject);
            }
        }

        // 원소 생성
        int count = Mathf.Min(36, symbols.Length);
        for (int i = 0; i < count; i++)
        {
            CreateElement(panel, templateImage, atomPrefab, symbols[i], colors[i]);
        }

        // 원본 템플릿 비활성화
        templateImage.SetActive(false);

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(panel.gameObject.scene);

        EditorUtility.DisplayDialog("완료",
            $"{count}개 원소가 생성되었습니다!\n\n" +
            "VR 손으로 원소를 잡으면 Atom이 생성됩니다.", "확인");
    }

    private static void CreateElement(Transform parent, GameObject template, GameObject atomPrefab, string symbol, Color color)
    {
        GameObject element = Object.Instantiate(template, parent);
        element.name = $"Element_{symbol}";
        element.SetActive(true);

        // 색상 설정
        var image = element.GetComponent<Image>();
        if (image != null)
        {
            image.color = color;
        }

        // AtomPullDispenser 설정
        var disp = element.GetComponent<AtomPullDispenser>();
        if (disp != null)
        {
            disp.elementSymbol = symbol;
            disp.atomColor = color;
            if (atomPrefab != null)
            {
                disp.atomPrefab = atomPrefab;
            }
        }

        // XRSimpleInteractable 이벤트 연결
        var interactable = element.GetComponent<XRSimpleInteractable>();
        if (interactable != null && disp != null)
        {
            // SerializedObject로 이벤트 연결
            SerializedObject so = new SerializedObject(interactable);
            SerializedProperty selectEnteredProp = so.FindProperty("m_SelectEntered.m_PersistentCalls.m_Calls");

            // 기존 이벤트 클리어 후 새로 추가
            selectEnteredProp.ClearArray();
            selectEnteredProp.InsertArrayElementAtIndex(0);

            SerializedProperty call = selectEnteredProp.GetArrayElementAtIndex(0);
            call.FindPropertyRelative("m_Target").objectReferenceValue = disp;
            call.FindPropertyRelative("m_TargetAssemblyTypeName").stringValue = "AtomPullDispenser, Assembly-CSharp";
            call.FindPropertyRelative("m_MethodName").stringValue = "OnGrabAttempt";
            call.FindPropertyRelative("m_Mode").intValue = 0;
            call.FindPropertyRelative("m_CallState").intValue = 2;

            so.ApplyModifiedProperties();
        }

        // Meta SDK PokeInteractable 및 관련 컴포넌트 추가
        AddMetaSDKInteraction(element, disp);

        // 텍스트 추가 (TextMeshPro - 3D용)
        var tmp3D = element.GetComponentInChildren<TextMeshPro>();
        if (tmp3D != null)
        {
            tmp3D.text = symbol;
            tmp3D.fontSize = 0.3f;  // World Space용 작은 크기
            tmp3D.enableAutoSizing = false;
        }

        // TextMeshProUGUI (UI용) 확인
        var tmpUI = element.GetComponentInChildren<TextMeshProUGUI>();
        if (tmpUI != null)
        {
            tmpUI.text = symbol;
            tmpUI.fontSize = 0.2f;  // World Space Canvas용 작은 크기
            tmpUI.enableAutoSizing = false;
        }

        // 텍스트가 없으면 새로 생성
        if (tmp3D == null && tmpUI == null)
        {
            GameObject textObj = new GameObject("SymbolText");
            textObj.transform.SetParent(element.transform, false);

            var rectTransform = textObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.localScale = Vector3.one;

            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = symbol;
            text.fontSize = 0.2f;  // World Space Canvas용
            text.enableAutoSizing = false;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.fontStyle = FontStyles.Bold;
        }

        EditorUtility.SetDirty(element);
    }

    private static void AddMetaSDKInteraction(GameObject element, AtomPullDispenser disp)
    {
        // BoxCollider 확인/추가
        var collider = element.GetComponent<BoxCollider>();
        if (collider == null)
        {
            collider = element.AddComponent<BoxCollider>();
        }

        // RectTransform 크기에 맞게 Collider 설정
        var rt = element.GetComponent<RectTransform>();
        if (rt != null)
        {
            float width = rt.rect.width > 0 ? rt.rect.width : 0.5f;
            float height = rt.rect.height > 0 ? rt.rect.height : 0.5f;
            collider.size = new Vector3(width, height, 0.1f);
            collider.center = Vector3.zero;
        }
        else
        {
            collider.size = new Vector3(0.5f, 0.5f, 0.1f);
        }
        collider.isTrigger = false;

        // Rigidbody 추가
        var rb = element.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = element.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
        rb.useGravity = false;

        // Meta SDK 타입을 모든 로드된 어셈블리에서 찾기
        System.Type grabbableType = null;
        System.Type handGrabType = null;
        System.Type wrapperType = null;

        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            if (grabbableType == null)
                grabbableType = assembly.GetType("Oculus.Interaction.Grabbable");
            if (handGrabType == null)
                handGrabType = assembly.GetType("Oculus.Interaction.HandGrab.HandGrabInteractable");
            if (wrapperType == null)
                wrapperType = assembly.GetType("Oculus.Interaction.InteractableUnityEventWrapper");

            if (grabbableType != null && handGrabType != null && wrapperType != null)
                break;
        }

        if (grabbableType == null)
        {
            Debug.LogError($"원소 {disp.elementSymbol}: Oculus.Interaction.Grabbable 타입을 찾을 수 없습니다! Meta XR SDK가 설치되어 있나요?");
            return;
        }

        Debug.Log($"Meta SDK 타입 찾음: Grabbable={grabbableType != null}, HandGrab={handGrabType != null}, Wrapper={wrapperType != null}");

        // Grabbable 추가
        Component grabbable = element.GetComponent(grabbableType);
        if (grabbable == null)
        {
            grabbable = element.AddComponent(grabbableType);
        }

        // SerializedObject로 Grabbable 필드 설정
        SerializedObject grabSO = new SerializedObject(grabbable);
        var rbProp = grabSO.FindProperty("_rigidbody");
        if (rbProp != null) rbProp.objectReferenceValue = rb;
        var targetProp = grabSO.FindProperty("_targetTransform");
        if (targetProp != null) targetProp.objectReferenceValue = element.transform;
        grabSO.ApplyModifiedProperties();
        EditorUtility.SetDirty(grabbable);

        Debug.Log($"원소 {disp.elementSymbol}: Grabbable 추가됨");

        // HandGrabInteractable 추가
        if (handGrabType != null)
        {
            Component handGrab = element.GetComponent(handGrabType);
            if (handGrab == null)
            {
                handGrab = element.AddComponent(handGrabType);
            }

            SerializedObject hgSO = new SerializedObject(handGrab);
            var pointableProp = hgSO.FindProperty("_pointableElement");
            if (pointableProp != null) pointableProp.objectReferenceValue = grabbable;
            var hgRbProp = hgSO.FindProperty("_rigidbody");
            if (hgRbProp != null) hgRbProp.objectReferenceValue = rb;
            hgSO.ApplyModifiedProperties();
            EditorUtility.SetDirty(handGrab);

            Debug.Log($"원소 {disp.elementSymbol}: HandGrabInteractable 추가됨");
        }

        // InteractableUnityEventWrapper는 사용하지 않음
        // AtomPullDispenser.Update()에서 Grabbable.State를 직접 체크함
    }
}
#endif
