using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace TestProject_Factura
{
    public class InputHandler : MonoBehaviour
    {
        private ITurretController turretController;
        private InputManager inputManager;

        private bool isGameStarted = false;

        [Inject]
        private void Construct(ITurretController turret, InputManager input)
        {
            turretController = turret;
            inputManager = input;
        }

        private void Start()
        {
            // Підписуємося на події зміни стану гри
            GameEvents.OnGameStateChanged += OnGameStateChanged;
        }

        private void OnDestroy()
        {
            // Відписуємося від подій
            GameEvents.OnGameStateChanged -= OnGameStateChanged;
        }

        private void Update()
        {
            if (!isGameStarted || inputManager == null)
                return;

            if (inputManager.IsInputActive)
            {
                // Оновлюємо обертання турелі на основі позиції вводу
                if (turretController != null)
                {
                    turretController.UpdateRotation(inputManager.GetWorldPosition());
                }

                // Перевіряємо стрільбу
                if (inputManager.IsShooting() && turretController != null && turretController.CanShoot)
                {
                    turretController.Shoot().Forget();
                }
            }
        }

        private void OnGameStateChanged(GameState newState)
        {
            // Активуємо/деактивуємо обробку вводу в залежності від стану гри
            if (newState == GameState.Playing)
            {
                isGameStarted = true;
                inputManager?.EnableInput();
            }
            else
            {
                isGameStarted = false;
                inputManager?.DisableInput();
            }
        }

        public void StartGame()
        {
            isGameStarted = true;
            inputManager?.EnableInput();
        }
    }
}
