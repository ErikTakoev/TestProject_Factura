using System;
using UnityEngine;

namespace TestProject_Factura
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private LayerMask groundLayerMask;
        
        private Vector2 currentInputPosition;
        private bool isShooting;
        private bool isInputActive;
        
        public bool IsInputActive => isInputActive;
        
        public void EnableInput()
        {
            isInputActive = true;
        }
        
        public void DisableInput()
        {
            isInputActive = false;
            isShooting = false;
        }
        
        private void Start()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }
        
        private void Update()
        {
            if (!isInputActive)
                return;
                
            // Отримуємо дані про введення в залежності від платформи
            ProcessInput();
        }
        
        private void ProcessInput()
        {
            // Обробка введення для PC
            if (Application.platform == RuntimePlatform.WindowsPlayer || 
                Application.platform == RuntimePlatform.OSXPlayer || 
                Application.platform == RuntimePlatform.LinuxPlayer || 
                Application.isEditor)
            {
                ProcessMouseInput();
            }
            // Обробка введення для мобільних пристроїв
            else if (Application.platform == RuntimePlatform.Android || 
                    Application.platform == RuntimePlatform.IPhonePlayer)
            {
                ProcessTouchInput();
            }
        }
        
        private void ProcessMouseInput()
        {
            // Позиція миші в пікселях екрану
            currentInputPosition = Input.mousePosition;
            
            // Перевіряємо натискання лівої кнопки миші для стрільби
            isShooting = Input.GetMouseButton(0);
        }
        
        private void ProcessTouchInput()
        {
            // За замовчуванням не стріляємо
            isShooting = false;
            
            // Перевіряємо, чи є активні дотики
            if (Input.touchCount > 0)
            {
                // Використовуємо перший дотик
                Touch touch = Input.GetTouch(0);
                
                // Зберігаємо позицію дотику
                currentInputPosition = touch.position;
                
                // Якщо дотик активний, стріляємо
                isShooting = (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled);
            }
        }
        
        // Отримати поточну позицію введення у світових координатах
        public Vector3 GetWorldPosition()
        {
            if (mainCamera == null)
                return Vector3.zero;
                
            Ray ray = mainCamera.ScreenPointToRay(currentInputPosition);
            
            // Якщо вказано маску шару для землі, використовуємо її для визначення точки перетину
            if (groundLayerMask.value != 0)
            {
                if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayerMask))
                {
                    return hit.point;
                }
            }
            
            // Якщо маска не вказана або перетин не знайдено, використовуємо плоску площину
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            if (groundPlane.Raycast(ray, out float distance))
            {
                return ray.GetPoint(distance);
            }
            
            return Vector3.zero;
        }
        
        // Перевірити, чи активна стрільба
        public bool IsShooting()
        {
            return isInputActive && isShooting;
        }
        
        // Отримати напрямок від поточної позиції камери до точки введення
        public Vector3 GetDirectionFromCamera()
        {
            if (mainCamera == null)
                return Vector3.forward;
                
            Vector3 worldPos = GetWorldPosition();
            Vector3 direction = worldPos - mainCamera.transform.position;
            return direction.normalized;
        }
    }
} 