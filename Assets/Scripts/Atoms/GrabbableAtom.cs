using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using AtomSim.Data;

namespace AtomSim.Atoms
{
    /// <summary>
    /// Component for the actual atom object that players can grab and interact with.
    /// Supports both VR (XR Interaction) and Desktop (mouse drag) input.
    /// </summary>
    [RequireComponent(typeof(XRGrabInteractable))]
    public class GrabbableAtom : MonoBehaviour
    {
        [Header("Data")]
        public AtomData atomData;

        [Header("Visual References")]
        [SerializeField] private TextMeshPro symbolText;
        [SerializeField] private ParticleSystem electronCloud;
        [SerializeField] private Renderer mainRenderer;

        [Header("Desktop Drag Settings")]
        [SerializeField] private bool enableDesktopDrag = true;
        [SerializeField] private float dragSpeed = 10f;
        [SerializeField] private float minDragDistance = 0.5f;
        [SerializeField] private float maxDragDistance = 10f;

        [Header("Hover Feedback")]
        [SerializeField] private float hoverScaleMultiplier = 1.1f;
        [SerializeField] private Color hoverOutlineColor = Color.yellow;

        private XRGrabInteractable interactable;
        private Vector3 originalScale;
        private bool isHovered;
        private bool isDragging;
        private float dragDistance;
        private Camera mainCamera;
        private Plane dragPlane;
        private Vector3 dragOffset;

        // Events
        public delegate void AtomGrabbedEvent(GrabbableAtom atom);
        public event AtomGrabbedEvent OnGrabbed;
        public event AtomGrabbedEvent OnReleased;

        private void Awake()
        {
            interactable = GetComponent<XRGrabInteractable>();
            originalScale = transform.localScale;
            mainCamera = Camera.main;

            // Auto-find references if not set
            if (mainRenderer == null) mainRenderer = GetComponent<Renderer>();
            if (symbolText == null) symbolText = GetComponentInChildren<TextMeshPro>();
            if (electronCloud == null) electronCloud = GetComponentInChildren<ParticleSystem>();
        }

        private void Start()
        {
            if (atomData != null)
            {
                Initialize(atomData);
            }

            // Subscribe to XR events
            if (interactable != null)
            {
                interactable.selectEntered.AddListener(OnXRGrabbed);
                interactable.selectExited.AddListener(OnXRReleased);
                interactable.hoverEntered.AddListener(OnXRHoverEnter);
                interactable.hoverExited.AddListener(OnXRHoverExit);
            }
        }

        private void OnDestroy()
        {
            if (interactable != null)
            {
                interactable.selectEntered.RemoveListener(OnXRGrabbed);
                interactable.selectExited.RemoveListener(OnXRReleased);
                interactable.hoverEntered.RemoveListener(OnXRHoverEnter);
                interactable.hoverExited.RemoveListener(OnXRHoverExit);
            }
        }

        private void Update()
        {
            if (!enableDesktopDrag) return;

            HandleDesktopInput();
        }

        #region Initialization

        public void Initialize(AtomData data)
        {
            atomData = data;
            UpdateVisuals();
            gameObject.name = $"Atom_{data.symbol}";
        }

        public void Initialize(PeriodicTableData.ElementInfo elementInfo)
        {
            atomData = PeriodicTableData.CreateAtomData(elementInfo);
            UpdateVisuals();
            gameObject.name = $"Atom_{elementInfo.symbol}";
        }

        private void UpdateVisuals()
        {
            if (atomData == null) return;

            // Update Text
            if (symbolText != null)
            {
                symbolText.text = atomData.symbol;
            }

            // Update Color (Main Renderer)
            if (mainRenderer != null)
            {
                // Clone material to avoid changing shared material
                Material mat = mainRenderer.material;
                mat.color = atomData.elementColor;
                // If using Hologram shader, set the main color property
                if (mat.HasProperty("_MainColor"))
                {
                    mat.SetColor("_MainColor", atomData.elementColor);
                }
            }

            // Update Particle System Color
            if (electronCloud != null)
            {
                var main = electronCloud.main;
                main.startColor = atomData.elementColor;
            }
        }

        #endregion

        #region XR Interaction Callbacks

        private void OnXRGrabbed(SelectEnterEventArgs args)
        {
            isDragging = true;
            OnGrabbed?.Invoke(this);
        }

        private void OnXRReleased(SelectExitEventArgs args)
        {
            isDragging = false;
            OnReleased?.Invoke(this);
        }

