#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using AtomSim.UI;
using AtomSim.Data;

namespace AtomSim.Editor
{
    /// <summary>
    /// Editor utility for setting up the Periodic Table UI and related prefabs.
    /// </summary>
    public class PeriodicTableSetup : EditorWindow
    {
        private GameObject atomPrefab;
        private bool createElementButtonPrefab = true;
        private bool createDropZone = true;
        private bool setupScene = true;

        [MenuItem("Atoms/Setup Periodic Table")]
        public static void ShowWindow()
        {
            GetWindow<PeriodicTableSetup>("Periodic Table Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("Periodic Table Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            atomPrefab = (GameObject)EditorGUILayout.ObjectField("Atom Prefab", atomPrefab, typeof(GameObject), false);

            EditorGUILayout.Space();
            createElementButtonPrefab = EditorGUILayout.Toggle("Create Element Button Prefab", createElementButtonPrefab);
            createDropZone = EditorGUILayout.Toggle("Create Drop Zone Prefab", createDropZone);
            setupScene = EditorGUILayout.Toggle("Setup Scene Objects", setupScene);

            EditorGUILayout.Space();

            if (GUILayout.Button("Setup All", GUILayout.Height(40)))
            {
                SetupAll();
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "This will create:\n" +
                "1. Element Button Prefab (for periodic table cells)\n" +
                "2. Drop Zone Prefab (for spawning atoms)\n" +
                "3. Scene objects (Canvas, Periodic Table, Drop Zone)",
                MessageType.Info);
        }

        private void SetupAll()
        {
            // Find Atom prefab if not assigned
            if (atomPrefab == null)
            {
                atomPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Atom.prefab");
            }

            if (atomPrefab == null)
            {
                Debug.LogError("Atom prefab not found! Please assign or create it first.");
                return;
            }

            EnsureDirectories();

            if (createElementButtonPrefab)
            {
                CreateElementButtonPrefab();
            }

            if (createDropZone)
            {
                CreateDropZonePrefab();
            }

            if (setupScene)
            {
                SetupSceneObjects();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Periodic Table Setup Complete!");
        }

        private void EnsureDirectories()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/UI"))
                AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
        }

        [MenuItem("Atoms/Create Element Button Prefab")]
        public static void CreateElementButtonPrefab()
        {
            EnsureDirectoriesStatic();

            // Create root object
            GameObject buttonObj = new GameObject("ElementButton");

            // Add RectTransform
            RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(45, 55);

            // Add Image for background
            Image bgImage = buttonObj.AddComponent<Image>();
            bgImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);

            // Add CanvasGroup for drag transparency
            buttonObj.AddComponent<CanvasGroup>();

            // Add ElementDragHandler
            ElementDragHandler dragHandler = buttonObj.AddComponent<ElementDragHandler>();

            // Add ElementTooltip
            buttonObj.AddComponent<ElementTooltip>();

            // Create Symbol Text
            GameObject symbolObj = new GameObject("SymbolText");
            symbolObj.transform.SetParent(buttonObj.transform, false);
            RectTransform symbolRect = symbolObj.AddComponent<RectTransform>();
            symbolRect.anchorMin = Vector2.zero;
            symbolRect.anchorMax = Vector2.one;
            symbolRect.offsetMin = new Vector2(2, 5);
            symbolRect.offsetMax = new Vector2(-2, -15);

            TextMeshProUGUI symbolText = symbolObj.AddComponent<TextMeshProUGUI>();
            symbolText.text = "X";
            symbolText.fontSize = 18;
            symbolText.fontStyle = FontStyles.Bold;
            symbolText.alignment = TextAlignmentOptions.Center;
            symbolText.color = Color.white;

            // Create Atomic Number Text
            GameObject numberObj = new GameObject("AtomicNumberText");
            numberObj.transform.SetParent(buttonObj.transform, false);
            RectTransform numberRect = numberObj.AddComponent<RectTransform>();
            numberRect.anchorMin = new Vector2(0, 1);
            numberRect.anchorMax = new Vector2(1, 1);
            numberRect.pivot = new Vector2(0.5f, 1);
            numberRect.anchoredPosition = new Vector2(0, -2);
            numberRect.sizeDelta = new Vector2(0, 12);

            TextMeshProUGUI numberText = numberObj.AddComponent<TextMeshProUGUI>();
            numberText.text = "0";
            numberText.fontSize = 8;
            numberText.alignment = TextAlignmentOptions.Center;
            numberText.color = new Color(1, 1, 1, 0.8f);

            // Create Element Name Text
            GameObject nameObj = new GameObject("ElementNameText");
            nameObj.transform.SetParent(buttonObj.transform, false);
            RectTransform nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0);
            nameRect.anchorMax = new Vector2(1, 0);
            nameRect.pivot = new Vector2(0.5f, 0);
            nameRect.anchoredPosition = new Vector2(0, 2);
            nameRect.sizeDelta = new Vector2(0, 10);

            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = "Name";
            nameText.fontSize = 6;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.color = new Color(1, 1, 1, 0.7f);
            nameText.enableWordWrapping = false;
            nameText.overflowMode = TextOverflowModes.Truncate;

            // Save as prefab
            string prefabPath = "Assets/Prefabs/UI/ElementButton.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(buttonObj, prefabPath);
            DestroyImmediate(buttonObj);

            Debug.Log($"Created Element Button Prefab at: {prefabPath}");
            Selection.activeObject = prefab;
        }

        [MenuItem("Atoms/Create Drop Zone Prefab")]
        public static void CreateDropZonePrefab()
        {
            EnsureDirectoriesStatic();

            // Create root object
            GameObject dropZoneObj = new GameObject("AtomDropZone");

            // Add RectTransform
            RectTransform rectTransform = dropZoneObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200, 200);

