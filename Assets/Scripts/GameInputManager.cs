using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class GameInputManager : MonoBehaviour
{
    [Header("Références principales")]
    public GridManager gridManager;
    public Camera mainCamera;
    public ArrowPreviewManager arrowPreviewManager;
    public LayerMask unitLayerMask;

    private TurnManager turnManager;

    void Start()
    {
        if (gridManager == null) gridManager = FindObjectOfType<GridManager>();
        if (mainCamera == null) mainCamera = Camera.main;
        if (arrowPreviewManager == null) arrowPreviewManager = FindObjectOfType<ArrowPreviewManager>();

        turnManager = FindObjectOfType<TurnManager>();
        if (turnManager == null) Debug.LogWarning("TurnManager introuvable !");
    }

    void Update()
    {
        // Si on survole une UI, on n'interprète pas de clic in-game
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        // ... affichage de la preview, inchangé ...

        if (Input.GetMouseButtonDown(0))
            HandleLeftClick();
    }

    private void HandleLeftClick()
    {
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0;

        // 1) Clic sur unité
        Collider2D[] hits = Physics2D.OverlapCircleAll(worldPos, 0.2f, unitLayerMask);
        if (hits.Length > 0)
        {
            // … même logique qu’avant pour OnUnitClicked …
            // (sélection, panel d’action…)
            return;
        }

        // 2) Clic sur case vide
        Vector3Int cellPos = gridManager.groundTilemap.WorldToCell(worldPos);
        Vector2Int gridPos = new Vector2Int(cellPos.x, cellPos.y);
        if (gridManager.selectedUnit != null && gridManager.highlightedTiles.Count > 0)
        {
            // … déplacement ou annulation …
            return;
        }

        // 3) Sinon, on ouvre uniquement le panel de fin de tour
        turnManager?.ShowEndTurnPanelOnly();
    }
}


















