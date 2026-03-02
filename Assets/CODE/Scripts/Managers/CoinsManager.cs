using UnityEngine;

public class CoinsManager : Singleton<CoinsManager>
{
    [SerializeField] int startingCoins = 100;
    public int CurrentCoins;

    private void Awake()
    {
        AddCoins(PlayerPrefs.GetInt(GameManager.COINS_PREFS, startingCoins));
        
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }
    
    public  void AddCoins(int amount)
    {
        CurrentCoins += amount;
        PlayerPrefs.SetInt(GameManager.COINS_PREFS, CurrentCoins);
        
        UIManager.Instance.UpdateCoinsText(true);
    }
    
    public bool RemoveCoins(int amount)
    {
        if (CurrentCoins < amount)
        {
            UIManager.Instance.ShowToastMessage("Not Enough Coins Available.");
            return false;
        }
        
        CurrentCoins -= amount;
        PlayerPrefs.SetInt(GameManager.COINS_PREFS, CurrentCoins);
        
        UIManager.Instance.UpdateCoinsText(false);

        return true;
    }
}