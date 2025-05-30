using System;
using System.Collections.Generic;
using System.Linq;
using com.cyborgAssets.inspectorButtonPro;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapPathFinder : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;
    [SerializeField] List<TileSettngs> tileSettngs = new List<TileSettngs>();
    [SerializeField] List<Vector3> path = new List<Vector3>();
    [SerializeField] private int endPoint = 0;
    [SerializeField] private int startPoint = 0;

    private Dictionary<string, Vector3> cache = new Dictionary<string, Vector3>();

    private float cachedLength = 0f;

    public float GetLength()
    {
        if (cachedLength != 0)
        {
            return cachedLength;
        }

        var length = 0f;
        for (var i = 1; i < path.Count; i++)
        {
            length += Vector3.Distance(path[i], path[i - 1]);
        }

        cachedLength = length;

        return length;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        foreach (var vector3 in path)
        {
            Gizmos.DrawSphere(vector3, 0.15f);
        }
    }

    public Vector3 GetPositionByDelta(float delta)
    {
        var idx = 0;
        return Move(path[0], delta, ref idx);
    }

    public Vector3 GetPositionByDelta(float delta, ref int index)
    {
        if (!cache.TryGetValue(delta.ToString(), out Vector3 result))
        {
            result = Move(path[0], delta, ref index);
            cache.Add(delta.ToString(), result);
        }

        return result;
    }

    public Vector3 GetStartPoint()
    {
        return path[0];
    }

    public Vector3 Move(Vector3 pos, float delta, ref int index)
    {
        var points = path;
        var next = index + 1;
        while (true)
        {
            var nextPoint = next >= points.Count - 1 ? points.Last() : points[next];
            var distance = Vector3.Distance(nextPoint, pos);

            var d = MathF.Min(distance, delta);
            pos = Vector3.MoveTowards(pos, nextPoint, d);
            if (delta > distance && delta < points.Count - 1)
            {
                delta -= d;
                next++;
            }
            else
            {
                break;
            }
        }

        index = next - 1;
        return pos;
    }

    public int GetClosestIndex(Vector3 pos)
    {
        var points = path;
        var closestDistance = Vector3.Distance(points[0], pos);
        var closestIndex = 0;

        for (var index = 1; index < points.Count; index++)
        {
            var point = points[index];
            var distance = Vector3.Distance(point, pos);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = index;
            }
            else
            {
                break;
            }
        }

        return closestIndex;
    }

    [ProButton]
    private void Setup()
    {
        cache.Clear();
        cachedLength = 0;
        path.Clear();
        var size = tilemap.size;
        var bounds = tilemap.cellBounds;
        for (var x = bounds.min.x; x < bounds.max.x; x++)
        {
            for (var y = bounds.min.y; y < bounds.max.y; y++)
            {
                var tile = tilemap.GetTile(new Vector3Int(x, y, 0));
                if (tile == null)
                {
                    continue;
                }

                var settings = tileSettngs.FirstOrDefault(x => x.TileName == tile.name);
                if (settings != null)
                {
                    var pos = tilemap.CellToWorld(new Vector3Int(x, y));
                    pos += settings.Offset;
                    path.Add(pos);
                }
            }
        }

        path = path.Skip(startPoint).Take(endPoint).ToList();
    }
}