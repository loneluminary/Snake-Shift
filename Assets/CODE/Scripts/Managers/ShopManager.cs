using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UI.Pagination;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShopManager : MonoBehaviour
{
    [Title("UI References")]
    [SerializeField] private PagedRect scrollRect;
    [SerializeField] private TMP_Text actionText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private GameObject ownedIndicator;

    [Title("Characters")]
    [SerializeField] private List<CharacterData> characters = new();

    [Title("Model Display")]
    [SerializeField] private Transform modelSpawnPoint;
    [SerializeField] private float rotationSensitivity = 0.2f;
    [SerializeField] private float maxRotationAngle = 90f;

    private Transform _currentModel;
    private int _currentCharacterIndex = 0;

    private void Awake()
    {
        InitializeScrollRect();
        LoadOwnedCharacters();
    }

    private void OnEnable()
    {
        LoadSelectedCharacter();
    }

    private void InitializeScrollRect()
    {
        if (!scrollRect) scrollRect = GetComponentInChildren<PagedRect>();

        scrollRect.PageChangedEvent.AddListener(OnPageChanged);
        for (int i = 0; i < characters.Count; i++) scrollRect.AddPageUsingTemplate();
        scrollRect.SetCurrentPage(1);
    }

    #region Page Management

    private void OnPageChanged(Page previous, Page next)
    {
        _currentCharacterIndex = scrollRect.CurrentPage - 1;

        UpdateCharacterModel();
        UpdateUI();
    }

    private void UpdateCharacterModel()
    {
        // Destroy old model
        if (_currentModel) Destroy(_currentModel.gameObject);

        // Spawn new model
        if (_currentCharacterIndex >= 0 && _currentCharacterIndex < characters.Count)
        {
            Transform modelPrefab = characters[_currentCharacterIndex].Model;
            Vector3 spawnPosition = modelSpawnPoint ? modelSpawnPoint.position : Vector3.zero;
            Quaternion spawnRotation = modelSpawnPoint ? modelSpawnPoint.rotation : Quaternion.identity;

            _currentModel = Instantiate(modelPrefab, spawnPosition, spawnRotation);
            if (modelSpawnPoint) _currentModel.SetParent(modelSpawnPoint, true);
        }
    }

    private void UpdateUI()
    {
        if (_currentCharacterIndex < 0 || _currentCharacterIndex >= characters.Count) return;

        CharacterData character = characters[_currentCharacterIndex];
        bool isOwned = character.IsOwned;

        // Update action button text
        if (actionText) actionText.text = isOwned ? "Select" : "Buy";

        // Update price text
        if (priceText)
        {
            priceText.gameObject.SetActive(!isOwned);
            priceText.text = $"${character.Price}";
        }

        // Update owned indicator
        if (ownedIndicator) ownedIndicator.SetActive(isOwned);
    }

    #endregion

    #region Buy/Select System

    public void TryBuyOrSelect()
    {
        if (_currentCharacterIndex < 0 || _currentCharacterIndex >= characters.Count) return;

        CharacterData character = characters[_currentCharacterIndex];

        if (!character.IsOwned)
        {
            TryBuyCharacter(character);
        }
        else
        {
            SelectCharacter();
        }
    }

    private void TryBuyCharacter(CharacterData character)
    {
        if (CoinsManager.Instance.RemoveCoins(character.Price))
        {
            // Mark as owned
            character.IsOwned = true;
            characters[_currentCharacterIndex] = character;

            // Save to PlayerPrefs
            SaveOwnedCharacter(_currentCharacterIndex);

            // Auto-select after purchase
            SelectCharacter();

            // Update UI
            UpdateUI();

            Debug.Log($"Purchased character: {character.Name} for {character.Price} coins");
        }
    }

    private void SelectCharacter()
    {
        // Save selected character
        PlayerPrefs.SetInt(GameManager.SELECTED_CHARACTER_PREFS, _currentCharacterIndex);
        PlayerPrefs.Save();

        // Apply skin to snake
        SnakeMovement snake = FindFirstObjectByType<SnakeMovement>();
        if (snake)
        {
            snake.LoadSkin();
        }

        Debug.Log($"Selected character index: {_currentCharacterIndex}");
    }

    #endregion

    #region Save/Load System

    private void SaveOwnedCharacter(int characterIndex)
    {
        // Load existing owned characters
        string ownedData = PlayerPrefs.GetString(GameManager.OWNED_CHARACTER_PREFS, "");
        List<int> ownedIndices = ParseOwnedCharacters(ownedData);

        // Add new character if not already owned
        if (!ownedIndices.Contains(characterIndex))
        {
            ownedIndices.Add(characterIndex);
        }

        // Save back to PlayerPrefs
        string savedData = string.Join(",", ownedIndices);
        PlayerPrefs.SetString(GameManager.OWNED_CHARACTER_PREFS, savedData);
        PlayerPrefs.Save();

        Debug.Log($"Saved owned characters: {savedData}");
    }

    private void LoadOwnedCharacters()
    {
        string ownedData = PlayerPrefs.GetString(GameManager.OWNED_CHARACTER_PREFS, "");
        List<int> ownedIndices = ParseOwnedCharacters(ownedData);

        // Mark characters as owned
        foreach (int index in ownedIndices)
        {
            if (index >= 0 && index < characters.Count)
            {
                CharacterData character = characters[index];
                character.IsOwned = true;
                characters[index] = character;
            }
        }

        // First character is always owned (default)
        if (characters.Count > 0 && !characters[0].IsOwned)
        {
            CharacterData firstCharacter = characters[0];
            firstCharacter.IsOwned = true;
            characters[0] = firstCharacter;
            SaveOwnedCharacter(0);
        }

        Debug.Log($"Loaded {ownedIndices.Count} owned characters");
    }

    private void LoadSelectedCharacter()
    {
        int selectedIndex = PlayerPrefs.GetInt(GameManager.SELECTED_CHARACTER_PREFS, 0);

        // Validate index
        if (selectedIndex >= 0 && selectedIndex < characters.Count)
        {
            scrollRect.SetCurrentPage(selectedIndex + 1);
        }
        else
        {
            scrollRect.SetCurrentPage(1);
        }
    }

    private List<int> ParseOwnedCharacters(string data)
    {
        List<int> indices = new();

        if (string.IsNullOrEmpty(data)) return indices;

        string[] parts = data.Split(',');
        foreach (string part in parts)
        {
            if (int.TryParse(part.Trim(), out int index))
            {
                indices.Add(index);
            }
        }

        return indices;
    }

    public void ResetShopData()
    {
        PlayerPrefs.DeleteKey(GameManager.OWNED_CHARACTER_PREFS);
        PlayerPrefs.DeleteKey(GameManager.SELECTED_CHARACTER_PREFS);
        PlayerPrefs.Save();

        // Reset all characters to not owned
        for (int i = 0; i < characters.Count; i++)
        {
            CharacterData character = characters[i];
            character.IsOwned = false;
            characters[i] = character;
        }

        LoadOwnedCharacters(); // Re-apply default owned character
        UpdateUI();

        Debug.Log("Shop data reset!");
    }

    #endregion

    #region Model Rotation

    public void OnDrag(BaseEventData eventData)
    {
        if (!_currentModel || eventData is not PointerEventData pointer) return;

        float deltaX = pointer.delta.y * rotationSensitivity;
        float deltaY = -pointer.delta.x * rotationSensitivity;

        Vector3 currentRotation = _currentModel.eulerAngles;
        currentRotation.x += deltaX;
        currentRotation.y += deltaY;

        // Normalize and clamp angles
        float normalizedX = NormalizeAngle(currentRotation.x);
        float normalizedY = NormalizeAngle(currentRotation.y);

        currentRotation.x = Mathf.Clamp(normalizedX, -maxRotationAngle, maxRotationAngle);
        currentRotation.y = Mathf.Clamp(normalizedY, -maxRotationAngle, maxRotationAngle);
        currentRotation.z = 0f;

        _currentModel.eulerAngles = currentRotation;
    }

    public void OnDragEnd()
    {
        if (_currentModel)
        {
            _currentModel.DORotate(Vector3.zero, 1f).SetEase(Ease.OutQuad);
        }
    }

    private float NormalizeAngle(float angle)
    {
        angle %= 360f;

        return angle switch
        {
            > 180f => angle - 360f,
            <= -180f => angle + 360f,
            _ => angle
        };
    }

    #endregion
}

[System.Serializable]
public struct CharacterData
{
    public string Name;
    public int Price;
    public bool IsOwned;

    public Transform Model;
}