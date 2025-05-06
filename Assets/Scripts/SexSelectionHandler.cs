using UnityEngine;
using UnityEngine.EventSystems;

public class SexSelectionHandler : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right)
            return;

        if (SexSelectionPanel.Instance == null)
        {
            Debug.LogWarning("SexSelectionPanel.Instance est null ! Vérifie que tu l'as mis en scène.");
            return;
        }

        var cardUI = GetComponent<UnitCardUI>();
        if (cardUI == null)
        {
            Debug.LogWarning("UnitCardUI manquant sur " + gameObject.name);
            return;
        }

        SexSelectionPanel.Instance.Show(cardUI);
    }
}


