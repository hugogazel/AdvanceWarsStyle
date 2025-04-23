using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // --- Singleton ---
    public static GameManager Instance { get; private set; }

    // --- Input lock (conservé de ta version originale) ---
    public static bool inputLocked = false;

    // --- Deck Building ---
    [Header("Deck Building")]
    public List<UnitData> player1Deck = new List<UnitData>();
    public List<UnitData> player2Deck = new List<UnitData>();
    [Tooltip("1 = construction du deck J1, 2 = construction du deck J2")]
    public int deckBuilderPlayer = 1;

    // --- Gameplay ---
    [Header("Gameplay")]
    [Tooltip("1 = tour du joueur 1 (Red), 2 = tour du joueur 2 (Blue)")]
    public int currentPlayerTurn = 1;

    void Awake()
    {
        // Singleton + persistance
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Reset de l'input lock à chaque début de scène
        inputLocked = false;
    }

    /// <summary>
    /// Appeler depuis ton DeckUIManager lorsque le joueur actuel clique sur Start.
    /// </summary>
    public void SubmitDeck(List<UnitData> deck)
    {
        if (deckBuilderPlayer == 1)
        {
            // Stocke le deck du J1
            player1Deck = new List<UnitData>(deck);
            deckBuilderPlayer = 2;
            // Recharge la même scène de deck builder pour J2
            SceneManager.LoadScene("DeckBuilderScene");
        }
        else if (deckBuilderPlayer == 2)
        {
            // Stocke le deck du J2
            player2Deck = new List<UnitData>(deck);
            // Les deux decks sont prêts → lancement de la partie
            StartGame();
        }
    }

    /// <summary>
    /// Charge la scène de jeu et initialise le tour du joueur 1.
    /// </summary>
    private void StartGame()
    {
        currentPlayerTurn = 1;
        SceneManager.LoadScene("FirstMapScene");
    }

    /// <summary>
    /// À appeler à la fin du tour d'un joueur pour passer au suivant.
    /// </summary>
    public void EndTurn()
    {
        currentPlayerTurn = (currentPlayerTurn == 1) ? 2 : 1;
        // Ici tu peux déclencher une UI de changement de tour, etc.
    }
}



