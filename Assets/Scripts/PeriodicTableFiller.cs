using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// PeriodicTablePanel의 Image를 복제해서 주기율표를 채웁니다.
/// PeriodicTablePanel에 이 스크립트를 붙이세요.
/// </summary>
public class PeriodicTableFiller : MonoBehaviour
{
    [Header("원본 이미지 (자식의 Image)")]
    public GameObject templateImage;

    [Header("Atom 프리팹")]
    public GameObject atomPrefab;

    [Header("생성할 원소 개수")]
    public int elementCount = 36;

    // 원소 데이터: 기호, 색상
    private static readonly string[] symbols = {
        "H", "He",
        "Li", "Be", "B", "C", "N", "O", "F", "Ne",
        "Na", "Mg", "Al", "Si", "P", "S", "Cl", "Ar",
        "K", "Ca", "Sc", "Ti", "V", "Cr", "Mn", "Fe", "Co", "Ni", "Cu", "Zn", "Ga", "Ge", "As", "Se", "Br", "Kr"
    };

    private static readonly Color[] colors = {
        // Period 1
        new Color(0.4f, 0.6f, 1f),      // H - 비금속
        new Color(0.9f, 0.5f, 0.9f),    // He - 비활성
        // Period 2
        new Color(1f, 0.4f, 0.4f),      // Li - 알칼리
        new Color(1f, 0.6f, 0.3f),      // Be - 알칼리토
        new Color(0.4f, 0.8f, 0.8f),    // B - 준금속
        new Color(0.4f, 0.6f, 1f),      // C
        new Color(0.4f, 0.6f, 1f),      // N
        new Color(0.4f, 0.6f, 1f),      // O
        new Color(0.8f, 0.4f, 1f),      // F - 할로겐
        new Color(0.9f, 0.5f, 0.9f),    // Ne
        // Period 3
        new Color(1f, 0.4f, 0.4f),      // Na
        new Color(1f, 0.6f, 0.3f),      // Mg
        new Color(0.6f, 0.8f, 0.6f),    // Al - 전이후금속
        new Color(0.4f, 0.8f, 0.8f),    // Si
        new Color(0.4f, 0.6f, 1f),      // P
        new Color(0.4f, 0.6f, 1f),      // S
        new Color(0.8f, 0.4f, 1f),      // Cl
        new Color(0.9f, 0.5f, 0.9f),    // Ar
        // Period 4
        new Color(1f, 0.4f, 0.4f),      // K
        new Color(1f, 0.6f, 0.3f),      // Ca
        new Color(1f, 0.8f, 0.4f),      // Sc - 전이금속
        new Color(1f, 0.8f, 0.4f),      // Ti
        new Color(1f, 0.8f, 0.4f),      // V
        new Color(1f, 0.8f, 0.4f),      // Cr
        new Color(1f, 0.8f, 0.4f),      // Mn
        new Color(1f, 0.8f, 0.4f),      // Fe
        new Color(1f, 0.8f, 0.4f),      // Co
        new Color(1f, 0.8f, 0.4f),      // Ni
        new Color(1f, 0.8f, 0.4f),      // Cu
        new Color(1f, 0.8f, 0.4f),      // Zn
        new Color(0.6f, 0.8f, 0.6f),    // Ga
        new Color(0.4f, 0.8f, 0.8f),    // Ge
        new Color(0.4f, 0.8f, 0.8f),    // As
        new Color(0.4f, 0.6f, 1f),      // Se
        new Color(0.8f, 0.4f, 1f),      // Br
        new Color(0.9f, 0.5f, 0.9f),    // Kr
    };

    void Start()
    {
        GenerateTable();
    }

    [ContextMenu("Generate Periodic Table")]
    public void GenerateTable()
    {
        if (templateImage == null)
        {
            // 자식에서 Image 찾기
            templateImage = transform.Find("Image")?.gameObject;
            if (templateImage == null)
            {
                Debug.LogError("PeriodicTableFiller: templateImage(원본 Image)를 찾을 수 없습니다!");
                return;
            }
        }

        // Atom 프리팹 자동 찾기
        if (atomPrefab == null)
        {
            var dispenser = templateImage.GetComponent<AtomPullDispenser>();
            if (dispenser != null)
            {
                atomPrefab = dispenser.atomPrefab;
            }
        }

        // 기존 원소들 삭제 (원본 Image 제외)
        ClearElements();

        int count = Mathf.Min(elementCount, symbols.Length);

        for (int i = 0; i < count; i++)
        {
            CreateElement(i, symbols[i], colors[i]);
        }

        // 원본 템플릿 비활성화
        templateImage.SetActive(false);

        Debug.Log($"PeriodicTableFiller: {count}개 원소 생성 완료");
    }

    private void CreateElement(int index, string symbol, Color color)
    {
        // 템플릿 복제
        GameObject element = Instantiate(templateImage, transform);
        element.name = $"Element_{symbol}";
        element.SetActive(true);

        // 색상 설정
        var image = element.GetComponent<Image>();
        if (image != null)
        {
            image.color = color;
        }

        // AtomPullDispenser 설정
        var dispenser = element.GetComponent<AtomPullDispenser>();
        if (dispenser != null)
        {
            dispenser.elementSymbol = symbol;
            dispenser.atomColor = color;
            if (atomPrefab != null)
            {
                dispenser.atomPrefab = atomPrefab;
            }
        }

        // 텍스트 추가 (없으면 생성)
        var text = element.GetComponentInChildren<TextMeshProUGUI>();
        if (text == null)
        {
            // 텍스트 오브젝트 생성
            GameObject textObj = new GameObject("SymbolText");
            textObj.transform.SetParent(element.transform, false);

            var rectTransform = textObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            text = textObj.AddComponent<TextMeshProUGUI>();
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.fontStyle = FontStyles.Bold;
            text.enableAutoSizing = true;
            text.fontSizeMin = 1;
            text.fontSizeMax = 100;
        }

        text.text = symbol;
    }

    private void ClearElements()
    {
        // 원본 Image 외의 모든 자식 삭제
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            if (child.gameObject != templateImage)
            {
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }
        }
    }
}
