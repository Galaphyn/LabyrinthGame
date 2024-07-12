using Godot;
using System;
using System.Collections.Generic;

//Each cell containing if they have a wall to the right of bottom, and which set it is in.
public class Cell
{
    public Set Set;
    public bool RightWall;
    public bool BottomWall;
    public bool SpecialStructure;
}

public class Set
{
    public List<Cell> Cells = new List<Cell>();
}

public partial class labyrinth_generator : TileMap
{
    private List<Set> sets; //List of sets in a row
    private List<Cell> cells; //List of cells in a row
    private Cell[,] maze; //2D array of all cells for maze
    private int bias = 32; //Basically the seed for generation?

    // Maze will be Row x Column in size.
    public int NumRow; //Number of rows of Cells
    public int NumCol; //Number of columns of cells

    private Random rand = new Random();

    public override void _Ready()
    {
        //GenerateMaze();
        //PrintMaze();
        //BuildMaze();
        //UpdateWallConnections();
    }

    public void GenerateMaze(int col, int row)
    {
        NumRow = 30;
        NumCol = 30;

        sets = new List<Set>();
        cells = new List<Cell>();
        maze = new Cell[NumCol, NumRow];

        for (int i = 0; i < NumCol; i++)
        {
            cells.Add(new Cell());
        }

        for (int i = 0; i < NumRow - 1; i++)
        {
            BuildRow(i);
        }
        BuildBottomRow();

        BuildMaze();
        UpdateWallConnections();
    }

    private void BuildRow(int curRow)
    {
        InitializeSets(); //Maybe remove from this and bottomrow, adding instead to the previous for loop
        for (int i = 0; i < cells.Count - 1; i++)
        {
            if (cells[i].Set == cells[i + 1].Set)
            {
                cells[i].RightWall = true;
            }
        }
        GenerateRightWall();
        GenerateBottomWall();
        UpdateMazeRow(curRow);
        NextRow();
    }

    private void NextRow()
    {
        foreach (Cell cell in cells)
        {
            cell.RightWall = false;
            if (cell.BottomWall)
            {
                cell.Set.Cells.Remove(cell);
                cell.Set = null;
                cell.BottomWall = false;
            }
        }
    }

    private void BuildBottomRow()
    {
        InitializeSets();

        //Since this is bottom, beeg bottom wall (BBW, if you will)
        foreach (Cell cell in cells)
        {
            cell.BottomWall = true;
        }

        for (int i = 0; i < cells.Count - 1; i++)
        {
            if (cells[i].Set != cells[i + 1].Set)
            {
                cells[i].RightWall = false;
            }
            else
            {
                cells[i].RightWall = true;
            }
        }
        //Ensure rightmost cell in row has right wall
        cells[cells.Count - 1].RightWall = true;
        UpdateMazeRow(NumRow - 1);
    }

    //Initialize Sets for empty cells
    private void InitializeSets()
    {
        foreach (Cell cell in cells)
        {
            if (cell.Set == null)
            {
                Set set = new Set();
                cell.Set = set;
                set.Cells.Add(cell);
                sets.Add(set);
            }
        }
    }

    private void GenerateRightWall()
    {
        for (int i = 0; i < cells.Count - 1; ++i)
        {
            int maxBias = 64;
            int bias = 32;
            int x = rand.Next(0, maxBias + 1);
            if (x > bias)
            {
                cells[i].RightWall = true;
            }
            else if (cells[i].Set == cells[i + 1].Set)
            {
                cells[i].RightWall = true;
            }
            else
            {
                MergeSets(i, i + 1);
            }
        }
        cells[cells.Count - 1].RightWall = true;
    }

    private void GenerateBottomWall()
    {
        foreach (Set set in sets.ToArray())
        {
            if (set.Cells.Count > 0)
            {
                List<int> cellIndices = new List<int>();

                if (set.Cells.Count == 1)
                {
                    cellIndices.Add(0);
                }
                else
                {
                    int x = rand.Next(1, set.Cells.Count + 1);
                    for (int i = 0; i < x; i++)
                    {
                        int index;
                        do
                        {
                            index = rand.Next(0, set.Cells.Count);
                        } while (cellIndices.Contains(index));
                        cellIndices.Add(index);
                    }
                }

                for (int i = 0; i < set.Cells.Count; i++)
                {
                    if (cellIndices.Contains(i))
                    {
                        set.Cells[i].BottomWall = false;
                    }
                    else
                    {
                        set.Cells[i].BottomWall = true;
                    }
                }
            }
            else
            {
                sets.Remove(set);
            }
        }
    }

    private void MergeSets(int set1, int set2)
    {
        cells[set2].Set.Cells.Remove(cells[set2]);
        cells[set1].Set.Cells.Add(cells[set2]);
        cells[set2].Set = cells[set1].Set;
    }

