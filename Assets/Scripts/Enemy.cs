using UnityEngine;

public class Enemy : CellObject
{
    public int Health = 3;
    public int AttackDamage = 3;

    private int m_CurrentHealth;
    private bool m_IsListeningForTurns;

    private void OnDestroy()
    {
        StopListeningForTurns();
    }

    private void StartListeningForTurns()
    {
        if (m_IsListeningForTurns
            || GameManager.Instance == null
            || GameManager.Instance.TurnManager == null)
        {
            return;
        }

        GameManager.Instance.TurnManager.OnTick += TurnHappened;
        m_IsListeningForTurns = true;
    }

    private void StopListeningForTurns()
    {
        if (!m_IsListeningForTurns
            || GameManager.Instance == null
            || GameManager.Instance.TurnManager == null)
        {
            return;
        }

        GameManager.Instance.TurnManager.OnTick -= TurnHappened;
        m_IsListeningForTurns = false;
    }

    public override void Init(Vector2Int coord)
    {
        base.Init(coord);
        m_CurrentHealth = Health;
        StartListeningForTurns();
    }

    public override bool PlayerWantsToEnter()
    {
        m_CurrentHealth -= 1;

        if (m_CurrentHealth <= 0)
        {
            GameManager.Instance.BoardManager.ClearCellObject(m_Cell, this);
            GameManager.Instance.EnemyDefeated();
            Destroy(gameObject);
        }

        return false;
    }

    bool MoveTo(Vector2Int coord)
    {
        var board = GameManager.Instance.BoardManager;
        var targetCell = board.GetCellData(coord);

        if (targetCell == null
            || !targetCell.Passable
            || targetCell.ContainedObject != null)
        {
            return false;
        }

        //remove enemy from current cell
        var currentCell = board.GetCellData(m_Cell);
        currentCell.ContainedObject = null;

        //add it to the next cell
        targetCell.ContainedObject = this;
        m_Cell = coord;
        transform.position = board.CellToWorld(coord);

        return true;
    }

    void TurnHappened()
    {
        //We added a public property that return the player current cell!
        var playerCell = GameManager.Instance.PlayerController.Cell;

        int xDist = playerCell.x - m_Cell.x;
        int yDist = playerCell.y - m_Cell.y;

        int absXDist = Mathf.Abs(xDist);
        int absYDist = Mathf.Abs(yDist);

        if ((xDist == 0 && absYDist == 1)
            || (yDist == 0 && absXDist == 1))
        {
            //we are adjacent to the player, attack!
            GameManager.Instance.ChangeFood(-AttackDamage);
        }
        else
        {
            if (absXDist > absYDist)
            {
                if (!TryMoveInX(xDist))
                {
                    //if our move was not successful (so no move and not attack)
                    //we try to move along Y
                    TryMoveInY(yDist);
                }
            }
            else
            {
                if (!TryMoveInY(yDist))
                {
                    TryMoveInX(xDist);
                }
            }
        }
    }

    bool TryMoveInX(int xDist)
    {
        if (xDist == 0)
        {
            return false;
        }

        //try to get closer in x

        //player to our right
        if (xDist > 0)
        {
            return MoveTo(m_Cell + Vector2Int.right);
        }

        //player to our left
        return MoveTo(m_Cell + Vector2Int.left);
    }

    bool TryMoveInY(int yDist)
    {
        if (yDist == 0)
        {
            return false;
        }

        //try to get closer in y

        //player on top
        if (yDist > 0)
        {
            return MoveTo(m_Cell + Vector2Int.up);
        }

        //player below
        return MoveTo(m_Cell + Vector2Int.down);
    }
}
