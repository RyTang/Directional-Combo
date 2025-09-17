using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class ContinuousMazeGenerator : MonoBehaviour
{
    public int chunkSize = 10; // TODO: Change this to read directly from the maze generator
    public GameObject chunkPrefab; // The prefab for maze chunks
    public GameObject player;
    public int maxChunks = 3;
    public LayerMask mazeLayerMask;
    [SerializeField] private GameEvent mazeDoneGenerating;

    [SerializeField] private float chunkRemoverBuffer = 2;

    public Vector2 mazeStartPosition;
    private Vector2Int previousChunkExit;
    private List<GameObject> activeChunks = new List<GameObject>();
    private List<PathNode> pathway = new List<PathNode>();
    private List<int> actionsRequiredList = new List<int>();
    private int actionCounter = 0;


    void Start()
    {
        actionCounter = 0;
        GenerateInitialChunk();

        mazeStartPosition = activeChunks[0].GetComponent<MazeGenerator>().GetStartPoint();

        mazeDoneGenerating.TriggerEvent();

        player ??= GameObject.FindGameObjectWithTag("Player");

        Debug.Assert(player is not null, $"Player Object is null in {this}");

    }

    void Update()
    {
        if (activeChunks.Count < maxChunks)
        {
            GenerateNextChunk();
        }

        // Optional: Remove distant chunks to optimize performance
        if (actionCounter >= actionsRequiredList[0] * chunkRemoverBuffer)
        {
            Debug.Log("Destroying Chunks");
            // Remove the oldest chunk
            Destroy(activeChunks[0]);
            activeChunks.RemoveAt(0);

            actionCounter = (int)MathF.Max(0, actionCounter - actionsRequiredList[0] * chunkRemoverBuffer);
            actionsRequiredList.RemoveAt(0);
        }
    }

    /// <summary>
    /// Checks if Input is correct, then consumes it. Returns position of the path to be in
    /// </summary>
    /// <param name="input">Direction Input that is Normalised</param>
    /// <returns>World Position to move to</returns>
    public Vector2? ConsumeIfCorrect(Vector2 input){
        if (input == pathway[0].Direction)
        {
            pathway.RemoveAt(0);
            actionCounter += 1;
            return pathway[0].Position;
        }
        return null;
    }

    void GenerateInitialChunk()
    {
        GameObject chunk = Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity);
        activeChunks.Add(chunk);

        MazeGenerator mazeGen = chunk.GetComponent<MazeGenerator>();
        mazeGen.GenerateMaze(null);
        // Add Pathway into the system
        pathway.AddRange(mazeGen.GetPathway());
        previousChunkExit = mazeGen.GetExitPoint();
        Debug.Log(string.Join(", ", pathway.Select(node => node.ToString())));
    }
    
    void GenerateNextChunk()
    {
        Vector3 nextPosition = GetNextChunkPosition();
        
        // If it collides then rollback one and regenerate the path
        // PROBLEM STATEMENT, the location of the chunk is at the bottom left of the chunk hence need to calculate it properly
        Collider2D collided = Physics2D.OverlapBox(nextPosition + new Vector3(chunkSize/2, chunkSize/2), Vector2.one * chunkSize/2, 0f, mazeLayerMask);
        if (collided != null){
            // Remove Previous CHanges Made
            MazeGenerator chunkToRemove = activeChunks[activeChunks.Count - 1].GetComponent<MazeGenerator>();
            
            // Clear Off previousy made Path
            int chunkToRemovePathCount = chunkToRemove.GetPathway().Count;
            pathway.RemoveRange(pathway.Count - chunkToRemovePathCount, chunkToRemovePathCount);
            Destroy(chunkToRemove.gameObject);
            activeChunks.RemoveAt(activeChunks.Count - 1);

            // Update Previous Chunk Exit
            previousChunkExit = activeChunks[activeChunks.Count - 1].GetComponent<MazeGenerator>().GetExitPoint();
            return;
        }

        // Beacuse of the Previous Chunk Exit pointing to something that doesn't exist anymore
        
        GameObject chunk = Instantiate(chunkPrefab, nextPosition, Quaternion.identity);
        activeChunks.Add(chunk);

        MazeGenerator mazeGen = chunk.GetComponent<MazeGenerator>();

        mazeGen.GenerateMaze(FindNewChunkEntrance(previousChunkExit));

        // Check if the last direction is the same as the new object direction, if so then remove the last direction
        List<PathNode> newPathway = mazeGen.GetPathway();
        if (Vector2.Angle(pathway[pathway.Count - 1].Direction, newPathway[0].Direction) < 0.01f)
        {
            Debug.Log($"Removed position {newPathway[0]}, comparing {pathway[pathway.Count - 1]} and {newPathway[0]}");
            Debug.Log($"Next Position: {newPathway[1]}");
            newPathway.RemoveAt(0); 
        }

        int actionsRequired = newPathway.Count;

        actionsRequiredList.Add(actionsRequired);

        // Add Pathway
        pathway.AddRange(mazeGen.GetPathway());
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

    Vector3 GetNextChunkPosition()
    {
        if (activeChunks.Count == 0) return Vector3.zero;

        GameObject lastChunk = activeChunks[activeChunks.Count - 1];

        Vector2 lastChunkPosition = lastChunk.transform.position;

        Vector3 nextPosition;

        // Determine the direction based on the exit point
        Vector2Int previousChunkExitDirection = GetExitDirection(previousChunkExit);

        nextPosition = previousChunkExitDirection * chunkSize + lastChunkPosition;

        return nextPosition;
    }

    Vector2Int GetExitDirection(Vector2Int exitPoint)
    {
        if (exitPoint.x == 0) return Vector2Int.left;
        else if (exitPoint.x == chunkSize - 1) return Vector2Int.right;
        else if (exitPoint.y == 0) return Vector2Int.down;
        else return Vector2Int.up;
    }

}
