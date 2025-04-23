using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UnitActionPanel : MonoBehaviour
{
    public Button waitButton;
    public Button attackButton;
    public Button eatButton; // Bouton Eat

    public TextMeshProUGUI stomachText; // Champ pour afficher la jauge d'estomac

    private UnitController currentUnit;

    public void ShowPanel(UnitController unit)
    {
        Debug.Log("ShowPanel() appelé pour " + unit.unitData.unitName);
        currentUnit = unit;
        transform.SetAsLastSibling();
        gameObject.SetActive(true);

        bool canAttack = CheckIfAttackPossible(unit);
        attackButton.interactable = canAttack;

        GridManager gm = FindObjectOfType<GridManager>();
        // Pour les prédateurs, activer le bouton Eat si une carcasse est présente dans une cellule adjacente
        if (unit.unitData.isPredator)
        {
            bool foundCarcass = false;
            if (gm != null)
            {
                List<GridCell> neighbors = gm.GetNeighbors(unit.position);
                foreach (GridCell cell in neighbors)
                {
                    Carcass carcass = gm.GetCarcassAtPosition(cell.position);
                    if (carcass != null)
                    {
                        foundCarcass = true;
                        break;
                    }
                }
            }
            eatButton.interactable = foundCarcass;
        }
        // Pour herbivores et omnivores, activer le bouton Eat s'il y a des ressources sur la cellule
        else if (unit.unitData.isHerbivore || unit.unitData.isOmnivore)
        {
            if (gm != null)
            {
                GridCell cell = gm.GetCell(unit.position);
                eatButton.interactable = (cell != null && cell.resources > 0);
            }
            else
            {
                eatButton.interactable = false;
            }
        }
        else
        {
            eatButton.interactable = false;
        }

        Debug.Log("Panneau affiché. canAttack = " + canAttack);
    }

    public void HidePanel()
    {
        Debug.Log("HidePanel() appelé");
        gameObject.SetActive(false);
        currentUnit = null;
    }

    // Mise à jour dynamique du texte de la jauge d'estomac
    void Update()
    {
        if (gameObject.activeSelf && currentUnit != null && stomachText != null)
        {
            stomachText.text = "Stomach : " + currentUnit.currentStomach + "/3";
            stomachText.gameObject.SetActive(true);
        }
    }

    public void OnWaitButton()
    {
        Debug.Log("OnWaitButton cliqué");
        if (currentUnit != null)
        {
            currentUnit.MarkAsWaiting();
        }
        HidePanel();
    }

    public void OnAttackButton()
    {
        Debug.Log("OnAttackButton cliqué");
        if (currentUnit != null)
        {
            UnitController enemyTarget = GetFirstAdjacentEnemy(currentUnit);
            if (enemyTarget != null)
            {
                Debug.Log("Cible d'attaque trouvée : " + enemyTarget.unitData.unitName);
                bool success = FindObjectOfType<CombatSystem>().Attack(currentUnit, enemyTarget);
                if (success)
                {
                    Debug.Log("Attaque réussie via bouton !");
                    currentUnit.MarkAsWaiting();
                }
                else
                {
                    Debug.Log("Attaque échouée.");
                }
            }
            else
            {
                Debug.Log("Aucun ennemi adjacent pour attaquer.");
            }
        }
        HidePanel();
    }

    public void OnEatButton()
    {
        Debug.Log("OnEatButton cliqué pour " + (currentUnit != null ? currentUnit.unitData.unitName : "aucune unité"));
        if (currentUnit != null)
        {
            GridManager gm = FindObjectOfType<GridManager>();
            // Pour les prédateurs, on veut incrémenter le compteur d'estomac et n'augmenter la biomasse
            // que lorsque le compteur atteint 3.
            if (currentUnit.unitData.isPredator)
            {
                bool foundCarcass = false;
                if (gm != null)
                {
                    List<GridCell> neighbors = gm.GetNeighbors(currentUnit.position);
                    foreach (GridCell cell in neighbors)
                    {
                        Carcass carcass = gm.GetCarcassAtPosition(cell.position);
                        if (carcass != null)
                        {
                            // Calculer la quantité nécessaire pour remplir une unité d'estomac.
                            // maxFillValue représente la valeur initiale de la carcasse,
                            // et on considère qu'il faut maxFillValue/3 pour remplir 1 "stomach unit".
                            int portionNeeded = Mathf.Max(1, Mathf.RoundToInt(carcass.maxFillValue / 3.0f));
                            // Consommer au maximum cette quantité, ou moins si la carcasse a moins.
                            int portionConsumed = Mathf.Min(portionNeeded, carcass.fillValue);
                            Debug.Log("Carcasse trouvée à " + cell.position + " avec fillValue: " + carcass.fillValue +
                                      ", portion consommée: " + portionConsumed);
                            // Incrémente le compteur d'estomac du prédateur d'une unité.
                            currentUnit.currentStomach++;
                            Debug.Log(currentUnit.unitData.unitName + " mange : jauge d'estomac = " + currentUnit.currentStomach + "/3");
                            // Si la jauge atteint 3, augmenter la biomasse de 1 et réinitialiser la jauge.
                            if (currentUnit.currentStomach >= 3)
                            {
                                currentUnit.IncreaseBiomass(1);
                                currentUnit.currentStomach = 0;
                                Debug.Log(currentUnit.unitData.unitName + " a rempli son estomac et gagne 1 biomasse !");
                            }
                            // Réduire la fillValue de la carcasse de la portion consommée.
                            carcass.fillValue -= portionConsumed;
                            if (carcass.fillValue <= 0)
                            {
                                Destroy(carcass.gameObject);
                                Debug.Log("La carcasse a disparu.");
                            }
                            else
                            {
                                Debug.Log("Carcasse restante: " + carcass.fillValue);
                            }
                            foundCarcass = true;
                            break;
                        }
                    }
                }
                if (!foundCarcass)
                {
                    Debug.Log("Aucune carcasse trouvée à proximité pour être mangée.");
                }
                currentUnit.MarkAsWaiting();
            }
            // Pour herbivores et omnivores, consommer les ressources de la cellule
            else if (currentUnit.unitData.isHerbivore || currentUnit.unitData.isOmnivore)
            {
                if (gm != null)
                {
                    GridCell cell = gm.GetCell(currentUnit.position);
                    if (cell != null)
                    {
                        Debug.Log("Avant consommation : ressources = " + cell.resources + " sur la cellule " + currentUnit.position);
                        gm.ConsumeTileResources(currentUnit.position, 1);
                        Debug.Log("Après consommation : ressources = " + cell.resources + " sur la cellule " + currentUnit.position);
                        // Pour herbivores, appeler la méthode EatResource qui gère l'estomac et la biomasse.
                        currentUnit.EatResource();
                        currentUnit.MarkAsWaiting();
                    }
                    else
                    {
                        Debug.Log("Aucune cellule trouvée pour la position " + currentUnit.position);
                    }
                }
            }
        }
        HidePanel();
    }

    private bool CheckIfAttackPossible(UnitController unit)
    {
        Debug.Log("CheckIfAttackPossible appelé pour " + unit.unitData.unitName);
        if (unit == null) return false;
        if (!unit.unitData.isPredator)
        {
            Debug.Log("L'unité n'est pas un prédateur, attaque impossible.");
            return false;
        }
        GridManager gridManager = FindObjectOfType<GridManager>();
        if (gridManager == null) return false;
        var neighbors = gridManager.GetNeighbors(unit.position);
        foreach (var cell in neighbors)
        {
            UnitController occupant = gridManager.GetUnitAtPosition(cell.position);
            if (occupant != null && !unit.IsAllied(occupant))
            {
                Debug.Log("Ennemi détecté à " + cell.position);
                return true;
            }
        }
        Debug.Log("Aucun ennemi détecté à proximité de " + unit.unitData.unitName);
        return false;
    }

    private UnitController GetFirstAdjacentEnemy(UnitController unit)
    {
        GridManager gridManager = FindObjectOfType<GridManager>();
        if (gridManager == null) return null;
        var neighbors = gridManager.GetNeighbors(unit.position);
        foreach (var cell in neighbors)
        {
            UnitController occupant = gridManager.GetUnitAtPosition(cell.position);
            if (occupant != null && !unit.IsAllied(occupant))
            {
                return occupant;
            }
        }
        return null;
    }
}


















