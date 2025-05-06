using UnityEngine;
using UnityEngine.EventSystems;

public class DeckCardRemover : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("[DeckCardRemover] Carte cliquée pour suppression : " + gameObject.name);
        // Récupérer la donnée de l'unité via le composant UnitCardUI
        UnitCardUI cardUI = GetComponent<UnitCardUI>();
        if (cardUI != null)
        {
            // Mise à jour du deck en retirant la carte (UnitData associée)
            DeckUIManager.Instance.RemoveCardFromDeck(cardUI.unitData);
        }
        // Détruire l'instance affichée dans le DeckPanel
        Destroy(gameObject);
    }
}
