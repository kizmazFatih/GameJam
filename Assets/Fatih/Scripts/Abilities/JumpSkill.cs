using UnityEngine;

public class JumpSkill : MonoBehaviour, IInteractable
{
    public SOItem item;

    public void Interact()
    {
        InventoryController.instance.player_inventory.AddItem(item, item.my_amount);
        Destroy(gameObject);
    }

   
}
