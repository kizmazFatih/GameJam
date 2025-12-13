using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    public PlayerInputs playerInputs;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        playerInputs = new PlayerInputs();
        
        
    }

    private void OnEnable()
    {
        // Inputları aktif et
        if (playerInputs != null)
            playerInputs.Enable();
    }

    private void OnDisable()
    {
        // Obje pasif olduğunda inputları durdur
        if (playerInputs != null)
            playerInputs.Disable();
    }





}
