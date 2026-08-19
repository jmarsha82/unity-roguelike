using UnityEngine;
using UnityEngine.UIElements;
public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }
    public BoardManager BoardManager;
    public PlayerController PlayerController;
    public TurnManager TurnManager { get; private set; }
    public UIDocument UIDoc;
    public int StartingFood = 20;
    public int LevelFoodBonus = 5;
    public int EnemyScoreValue = 25;
    private Label m_FoodLabel;
    private int m_FoodAmount;
    private int m_CurrentLevel = 1;
    private int m_Score;
    private bool m_IsGameOver;
    private VisualElement m_GameOverPanel;
    private Label m_GameOverMessage;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        TurnManager = new TurnManager();
        TurnManager.OnTick += OnTurnHappen;

        if (UIDoc != null)
        {
            m_FoodLabel = UIDoc.rootVisualElement.Q<Label>("FoodLabel");

            m_GameOverPanel = UIDoc.rootVisualElement.Q<VisualElement>("GameOverPanel");
            if (m_GameOverPanel != null)
            {
                m_GameOverMessage = m_GameOverPanel.Q<Label>("GameOverMessage");
            }
        }

        StartNewGame();
    }

    void OnTurnHappen()
    {
        ChangeFood(-1);
    }

    public void ChangeFood(int amount)
    {
        if (m_IsGameOver)
        {
            return;
        }

        m_FoodAmount += amount;
        UpdateFoodLabel();
    }

    public void WaitTurn()
    {
        if (m_IsGameOver || TurnManager == null)
        {
            return;
        }

        TurnManager.Tick();
    }

    public void EnemyDefeated()
    {
        m_Score += EnemyScoreValue;
        UpdateFoodLabel();
    }

    void UpdateFoodLabel()
    {
        if (m_FoodLabel != null)
        {
            int turnCount = TurnManager != null ? TurnManager.TurnCount : 1;
            m_FoodLabel.text = "Food : " + m_FoodAmount
                + " | Level : " + m_CurrentLevel
                + " | Turn : " + turnCount
                + " | Score : " + m_Score;

            if (m_FoodAmount <= 0)
            {
                EndGame();
            }
        }
        else if (m_FoodAmount <= 0)
        {
            EndGame();
        }
    }

    void EndGame()
    {
        if (m_IsGameOver)
        {
            return;
        }

        m_IsGameOver = true;
        PlayerController.GameOver();

        if (m_GameOverPanel != null)
        {
            m_GameOverPanel.style.visibility = Visibility.Visible;
        }

        if (m_GameOverMessage != null)
        {
            m_GameOverMessage.text = "Game Over!\n\nYou traveled through " + m_CurrentLevel
                + " levels\nScore : " + m_Score;
        }
    }

    public void StartNewGame()
    {
        m_IsGameOver = false;

        if (m_GameOverPanel != null)
        {
            m_GameOverPanel.style.visibility = Visibility.Hidden;
        }

        m_CurrentLevel = 1;
        m_Score = 0;
        m_FoodAmount = StartingFood;

        if (TurnManager != null)
        {
            TurnManager.Reset();
        }

        BoardManager.Clean();
        BoardManager.Init(m_CurrentLevel);

        PlayerController.Init();
        PlayerController.Spawn(BoardManager, new Vector2Int(1, 1));
        UpdateFoodLabel();
    }

    public void NewLevel()
    {
        BoardManager.Clean();
        m_CurrentLevel++;
        m_FoodAmount += LevelFoodBonus;
        BoardManager.Init(m_CurrentLevel);

        PlayerController.Spawn(BoardManager, new Vector2Int(1, 1));
        UpdateFoodLabel();
    }
}
