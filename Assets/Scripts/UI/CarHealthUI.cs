using System;
using UnityEngine;
using UnityEngine.UI;

namespace TestProject_Factura
{
    public class CarHealthUI : MonoBehaviour
    {
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Image fillImage;
        [SerializeField] private Gradient healthGradient;
        
        private void OnEnable()
        {
            // Підписуємося на події зміни HP автомобіля
            GameEvents.OnCarHPChanged += UpdateHealthBar;
        }
        
        private void OnDisable()
        {
            // Відписуємося від подій
            GameEvents.OnCarHPChanged -= UpdateHealthBar;
        }
        
        private void UpdateHealthBar(float currentHP)
        {
            if (healthSlider == null)
                return;
                
            // Оновлюємо значення слайдера
            healthSlider.value = currentHP;
            
            // Змінюємо колір заповнення в залежності від кількості HP
            if (fillImage != null && healthGradient != null)
            {
                float normalizedHP = currentHP / healthSlider.maxValue;
                fillImage.color = healthGradient.Evaluate(normalizedHP);
            }
        }
        
        public void Initialize(float maxHP)
        {
            if (healthSlider != null)
            {
                healthSlider.maxValue = maxHP;
                healthSlider.value = maxHP;
                
                // Змінюємо колір заповнення на початковий
                if (fillImage != null && healthGradient != null)
                {
                    fillImage.color = healthGradient.Evaluate(1f);
                }
            }
        }
    }
} 