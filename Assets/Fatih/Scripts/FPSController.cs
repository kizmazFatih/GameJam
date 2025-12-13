using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    private bool _isExhausted = false;

    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrainRate = 15f;
    [SerializeField] private float staminaRegenRate = 10f;
    [SerializeField] private Slider staminaSlider;

    private float _currentStamina;

    private CharacterController _characterController;

    [Header("Movement Speeds")]
    [SerializeField] private float walkSpeed = 5.0f;
    [SerializeField] private float sprintSpeed = 9.0f;
    [SerializeField] private float crouchSpeed = 2.5f;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 20.0f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1.0f;
    [SerializeField] private float dashStaminaCost = 25f;
    private bool _isDashing = false;
    private float _lastDashTime;

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

    [Header("Smooth Settings")]
    [SerializeField] private float smoothTime = 0.05f;

    [Header("Slope & Slide Settings")]
    [SerializeField] private float slideSpeed = 8.0f; // Kayma hızı
    [SerializeField] private float slopeRayLength = 1.5f; // Eğim tespiti için ışın uzunluğu
    [SerializeField] private float slopeForce = 5.0f; // Yere yapıştırma kuvveti

    private float _currentVelocity;
    private float _gravity = -9.81f;
    private float _verticalVelocity;
    
    // Eğim tespiti için değişkenler
    private Vector3 _hitNormal;
    private bool _isSliding = false;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();

        _characterController.height = standingHeight;
        _characterController.center = new Vector3(0, standingCenterY, 0);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _lastDashTime = -dashCooldown;

        _currentStamina = maxStamina;
        if (staminaSlider != null)
        {
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = _currentStamina;
        }
    }

    private void Update()
    {
        ApplyRotation();

        // Eğimde kayıyor muyuz kontrol et
        CheckSlopeLogic();

        if (!_isDashing)
        {
            HandleStanceAndSpeed();
            HandleDash();
        }

        ApplyGravity();
        ApplyMovement();
    }

    private void CheckSlopeLogic()
    {
        // Karakterin altındaki zemine bir ışın (Raycast) gönderiyoruz
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, slopeRayLength))
        {
            _hitNormal = hitInfo.normal;
            
            // Zemin açısını hesapla
            float slopeAngle = Vector3.Angle(Vector3.up, _hitNormal);

            // Eğer açı, CharacterController'ın limitinden büyükse kaymaya başla
            if (slopeAngle > _characterController.slopeLimit && slopeAngle < 80f) // 80f çok dik duvarlar için koruma
            {
                _isSliding = true;
            }
            else
            {
                _isSliding = false;
            }
        }
        else
        {
            _isSliding = false;
            _hitNormal = Vector3.up;
        }
    }

    private void HandleDash()
    {
        // Kayarken dash atmayı engelleyebiliriz (isteğe bağlı), şimdilik açık bırakıyorum.
        bool dashInput = false;
        
        // Input Manager null kontrolü eklemek iyi bir alışkanlıktır
        if(InputManager.instance != null)
             dashInput = InputManager.instance.playerInputs.Player.Dash.WasPressedThisFrame();

        if (dashInput &&
            Time.time >= _lastDashTime + dashCooldown &&
            InventoryController.instance.CheckSkill(PlayerSkill.Dash) &&
            !_isExhausted &&
            _currentStamina >= dashStaminaCost)
        {
            StartCoroutine(PerformDash());
        }
    }

    private IEnumerator PerformDash()
    {
        _currentStamina -= dashStaminaCost;

        if (_currentStamina <= 0)
        {
            _currentStamina = 0;
            _isExhausted = true;
        }

        if (staminaSlider != null) staminaSlider.value = _currentStamina;

        _isDashing = true;
        _lastDashTime = Time.time;

        yield return new WaitForSeconds(dashDuration);

        _isDashing = false;
    }

    private void HandleStanceAndSpeed()
    {
        // InputManager kontrolleri
        if (InputManager.instance == null) return;

        bool isSprinting = InputManager.instance.playerInputs.Player.Sprint.IsPressed();
        bool isCrouchingInput = InputManager.instance.playerInputs.Player.Crouch.IsPressed();

        Vector2 inputVector = InputManager.instance.playerInputs.Player.Move.ReadValue<Vector2>();
        bool isMoving = inputVector.magnitude > 0.1f;

        if (isCrouchingInput && !_isCrouching && InventoryController.instance.CheckSkill(PlayerSkill.Crouch))
        {
            _isCrouching = true;
            _characterController.height = crouchHeight;
            _characterController.center = new Vector3(0, crouchCenterY, 0);
        }
        else if (!isCrouchingInput && _isCrouching)
        {
            _isCrouching = false;
            _characterController.height = standingHeight;
            _characterController.center = new Vector3(0, standingCenterY, 0);
        }

        if (_currentStamina <= 0)
        {
            _isExhausted = true;
            _currentStamina = 0;
        }
        else if (_currentStamina >= 20f)
        {
            _isExhausted = false;
        }

        if (_isCrouching)
        {
            _targetSpeed = crouchSpeed;
            RegenerateStamina();
        }
        else if (isSprinting && isMoving && !_isExhausted && _currentStamina > 0 && InventoryController.instance.CheckSkill(PlayerSkill.Sprint))
        {
            _targetSpeed = sprintSpeed;
            _currentStamina -= staminaDrainRate * Time.deltaTime;
        }
        else
        {
            _targetSpeed = walkSpeed;
            RegenerateStamina();
        }

        _currentStamina = Mathf.Clamp(_currentStamina, 0, maxStamina);

        if (staminaSlider != null)
        {
            staminaSlider.value = _currentStamina;
        }
    }

    private void RegenerateStamina()
    {
        if (_currentStamina < maxStamina)
        {
            _currentStamina += staminaRegenRate * Time.deltaTime;
        }
    }

    private void ApplyGravity()
    {
        bool jumpInput = false;
        if(InputManager.instance != null) 
            jumpInput = InputManager.instance.playerInputs.Player.Jump.WasPressedThisFrame();

        // Yerçekimi ve Zıplama Mantığı
        if (_characterController.isGrounded)
        {
            // Yere tam basarken hafif bir aşağı kuvvet uyguluyoruz ki isGrounded titremesin
            // Ancak zıplarken bu kuvveti anında yeneceğiz
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2.0f; 
            }

            // Kayma durumunda zıplamayı engellemek istersen && !_isSliding ekleyebilirsin.
            // Ama şimdilik oyuncunun zıplayabilmesini istiyorsun.
            if (jumpInput && InventoryController.instance.CheckSkill(PlayerSkill.Jump))
            {
                // Zıplama formülü: v = sqrt(h * -2 * g)
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * _gravity * gravityMultiplier);
            }
        }

        // Yerçekimini uygula
        _verticalVelocity += _gravity * gravityMultiplier * Time.deltaTime;
    }

    private void ApplyRotation()
    {
        if (Camera.main != null)
        {
            var targetAngle = Camera.main.transform.eulerAngles.y;
            var angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _currentVelocity, smoothTime);
            transform.rotation = Quaternion.Euler(0.0f, angle, 0.0f);
        }
    }

    private void ApplyMovement()
    {
        Vector2 input = Vector2.zero;
        if (InputManager.instance != null)
            input = InputManager.instance.playerInputs.Player.Move.ReadValue<Vector2>();

        if (InventoryController.instance.CheckSkill(PlayerSkill.Vertical) == false) input.y = 0f;
        if (InventoryController.instance.CheckSkill(PlayerSkill.Horizontal) == false) input.x = 0f;

        Vector3 inputDirection = (transform.right * input.x + transform.forward * input.y).normalized;
        float currentTargetSpeed = _targetSpeed; // Yürüyüş, Koşu veya Eğilme hızı
        
        Vector3 finalMoveVelocity = inputDirection * currentTargetSpeed;

        // 2. Kayma Fiziği (Slide)
        if (_isSliding)
        {
            // Kayma yönünü hesapla
            Vector3 slideDirection = new Vector3(_hitNormal.x, -_hitNormal.y, _hitNormal.z);
            Vector3.OrthoNormalize(ref _hitNormal, ref slideDirection);

            // ÖNEMLİ: Oyuncunun hareketine zıt yönde veya aşağı doğru bir "çekim" kuvveti ekliyoruz.
            // Bu sayede Sprint (9.0f) hızı, Kayma (örn: 6.0f) hızından büyükse oyuncu yukarı tırmanabilir.
            finalMoveVelocity += slideDirection * slideSpeed;
        }

        // 3. Dash (Atılma) Kontrolü
        if (_isDashing)
        {
            // Dash atarken kayma fiziğini yok sayıyoruz ki oyuncu kaçabilsin
            Vector3 dashMove = finalMoveVelocity.normalized;
            if (dashMove.magnitude < 0.1f) dashMove = transform.forward;
            finalMoveVelocity = dashMove * dashSpeed;
        }

        // 4. Yerçekimi ve Dikey Hızın Eklenmesi
        finalMoveVelocity.y = _verticalVelocity;

        // 5. Son Hareketi Uygula
        _characterController.Move(finalMoveVelocity * Time.deltaTime);
    }
}