using UnityEngine;
using System.Collections.Generic;

public class DeckDisplayManager : MonoBehaviour
{
    // Le panel dans lequel seront affichées les cartes dans FirstMapScene
    public Transform unitCardPanel;

    // Le prefab de carte (il peut s'agir du même que celui utilisé dans le DeckUIManager, ou d'une version adaptée pour l'affichage en jeu)
    public GameObject cardPrefab;

    private void Start()
    {
        // Vérifier que le DeckUIManager existe et que le deck est disponible
        if (DeckUIManager.Instance == null)
        {
            Debug.LogError("DeckUIManager non trouvé ! Assurez-vous qu'il est persistant ou que les données du deck sont correctement stockées.");
            return;
        }

        List<UnitData> deck = DeckUIManager.Instance.selectedDeckCards;
        Debug.Log("[DeckDisplayManager] Affichage du deck contenant " + deck.Count + " cartes.");

        // Vider le panel d'affichage (au cas où il y aurait déjà des enfants)
        foreach (Transform child in unitCardPanel)
        {
            Destroy(child.gameObject);
        }

        // Pour chaque carte dans le deck, instancier une visualisation dans le panel
        foreach (UnitData data in deck)
        {
            GameObject cardObj = Instantiate(cardPrefab, unitCardPanel);
            UnitCardUI cardUI = cardObj.GetComponent<UnitCardUI>();
            if (cardUI != null)
            {
                cardUI.Initialize(data);
            }
            else
            {
                Debug.LogWarning("Le prefab de carte n'a pas de composant UnitCardUI attaché.");
            }
        }
    }
}

