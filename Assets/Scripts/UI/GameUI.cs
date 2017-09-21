using UnityEngine;
using UnityEngine.EventSystems;

public class GameUI : MonoBehaviour {
    public HexGrid hexGrid;

    HexCell currentCell;

    HexUnit selectedUnit;

    void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButtonDown(0))
            {
                DoSelection();
            } else if (selectedUnit)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    DoMove();
                }
                else
                {
                    DoPathfinding();
                }
            }
        }    
    } 

    public void SetEditMode (bool toggle)
    {
        enabled = !toggle;
        hexGrid.ShowUI(!toggle);
        hexGrid.ClearPath();
    }

    bool UpdateCurrentCell ()
    {
        HexCell cell = hexGrid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));

        if (cell != currentCell)
        {
            currentCell = cell;
            return true;
        }
        return false;
    }

    void DoSelection()
    {
        hexGrid.ClearPath();
        UpdateCurrentCell();
        if (currentCell)
        {
            selectedUnit = currentCell.Unit;
        }
    }

    void DoPathfinding()
    {
        if(UpdateCurrentCell())
        {
            if (currentCell && selectedUnit.IsValidDestination(currentCell))
            {
                hexGrid.FindPath(selectedUnit.Location, currentCell, HexPathMetrics.testSpeed);
            } else
            {
                hexGrid.ClearPath();
            }
        }
    }

    void DoMove()
    {
        if (hexGrid.HasPath)
        {
            selectedUnit.Travel(hexGrid.GetPath());
            hexGrid.ClearPath();
        }
    }

}
