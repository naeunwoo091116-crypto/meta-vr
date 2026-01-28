using UnityEngine;
using UnityEditor;
using UnityEngine.XR.Interaction.Toolkit;
using System;

/// <summary>
/// 모든 인터랙션 컴포넌트 제거 - 시뮬레이션 복구용
/// </summary>
public class MetaPokeSetup : EditorWindow
{
    [MenuItem("Atoms/인터랙션 컴포넌트 전체 제거")]
    public static void ShowWindow()
    {
        GetWindow<MetaPokeSetup>("인터랙션 정리");
    }

    private void OnGUI()
    {
        GUILayout.Label("시뮬레이션 복구", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "시뮬레이션이 안 나오는 문제 해결:\n" +
            "모든 Element에서 Grabbable, Wrapper 등 제거\n" +
            "HandGrabDetector만 사용",
            MessageType.Warning);

        GUILayout.Space(10);

        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("모든 인터랙션 컴포넌트 제거", GUILayout.Height(40)))
        {
            RemoveEverything();
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(10);

        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("HandGrabDetector만 추가", GUILayout.Height(40)))
        {
            RemoveEverything();
            AddHandGrabDetector();
        }
        GUI.backgroundColor = Color.white;
    }

    private static void RemoveEverything()
    {
        Type grabbableType = null;
        Type pokeInteractableType = null;
        Type pointableWrapperType = null;
        Type interactableWrapperType = null;

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (grabbableType == null)
                grabbableType = assembly.GetType("Oculus.Interaction.Grabbable");
            if (pokeInteractableType == null)
                pokeInteractableType = assembly.GetType("Oculus.Interaction.PokeInteractable");
            if (pointableWrapperType == null)
                pointableWrapperType = assembly.GetType("Oculus.Interaction.PointableUnityEventWrapper");
            if (interactableWrapperType == null)
                interactableWrapperType = assembly.GetType("Oculus.Interaction.InteractableUnityEventWrapper");
        }

        var elements = GameObject.FindObjectsOfType<AtomPullDispenser>();
        int count = 0;

        foreach (var element in elements)
        {
            // 모든 것 제거
            if (grabbableType != null)
            {
                var c = element.GetComponent(grabbableType);
                if (c != null) { Undo.DestroyObjectImmediate(c); count++; }
            }
            if (pointableWrapperType != null)
            {
                var c = element.GetComponent(pointableWrapperType);
                if (c != null) { Undo.DestroyObjectImmediate(c); count++; }
            }
            if (interactableWrapperType != null)
            {
                var c = element.GetComponent(interactableWrapperType);
                if (c != null) { Undo.DestroyObjectImmediate(c); count++; }
            }
            if (pokeInteractableType != null)
            {
                var c = element.GetComponent(pokeInteractableType);
                if (c != null) { Undo.DestroyObjectImmediate(c); count++; }
            }

            var xr1 = element.GetComponent<XRSimpleInteractable>();
            if (xr1 != null) { Undo.DestroyObjectImmediate(xr1); count++; }

            var xr2 = element.GetComponent<XRBaseInteractable>();
            if (xr2 != null) { Undo.DestroyObjectImmediate(xr2); count++; }

            var h1 = element.GetComponent<SimplePokeHandler>();
            if (h1 != null) { Undo.DestroyObjectImmediate(h1); count++; }

            var h2 = element.GetComponent<SimpleGrabHandler>();
            if (h2 != null) { Undo.DestroyObjectImmediate(h2); count++; }

            var h3 = element.GetComponent<RuntimeGrabDetector>();
            if (h3 != null) { Undo.DestroyObjectImmediate(h3); count++; }

            var h4 = element.GetComponent<HandGrabDetector>();
            if (h4 != null) { Undo.DestroyObjectImmediate(h4); count++; }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log($"[정리] {count}개 컴포넌트 제거 완료");
    }

    private static void AddHandGrabDetector()
    {
        var elements = GameObject.FindObjectsOfType<AtomPullDispenser>();
        int count = 0;

        foreach (var element in elements)
        {
            var detector = element.GetComponent<HandGrabDetector>();
            if (detector == null)
            {
                Undo.AddComponent(element.gameObject, typeof(HandGrabDetector));
                count++;
            }

            // Collider를 Trigger로 설정
            var collider = element.GetComponent<Collider>();
            if (collider != null)
            {
                Undo.RecordObject(collider, "Set Trigger");
                collider.isTrigger = true;
            }

            EditorUtility.SetDirty(element.gameObject);
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log($"[설정] {count}개 Element에 HandGrabDetector 추가 완료");
    }
}
