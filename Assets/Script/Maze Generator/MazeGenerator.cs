using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;

public class MazeGenerator : MonoBehaviour
{
    public int mazeWidth = 10;
    public int mazeHeight = 10;
    public int minCorridorLength = 1;
    public int maxCorridorLength = 5;
    public GameObject wallPrefab;
    private int[,] mazeGrid;
    private Vector2Int startPoint;
    private Vector2Int exitPoint;
    private bool pathFound = false;
    private int gridWidth;
    private int gridHeight;
    private Stack<Vector2Int> pathStack;
    private List<PathNode> pathList = new List<PathNode>();


    public void GenerateMaze(Vector2Int? entryPoint)
    {
        pathList = new List<PathNode>();
        // Make it 1 cell smaller so that you can generate the corridors on all sides
        gridWidth = mazeWidth;
        gridHeight = mazeHeight;

        mazeGrid = new int[gridWidth, gridHeight];

        while (!pathFound || !IsAtEdge(exitPoint))
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    mazeGrid[x, y] = 0; // Initialize with walls
                }
            }

            Vector2Int startCell = entryPoint ?? new Vector2Int(0, Random.Range(0, gridHeight));
            pathFound = false; // Reset pathFound flag
            GeneratePath(startCell);
        }

        // Transform back to original size
        DrawMaze();
    }

    public Vector2Int GetStartPoint()
    {
        return startPoint;
    }

    public Vector2Int GetExitPoint()
    {
        return exitPoint;
    }

    public List<PathNode> GetPathway() {
        return pathList;
    }

    private void GeneratePath(Vector2Int startCell)
    {
        startPoint = startCell;
        Vector2Int current = startCell;
        pathStack = new Stack<Vector2Int>();
        mazeGrid[current.x, current.y] = 1;
        pathStack.Push(current);

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (pathStack.Count > 0)
        {
            List<Vector2Int> validDirections = new List<Vector2Int>();
            directions.Shuffle();

            int corridorLength = Random.Range(minCorridorLength, maxCorridorLength);

            validDirections.AddRange(directions.Where(dir => IsValidCorridor(current, dir, corridorLength)));

            if (validDirections.Count == 0)
            {
                if (IsAtEdge(current) && !IsExitSameSideAsEntrance(current)){
                    exitPoint = current;
                    pathFound = true;
                    return;
                }
                else if (!IsAtEdge(current)){
                    Vector2Int closestEdge = current;
                    float minDistance = float.MaxValue;
                    foreach (Vector2Int dir in directions)
                    {
                        Vector2Int edgeCell = current;
                        while (IsValidCell(edgeCell) && !IsAtEdge(edgeCell))
                        {
                            edgeCell += dir;
                        }
                        if (IsValidCell(edgeCell) && !IsExitSameSideAsEntrance(edgeCell))
                        {
                            float distance = Vector2Int.Distance(current, edgeCell);
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                closestEdge = edgeCell;
                            }
                        }
                    }
                    if (closestEdge != current && IsValidCorridor(current, closestEdge)){
                        Vector2Int direction = GetDirection(current, closestEdge);
                        int length = (closestEdge - current).x != 0 ? Mathf.Abs((closestEdge - current).x) : Mathf.Abs((closestEdge - current).y);
                        for (int i = 0; i <= length; i++)
                        {
                            Vector2Int corridorCell = current + direction * i;
                            if (IsValidCell(corridorCell))
                            {
                                mazeGrid[corridorCell.x, corridorCell.y] = 1;
                                pathStack.Push(corridorCell);
                            }
                        }
                        exitPoint = closestEdge;
                        pathFound = true;
                        return;
                    }

                }

                while (pathStack.Count > 0)
                {
                    mazeGrid[current.x, current.y] = 0;
                    pathStack.Pop();
                    if (pathStack.Count > 0)
                    {
                        current = pathStack.Peek();
                    }
                }
                continue;
            }

            Vector2Int chosenDir = validDirections[Random.Range(0, validDirections.Count)];
            Vector2Int nextCell = current + chosenDir * corridorLength;

            if (!IsValidCell(nextCell))
            {
                continue;
            }

            for (int i = 0; i <= corridorLength; i++)
            {
                Vector2Int corridorCell = current + chosenDir * i;
                if (IsValidCell(corridorCell))
                {
                    // Add to Pathway
                    mazeGrid[corridorCell.x, corridorCell.y] = 1;
                    pathStack.Push(corridorCell);
                }
            }
            current = nextCell;
        }
    }

    private Vector2 GridToWorldSpace(Vector2Int cell){
        return (Vector2) transform.position + cell;
    }

    private bool IsExitSameSideAsEntrance(Vector2Int cell) {
        return cell.x == startPoint.x || cell.y == startPoint.y;
    }

    // Ensure the whole corridor is valid (not just the last cell)
    private bool IsValidCorridor(Vector2Int start, Vector2Int direction, int length)
    {
        for (int i = 1; i <= length; i++)
        {
            Vector2Int checkCell = start + direction * i;
            if (!IsValidCell(checkCell) || mazeGrid[checkCell.x, checkCell.y] == 1)
            {
                return false;
            }
        }
        return true;
    }

    private bool IsValidCorridor(Vector2Int start, Vector2Int end)
    {
        Vector2Int direction = GetDirection(start, end);

        int length = (end - start).x != 0 ? Mathf.Abs((end - start).x) : Mathf.Abs((end - start).y);

        for (int i = 1; i <= length; i++)
        {
            Vector2Int checkCell = start + direction * i;
            if (!IsValidCell(checkCell) || mazeGrid[checkCell.x, checkCell.y] == 1)
            {
                return false;
            }
        }
        return true;
    }

    private Vector2Int GetDirection(Vector2Int start, Vector2Int end){
        Vector2Int direction = end - start;

        if (direction.x > 0) return Vector2Int.right;
        if (direction.x < 0) return Vector2Int.left;
        if (direction.y > 0) return Vector2Int.up;
        return Vector2Int.down;
    }

    // Ensures exit is always on an edge
    private bool IsAtEdge(Vector2Int cell)
    {
        return cell.x == 0 || cell.x == gridWidth - 1 || cell.y == 0 || cell.y == gridHeight - 1;
    }

    private bool IsValidCell(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < gridWidth && cell.y >= 0 && cell.y < gridHeight;
    }

    private void DrawMaze()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (mazeGrid[x, y] == 0)
                {
                    Vector3 position = transform.position + new Vector3(x, y, 0);
                    Instantiate(wallPrefab, position, Quaternion.identity, transform);
                }
            }
        }

        GenerateInputPathway();
    }

    private void GenerateInputPathway(){
        Vector2Int[] pathArray = pathStack.ToArray();
        pathStack.Reverse();

        HashSet<Vector2Int> seen = new HashSet<Vector2Int>();
        List<Vector2Int> result = new List<Vector2Int>();

        for (int i = pathArray.Length - 1; i >= 0; i--) {
            if (seen.Add(pathArray[i])){
                result.Add(pathArray[i]);
            }
        }

        pathArray = result.ToArray();

        // Skip First Cell as you will start there
        Vector2 lastPos = LocalToWorldSpace(pathArray[0]);
        Vector2 lastDir = Vector2.one;
        
        for (int i = 1; i < pathArray.Length; i++)
        {
            Vector2 currentPos = LocalToWorldSpace(pathArray[i]);
            if (currentPos == lastPos) continue; // Skip Duplicates
            Vector2 currentDir = currentPos - lastPos; // IT is 1 cell difference at a time -> TODO: Maybe need to change it to be normalized

            if (currentPos != lastPos && currentDir != lastDir) // Direction changed
            {
                pathList.Add(new PathNode(lastPos, currentDir));
            }

            lastPos = currentPos;
            lastDir = currentDir;
        }
    }

    // private void GenerateInputPathway()
    // {
    //     Vector2Int[] pathArray = pathStack.ToArray();
    //     pathStack.Reverse();

    //     HashSet<Vector2Int> seen = new HashSet<Vector2Int>();
    //     List<Vector2Int> result = new List<Vector2Int>();

    //     for (int i = pathArray.Length - 1; i >= 0; i--)
    //     {
    //         if (seen.Add(pathArray[i]))
    //         {
    //             result.Add(pathArray[i]);
    //         }
    //     }

    //     pathArray = result.ToArray();

    //     // Skip First Cell as you will start there
    //     Vector2 lastPos = LocalToWorldSpace(pathArray[0]);
    //     Vector2 lastDir = Vector2.one;

    //     for (int i = 1; i < pathArray.Length; i++)
    //     {
    //         Vector2 currentPos = LocalToWorldSpace(pathArray[i]);
    //         if (currentPos == lastPos) continue; // Skip Duplicates
    //         Vector2 currentDir = currentPos - lastPos; // IT is 1 cell difference at a time -> TODO: Maybe need to change it to be normalized

    //         if (currentPos != lastPos && currentDir != lastDir) // Direction changed
    //         {
    //             pathList.Add(new PathNode(lastPos, currentDir));
    //         }

    //         lastPos = currentPos;
    //         lastDir = currentDir;
    //     }
    // }

    /// <summary>
    /// Gets the World Space of the Local Space in comparison to the current object
    /// </summary>
    /// <param name="localPos">The Local Position in the object</param>
    /// <returns>World Space of the object</returns>
    public Vector2 LocalToWorldSpace(Vector2 localPos){
        return transform.TransformPoint(localPos);
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireCube(new Vector3(transform.position.x + startPoint.x, transform.position.y + startPoint.y, 1), Vector2.one);

        Gizmos.color = Color.red;

        Gizmos.DrawWireCube(new Vector3(transform.position.x + exitPoint.x, transform.position.y + exitPoint.y, 1), Vector2.one);
    }
}

public static class ListExtensions
{
    public static void Shuffle<T>(this T[] array)
    {
        for (int i = 0; i < array.Length; i++){
            int rand = Random.Range(0, array.Length);
            (array[i], array[rand]) = (array[rand], array[i]);
        }
    }
}
