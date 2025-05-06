using UnityEngine;
using UnityEngine.UI;

public class SexSelectionPanel : MonoBehaviour
{
    public static SexSelectionPanel Instance { get; private set; }

    [Header("UI Elements")]
    public GameObject panel;         // À désactiver par défaut
    public Button maleButton;
    public Button femaleButton;

    private UnitCardUI targetCard;

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        Instance = this;
        panel.SetActive(false);
        maleButton.onClick.AddListener(() => ApplySex(Sex.Male));
        femaleButton.onClick.AddListener(() => ApplySex(Sex.Female));
    }

    public void Show(UnitCardUI cardUI)
    {
        targetCard = cardUI;
        panel.transform.position = Input.mousePosition;
        panel.SetActive(true);
    }

    private void ApplySex(Sex s)
    {
        targetCard.unitData.sex = s;
        panel.SetActive(false);
    }
}

