#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using AtomSim.UI;

namespace AtomSim.Editor
{
    /// <summary>
    /// PeriodicTablePanel에 원소 버튼 시스템을 빠르게 설정합니다.
    /// </summary>
    public class PeriodicTableQuickSetup
    {
        [MenuItem("Atoms/Setup Periodic Table (VR Grab)")]
        public static void SetupPeriodicTable()
        {
            // 1. PeriodicTablePanel 찾기
            var allRectTransforms = Object.FindObjectsOfType<RectTransform>(true);
            RectTransform periodicTablePanel = null;

            foreach (var rt in allRectTransforms)
            {
                if (rt.name == "PeriodicTablePanel")
                {
                    periodicTablePanel = rt;
                    break;
                }
            }

            if (periodicTablePanel == null)
            {
                EditorUtility.DisplayDialog("오류", "PeriodicTablePanel을 찾을 수 없습니다!\n씬에 PeriodicTablePanel이 있는지 확인하세요.", "확인");
                return;
            }

            // 2. PeriodicTableGenerator 추가
            var generator = periodicTablePanel.GetComponent<PeriodicTableGenerator>();
            if (generator == null)
            {
                generator = periodicTablePanel.gameObject.AddComponent<PeriodicTableGenerator>();
                Debug.Log("PeriodicTableGenerator 컴포넌트 추가됨");
            }

            // 3. Atom 프리팹 찾아서 설정
            GameObject atomPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Atom.prefab");
            if (atomPrefab != null)
            {
                SerializedObject serializedGen = new SerializedObject(generator);
                serializedGen.FindProperty("atomPrefab").objectReferenceValue = atomPrefab;
                serializedGen.ApplyModifiedProperties();
                Debug.Log("Atom 프리팹 연결됨");
            }
            else
            {
                Debug.LogWarning("Assets/Atom.prefab을 찾을 수 없습니다!");
            }

            // 4. ElementButton 프리팹 생성
            GameObject elementButtonPrefab = CreateElementButtonPrefab();
            if (elementButtonPrefab != null)
            {
                SerializedObject serializedGen = new SerializedObject(generator);
                serializedGen.FindProperty("elementButtonPrefab").objectReferenceValue = elementButtonPrefab;
                serializedGen.ApplyModifiedProperties();
                Debug.Log("ElementButton 프리팹 연결됨");
            }

            // 5. GridLayoutGroup 확인 및 설정
            var gridLayout = periodicTablePanel.GetComponent<GridLayoutGroup>();
            if (gridLayout == null)
            {
                gridLayout = periodicTablePanel.gameObject.AddComponent<GridLayoutGroup>();
            }

            // 현재 패널 크기에 맞게 셀 크기 조정
            gridLayout.cellSize = new Vector2(0.045f, 0.045f);  // World Space Canvas용 작은 크기
            gridLayout.spacing = new Vector2(0.002f, 0.002f);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 18;
            gridLayout.childAlignment = TextAnchor.UpperLeft;

            EditorUtility.SetDirty(periodicTablePanel.gameObject);
            Selection.activeGameObject = periodicTablePanel.gameObject;

            EditorUtility.DisplayDialog("설정 완료!",
                "PeriodicTablePanel 설정이 완료되었습니다!\n\n" +
                "사용 방법:\n" +
                "1. Play 모드로 들어가면 원소 버튼이 자동 생성됩니다\n" +
                "2. VR 손으로 원소 버튼을 잡으면(Grab) Atom이 생성됩니다\n" +
                "3. 생성된 Atom을 끌어서 이동할 수 있습니다",
                "확인");
        }

        [MenuItem("Atoms/Create ElementButton Prefab")]
        public static GameObject CreateElementButtonPrefab()
        {
            // 디렉토리 확인
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/UI"))
                AssetDatabase.CreateFolder("Assets/Prefabs", "UI");

            // 버튼 오브젝트 생성
            GameObject buttonObj = new GameObject("ElementButton");

            // RectTransform
            var rectTransform = buttonObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(0.045f, 0.045f);

            // Image (배경)
            var image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.3f, 0.5f, 0.8f, 0.9f);

            // BoxCollider (VR 인터랙션용)
            var collider = buttonObj.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.045f, 0.045f, 0.01f);

            // CanvasGroup
            buttonObj.AddComponent<CanvasGroup>();

            // 기호 텍스트
            GameObject symbolObj = new GameObject("SymbolText");
            symbolObj.transform.SetParent(buttonObj.transform, false);

            var symbolRect = symbolObj.AddComponent<RectTransform>();
            symbolRect.anchorMin = Vector2.zero;
            symbolRect.anchorMax = Vector2.one;
            symbolRect.offsetMin = new Vector2(0.002f, 0.005f);
            symbolRect.offsetMax = new Vector2(-0.002f, -0.012f);

            var symbolText = symbolObj.AddComponent<TextMeshProUGUI>();
            symbolText.text = "X";
            symbolText.fontSize = 0.025f;
            symbolText.fontStyle = FontStyles.Bold;
            symbolText.alignment = TextAlignmentOptions.Center;
            symbolText.color = Color.white;
            symbolText.enableAutoSizing = true;
            symbolText.fontSizeMin = 0.01f;
            symbolText.fontSizeMax = 0.03f;

            // 원자번호 텍스트
            GameObject numberObj = new GameObject("NumberText");
            numberObj.transform.SetParent(buttonObj.transform, false);

            var numberRect = numberObj.AddComponent<RectTransform>();
            numberRect.anchorMin = new Vector2(0, 1);
            numberRect.anchorMax = new Vector2(1, 1);
            numberRect.pivot = new Vector2(0.5f, 1);
            numberRect.anchoredPosition = new Vector2(0, -0.001f);
            numberRect.sizeDelta = new Vector2(0, 0.01f);

            var numberText = numberObj.AddComponent<TextMeshProUGUI>();
            numberText.text = "0";
            numberText.fontSize = 0.008f;
            numberText.alignment = TextAlignmentOptions.Center;
            numberText.color = new Color(1, 1, 1, 0.7f);

            // ElementButton 컴포넌트
            var elementButton = buttonObj.AddComponent<ElementButton>();

            // 프리팹 저장
            string prefabPath = "Assets/Prefabs/UI/ElementButton.prefab";

            // 기존 프리팹이 있으면 삭제
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                AssetDatabase.DeleteAsset(prefabPath);
            }

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(buttonObj, prefabPath);
            Object.DestroyImmediate(buttonObj);

            Debug.Log($"ElementButton 프리팹 생성됨: {prefabPath}");
            return prefab;
        }

        [MenuItem("Atoms/Generate Table Now (Play Mode Only)")]
        public static void GenerateTableNow()
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("알림", "Play 모드에서만 실행할 수 있습니다.", "확인");
                return;
            }

            var generator = Object.FindObjectOfType<PeriodicTableGenerator>();
            if (generator != null)
            {
                generator.GenerateTable();
                Debug.Log("주기율표 생성 완료!");
            }
            else
            {
                Debug.LogError("PeriodicTableGenerator를 찾을 수 없습니다!");
            }
        }
    }
}
#endif
