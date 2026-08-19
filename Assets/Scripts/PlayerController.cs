using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private BoardManager m_Board;
    private Vector2Int m_CellPosition;
    private Animator m_Animator;
    private bool m_IsGameOver;
    private bool m_IsMoving;
    private Vector3 m_MoveTarget;

    public float MoveSpeed = 5.0f;
    public Vector2Int Cell => m_CellPosition;

    public void Init()
    {
        m_IsGameOver = false;
        m_IsMoving = false;
    }

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
    }

    public void Spawn(BoardManager boardManager, Vector2Int cell)
    {
        m_Board = boardManager;
        MoveTo(cell, true);
    }

    public void MoveTo(Vector2Int cell, bool immediate = false)
    {
        m_CellPosition = cell;

        if (immediate)
        {
            m_IsMoving = false;
            transform.position = m_Board.CellToWorld(m_CellPosition);
        }
        else
        {
            m_IsMoving = true;
            m_MoveTarget = m_Board.CellToWorld(m_CellPosition);
        }

        if (m_Animator != null)
        {
            m_Animator.SetBool("Moving", m_IsMoving);
        }
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
                return true;
            }
        }
        return false;
    }

    private void Update()
    {
        Vector2Int direction = Vector2Int.zero;
        bool hasMoved = false;
        
        if (m_IsGameOver)
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame)
            {
                GameManager.Instance.StartNewGame();
            }

            return;
        }

        if (m_IsMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, m_MoveTarget, MoveSpeed * Time.deltaTime);

            if (transform.position == m_MoveTarget)
            {
                m_IsMoving = false;
                if (m_Animator != null)
                {
                    m_Animator.SetBool("Moving", false);
                }

                var cellData = m_Board.GetCellData(m_CellPosition);
                if (cellData.ContainedObject != null)
                    cellData.ContainedObject.PlayerEntered();
            }

            return;
        }

        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            direction = Vector2Int.up;
            hasMoved = true;
        }
        else if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            direction = Vector2Int.up;
            hasMoved = true;
        }
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            direction = Vector2Int.down;
            hasMoved = true;
        }
        else if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            direction = Vector2Int.down;
            hasMoved = true;
        }
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            direction = Vector2Int.right;
            hasMoved = true;
        }
        else if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            direction = Vector2Int.right;
            hasMoved = true;
        }
        else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            direction = Vector2Int.left;
            hasMoved = true;
        }
        else if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            direction = Vector2Int.left;
            hasMoved = true;
        }

        if (hasMoved)
        {
            TryMove(direction);
        }
        else if (Keyboard.current.spaceKey.wasPressedThisFrame
            || Keyboard.current.periodKey.wasPressedThisFrame)
        {
            GameManager.Instance.WaitTurn();
        }
    }

    public void GameOver()
    {
        m_IsGameOver = true;
    }

}
