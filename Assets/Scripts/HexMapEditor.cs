using UnityEngine;
using UnityEngine.EventSystems;
using System.IO;

public class HexMapEditor : MonoBehaviour {

    public HexGrid hexGrid;
    
    int activeElevation;
    int activeWaterLevel;
    int activeUrbanLevel, activeFarmLevel, activePlantLevel, activeSpecialIndex;
    int activeTerrainTypeIndex;

    bool applyElevation = true;
    bool applyWaterLevel = true;
    bool applyUrbanLevel = true;
    bool applyFarmLevel = true;
    bool applyPlantLevel = true;
    bool applySpecialIndex;

    int brushSize;

    bool isDrag;
    HexDirection dragDirection;
    HexCell previousCell;
    HexCell previousPreviousCell;

    enum OptionalToggle
    {
        Ignore, Yes, No
    }

    OptionalToggle riverMode, roadMode, walledMode;

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
            if(activeTerrainTypeIndex >= 0)
            {
                cell.TerrainTypeIndex = activeTerrainTypeIndex;
            }
            if (applyElevation)
            {
                cell.Elevation = activeElevation;
            }
            if (applyWaterLevel)
            {
                cell.WaterLevel = activeWaterLevel;
            }
            if (applySpecialIndex)
            {
                cell.SpecialIndex = activeSpecialIndex;
            }
            if(riverMode == OptionalToggle.No)
            {
                cell.RemoveRiver();
            }
            if(roadMode == OptionalToggle.No)
            {
                cell.RemoveRoads();
            }
            if(walledMode != OptionalToggle.Ignore)
            {
                cell.Walled = walledMode == OptionalToggle.Yes;
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

    public void SetTerrainTypeIndex(int index)
    {
        activeTerrainTypeIndex = index;
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

    public void SetWalledMode (int mode)
    {
        walledMode = (OptionalToggle)mode;
    }

    public void SetApplySpecialIndex(bool toggle)
    {
        applySpecialIndex = toggle;
    }

    public void SetSpecialIndex(float level)
    {
        activeSpecialIndex = (int)level;
    }

    ////////////////////////////
    //Saving and Loading
    ////////////////////////////
    public void Save()
    {
        string path = Path.Combine(Application.persistentDataPath, "test.map");
        using (
            BinaryWriter writer =
                new BinaryWriter(File.Open(path, FileMode.Create))
                )
        {
            writer.Write(0);
            hexGrid.Save(writer);
        }
    }
    public void Load()
    {
        string path = Path.Combine(Application.persistentDataPath, "test.map");
        using (
            BinaryReader reader =
                new BinaryReader(File.OpenRead(path)))
        {
            int header = reader.ReadInt32();
            
            if (header == 0)
            {
                hexGrid.Load(reader);
            } else
            {
                Debug.LogWarning("Unknown Map Format " + header);
            }
            
        }
    }
}
