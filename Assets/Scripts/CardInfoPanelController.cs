using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardInfoPanelController : MonoBehaviour
{
    public static CardInfoPanelController Instance { get; private set; }

    [Header("Content Panel (désactivé au départ)")]
    public GameObject content;

    [Header("Textes")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI biomassText;
    public TextMeshProUGUI movePointsText;
    public TextMeshProUGUI dietText;

    [Header("Artwork")]
    public Image artworkImage;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Cache le content tant qu'aucune carte n'est cliquée
        if (content != null)
            content.SetActive(false);
    }

    /// <summary>
    /// Met à jour le panel avec les données de l’unité,
    /// l’active s’il était masqué.
    /// </summary>
    public void UpdateInfo(UnitData data)
    {
        if (data == null || content == null) return;

        // Active pour la première fois
        if (!content.activeSelf)
            content.SetActive(true);

        // Textes
        nameText.text = data.unitName;
        biomassText.text = $"Biomass : {data.biomass}";
        movePointsText.text = $"Move : {data.maxMovePoints}";
        if (data.isPredator) dietText.text = "Diet : Predator";
        else if (data.isHerbivore) dietText.text = "Diet : Herbivore";
        else if (data.isOmnivore) dietText.text = "Diet : Omnivore";
        else dietText.text = "Diet : None";

        // Artwork
        if (artworkImage != null)
        {
            artworkImage.sprite = data.artwork;
            artworkImage.enabled = data.artwork != null;
        }
    }
}



