using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// Tracks time, moves, and apple objectives; integrates with win/fail UI; exposes power-ups;
/// and includes legacy signatures used by LevelManager/SpiralGate/SnakeMovement.
public class ObjectiveTracker : MonoBehaviour
{
    [Header("Time Objective")]
    public bool  useTimeLimit = false;
    [Tooltip("Total time (seconds) when the level starts (if time objective is on)")]
    public float timeLimitSeconds = 60f;

    [Header("Moves (Taps) Objective")]
    public bool useMovesLimit = false;
    [Tooltip("Maximum allowed moves (taps) for the level")]
    public int movesBudget = 30;

    [Header("Apples Objective")]
    [Tooltip("Total apples required in this level (0 = not required)")]
    public int applesTotal = 0;

    [Header("Single Objective Text Output (assign ONE of these)")]
    [Tooltip("TextMeshPro field to display combined objective info, e.g. ⏱ 00:32   🌀 12 moves   🍎 3/5")]
    public TMP_Text objectiveTextTMP;
    [Tooltip("Legacy Text field alternative (if not using TMP)")]
    public Text     objectiveText;

    [Header("Formatting")]
    [Tooltip("mm:ss or ss")]
    public string timeFormat   = "mm':'ss";
    [Tooltip("Label used when showing remaining moves")]
    public string movesLabel   = "moves";
    [Tooltip("Apple icon/label")]
    public string applesIcon   = "🍎";
    [Tooltip("Separator between parts (used only when a single objective is active)")]
    public string separator    = "   ";   // three spaces

    [Header("Events (optional hooks for SFX/VFX/Toasts)")]
    public UnityEvent OnOutOfTime;
    public UnityEvent OnOutOfMoves;
    public UnityEvent OnApplesMissingAtSpiral;
    
    public float TimeRemaining   { get; private set; }
    public int   MovesUsed       { get; private set; }
    public int   ApplesCollected { get; private set; }
    public int   ApplesRemaining => Mathf.Max(0, applesTotal - ApplesCollected);

    // Internals
    private bool _failing     = false;
    private bool _won         = false;
    private bool _timerPaused = false;

    // Cache of which objectives are enabled by meta (if your LevelMeta uses this)
    private Objectives _enabledMask = Objectives.None;

    void Start()
    {
        ResetStateAndUI();
    }

    void Update()
    {
        if (_failing || _won) return;

        if (useTimeLimit && !_timerPaused)
        {
            TimeRemaining -= Time.deltaTime;
            if (TimeRemaining <= 0f)
            {
                TimeRemaining = 0f;
                UpdateAllObjectivesText();
                TriggerFail(ObjectiveFailReason.OutOfTime, "Time is up!");
                return;
            }
            UpdateAllObjectivesText();
        }
    }
    
    /// <summary>Call whenever a move/tap should count against the budget.</summary>
    public void ConsumeMove()
    {
        if (_failing || _won) return;
        if (!useMovesLimit) return;

        MovesUsed++;
        UpdateAllObjectivesText();

        if (MovesUsed > movesBudget)
        {
            TriggerFail(ObjectiveFailReason.OutOfMoves, "No moves remaining!");
        }
    }

    /// <summary>Refund moves (e.g., after Undo).</summary>
    public void RefundMove(int count = 1)
    {
        if (count <= 0) return;
        MovesUsed = Mathf.Max(0, MovesUsed - count);
        UpdateAllObjectivesText();
    }

    /// <summary>Power-up: add seconds. If cap > 0, clamp to cap.</summary>
    public void AddTimeSeconds(float seconds, float cap = 0f)
    {
        if (!useTimeLimit) return;
        if (seconds <= 0f) return;

        TimeRemaining += seconds;
        if (cap > 0f) TimeRemaining = Mathf.Min(TimeRemaining, cap);
        UpdateAllObjectivesText();
    }

    /// <summary>Power-up: expand moves budget. If cap > 0, clamp.</summary>
    public void AddMovesBudget(int extra, int cap = 0)
    {
        if (!useMovesLimit) return;
        if (extra <= 0) return;

        movesBudget += extra;
        if (cap > 0) movesBudget = Mathf.Min(movesBudget, cap);
        UpdateAllObjectivesText();
    }

    /// <summary>Call when an apple is collected.</summary>
    public void OnFruitCollected(int count = 1)
    {
        if (_failing || _won) return;
        ApplesCollected += Mathf.Max(1, count);
        ApplesCollected = Mathf.Clamp(ApplesCollected, 0, applesTotal);
        UpdateAllObjectivesText();
    }

    /// <summary>
    /// Non-legacy path: call when the snake actually finishes entering the spiral (after gate).
    /// </summary>
    public void OnReachedSpiral()
    {
        if (_failing || _won) return;

        if (ApplesRemaining > 0)
        {
            OnApplesMissingAtSpiral?.Invoke();
            TriggerFail(ObjectiveFailReason.ApplesMissing, "Collect all apples before entering the spiral!");
            return;
        }

        _won = true;
        GameManager.Instance.OnGameWin?.Invoke();
    }

