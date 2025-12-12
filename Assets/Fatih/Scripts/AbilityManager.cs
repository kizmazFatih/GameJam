using System.Collections.Generic;
using UnityEngine;
using System;

public class AbilityManager : MonoBehaviour
{
    public static AbilityManager Instance; // Singleton yapalım, her yerden ulaşalım

    [Header("Ayarlar")]
    public int maxSlots = 3;
    
    // O an aktif olan yeteneklerin listesi
    // HashSet kullanıyoruz çünkü aynı yetenekten 2 tane olamaz ve araması çok hızlı
    private HashSet<MovementType> activeAbilities = new HashSet<MovementType>();

    // UI'ın kendini güncellemesi için bir Event (Olay)
    public event Action OnAbilitiesChanged;

    private void Awake()
    {
        // Basit Singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Başlangıçta varsayılan yetenekleri ekleyelim (İstersen boş bırak)
        ToggleAbility(MovementType.Move);
        ToggleAbility(MovementType.Jump);
        ToggleAbility(MovementType.Sprint);
    }

    // Yeteneği Aç/Kapa (Toggle)
    public void ToggleAbility(MovementType type)
    {
        // Eğer zaten varsa -> ÇIKAR
        if (activeAbilities.Contains(type))
        {
            activeAbilities.Remove(type);
        }
        // Yoksa -> EKLE (Ama yer varsa)
        else
        {
            if (activeAbilities.Count < maxSlots)
            {
                activeAbilities.Add(type);
            }
            else
            {
                Debug.Log("Slotlar dolu! Birini çıkarman lazım.");
                // Buraya "Bip" sesi veya uyarı efekti ekleyebilirsin
                return; 
            }
        }

        // Değişiklik oldu, UI'a haber ver
        OnAbilitiesChanged?.Invoke();
    }

    // Diğer scriptler (FPSMovement) bunu soracak
    public bool CanUse(MovementType type)
    {
        return activeAbilities.Contains(type);
    }
    
    // UI için kaç slot dolu bilgisi
    public int GetActiveCount()
    {
        return activeAbilities.Count;
    }
}