using System.Collections.Generic;
using UnityEngine;

public static class TileGrid
{
    public const int Size = 15;

    private static Tile[,]  _grid;
    private static Vector3  _origin;     // world position of tile [0,0]
    private static float    _tileSize;   // world units per tile
    private static readonly HashSet<Vector2Int> _blockedPositions = new();


    public static void Initialize(Tile[,] tiles, Vector3 origin, float tileSize)
    {
        _grid     = tiles;
        _origin   = origin;
        _tileSize = tileSize;
        _blockedPositions.Clear();
    }

    public static Tile GetTile(int row, int col)
    {
        if (row < 0 || row >= Size || col < 0 || col >= Size) return null;
        return _grid[row, col];
    }

    public static Tile GetTile(Vector2Int gridPos) => GetTile(gridPos.x, gridPos.y);

    public static Tile[] GetRow(int row)
    {
        if (row < 0 || row >= Size) return System.Array.Empty<Tile>();
        var tiles = new Tile[Size];
        for (int col = 0; col < Size; col++)
            tiles[col] = _grid[row, col];
        return tiles;
    }

    public static Tile[] GetColumn(int col)
    {
        if (col < 0 || col >= Size) return System.Array.Empty<Tile>();
        var tiles = new Tile[Size];
        for (int row = 0; row < Size; row++)
            tiles[row] = _grid[row, col];
        return tiles;
    }

    public static List<Tile> GetNeighbors(int row, int col, int radius)
    {
        var result = new List<Tile>();
        for (int r = row - radius; r <= row + radius; r++)
        {
            for (int c = col - radius; c <= col + radius; c++)
            {
                float distance = Mathf.Sqrt((r - row) * (r - row) + (c - col) * (c - col));
                if (distance > radius) continue;

                var tile = GetTile(r, c);
                if (tile != null) result.Add(tile);
            }
        }
        return result;
    }

    public static Vector3 GridToWorld(int row, int col)
    {
        return _origin + new Vector3(col * _tileSize, row * _tileSize, 0f);
    }

    public static Vector3 GridToWorld(Vector2Int gridPos) => GridToWorld(gridPos.x, gridPos.y);

    public static Vector2Int WorldToGrid(Vector3 worldPos)
    {
        var local = worldPos - _origin;
        int col = Mathf.RoundToInt(local.x / _tileSize);
        int row = Mathf.RoundToInt(local.y / _tileSize);
        return new Vector2Int(row, col);
    }

    public static bool IsInBounds(int row, int col)
    {
        return row >= 0 && row < Size && col >= 0 && col < Size;
    }

    public static bool IsInBounds(Vector2Int pos) => IsInBounds(pos.x, pos.y);

    public static void SetBlocked(Vector2Int pos, bool blocked)
    {
        if (blocked)
            _blockedPositions.Add(pos);
        else
            _blockedPositions.Remove(pos);
    }
    
    public static bool IsBlocked(Vector2Int pos) => _blockedPositions.Contains(pos);
    
    public static void ClearBlocked() => _blockedPositions.Clear();
}
