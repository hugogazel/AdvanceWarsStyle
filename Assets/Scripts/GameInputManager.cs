using System.Collections.Generic;
using UnityEngine;
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
        // Initialisation des références
        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager>();
        if (mainCamera == null)
            mainCamera = Camera.main;
        if (arrowPreviewManager == null)
            arrowPreviewManager = FindObjectOfType<ArrowPreviewManager>();
        turnManager = FindObjectOfType<TurnManager>();
        if (turnManager == null)
            Debug.LogWarning("TurnManager introuvable !");
    }

    void Update()
    {
        // 1) Si l'UI d'action d'unité est ouverte, on ignore le clic
        if (gridManager?.actionPanel?.gameObject.activeInHierarchy == true)
            return;

        // 2) Preview de chemin
        if (gridManager?.selectedUnit != null && !Input.GetMouseButton(0))
        {
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0;
            Vector3Int cell = gridManager.groundTilemap.WorldToCell(worldPos);
            Vector2Int gridPos = new Vector2Int(cell.x, cell.y);
            List<GridCell> path = gridManager.FindPath(
                gridManager.selectedUnit,
                gridManager.selectedUnit.position,
                gridPos
            );
            arrowPreviewManager?.ShowPathPreview(path);
        }

        // 3) Clic gauche
        if (Input.GetMouseButtonDown(0))
            HandleLeftClick();
    }

    private void HandleLeftClick()
    {
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0;

        // A) Détection d'une unité sous le curseur
        Collider2D[] hits = Physics2D.OverlapCircleAll(worldPos, 0.2f, unitLayerMask);
        if (hits.Length > 0)
        {
            // On prend le plus proche
            Collider2D closest = hits[0];
            float minDist = Vector2.Distance(worldPos, closest.transform.position);
            for (int i = 1; i < hits.Length; i++)
            {
                float d = Vector2.Distance(worldPos, hits[i].transform.position);
                if (d < minDist)
                {
                    minDist = d;
                    closest = hits[i];
                }
            }
            UnitController uc = closest.GetComponent<UnitController>();
            if (uc != null)
            {
                OnUnitClicked(uc);
                return;
            }
        }

        // B) Sinon on considère un clic sur une case vide
        Vector3Int cell = gridManager.groundTilemap.WorldToCell(worldPos);
        Vector2Int gridPos = new Vector2Int(cell.x, cell.y);
        UnitController ucAtCell = gridManager.GetUnitAtPosition(gridPos);
        if (ucAtCell != null)
        {
            OnUnitClicked(ucAtCell);
        }
        else
        {
            OnEmptyCellClicked(gridPos);
        }
    }

    private void OnUnitClicked(UnitController clickedUnit)
    {
        // Ignorer si l'unité a déjà agi
        if (clickedUnit.hasActed)
            return;
        // Ignorer si ce n'est pas au tour de l'équipe
        if (turnManager != null && clickedUnit.team != turnManager.currentTeam)
            return;

        // Si aucune unité sélectionnée : on la sélectionne et on affiche ses cases atteignables
        if (gridManager.selectedUnit == null)
        {
            SelectUnit(clickedUnit);
        }
        else
        {
            // Si on reclique sur la même unité : on affiche le panneau d'action
            if (gridManager.selectedUnit == clickedUnit)
            {
                gridManager.actionPanel?.ShowPanel(clickedUnit);
            }
            // Si on clique sur une unité alliée différente : on change la sélection
            else if (gridManager.selectedUnit.IsAllied(clickedUnit))
            {
                SelectUnit(clickedUnit);
            }
            else
            {
                // Ici, vous pourriez déclencher un mode "attaque" ou afficher un message
                Debug.Log("Pour attaquer, utilisez le bouton dédié.");
            }
        }
    }

    private void SelectUnit(UnitController unit)
    {
        gridManager.selectedUnit = unit;
        var reachable = gridManager.ComputeReachableTiles(unit, unit.position);
        gridManager.ClearHighlightedTiles();
        gridManager.HighlightReachableTiles(reachable, gridManager.defaultHighlightColor);
    }

    private void OnEmptyCellClicked(Vector2Int gridPos)
    {
        // Si une unité est sélectionnée et qu'il existe des cases surlignées
        if (gridManager.selectedUnit != null && gridManager.highlightedTiles.Count > 0)
        {
            GridCell cell = gridManager.GetCell(gridPos);
            // Si la case est atteignable et que l'unité n'a pas encore agi
            if (cell != null &&
                gridManager.highlightedTiles.Contains(cell) &&
               !gridManager.selectedUnit.hasActed)
            {
                gridManager.selectedUnit.MoveTo(gridPos);
            }
            else
            {
                // Annulation du déplacement
                gridManager.ClearHighlightedTiles();
                arrowPreviewManager?.ClearPathPreview();
                gridManager.selectedUnit = null;
            }
        }
        else
        {
            // Si aucune unité sélectionnée → on termine le tour
            turnManager?.EndTurn();
        }
    }
}
















