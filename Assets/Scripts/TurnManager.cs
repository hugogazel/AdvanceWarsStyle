using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    [Header("Decks (GameManager)")]
    public List<UnitData> player1Deck;
    public List<UnitData> player2Deck;

    [Header("Hand UI")]
    public Transform unitCardPanel;
    public GameObject cardPrefab;

    [Header("Relais Tour")]
    public GameObject turnChangePanel;      // Le panel couvrant la map au début du tour
    public Button readyButton;              // Bouton "I'm ready"
    public TextMeshProUGUI playerTurnText;  // Le texte "J1 Turn" / "J2 Turn"

    [Header("Panel de fin de tour")]
    public GameObject turnEndPanel;         // Le panel de confirmation de fin de tour

    [Header("Tour courant")]
    public Team currentTeam = Team.J1Team;

    [HideInInspector] public bool hasDeployedThisTurn = false;
    [HideInInspector] public bool j1FreeDropUsed = false;
    [HideInInspector] public bool j2FreeDropUsed = false;

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        j1FreeDropUsed = false;
        j2FreeDropUsed = false;
    }

    private void Start()
    {
        // Récupère les decks
        player1Deck = GameManager.Instance.player1Deck;
        player2Deck = GameManager.Instance.player2Deck;

        // Abonne le bouton "I'm ready"
        readyButton.onClick.AddListener(OnReadyClicked);

        // Démarre le premier tour
        StartTurn();
    }

    /// <summary>
    /// Initialise un tour : affiche le panel de relais, bloque l'input et prépare l'UI.
    /// </summary>
    private void StartTurn()
    {
        // Détermine l'équipe courante
        currentTeam = (GameManager.Instance.currentPlayerTurn == 1)
            ? Team.J1Team
            : Team.J2Team;

        // Affiche la main
        ShowPlayerHand();

        // Reset des unités
        ResetUnitsForCurrentTeam();

        // Prépare le texte
        playerTurnText.text = (currentTeam == Team.J1Team) ? "J1 Turn" : "J2 Turn";

        // Affiche le panel de relais
        turnChangePanel.SetActive(true);

        // Bloque tout input de jeu
        GameManager.inputLocked = true;

        // Marque qu'on n'a pas encore déployé
        hasDeployedThisTurn = false;
    }

    /// <summary>
    /// Appelé quand le joueur clique sur "I'm ready" : ferme le relais et débloque l'input.
    /// </summary>
    private void OnReadyClicked()
    {
        turnChangePanel.SetActive(false);
        GameManager.inputLocked = false;
    }

    private void ShowPlayerHand()
    {
        foreach (Transform c in unitCardPanel)
            Destroy(c.gameObject);

        var deck = (currentTeam == Team.J1Team) ? player1Deck : player2Deck;
        foreach (var data in deck)
            Instantiate(cardPrefab, unitCardPanel).GetComponent<UnitCardUI>().Initialize(data);
    }

    public void ShowEndTurnPanelOnly()
    {
        turnEndPanel?.SetActive(true);
    }

    public void OnCancelEndTurn()
    {
        turnEndPanel?.SetActive(false);
    }

    public void OnConfirmEndTurn()
    {
        // Change le tour dans le GameManager
        GameManager.Instance.EndTurn();

        // Relance un nouveau tour
        StartTurn();

        // Masque le panel de fin de tour
        turnEndPanel?.SetActive(false);
    }

    private void ResetUnitsForCurrentTeam()
    {
        foreach (var unit in FindObjectsOfType<UnitController>())
            if (unit.team == currentTeam)
                unit.ResetVisuals();
    }
}


















