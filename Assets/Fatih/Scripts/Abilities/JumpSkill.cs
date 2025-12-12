using UnityEngine;

public class JumpSkill : MonoBehaviour, IInteractable
{
    public SOItem item;

    public void Interact()
    {
        InventoryController.instance.player_inventory.AddItem(item, item.my_amount);

        SkillTooltipManager.Instance.HideTooltip();

        Destroy(gameObject);
    }
    private void OnMouseEnter()
    {
        if (item != null && SkillTooltipManager.Instance != null)
        {
        
            SkillTooltipManager.Instance.ShowTooltip(item.skillName, item.pickupMessage, transform);
        }
    }

    private void OnMouseExit()
    {
        // Baloncuğu kapat
        SkillTooltipManager.Instance.HideTooltip();
    }
}

