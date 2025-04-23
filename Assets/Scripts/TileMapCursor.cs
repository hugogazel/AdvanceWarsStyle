using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapCursor : MonoBehaviour
{
    public Tilemap tilemap;  // Référence au Tilemap à suivre
    public Camera mainCamera; // Référence à la caméra principale

    void Update()
    {
        // Récupérer la position de la souris en monde
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0; // Garder le curseur en 2D

        // Convertir en position de cellule du Tilemap
        Vector3Int cellPosition = tilemap.WorldToCell(mouseWorldPos);

        // Placer le curseur au centre de la cellule
        transform.position = tilemap.GetCellCenterWorld(cellPosition);
    }
}


