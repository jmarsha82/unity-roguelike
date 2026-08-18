using UnityEngine;
using UnityEngine.UIElements;
public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }
    public BoardManager BoardManager;
    public PlayerController PlayerController;
    public TurnManager TurnManager { get; private set; }
    public UIDocument UIDoc;
    private Label m_FoodLabel;
    private int m_FoodAmount = 100;

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
        BoardManager.Init();
        TurnManager = new TurnManager();
        TurnManager.OnTick += OnTurnHappen;
        PlayerController.Spawn(BoardManager, new Vector2Int(1, 1));

        if (UIDoc != null)
        {
            m_FoodLabel = UIDoc.rootVisualElement.Q<Label>("FoodLabel");
            UpdateFoodLabel();
        }
    }

    void OnTurnHappen()
    {
        m_FoodAmount -= 1;
        UpdateFoodLabel();
    }

    void UpdateFoodLabel()
    {
        if (m_FoodLabel != null)
        {
            m_FoodLabel.text = "Food : " + m_FoodAmount;
        }
    }
}