        private void OnXRHoverEnter(HoverEnterEventArgs args)
        {
            SetHoverState(true);
        }

        private void OnXRHoverExit(HoverExitEventArgs args)
        {
            SetHoverState(false);
        }

        #endregion

        #region Desktop Mouse Drag

        private void HandleDesktopInput()
        {
            // Skip if XR is grabbing
            if (interactable != null && interactable.isSelected) return;

            if (Input.GetMouseButtonDown(0))
            {
                TryStartDrag();
            }
            else if (Input.GetMouseButton(0) && isDragging)
            {
                ContinueDrag();
            }
            else if (Input.GetMouseButtonUp(0) && isDragging)
            {
                EndDrag();
            }

            // Mouse wheel to adjust drag distance
            if (isDragging)
            {
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (Mathf.Abs(scroll) > 0.01f)
                {
                    dragDistance = Mathf.Clamp(dragDistance + scroll * 2f, minDragDistance, maxDragDistance);
                    UpdateDragPlane();
                }
            }
        }

        private void TryStartDrag()
        {
            if (mainCamera == null) return;

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f))
            {
                if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
                {
                    StartDrag(hit.point);
                }
            }
        }

        private void StartDrag(Vector3 hitPoint)
        {
            isDragging = true;
            dragDistance = Vector3.Distance(mainCamera.transform.position, transform.position);
            dragOffset = transform.position - hitPoint;
            UpdateDragPlane();

            // Make rigidbody kinematic during drag
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }

            OnGrabbed?.Invoke(this);
        }

        private void UpdateDragPlane()
        {
            if (mainCamera == null) return;
            dragPlane = new Plane(-mainCamera.transform.forward, transform.position);
        }

        private void ContinueDrag()
        {
            if (mainCamera == null) return;

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            float enter;

            if (dragPlane.Raycast(ray, out enter))
            {
                Vector3 targetPos = ray.GetPoint(enter) + dragOffset;

                // Smooth movement
                transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * dragSpeed);
            }
        }

        private void EndDrag()
        {
            isDragging = false;

            // Restore rigidbody
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }

            OnReleased?.Invoke(this);
        }

        #endregion

        #region Hover Feedback

        private void SetHoverState(bool hover)
        {
            isHovered = hover;

            if (hover)
            {
                transform.localScale = originalScale * hoverScaleMultiplier;
            }
            else
            {
                transform.localScale = originalScale;
            }
        }

        // Desktop mouse hover detection
        private void OnMouseEnter()
        {
            if (!isDragging && enableDesktopDrag)
            {
                SetHoverState(true);
            }
        }

        private void OnMouseExit()
        {
            if (!isDragging && enableDesktopDrag)
            {
                SetHoverState(false);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Changes this atom to a different element.
        /// </summary>
        public void ChangeElement(AtomData newData)
        {
            var oldData = atomData;
            Initialize(newData);
            Debug.Log($"Atom changed from {oldData?.symbol ?? "null"} to {newData.symbol}");
        }

        /// <summary>
        /// Changes this atom to a different element by atomic number.
        /// </summary>
        public void ChangeElement(int atomicNumber)
        {
            var elementInfo = PeriodicTableData.GetElementByAtomicNumber(atomicNumber);
            if (elementInfo.HasValue)
            {
                var newData = PeriodicTableData.CreateAtomData(elementInfo.Value);
                ChangeElement(newData);
            }
        }

        /// <summary>
        /// Changes this atom to a different element by symbol.
        /// </summary>
        public void ChangeElement(string symbol)
        {
            var elementInfo = PeriodicTableData.GetElementBySymbol(symbol);
            if (elementInfo.HasValue)
            {
                var newData = PeriodicTableData.CreateAtomData(elementInfo.Value);
                ChangeElement(newData);
            }
        }

        /// <summary>
        /// Returns true if this atom is currently being held.
        /// </summary>
        public bool IsGrabbed()
        {
            if (interactable != null && interactable.isSelected) return true;
            return isDragging;
        }

        /// <summary>
        /// Gets the element symbol.
        /// </summary>
        public string GetSymbol()
        {
            return atomData?.symbol ?? "";
        }

        /// <summary>
        /// Gets the atomic number.
        /// </summary>
        public int GetAtomicNumber()
        {
            return atomData?.atomicNumber ?? 0;
        }

        #endregion
    }
}