            // Add Image for background
            Image bgImage = dropZoneObj.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);

            // Add AtomDropZone component
            AtomDropZone dropZone = dropZoneObj.AddComponent<AtomDropZone>();

            // Create instruction text
            GameObject textObj = new GameObject("InstructionText");
            textObj.transform.SetParent(dropZoneObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            TextMeshProUGUI instructionText = textObj.AddComponent<TextMeshProUGUI>();
            instructionText.text = "Drag element here\nto spawn atom";
            instructionText.fontSize = 16;
            instructionText.alignment = TextAlignmentOptions.Center;
            instructionText.color = Color.white;

            // Link references via SerializedObject
            SerializedObject serializedDropZone = new SerializedObject(dropZone);
            serializedDropZone.FindProperty("dropZoneImage").objectReferenceValue = bgImage;
            serializedDropZone.FindProperty("instructionText").objectReferenceValue = instructionText;

            // Find and assign atom prefab
            GameObject atomPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Atom.prefab");
            if (atomPrefab != null)
            {
                serializedDropZone.FindProperty("atomPrefab").objectReferenceValue = atomPrefab;
            }

            serializedDropZone.ApplyModifiedProperties();

            // Save as prefab
            string prefabPath = "Assets/Prefabs/UI/AtomDropZone.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(dropZoneObj, prefabPath);
            DestroyImmediate(dropZoneObj);

            Debug.Log($"Created Drop Zone Prefab at: {prefabPath}");
            Selection.activeObject = prefab;
        }

        [MenuItem("Atoms/Setup Scene Objects")]
        public static void SetupSceneObjects()
        {
            // Find or create Canvas
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("PeriodicTableCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();

                var scaler = canvasObj.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
            }

            // Add EventSystem if not present
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // Create Periodic Table container
            GameObject tableContainer = new GameObject("PeriodicTable");
            tableContainer.transform.SetParent(canvas.transform, false);
            RectTransform tableRect = tableContainer.AddComponent<RectTransform>();
            tableRect.anchorMin = new Vector2(0, 1);
            tableRect.anchorMax = new Vector2(0, 1);
            tableRect.pivot = new Vector2(0, 1);
            tableRect.anchoredPosition = new Vector2(20, -20);

            // Add PeriodicTableUI component
            PeriodicTableUI tableUI = tableContainer.AddComponent<PeriodicTableUI>();

            // Load and assign element button prefab
            GameObject elementButtonPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/ElementButton.prefab");
            if (elementButtonPrefab != null)
            {
                SerializedObject serializedTable = new SerializedObject(tableUI);
                serializedTable.FindProperty("elementButtonPrefab").objectReferenceValue = elementButtonPrefab;
                serializedTable.ApplyModifiedProperties();
            }

            // Create Drop Zone
            GameObject dropZoneObj = new GameObject("DropZone");
            dropZoneObj.transform.SetParent(canvas.transform, false);
            RectTransform dropRect = dropZoneObj.AddComponent<RectTransform>();
            dropRect.anchorMin = new Vector2(1, 0.5f);
            dropRect.anchorMax = new Vector2(1, 0.5f);
            dropRect.pivot = new Vector2(1, 0.5f);
            dropRect.anchoredPosition = new Vector2(-50, 0);
            dropRect.sizeDelta = new Vector2(250, 250);

            Image dropImage = dropZoneObj.AddComponent<Image>();
            dropImage.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);

            AtomDropZone dropZone = dropZoneObj.AddComponent<AtomDropZone>();

            // Create drop zone text
            GameObject dropTextObj = new GameObject("DropText");
            dropTextObj.transform.SetParent(dropZoneObj.transform, false);
            RectTransform dropTextRect = dropTextObj.AddComponent<RectTransform>();
            dropTextRect.anchorMin = Vector2.zero;
            dropTextRect.anchorMax = Vector2.one;
            dropTextRect.offsetMin = Vector2.zero;
            dropTextRect.offsetMax = Vector2.zero;

            TextMeshProUGUI dropText = dropTextObj.AddComponent<TextMeshProUGUI>();
            dropText.text = "Drag Element Here\nto Spawn Atom";
            dropText.fontSize = 20;
            dropText.alignment = TextAlignmentOptions.Center;
            dropText.color = Color.white;

            // Link drop zone references
            SerializedObject serializedDropZone = new SerializedObject(dropZone);
            serializedDropZone.FindProperty("dropZoneImage").objectReferenceValue = dropImage;
            serializedDropZone.FindProperty("instructionText").objectReferenceValue = dropText;

            GameObject atomPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Atom.prefab");
            if (atomPrefab != null)
            {
                serializedDropZone.FindProperty("atomPrefab").objectReferenceValue = atomPrefab;
            }
            serializedDropZone.FindProperty("use3DSpawning").boolValue = true;
            serializedDropZone.ApplyModifiedProperties();

            // Create Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(canvas.transform, false);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1);
            titleRect.anchorMax = new Vector2(0.5f, 1);
            titleRect.pivot = new Vector2(0.5f, 1);
            titleRect.anchoredPosition = new Vector2(0, -10);
            titleRect.sizeDelta = new Vector2(600, 40);

            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Periodic Table of Elements";
            titleText.fontSize = 28;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;

            Debug.Log("Scene objects created successfully!");
            Selection.activeGameObject = tableContainer;
        }

        private static void EnsureDirectoriesStatic()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/UI"))
                AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
        }
    }
}
#endif
