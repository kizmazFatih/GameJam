using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{
   public static InventoryController instance;

   public SOInventory player_inventory;


   private bool is_open = false;

   [SerializeField] private Transform bottom_slots_parent;

   public List<Transform> T_slots = new List<Transform>();

   public GameObject UI_prefab;


   private void Awake()
   {
      if (instance == null)
      {
         instance = this;
      }
      else if (instance != this)
      {
         Destroy(gameObject);
      }
      
      for (int i = 0; i < bottom_slots_parent.childCount; i++)
      {
         T_slots.Add(bottom_slots_parent.GetChild(i));
      }

      
   }
  


   public void UpdateSlotUI(int slot_index)
   {
      if (T_slots[slot_index].childCount == 0)
      {
         Instantiate(UI_prefab, T_slots[slot_index]);
      }

      T_slots[slot_index].GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = player_inventory.slots[slot_index].amount.ToString();

      T_slots[slot_index].GetChild(0).GetComponent<RawImage>().texture = player_inventory.slots[slot_index].item_image; 

      //Handle.instance.SetHandlePrefab();

   }

   public void ChangeSlotsEach(Transform slot1, Transform slot2)
   {
      int x = T_slots.IndexOf(slot1);
      int y = T_slots.IndexOf(slot2);
      //Debug.Log(y);

      Slot temple = new Slot();

      temple.isFull = player_inventory.slots[x].isFull;
      temple.amount = player_inventory.slots[x].amount;
      temple.item = player_inventory.slots[x].item;
      temple.item_image = player_inventory.slots[x].item_image;
      temple.prefab = player_inventory.slots[x].prefab;


      player_inventory.slots[x].isFull = player_inventory.slots[y].isFull;
      player_inventory.slots[x].amount = player_inventory.slots[y].amount;
      player_inventory.slots[x].item = player_inventory.slots[y].item;
      player_inventory.slots[x].item_image = player_inventory.slots[y].item_image;
      player_inventory.slots[x].prefab = player_inventory.slots[y].prefab;

      player_inventory.slots[y].isFull = temple.isFull;
      player_inventory.slots[y].amount = temple.amount;
      player_inventory.slots[y].item = temple.item;
      player_inventory.slots[y].item_image = temple.item_image;
      player_inventory.slots[y].prefab = temple.prefab;

      //Handle.instance.SetHandlePrefab();
   }
  
   public void DeleteItem(int slot_index)
   {
      Destroy(T_slots[slot_index].transform.GetChild(0).gameObject);

      player_inventory.slots[slot_index].isFull = false;
      player_inventory.slots[slot_index].amount = 0;
      player_inventory.slots[slot_index].item = null;
      player_inventory.slots[slot_index].item_image = null;
      player_inventory.slots[slot_index].prefab = null;

     // Handle.instance.SetHandlePrefab();
      UpdateSlotUI(slot_index);

   }

    public void DecreaseItemAmount(int slot_index)
    {
      player_inventory.slots[slot_index].amount--;
      if (player_inventory.slots[slot_index].amount == 0)
      {
         DeleteItem(slot_index);
      }
      UpdateSlotUI(slot_index);
    }
    public int FindMyIndex(Transform slot)
    {
      int x = T_slots.IndexOf(slot);
      return x;
    }

   /* public void DropItem(int slot_index)
   {
      
      DeleteItem(slot_index);
   }*/









  


}