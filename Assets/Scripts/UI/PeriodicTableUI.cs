using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using AtomSim.Data;
using TMPro;

namespace AtomSim.UI
{
    /// <summary>
    /// Generates a complete periodic table UI with drag and drop functionality.
    /// Supports all 118 elements with proper positioning including lanthanides and actinides.
    /// </summary>
    public class PeriodicTableUI : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private GameObject elementButtonPrefab;
        [SerializeField] private Transform tableRoot;
        [SerializeField] private Vector2 cellSize = new Vector2(45, 55);
        [SerializeField] private Vector2 cellSpacing = new Vector2(2, 2);

        [Header("Lanthanide/Actinide Offset")]
        [SerializeField] private float lanthanideYOffset = 80f;
        [SerializeField] private int lanthanideStartColumn = 3;

        [Header("Optional: Manual Element List")]
        [Tooltip("Leave empty to use all 118 elements from PeriodicTableData")]
        public List<AtomData> manualElements;

        [Header("Runtime Options")]
        [SerializeField] private bool generateOnStart = true;
        [SerializeField] private bool useAllElements = true;

        [Header("Title")]
        [SerializeField] private TextMeshProUGUI titleText;

        private Dictionary<int, GameObject> elementButtons = new Dictionary<int, GameObject>();

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
            if (tableRoot == null) tableRoot = transform;

            ClearTable();

            if (elementButtonPrefab == null)
            {
                Debug.LogError("PeriodicTableUI: Element button prefab is missing!");
                return;
            }

            if (useAllElements)
            {
                GenerateAllElements();
            }
            else if (manualElements != null && manualElements.Count > 0)
            {
                GenerateFromManualList();
            }
            else
            {
                Debug.LogWarning("PeriodicTableUI: No elements to generate. Enable 'Use All Elements' or add manual elements.");
            }

