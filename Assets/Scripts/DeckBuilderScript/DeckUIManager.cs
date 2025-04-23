using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections.Generic;

public class DeckUIManager : MonoBehaviour
{
    public static DeckUIManager Instance { get; private set; }

    [Header("Prefabs & Containers")]
    public GameObject cardPrefab;
    public Transform availableCardsContainer;
    public Transform deckPanelContainer;

    [Header("Deck Data")]
    public List<UnitData> allUnitDatas;             // Toutes les unités dispo
    public List<UnitData> selectedDeckCards = new List<UnitData>();

    [Header("Deck Config")]
    public int maxCardsInDeck = 10;
    public float maxTotalBiomass = 10f;

    [Header("UI")]
    public Button startGameButton;

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // On retire la persistence pour que les références aux containers soient remises à jour
        // DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        // Si on recharge la scène pour le joueur 2, on vide l'ancienne sélection
        if (GameManager.Instance.deckBuilderPlayer == 2)
            selectedDeckCards.Clear();
    }

    private void Start()
    {
        // Peupler la liste des cartes disponibles
        PopulateAvailableCards();

        // Vider l'affichage du deck (pour ne pas voir les cartes de J1)
        ClearDeckPanel();

        // Désactiver le bouton tant que le deck n'est pas plein
        if (startGameButton != null)
            startGameButton.interactable = false;
    }

    void ClearDeckPanel()
    {
        foreach (Transform child in deckPanelContainer)
            Destroy(child.gameObject);
    }

    public void PopulateAvailableCards()
    {
        foreach (Transform child in availableCardsContainer)
            Destroy(child.gameObject);

        foreach (UnitData data in allUnitDatas)
        {
            var cardObj = Instantiate(cardPrefab, availableCardsContainer);
            cardObj.GetComponent<UnitCardUI>().Initialize(data);
        }
    }

    public void AddCardToDeck(UnitData data)
    {
        float currentBiomass = selectedDeckCards.Sum(d => d.biomass);

        if (currentBiomass + data.biomass > maxTotalBiomass) return;
        if (selectedDeckCards.Count >= maxCardsInDeck) return;

        selectedDeckCards.Add(data);
        var cardObj = Instantiate(cardPrefab, deckPanelContainer);
        cardObj.GetComponent<UnitCardUI>().Initialize(data);
        cardObj.AddComponent<DeckCardRemover>();

        currentBiomass += data.biomass;
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
            float currentBiomass = selectedDeckCards.Sum(d => d.biomass);
            if (selectedDeckCards.Count < maxCardsInDeck &&
                currentBiomass < maxTotalBiomass &&
                startGameButton != null)
            {
                startGameButton.interactable = false;
            }
        }
    }

    /// <summary>
    /// Appelée par le bouton Start quand le deck est plein.
    /// Envoie la liste à GameManager qui gère J1→J2→lancement de la partie.
    /// </summary>
    public void StartGame()
    {
        float currentBiomass = selectedDeckCards.Sum(d => d.biomass);

        if (selectedDeckCards.Count >= maxCardsInDeck ||
            Mathf.Approximately(currentBiomass, maxTotalBiomass))
        {
            GameManager.Instance.SubmitDeck(new List<UnitData>(selectedDeckCards));
        }
    }
}














