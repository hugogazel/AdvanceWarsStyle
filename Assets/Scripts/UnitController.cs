using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UnitController : MonoBehaviour
{
    [Header("Data")]
    public UnitData unitData;
    public GameObject carcassPrefab;

    [Header("State")]
    public Vector2Int position;
    public int movePoints;
    public bool isMoving = false;
    public bool hasActed = false;

    [Header("Team Settings")]
    public Team team = Team.J1Team;

    [Header("Stats")]
    public int currentBiomass;
    public int currentLifePoints;
    public int currentStomach = 0;

    [Header("UI")]
    public TextMeshProUGUI lifePointsText;

    private GridManager gridManager;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    void Awake()
    {
        // Hérite du defaultTeam si défini
        if (unitData != null)
            team = unitData.defaultTeam;

        // Récupère la GridManager
        gridManager = FindObjectOfType<GridManager>();
        if (gridManager == null)
            Debug.LogError("GridManager non trouvé !");

        // Calcule la position sur la grille et snap
        position = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.y)
        );
        SnapToGrid();
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (unitData != null)
        {
            movePoints = unitData.maxMovePoints;
            currentBiomass = unitData.biomass;
            currentLifePoints = unitData.lifePoints;
        }

        UpdateLifePointsText();
        SnapToGrid();
    }

    private void UpdateLifePointsText()
    {
        if (lifePointsText != null)
            lifePointsText.text = currentLifePoints.ToString();
    }

    public void TakeDamage(int damage)
    {
        currentLifePoints -= damage;
        UpdateLifePointsText();

        if (currentLifePoints <= 0)
            Die();
    }

    private void Die()
    {
        // Instancie une carcasse
        Vector3 spawnPos = transform.position;
        if (gridManager?.groundTilemap != null)
        {
            spawnPos = gridManager.groundTilemap
                .GetCellCenterWorld(new Vector3Int(position.x, position.y, 0));
        }

        if (carcassPrefab != null)
        {
            GameObject c = Instantiate(carcassPrefab, spawnPos, Quaternion.identity);
            var carc = c.GetComponent<Carcass>();
            if (carc != null)
            {
                carc.fillValue = currentBiomass;
                carc.maxFillValue = currentBiomass;
            }
        }

        Destroy(gameObject);
    }

    public void MoveTo(Vector2Int targetPosition)
    {
        if (hasActed || isMoving || gridManager == null)
            return;

        // Si on reclique sur la même case, affiche le menu d'action
        if (targetPosition == position)
        {
            gridManager.actionPanel?.ShowPanel(this);
            return;
        }

        GridCell targetCell = gridManager.GetCell(targetPosition);
        if (targetCell == null)
            return;

        // Vérifie s'il y a un obstacle ou une unité ennemie
        var occupant = gridManager.GetUnitAtPosition(targetPosition);
        if (occupant != null && !IsAllied(occupant))
            return;

        List<GridCell> path = gridManager.FindPath(this, position, targetPosition);
        if (path == null || path.Count == 0)
            return;

        // Lance la coroutine de déplacement
        StartCoroutine(MoveAlongPath(path));
    }

    private IEnumerator MoveAlongPath(List<GridCell> path)
    {
        isMoving = true;

        foreach (var cell in path)
        {
            Vector3 targetWorld = gridManager.groundTilemap
                .GetCellCenterWorld(new Vector3Int(cell.position.x, cell.position.y, 0));
            while (Vector3.Distance(transform.position, targetWorld) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position, targetWorld, 2f * Time.deltaTime);
                yield return null;
            }
            position = cell.position;
            SnapToGrid();
        }

        isMoving = false;
        gridManager.actionPanel?.ShowPanel(this);
    }

    public void EatResource()
    {
        // Herbivores et omnivores nourrissent leur estomac
        if (unitData.isHerbivore || unitData.isOmnivore)
        {
            currentStomach++;
            if (currentStomach >= 3)
            {
                IncreaseBiomass(1);
                currentStomach = 0;
            }
        }
    }

    public void IncreaseBiomass(int amount)
    {
        currentBiomass += amount;
    }

    public void MarkAsWaiting()
    {
        hasActed = true;
        spriteRenderer.color = Color.gray;
        gridManager.selectedUnit = null;
        gridManager.ClearHighlightedTiles();
    }

    public void ResetVisuals()
    {
        hasActed = false;
        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;
    }

    public bool IsAllied(UnitController other)
    {
        return this.team == other.team;
    }

    private void SnapToGrid()
    {
        if (gridManager?.groundTilemap != null)
        {
            transform.position = gridManager.groundTilemap
                .GetCellCenterWorld(new Vector3Int(position.x, position.y, 0));
        }
    }
}



















