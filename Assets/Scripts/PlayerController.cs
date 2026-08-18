using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private BoardManager m_Board;
    private Vector2Int m_CellPosition;

    public void Spawn(BoardManager boardManager, Vector2Int cell)
    {
        m_Board = boardManager;
        MoveTo(cell);
    }

    public void MoveTo(Vector2Int cell)
    {
        m_CellPosition = cell;
        transform.position = m_Board.CellToWorld(m_CellPosition);
    }

    public bool TryMove(Vector2Int direction)
    {
        if (m_Board == null)
        {
            Debug.LogWarning("PlayerController tried to move before it was spawned.");
            return false;
        }

        Vector2Int newCellTarget = m_CellPosition + direction;
        BoardManager.CellData cellData = m_Board.GetCellData(newCellTarget);

        if (cellData != null && cellData.Passable)
        {
            GameManager.Instance.TurnManager.Tick();
            MoveTo(newCellTarget);
            return true;
        }

        return false;
    }

    private void Update()
    {
        Vector2Int direction = Vector2Int.zero;
        bool hasMoved = false;

        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            direction = Vector2Int.up;
            hasMoved = true;
        }
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            direction = Vector2Int.down;
            hasMoved = true;
        }
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            direction = Vector2Int.right;
            hasMoved = true;
        }
        else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            direction = Vector2Int.left;
            hasMoved = true;
        }

        if (hasMoved)
        {
            TryMove(direction);
        }
    }

}
