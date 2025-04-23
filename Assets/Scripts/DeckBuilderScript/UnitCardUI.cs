using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UnitCardUI : MonoBehaviour
{
    [Header("References to UI Elements")]
    public Image cardBackground;      // Image d'arrière-plan du panneau de la carte
    public Image unitSpriteImage;     // Image pour afficher le sprite de l'unité (généralement sur un enfant)
    public TextMeshProUGUI biomassText; // Texte pour la biomasse

    [Header("Unit Data")]
    public UnitData unitData;         // Données de l'unité
    public GameObject unitPrefab;     // Prefab de l'unité (pour deployment)

    // Références internes
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;

    private void Awake()
    {
        // Récupération des composants attachés à cet objet.
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            // S'il n'existe pas, on l'ajoute pour la gestion de l'alpha si nécessaire.
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        canvas = GetComponentInParent<Canvas>();

        // Forcer l'activation du GameObject parent (celui-ci) si besoin
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        // Forcer l'activation du composant Image du parent (celui qui est sur le GameObject racine)
        // On utilise GetComponent<Image>() ici pour récupérer le composant Image attaché directement au prefab.
        Image parentImage = GetComponent<Image>();
        if (parentImage != null)
        {
            if (!parentImage.gameObject.activeSelf)
                parentImage.gameObject.SetActive(true);
            parentImage.enabled = true;
        }

        // Forcer l'activation des autres composants UI définis dans l'inspecteur.
        if (cardBackground != null)
        {
            if (!cardBackground.gameObject.activeSelf)
                cardBackground.gameObject.SetActive(true);
            cardBackground.enabled = true;
        }
        if (unitSpriteImage != null)
        {
            if (!unitSpriteImage.gameObject.activeSelf)
                unitSpriteImage.gameObject.SetActive(true);
            unitSpriteImage.enabled = true;
        }
        if (biomassText != null)
        {
            if (!biomassText.gameObject.activeSelf)
                biomassText.gameObject.SetActive(true);
            biomassText.enabled = true;
        }
        Debug.Log("[UnitCardUI] Awake called sur " + gameObject.name +
          " avec Image = " + (GetComponent<Image>() != null) +
          " et CanvasGroup = " + (GetComponent<CanvasGroup>() != null));
    }

    private void Start()
    {
        // Optionnel : on lance une coroutine pour vérifier que tous les composants restent activés après
        StartCoroutine(ForceActivationAfterFrame());
    }

    private IEnumerator ForceActivationAfterFrame()
    {
        // Attendre la fin de la frame pour vérifier l'état
        yield return null;

        // Forcer à nouveau l'activation (au cas où quelque chose désactiverait les composants après Awake())
        Image parentImage = GetComponent<Image>();
        if (parentImage != null)
        {
            if (!parentImage.gameObject.activeSelf)
                parentImage.gameObject.SetActive(true);
            parentImage.enabled = true;
            Debug.Log($"[ForceActivationAfterFrame] Parent Image activé sur {gameObject.name}");
        }

        if (cardBackground != null)
        {
            if (!cardBackground.gameObject.activeSelf)
                cardBackground.gameObject.SetActive(true);
            cardBackground.enabled = true;
            Debug.Log($"[ForceActivationAfterFrame] CardBackground activé sur {gameObject.name}");
        }
        if (unitSpriteImage != null)
        {
            if (!unitSpriteImage.gameObject.activeSelf)
                unitSpriteImage.gameObject.SetActive(true);
            unitSpriteImage.enabled = true;
            Debug.Log($"[ForceActivationAfterFrame] UnitSpriteImage activé sur {gameObject.name}");
        }
        if (biomassText != null)
        {
            if (!biomassText.gameObject.activeSelf)
                biomassText.gameObject.SetActive(true);
            biomassText.enabled = true;
            Debug.Log($"[ForceActivationAfterFrame] BiomassText activé sur {gameObject.name}");
        }
    }

    // Cette méthode sert uniquement à initialiser l'affichage de la carte.
    public void Initialize(UnitData data)
    {
        Debug.Log($"[UnitCardUI::Initialize] Pour '{gameObject.name}' avec unitData = '{(data != null ? data.unitName : "NULL")}'");

        unitData = data;
        unitPrefab = data.unitPrefab;

        if (unitSpriteImage != null)
        {
            if (data.unitSprite != null)
            {
                unitSpriteImage.sprite = data.unitSprite;
                Debug.Log($"[UnitCardUI::Initialize] Sprite '{data.unitSprite.name}' assigné à '{gameObject.name}', colorAlpha={unitSpriteImage.color.a}");
            }
            else
            {
                Debug.LogWarning($"[UnitCardUI::Initialize] data.unitSprite est null pour {data.unitName} !");
            }
        }
        else
        {
            Debug.LogWarning($"[UnitCardUI::Initialize] unitSpriteImage n'est pas assigné sur '{gameObject.name}'");
        }

        if (biomassText != null)
        {
            biomassText.text = data.biomass.ToString();
        }
    }
}








