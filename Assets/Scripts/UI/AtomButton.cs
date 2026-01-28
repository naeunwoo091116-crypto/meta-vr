using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using AtomSim.Data;
using AtomSim.Atoms;

namespace AtomSim.UI
{
    /// <summary>
    /// Component for the Periodic Table button.
    /// Spawns a GrabbableAtom into the user's hand when interacted with.
    /// </summary>
    public class AtomButton : MonoBehaviour
    {
        [Header("Data")]
        public AtomData AtomData;

        [Header("References")]
        [SerializeField] private TextMeshPro symbolText;
        [SerializeField] private TMPro.TextMeshProUGUI symbolTextUI; 
        [SerializeField] private Renderer buttonRenderer;
        [SerializeField] private UnityEngine.UI.Image buttonImage;
        
        [Header("Spawning")]
        [SerializeField] private GameObject atomPrefab; // The GrabbableAtom prefab to spawn

        private IXRSelectInteractor grabbingHand;
        private XRBaseInteractable interactable; // Changed to Base for compatibility
        private UnityEngine.UI.Button uiButton;

        private void Awake()
        {
            interactable = GetComponent<XRBaseInteractable>();
            uiButton = GetComponent<UnityEngine.UI.Button>();
        }

        private void OnEnable()
        {
            if (interactable != null)
            {
                interactable.selectEntered.AddListener(OnGrabAttempt);
            }
            if (uiButton != null)
            {
                uiButton.onClick.AddListener(OnUiClick);
            }
        }

        private void OnDisable()
        {
            if (interactable != null)
            {
                interactable.selectEntered.RemoveListener(OnGrabAttempt);
            }
            if (uiButton != null)
            {
                uiButton.onClick.RemoveListener(OnUiClick);
            }
        }

        public void Initialize(AtomData data)
        {
            AtomData = data;
            UpdateVisuals();
        }

        public void UpdateVisuals()
        {
            if (AtomData == null) return;
            if (symbolText != null) symbolText.text = AtomData.symbol;
            if (symbolTextUI != null) symbolTextUI.text = AtomData.symbol;
            if (buttonRenderer != null) buttonRenderer.material.color = AtomData.elementColor;
            if (buttonImage != null) buttonImage.color = AtomData.elementColor;
        }

        // Connect this to the XR Grab/Simple Interactable 'Select Entered' event
        public void OnGrabAttempt(SelectEnterEventArgs args)
        {
            grabbingHand = args.interactorObject;
            SpawnAtom(args.manager);
        }

        private void OnUiClick()
        {
            // For UI clicks, we need to find the active XR Interaction Manager and a hand to grab with.
            // This is trickier than direct VR interaction.
            // For now, we'll try to find the "Right Hand" or "Left Hand" interactor in the scene, 
            // or just spawn it in front of the camera if no hand is readily available/grabbing.
            
            // NOTE: In a robust VR system, you'd track which hand 'clicked' (e.g. via raycast).
            // Here we will try to find a default interactor.
            
            var interactionManager = FindObjectOfType<XRInteractionManager>();
            if (interactionManager == null)
            {
                 Debug.LogError("No XRInteractionManager found.");
                 return;
            }

            // Heuristic: Try to find a Direct Interactor
            var interactors = FindObjectsOfType<XRDirectInteractor>();
            foreach(var interactor in interactors)
            {
                 // Just pick the first one for now, or maybe the one currently selecting the UI?
                 // Since this is a UI button click (likely ray interactor), we might want the ray interactor's hand.
                 grabbingHand = interactor; 
                 // If we find one, we use it.
                 if (grabbingHand != null) break; 
            }
            
            // If still null, try Ray Interactors
            if (grabbingHand == null)
            {
                 var rayInteractors = FindObjectsOfType<XRRayInteractor>();
                 if (rayInteractors.Length > 0) grabbingHand = rayInteractors[0];
            }

            SpawnAtom(interactionManager);
        }

        private void SpawnAtom(XRInteractionManager manager)
        {
            if (atomPrefab == null)
            {
                Debug.LogWarning("AtomButton: Missing Prefab reference.");
                return;
            }

            Vector3 spawnPos = transform.position; // Default to button pos
            Quaternion spawnRot = Quaternion.identity;

            if (grabbingHand != null)
            {
                spawnPos = grabbingHand.transform.position;
            }
            else
            {
                // Fallback: Spawn in front of camera
                if (Camera.main != null)
                {
                   spawnPos = Camera.main.transform.position + Camera.main.transform.forward * 0.5f;
                }
            }
 
            // 1. Instantiate 
            GameObject newAtom = Instantiate(atomPrefab, spawnPos, spawnRot);
            
            // 2. Initialize Atom Data
            var grabbableAtom = newAtom.GetComponent<GrabbableAtom>();
            if (grabbableAtom != null)
            {
                grabbableAtom.Initialize(AtomData);
            }

            // 3. Force Grab Transfer
            if (grabbingHand != null)
            {
                var atomInteractable = newAtom.GetComponent<XRGrabInteractable>();
                if (atomInteractable != null)
                {
                    // Force the hand to grab the new object
                    // We might need to select exit whatever it's currently holding (like the UI ray?)
                    // manager.SelectExit(grabbingHand, grabbingHand.firstInteractableSelected); 
                    
                    // Simple attempt:
                    manager.SelectEnter(grabbingHand, atomInteractable);
                }
            }
        }
    }
}
