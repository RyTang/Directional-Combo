using UnityEngine;

public struct PathNode {
    public Vector2 Position;
    public Vector2Int Direction;

    public PathNode(Vector2 position, Vector2Int direction){
        Position = position;
        Direction = direction;
    }
}