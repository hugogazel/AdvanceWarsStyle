using System.Collections.Generic;
using UnityEngine;

public class CamouflageManager : MonoBehaviour
{
    public GridManager gridManager;   // Référence au gestionnaire de grille
    public TurnManager turnManager;   // Référence au gestionnaire de tours

    // Pour un système de fog of war (vision)
    public int defaultVisionRange = 3;

    void Start()
    {
        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager>();
        if (turnManager == null)
            turnManager = FindObjectOfType<TurnManager>();
    }

    void Update()
    {
        // Vérifie que nos références sont définies
        if (turnManager == null || gridManager == null)
            return;

        Team currentTeam = turnManager.currentTeam;
        UnitController[] allUnits = FindObjectsOfType<UnitController>();

        foreach (UnitController unit in allUnits)
        {
            // On récupère le SpriteRenderer
            SpriteRenderer sr = unit.GetComponent<SpriteRenderer>();
            if (sr == null)
                continue;

            // On récupère le canvas des points de vie (assumé être un enfant de l'unité)
            Canvas lifePointCanvas = unit.GetComponentInChildren<Canvas>();

            // Les unités de la team en cours sont toujours visibles
            if (unit.team == currentTeam)
            {
                if (!sr.enabled)
                {
                    sr.enabled = true;
                    Debug.Log("Unit " + unit.name + " (team " + unit.team + ") est alliée et visible.");
                }
                if (lifePointCanvas != null && !lifePointCanvas.gameObject.activeSelf)
                {
                    lifePointCanvas.gameObject.SetActive(true);
                }
                continue;
            }

            // Pour une unité adverse, vérifier le camouflage
            bool camo = IsUnitCamouflaged(unit, currentTeam);
            sr.enabled = !camo;
            if (lifePointCanvas != null)
            {
                lifePointCanvas.gameObject.SetActive(!camo);
            }
            Debug.Log("Unit " + unit.name + " (team " + unit.team + ") camouflage=" + camo);
        }

        // Vous pouvez aussi appeler ici UpdateFogOfWar() pour un système de fog of war complet.
    }


    /// <summary>
    /// Retourne true si l'unité est camouflée pour un observateur appartenant à observerTeam.
    /// Conditions :
    /// - L'unité adverse doit se trouver sur une forêt.
    /// - Aucune unité de observerTeam ne doit se trouver sur une cellule adjacente.
    /// Les unités alliées sont toujours visibles.
    /// </summary>
    public bool IsUnitCamouflaged(UnitController unit, Team observerTeam)
    {
        // Si l'unité appartient à l'équipe observatrice, elle est visible.
        if (unit.team == observerTeam)
        {
            Debug.Log("IsUnitCamouflaged: " + unit.name + " est alliée => visible");
            return false;
        }

        // Récupérer la cellule sur laquelle se trouve l'unité.
        GridCell cell = gridManager.GetCell(unit.position);
        if (cell == null)
        {
            Debug.Log("IsUnitCamouflaged: " + unit.name + " n'a pas de cell => visible");
            return false;
        }

        // L'unité n'est camouflée que si elle est sur une forêt.
        if (cell.terrain != TerrainType.Forest)
        {
            Debug.Log("IsUnitCamouflaged: " + unit.name + " terrain=" + cell.terrain + " (non Forest) => visible");
            return false;
        }

        // Vérifier si une unité de observerTeam se trouve sur une cellule adjacente.
        List<GridCell> neighbors = gridManager.GetNeighbors(unit.position);
        foreach (GridCell neighbor in neighbors)
        {
            UnitController adjacentUnit = gridManager.GetUnitAtPosition(neighbor.position);
            if (adjacentUnit != null && adjacentUnit.team == observerTeam)
            {
                Debug.Log("IsUnitCamouflaged: " + unit.name + " a " + adjacentUnit.name + " adjacent de team " + observerTeam + " => visible");
                return false;
            }
        }

        Debug.Log("IsUnitCamouflaged: " + unit.name + " est camouflée pour l'observateur de team " + observerTeam);
        return true;
    }

    /// <summary>
    /// Calcule l'ensemble des cellules visibles pour une équipe donnée en combinant
    /// le champ de vision de toutes ses unités. (Pour un système de fog of war de base)
    /// </summary>
    public List<Vector2Int> CalculateVisibleCells(Team team)
    {
        HashSet<Vector2Int> visibleCells = new HashSet<Vector2Int>();
        UnitController[] allUnits = FindObjectsOfType<UnitController>();
        foreach (UnitController unit in allUnits)
        {
            if (unit.team == team)
            {
                int visionRange = defaultVisionRange;
                Vector2Int center = unit.position;
                for (int dx = -visionRange; dx <= visionRange; dx++)
                {
                    for (int dy = -visionRange; dy <= visionRange; dy++)
                    {
                        if (dx * dx + dy * dy <= visionRange * visionRange)
                        {
                            Vector2Int pos = new Vector2Int(center.x + dx, center.y + dy);
                            visibleCells.Add(pos);
                        }
                    }
                }
            }
        }
        return new List<Vector2Int>(visibleCells);
    }

    /// <summary>
    /// Exemple de méthode pour mettre à jour le fog of war.
    /// Cette méthode pourrait, par exemple, assombrir les cellules non visibles.
    /// L'implémentation dépend de votre système d'affichage.
    /// </summary>
    public void UpdateFogOfWar()
    {
        List<Vector2Int> visibleCells = CalculateVisibleCells(turnManager.currentTeam);
        // Implémentez ici la mise à jour du rendu du fog of war.
    }
}
