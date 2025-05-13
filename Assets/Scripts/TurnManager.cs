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
        // Récupère les decks depuis le GameManager
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

        // Affiche la main du joueur
        ShowPlayerHand();

        // Reset des unités de cette équipe
        ResetUnitsForCurrentTeam();

        // Prépare le texte "J1 Turn" / "J2 Turn"
        playerTurnText.text = (currentTeam == Team.J1Team) ? "J1 Turn" : "J2 Turn";

        // Affiche le panel de relais et bloque l'input
        turnChangePanel.SetActive(true);
        GameManager.inputLocked = true;

        // Reset du flag de déploiement
        hasDeployedThisTurn = false;
    }

    /// <summary>
    /// Ferme le panel de relais et débloque l'input.
    /// </summary>
    private void OnReadyClicked()
    {
        turnChangePanel.SetActive(false);
        GameManager.inputLocked = false;
    }

    /// <summary>
    /// Affiche dans l'UI toutes les cartes du deck courant,
    /// et applique la couleur correspondant au sexe sélectionné.
    /// </summary>
    private void ShowPlayerHand()
    {
        // 1) Vide le panneau
        foreach (Transform c in unitCardPanel)
            Destroy(c.gameObject);

        // 2) Récupère le deck du joueur actif
        var deck = (currentTeam == Team.J1Team)
            ? player1Deck
            : player2Deck;

        // 3) Pour chaque carte de données, instancie et initialize l'UI
        foreach (var data in deck)
        {
            var ui = Instantiate(cardPrefab, unitCardPanel)
                        .GetComponent<UnitCardUI>();
            ui.Initialize(data);
            // → applique la teinte bleu/rose selon data.sex
            ui.ApplySexColor();
        }
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
        // Termine le tour dans le GameManager
        GameManager.Instance.EndTurn();

        // Relance un nouveau tour
        StartTurn();

        // Masque le panel de fin de tour
        turnEndPanel?.SetActive(false);
    }

    /// <summary>
    /// Réinitialise les unités (visuels, peut-être états) de l'équipe courante.
    /// </summary>
    private void ResetUnitsForCurrentTeam()
    {
        foreach (var unit in FindObjectsOfType<UnitController>())
            if (unit.team == currentTeam)
                unit.ResetVisuals();
    }
}



















