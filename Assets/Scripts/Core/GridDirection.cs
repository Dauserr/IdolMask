using UnityEngine;

// Matches TileGrid convention: x = row (world Y), y = col (world X)
// Use these instead of Vector2Int.up/right so everything stays consistent
public static class GridDirection
{
    public static readonly Vector2Int Up    = new( 1,  0);
    public static readonly Vector2Int Down  = new(-1,  0);
    public static readonly Vector2Int Right = new( 0,  1);
    public static readonly Vector2Int Left  = new( 0, -1);
}
