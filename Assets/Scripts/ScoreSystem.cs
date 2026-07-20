using System;
using System.Collections.Generic;
using UnityEngine;

public class ScoreSystem
{
    public event Action<int, int> OnScoreRefresh;

    private DataScore _data;

    private int _score;
    public int Score => _score;

    public ScoreSystem(DataScore data)
    {
        _data = data;
    }

    public void AddScore(List<Match> matches)
    {
        foreach (var match in matches)
        {
            switch (match.Type)
            {
                case MatchType.Line3:
                    AddScore(_data.Line3);
                    break;

                case MatchType.Line4:
                    AddScore(_data.Line4);
                    break;

                case MatchType.Line5:
                    AddScore(_data.Line5);
                    break;

                case MatchType.LShape:
                    AddScore(_data.LShape);
                    break;

                case MatchType.TShape:
                    AddScore(_data.TShape);
                    break;
            }
        }
    }

    private void AddScore(int amount)
    {
        _score += amount;
        OnScoreRefresh?.Invoke(_score, amount);
    }

    public void ResetScore()
    {
        _score = 0;
        OnScoreRefresh?.Invoke(_score, 0);
    }
}
