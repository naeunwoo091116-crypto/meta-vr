using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using AtomSim.Data;
using AtomSim.Atoms;

namespace AtomSim.UI
{
    /// <summary>
    /// Controller for the existing PeriodicTablePanel.
    /// Detects clicks/touches on the periodic table image and spawns corresponding atoms.
    /// Attach this to the PeriodicTablePanel GameObject.
    /// </summary>
    public class PeriodicTableController : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("References")]
        [SerializeField] private RectTransform periodicTableImage;
        [SerializeField] private GameObject atomPrefab;
        [SerializeField] private Transform spawnParent;

        [Header("Table Layout Settings")]
        [Tooltip("Number of columns in periodic table (standard = 18)")]
        [SerializeField] private int columns = 18;
        [Tooltip("Number of rows in periodic table (standard = 10 including lanthanides/actinides)")]
        [SerializeField] private int rows = 10;

        [Header("Drag Preview")]
        [SerializeField] private GameObject dragPreviewPrefab;
        [SerializeField] private Canvas parentCanvas;

        [Header("Spawn Settings")]
        [SerializeField] private Vector3 spawnOffset = new Vector3(0, 0, 2f);
        [SerializeField] private bool spawnInWorldSpace = true;

        // Runtime
        private GameObject currentDragPreview;
        private AtomData currentDragElement;
        private RectTransform canvasRect;
        private Camera mainCamera;
        private bool isDragging;

        // Standard periodic table layout mapping
        // [period, group] -> atomic number (0 = empty)
        private static readonly int[,] periodicTableLayout = new int[10, 18]
        {
            // Period 1
            { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2 },
            // Period 2
            { 3, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 6, 7, 8, 9, 10 },
            // Period 3
            { 11, 12, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 13, 14, 15, 16, 17, 18 },
            // Period 4
            { 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36 },
            // Period 5
            { 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54 },
            // Period 6
            { 55, 56, 57, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86 },
            // Period 7
            { 87, 88, 89, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118 },
            // Empty row (gap before lanthanides)
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            // Lanthanides (Ce to Lu)
            { 0, 0, 0, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 0 },
            // Actinides (Th to Lr)
            { 0, 0, 0, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 100, 101, 102, 103, 0 }
        };

        private void Awake()
        {
            mainCamera = Camera.main;

            if (periodicTableImage == null)
            {
                periodicTableImage = GetComponent<RectTransform>();
            }

            if (parentCanvas == null)
            {
                parentCanvas = GetComponentInParent<Canvas>();
            }

            if (parentCanvas != null)
            {
                canvasRect = parentCanvas.GetComponent<RectTransform>();
            }

            // Find atom prefab if not assigned
            if (atomPrefab == null)
            {
                atomPrefab = Resources.Load<GameObject>("Atom");
                if (atomPrefab == null)
                {
                    // Try loading from Assets folder
                    #if UNITY_EDITOR
                    atomPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Atom.prefab");
                    #endif
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            var element = GetElementAtPosition(eventData.position);
            if (element.HasValue)
            {
                currentDragElement = PeriodicTableData.CreateAtomData(element.Value);
                Debug.Log($"Selected element: {element.Value.elementName} ({element.Value.symbol})");
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (currentDragElement == null) return;

            isDragging = true;
            CreateDragPreview(eventData.position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging || currentDragPreview == null) return;

            UpdateDragPreviewPosition(eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging) return;

            isDragging = false;

            // Spawn atom at drop position
            if (currentDragElement != null)
            {
                SpawnAtom(eventData.position);
            }

            // Clean up preview
            if (currentDragPreview != null)
            {
                Destroy(currentDragPreview);
                currentDragPreview = null;
            }

            currentDragElement = null;
        }

        private PeriodicTableData.ElementInfo? GetElementAtPosition(Vector2 screenPosition)
        {
            if (periodicTableImage == null) return null;

            // Convert screen position to local position in the image
            Vector2 localPoint;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                periodicTableImage, screenPosition, parentCanvas?.worldCamera, out localPoint))
            {
                return null;
            }

            // Get image dimensions
            Rect rect = periodicTableImage.rect;

            // Normalize position (0-1)
            float normalizedX = (localPoint.x - rect.xMin) / rect.width;
            float normalizedY = (localPoint.y - rect.yMin) / rect.height;

            // Flip Y because UI coordinates have Y going up
            normalizedY = 1f - normalizedY;

            // Convert to grid position
            int col = Mathf.FloorToInt(normalizedX * columns);
            int row = Mathf.FloorToInt(normalizedY * rows);

            // Clamp to valid range
            col = Mathf.Clamp(col, 0, columns - 1);
            row = Mathf.Clamp(row, 0, rows - 1);

            // Get atomic number from layout
            int atomicNumber = periodicTableLayout[row, col];

            if (atomicNumber <= 0 || atomicNumber > 118)
            {
                return null;
            }

            return PeriodicTableData.GetElementByAtomicNumber(atomicNumber);
        }

        private void CreateDragPreview(Vector2 screenPosition)
        {
            if (currentDragElement == null) return;

            // Create preview object
            if (dragPreviewPrefab != null)
            {
                currentDragPreview = Instantiate(dragPreviewPrefab, parentCanvas.transform);
            }
            else
            {
                // Create a simple preview
                currentDragPreview = new GameObject("DragPreview");
                currentDragPreview.transform.SetParent(parentCanvas.transform, false);

                var rectTransform = currentDragPreview.AddComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(60, 60);

                var image = currentDragPreview.AddComponent<Image>();
                image.color = currentDragElement.elementColor;

                var textObj = new GameObject("Symbol");
                textObj.transform.SetParent(currentDragPreview.transform, false);
                var textRect = textObj.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;

                var text = textObj.AddComponent<TextMeshProUGUI>();
                text.text = currentDragElement.symbol;
                text.fontSize = 24;
                text.fontStyle = FontStyles.Bold;
                text.alignment = TextAlignmentOptions.Center;
                text.color = Color.white;
            }

            // Add CanvasGroup for transparency
            var canvasGroup = currentDragPreview.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = currentDragPreview.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 0.8f;
            canvasGroup.blocksRaycasts = false;

            UpdateDragPreviewPosition(screenPosition);
        }

        private void UpdateDragPreviewPosition(Vector2 screenPosition)
        {
            if (currentDragPreview == null || canvasRect == null) return;

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, screenPosition, parentCanvas?.worldCamera, out localPoint);

            var previewRect = currentDragPreview.GetComponent<RectTransform>();
            if (previewRect != null)
            {
                previewRect.anchoredPosition = localPoint;
            }
        }

        private void SpawnAtom(Vector2 screenPosition)
        {
            if (atomPrefab == null || currentDragElement == null)
            {
                Debug.LogWarning("Cannot spawn atom: Missing prefab or element data");
                return;
            }

            Vector3 spawnPosition;

            if (spawnInWorldSpace && mainCamera != null)
            {
                // Spawn in 3D world space in front of camera
                Ray ray = mainCamera.ScreenPointToRay(screenPosition);
                spawnPosition = ray.origin + ray.direction * spawnOffset.z;
            }
            else
            {
                // Spawn at fixed position
                spawnPosition = mainCamera != null
                    ? mainCamera.transform.position + mainCamera.transform.forward * spawnOffset.z + spawnOffset
                    : spawnOffset;
            }

            // Instantiate atom
            GameObject atomObj = Instantiate(atomPrefab, spawnPosition, Quaternion.identity, spawnParent);
            atomObj.name = $"Atom_{currentDragElement.symbol}";

            // Initialize with element data
            var grabbableAtom = atomObj.GetComponent<GrabbableAtom>();
            if (grabbableAtom != null)
            {
                grabbableAtom.Initialize(currentDragElement);
            }

            Debug.Log($"Spawned {currentDragElement.elementName} atom at {spawnPosition}");

            // Fire event
            OnAtomSpawned?.Invoke(grabbableAtom, currentDragElement);
        }

        /// <summary>
        /// Gets the element at a specific grid position.
        /// </summary>
        public PeriodicTableData.ElementInfo? GetElementAtGrid(int row, int col)
        {
            if (row < 0 || row >= rows || col < 0 || col >= columns) return null;

            int atomicNumber = periodicTableLayout[row, col];
            if (atomicNumber <= 0 || atomicNumber > 118) return null;

            return PeriodicTableData.GetElementByAtomicNumber(atomicNumber);
        }

        /// <summary>
        /// Manually spawn an atom by atomic number.
        /// </summary>
        public GrabbableAtom SpawnAtomByNumber(int atomicNumber, Vector3 position)
        {
            var elementInfo = PeriodicTableData.GetElementByAtomicNumber(atomicNumber);
            if (!elementInfo.HasValue || atomPrefab == null) return null;

            var atomData = PeriodicTableData.CreateAtomData(elementInfo.Value);

            GameObject atomObj = Instantiate(atomPrefab, position, Quaternion.identity, spawnParent);
            atomObj.name = $"Atom_{atomData.symbol}";

            var grabbableAtom = atomObj.GetComponent<GrabbableAtom>();
            if (grabbableAtom != null)
            {
                grabbableAtom.Initialize(atomData);
            }

            return grabbableAtom;
        }

        /// <summary>
        /// Manually spawn an atom by symbol.
        /// </summary>
        public GrabbableAtom SpawnAtomBySymbol(string symbol, Vector3 position)
        {
            var elementInfo = PeriodicTableData.GetElementBySymbol(symbol);
            if (!elementInfo.HasValue) return null;

            return SpawnAtomByNumber(elementInfo.Value.atomicNumber, position);
        }

        // Events
        public delegate void AtomSpawnedEvent(GrabbableAtom atom, AtomData data);
        public event AtomSpawnedEvent OnAtomSpawned;
    }
}
