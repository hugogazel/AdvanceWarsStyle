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
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        // Affichage dynamique de la preview de chemin
        UpdatePathPreview();

        if (Input.GetMouseButtonDown(0))
            HandleLeftClick();
    }

    private void UpdatePathPreview()
    {
        arrowPreviewManager.ClearPathPreview();

        var sel = gridManager.selectedUnit;
        if (sel == null || gridManager.highlightedTiles.Count == 0)
            return;

        Vector3 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0f;
        Vector3Int cellPos = gridManager.groundTilemap.WorldToCell(worldPos);
        Vector2Int gridPos = new Vector2Int(cellPos.x, cellPos.y);

        if (gridManager.highlightedTiles.Exists(c => c.position == gridPos))
        {
            var path = gridManager.FindPath(sel, sel.position, gridPos);
            if (path != null && path.Count > 0)
                arrowPreviewManager.ShowPathPreview(path);  // <-- méthode correcte :contentReference[oaicite:0]{index=0}&#8203;:contentReference[oaicite:1]{index=1}
        }
    }

    private void HandleLeftClick()
    {
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0f;

        // 1) Clic sur unité
        Collider2D[] hits = Physics2D.OverlapCircleAll(worldPos, 0.2f, unitLayerMask);
        if (hits.Length > 0)
        {
            var uc = hits[0].GetComponent<UnitController>();
            if (uc != null &&
                uc.team == turnManager.currentTeam &&
                !uc.hasActed)
            {
                gridManager.selectedUnit = uc;
                var tiles = gridManager.ComputeReachableTiles(uc, uc.position);
                gridManager.HighlightReachableTiles(tiles, gridManager.defaultHighlightColor);
            }
            return;
        }

        // 2) Clic sur case vide
        Vector3Int cellPos = gridManager.groundTilemap.WorldToCell(worldPos);
        Vector2Int gridPos = new Vector2Int(cellPos.x, cellPos.y);
        if (gridManager.selectedUnit != null && gridManager.highlightedTiles.Count > 0)
        {
            if (gridManager.highlightedTiles.Exists(c => c.position == gridPos))
            {
                gridManager.selectedUnit.MoveTo(gridPos);
            }
            else
            {
                gridManager.ClearHighlightedTiles();
                arrowPreviewManager.ClearPathPreview();
                gridManager.selectedUnit = null;
            }
            return;
        }

        // 3) Autre clic -> fin de tour
        turnManager?.ShowEndTurnPanelOnly();
    }
}





















