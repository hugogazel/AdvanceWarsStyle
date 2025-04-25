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
    public TextMeshProUGUI playerTurnText;
    public float playerTurnDisplayTime = 2f;

    [Header("Panel de fin de tour")]
    public GameObject turnEndPanel;

    [Header("Tour courant")]
    public Team currentTeam = Team.J1Team;

    [HideInInspector]
    public bool hasDeployedThisTurn = false;

    // ← Nouveaux flags pour le free-drop unique
    [HideInInspector] public bool j1FreeDropUsed = false;
    [HideInInspector] public bool j2FreeDropUsed = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        // On démarre avec aucun free-drop consommé
        j1FreeDropUsed = false;
        j2FreeDropUsed = false;
    }

    private void Start()
    {
        player1Deck = GameManager.Instance.player1Deck;
        player2Deck = GameManager.Instance.player2Deck;
        currentTeam = (GameManager.Instance.currentPlayerTurn == 1)
            ? Team.J1Team
            : Team.J2Team;

        ShowPlayerHand();
        ResetUnitsForCurrentTeam();
        ShowPlayerTurnText();

        hasDeployedThisTurn = false;
    }

    private void ShowPlayerHand()
    {
        foreach (Transform c in unitCardPanel)
            Destroy(c.gameObject);

        var deck = (currentTeam == Team.J1Team) ? player1Deck : player2Deck;
        foreach (var data in deck)
            Instantiate(cardPrefab, unitCardPanel).GetComponent<UnitCardUI>().Initialize(data);
    }

    public void ShowPlayerTurnText()
    {
        if (playerTurnText == null) return;
        playerTurnText.text = currentTeam == Team.J1Team ? "J1 Turn" : "J2 Turn";
        playerTurnText.gameObject.SetActive(true);
        StartCoroutine(HidePlayerTurnTextAfterDelay());
    }

    private IEnumerator HidePlayerTurnTextAfterDelay()
    {
        yield return new WaitForSeconds(playerTurnDisplayTime);
        playerTurnText.gameObject.SetActive(false);
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
        GameManager.Instance.EndTurn();
        currentTeam = (GameManager.Instance.currentPlayerTurn == 1)
            ? Team.J1Team
            : Team.J2Team;

        hasDeployedThisTurn = false;
        ResetUnitsForCurrentTeam();
        ShowPlayerHand();
        ShowPlayerTurnText();

        turnEndPanel?.SetActive(false);
    }

    private void ResetUnitsForCurrentTeam()
    {
        foreach (var unit in FindObjectsOfType<UnitController>())
            if (unit.team == currentTeam)
                unit.ResetVisuals();
    }
}

















