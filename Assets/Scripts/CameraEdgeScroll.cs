using UnityEngine;

public class CameraEdgeScroll : MonoBehaviour
{
    public float scrollSpeed = 5f;     // Vitesse de déplacement
    public float edgeThreshold = 10f; // Distance en pixels depuis le bord

    // Limites de la caméra dans le monde (pour ne pas sortir de la carte)
    public float minX = -10f, maxX = 10f;
    public float minY = -5f, maxY = 5f;

    void Update()
    {
        Vector3 pos = transform.position;

        // Récupérer la position de la souris en pixels (0 -> Screen.width/height)
        float mouseX = Input.mousePosition.x;
        float mouseY = Input.mousePosition.y;

        // Déplacement horizontal
        if (mouseX < edgeThreshold)
        {
            pos.x -= scrollSpeed * Time.deltaTime;
        }
        else if (mouseX > Screen.width - edgeThreshold)
        {
            pos.x += scrollSpeed * Time.deltaTime;
        }

        // Déplacement vertical
        if (mouseY < edgeThreshold)
        {
            pos.y -= scrollSpeed * Time.deltaTime;
        }
        else if (mouseY > Screen.height - edgeThreshold)
        {
            pos.y += scrollSpeed * Time.deltaTime;
        }

        // On “clamp” la position pour ne pas sortir de la map
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        // Appliquer la nouvelle position à la caméra
        transform.position = pos;
    }
}

