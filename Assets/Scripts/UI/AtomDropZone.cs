using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using AtomSim.Data;
using AtomSim.Atoms;

namespace AtomSim.UI
{
    /// <summary>
    /// Drop zone where elements can be dropped to spawn/modify atoms.
    /// Place this on a UI element or 3D object where players can drop elements.
    /// </summary>
    public class AtomDropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Atom Spawning")]
        [SerializeField] private GameObject atomPrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private bool spawnOnDrop = true;

        [Header("Existing Atom Reference")]
        [Tooltip("If set, this atom will be modified instead of spawning a new one")]
        [SerializeField] private GrabbableAtom targetAtom;

        [Header("Visual Feedback")]
        [SerializeField] private Image dropZoneImage;
        [SerializeField] private TextMeshProUGUI instructionText;
        [SerializeField] private Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        [SerializeField] private Color highlightColor = new Color(0.3f, 0.6f, 1f, 0.7f);
        [SerializeField] private Color acceptColor = new Color(0.3f, 1f, 0.3f, 0.7f);

        [Header("3D World Settings")]
        [SerializeField] private bool use3DSpawning = false;
        [SerializeField] private float spawnDistance = 0.5f;

        // Events
        public delegate void AtomSpawnedEvent(GrabbableAtom atom, AtomData data);
        public event AtomSpawnedEvent OnAtomSpawned;

        public delegate void AtomModifiedEvent(GrabbableAtom atom, AtomData oldData, AtomData newData);
        public event AtomModifiedEvent OnAtomModified;

        private GrabbableAtom lastSpawnedAtom;
        private bool isHighlighted;

        private void Awake()
        {
            if (dropZoneImage == null) dropZoneImage = GetComponent<Image>();
            if (spawnPoint == null) spawnPoint = transform;

            SetColor(normalColor);
        }

        private void OnEnable()
        {
            ElementDragHandler.OnDragStarted += OnElementDragStarted;
            ElementDragHandler.OnDragEnded += OnElementDragEnded;
        }

        private void OnDisable()
        {
            ElementDragHandler.OnDragStarted -= OnElementDragStarted;
            ElementDragHandler.OnDragEnded -= OnElementDragEnded;
        }

        private void OnElementDragStarted(ElementDragHandler handler)
        {
            // Show highlight when dragging starts
            SetColor(highlightColor);
            if (instructionText != null)
            {
                instructionText.text = "Drop here";
            }
        }

        private void OnElementDragEnded(ElementDragHandler handler)
        {
            // Reset visual when dragging ends
            SetColor(normalColor);
            if (instructionText != null)
            {
                instructionText.text = "Drag element here";
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (ElementDragHandler.CurrentDragging != null)
            {
                isHighlighted = true;
                SetColor(acceptColor);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (isHighlighted)
            {
                isHighlighted = false;
                if (ElementDragHandler.CurrentDragging != null)
                {
                    SetColor(highlightColor);
                }
                else
                {
                    SetColor(normalColor);
                }
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            var dragHandler = ElementDragHandler.CurrentDragging;
            if (dragHandler == null || dragHandler.atomData == null)
            {
                Debug.LogWarning("AtomDropZone: No valid element data in dropped item.");
                return;
            }

            AtomData elementData = dragHandler.atomData;

            // Check if we should modify an existing atom or spawn a new one
            if (targetAtom != null)
            {
                ModifyAtom(targetAtom, elementData);
            }
            else if (spawnOnDrop)
            {
                SpawnAtom(elementData);
            }

            // Visual feedback
            SetColor(acceptColor);
            Invoke(nameof(ResetColor), 0.3f);
        }

        /// <summary>
        /// Spawns a new atom with the given element data.
        /// </summary>
        public GrabbableAtom SpawnAtom(AtomData elementData)
        {
            if (atomPrefab == null)
            {
                Debug.LogError("AtomDropZone: Atom prefab not assigned!");
                return null;
            }

            Vector3 spawnPos = CalculateSpawnPosition();
            Quaternion spawnRot = Quaternion.identity;

            // Instantiate the atom
            GameObject newAtomObj = Instantiate(atomPrefab, spawnPos, spawnRot);
            newAtomObj.name = $"Atom_{elementData.symbol}";

            // Initialize with element data
            GrabbableAtom grabbableAtom = newAtomObj.GetComponent<GrabbableAtom>();
            if (grabbableAtom != null)
            {
                grabbableAtom.Initialize(elementData);
            }

            lastSpawnedAtom = grabbableAtom;

            // Try to make it grabbable immediately in VR
            TryAutoGrab(newAtomObj);

            // Notify listeners
            OnAtomSpawned?.Invoke(grabbableAtom, elementData);

            Debug.Log($"AtomDropZone: Spawned {elementData.elementName} atom at {spawnPos}");
            return grabbableAtom;
        }

        /// <summary>
        /// Modifies an existing atom to become a different element.
        /// </summary>
        public void ModifyAtom(GrabbableAtom atom, AtomData newElementData)
        {
            if (atom == null)
            {
                Debug.LogWarning("AtomDropZone: Cannot modify null atom.");
                return;
            }

            AtomData oldData = atom.atomData;
            atom.Initialize(newElementData);
            atom.gameObject.name = $"Atom_{newElementData.symbol}";

            // Notify listeners
            OnAtomModified?.Invoke(atom, oldData, newElementData);

            Debug.Log($"AtomDropZone: Modified atom from {oldData?.symbol ?? "null"} to {newElementData.symbol}");
        }

        /// <summary>
        /// Sets the target atom that will be modified when elements are dropped.
        /// </summary>
        public void SetTargetAtom(GrabbableAtom atom)
        {
            targetAtom = atom;
        }

        /// <summary>
        /// Gets the last spawned atom.
        /// </summary>
        public GrabbableAtom GetLastSpawnedAtom()
        {
            return lastSpawnedAtom;
        }

        private Vector3 CalculateSpawnPosition()
        {
            if (use3DSpawning)
            {
                // For 3D world space, spawn in front of the camera or at spawn point
                if (Camera.main != null)
                {
                    return Camera.main.transform.position + Camera.main.transform.forward * spawnDistance;
                }
            }

            // For UI or default, use the spawn point position
            return spawnPoint.position;
        }

        private void TryAutoGrab(GameObject atomObj)
        {
            // Try to find an XR interactor to grab the atom automatically
            var interactable = atomObj.GetComponent<XRGrabInteractable>();
            if (interactable == null) return;

            var manager = FindObjectOfType<XRInteractionManager>();
            if (manager == null) return;

            // Try to find an available direct interactor
            var directInteractors = FindObjectsOfType<XRDirectInteractor>();
            foreach (var interactor in directInteractors)
            {
                if (!interactor.hasSelection)
                {
                    // This interactor is free, try to grab
                    // Note: This might not work in all cases, as the hand may not be near the atom
                    // The user would need to grab it manually
                    break;
                }
            }
        }

        private void SetColor(Color color)
        {
            if (dropZoneImage != null)
            {
                dropZoneImage.color = color;
            }
        }

        private void ResetColor()
        {
            SetColor(normalColor);
        }

        // For 3D collision-based drops (optional)
        private void OnTriggerEnter(Collider other)
        {
            // Handle 3D world drop if needed
            var dragHandler = other.GetComponent<ElementDragHandler>();
            if (dragHandler != null && dragHandler.atomData != null)
            {
                if (targetAtom != null)
                {
                    ModifyAtom(targetAtom, dragHandler.atomData);
                }
                else if (spawnOnDrop)
                {
                    SpawnAtom(dragHandler.atomData);
                }
            }
        }
    }
}
