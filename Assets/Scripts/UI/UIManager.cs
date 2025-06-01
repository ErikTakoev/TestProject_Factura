using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace TestProject_Factura
{
    public class UIManager : MonoBehaviour
    {
        [Header("Screens")]
        [SerializeField] private GameObject gameOverScreen;
        [SerializeField] private GameObject winScreen;
        [SerializeField] private GameObject menuScreen;

        [Header("Gameplay UI")]
        [SerializeField] private GameObject gameplayUI;
        [SerializeField] private TextMeshProUGUI bulletCountText;

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
            // Підписуємося на події зміни стану гри
            GameEvents.OnGameStateChanged += OnGameStateChanged;
            GameEvents.OnBulletCountChanged += OnBulletCountChanged;
        }

        private void Start()
        {
            // Налаштовуємо кнопки
            SetupButtons();

            // Початковий стан UI
            ShowMenuScreen();
        }

        private void OnDestroy()
        {
            // Відписуємося від подій
            GameEvents.OnGameStateChanged -= OnGameStateChanged;
            GameEvents.OnBulletCountChanged -= OnBulletCountChanged;
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

        private void OnBulletCountChanged(int count)
        {
            if (bulletCountText != null)
            {
                bulletCountText.text = count.ToString();
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
            carHealthUI?.gameObject.SetActive(false);
            gameOverScreen?.SetActive(false);
            winScreen?.SetActive(false);
        }

        private void ShowGameplayUI()
        {
            menuScreen?.SetActive(false);
            gameplayUI?.SetActive(true);
            carHealthUI?.gameObject.SetActive(true);
            gameOverScreen?.SetActive(false);
            winScreen?.SetActive(false);
        }

        private void ShowGameOverScreen()
        {
            menuScreen?.SetActive(false);
            gameplayUI?.SetActive(false);
            carHealthUI?.gameObject.SetActive(false);
            gameOverScreen?.SetActive(true);
            winScreen?.SetActive(false);
        }

        private void ShowWinScreen()
        {
            menuScreen?.SetActive(false);
            gameplayUI?.SetActive(false);
            carHealthUI?.gameObject.SetActive(false);
            gameOverScreen?.SetActive(false);
            winScreen?.SetActive(true);
        }
    }
}