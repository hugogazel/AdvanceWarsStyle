using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UnitActionPanel : MonoBehaviour
{
    public Button waitButton;
    public Button attackButton;
    public Button eatButton;
    public Button cancelButton;           // ← Nouveau bouton

    public TextMeshProUGUI stomachText;
    private UnitController currentUnit;

    public void ShowPanel(UnitController unit)
    {
        currentUnit = unit;
        transform.SetAsLastSibling();
        gameObject.SetActive(true);

        // — Attache le listener du Cancel, et rend le bouton visible
        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(OnCancelButton);
        cancelButton.gameObject.SetActive(true);

        // … configuration des autres boutons …
        bool canAttack = CheckIfAttackPossible(unit);
        attackButton.interactable = canAttack;

        GridManager gm = FindObjectOfType<GridManager>();
        if (unit.unitData.isPredator)
        {
            bool foundCarcass = false;
            foreach (GridCell cell in gm.GetNeighbors(unit.position))
            {
                if (gm.GetCarcassAtPosition(cell.position) != null)
                {
                    foundCarcass = true; break;
                }
            }
            eatButton.interactable = foundCarcass;
        }
        else if (unit.unitData.isHerbivore || unit.unitData.isOmnivore)
        {
            var cell = gm.GetCell(unit.position);
            eatButton.interactable = (cell != null && cell.resources > 0);
        }
        else
        {
            eatButton.interactable = false;
        }
    }

    public void HidePanel()
    {
        // Nettoie le listener du Cancel et masquage
        cancelButton.onClick.RemoveAllListeners();
        cancelButton.gameObject.SetActive(false);

        gameObject.SetActive(false);
        currentUnit = null;
    }

    public void OnWaitButton()
    {
        currentUnit?.MarkAsWaiting();
        HidePanel();
    }

    public void OnAttackButton()
    {
        if (currentUnit != null)
        {
            UnitController enemy = GetFirstAdjacentEnemy(currentUnit);
            if (enemy != null && FindObjectOfType<CombatSystem>().Attack(currentUnit, enemy))
                currentUnit.MarkAsWaiting();
        }
        HidePanel();
    }

    public void OnEatButton()
    {
        if (currentUnit != null)
        {
            GridManager gm = FindObjectOfType<GridManager>();
            if (currentUnit.unitData.isPredator)
            {
                // … logique Eat carnivore …
            }
            else
            {
                var cell = gm.GetCell(currentUnit.position);
                if (cell != null)
                {
                    gm.ConsumeTileResources(currentUnit.position, 1);
                    currentUnit.EatResource();
                    currentUnit.MarkAsWaiting();
                }
            }
        }
        HidePanel();
    }

    private void OnCancelButton()
    {
        currentUnit?.CancelMove();
        HidePanel();
    }

    private bool CheckIfAttackPossible(UnitController unit)
    {
        if (unit == null || !unit.unitData.isPredator) return false;
        var gm = FindObjectOfType<GridManager>();
        foreach (var cell in gm.GetNeighbors(unit.position))
        {
            var u = gm.GetUnitAtPosition(cell.position);
            if (u != null && !unit.IsAllied(u))
                return true;
        }
        return false;
    }

    private UnitController GetFirstAdjacentEnemy(UnitController unit)
    {
        var gm = FindObjectOfType<GridManager>();
        foreach (var cell in gm.GetNeighbors(unit.position))
        {
            var u = gm.GetUnitAtPosition(cell.position);
            if (u != null && !unit.IsAllied(u))
                return u;
        }
        return null;
    }

    void Update()
    {
        if (gameObject.activeSelf && currentUnit != null && stomachText != null)
        {
            stomachText.text = "Stomach : " + currentUnit.currentStomach + "/3";
        }
    }
}



















