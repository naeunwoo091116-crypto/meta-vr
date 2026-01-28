using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using AtomSim.Data;

namespace AtomSim.UI
{
    /// <summary>
    /// Handles drag and drop functionality for periodic table elements.
    /// Attach this to each element button in the periodic table.
    /// </summary>
    public class ElementDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Element Data")]
        public AtomData atomData;

        [Header("Visual References")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TextMeshProUGUI symbolText;
        [SerializeField] private TextMeshProUGUI atomicNumberText;
        [SerializeField] private TextMeshProUGUI elementNameText;

        [Header("Drag Settings")]
        [SerializeField] private float dragScale = 1.2f;
        [SerializeField] private float hoverScale = 1.1f;

        // Runtime references
        private Canvas parentCanvas;
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Vector3 originalScale;
        private Vector2 originalPosition;
        private Transform originalParent;
        private int originalSiblingIndex;

        // Static reference for drop zones to access
        public static ElementDragHandler CurrentDragging { get; private set; }

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            parentCanvas = GetComponentInParent<Canvas>();
            originalScale = rectTransform.localScale;

            // Auto-find references if not set
            if (backgroundImage == null) backgroundImage = GetComponent<Image>();
            if (symbolText == null) symbolText = GetComponentInChildren<TextMeshProUGUI>();
        }

        /// <summary>
        /// Initialize the element button with AtomData.
        /// </summary>
        public void Initialize(AtomData data)
        {
            atomData = data;
            UpdateVisuals();
        }

        /// <summary>
        /// Initialize the element button with ElementInfo data.
        /// </summary>
        public void Initialize(PeriodicTableData.ElementInfo info)
        {
            atomData = PeriodicTableData.CreateAtomData(info);
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (atomData == null) return;

            if (symbolText != null)
            {
                symbolText.text = atomData.symbol;
            }

            if (atomicNumberText != null)
            {
                atomicNumberText.text = atomData.atomicNumber.ToString();
            }

            if (elementNameText != null)
            {
                elementNameText.text = atomData.elementName;
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = atomData.elementColor;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (atomData == null) return;

            CurrentDragging = this;
            originalPosition = rectTransform.anchoredPosition;
            originalParent = transform.parent;
            originalSiblingIndex = transform.GetSiblingIndex();

            // Move to root canvas so it renders on top of everything
            if (parentCanvas != null)
            {
                transform.SetParent(parentCanvas.transform);
            }

            // Scale up and make semi-transparent
            rectTransform.localScale = originalScale * dragScale;
            canvasGroup.alpha = 0.8f;
            canvasGroup.blocksRaycasts = false;

            // Notify listeners
            OnDragStarted?.Invoke(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (atomData == null) return;

            // Follow mouse/touch position
            if (parentCanvas != null)
            {
                Vector2 pos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentCanvas.transform as RectTransform,
                    eventData.position,
                    parentCanvas.worldCamera,
                    out pos
                );
                rectTransform.localPosition = pos;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            CurrentDragging = null;

            // Reset visual state
            rectTransform.localScale = originalScale;
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;

            // Return to original position and parent
            transform.SetParent(originalParent);
            transform.SetSiblingIndex(originalSiblingIndex);
            rectTransform.anchoredPosition = originalPosition;

            // Notify listeners
            OnDragEnded?.Invoke(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (CurrentDragging != null) return; // Don't hover effect while dragging

            rectTransform.localScale = originalScale * hoverScale;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (CurrentDragging == this) return; // Don't reset if we're being dragged

            rectTransform.localScale = originalScale;
        }

        // Events for external listeners
        public delegate void DragEvent(ElementDragHandler handler);
        public static event DragEvent OnDragStarted;
        public static event DragEvent OnDragEnded;
    }
}
