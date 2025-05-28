using System;
using UnityEngine;

public class AvoidMenuUI : MonoBehaviour
{
    [SerializeField] private MenuUIController _menuUIController;
    [SerializeField] private InventoryUIController _inventoryUIController;
    [SerializeField] private DialogueUIController _dialogueUIController;
    [SerializeField] private PasswordUIController _passwordUIController;
    
    private BaseAvoidUIController _activeController;
    public bool IsMenuOpen => _activeController;
    
    private float _timer = 0f; // 타이머
    public int StepCount { get; set; } = 0;
    
    public event Action _onCloseMenu;
    
    
    private void Awake()
    {
        _menuUIController.OnOpenInventory += OpenInventory;
        _menuUIController.OnCloseMenu += CloseMenu;
        
        _dialogueUIController.OnOpenDialogue += OpenDialogue;
        _dialogueUIController.OnCloseDialogue += CloseDialogue;
        
        _passwordUIController.OnClosePasswordUI += ClosePasswordUI;
        _inventoryUIController.OnReturnMenu += CloseInventory;
        
        _menuUIController.Hide();
        _inventoryUIController.Hide();
        _dialogueUIController.Hide();
        _passwordUIController.Hide();
        _activeController = null;
    }
    
    private void Update()
    {
        _timer += Time.unscaledDeltaTime;

        if (_activeController is MenuUIController menuController)
            menuController.UpdateTimer(_timer);
    }
    
    public void OpenMenu(Action onClose = null)
    {
        _menuUIController.Show();
        _menuUIController.SetStepCount(StepCount);
        
        _inventoryUIController.Hide();
        _activeController = _menuUIController;
        Time.timeScale = 0f;
    }
    
    private void CloseMenu()
    {
        _onCloseMenu?.Invoke();
        _menuUIController.Hide();
        _inventoryUIController.Hide();
        _dialogueUIController.Hide();
        _passwordUIController.Hide();
        _activeController = null;
        Time.timeScale = 1f;
    }
    
    private void OpenInventory()
    {
        _menuUIController.Hide();
        _inventoryUIController.Show();
        _activeController = _inventoryUIController;
    }
    
    private void CloseInventory()
    {
        _inventoryUIController.Hide();
        _menuUIController.Show();
        _activeController = _menuUIController;
    }

    private void OpenDialogue()
    {
        _menuUIController.Hide();
        _inventoryUIController.Hide();
        _passwordUIController.Hide();
        _dialogueUIController.Show();
        _activeController = _dialogueUIController;
    }
    
    private void CloseDialogue()
    {
        _dialogueUIController.Hide();
        CloseMenu();
        _activeController = null;
    }

    public void OpenPasswordUI(string password, Action onOpenSafeBox = null)
    {
        _menuUIController.Hide();
        _inventoryUIController.Hide();
        _dialogueUIController.Hide();
        _passwordUIController.Show(password, onOpenSafeBox);
        _activeController = _passwordUIController;
    }
    
    private void ClosePasswordUI()
    {
        _passwordUIController.Hide();
        _onCloseMenu?.Invoke();
        _activeController = null;
    }
    
    #region Player Input - UI

    public void OnMoveUp()
    {
        _activeController?.OnMoveUp();
    }
    
    public void OnMoveDown()
    {
        _activeController?.OnMoveDown();
    }
    
    public void OnMoveLeft()
    {
        _activeController?.OnMoveLeft();
    }
    
    public void OnMoveRight()
    {
        _activeController?.OnMoveRight();
    }
    
    public void OnSubmit()
    {
        _activeController?.OnSubmit();
    }
    
    public void OnCancel()
    {
        _activeController?.OnCancel();
    }

    #endregion

    private void OnDestroy()
    {
        _menuUIController.OnOpenInventory -= OpenInventory;
        _menuUIController.OnCloseMenu -= CloseMenu;
        _inventoryUIController.OnReturnMenu -= CloseInventory;
        _dialogueUIController.OnOpenDialogue -= OpenDialogue;
        _dialogueUIController.OnCloseDialogue -= CloseDialogue;
        
        Time.timeScale = 1f;
    }
}
