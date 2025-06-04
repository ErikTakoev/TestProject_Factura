using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TestProject_Factura
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private LayerMask groundLayerMask;

        // Reference to the Input Actions asset
        private GameplayControls gameplayControls;
        private InputAction positionAction;
        private InputAction shootAction;

        private bool isShooting;
        private bool isInputActive;

        public bool IsInputActive => isInputActive;

        private void Awake()
        {
            // Initialize the Input Actions
            gameplayControls = new GameplayControls();

            // Get references to specific actions
            positionAction = gameplayControls.Gameplay.Position;
            shootAction = gameplayControls.Gameplay.Shoot;

            // Set up callbacks
            shootAction.performed += ctx => isShooting = ctx.ReadValueAsButton();
            shootAction.canceled += ctx => isShooting = false;
        }

        public void EnableInput()
        {
            isInputActive = true;
            gameplayControls.Gameplay.Enable();
        }

        public void DisableInput()
        {
            isInputActive = false;
            isShooting = false;
            gameplayControls.Gameplay.Disable();
        }

        private void OnEnable()
        {
            // Enable the input actions when component is enabled
            if (isInputActive)
            {
                gameplayControls.Gameplay.Enable();
            }
        }

        private void OnDisable()
        {
            // Disable the input actions when component is disabled
            gameplayControls.Gameplay.Disable();
        }

        // Отримати поточну позицію введення у світових координатах
        public Vector3 GetWorldPosition()
        {
            if (mainCamera == null)
                return Vector3.zero;

            var currentInputPosition = positionAction.ReadValue<Vector2>();
            Ray ray = mainCamera.ScreenPointToRay(currentInputPosition);

            // Якщо вказано маску шару для землі, використовуємо її для визначення точки перетину
            if (groundLayerMask.value != 0)
            {
                if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayerMask))
                {
                    return hit.point;
                }
            }
            return Vector3.zero;
        }

        // Перевірити, чи активна стрільба
        public bool IsShooting()
        {
            return isInputActive && isShooting;
        }
    }
}