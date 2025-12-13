using UnityEngine;

public class Platform : MonoBehaviour, IInteractable
{
    private Transform topTransform;
    [SerializeField] private GameObject myDoor;

    private void Start()
    {
        topTransform = transform.GetChild(0);
    }

    public void Interact()
    {
        if (topTransform.childCount > 0) return;
        if (InventoryController.instance.player_inventory.slots[Handle.instance.index] == null) return;

        Instantiate(InventoryController.instance.player_inventory.slots[Handle.instance.index].prefab, topTransform.transform.position, Quaternion.identity);
        InventoryController.instance.DeleteItem(Handle.instance.index);

    }


}
