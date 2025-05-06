using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections.Generic;
using TMPro;

public class DeckUIManager : MonoBehaviour
{
    public static DeckUIManager Instance { get; private set; }

    [Header("Prefabs & Containers")]
    public GameObject cardPrefab;
    public Transform availableCardsContainer;
    public Transform deckPanelContainer;

    [Header("Deck Data")]
    public List<UnitData> allUnitDatas;
    public List<UnitData> selectedDeckCards = new List<UnitData>();

    [Header("Deck Config")]
    public int maxCardsInDeck = 10;
    public float maxTotalBiomass = 10f;

    [Header("UI")]
    public Button startGameButton;
    public TextMeshProUGUI playerNameText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        Instance = this;
    }

    private void OnEnable()
    {
        if (GameManager.Instance.deckBuilderPlayer == 2)
            selectedDeckCards.Clear();
    }

    private void Start()
    {
        playerNameText.text =
            GameManager.Instance.deckBuilderPlayer == 1 ? "J1" : "J2";

        PopulateAvailableCards();
        ClearDeckPanel();
        startGameButton.interactable = false;
    }

    private void ClearDeckPanel()
    {
        foreach (Transform child in deckPanelContainer)
            Destroy(child.gameObject);
    }

    /// <summary>
    /// Cartes dispo : non colorées (restent brun).
    /// </summary>
    public void PopulateAvailableCards()
    {
        foreach (Transform child in availableCardsContainer)
            Destroy(child.gameObject);

        foreach (UnitData data in allUnitDatas)
        {
            var cardObj = Instantiate(cardPrefab, availableCardsContainer);
            var ui = cardObj.GetComponent<UnitCardUI>();
            ui.Initialize(data);                   // ← un seul paramètre
            cardObj.AddComponent<SexSelectionHandler>();
        }
    }

    /// <summary>
    /// Ajout dans le deck : clone des données, suppression clic gauche,
    /// sélection du sexe clic droit.
    /// </summary>
    public void AddCardToDeck(UnitData originalData)
    {
        float currentBiomass = selectedDeckCards.Sum(d => d.biomass);
        if (currentBiomass + originalData.biomass > maxTotalBiomass) return;
        if (selectedDeckCards.Count >= maxCardsInDeck) return;

        // Clone runtime
        UnitData runtimeData = ScriptableObject.Instantiate(originalData);
        selectedDeckCards.Add(runtimeData);

        var cardObj = Instantiate(cardPrefab, deckPanelContainer);
        var ui = cardObj.GetComponent<UnitCardUI>();
        ui.Initialize(runtimeData);             // ← un seul paramètre

        cardObj.AddComponent<DeckCardRemover>();      // clic gauche
        cardObj.AddComponent<SexSelectionHandler>();  // clic droit

        currentBiomass += runtimeData.biomass;
        if ((selectedDeckCards.Count >= maxCardsInDeck ||
             Mathf.Approximately(currentBiomass, maxTotalBiomass))
            && startGameButton != null)
        {
            startGameButton.interactable = true;
        }
    }

    public void RemoveCardFromDeck(UnitData data)
    {
        if (selectedDeckCards.Remove(data))
        {
            float bm = selectedDeckCards.Sum(d => d.biomass);
            startGameButton.interactable =
                !(selectedDeckCards.Count < maxCardsInDeck && bm < maxTotalBiomass);
        }
    }

    public void StartGame()
    {
        float bm = selectedDeckCards.Sum(d => d.biomass);
        if (selectedDeckCards.Count >= maxCardsInDeck ||
            Mathf.Approximately(bm, maxTotalBiomass))
            GameManager.Instance.SubmitDeck(new List<UnitData>(selectedDeckCards));
    }
}





















