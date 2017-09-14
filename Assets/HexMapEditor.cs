using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour {

    public Color[] colors;

    public HexGrid hexGrid;

    private Color activeColor;
    int activeElevation;
    int activeWaterLevel;
    int activeUrbanLevel, activeFarmLevel, activePlantLevel;

    bool applyColor;
    bool applyElevation = true;
    bool applyWaterLevel = true;
    bool applyUrbanLevel = true;
    bool applyFarmLevel = true;
    bool applyPlantLevel = true;

    int brushSize;

    bool isDrag;
    HexDirection dragDirection;
    HexCell previousCell;
    HexCell previousPreviousCell;

    enum OptionalToggle
    {
        Ignore, Yes, No
    }

    OptionalToggle riverMode, roadMode;

    private void Awake()
    {
        SelectColor(0);    
    }

    void Update()
    {
        if (Input.GetMouseButton(0) &&
            !EventSystem.current.IsPointerOverGameObject())
        {
            HandleInput();
        } else
        {
            previousCell = null;
        }
    }

    void EditCells(HexCell center)
    {
        int centerX = center.coordinates.X;
        int centerZ = center.coordinates.Z;

        for (int r = 0, z = centerZ - brushSize; z <= centerZ; z++, r++)
        {
            for(int x = centerX - r; x <= centerX + brushSize; x++)
            {
                EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
            }
        }
        for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++)
        {
            for (int x = centerX - brushSize; x <= centerX + r; x++)
            {
                EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
            }
        }
    }

    void EditCell(HexCell cell)
    {
        if (cell)
        {
            if (applyColor)
            {
                cell.Color = activeColor;
            }
            if (applyElevation)
            {
                cell.Elevation = activeElevation;
            }
            if (applyWaterLevel)
            {
                cell.WaterLevel = activeWaterLevel;
            }
            if(riverMode == OptionalToggle.No)
            {
                cell.RemoveRiver();
            }
            if(roadMode == OptionalToggle.No)
            {
                cell.RemoveRoads();
            }
            if (applyUrbanLevel)
            {
                cell.UrbanLevel = activeUrbanLevel;
            }
            if(applyPlantLevel)
            {
                cell.PlantLevel = activePlantLevel;
            }
            if (applyFarmLevel)
            {
                cell.FarmLevel = activeFarmLevel;
            }
            if (isDrag)
            {
                HexCell otherCell = cell.GetNeighbor(dragDirection.Opposite());
                if (otherCell)
                {
                    if (riverMode == OptionalToggle.Yes)
                    {
                        otherCell.SetOutgoingRiver(dragDirection);
                    }
                    if(roadMode == OptionalToggle.Yes)
                    {
                        otherCell.AddRoad(dragDirection);
                    }
                }
            }
        }
    }

    void HandleInput()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit))
        {
            HexCell currentCell = hexGrid.GetCell(hit.point);
            //Do we have a previous cell (thus are dragging), is it not the current cell,
            //and finally is it not the one we dragged from earlier (reducing jitters)
            if(previousCell && previousCell != currentCell && currentCell != previousPreviousCell)
            {
                ValidateDrag(currentCell);
            } else
            {
                isDrag = false;
            }
            EditCells(currentCell);
            previousPreviousCell = previousCell;
            previousCell = currentCell;
        } else
        {
            previousPreviousCell = null;
            previousCell = null;
        }
    }

    void ValidateDrag (HexCell currentCell)
    {
        for (
            dragDirection = HexDirection.NE;
            dragDirection <= HexDirection.NW;
            dragDirection++)
        {
            if(previousCell.GetNeighbor(dragDirection) == currentCell)
            {
                isDrag = true;
                return;
            }
        }
        isDrag = false;
    }

    public void SelectColor (int index)
    {
        applyColor = index >= 0;
        if (applyColor)
        {
            activeColor = colors[index];
        }
    }

    public void SetElevation (float elevation)
    {
        activeElevation = (int)elevation;
    }

    public void SetApplyElevation(bool toggle)
    {
        applyElevation = toggle;
    }

    public void SetApplyWaterLevel (bool toggle)
    {
        applyWaterLevel = toggle;
    }

    public void SetWaterLevel (float level)
    {
        activeWaterLevel = (int)level;
    }

    public void SetBrushSize(float size)
    {
        brushSize = (int)size;
    }

    public void ShowUI (bool visible)
    {
        hexGrid.ShowUI(visible);
    }

    public void SetRiverMode (int mode)
    {
        riverMode = (OptionalToggle)mode;
    }

    public void SetRoadMode (int mode)
    {
        roadMode = (OptionalToggle)mode;
    }

    public void SetApplyUrbanLevel (bool toggle)
    {
        applyUrbanLevel = toggle;
    }

    public void SetUrbanLevel (float level)
    {
        activeUrbanLevel = (int)level;
    }

    public void SetApplyFarmLevel (bool toggle)
    {
        applyFarmLevel = toggle;
    }

    public void SetFarmLevel (float level)
    {
        activeFarmLevel = (int)level;
    }

    public void SetApplyPlantLevel (bool toggle)
    {
        applyPlantLevel = toggle;
    }

    public void SetPlantLevel (float level)
    {
        activePlantLevel = (int)level;
    }
}