            if (titleText != null)
            {
                titleText.text = "Periodic Table of Elements";
            }
        }

        private void GenerateAllElements()
        {
            var allElements = PeriodicTableData.GetAllElements();
            Debug.Log($"PeriodicTableUI: Generating {allElements.Count} elements");

            foreach (var elementInfo in allElements)
            {
                CreateElementButton(elementInfo);
            }
        }

        private void GenerateFromManualList()
        {
            foreach (var atomData in manualElements)
            {
                if (atomData == null) continue;

                var elementInfo = new PeriodicTableData.ElementInfo(
                    atomData.atomicNumber,
                    atomData.symbol,
                    atomData.elementName,
                    atomData.atomicMass,
                    atomData.group,
                    atomData.period,
                    atomData.category,
                    atomData.elementColor
                );

                CreateElementButton(elementInfo);
            }
        }

        private void CreateElementButton(PeriodicTableData.ElementInfo elementInfo)
        {
            GameObject buttonObj = Instantiate(elementButtonPrefab, tableRoot);
            buttonObj.name = $"Element_{elementInfo.symbol}_{elementInfo.atomicNumber}";

            // Calculate position
            Vector2 position = CalculateElementPosition(elementInfo);

            // Apply position
            RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = position;
                rectTransform.sizeDelta = cellSize;
                rectTransform.localRotation = Quaternion.identity;
                rectTransform.localScale = Vector3.one;
            }

            // Initialize the element drag handler
            var dragHandler = buttonObj.GetComponent<ElementDragHandler>();
            if (dragHandler != null)
            {
                dragHandler.Initialize(elementInfo);
            }

            // Fallback to AtomButton if ElementDragHandler not present
            var atomButton = buttonObj.GetComponent<AtomButton>();
            if (atomButton != null && dragHandler == null)
            {
                var atomData = PeriodicTableData.CreateAtomData(elementInfo);
                atomButton.Initialize(atomData);
            }

            // Setup visual elements
            SetupElementVisuals(buttonObj, elementInfo);

            elementButtons[elementInfo.atomicNumber] = buttonObj;
        }

        private Vector2 CalculateElementPosition(PeriodicTableData.ElementInfo elementInfo)
        {
            int col = elementInfo.group - 1;
            int row = elementInfo.period - 1;

            // Handle lanthanides (period 9 in our data = row 8 below main table)
            if (elementInfo.period == 9)
            {
                col = lanthanideStartColumn + (elementInfo.group - 4);
                row = 7; // First row below the gap
                return new Vector2(
                    col * (cellSize.x + cellSpacing.x),
                    -(row * (cellSize.y + cellSpacing.y) + lanthanideYOffset)
                );
            }

            // Handle actinides (period 10 in our data = row 9 below main table)
            if (elementInfo.period == 10)
            {
                col = lanthanideStartColumn + (elementInfo.group - 4);
                row = 8; // Second row below the gap
                return new Vector2(
                    col * (cellSize.x + cellSpacing.x),
                    -(row * (cellSize.y + cellSpacing.y) + lanthanideYOffset)
                );
            }

            // Standard position
            return new Vector2(
                col * (cellSize.x + cellSpacing.x),
                -row * (cellSize.y + cellSpacing.y)
            );
        }

        private void SetupElementVisuals(GameObject buttonObj, PeriodicTableData.ElementInfo elementInfo)
        {
            // Find and set up text elements
            var texts = buttonObj.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var text in texts)
            {
                string name = text.gameObject.name.ToLower();
                if (name.Contains("symbol"))
                {
                    text.text = elementInfo.symbol;
                }
                else if (name.Contains("number") || name.Contains("atomic"))
                {
                    text.text = elementInfo.atomicNumber.ToString();
                }
                else if (name.Contains("name"))
                {
                    text.text = elementInfo.elementName;
                }
                else if (name.Contains("mass"))
                {
                    text.text = elementInfo.atomicMass.ToString("F2");
                }
            }

            // If no specific text elements found, use the first TMP text for symbol
            if (texts.Length > 0 && !texts[0].gameObject.name.ToLower().Contains("symbol"))
            {
                texts[0].text = elementInfo.symbol;
            }

            // Set background color
            var image = buttonObj.GetComponent<Image>();
            if (image != null)
            {
                image.color = elementInfo.color;
            }

            // Add tooltip or hover info
            var tooltip = buttonObj.GetComponent<ElementTooltip>();
            if (tooltip != null)
            {
                tooltip.SetElementInfo(elementInfo);
            }
        }

        private void ClearTable()
        {
            var children = new List<GameObject>();
            foreach (Transform child in tableRoot)
            {
                children.Add(child.gameObject);
            }

            foreach (var child in children)
            {
                if (Application.isPlaying)
                    Destroy(child);
                else
                    DestroyImmediate(child);
            }

            elementButtons.Clear();
        }

        /// <summary>
        /// Gets the button GameObject for a specific element.
        /// </summary>
        public GameObject GetElementButton(int atomicNumber)
        {
            elementButtons.TryGetValue(atomicNumber, out var button);
            return button;
        }

        /// <summary>
        /// Gets the button GameObject for a specific element by symbol.
        /// </summary>
        public GameObject GetElementButton(string symbol)
        {
            var elementInfo = PeriodicTableData.GetElementBySymbol(symbol);
            if (elementInfo.HasValue)
            {
                return GetElementButton(elementInfo.Value.atomicNumber);
            }
            return null;
        }

        /// <summary>
        /// Highlights a specific element.
        /// </summary>
        public void HighlightElement(int atomicNumber, Color highlightColor)
        {
            var button = GetElementButton(atomicNumber);
            if (button != null)
            {
                var image = button.GetComponent<Image>();
                if (image != null)
                {
                    image.color = highlightColor;
                }
            }
        }

        /// <summary>
        /// Resets all element colors to their original category colors.
        /// </summary>
        public void ResetAllColors()
        {
            foreach (var kvp in elementButtons)
            {
                var elementInfo = PeriodicTableData.GetElementByAtomicNumber(kvp.Key);
                if (elementInfo.HasValue)
                {
                    var image = kvp.Value.GetComponent<Image>();
                    if (image != null)
                    {
                        image.color = elementInfo.Value.color;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the total width of the periodic table.
        /// </summary>
        public float GetTableWidth()
        {
            return 18 * (cellSize.x + cellSpacing.x);
        }

        /// <summary>
        /// Gets the total height of the periodic table (including lanthanides/actinides).
        /// </summary>
        public float GetTableHeight()
        {
            return 10 * (cellSize.y + cellSpacing.y) + lanthanideYOffset;
        }
    }
}
