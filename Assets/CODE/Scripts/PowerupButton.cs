using DTT.Utils.Extensions;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum PowerupType
{
    AddTime,
    AddMoves,
    UndoStep,
    SkipLevel
}

[RequireComponent(typeof(Button))]
public class PowerupButton : MonoBehaviour
{
    public PowerupType Type;
    public int Cost = 20;
    
    [Title("Labels")]
    [SerializeField] TMP_Text label;
    [SerializeField] TMP_Text priceLabel;

    [Title("Amounts")] 
    [SerializeField] float addTimeSeconds = 5f;
    [SerializeField] int addMovesCount = 3;

    [Title("Caps (optional)")] 
    public float timeLimitCap; // 0 => no cap
    public int movesLimitCap; // 0 => no cap
    
    private Button btn;

    private void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
        
        UpdateLabel();
    }

    private void OnEnable()
    {
        RefreshByObjectives(FindFirstObjectByType<ObjectiveTracker>());
        UpdateLabel();
    }

    public void RefreshByObjectives(ObjectiveTracker tracker)
    {
        bool active = true;
        if (tracker)
        {
            active = Type switch
            {
                PowerupType.AddTime => tracker.useTimeLimit,
                PowerupType.AddMoves => tracker.useMovesLimit,
                _ => active
            };
        }
            
        gameObject.SetActive(active);
    }

    private void UpdateLabel()
    {
        if (label) label.text = Type.ToString().AddSpacesBeforeCapitals();
        if (priceLabel) priceLabel.text = Cost.ToString();
    }

    public void OnClick()
    {
        switch (Type)
        {
            case PowerupType.AddTime:
            {
                if (!CoinsManager.Instance.RemoveCoins(Cost))
                {
                    UIManager.Instance.ShowToastMessage($"Not enough coins ({Cost})");
                    return;
                }

                FindFirstObjectByType<ObjectiveTracker>()?.AddTimeSeconds(addTimeSeconds, timeLimitCap);
                break;
            }
            case PowerupType.AddMoves:
            {
                if (!CoinsManager.Instance.RemoveCoins(Cost))
                {
                    UIManager.Instance.ShowToastMessage($"Not enough coins ({Cost})");
                    return;
                }

                FindFirstObjectByType<ObjectiveTracker>()?.AddMovesBudget(addMovesCount, movesLimitCap);
                break;
            }
            case PowerupType.UndoStep:
            {
                var snake = FindFirstObjectByType<SnakeMovement>();
                if (!snake)
                {
                    UIManager.Instance.ShowToastMessage("No snake found");
                    return;
                }
                
                if (!CoinsManager.Instance.RemoveCoins(Cost))
                {
                    UIManager.Instance.ShowToastMessage($"Not enough coins ({Cost})");
                    return;
                }

                if (!snake.TryUndo()) CoinsManager.Instance.AddCoins(Cost);
                break;
            }
            case PowerupType.SkipLevel:
            {
                if (!CoinsManager.Instance.RemoveCoins(Cost))
                {
                    UIManager.Instance.ShowToastMessage($"Not enough coins ({Cost})");
                    return;
                }

                GameManager.Instance.UnlockNextLevel();
                GameManager.Instance.Restart();
                break;
            }
        }
    }
}