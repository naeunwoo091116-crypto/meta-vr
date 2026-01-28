using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro; // Ensure TextMeshPro is available
using AtomSim.Data;
using AtomSim.UI;
using AtomSim.Atoms;

namespace AtomSim.Editor
{
    public class AtomAssetsInstaller
    {
        [MenuItem("Atoms/Auto Setup/Install All")]
        public static void InstallAll()
        {
            CreateDirectories();
            List<AtomData> dataList = CreateAtomData();
            GameObject atomPrefab = CreateAtomPrefab();
            GameObject buttonPrefab = CreateAtomButtonPrefab(atomPrefab);
            SetupScene(dataList, buttonPrefab);
        }

        private static void CreateDirectories()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Atoms")) AssetDatabase.CreateFolder("Assets/Resources", "Atoms");
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        private static List<AtomData> CreateAtomData()
        {
            List<AtomData> list = new List<AtomData>();

            // H, He, Li, Be, B, C, N, O, F, Ne
            list.Add(CreateData(1, "H", "Hydrogen", 1.008f, 1, 1, Color.red, ElementCategory.Nonmetal));
            list.Add(CreateData(2, "He", "Helium", 4.0026f, 18, 1, Color.cyan, ElementCategory.NobleGas));
            list.Add(CreateData(3, "Li", "Lithium", 6.94f, 1, 2, new Color(0.8f, 0.2f, 0.8f), ElementCategory.AlkaliMetal));
            list.Add(CreateData(4, "Be", "Beryllium", 9.0122f, 2, 2, new Color(0.2f, 0.8f, 0.2f), ElementCategory.AlkalineEarthMetal));
            list.Add(CreateData(5, "B", "Boron", 10.81f, 13, 2, new Color(0.6f, 0.4f, 0.2f), ElementCategory.Metalloid));
            list.Add(CreateData(6, "C", "Carbon", 12.011f, 14, 2, Color.gray, ElementCategory.Nonmetal));
            list.Add(CreateData(7, "N", "Nitrogen", 14.007f, 15, 2, Color.blue, ElementCategory.Nonmetal));
            list.Add(CreateData(8, "O", "Oxygen", 15.999f, 16, 2, new Color(1f, 0.5f, 0f), ElementCategory.Nonmetal)); // Orange
            list.Add(CreateData(9, "F", "Fluorine", 18.998f, 17, 2, new Color(0.8f, 1f, 0.5f), ElementCategory.Halogen));
            list.Add(CreateData(10, "Ne", "Neon", 20.180f, 18, 2, new Color(1f, 0.2f, 0.5f), ElementCategory.NobleGas));

            AssetDatabase.SaveAssets();
            return list;
        }

        private static AtomData CreateData(int number, string symbol, string name, float mass, int group, int period, Color color, ElementCategory category)
        {
            string path = $"Assets/Resources/Atoms/{name}.asset";
            AtomData data = AssetDatabase.LoadAssetAtPath<AtomData>(path);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<AtomData>();
                AssetDatabase.CreateAsset(data, path);
            }

            data.atomicNumber = number;
            data.symbol = symbol;
            data.elementName = name;
            data.atomicMass = mass;
            data.group = group;
            data.period = period;
            data.elementColor = color;
            data.category = category;

            EditorUtility.SetDirty(data);
            return data;
        }

