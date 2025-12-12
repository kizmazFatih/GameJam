using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    private CharacterController _characterController;

    [Header("Movement Settings")]
    [SerializeField] private float speed = 5.0f;
    [SerializeField] private float smoothTime = 0.05f;


    [Header("Other Controls")]
    public bool canRotate = true; // Mouse ile dönüş

    // Değişkenler
    private float _currentVelocity;
    private float _gravity = -9.81f;
    [SerializeField] private float gravityMultiplier = 3.0f;
    private float _verticalVelocity;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        ApplyRotation();
        ApplyGravity();
        ApplyMovement();
    }

    private void ApplyGravity()
    {
        if (_characterController.isGrounded && _verticalVelocity < 0.0f)
        {
            _verticalVelocity = -2.0f;
        }
        else
        {
            _verticalVelocity += _gravity * gravityMultiplier * Time.deltaTime;
        }
    }

    private void ApplyRotation()
    {
        if (!canRotate) return;

        if (Camera.main != null)
        {
            var targetAngle = Camera.main.transform.eulerAngles.y;
            var angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _currentVelocity, smoothTime);
            transform.rotation = Quaternion.Euler(0.0f, angle, 0.0f);
        }
    }

    private void ApplyMovement()
    {
        Vector2 input = InputManager.instance.playerInputs.Player.Move.ReadValue<Vector2>();

        // 2. İSTEĞE GÖRE FİLTRELEME (Burayı değiştirdik)
        
        // Eğer W-S (İleri/Geri) hareketi kapalıysa, inputun Y değerini sıfırla
        if (InventoryController.instance.CheckSkill(PlayerSkill.Vertical) == false) 
        {
            input.y = 0f; 
        }

        // Eğer A-D (Sağ/Sol) hareketi kapalıysa, inputun X değerini sıfırla
        if (InventoryController.instance.CheckSkill(PlayerSkill.Horizontal) == false) 
        {
            input.x = 0f; 
        }

        // 3. Filtrelenmiş input ile hareket vektörünü oluştur
        Vector3 move = (transform.right * input.x) + (transform.forward * input.y);
        
        // 4. Hareketi uygula (Gravity bağımsız)
        Vector3 finalMovement = (move * speed) + (Vector3.up * _verticalVelocity);

        _characterController.Move(finalMovement * Time.deltaTime);
    }
}