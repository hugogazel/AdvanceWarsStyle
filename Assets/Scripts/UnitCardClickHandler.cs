using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(UnitCardUI))]
public class UnitCardClickHandler : MonoBehaviour, IPointerClickHandler
{
    private UnitCardUI cardUI;

    private void Awake()
    {
        cardUI = GetComponent<UnitCardUI>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (cardUI.unitData != null)
        {
            CardInfoPanelController.Instance.UpdateInfo(cardUI.unitData);
        }
    }
}


