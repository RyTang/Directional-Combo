using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using System.Collections;

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
    [SerializeField] private Stack<Vector2Int> pathStack;

    [SerializeField] private List<PathNode> pathList = new List<PathNode>();


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

    /// <summary>
    /// Generates a random path through the maze using a depth-first search algorithm with backtracking.
    /// The algorithm creates corridors of variable length and ensures the path reaches an edge that's
    /// not on the same side as the entrance to create a valid exit point.
    /// </summary>
    /// <param name="startCell">The starting cell position for path generation</param>
    private void GeneratePath(Vector2Int startCell)
    {
        // Initialize path generation starting point and data structures
        startPoint = startCell;
        Vector2Int current = startCell;
        pathStack = new Stack<Vector2Int>();
        mazeGrid[current.x, current.y] = 1; // Mark starting cell as path (1 = path, 0 = wall)
        pathStack.Push(current);

        // Define the four cardinal directions for corridor generation
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        // Main path generation loop using depth-first search with backtracking
        while (pathStack.Count > 0)
        {
            List<Vector2Int> validDirections = new List<Vector2Int>();
            directions.Shuffle(); // Randomize direction order for varied maze patterns

            // Generate a random corridor length within specified bounds
            int corridorLength = Random.Range(minCorridorLength, maxCorridorLength);

            // Find all directions where we can create a valid corridor of the desired length
            validDirections.AddRange(directions.Where(dir => IsValidCorridor(current, dir, corridorLength)));

            // CASE 1: No valid directions available - handle dead ends and exit conditions
            if (validDirections.Count == 0)
            {
                // Check if current position can serve as a valid exit point
                // Must be at edge and not on the same side as entrance
                if (IsAtEdge(current) && !IsExitSameSideAsEntrance(current))
                {
                    exitPoint = current;
                    pathStack.Push(current); // Add the exit point to the path stack
                    pathFound = true;
                    return;
                }
                
                // If not at edge, try to find the closest valid edge and create a direct path to it
                else if (!IsAtEdge(current))
                {
                    Vector2Int closestEdge = current;
                    float minDistance = float.MaxValue;
                    
                    // Search in all directions to find the closest valid edge
                    foreach (Vector2Int dir in directions)
                    {
                        Vector2Int edgeCell = current;
                        // Move in direction until hitting an edge
                        while (IsValidCell(edgeCell) && !IsAtEdge(edgeCell))
                        {
                            edgeCell += dir;
                        }
                        
                        // Check if this edge is valid (exists and not same side as entrance)
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

                    // If a valid edge was found and we can create a corridor to it, do so
                    if (closestEdge != current && IsValidCorridor(current, closestEdge))
                    {
                        Vector2Int direction = GetDirection(current, closestEdge);
                        int length = (closestEdge - current).x != 0 ? Mathf.Abs((closestEdge - current).x) : Mathf.Abs((closestEdge - current).y);
                        
                        // Create corridor from current position to the edge
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
                        pathStack.Push(closestEdge); // Add the exit point to the path stack
                        pathFound = true;
                        return;
                    }
                }

                // BACKTRACKING: If no valid moves or exit found, backtrack by removing current path
                // This implements the backtracking aspect of the depth-first search
                while (pathStack.Count > 0)
                {
                    mazeGrid[current.x, current.y] = 0; // Mark cell as wall again
                    pathStack.Pop();
                    if (pathStack.Count > 0)
                    {
                        current = pathStack.Peek(); // Move back to previous cell
                    }
                }
                continue;
            }

            // CASE 2: Valid directions available - extend the path
            // Choose a random valid direction and create a corridor
            Vector2Int chosenDir = validDirections[Random.Range(0, validDirections.Count)];
            Vector2Int nextCell = current + chosenDir * corridorLength;

            // Safety check - ensure the destination cell is valid
            if (!IsValidCell(nextCell))
            {
                continue;
            }

            // Create the corridor by marking all cells in the path
            for (int i = 0; i <= corridorLength; i++)
            {
                Vector2Int corridorCell = current + chosenDir * i;
                if (IsValidCell(corridorCell))
                {
                    mazeGrid[corridorCell.x, corridorCell.y] = 1; // Mark as path
                    pathStack.Push(corridorCell); // Add to path stack for potential backtracking
                }
            }
            current = nextCell; // Move to the end of the newly created corridor
        }
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

    /// <summary>
    /// Returns the direction (Vector2Int) from the given cell to the closest edge of the maze
    /// </summary>
    /// <param name="cell">Cell To check</param>
    /// <returns>Direction</returns>
    private Vector2Int GetExitDirection(Vector2Int cell)
    {
        int leftDist = cell.x;
        int rightDist = gridWidth - 1 - cell.x;
        int downDist = cell.y;
        int upDist = gridHeight - 1 - cell.y;

        int minDist = Mathf.Min(leftDist, rightDist, downDist, upDist);

        if (minDist == leftDist)
            return Vector2Int.left;
        if (minDist == rightDist)
            return Vector2Int.right;
        if (minDist == downDist)
            return Vector2Int.down;
        // else upDist is smallest
        return Vector2Int.up;
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

    private void GenerateInputPathway()
    {
        Vector2Int[] pathArray = pathStack.ToArray();
        pathStack.Reverse();

        HashSet<Vector2Int> seen = new HashSet<Vector2Int>();
        List<Vector2Int> result = new List<Vector2Int>();

        for (int i = pathArray.Length - 1; i >= 0; i--)
        {
            if (seen.Add(pathArray[i]))
            {
                result.Add(pathArray[i]);
            }
        }

        pathArray = result.ToArray();

        // Skip First Cell as you will start there
        Vector2 curPos = LocalToWorldSpace(pathArray[0]);
        Vector2 curDir = Vector2.one;
        Vector2 nextDir = Vector2.one, nextPos;

        for (int i = 1; i < pathArray.Length; i++)
        {
            nextPos = LocalToWorldSpace(pathArray[i]);
            if (nextPos == curPos) continue; // Skip Duplicates
            nextDir = nextPos - curPos; // IT is 1 cell difference at a time -> TODO: Maybe need to change it to be normalized

            if (nextPos != curPos && nextDir != curDir) // Direction changed
            {
                pathList.Add(new PathNode(curPos, nextDir));
            }

            curPos = nextPos;
            curDir = nextDir;
        }

        // Get the Last One
        nextDir = GetExitDirection(exitPoint);

        // If current direction then update last final point
        if (curDir != nextDir)
        {
            pathList.Add(new PathNode(curPos, nextDir));
        }
    }


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
