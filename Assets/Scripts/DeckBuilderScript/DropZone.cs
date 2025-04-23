using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("[DropZone] OnDrop appelé sur " + gameObject.name);
        if (eventData.pointerDrag != null)
        {
            // Rattacher l'objet à cette zone (le DeckPanel)
            eventData.pointerDrag.transform.SetParent(transform, false);

            // Récupérer le composant UnitCardUI pour obtenir la donnée UnitData
            UnitCardUI unitCard = eventData.pointerDrag.GetComponent<UnitCardUI>();
            if (unitCard != null)
            {
                // Utiliser le singleton de DeckUIManager pour ajouter la carte au deck
                DeckUIManager.Instance.AddCardToDeck(unitCard.unitData);
                Debug.Log("[DropZone] Carte ajoutée au deck : " + unitCard.unitData.unitName);
            }
            else
            {
                Debug.LogWarning("[DropZone] Le composant UnitCardUI est introuvable sur l'objet draggué.");
            }
        }
    }
}



