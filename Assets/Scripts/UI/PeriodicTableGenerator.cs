using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace AtomSim.UI
{
    /// <summary>
    /// 기존 PeriodicTablePanel에 원소 버튼들을 자동 생성합니다.
    /// PeriodicTablePanel에 이 스크립트를 부착하세요.
    /// </summary>
    public class PeriodicTableGenerator : MonoBehaviour
    {
        [Header("필수 참조")]
        [SerializeField] private GameObject elementButtonPrefab;
        [SerializeField] private GameObject atomPrefab;

        [Header("생성 설정")]
        [SerializeField] private bool generateOnStart = true;
        [SerializeField] private int maxElements = 36; // 기본적으로 주기 1-4까지 (36개)

        private Transform contentParent;

        // 원소 데이터 (기호, 이름, 원자번호, 색상)
        private static readonly ElementData[] elements = new ElementData[]
        {
            // Period 1
            new ElementData("H", "Hydrogen", 1, new Color(0.4f, 0.6f, 1f)),
            new ElementData("He", "Helium", 2, new Color(0.9f, 0.5f, 0.9f)),

            // Period 2
            new ElementData("Li", "Lithium", 3, new Color(1f, 0.4f, 0.4f)),
            new ElementData("Be", "Beryllium", 4, new Color(1f, 0.6f, 0.3f)),
            new ElementData("B", "Boron", 5, new Color(0.4f, 0.8f, 0.8f)),
            new ElementData("C", "Carbon", 6, new Color(0.4f, 0.6f, 1f)),
            new ElementData("N", "Nitrogen", 7, new Color(0.4f, 0.6f, 1f)),
            new ElementData("O", "Oxygen", 8, new Color(0.4f, 0.6f, 1f)),
            new ElementData("F", "Fluorine", 9, new Color(0.8f, 0.4f, 1f)),
            new ElementData("Ne", "Neon", 10, new Color(0.9f, 0.5f, 0.9f)),

            // Period 3
            new ElementData("Na", "Sodium", 11, new Color(1f, 0.4f, 0.4f)),
            new ElementData("Mg", "Magnesium", 12, new Color(1f, 0.6f, 0.3f)),
            new ElementData("Al", "Aluminum", 13, new Color(0.6f, 0.8f, 0.6f)),
            new ElementData("Si", "Silicon", 14, new Color(0.4f, 0.8f, 0.8f)),
            new ElementData("P", "Phosphorus", 15, new Color(0.4f, 0.6f, 1f)),
            new ElementData("S", "Sulfur", 16, new Color(0.4f, 0.6f, 1f)),
            new ElementData("Cl", "Chlorine", 17, new Color(0.8f, 0.4f, 1f)),
            new ElementData("Ar", "Argon", 18, new Color(0.9f, 0.5f, 0.9f)),

            // Period 4
            new ElementData("K", "Potassium", 19, new Color(1f, 0.4f, 0.4f)),
            new ElementData("Ca", "Calcium", 20, new Color(1f, 0.6f, 0.3f)),
            new ElementData("Sc", "Scandium", 21, new Color(1f, 0.8f, 0.4f)),
            new ElementData("Ti", "Titanium", 22, new Color(1f, 0.8f, 0.4f)),
            new ElementData("V", "Vanadium", 23, new Color(1f, 0.8f, 0.4f)),
            new ElementData("Cr", "Chromium", 24, new Color(1f, 0.8f, 0.4f)),
            new ElementData("Mn", "Manganese", 25, new Color(1f, 0.8f, 0.4f)),
            new ElementData("Fe", "Iron", 26, new Color(1f, 0.8f, 0.4f)),
            new ElementData("Co", "Cobalt", 27, new Color(1f, 0.8f, 0.4f)),
            new ElementData("Ni", "Nickel", 28, new Color(1f, 0.8f, 0.4f)),
            new ElementData("Cu", "Copper", 29, new Color(1f, 0.8f, 0.4f)),
            new ElementData("Zn", "Zinc", 30, new Color(1f, 0.8f, 0.4f)),
            new ElementData("Ga", "Gallium", 31, new Color(0.6f, 0.8f, 0.6f)),
            new ElementData("Ge", "Germanium", 32, new Color(0.4f, 0.8f, 0.8f)),
            new ElementData("As", "Arsenic", 33, new Color(0.4f, 0.8f, 0.8f)),
            new ElementData("Se", "Selenium", 34, new Color(0.4f, 0.6f, 1f)),
            new ElementData("Br", "Bromine", 35, new Color(0.8f, 0.4f, 1f)),
            new ElementData("Kr", "Krypton", 36, new Color(0.9f, 0.5f, 0.9f)),
        };

        private struct ElementData
        {
            public string symbol;
            public string name;
            public int number;
            public Color color;

            public ElementData(string symbol, string name, int number, Color color)
            {
                this.symbol = symbol;
                this.name = name;
                this.number = number;
                this.color = color;
            }
        }

        private void Start()
        {
            if (generateOnStart)
            {
                GenerateTable();
            }
        }

        [ContextMenu("Generate Periodic Table")]
        public void GenerateTable()
        {
            // 기존 자식들 삭제 (Image 제외)
            ClearExistingButtons();

            // 콘텐츠 부모 찾기 (GridLayoutGroup이 있는 곳)
            contentParent = GetComponent<GridLayoutGroup>() != null ? transform : transform;

            if (elementButtonPrefab == null)
            {
                Debug.LogError("PeriodicTableGenerator: elementButtonPrefab이 없습니다! 자동 생성합니다.");
                CreateDefaultButtons();
                return;
            }

            // Atom 프리팹 자동 찾기
            if (atomPrefab == null)
            {
                atomPrefab = Resources.Load<GameObject>("Atom");
                if (atomPrefab == null)
                {
                    #if UNITY_EDITOR
                    atomPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Atom.prefab");
                    #endif
                }
            }

            // 원소 버튼 생성
            int count = Mathf.Min(maxElements, elements.Length);
            for (int i = 0; i < count; i++)
            {
                CreateElementButton(elements[i]);
            }

            Debug.Log($"PeriodicTableGenerator: {count}개의 원소 버튼 생성 완료");
        }

        private void CreateElementButton(ElementData data)
        {
            GameObject buttonObj = Instantiate(elementButtonPrefab, contentParent);
            buttonObj.name = $"Element_{data.symbol}";

            // ElementButton 컴포넌트 설정
            var elementButton = buttonObj.GetComponent<ElementButton>();
            if (elementButton == null)
            {
                elementButton = buttonObj.AddComponent<ElementButton>();
            }

            elementButton.Setup(data.symbol, data.name, data.number, data.color);
            elementButton.SetAtomPrefab(atomPrefab);
        }

        /// <summary>
        /// 프리팹 없이 기본 버튼 생성
        /// </summary>
        private void CreateDefaultButtons()
        {
            if (atomPrefab == null)
            {
                #if UNITY_EDITOR
                atomPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Atom.prefab");
                #endif
            }

            int count = Mathf.Min(maxElements, elements.Length);
            for (int i = 0; i < count; i++)
            {
                CreateDefaultElementButton(elements[i]);
            }

            Debug.Log($"PeriodicTableGenerator: {count}개의 기본 원소 버튼 생성 완료");
        }

        private void CreateDefaultElementButton(ElementData data)
        {
            // 기본 버튼 오브젝트 생성
            GameObject buttonObj = new GameObject($"Element_{data.symbol}");
            buttonObj.transform.SetParent(contentParent, false);

            // RectTransform
            var rectTransform = buttonObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(50, 50);

            // Image (배경)
            var image = buttonObj.AddComponent<Image>();
            image.color = data.color;

            // BoxCollider (VR 인터랙션용)
            var collider = buttonObj.AddComponent<BoxCollider>();
            collider.size = new Vector3(50, 50, 10);

            // 텍스트 오브젝트 (기호)
            GameObject textObj = new GameObject("SymbolText");
            textObj.transform.SetParent(buttonObj.transform, false);

            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = data.symbol;
            text.fontSize = 24;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;

            // ElementButton 컴포넌트
            var elementButton = buttonObj.AddComponent<ElementButton>();
            elementButton.Setup(data.symbol, data.name, data.number, data.color);
            elementButton.SetAtomPrefab(atomPrefab);
        }

        private void ClearExistingButtons()
        {
            var children = new List<GameObject>();
            foreach (Transform child in transform)
            {
                // "Image" 라는 이름의 자식은 유지 (주기율표 배경 이미지)
                if (child.name != "Image")
                {
                    children.Add(child.gameObject);
                }
            }

            foreach (var child in children)
            {
                if (Application.isPlaying)
                    Destroy(child);
                else
                    DestroyImmediate(child);
            }
        }

        /// <summary>
        /// 특정 원소의 Atom 생성 (외부 호출용)
        /// </summary>
        public GameObject SpawnAtom(string symbol, Vector3 position)
        {
            if (atomPrefab == null) return null;

            GameObject newAtom = Instantiate(atomPrefab, position, Quaternion.identity);

            var atomConnector = newAtom.GetComponent<AtomConnector>();
            if (atomConnector != null)
            {
                atomConnector.elementSymbol = symbol;

                var textDisplay = newAtom.GetComponentInChildren<TextMeshPro>();
                if (textDisplay != null)
                {
                    textDisplay.text = symbol;
                }
            }

            return newAtom;
        }
    }
}