    private void UpdateMazeRow(int row)
    {
        for (int i = 0; i < cells.Count; i++)
        {
            Cell cell = new Cell()
            {
                RightWall = cells[i].RightWall,
                BottomWall = cells[i].BottomWall,
                Set = cells[i].Set
            };
            maze[i, row] = cell;
        }
    }

    private void BuildMaze()
    {
        Vector2I textureAtlas = new Vector2I(0, 0);
        for (int i = 0; i < NumRow; i++)
        {
            for (int j = 0; j < NumCol; j++)
            {
                Vector2I cellPosition = new Vector2I(j * 2 + 1, i * 2 + 1);
                if (maze[j, i].RightWall)
                {
                    SetCell(0, cellPosition + new Vector2I(1, 0), 0, textureAtlas);  // Place right wall tile
                    SetCell(0, cellPosition + new Vector2I(1, 1), 0, textureAtlas);
                    SetCell(0, cellPosition + new Vector2I(1, -1), 0, textureAtlas);
                }

                if (maze[j, i].BottomWall)
                {
                    SetCell(0, cellPosition + new Vector2I(0, 1), 0, textureAtlas);  // Place bottom wall tile
                    SetCell(0, cellPosition + new Vector2I(1, 1), 0, textureAtlas);
                    SetCell(0, cellPosition + new Vector2I(-1, 1), 0, textureAtlas);
                }
            }
        }

        // Surround the entire maze with walls
        for (int i = 0; i < NumRow * 2 + 1; i++)
        {
            SetCell(0, new Vector2I(0, i), 0, textureAtlas); ; // Left boundary
            SetCell(0, new Vector2I(NumCol * 2, i), 0, textureAtlas); ; // Right boundary
        }

        for (int j = 0; j < NumCol * 2 + 1; j++)
        {
            SetCell(0, new Vector2I(j, 0), 0, textureAtlas); ; // Top boundary
            SetCell(0, new Vector2I(j, NumRow * 2), 0, textureAtlas); ; // Bottom boundary
        }
    }

    private void PrintMaze()
    {
        for (int i = 0; i < NumRow; i++)
        {
            for (int j = 0; j < NumCol; j++)
            {
                GD.Print($"Cell[{j}, {i}] - RightWall: {maze[j, i].RightWall}, BottomWall: {maze[j, i].BottomWall}");
            }
        }
    }

    private bool IsWallAt(Vector2I position)
    {
        return GetCellSourceId(0, position) != -1; // Assuming -1 means no tile
    }

    private int GetWallBitmask(Vector2I position)
    {
        int bitmask = 0;

        // Check for walls in each direction (left, right, up, down)
        if (IsWallAt(position + new Vector2I(-1, 0))) bitmask |= 1; // Left
        if (IsWallAt(position + new Vector2I(1, 0))) bitmask |= 2;  // Right
        if (IsWallAt(position + new Vector2I(0, -1))) bitmask |= 4; // Up
        if (IsWallAt(position + new Vector2I(0, 1))) bitmask |= 8;  // Down

        return bitmask;
    }

    //Map of wall tiles based on their connections from GetWallBitmask.
    private Dictionary<int, Vector2I> bitmaskToTileId = new Dictionary<int, Vector2I>
    {
        { 0, new Vector2I(0, 0) }, // No neighbors
        { 1, new Vector2I(3, 0) }, // Left
        { 2, new Vector2I(1, 0) }, // Right
        { 3, new Vector2I(2, 0) }, // Left, Right
        { 4, new Vector2I(0, 3) }, // Up
        { 5, new Vector2I(3, 3) }, // Left, Up
        { 6, new Vector2I(1, 3) }, // Right, Up
        { 7, new Vector2I(2, 3) }, // Left, Right, Up
        { 8, new Vector2I(0, 1) }, // Down
        { 9, new Vector2I(3, 1) }, // Left, Down
        { 10, new Vector2I(1, 1) }, // Right, Down
        { 11, new Vector2I(2, 1) }, // Left, Right, Down
        { 12, new Vector2I(0, 2) }, // Up, Down
        { 13, new Vector2I(3, 2) }, // Left, Up, Down
        { 14, new Vector2I(1, 2) }, // Right, Up, Down
        { 15, new Vector2I(2, 2) }, // All sides
    };


    private void UpdateWallConnections()
    {
        for (int i = 0; i < NumRow * 2 + 1; i++)
        {
            for (int j = 0; j < NumCol * 2 + 1; j++)
            {
                Vector2I cellPosition = new Vector2I(j, i);
                if (IsWallAt(cellPosition))
                {
                    int bitmask = GetWallBitmask(cellPosition);
                    Vector2I tileId = bitmaskToTileId[bitmask]; // Get the tile ID based on the bitmask
                    SetCell(0, cellPosition, 0, tileId);
                }
            }
        }
    }
}
