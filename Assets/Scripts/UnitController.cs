using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UnitController : MonoBehaviour
{
    public UnitData unitData;           // Données de l'unité (nom, sprite, biomasse, lifePoints, etc.)
    public Vector2Int position;
    public int movePoints;
    public bool isMoving = false;

    [Header("Team Settings")]
    public Team team = Team.Red;
    public bool hasActed = false;

    private GridManager gridManager;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    // Pour les mécaniques internes (ex : valeur utilisée pour la carcasse)
    public int currentBiomass;

    // Nouveau champ pour les points de vie (qui déterminent la survie)
    public int currentLifePoints;

    // Champ UI pour afficher les Life Points (remplace l'ancien BiomassText)
    public TextMeshProUGUI lifePointsText;

    // Jauge d'estomac pour les herbivores (utilisée pour les interactions, si nécessaire)
    public int currentStomach = 0; // Va de 0 à 3

    // Prefab de carcasse instancié à la mort de l'unité
    public GameObject carcassPrefab; // À assigner dans l'inspecteur

    void Awake()
    {
        gridManager = FindObjectOfType<GridManager>();
        if (gridManager == null)
        {
            Debug.LogError("GridManager non trouvé !");
            return;
        }
        // Mettre à jour position interne
        position = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.y)
        );
        SnapToGrid();  // ← maintenant dès Awake(), avant tout autre script
    }

    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        if (gridManager == null)
        {
            Debug.LogError("❌ GridManager non trouvé !");
            return;
        }
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (unitData != null)
        {
            movePoints = unitData.maxMovePoints;
            currentBiomass = unitData.biomass;
            currentLifePoints = unitData.lifePoints;
        }

        currentStomach = 0;
        UpdateLifePointsText();

        position = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.y)
        );
        SnapToGrid();

        // ← Lignes de surlignage auto supprimées ici
        // List<GridCell> reachable = gridManager.ComputeReachableTiles(this, position);
        // gridManager.HighlightReachableTiles(reachable, gridManager.defaultHighlightColor);
    }


    /// <summary>
    /// Met à jour le champ UI affichant les Life Points.
    /// </summary>
    private void UpdateLifePointsText()
    {
        if (lifePointsText != null)
        {
            lifePointsText.text = currentLifePoints.ToString();
        }
    }

    /// <summary>
    /// Applique des dégâts en soustrayant le montant aux Life Points.
    /// </summary>
    public void TakeDamage(int damage)
    {
        currentLifePoints -= damage;
        Debug.Log(unitData.unitName + " subit " + damage + " dégâts. PV restants = " + currentLifePoints);
        UpdateLifePointsText();

        if (currentLifePoints <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Méthode appelée lorsque l'unité meurt.
    /// Instancie une carcasse à la position de la mort avant de détruire l'unité.
    /// </summary>
    private void Die()
    {
        Debug.Log("💀 " + unitData.unitName + " est mort !");

        // Calculer la position centrée sur la cellule (à partir de la groundTilemap)
        Vector3 spawnPos = transform.position;
        if (gridManager != null && gridManager.groundTilemap != null)
        {
            spawnPos = gridManager.groundTilemap.GetCellCenterWorld(new Vector3Int(position.x, position.y, 0));
        }

        // Instancier une carcasse à la position calculée
        if (carcassPrefab != null)
        {
            GameObject carcassObject = Instantiate(carcassPrefab, spawnPos, Quaternion.identity);
            Carcass carcass = carcassObject.GetComponent<Carcass>();
            if (carcass != null)
            {
                // Ici, la carcasse reçoit la biomasse de l'unité au moment de sa mort.
                // Ainsi, si l'unité avait 6 de biomasse, alors fillValue et maxFillValue seront tous deux égaux à 6.
                carcass.fillValue = currentBiomass;
                carcass.maxFillValue = currentBiomass;
            }
            else
            {
                Debug.LogWarning("Le prefab de carcasse n'a pas de script Carcass attaché !");
            }
            Debug.Log("Carcasse instanciée à : " + spawnPos);
        }

        Destroy(gameObject);
    }


    /// <summary>
    /// Méthode utilisée pour les herbivores afin de consommer une ressource sur la cellule.
    /// </summary>
    public void EatResource()
    {
        if (unitData.isHerbivore)
        {
            currentStomach++;
            Debug.Log(unitData.unitName + " mange : jauge d'estomac = " + currentStomach + "/3");

            // Si la jauge atteint 3, augmenter la biomasse et réinitialiser l'estomac
            if (currentStomach >= 3)
            {
                IncreaseBiomass(1);
                currentStomach = 0;
                Debug.Log(unitData.unitName + " a rempli son estomac et gagne 1 biomasse !");
            }
        }
    }

    /// <summary>
    /// Augmente la biomasse de l'unité (utilisée pour d'autres mécaniques) et affiche un message.
    /// </summary>
    public void IncreaseBiomass(int amount)
    {
        currentBiomass += amount;
        Debug.Log(unitData.unitName + " augmente sa biomasse de " + amount + ". Nouvelle biomasse: " + currentBiomass);
    }

    /// <summary>
    /// Déplace l'unité vers la position cible, en vérifiant les obstacles.
    /// Permet également de sélectionner l'unité si la position cible est la même que celle actuelle.
    /// </summary>
    public void MoveTo(Vector2Int targetPosition)
    {
        // Si l'unité est déjà sur la case cible, on affiche simplement le panneau d'action
        if (targetPosition == this.position)
        {
            if (gridManager.actionPanel != null)
            {
                gridManager.actionPanel.ShowPanel(this);
                Debug.Log("UnitActionPanel affiché pour l'unité à sa propre position.");
            }
            return;
        }

        if (hasActed)
        {
            Debug.Log("❌ Unité a déjà agi et ne peut plus bouger.");
            return;
        }
        if (isMoving || gridManager.GetCell(targetPosition) == null)
        {
            Debug.Log("❌ Déplacement impossible vers : " + targetPosition);
            return;
        }

        UnitController occupant = gridManager.GetUnitAtPosition(targetPosition);
        Carcass carcassAtTarget = gridManager.GetCarcassAtPosition(targetPosition);
        if ((occupant != null && occupant != this) || (carcassAtTarget != null))
        {
            Debug.Log("❌ Case occupée par " + (occupant != null ? occupant.unitData.unitName : "une carcasse"));
            return;
        }

        Debug.Log("🚀 Déplacement confirmé vers : " + targetPosition);
        List<GridCell> path = gridManager.FindPath(this, position, targetPosition);
        if (path != null && path.Count > 0)
        {
            Debug.Log("✅ Chemin trouvé, déplacement en cours... Longueur : " + path.Count);
            gridManager.ClearHighlightedTiles();
            StartCoroutine(MoveAlongPath(path));
        }
        else
        {
            Debug.Log("❌ Aucun chemin valide trouvé !");
        }
    }

    IEnumerator MoveAlongPath(List<GridCell> path)
    {
        Debug.Log("🚶 Déplacement en cours...");
        isMoving = true;
        if (path == null || path.Count == 0)
        {
            Debug.LogError("❌ Chemin vide !");
            isMoving = false;
            yield break;
        }
        foreach (GridCell cell in path)
        {
            Vector3 targetPos = gridManager.groundTilemap.GetCellCenterWorld((Vector3Int)cell.position);
            while (Vector3.Distance(transform.position, targetPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, 2f * Time.deltaTime);
                yield return null;
            }
            position = cell.position;
            SnapToGrid();
        }
        isMoving = false;
        Debug.Log("✅ Déplacement terminé !");

        if (gridManager.actionPanel != null)
        {
            gridManager.actionPanel.ShowPanel(this);
            Debug.Log("UnitActionPanel affiché via référence directe.");
        }
        else
        {
            Debug.Log("Pas de panneau d'action assigné dans GridManager !");
            gridManager.selectedUnit = null;
            gridManager.ClearHighlightedTiles();
        }
    }

    private void SnapToGrid()
    {
        Vector3 snappedPosition = gridManager.groundTilemap.GetCellCenterWorld((Vector3Int)position);
        transform.position = snappedPosition;
    }

    /// <summary>
    /// Marque l'unité comme ayant agi, change sa couleur, et nettoie la sélection.
    /// </summary>
    public void MarkAsWaiting()
    {
        hasActed = true;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.gray;
        }
        Debug.Log(unitData.unitName + " est maintenant épuisé (attente).");
        gridManager.selectedUnit = null;
        gridManager.ClearHighlightedTiles();
    }

    public void ResetVisuals()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }

    /// <summary>
    /// Indique si cette unité est alliée avec une autre, en comparant leurs équipes.
    /// </summary>
    public bool IsAllied(UnitController other)
    {
        return this.team == other.team;
    }
}


















