using UnityEngine;
using System.Collections.Generic;

public class GameInitializer : MonoBehaviour
{
    void Start()
    {
        var gm = GameManager.Instance;
        List<UnitData> deck1 = gm.player1Deck;
        List<UnitData> deck2 = gm.player2Deck;

        // Par exemple, stocke-les dans ton TurnManager ou GridManager
        var tm = FindObjectOfType<TurnManager>();
        tm.player1Deck = deck1;
        tm.player2Deck = deck2;

        // Assigne l'équipe de départ
        tm.currentTeam = Team.Red;
        tm.StartGameLoop();
    }
}

