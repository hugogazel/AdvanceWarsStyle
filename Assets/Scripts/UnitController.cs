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

    [Header("Team")]
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
        if (lifePointsText == null)
            lifePointsText = GetComponentInChildren<TextMeshProUGUI>();

        gridManager = FindObjectOfType<GridManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        if (unitData != null)
        {
            movePoints = unitData.maxMovePoints;
            currentBiomass = unitData.biomass;
            currentLifePoints = unitData.lifePoints;
        }
        else
        {
            currentLifePoints = 0;
            Debug.LogWarning($"[{name}] unitData non assigné !");
        }

        if (lifePointsText != null)
        {
            lifePointsText.gameObject.SetActive(true);
            UpdateLifePointsText();
        }

        SnapToGrid();
    }

    private void UpdateLifePointsText()
    {
        if (lifePointsText != null)
            lifePointsText.text = currentLifePoints.ToString();
    }

    public void TakeDamage(int dmg)
    {
        currentLifePoints -= dmg;
        UpdateLifePointsText();
        if (currentLifePoints <= 0) Die();
    }

    private void Die()
    {
        Vector3 pos = transform.position;
        if (gridManager?.groundTilemap != null)
            pos = gridManager.groundTilemap.GetCellCenterWorld(
                new Vector3Int(position.x, position.y, 0));

        if (carcassPrefab != null)
        {
            var c = Instantiate(carcassPrefab, pos, Quaternion.identity);
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
        if (hasActed || isMoving) return;

        if (targetPosition == position)
        {
            gridManager.actionPanel?.ShowPanel(this);
            return;
        }

        var occ = gridManager.GetUnitAtPosition(targetPosition);
        if (occ != null && !IsAllied(occ)) return;

        var path = gridManager.FindPath(this, position, targetPosition);
        if (path == null || path.Count == 0) return;

        StartCoroutine(MoveAlongPath(path));
    }

    IEnumerator MoveAlongPath(List<GridCell> path)
    {
        isMoving = true;
        gridManager.ClearHighlightedTiles();
        FindObjectOfType<ArrowPreviewManager>()?.ClearPathPreview();

        foreach (var cell in path)
        {
            Vector3 tgt = gridManager.groundTilemap
                .GetCellCenterWorld((Vector3Int)cell.position);
            while (Vector3.Distance(transform.position, tgt) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position, tgt, 2f * Time.deltaTime);
                yield return null;
            }
            position = cell.position;
            SnapToGrid();
        }

        isMoving = false;
        // Summoning sickness after move
        MarkAsWaiting();
        gridManager.actionPanel?.ShowPanel(this);
    }

    /// <summary>
    /// Herbivores et omnivores consomment et gagnent en biomasse.
    /// </summary>
    public void EatResource()
    {
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

    /// <summary>
    /// Ajoute de la biomasse à l’unité.
    /// </summary>
    public void IncreaseBiomass(int amount)
    {
        currentBiomass += amount;
    }

    /// <summary>
    /// Épuise l’unité visuellement et empêche toute action.
    /// </summary>
    public void MarkAsWaiting()
    {
        hasActed = true;
        if (spriteRenderer != null)
            spriteRenderer.color = Color.gray;
        gridManager.selectedUnit = null;
        gridManager.ClearHighlightedTiles();
    }

    /// <summary>
    /// Réactive l’unité en début de tour.
    /// </summary>
    public void ResetVisuals()
    {
        hasActed = false;
        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;
    }

    public bool IsAllied(UnitController o) => team == o.team;

    private void SnapToGrid()
    {
        if (gridManager?.groundTilemap == null) return;
        transform.position = gridManager.groundTilemap.GetCellCenterWorld(
            new Vector3Int(position.x, position.y, 0));
    }

    private void OnMouseDown()
    {
        if (team != TurnManager.Instance.currentTeam || hasActed) return;
        gridManager.selectedUnit = this;
        var tiles = gridManager.ComputeReachableTiles(this, position);
        gridManager.HighlightReachableTiles(tiles, gridManager.defaultHighlightColor);
    }
}





















