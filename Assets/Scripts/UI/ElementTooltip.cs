using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using AtomSim.Data;

namespace AtomSim.UI
{
    /// <summary>
    /// Displays tooltip information when hovering over an element.
    /// </summary>
    public class ElementTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Tooltip Panel")]
        [SerializeField] private GameObject tooltipPanel;
        [SerializeField] private TextMeshProUGUI tooltipText;
        [SerializeField] private Vector2 tooltipOffset = new Vector2(60, 0);

        [Header("Settings")]
        [SerializeField] private float showDelay = 0.3f;
        [SerializeField] private bool followMouse = true;

        private PeriodicTableData.ElementInfo? elementInfo;
        private Canvas parentCanvas;
        private RectTransform tooltipRect;
        private bool isHovering;
        private float hoverTimer;

        // Shared tooltip instance for all elements (optional)
        private static GameObject sharedTooltip;
        private static TextMeshProUGUI sharedTooltipText;
        private static RectTransform sharedTooltipRect;

        private void Awake()
        {
            parentCanvas = GetComponentInParent<Canvas>();

            if (tooltipPanel != null)
            {
                tooltipRect = tooltipPanel.GetComponent<RectTransform>();
                tooltipPanel.SetActive(false);
            }
        }

        private void Update()
        {
            if (isHovering)
            {
                hoverTimer += Time.deltaTime;
                if (hoverTimer >= showDelay && !IsTooltipVisible())
                {
                    ShowTooltip();
                }

                if (followMouse && IsTooltipVisible())
                {
                    UpdateTooltipPosition();
                }
            }
        }

        public void SetElementInfo(PeriodicTableData.ElementInfo info)
        {
            elementInfo = info;
        }

        public void SetElementInfo(AtomData atomData)
        {
            if (atomData == null) return;

            elementInfo = new PeriodicTableData.ElementInfo(
                atomData.atomicNumber,
                atomData.symbol,
                atomData.elementName,
                atomData.atomicMass,
                atomData.group,
                atomData.period,
                atomData.category,
                atomData.elementColor
            );
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovering = true;
            hoverTimer = 0f;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovering = false;
            hoverTimer = 0f;
            HideTooltip();
        }

        private void ShowTooltip()
        {
            if (!elementInfo.HasValue) return;

            var info = elementInfo.Value;
            string tooltipContent = FormatTooltipText(info);

            // Use local tooltip if available
            if (tooltipPanel != null && tooltipText != null)
            {
                tooltipText.text = tooltipContent;
                tooltipPanel.SetActive(true);
                UpdateTooltipPosition();
            }
            // Otherwise try shared tooltip
            else if (sharedTooltip != null && sharedTooltipText != null)
            {
                sharedTooltipText.text = tooltipContent;
                sharedTooltip.SetActive(true);
                UpdateSharedTooltipPosition();
            }
        }

        private void HideTooltip()
        {
            if (tooltipPanel != null)
            {
                tooltipPanel.SetActive(false);
            }

            if (sharedTooltip != null)
            {
                sharedTooltip.SetActive(false);
            }
        }

        private bool IsTooltipVisible()
        {
            if (tooltipPanel != null) return tooltipPanel.activeSelf;
            if (sharedTooltip != null) return sharedTooltip.activeSelf;
            return false;
        }

        private void UpdateTooltipPosition()
        {
            if (tooltipRect == null || parentCanvas == null) return;

            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                Input.mousePosition,
                parentCanvas.worldCamera,
                out mousePos
            );

            tooltipRect.anchoredPosition = mousePos + tooltipOffset;
            ClampToScreen(tooltipRect);
        }

        private void UpdateSharedTooltipPosition()
        {
            if (sharedTooltipRect == null || parentCanvas == null) return;

            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                Input.mousePosition,
                parentCanvas.worldCamera,
                out mousePos
            );

            sharedTooltipRect.anchoredPosition = mousePos + tooltipOffset;
            ClampToScreen(sharedTooltipRect);
        }

        private void ClampToScreen(RectTransform rect)
        {
            if (parentCanvas == null) return;

            var canvasRect = parentCanvas.transform as RectTransform;
            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);

            // Simple clamping - could be improved
            Vector2 pos = rect.anchoredPosition;
            Vector2 size = rect.sizeDelta;
            Vector2 canvasSize = canvasRect.sizeDelta;

            // Clamp right edge
            if (pos.x + size.x > canvasSize.x / 2)
            {
                pos.x = canvasSize.x / 2 - size.x;
            }
            // Clamp bottom edge
            if (pos.y - size.y < -canvasSize.y / 2)
            {
                pos.y = -canvasSize.y / 2 + size.y;
            }

            rect.anchoredPosition = pos;
        }

        private string FormatTooltipText(PeriodicTableData.ElementInfo info)
        {
            return $"<b>{info.elementName}</b>\n" +
                   $"Symbol: {info.symbol}\n" +
                   $"Atomic Number: {info.atomicNumber}\n" +
                   $"Atomic Mass: {info.atomicMass:F3}\n" +
                   $"Category: {FormatCategory(info.category)}\n" +
                   $"Group: {info.group}, Period: {info.period}";
        }

        private string FormatCategory(ElementCategory category)
        {
            switch (category)
            {
                case ElementCategory.AlkaliMetal: return "Alkali Metal";
                case ElementCategory.AlkalineEarthMetal: return "Alkaline Earth Metal";
                case ElementCategory.TransitionMetal: return "Transition Metal";
                case ElementCategory.PostTransitionMetal: return "Post-Transition Metal";
                case ElementCategory.Metalloid: return "Metalloid";
                case ElementCategory.Nonmetal: return "Nonmetal";
                case ElementCategory.Halogen: return "Halogen";
                case ElementCategory.NobleGas: return "Noble Gas";
                case ElementCategory.Lanthanide: return "Lanthanide";
                case ElementCategory.Actinide: return "Actinide";
                default: return "Unknown";
            }
        }

        /// <summary>
        /// Sets up a shared tooltip panel that all elements can use.
        /// Call this once during scene initialization.
        /// </summary>
        public static void SetupSharedTooltip(GameObject tooltipObj, TextMeshProUGUI text)
        {
            sharedTooltip = tooltipObj;
            sharedTooltipText = text;
            if (tooltipObj != null)
            {
                sharedTooltipRect = tooltipObj.GetComponent<RectTransform>();
                tooltipObj.SetActive(false);
            }
        }
    }
}
