using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Inventory", menuName = "ScriptableObjects/Inventory", order = 1)]
public class SOInventory : ScriptableObject
{
    public List<Slot> slots = new List<Slot>();

    public void AddItem(SOItem new_item, int new_amount)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            Slot slot = slots[i];

            if (slot.isFull == false)
            {
                slot.AddItemToSlot(new_item, new_amount);
                InventoryController.instance.UpdateSlotUI(i);
                break;
            }
            else if (slot.item == new_item)
            {
                if (slot.amount < new_item.max_stack)
                {
                    int total_amount = slot.amount + new_amount;
                    if (total_amount > new_item.max_stack)
                    {
                        slot.amount = new_item.max_stack;
                        var extra_amount = total_amount - new_item.max_stack;
                        InventoryController.instance.UpdateSlotUI(i);
                        AddItem(new_item, extra_amount);
                        
                        break;
                    }
                    else
                    {
                        slot.amount = total_amount;
                        InventoryController.instance.UpdateSlotUI(i);
                    }

                    break;
                }
            }
        }
    }


}

[System.Serializable]
public class Slot
{
    public bool isFull = false;
    public Texture item_image;
    public GameObject prefab;
    public SOItem item;
    public int amount;


    public void AddItemToSlot(SOItem new_item, int new_amount)
    {
        isFull = true;

        item = new_item;
        amount = new_amount;
        item_image = item.my_image;
        prefab = item.my_prefab;
    }

}
