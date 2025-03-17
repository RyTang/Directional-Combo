using UnityEngine;
using System.Collections.Generic;

public class ContinuousMazeGenerator : MonoBehaviour
{
    public int chunkSize = 10; // TODO: Change this to read directly from the maze generator
    public GameObject chunkPrefab; // The prefab for maze chunks
    public int maxChunks = 3;
    private int generatedChunks = 0;


    private Vector2Int previousChunkExit;
    private Vector2Int nextChunkEntrance;
    private List<GameObject> activeChunks = new List<GameObject>();

    void Start()
    {
        generatedChunks = 1;
        GenerateInitialChunk();
    }

    void Update()
    {
        if (generatedChunks < maxChunks)
        {
            GenerateNextChunk();
            generatedChunks++;
        }

        // Optional: Remove distant chunks to optimize performance
        // for (int i = activeChunks.Count - 1; i >= 0; i--)
        // {
        //     if (PlayerDistanceToChunk(activeChunks[i]) > 2 * chunkSize)
        //     {
        //         Destroy(activeChunks[i]);
        //         activeChunks.RemoveAt(i);
        //     }
        // }
    }

    void GenerateInitialChunk()
    {
        GameObject chunk = Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity);
        activeChunks.Add(chunk);

        MazeGenerator mazeGen = chunk.GetComponent<MazeGenerator>();
        mazeGen.GenerateMaze(null);
        previousChunkExit = mazeGen.GetExitPoint();
    }
    
    void GenerateNextChunk()
    {
        Vector3 nextPosition = GetNextChunkPosition();
        GameObject chunk = Instantiate(chunkPrefab, nextPosition, Quaternion.identity);
        activeChunks.Add(chunk);

        MazeGenerator mazeGen = chunk.GetComponent<MazeGenerator>();

        mazeGen.GenerateMaze(FindNewChunkEntrance(previousChunkExit));
        previousChunkExit = mazeGen.GetExitPoint();
    }

    private Vector2Int FindNewChunkEntrance(Vector2Int exitPoint)
    {
        int newX = exitPoint.x;
        int newY = exitPoint.y;

        // Invert if they are at the edge to get the opposite boudnary
        // Add + 1 due to array index starting at 0
        if (exitPoint.x == 0 || exitPoint.x + 1 == chunkSize)
        {
            newX = chunkSize - (exitPoint.x + 1);
        }
        // IF Bottom Exit -> Top Entrance, Top Exit -> Bottom Entrance
        if (exitPoint.y == 0 || exitPoint.y + 1 == chunkSize)
        {
            newY = chunkSize - (exitPoint.y + 1);
        }

        return new Vector2Int(newX, newY);
    }

    Vector2Int GetExitDirection(Vector2Int exitPoint){
        if (exitPoint.x == 0) return Vector2Int.left;
        else if (exitPoint.x == chunkSize - 1) return Vector2Int.right;
        else if (exitPoint.y == 0) return Vector2Int.down;
        else return Vector2Int.up;
    }

    float PlayerDistanceToChunk(GameObject chunk)
    {
        Bounds chunkBounds = chunk.GetComponent<Renderer>().bounds;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        return Vector3.Distance(player.transform.position, chunkBounds.center);
    }

    Vector3 GetNextChunkPosition()
    {
        if (activeChunks.Count == 0) return Vector3.zero;

        GameObject lastChunk = activeChunks[activeChunks.Count - 1];

        Vector2 lastChunkPosition = lastChunk.transform.position;

        Vector3 nextPosition;

        // Determine the direction based on the exit point
        Vector2Int previousChunkExitDirection = GetExitDirection(previousChunkExit);

        nextPosition = previousChunkExitDirection * chunkSize + lastChunkPosition;


        // if (previousChunkExit.x == 0) // Left exit
        // {
        //     nextPosition = new Vector3(lastChunkPosition.x - chunkSize, lastChunkPosition.y);
        // }
        // else if (previousChunkExit.x == chunkSize - 1) // Right exit
        // {
        //     nextPosition = new Vector3(lastChunkPosition.x + chunkSize, lastChunkPosition.y);
        // }
        // else if (previousChunkExit.y == 0) // Bottom exit (default)
        // {
        //     nextPosition = new Vector3(lastChunkPosition.x, lastChunkPosition.y + chunkSize);
        // }
        // else if (previousChunkExit.y == chunkSize - 1) // Top exit
        // {
        //     nextPosition = new Vector3(lastChunkPosition.x, lastChunkPosition.y - chunkSize);
        // }
        // else {
        //     Debug.LogError($"Encountered Error while getting Next Position for Chunk: {lastChunkPosition}");
        //     nextPosition = Vector3.zero;
        // }
        return nextPosition;
    }
}