    private void UpdateAllObjectivesText()
    {
        // Build parts independently
        var parts = new List<string>();

        if (useTimeLimit)
        {
            parts.Add($"Time: {FormatTime(TimeRemaining)}");  // ✅ Added label
        }

        if (useMovesLimit)
        {
            int remaining = Mathf.Max(0, movesBudget - MovesUsed);
            parts.Add($"{remaining} {movesLabel}");
        }

        if (applesTotal > 0)
        {
            parts.Add($"{applesIcon} {ApplesCollected}/{applesTotal}");
        }

        string display;
        if (parts.Count <= 1)
        {
            // Single objective -> keep single-line (backwards compatible)
            display = parts.Count == 1 ? parts[0] : "";
        }
        else
        {
            // Multiple objectives -> one per line
            display = string.Join("\n", parts);
        }

        if (objectiveTextTMP) objectiveTextTMP.text = display;
        if (objectiveText)    objectiveText.text    = display;
    }

    private string FormatTime(float seconds)
    {
        seconds = Mathf.Max(0f, seconds);
        int s = Mathf.FloorToInt(seconds % 60f);
        int m = Mathf.FloorToInt(seconds / 60f);
        switch (timeFormat)
        {
            case "ss":       return Mathf.CeilToInt(seconds).ToString();
            case "mm':'ss":
            default:         return $"{m:00}:{s:00}";
        }
    }
    
    private void TriggerFail(ObjectiveFailReason reason, string message)
    {
        if (_failing || _won) return;
        _failing = true;

        switch (reason)
        {
            case ObjectiveFailReason.OutOfTime:     OnOutOfTime?.Invoke(); break;
            case ObjectiveFailReason.OutOfMoves:    OnOutOfMoves?.Invoke(); break;
            case ObjectiveFailReason.ApplesMissing: OnApplesMissingAtSpiral?.Invoke(); break;
        }

        GameManager.Instance.OnGameLose?.Invoke(reason.ToString());
    }
    
    /// Initializes which objectives are active based on LevelMeta and the spawned level root.
    public void BeginLevel(LevelSO so, GameObject levelInstance)
    {
        if (!so)
        {
            _enabledMask = Objectives.None;
            useTimeLimit = false;
            useMovesLimit = false;
            applesTotal = 0;
            ResetStateAndUI();
            return;
        }

        _enabledMask = so.enabledObjectives;

        // Apples
        bool applesOn = (_enabledMask & Objectives.CollectFruits) != 0;
        if (applesOn)
        {
            applesTotal = so.CollectFuits > 0 ? so.CollectFuits : CountApplesIn(levelInstance);
        }
        else applesTotal = 0;

        // Moves
        bool movesOn = (_enabledMask & Objectives.LimitedMoves) != 0;
        useMovesLimit = movesOn;
        movesBudget = Mathf.Max(1, so.MaxMoves);

        // Time
        bool timeOn = (_enabledMask & Objectives.Timer) != 0;
        useTimeLimit = timeOn;
        timeLimitSeconds = Mathf.Max(0.1f, so.Timer);

        // Fresh state
        _timerPaused = false;
        _failing = false;
        _won = false;
        MovesUsed = 0;
        ApplesCollected = 0;
        TimeRemaining = useTimeLimit ? timeLimitSeconds : 0f;

        UpdateAllObjectivesText();
        // NEW: Update power-up buttons’ CanvasGroups for this round
        var buttons = Object.FindObjectsByType<PowerupButton>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (buttons != null)
        {
            foreach (var b in buttons) if (b) b.RefreshByObjectives(this);
        }
    }

    /// Used by SpiralGate to see if the player can enter right now.
    /// Returns false and a message when blocked.
    public bool CanEnterSpiral(out string reason)
    {
        // Apples first (explicit requirement)
        if (ApplesRemaining > 0)
        {
            reason = $"Collect all apples ({ApplesCollected}/{applesTotal}).";
            return false;
        }

        // Moves budget
        if (useMovesLimit && MovesUsed > movesBudget)
        {
            reason = "No moves remaining.";
            return false;
        }

        // Time budget
        if (useTimeLimit && TimeRemaining <= 0f)
        {
            reason = "Out of time.";
            return false;
        }

        reason = null;
        return true;
    }

    /// <summary>SpiralGate stops the countdown once the exit is reached/animating.</summary>
    public void StopTimer() => _timerPaused = true;

    /// <summary>SnakeMovement calls this when a counted move is made.</summary>
    public void OnMoveUsed() => ConsumeMove();
    
    public void ResetStateAndUI()
    {
        _failing = false;
        _won = false;
        _timerPaused = false;

        TimeRemaining   = useTimeLimit ? Mathf.Max(0f, timeLimitSeconds) : 0f;
        MovesUsed       = 0;
        ApplesCollected = 0;

        UpdateAllObjectivesText();
    }

    private int CountApplesIn(GameObject root)
    {
        if (!root) return 0;
        int count = 0;
        var colls = root.GetComponentsInChildren<Collider>(true);
        foreach (var c in colls)
            if (c && c.gameObject.CompareTag("Apple")) count++;
        return count;
    }
}

/// <summary>Reasons why the level failed; used by GameManager to present messages/icons.</summary>
public enum ObjectiveFailReason
{
    OutOfTime,
    OutOfMoves,
    ApplesMissing
}
