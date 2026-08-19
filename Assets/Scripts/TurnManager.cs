using UnityEngine;

public class TurnManager
{
    public event System.Action OnTick;

    public int TurnCount { get; private set; }

    public TurnManager()
    {
        TurnCount = 1;
    }

    public void Tick()
    {
        TurnCount++;
        OnTick?.Invoke();
    }

    public void Reset()
    {
        TurnCount = 1;
    }
}
