using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private BoardManager m_Board;
    private Vector2Int m_CellPosition;
    private bool m_IsGameOver;

    public void Init()
    {
        m_IsGameOver = false;
    }

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

            if (cellData.ContainedObject == null)
            {
                MoveTo(newCellTarget);
                return true;
            }
            else if (cellData.ContainedObject.PlayerWantsToEnter())
            {
                MoveTo(newCellTarget);
                //Call PlayerEntered AFTER moving the player! Otherwise not in cell yet
                cellData.ContainedObject.PlayerEntered();
                return true;
            }
        }
        return false;
    }

    private void Update()
    {
        if (m_IsGameOver)
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame)
            {
                GameManager.Instance.StartNewGame();
            }

            return;
        }
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

    public void GameOver()
    {
        m_IsGameOver = true;
    }

}
