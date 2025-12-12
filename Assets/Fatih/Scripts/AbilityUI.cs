using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AbilityUI : MonoBehaviour
{
    [System.Serializable]
    public struct AbilityButton
    {
        public MovementType type;
        public Button button;
        public Image iconOrBackground; // Rengini değiştireceğimiz görsel
    }

    public List<AbilityButton> buttons; // Inspector'dan dolduracağız
    
    [Header("Renkler")]
    public Color activeColor = Color.green;
    public Color inactiveColor = Color.gray;

    private void Start()
    {
        // Event'e abone ol (Manager değişince burası çalışsın)
        if (AbilityManager.Instance != null)
        {
            AbilityManager.Instance.OnAbilitiesChanged += UpdateUI;
            
            // Butonlara tıklama özelliği ekle
            foreach (var item in buttons)
            {
                // Lambda ifadesiyle butonun kendi tipini göndermesini sağlıyoruz
                item.button.onClick.AddListener(() => OnButtonClick(item.type));
            }
            
            // İlk açılışta güncelle
            UpdateUI();
        }
    }

    private void OnButtonClick(MovementType type)
    {
        AbilityManager.Instance.ToggleAbility(type);
    }

    private void UpdateUI()
    {
        foreach (var item in buttons)
        {
            bool isActive = AbilityManager.Instance.CanUse(item.type);
            
            // Aktifse Yeşil, Değilse Gri yap
            item.iconOrBackground.color = isActive ? activeColor : inactiveColor;
        }
    }
}