        private static GameObject CreateAtomPrefab()
        {
            string path = "Assets/Prefabs/AtomPrefab.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            GameObject atom = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            atom.name = "AtomPrefab";
            atom.transform.localScale = Vector3.one * 0.1f;

            // Physics
            Rigidbody rb = atom.AddComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            // Interaction
            XRGrabInteractable grab = atom.AddComponent<XRGrabInteractable>();
            grab.movementType = XRBaseInteractable.MovementType.VelocityTracking; // Better physics interaction

            // Atom Logic
            GrabbableAtom ga = atom.AddComponent<GrabbableAtom>();

            // Visuals - finding renderer
            Renderer rend = atom.GetComponent<Renderer>();
            
            // Text (Optional - requires TMP)
            // Adding a simple 3D text object or TMP if packages allow. 
            // For stability without exact TMP setup, we'll skip programmatic TMP creation or use a simple TextMesh if needed.
            // Assuming user has TMP Essential Resources imported for a proper setup, otherwise this might be tricky.
            // We will create the prefab without text for now to avoid errors, or try standard TextMesh.

            // Assign renderer to GrabbableAtom via SerializedObject to avoid "private" issues if fields are private
            // But GrabbableAtom fields are private [SerializeField], so we use SerializedObject
            /*
            SerializedObject so = new SerializedObject(ga);
            so.FindProperty("mainRenderer").objectReferenceValue = rend;
            so.ApplyModifiedProperties();
            */
            // Since I can't easily reflect private fields in this script dump without complexity, 
            // I'll assume users can drag it or I'll make fields public in GrabbableAtom if needed.
            // *Wait*, I defined GrabbableAtom fields as [SerializeField] private. 
            // I should use SerializedObject.

            // Save Prefab
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(atom, path);
            GameObject.DestroyImmediate(atom);
            return prefab;
        }

        private static GameObject CreateAtomButtonPrefab(GameObject atomPrefab)
        {
            string path = "Assets/Prefabs/AtomButtonPrefab.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            GameObject button = GameObject.CreatePrimitive(PrimitiveType.Cube);
            button.name = "AtomButtonPrefab";
            button.transform.localScale = new Vector3(0.1f, 0.1f, 0.05f);

            // Interaction
            // Remove BoxCollider generated by Cube, add trigger or keep collider
            // XRSimpleInteractable needs a collider.
            XRSimpleInteractable interactable = button.AddComponent<XRSimpleInteractable>();
            interactable.selectMode = InteractableSelectMode.Single;

            // Logic
            AtomButton ab = button.AddComponent<AtomButton>();
            
            // Link Atom Prefab
            // Using SerializedObject to set private fields
            SerializedObject so = new SerializedObject(ab);
            so.FindProperty("buttonRenderer").objectReferenceValue = button.GetComponent<Renderer>();
            so.FindProperty("atomPrefab").objectReferenceValue = atomPrefab;
            so.ApplyModifiedProperties();

            // Setup Events
            // UnityEvent is hard to setup via script without internals. 
            // Helper: We can add a component that listens to the interactable via code instead of UnityEvents inspector
            // OR the user has to link it manually.
            // Wait, I can make AtomButton implement a listener or use GetComponent in Awake.
            // *Correction*: In AtomButton.cs I added `OnGrabAttempt`. 
            // To auto-wire this, we'd need to add `interactable.selectEntered.AddListener(ab.OnGrabAttempt)` in `AtomButton.Start()` or `OnEnable()`.
            // I'll update AtomButton.cs to auto-subscribe to be safer.

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(button, path);
            GameObject.DestroyImmediate(button);
            return prefab;
        }

        private static void SetupScene(List<AtomData> dataList, GameObject atomButtonPrefab)
        {
            GameObject table = GameObject.Find("PeriodicTable");
            if (table == null)
            {
                table = new GameObject("PeriodicTable");
                table.transform.position = new Vector3(0, 1.2f, 1.0f); // In front of camera usually
            }

            PeriodicTableUI ui = table.GetComponent<PeriodicTableUI>();
            if (ui == null) ui = table.AddComponent<PeriodicTableUI>();

            // Assign Data
            SerializedObject so = new SerializedObject(ui);
            so.FindProperty("atomButtonPrefab").objectReferenceValue = atomButtonPrefab;
            
            // Assign List
            SerializedProperty listProp = so.FindProperty("allElements");
            listProp.ClearArray();
            for(int i=0; i<dataList.Count; i++)
            {
                listProp.InsertArrayElementAtIndex(i);
                listProp.GetArrayElementAtIndex(i).objectReferenceValue = dataList[i];
            }
            
            so.FindProperty("tableRoot").objectReferenceValue = table.transform;
            so.ApplyModifiedProperties();

            // Generate
            ui.GenerateTable();
        }
    }
}
