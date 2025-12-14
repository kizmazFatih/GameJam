
using UnityEngine;

public class Door : MonoBehaviour
{
    private Animator animator;


    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    public void Open()
    {
        
        animator.SetBool("isOpen", true);

    }

}
