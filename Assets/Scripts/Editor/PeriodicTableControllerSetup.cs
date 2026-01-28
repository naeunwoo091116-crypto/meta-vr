#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using AtomSim.UI;

namespace AtomSim.Editor
{
    /// <summary>
    /// Simple setup utility for adding PeriodicTableController to existing panel.
    /// </summary>
    public class PeriodicTableControllerSetup : EditorWindow
    {
        [MenuItem("Atoms/Setup Existing Periodic Table Panel")]
        public static void SetupExistingPanel()
        {
            // Find PeriodicTablePanel in scene
            var allObjects = GameObject.FindObjectsOfType<RectTransform>(true);
            RectTransform periodicTablePanel = null;

            foreach (var obj in allObjects)
            {
                if (obj.name == "PeriodicTablePanel")
                {
                    periodicTablePanel = obj;
                    break;
                }
            }

            if (periodicTablePanel == null)
            {
                EditorUtility.DisplayDialog("Error", "PeriodicTablePanel not found in scene!", "OK");
                return;
            }

            // Add PeriodicTableController if not present
            var controller = periodicTablePanel.GetComponent<PeriodicTableController>();
            if (controller == null)
            {
                controller = periodicTablePanel.gameObject.AddComponent<PeriodicTableController>();
                Debug.Log("Added PeriodicTableController to PeriodicTablePanel");
            }

            // Find the Image child (periodic table image)
            var imageChild = periodicTablePanel.Find("Image");
            if (imageChild != null)
            {
                SerializedObject serializedController = new SerializedObject(controller);
                serializedController.FindProperty("periodicTableImage").objectReferenceValue = imageChild.GetComponent<RectTransform>();
                serializedController.ApplyModifiedProperties();
                Debug.Log("Set periodicTableImage reference");
            }

            // Find and assign atom prefab
            GameObject atomPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Atom.prefab");
            if (atomPrefab != null)
            {
                SerializedObject serializedController = new SerializedObject(controller);
                serializedController.FindProperty("atomPrefab").objectReferenceValue = atomPrefab;
                serializedController.ApplyModifiedProperties();
                Debug.Log("Set atomPrefab reference");
            }
            else
            {
                Debug.LogWarning("Atom.prefab not found at Assets/Atom.prefab");
            }

            // Find parent canvas
            var canvas = periodicTablePanel.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                SerializedObject serializedController = new SerializedObject(controller);
                serializedController.FindProperty("parentCanvas").objectReferenceValue = canvas;
                serializedController.ApplyModifiedProperties();
            }

            Selection.activeGameObject = periodicTablePanel.gameObject;
            EditorUtility.SetDirty(periodicTablePanel.gameObject);

            EditorUtility.DisplayDialog("Success",
                "PeriodicTableController added to PeriodicTablePanel!\n\n" +
                "How to use:\n" +
                "1. Click on an element in the periodic table\n" +
                "2. Drag to spawn an atom\n" +
                "3. The atom will appear in 3D space",
                "OK");
        }

        [MenuItem("Atoms/Create Atom Prefab (if missing)")]
        public static void CreateAtomPrefabIfMissing()
        {
            // Check if Atom.prefab exists
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Atom.prefab");
            if (existingPrefab != null)
            {
                EditorUtility.DisplayDialog("Info", "Atom.prefab already exists!", "OK");
                Selection.activeObject = existingPrefab;
                return;
            }

            // Create atom prefab
            GameObject atomObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            atomObj.name = "Atom";
            atomObj.transform.localScale = Vector3.one * 0.1f;
            atomObj.tag = "Atom";

            // Add Rigidbody
            var rb = atomObj.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = false;

            // Add XRGrabInteractable if XR Interaction Toolkit is available
            var xrGrabType = System.Type.GetType("UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable, Unity.XR.Interaction.Toolkit");
            if (xrGrabType != null)
            {
                atomObj.AddComponent(xrGrabType);
            }

            // Add GrabbableAtom
            var grabbableAtomType = System.Type.GetType("AtomSim.Atoms.GrabbableAtom, Assembly-CSharp");
            if (grabbableAtomType != null)
            {
                atomObj.AddComponent(grabbableAtomType);
            }

            // Create TextMeshPro child for symbol
            var textObj = new GameObject("SymbolText");
            textObj.transform.SetParent(atomObj.transform, false);
            textObj.transform.localPosition = Vector3.zero;
            textObj.transform.localScale = Vector3.one * 10f; // Scale up for visibility

            var tmpType = System.Type.GetType("TMPro.TextMeshPro, Unity.TextMeshPro");
            if (tmpType != null)
            {
                var tmp = textObj.AddComponent(tmpType);
                var textProp = tmpType.GetProperty("text");
                if (textProp != null) textProp.SetValue(tmp, "X");

                var alignProp = tmpType.GetProperty("alignment");
                if (alignProp != null) alignProp.SetValue(tmp, 514); // Center

                var sizeProp = tmpType.GetProperty("fontSize");
                if (sizeProp != null) sizeProp.SetValue(tmp, 2f);
            }

            // Save as prefab
            string prefabPath = "Assets/Atom.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(atomObj, prefabPath);
            DestroyImmediate(atomObj);

            Debug.Log($"Created Atom prefab at: {prefabPath}");
            Selection.activeObject = prefab;

            EditorUtility.DisplayDialog("Success", "Atom.prefab created!", "OK");
        }
    }
}
#endif
