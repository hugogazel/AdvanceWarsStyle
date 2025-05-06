using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UnitCardUI : MonoBehaviour
{
    [Header("References to UI Elements")]
    public Image cardBackground;      // Image d'arrière-plan du panneau de la carte
    public Image unitSpriteImage;     // Image pour afficher le sprite de l'unité
    public TextMeshProUGUI biomassText; // Texte pour la biomasse

    [Header("Sex Icon (new)")]
    public Image sexIcon;             // Icône de sexe (à assigner en Inspector)
    public Sprite maleSprite;         // Sprite ♂
    public Sprite femaleSprite;       // Sprite ♀

    [Header("Unit Data")]
    public UnitData unitData;         // Données de l'unité
    public GameObject unitPrefab;     // Prefab de l'unité (pour déploiement)

    // Références internes
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();

        // Forcer l'activation de tous les composants UI
        ForceActivateUI();
        Debug.Log("[UnitCardUI] Awake on " + gameObject.name);
    }

    private void Start()
    {
        // Vérifie encore après un frame au cas où
        StartCoroutine(ForceActivationAfterFrame());
    }

    private void ForceActivateUI()
    {
        // Parent Image
        var parentImage = GetComponent<Image>();
        if (parentImage != null)
        {
            parentImage.gameObject.SetActive(true);
            parentImage.enabled = true;
        }
        // Background, Sprite, Text
        if (cardBackground != null) { cardBackground.gameObject.SetActive(true); cardBackground.enabled = true; }
        if (unitSpriteImage != null) { unitSpriteImage.gameObject.SetActive(true); unitSpriteImage.enabled = true; }
        if (biomassText != null) { biomassText.gameObject.SetActive(true); biomassText.enabled = true; }
        // Sex icon
        if (sexIcon != null) { sexIcon.gameObject.SetActive(true); sexIcon.enabled = true; }
    }

    private IEnumerator ForceActivationAfterFrame()
    {
        yield return null;
        ForceActivateUI();
        Debug.Log($"[ForceActivationAfterFrame] UI forcée sur {gameObject.name}");
    }

    /// <summary>
    /// Initialise l'affichage de la carte.
    /// </summary>
    public void Initialize(UnitData data)
    {
        unitData = data;
        unitPrefab = data.unitPrefab;

        // Sprite
        if (unitSpriteImage != null && data.unitSprite != null)
        {
            unitSpriteImage.sprite = data.unitSprite;
        }
        // Biomasse
        if (biomassText != null)
        {
            biomassText.text = data.biomass.ToString();
        }
        // Icône de sexe
        UpdateSexIcon();
    }

    /// <summary>
    /// Met à jour l'icône de sexe d'après unitData.sex
    /// </summary>
    public void UpdateSexIcon()
    {
        if (sexIcon == null) return;

        switch (unitData.sex)
        {
            case Sex.Male:
                sexIcon.sprite = maleSprite;
                sexIcon.gameObject.SetActive(true);
                break;
            case Sex.Female:
                sexIcon.sprite = femaleSprite;
                sexIcon.gameObject.SetActive(true);
                break;
            default:
                sexIcon.gameObject.SetActive(false);
                break;
        }
    }
}









