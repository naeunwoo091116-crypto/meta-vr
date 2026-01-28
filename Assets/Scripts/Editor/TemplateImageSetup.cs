#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// 주기율표 템플릿 Image를 설정하는 에디터 스크립트.
/// 메뉴: Atoms > Setup Template Image
/// </summary>
public class TemplateImageSetup
{
    [MenuItem("Atoms/Setup Template Image (템플릿 설정)")]
    public static void SetupTemplateImage()
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

        // AtomPullDispenser 추가
        var dispenser = templateImage.GetComponent<AtomPullDispenser>();
        if (dispenser == null)
        {
            dispenser = templateImage.AddComponent<AtomPullDispenser>();
            Debug.Log("AtomPullDispenser 컴포넌트 추가됨");
        }

        // atomPrefab 설정
        if (dispenser.atomPrefab == null)
        {
            dispenser.atomPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Atom.prefab");
            if (dispenser.atomPrefab != null)
            {
                Debug.Log("atomPrefab 설정됨: Assets/Atom.prefab");
            }
            else
            {
                Debug.LogWarning("Assets/Atom.prefab을 찾을 수 없습니다!");
            }
        }

        // BoxCollider 추가
        var collider = templateImage.GetComponent<BoxCollider>();
        if (collider == null)
        {
            collider = templateImage.AddComponent<BoxCollider>();
            var rt = templateImage.GetComponent<RectTransform>();
            if (rt != null)
            {
                collider.size = new Vector3(rt.rect.width, rt.rect.height, 0.01f);
            }
            else
            {
                collider.size = new Vector3(0.05f, 0.05f, 0.01f);
            }
            collider.isTrigger = true;
            Debug.Log("BoxCollider 추가됨");
        }

        // Rigidbody 추가
        var rb = templateImage.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = templateImage.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            Debug.Log("Rigidbody 추가됨");
        }

        // Meta SDK Grabbable 추가
        var grabbableType = System.Type.GetType("Oculus.Interaction.Grabbable, Oculus.Interaction.Runtime");
        if (grabbableType != null)
        {
            var grabbable = templateImage.GetComponent(grabbableType);
            if (grabbable == null)
            {
                templateImage.AddComponent(grabbableType);
                Debug.Log("Grabbable 추가됨");
            }
        }
        else
        {
            Debug.LogWarning("Oculus.Interaction.Grabbable 타입을 찾을 수 없습니다.");
        }

        // XRSimpleInteractable 추가 (XR Toolkit 호환)
        var xrType = System.Type.GetType("UnityEngine.XR.Interaction.Toolkit.XRSimpleInteractable, Unity.XR.Interaction.Toolkit");
        if (xrType != null)
        {
            var xrInteractable = templateImage.GetComponent(xrType);
            if (xrInteractable == null)
            {
                templateImage.AddComponent(xrType);
                Debug.Log("XRSimpleInteractable 추가됨");
            }
        }

        EditorUtility.SetDirty(templateImage);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(templateImage.scene);

        EditorUtility.DisplayDialog("완료", 
            "템플릿 Image 설정 완료!\n\n" +
            "이제 'Atoms > Fill Periodic Table' 메뉴를 실행하세요.", "확인");
    }

    [MenuItem("Atoms/Debug: Check Template Components")]
    public static void CheckTemplateComponents()
    {
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
            Debug.LogError("PeriodicTablePanel을 찾을 수 없습니다!");
            return;
        }

        Transform imageChild = panel.Find("Image");
        if (imageChild == null)
        {
            Debug.LogError("Image 자식을 찾을 수 없습니다!");
            return;
        }

        GameObject img = imageChild.gameObject;
        
        Debug.Log("=== 템플릿 Image 컴포넌트 상태 ===");
        
        var dispenser = img.GetComponent<AtomPullDispenser>();
        if (dispenser != null)
        {
            Debug.Log($"✓ AtomPullDispenser 있음 - atomPrefab: {(dispenser.atomPrefab != null ? dispenser.atomPrefab.name : "NULL")}");
        }
        else
        {
            Debug.LogWarning("✗ AtomPullDispenser 없음");
        }

        var collider = img.GetComponent<BoxCollider>();
        Debug.Log($"{(collider != null ? "✓" : "✗")} BoxCollider: {(collider != null ? "있음" : "없음")}");

        var rb = img.GetComponent<Rigidbody>();
        Debug.Log($"{(rb != null ? "✓" : "✗")} Rigidbody: {(rb != null ? "있음" : "없음")}");

        var grabbableType = System.Type.GetType("Oculus.Interaction.Grabbable, Oculus.Interaction.Runtime");
        if (grabbableType != null)
        {
            var grabbable = img.GetComponent(grabbableType);
            Debug.Log($"{(grabbable != null ? "✓" : "✗")} Grabbable: {(grabbable != null ? "있음" : "없음")}");
        }

        Debug.Log("=================================");
    }
}
#endif
