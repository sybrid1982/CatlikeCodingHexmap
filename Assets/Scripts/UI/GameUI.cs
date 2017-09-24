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
        if(toggle)
        {
            Shader.EnableKeyword("HEX_MAP_EDIT_MODE");
        } else
        {
            Shader.DisableKeyword("HEX_MAP_EDIT_MODE");
        }
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
                hexGrid.FindPath(selectedUnit.Location, currentCell, selectedUnit);
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

    // This is a debug function that could be adjusted for use in a game
    
    public void RefreshVision()
    {
        // Set everyone's vision blank
        int z = 0;
        for(int i = 0; i < hexGrid.cellCountX * hexGrid.cellCountZ; i++)
        {
            int x = i % hexGrid.cellCountZ;
            if(i > 0 && x == 0)
            {
                z++;
            }
            HexCoordinates coords = new HexCoordinates(x, z);
            HexCell cell = hexGrid.GetCell(coords);
            cell.ResetVisibility();
        }
        // Tell the hexgrid to tell each unit to refresh its vision
        hexGrid.RefreshAllUnitsVision();
    }
}
