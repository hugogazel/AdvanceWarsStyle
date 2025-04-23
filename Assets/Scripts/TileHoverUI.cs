using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class TileHoverUI : MonoBehaviour
{
    [Header("UI Références")]
    public GameObject tileInfoPanel;         // Panel d'info en bas à gauche
    public TextMeshProUGUI unitNameText;       // Affiche le nom de l'unité
    public TextMeshProUGUI stomachText;        // Affiche la jauge d'estomac
    public TextMeshProUGUI biomassValueText;   // Affiche la biomasse actuelle
    public TextMeshProUGUI tileNameText;       // Affiche le nom du terrain
    public TextMeshProUGUI tileResourceText;   // Affiche les ressources du terrain
    public TextMeshProUGUI carcassText;        // Affiche les infos de la carcasse

    [Header("Références Scène")]
    public Camera mainCamera;
    public Tilemap environmentTilemap;   // Tuile d'environnement (forêt, montagne, etc.)
    public Tilemap groundTilemap;        // Tuile de sol
    public Tilemap obstacleTilemap;      // Tuile des obstacles
    public MapDataManager mapDataManager;

    [Header("Unit Detection")]
    public LayerMask unitLayerMask;  // Layer des unités

    private void Start()
    {
        if (tileInfoPanel)
            tileInfoPanel.SetActive(false);
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Update()
    {
        if (!tileInfoPanel || !mainCamera)
            return;

        // Convertir la position de la souris en coordonnées monde
        Vector3 mousePos = Input.mousePosition;
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
        worldPos.z = 0;

        // Détecter l'unité sous la souris
        UnitController hoveredUnit = GetUnitUnderMouse(worldPos);

        // Détecter la tuile sous la souris
        Vector3Int cellPos;
        TileBase tileBase = GetTileUnderMouse(worldPos, out cellPos);
        VariantTileData tileData = null;
        if (tileBase != null && mapDataManager != null)
            tileData = mapDataManager.GetTileData(tileBase);

        // Détecter une carcasse sur la cellule sous le curseur
        Carcass hoveredCarcass = null;
        Vector2Int gridPos = new Vector2Int(cellPos.x, cellPos.y);
        GridManager gm = FindObjectOfType<GridManager>();
        if (gm != null)
            hoveredCarcass = gm.GetCarcassAtPosition(gridPos);

        // Si aucune info n'est disponible, désactiver le panneau
        if (hoveredUnit == null && tileData == null && hoveredCarcass == null)
        {
            tileInfoPanel.SetActive(false);
            return;
        }
        tileInfoPanel.SetActive(true);

        // Mise à jour des infos d'unité
        if (hoveredUnit != null)
        {
            unitNameText.text = "Unité : " + hoveredUnit.unitData.unitName;
            stomachText.text = "Stomach : " + hoveredUnit.currentStomach + "/3";
            biomassValueText.text = "Biomass : " + hoveredUnit.currentBiomass;
        }
        else
        {
            unitNameText.text = "";
            stomachText.text = "";
            biomassValueText.text = "";
        }

        // Mise à jour des infos de la carcasse
        if (hoveredCarcass != null)
        {
            // Affichage sous la forme "Carcass : fillValue / maxFillValue"
            carcassText.text = "Carcass : " + hoveredCarcass.fillValue + " / " + hoveredCarcass.maxFillValue;
        }
        else
        {
            carcassText.text = "";
        }

        // Mise à jour des infos du terrain
        if (tileData != null)
        {
            tileNameText.text = "Terrain : " + tileData.terrainName;
            tileResourceText.text = "Ressources : " + tileData.resources;
        }
        else
        {
            tileNameText.text = "";
            tileResourceText.text = "";
        }
    }

    private UnitController GetUnitUnderMouse(Vector3 worldPos)
    {
        float radius = 0.2f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(worldPos, radius, unitLayerMask);
        if (hits.Length == 0)
            return null;
        Collider2D closest = hits[0];
        float minDist = Vector2.Distance(worldPos, closest.transform.position);
        for (int i = 1; i < hits.Length; i++)
        {
            float dist = Vector2.Distance(worldPos, hits[i].transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = hits[i];
            }
        }
        return closest.GetComponent<UnitController>();
    }

    private TileBase GetTileUnderMouse(Vector3 worldPos, out Vector3Int cellPos)
    {
        // Recherche dans l'environmentTilemap
        cellPos = environmentTilemap.WorldToCell(worldPos);
        TileBase tile = environmentTilemap.GetTile(cellPos);
        if (tile != null)
            return tile;

        // Recherche dans la groundTilemap
        if (groundTilemap != null)
        {
            cellPos = groundTilemap.WorldToCell(worldPos);
            tile = groundTilemap.GetTile(cellPos);
            if (tile != null)
                return tile;
        }

        // Recherche dans l'obstacleTilemap
        if (obstacleTilemap != null)
        {
            cellPos = obstacleTilemap.WorldToCell(worldPos);
            tile = obstacleTilemap.GetTile(cellPos);
            if (tile != null)
                return tile;
        }
        return null;
    }
}







