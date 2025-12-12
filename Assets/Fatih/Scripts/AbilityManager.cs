using System.Collections.Generic;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    // Aktif yetenekleri tutan liste
    public HashSet<MovementType> equippedAbilities = new HashSet<MovementType>();

    // Maksimum slot sayısı (Level atladıkça bunu artırabilirsin)
    public int maxSlots = 3;

    private void Start()
    {
        // ÖRNEK: Oyun başında varsayılan olarak 3 yetenek verelim
        EquipAbility(MovementType.Move);
        EquipAbility(MovementType.Jump);
        EquipAbility(MovementType.Sprint);
        // Crouch eklemedik, yani başta eğilemeyecek.
    }

    public bool EquipAbility(MovementType ability)
    {
        if (equippedAbilities.Count >= maxSlots)
        {
            Debug.Log("Slotlar dolu! Birini çıkarman lazım.");
            return false;
        }

        if (!equippedAbilities.Contains(ability))
        {
            equippedAbilities.Add(ability);
            Debug.Log(ability + " yeteneği eklendi.");
            return true;
        }
        return false;
    }

    public void UnequipAbility(MovementType ability)
    {
        if (equippedAbilities.Contains(ability))
        {
            equippedAbilities.Remove(ability);
            Debug.Log(ability + " yeteneği çıkarıldı.");
        }
    }

    // Kritik Fonksiyon: Bu yetenek bizde var mı?
    public bool CanUse(MovementType ability)
    {
        return equippedAbilities.Contains(ability);
    }
}