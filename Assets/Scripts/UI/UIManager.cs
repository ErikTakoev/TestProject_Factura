using System;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using Cysharp.Threading.Tasks;

namespace TestProject_Factura
{
    public class UIManager : MonoBehaviour
    {
        [Header("Screens")]
        [SerializeField] private GameObject gameOverScreen;
        [SerializeField] private GameObject winScreen;
        [SerializeField] private GameObject gameplayUI;
        [SerializeField] private GameObject menuScreen;
        
        [Header("Health UI")]
        [SerializeField] private CarHealthUI carHealthUI;
        
        [Header("Buttons")]
        [SerializeField] private Button restartButton;
        [SerializeField] private Button startButton;
        
        private IGameManager gameManager;
        
        [Inject]
        private void Construct(IGameManager manager, CarConfig carConfig)
        {
            gameManager = manager;
            
            if (carHealthUI != null)
            {
                carHealthUI.Initialize(carConfig.maxHP);
            }
        }
        
        private void Start()
        {
            // Підписуємося на події зміни стану гри
            GameEvents.OnGameStateChanged += OnGameStateChanged;
            
            // Налаштовуємо кнопки
            SetupButtons();
            
            // Початковий стан UI
            ShowMenuScreen();
        }
        
        private void OnDestroy()
        {
            // Відписуємося від подій
            GameEvents.OnGameStateChanged -= OnGameStateChanged;
        }
        
        private void SetupButtons()
        {
            if (startButton != null)
            {
                startButton.onClick.AddListener(() => gameManager.StartGame().Forget());
            }
            
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(() => gameManager.RestartGame());
            }
        }
        
        private void OnGameStateChanged(GameState newState)
        {
            switch (newState)
            {
                case GameState.Menu:
                    ShowMenuScreen();
                    break;
                case GameState.Playing:
                    ShowGameplayUI();
                    break;
                case GameState.GameOver:
                    ShowGameOverScreen();
                    break;
                case GameState.Win:
                    ShowWinScreen();
                    break;
            }
        }
        
        private void ShowMenuScreen()
        {
            menuScreen?.SetActive(true);
            gameplayUI?.SetActive(false);
            gameOverScreen?.SetActive(false);
            winScreen?.SetActive(false);
        }
        
        private void ShowGameplayUI()
        {
            menuScreen?.SetActive(false);
            gameplayUI?.SetActive(true);
            gameOverScreen?.SetActive(false);
            winScreen?.SetActive(false);
        }
        
        private void ShowGameOverScreen()
        {
            menuScreen?.SetActive(false);
            gameplayUI?.SetActive(false);
            gameOverScreen?.SetActive(true);
            winScreen?.SetActive(false);
        }
        
        private void ShowWinScreen()
        {
            menuScreen?.SetActive(false);
            gameplayUI?.SetActive(false);
            gameOverScreen?.SetActive(false);
            winScreen?.SetActive(true);
        }
    }
} 