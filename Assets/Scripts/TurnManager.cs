using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    [Header("Notification de tour")]
    [Tooltip("Simple TextMeshProUGUI pour 'J1 Turn' / 'J2 Turn'")]
    public TextMeshProUGUI playerTurnText;
    [Tooltip("Durée d'affichage du texte en secondes")]
    public float playerTurnDisplayTime = 2f;

    [Header("Panel de fin de tour")]
    public GameObject turnEndPanel;

    [Header("Tour courant")]
    public Team currentTeam = Team.J1Team;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    private void Start()
    {
        // Charger les decks
        player1Deck = GameManager.Instance.player1Deck;
        player2Deck = GameManager.Instance.player2Deck;

        // Équipe de départ
        currentTeam = (GameManager.Instance.currentPlayerTurn == 1)
            ? Team.J1Team
            : Team.J2Team;

        // Affichage initial
        ShowPlayerHand();
        ShowPlayerTurnText();
    }

    private void ShowPlayerHand()
    {
        foreach (Transform c in unitCardPanel)
            Destroy(c.gameObject);

        var deck = (currentTeam == Team.J1Team) ? player1Deck : player2Deck;
        foreach (var data in deck)
        {
            var go = Instantiate(cardPrefab, unitCardPanel);
            go.GetComponent<UnitCardUI>()?.Initialize(data);
        }
    }

    public void ShowPlayerTurnText()
    {
        if (playerTurnText == null) return;

        playerTurnText.text = (currentTeam == Team.J1Team)
            ? "J1 Turn"
            : "J2 Turn";
        playerTurnText.gameObject.SetActive(true);
        StartCoroutine(HidePlayerTurnTextAfterDelay());
    }

    private IEnumerator HidePlayerTurnTextAfterDelay()
    {
        yield return new WaitForSeconds(playerTurnDisplayTime);
        playerTurnText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Ouvre le panel de fin de tour (sans changer d’équipe).
    /// </summary>
    public void ShowEndTurnPanelOnly()
    {
        if (turnEndPanel != null)
            turnEndPanel.SetActive(true);
    }

    /// <summary>
    /// Appelé par le bouton "Return" du TurnEndPanel.
    /// </summary>
    public void OnCancelEndTurn()
    {
        // Ne change pas d'équipe, ferme juste le panel
        turnEndPanel.SetActive(false);
    }

    /// <summary>
    /// Appelé par le bouton "End Turn" du TurnEndPanel.
    /// Change d’équipe, met à jour l’UI, puis ferme le panel.
    /// </summary>
    public void OnConfirmEndTurn()
    {
        // Change d’équipe dans GameManager
        GameManager.Instance.EndTurn();

        // Met à jour localement
        currentTeam = (GameManager.Instance.currentPlayerTurn == 1)
            ? Team.J1Team
            : Team.J2Team;

        // Rafraîchit l’UI
        ShowPlayerHand();
        ShowPlayerTurnText();

        turnEndPanel.SetActive(false);
    }
}
















