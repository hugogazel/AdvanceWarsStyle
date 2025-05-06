using UnityEngine;
using System.Collections.Generic;

public class DeckDisplayManager : MonoBehaviour
{
    [Header("UI")]
    public Transform unitCardPanel;  // Parent où instancier les cartes
    public GameObject cardPrefab;    // Le prefab de carte (avec UnitCardUI)

    private void Start()
    {
        // 1) Vide le panel
        foreach (Transform child in unitCardPanel)
            Destroy(child.gameObject);

        // 2) Récupère le deck du joueur actif
        List<UnitData> deck = (GameManager.Instance.currentPlayerTurn == 1)
            ? GameManager.Instance.player1Deck
            : GameManager.Instance.player2Deck;

        // 3) Pour chaque carte …
        foreach (UnitData data in deck)
        {
            // **DEBUG** : quelle unité et quel sexe ?
            Debug.Log($"[DeckDisplayManager] Instantiating '{data.unitName}' with sex = {data.sex}");

            // a) Instancie l’UI
            GameObject cardObj = Instantiate(cardPrefab, unitCardPanel);
            var ui = cardObj.GetComponent<UnitCardUI>();

            // **DEBUG** : l’UI et son background sont-ils valides ?
            if (ui.cardBackground == null)
                Debug.LogWarning("[DeckDisplayManager] cardBackground IS NULL on UnitCardUI!");
            else
                Debug.Log($"[DeckDisplayManager] cardBackground initial color = {ui.cardBackground.color}");

            // b) Initialise la carte (remet le fond d’origine)
            ui.Initialize(data);

            // **DEBUG** : couleur après Initialize ?
            Debug.Log($"[DeckDisplayManager] after Initialize, color = {ui.cardBackground.color}");

            // c) Applique la couleur Male/Female
            ui.ApplySexColor();

            // **DEBUG** : couleur après recoloration ?
            Debug.Log($"[DeckDisplayManager] after ApplySexColor, color = {ui.cardBackground.color}");
        }
    }
}




