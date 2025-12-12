using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
   private Transform parent_after_drag;
   private RawImage image;
   
   private void Start() {
       image = GetComponent<RawImage>();
       transform.GetChild(0).GetComponent<TextMeshProUGUI>().raycastTarget = false;
   }

   public void OnBeginDrag(PointerEventData eventData)
   {
       parent_after_drag = transform.parent;
       transform.SetParent(transform.root);
        
       transform.SetAsLastSibling();
       image.raycastTarget = false;
       
   }

   public void OnDrag(PointerEventData eventData)
   {
        transform.position = eventData.position;
   }

   public void OnEndDrag(PointerEventData eventData)
   {
        
        var target = eventData.pointerCurrentRaycast.gameObject;
        if(target != null)
        {
          if(target.tag == "Trash")
          {
            int x =InventoryController.instance.FindMyIndex(parent_after_drag);
            transform.SetParent(parent_after_drag);
            transform.SetAsLastSibling();
            InventoryController.instance.DeleteItem(x);
          }
          else
          {
            Transform slotTransform = GetSlotParent(target.transform);
            if (slotTransform != null)   {ChangeSlot(slotTransform);}
          }
        }
        else
        {
        transform.SetParent(parent_after_drag);
        transform.SetAsLastSibling();
        }
        image.raycastTarget = true;
        
   }
   Transform GetSlotParent(Transform t)
   {
    while (t != null && t.tag != "Slot")
        t = t.parent;
    return t;
  }

   public void ChangeSlot(Transform other_slot)
   {
      
     if(other_slot.childCount != 0)
     {
       other_slot.GetChild(0).SetParent(parent_after_drag);
     }
             
     transform.SetParent(other_slot);
     transform.SetAsLastSibling();


     InventoryController.instance.ChangeSlotsEach(transform.parent, parent_after_drag);
   }


}
