using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class CardDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // Indique si cet objet est un clone créé pour le drag
    public bool isClone = false;

    private Vector3 startPosition;   // Position de départ du clone
    private Transform startParent;   // Parent d'origine du clone
    private CanvasGroup canvasGroup; // Pour gérer la transparence et les raycasts
    private Canvas canvas;           // Référence au Canvas principal

    private Vector2 pointerOffset;   // Décalage calculé entre la position du pointeur et l'objet

    private void Awake()
    {
        // ← Désactive ce script dès qu’on n’est pas en DeckBuilderScene
        if (SceneManager.GetActiveScene().name != "DeckBuilderScene")
        {
            enabled = false;
            return;
        }

        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();

        Debug.Log("[CardDraggable] Awake - " + gameObject.name +
                  " : canvasGroup found = " + (canvasGroup != null) +
                  ", canvas found = " + (canvas != null) +
                  ", isClone = " + isClone);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Si ce n'est pas un clone, créer une copie et déclencher son drag,
        // afin de laisser l'original dans l'AvailableCardsPanel.
        if (!isClone)
        {
            GameObject clone = Instantiate(gameObject, canvas.transform);
            // Marquer le clone pour qu'il soit "draggable"
            clone.GetComponent<CardDraggable>().isClone = true;
            // Positionner le clone exactement au même endroit que l'original
            clone.transform.position = transform.position;
            // Rediriger l'événement vers le clone
            eventData.pointerDrag = clone;
            clone.GetComponent<CardDraggable>().OnBeginDrag(eventData);
            return;
        }

        // Pour le clone, stocker la position et le parent d'origine pour pouvoir le traiter en fin de drag.
        startPosition = transform.position;
        startParent = transform.parent;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0.6f;
            canvasGroup.blocksRaycasts = false;
        }

        // Calcule l'offset afin que la carte suive correctement le curseur
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out Vector2 localPoint);
        pointerOffset = (Vector2)transform.localPosition - localPoint;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Seul le clone est déplacé (l'original reste en place)
        if (!isClone)
            return;

        if (canvas != null)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                canvas.worldCamera,
                out Vector2 localPoint);

            // Appliquer l'offset afin que la carte soit exactement sous le curseur
            transform.localPosition = localPoint + pointerOffset;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Seul le clone gère le drop
        if (!isClone)
            return;

        Debug.Log("[CardDraggable] OnEndDrag called on " + gameObject.name);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }

        // Vérifier la zone sous le curseur au moment du drop
        GameObject pointerObj = eventData.pointerEnter;
        Debug.Log("[CardDraggable] pointerEnter = " + (pointerObj != null ? pointerObj.name : "NULL"));

        // Si le drop n'est pas effectué sur une zone valide (DropZone), détruire le clone.
        if (pointerObj == null || pointerObj.GetComponent<DropZone>() == null)
        {
            Debug.Log("[CardDraggable] Drop non valide. Destruction du clone.");
            Destroy(gameObject);
        }
        // Sinon, on laisse le clone dans le DeckPanel. La DropZone s'occupera de l'attacher correctement.
    }
}




