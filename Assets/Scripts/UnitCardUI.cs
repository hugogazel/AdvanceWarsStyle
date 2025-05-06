using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UnitCardUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Image cardBackground;        // Le slot brun sur lequel on change la couleur
    public Image unitSpriteImage;       // L’image de l’unité
    public TextMeshProUGUI biomassText; // Le texte de biomasse

    [Header("Sex Colors")]
    public Color maleColor = new Color(0.4f, 0.6f, 1f, 1f);
    public Color femaleColor = new Color(1f, 0.5f, 0.7f, 1f);

    [HideInInspector] public UnitData unitData;     // Les données clonées ou non
    [HideInInspector] public GameObject unitPrefab; // Le prefab à instancier en jeu

    private Color _originalColor;

    private void Awake()
    {
        // Sauvegarde de la couleur d'origine du slot
        if (cardBackground != null)
            _originalColor = cardBackground.color;

        // Force l'activation immédiate des composants UI
        var parentImage = GetComponent<Image>();
        if (parentImage != null) parentImage.enabled = true;
        if (cardBackground != null) cardBackground.enabled = true;
        if (unitSpriteImage != null) unitSpriteImage.enabled = true;
        if (biomassText != null) biomassText.enabled = true;

        // si tu n'as pas fait drag&drop en Inspector, on récupère l'Image du root
        if (cardBackground == null)
            cardBackground = GetComponent<Image>();

        // stocke la couleur d'origine
        if (cardBackground != null)
            _originalColor = cardBackground.color;
    }

    private IEnumerator Start()
    {
        yield return null;

        // À nouveau, on ré-obtient l'Image du parent pour la ré-activer après un frame
        var parentImage = GetComponent<Image>();
        if (parentImage != null) parentImage.enabled = true;
        if (cardBackground != null) cardBackground.enabled = true;
        if (unitSpriteImage != null) unitSpriteImage.enabled = true;
        if (biomassText != null) biomassText.enabled = true;
    }

    /// <summary>
    /// Initialise la carte (fond remis à la couleur d’origine).
    /// </summary>
    public void Initialize(UnitData data)
    {
        unitData = data;
        unitPrefab = data.unitPrefab;

        if (unitSpriteImage != null && data.unitSprite != null)
            unitSpriteImage.sprite = data.unitSprite;

        if (biomassText != null)
            biomassText.text = data.biomass.ToString();

        if (cardBackground != null)
            cardBackground.color = _originalColor;
    }

    /// <summary>
    /// Appelée APRÈS sélection du sexe : applique bleu ou rose.
    /// </summary>
    public void ApplySexColor()
    {
        if (cardBackground == null || unitData == null)
            return;

        cardBackground.color =
            (unitData.sex == Sex.Male) ? maleColor : femaleColor;
    }
}














