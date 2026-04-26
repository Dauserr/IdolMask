using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoyTrap : MonoBehaviour
{
    private readonly List<Tile>     _affectedTiles = new();
    private readonly List<Coroutine> _rowRoutines  = new();
    private Coroutine _mainRoutine;

    public void Activate(TrapConfig config, float speedMultiplier)
    {
        StopAll();
        _mainRoutine = StartCoroutine(ActivateRoutine(config, speedMultiplier));
    }

    private IEnumerator ActivateRoutine(TrapConfig config, float speedMultiplier)
    {
        // randomly target odd rows or even rows
        int startIndex   = Random.value > 0.5f ? 0 : 1;
        var selectedRows = new List<int>();
        for (int row = startIndex; row < TileGrid.Size; row += 2)
            selectedRows.Add(row);

        // one of 6 activation patterns chosen at random
        int pattern = Random.Range(0, 6);

        switch (pattern)
        {
            case 0: // all rows sweep left→right at the same time
                foreach (int row in selectedRows)
                    _rowRoutines.Add(StartCoroutine(SweepRow(row, fromLeft: true, config, speedMultiplier)));
                break;

            case 1: // all rows sweep left→right but start at staggered times
                foreach (int row in selectedRows)
                {
                    float delay = Random.Range(config.staggerTimingMin, config.staggerTimingMax) / speedMultiplier;
                    _rowRoutines.Add(StartCoroutine(DelayedSweep(row, fromLeft: true, delay, config, speedMultiplier)));
                }
                break;

            case 2: // alternating rows go left→right and right→left at the same time
                for (int i = 0; i < selectedRows.Count; i++)
                {
                    bool fromLeft = i % 2 == 0;
                    _rowRoutines.Add(StartCoroutine(SweepRow(selectedRows[i], fromLeft, config, speedMultiplier)));
                }
                break;

            case 3: // alternating rows left/right but with staggered start times
                for (int i = 0; i < selectedRows.Count; i++)
                {
                    bool  fromLeft = i % 2 == 0;
                    float delay    = Random.Range(config.staggerTimingMin, config.staggerTimingMax) / speedMultiplier;
                    _rowRoutines.Add(StartCoroutine(DelayedSweep(selectedRows[i], fromLeft, delay, config, speedMultiplier)));
                }
                break;

            case 4: // all rows sweep right→left at the same time
                foreach (int row in selectedRows)
                    _rowRoutines.Add(StartCoroutine(SweepRow(row, fromLeft: false, config, speedMultiplier)));
                break;

            case 5: // all rows sweep right→left but start at staggered times
                foreach (int row in selectedRows)
                {
                    float delay = Random.Range(config.staggerTimingMin, config.staggerTimingMax) / speedMultiplier;
                    _rowRoutines.Add(StartCoroutine(DelayedSweep(row, fromLeft: false, delay, config, speedMultiplier)));
                }
                break;
        }

        yield break;
    }

    // sweep one row tile by tile from one end to the other
    private IEnumerator SweepRow(int row, bool fromLeft, TrapConfig config, float speedMultiplier)
    {
        int start = fromLeft ? 0 : TileGrid.Size - 1;
        int end   = fromLeft ? TileGrid.Size : -1;
        int step  = fromLeft ? 1 : -1;

        for (int col = start; col != end; col += step)
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

    private IEnumerator DelayedSweep(int row, bool fromLeft, float delay, TrapConfig config, float speedMultiplier)
    {
        yield return new WaitForSeconds(delay);
        yield return StartCoroutine(SweepRow(row, fromLeft, config, speedMultiplier));
    }

    public void Deactivate()
    {
        StopAll();
        foreach (var tile in _affectedTiles)
        {
            if (tile != null)
                tile.ForceReset();
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
        foreach (var r in _rowRoutines)
            if (r != null) StopCoroutine(r);
        _rowRoutines.Clear();
    }

    private void OnDestroy() => StopAll();
}
