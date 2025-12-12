using UnityEngine;

public class JumpSkill : MonoBehaviour,IInteractable
{
    public SOItem item;

    public void Interact()
    {
        Destroy(gameObject);
    }

    void Start()
    {

    }

    void Update()
    {

    }
}
