using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CandleManager : MonoBehaviour
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
            AllCandlesExtinguished();
        }
    }
    
    private void AllCandlesExtinguished()
    {
        Debug.Log("=== ALL CANDLES HAVE BEEN EXTINGUISHED! ===");
        // TODO puzzle finished event
    }
}