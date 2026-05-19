using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CandleManager : Puzzle
{
    [SerializeField] private List<CandleExtinguish> _candles = new();
    
    public void RegisterCandle(CandleExtinguish candle)
    {
        if (!_candles.Contains(candle))
        {
            _candles.Add(candle);
            Debug.Log($"Candle {candle.gameObject.name} registered. Total candles: {_candles.Count}");
        }
    }
    
    public void CandleExtinguished(CandleExtinguish candle)
    {
        Debug.Log($"Candle {candle.gameObject.name} has been extinguished!");
        CheckAllCandlesExtinguished();
    }
    
    private void CheckAllCandlesExtinguished()
    {
        if (_candles.Count == 0)
        {
            Debug.Log("No candles registered!");
            return;
        }
        
        bool allExtinguished = _candles.All(candle => candle.GetIsExtinguished());

        if (allExtinguished)
        {
            Solve(); // All candles have been extinguished, puzzle is solved.
        }
    }
}