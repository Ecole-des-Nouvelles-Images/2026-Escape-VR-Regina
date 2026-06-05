using System.Collections.Generic;
using UnityEngine;

public class BlackBoardManager : Puzzle
{
    [Header("===== Settings =====")] [SerializeField]
    private List<BlackBoardHandler> _socketsBlackBoard = new List<BlackBoardHandler>();

    public void SocketIsOccuped()
    {
        int correctNumber = 0;
        foreach (var socket in _socketsBlackBoard)
        {
            if (socket.IsOccuped)
            {
                correctNumber++;
            }
        }

        if (correctNumber == _socketsBlackBoard.Count)
        {
            SocketVerification();
        }
    }

    private void SocketVerification()
    {
        int CorrectSocket = 0;
        foreach (var socket in _socketsBlackBoard)
        {
            if (socket.IsObject())
            {
                CorrectSocket++;
            }
        }

        if (CorrectSocket == _socketsBlackBoard.Count)
        {
            Solve();
        }
        else
        {
            EventBus.OnGameLoose?.Invoke();
        }
    }
    
    public override void Solve()
    {
        base.Solve();
        EventBus.OnGameWin?.Invoke();
    }
}