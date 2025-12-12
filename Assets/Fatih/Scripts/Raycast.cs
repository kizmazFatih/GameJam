using System;
using UnityEngine;

public class Raycast : MonoBehaviour
{
    [SerializeField] private GameObject interactButtonUI;

    void Update()
    {
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit))
        {
            if (hit.transform.TryGetComponent(out IInteractable interactable))
            {

                ShowInteractUI();

                if (InputManager.instance.playerInputs.Player.Interaction.WasPressedThisFrame())
                {
                    interactable.Interact();
                }
            }
            else
            {
                HideInteractUI();
            }
        }
        else
        {
            HideInteractUI();
        }
    }

    private void ShowInteractUI()
    {
        interactButtonUI.SetActive(true);
    }

    private void HideInteractUI()
    {
        interactButtonUI.SetActive(false);
    }
}
