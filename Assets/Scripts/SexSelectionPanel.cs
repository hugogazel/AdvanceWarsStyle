using UnityEngine;
using UnityEngine.UI;

public class SexSelectionPanel : MonoBehaviour
{
    public static SexSelectionPanel Instance { get; private set; }

    public GameObject panel;
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
        // 1) Stocke le choix
        targetCard.unitData.sex = s;
        // 2) Recolorie le fond
        targetCard.ApplySexColor();
        // 3) Ferme le panel
        panel.SetActive(false);
    }
}


