using UnityEngine;

public struct PathNode {
    public Vector2 Position;
    public Vector2 Direction;

    public PathNode(Vector2 position, Vector2 direction){
        Position = position;
        Direction = direction;
    }

    public override string ToString()
    {
        return $"[{Position}, {Direction}]";
    }
}