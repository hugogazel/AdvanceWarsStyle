using UnityEngine;
using UnityEngine.EventSystems;

public class SexSelectionHandler : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            var cardUI = GetComponent<UnitCardUI>();
            SexSelectionPanel.Instance.Show(cardUI);
        }
    }
}

