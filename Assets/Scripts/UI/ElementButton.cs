using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

namespace AtomSim.UI
{
    /// <summary>
    /// 원소 버튼 - VR 손으로 잡으면(Grab) 해당 원소의 Atom 프리팹이 생성됩니다.
    /// PeriodicTablePanel 안의 각 원소 칸에 부착합니다.
    /// </summary>
    public class ElementButton : MonoBehaviour
    {
        [Header("원소 정보")]
        public string elementSymbol = "H";
        public string elementName = "Hydrogen";
        public int atomicNumber = 1;
        public Color elementColor = Color.white;

        [Header("참조")]
        [SerializeField] private TextMeshProUGUI symbolText;
        [SerializeField] private TextMeshProUGUI numberText;
        [SerializeField] private UnityEngine.UI.Image backgroundImage;
        [SerializeField] private GameObject atomPrefab;

        [Header("스폰 설정")]
        [SerializeField] private Vector3 spawnOffset = new Vector3(0, 0, 0.3f);

        private XRSimpleInteractable interactable;

        private void Awake()
        {
            // XRSimpleInteractable 자동 추가
            interactable = GetComponent<XRSimpleInteractable>();
            if (interactable == null)
            {
                interactable = gameObject.AddComponent<XRSimpleInteractable>();
            }

            // 컴포넌트 자동 찾기
            if (symbolText == null) symbolText = GetComponentInChildren<TextMeshProUGUI>();
            if (backgroundImage == null) backgroundImage = GetComponent<UnityEngine.UI.Image>();
        }

        private void OnEnable()
        {
            if (interactable != null)
            {
                interactable.selectEntered.AddListener(OnGrabbed);
            }
        }

        private void OnDisable()
        {
            if (interactable != null)
            {
                interactable.selectEntered.RemoveListener(OnGrabbed);
            }
        }

        /// <summary>
        /// 원소 정보 설정
        /// </summary>
        public void Setup(string symbol, string name, int number, Color color)
        {
            elementSymbol = symbol;
            elementName = name;
            atomicNumber = number;
            elementColor = color;

            UpdateVisuals();
        }

        /// <summary>
        /// 비주얼 업데이트
        /// </summary>
        public void UpdateVisuals()
        {
            if (symbolText != null)
            {
                symbolText.text = elementSymbol;
            }

            if (numberText != null)
            {
                numberText.text = atomicNumber.ToString();
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = elementColor;
            }

            gameObject.name = $"Element_{elementSymbol}";
        }

        /// <summary>
        /// VR 손으로 잡았을 때 호출
        /// </summary>
        private void OnGrabbed(SelectEnterEventArgs args)
        {
            SpawnAtom(args.interactorObject);
        }

        /// <summary>
        /// Atom 프리팹 생성
        /// </summary>
        public GameObject SpawnAtom(IXRSelectInteractor interactor = null)
        {
            if (atomPrefab == null)
            {
                Debug.LogError($"ElementButton: atomPrefab이 설정되지 않았습니다! ({elementSymbol})");
                return null;
            }

            // 스폰 위치 계산
            Vector3 spawnPos;
            if (interactor != null)
            {
                // 손 위치에서 약간 앞에 생성
                Transform handTransform = interactor.transform;
                spawnPos = handTransform.position + handTransform.forward * spawnOffset.z;
            }
            else
            {
                // 버튼 앞에 생성
                spawnPos = transform.position + transform.forward * spawnOffset.z;
            }

            // Atom 생성
            GameObject newAtom = Instantiate(atomPrefab, spawnPos, Quaternion.identity);

            // AtomConnector에 원소 기호 설정
            var atomConnector = newAtom.GetComponent<AtomConnector>();
            if (atomConnector != null)
            {
                atomConnector.elementSymbol = elementSymbol;

                // 텍스트 업데이트
                var textDisplay = newAtom.GetComponentInChildren<TextMeshPro>();
                if (textDisplay != null)
                {
                    textDisplay.text = elementSymbol;
                }
            }

            // 생성 후 손에 자동으로 잡히게 하기
            if (interactor != null)
            {
                var grabInteractable = newAtom.GetComponent<XRGrabInteractable>();
                if (grabInteractable != null)
                {
                    var manager = FindObjectOfType<XRInteractionManager>();
                    if (manager != null)
                    {
                        // 약간의 지연 후 잡기 (현재 선택 해제 후)
                        StartCoroutine(DelayedGrab(manager, interactor, grabInteractable));
                    }
                }
            }

            Debug.Log($"Spawned {elementSymbol} atom at {spawnPos}");
            return newAtom;
        }

        private System.Collections.IEnumerator DelayedGrab(XRInteractionManager manager, IXRSelectInteractor interactor, XRGrabInteractable grabInteractable)
        {
            yield return new WaitForEndOfFrame();

            // 손에 자동으로 잡히게 하기
            if (interactor != null && grabInteractable != null && manager != null)
            {
                manager.SelectEnter(interactor, grabInteractable);
            }
        }

        /// <summary>
        /// Atom 프리팹 설정
        /// </summary>
        public void SetAtomPrefab(GameObject prefab)
        {
            atomPrefab = prefab;
        }
    }
}
