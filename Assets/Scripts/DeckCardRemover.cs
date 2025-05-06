using UnityEngine;
using UnityEngine.EventSystems;

public class DeckCardRemover : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        // On n'intercepte que le clic GAUCHE pour suppression
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        Debug.Log("[DeckCardRemover] Carte cliquée pour suppression : " + gameObject.name);
        UnitCardUI cardUI = GetComponent<UnitCardUI>();
        if (cardUI != null)
            DeckUIManager.Instance.RemoveCardFromDeck(cardUI.unitData);

        Destroy(gameObject);
    }
}

