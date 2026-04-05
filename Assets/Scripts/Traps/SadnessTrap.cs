using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// exactly like JoyTrap but works on vertical columns instead of horizontal rows
// "up" = high row index (top of screen), "bottom" = row 0
public class SadnessTrap : MonoBehaviour
{
    private readonly List<Tile>      _affectedTiles = new();
    private readonly List<Coroutine> _colRoutines   = new();
    private Coroutine _mainRoutine;

    public void Activate(TrapConfig config, float speedMultiplier)
    {
        StopAll();
        _mainRoutine = StartCoroutine(ActivateRoutine(config, speedMultiplier));
    }

    private IEnumerator ActivateRoutine(TrapConfig config, float speedMultiplier)
    {
        // randomly target odd columns or even columns
        int startIndex = Random.value > 0.5f ? 0 : 1;
        var selectedCols = new List<int>();
        for (int col = startIndex; col < TileGrid.Size; col += 2)
            selectedCols.Add(col);

        int pattern = Random.Range(0, 6);

        switch (pattern)
        {
            case 0: // all columns sweep top→bottom at the same time
                foreach (int col in selectedCols)
                    _colRoutines.Add(StartCoroutine(SweepColumn(col, fromTop: true, config, speedMultiplier)));
                break;

            case 1: // all columns sweep top→bottom at staggered times
                foreach (int col in selectedCols)
                {
                    float delay = Random.Range(config.staggerTimingMin, config.staggerTimingMax) / speedMultiplier;
                    _colRoutines.Add(StartCoroutine(DelayedSweep(col, fromTop: true, delay, config, speedMultiplier)));
                }
                break;

            case 2: // alternating columns go top→bottom and bottom→top at the same time
                for (int i = 0; i < selectedCols.Count; i++)
                {
                    bool fromTop = i % 2 == 0;
                    _colRoutines.Add(StartCoroutine(SweepColumn(selectedCols[i], fromTop, config, speedMultiplier)));
                }
                break;

            case 3: // alternating top/bottom with staggered start times
                for (int i = 0; i < selectedCols.Count; i++)
                {
                    bool  fromTop = i % 2 == 0;
                    float delay   = Random.Range(config.staggerTimingMin, config.staggerTimingMax) / speedMultiplier;
                    _colRoutines.Add(StartCoroutine(DelayedSweep(selectedCols[i], fromTop, delay, config, speedMultiplier)));
                }
                break;

            case 4: // all columns sweep bottom→top at the same time
                foreach (int col in selectedCols)
                    _colRoutines.Add(StartCoroutine(SweepColumn(col, fromTop: false, config, speedMultiplier)));
                break;

            case 5: // all columns sweep bottom→top at staggered times
                foreach (int col in selectedCols)
                {
                    float delay = Random.Range(config.staggerTimingMin, config.staggerTimingMax) / speedMultiplier;
                    _colRoutines.Add(StartCoroutine(DelayedSweep(col, fromTop: false, delay, config, speedMultiplier)));
                }
                break;
        }

        yield break;
    }

    // sweep one column tile by tile from top or bottom
    private IEnumerator SweepColumn(int col, bool fromTop, TrapConfig config, float speedMultiplier)
    {
        int start = fromTop ? TileGrid.Size - 1 : 0;
        int end   = fromTop ? -1 : TileGrid.Size;
        int step  = fromTop ? -1 : 1;

        for (int row = start; row != end; row += step)
        {
            var tile = TileGrid.GetTile(row, col);
            if (tile != null)
            {
                _affectedTiles.Add(tile);
                tile.StartCrack();
            }
            yield return new WaitForSeconds(config.tileWaveDelay / speedMultiplier);
        }
    }

    private IEnumerator DelayedSweep(int col, bool fromTop, float delay, TrapConfig config, float speedMultiplier)
    {
        yield return new WaitForSeconds(delay);
        yield return StartCoroutine(SweepColumn(col, fromTop, config, speedMultiplier));
    }

    public void Deactivate()
    {
        StopAll();
        foreach (var tile in _affectedTiles)
        {
            if (tile != null)
                tile.StartRespawn();
        }
        _affectedTiles.Clear();
    }

    private void StopAll()
    {
        if (_mainRoutine != null)
        {
            StopCoroutine(_mainRoutine);
            _mainRoutine = null;
        }
        foreach (var r in _colRoutines)
            if (r != null) StopCoroutine(r);
        _colRoutines.Clear();
    }

    private void OnDestroy() => StopAll();
}
