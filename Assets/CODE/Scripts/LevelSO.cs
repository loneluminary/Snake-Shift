using Sirenix.OdinInspector;
using UnityEngine;

[System.Flags]
public enum Objectives
{
    None = 0,
    CollectFruits = 1 << 0,
    LimitedMoves = 1 << 1,
    Timer = 1 << 2,
}

[CreateAssetMenu(menuName = "Level Data", fileName = "New Level Data")]
public class LevelSO : ScriptableObject
{
    [Title("Info")]
    public string LevelName = "Level";
    [TextArea] public string Description;

    [Title("Objective")]
    public Objectives enabledObjectives = Objectives.CollectFruits | Objectives.LimitedMoves | Objectives.Timer;
    [ShowIf("_limitedMoves")] public int MaxMoves = 20;
    [ShowIf("_timer")] public float Timer = 30f;
    [ShowIf("_collectFruits")] public int CollectFuits;
    [TextArea, Space] public string ObjectiveMessage;
    
    [Title("Rewards")]
    public int RewardCoins = 10;
    
    [Space(20)] 
    public GameObject Prefab;
    
    #if UNITY_EDITOR
    private bool _limitedMoves => enabledObjectives.HasFlag(Objectives.LimitedMoves);
    private bool _timer => enabledObjectives.HasFlag(Objectives.Timer);
    private bool _collectFruits => enabledObjectives.HasFlag(Objectives.CollectFruits);
    #endif
}