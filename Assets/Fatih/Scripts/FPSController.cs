using UnityEngine;
// Input System namespace'ini eklemeyi unutma
using UnityEngine.InputSystem; 

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    private CharacterController _characterController;

    [Header("Movement Speeds")]
    [SerializeField] private float walkSpeed = 5.0f;
    [SerializeField] private float sprintSpeed = 9.0f;
    [SerializeField] private float crouchSpeed = 2.5f;
    
    // Anlık hızı takip etmek için private değişken
    private float _targetSpeed; 

    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float gravityMultiplier = 3.0f;

    [Header("Crouch Settings")]
    [SerializeField] private float crouchHeight = 1.0f;
    [SerializeField] private float standingHeight = 2.0f;
    [SerializeField] private float crouchCenterY = 0.5f;
    [SerializeField] private float standingCenterY = 0.0f;
    private bool _isCrouching = false;

    [Header("Input Axis Controls (Filters)")]
    public bool canMoveForwardBackward = true;
    public bool canMoveStrafe = true;
    public bool canRotate = true;

    [Header("Smooth Settings")]
    [SerializeField] private float smoothTime = 0.05f;

    // Fizik Değişkenleri
    private float _currentVelocity;
    private float _gravity = -9.81f;
    private float _verticalVelocity;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        
        // Başlangıç boyunu ayarla
        _characterController.height = standingHeight;
        _characterController.center = new Vector3(0, standingCenterY, 0);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        ApplyRotation();
        HandleStanceAndSpeed(); // Eğilme ve Hız belirleme
        ApplyGravity();
        ApplyMovement();
    }

    private void HandleStanceAndSpeed()
    {
        // 1. INPUTLARI OKU
        // Not: Input System Asset'inde "Sprint" ve "Crouch" action'larını tanımlamış olmalısın.
        // Genelde: Sprint -> Left Shift, Crouch -> C veya Left Ctrl
        
        // Input Manager'dan gelen veriler (Senin yapına uygun şekilde)
        bool isSprinting = InputManager.instance.playerInputs.Player.Sprint.IsPressed();
        bool isCrouchingInput = InputManager.instance.playerInputs.Player.Crouch.IsPressed();

        // 2. EĞİLME MANTIĞI (CROUCH)
        if (isCrouchingInput && !_isCrouching)
        {
            // Eğilmeye başla
            _isCrouching = true;
            _characterController.height = crouchHeight;
            _characterController.center = new Vector3(0, crouchCenterY, 0);
        }
        else if (!isCrouchingInput && _isCrouching)
        {
            // Ayağa kalk (İstersen buraya tavana çarpıyor mu kontrolü eklenebilir)
            _isCrouching = false;
            _characterController.height = standingHeight;
            _characterController.center = new Vector3(0, standingCenterY, 0);
        }

        // 3. HIZ BELİRLEME (Priority: Crouch > Sprint > Walk)
        if (_isCrouching)
        {
            _targetSpeed = crouchSpeed;
        }
        else if (isSprinting)
        {
            _targetSpeed = sprintSpeed;
        }
        else
        {
            _targetSpeed = walkSpeed;
        }
    }

    private void ApplyGravity()
    {
        // Zıplama Input'u (Space tuşu)
        bool jumpInput = InputManager.instance.playerInputs.Player.Jump.WasPressedThisFrame();

        if (_characterController.isGrounded)
        {
            // Yere bastığında vertical velocity'yi sıfırla (hafif eksi bırakıyoruz ki yere tam bassın)
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2.0f;
            }

            // ZIPLAMA MANTIĞI
            if (jumpInput)
            {
                // Fizik Formülü: v = karekök(h * -2 * g)
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * _gravity * gravityMultiplier);
            }
        }

        // Yerçekimi uygula
        _verticalVelocity += _gravity * gravityMultiplier * Time.deltaTime;
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

        // Filtreleme
        if (!canMoveForwardBackward) input.y = 0f;
        if (!canMoveStrafe) input.x = 0f;

        Vector3 move = (transform.right * input.x) + (transform.forward * input.y);
        
        // Hızı _targetSpeed ile çarpıyoruz
        Vector3 finalMovement = (move * _targetSpeed) + (Vector3.up * _verticalVelocity);

        _characterController.Move(finalMovement * Time.deltaTime);
    }
}