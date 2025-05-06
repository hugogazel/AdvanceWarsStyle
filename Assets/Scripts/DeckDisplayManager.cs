using UnityEngine;
using System.Collections.Generic;

public class DeckDisplayManager : MonoBehaviour
{
    public Transform unitCardPanel;
    public GameObject cardPrefab;

    private void Start()
    {
        // Récupère le deck persistant via GameManager
        List<UnitData> deck = (GameManager.Instance.currentPlayerTurn == 1)
            ? GameManager.Instance.player1Deck
            : GameManager.Instance.player2Deck;

        foreach (Transform child in unitCardPanel)
            Destroy(child.gameObject);

        foreach (UnitData data in deck)
        {
            var cardObj = Instantiate(cardPrefab, unitCardPanel);
            var ui = cardObj.GetComponent<UnitCardUI>();
            ui.Initialize(data);
            cardObj.AddComponent<SexSelectionHandler>(); // si besoin de modifier en jeu
        }
    }
}


