using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.XR;

public class InventoryController : MonoBehaviour
{
    public static InventoryController instance;
    public SOInventory player_inventory;

    [SerializeField] private Transform bottom_slots_parent;
    public List<Transform> T_slots = new List<Transform>();
    public GameObject UI_prefab;

    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);

        T_slots.Clear();
        for (int i = 0; i < bottom_slots_parent.childCount; i++)
        {
            T_slots.Add(bottom_slots_parent.GetChild(i));
        }
    }

    private void Start()
    {
        for (int i = 0; i < player_inventory.slots.Count; i++)
        {
            if (player_inventory.slots[i].isFull && player_inventory.slots[i].item == null)
            {
                DeleteItem(i);
            }

            else if (player_inventory.slots[i].isFull && player_inventory.slots[i].item != null)
            {
                UpdateSlotUI(i);
            }
        }
    }

    public void UpdateSlotUI(int slot_index)
    {
        if (player_inventory.slots[slot_index].isFull && T_slots[slot_index].childCount == 0)
        {
            Instantiate(UI_prefab, T_slots[slot_index]);
        }

        if (T_slots[slot_index].childCount > 0)
        {
            if (player_inventory.slots[slot_index].item != null)
            {
                T_slots[slot_index].GetChild(0).GetComponent<RawImage>().texture = player_inventory.slots[slot_index].item_image;
            }
        }
    }

    public void ChangeSlotsEach(Transform slot1, Transform slot2)
    {
        int x = T_slots.IndexOf(slot1);
        int y = T_slots.IndexOf(slot2);

        Slot temple = new Slot
        {
            isFull = player_inventory.slots[x].isFull,
            amount = player_inventory.slots[x].amount,
            item = player_inventory.slots[x].item,
            item_image = player_inventory.slots[x].item_image,
            prefab = player_inventory.slots[x].prefab
        };

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

        Handle.instance.SetHandlePrefab();
    }

    public void DeleteItem(int slot_index)
    {
        if (slot_index < 0 || slot_index >= T_slots.Count) return;

        if (T_slots[slot_index].childCount > 0)
        {
            Destroy(T_slots[slot_index].GetChild(0).gameObject);
        }

        if (slot_index < player_inventory.slots.Count)
        {
            player_inventory.slots[slot_index].isFull = false;
            player_inventory.slots[slot_index].amount = 0;
            player_inventory.slots[slot_index].item = null; // İşte burası en önemlisi
            player_inventory.slots[slot_index].item_image = null;
            player_inventory.slots[slot_index].prefab = null;
        }
        Handle.instance.SetHandlePrefab();
    }

    public void DropItem(int slot_index)
    {
        if (slot_index < 0 || slot_index >= T_slots.Count) return;
        if (player_inventory.slots[slot_index].prefab == null) return;

        var spawnObject = player_inventory.slots[slot_index].prefab;
        DeleteItem(slot_index);
        var spawnedObject = Instantiate(spawnObject, transform.position + (Vector3.up * 2), Quaternion.identity);
        spawnedObject.GetComponent<Rigidbody>().AddForce(transform.forward * 5f, ForceMode.Impulse);
        Handle.instance.SetHandlePrefab();

    }

    public void DecreaseItemAmount(int slot_index)
    {
        player_inventory.slots[slot_index].amount--;
        if (player_inventory.slots[slot_index].amount <= 0)
        {
            DeleteItem(slot_index);
        }
        else
        {
            UpdateSlotUI(slot_index);
        }
    }

    public int FindMyIndex(Transform slot)
    {
        return T_slots.IndexOf(slot);
    }

    public void RemoveRandomSkill()
    {
        List<int> skillSlotIndices = new List<int>();

        for (int i = 0; i < player_inventory.slots.Count; i++)
        {

            if (player_inventory.slots[i].isFull && player_inventory.slots[i].item != null)
            {
                skillSlotIndices.Add(i);
            }
        }

        if (skillSlotIndices.Count == 0)
        {
            Debug.Log("Çalınacak skill bulunamadı.");
            return;
        }

        int randomIndex = Random.Range(0, skillSlotIndices.Count);
        int slotToDelete = skillSlotIndices[randomIndex];

        string stolenSkillName = player_inventory.slots[slotToDelete].item?.skillName ?? "Bilinmeyen Yetenek";

        StartCoroutine(StanSkill(slotToDelete));


        Debug.Log($"DİKKAT! Örümcek '{stolenSkillName}' yeteneğini 10 saniyeliğine etkisiz bıraktı!");
    }

    IEnumerator StanSkill(int slotIndex)
    {
        var stannedItem = player_inventory.slots[slotIndex].item;
        DeleteItem(slotIndex);

        yield return new WaitForSeconds(10f);
        player_inventory.AddItem(stannedItem, player_inventory.slots[slotIndex].amount);

    }


    public bool CheckSkill(PlayerSkill playerSkill)
    {
        for (int i = 0; i < player_inventory.slots.Count; i++)
        {
            if (!player_inventory.slots[i].isFull || player_inventory.slots[i].item == null)
                continue;

            if (player_inventory.slots[i].item.player_skill == playerSkill)
            {
                return true;
            }
        }
        return false;
    }
}