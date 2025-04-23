// TurnManager.cs
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    // 1) Singleton
    public static TurnManager Instance { get; private set; }

    [Header("Decks")]
    public List<UnitData> player1Deck;      // À remplir dans l’inspecteur (les UnitData de J1)
    public List<UnitData> player2Deck;      // À remplir dans l’inspecteur (les UnitData de J2)

    [Header("Zones UI")]
    public Transform deckSpawnZoneRed;      // Parent pour les cartes Red (drag & drop dans l’éditeur)
    public Transform deckSpawnZoneBlue;     // Parent pour les cartes Blue

    [Header("Prefabs")]
    public GameObject cardPrefab;           // Votre prefab de carte UI (UnitCardUI)

    [Header("UI Panels")]
    public GameObject turnEndPanel;         // Le panel « Fin de tour » à activer
                                            // (drag & drop depuis la hiérarchie)

    [Header("Tour courant")]
    public Team currentTeam = Team.Red;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // (Optionnel) DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Lance la boucle de jeu (à appeler après que J2 ait validé son deck).
    /// </summary>
    public void StartGameLoop()
    {
        currentTeam = Team.Red;
        PopulateHandFor(currentTeam);
    }

    /// <summary>
    /// Termine le tour et passe à l’autre joueur.
    /// </summary>
    public void EndTurn()
    {
        currentTeam = (currentTeam == Team.Red) ? Team.Blue : Team.Red;
        PopulateHandFor(currentTeam);
    }

    /// <summary>
    /// Affiche le panel « Fin de tour ».
    /// </summary>
    public void ShowTurnEndPanel()
    {
        if (turnEndPanel != null)
            turnEndPanel.SetActive(true);
    }

    /// <summary>
    /// (Ré)instancie les cartes de la main pour l’équipe donnée.
    /// </summary>
    private void PopulateHandFor(Team t)
    {
        var deck = (t == Team.Red) ? player1Deck : player2Deck;
        var zone = (t == Team.Red) ? deckSpawnZoneRed : deckSpawnZoneBlue;

        // Vider la zone
        foreach (Transform child in zone)
            Destroy(child.gameObject);

        // Ré-instancier chaque carte du deck
        foreach (var data in deck)
        {
            GameObject cardObj = Instantiate(cardPrefab, zone);
            var ui = cardObj.GetComponent<UnitCardUI>();
            if (ui != null)
                ui.Initialize(data);
        }
    }
}











