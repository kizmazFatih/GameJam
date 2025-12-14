using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [SerializeField] private FPSCameraEffects cameraEffects;
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
    [SerializeField] private float slideSpeed = 8.0f;
    [SerializeField] private float slopeRayLength = 1.5f;
    [SerializeField] private float slopeForce = 5.0f;

    // --- YENİ EKLENEN KISIM: YUMRUK AYARLARI ---
    [Header("Combat Settings")]
    [SerializeField] private float punchDamage = 20f;      // Vuruş hasarı
    [SerializeField] private float punchRange = 2.5f;      // Vuruş menzili
    [SerializeField] private float punchCooldown = 0.6f;   // İki vuruş arası bekleme süresi
    [SerializeField] private float punchStaminaCost = 15f; // Vuruşun harcadığı enerji
    [SerializeField] private LayerMask hitLayers;          // Neye vurabiliriz? (Enemy, Default vb.)
    [SerializeField] private Transform cameraTransform;    // Raycast için kamera referansı
    [SerializeField] private Animator weaponAnimator;      // Eğer kolların animasyonu varsa buraya ata

    private float _lastPunchTime;
    // ---------------------------------------------

    private float _currentVelocity;
    private float _gravity = -9.81f;
    private float _verticalVelocity;

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
        _lastPunchTime = -punchCooldown; // Oyun başlar başlamaz vurabilelim diye

        _currentStamina = maxStamina;
        if (staminaSlider != null)
        {
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = _currentStamina;
        }

        // Eğer kamera atanmadıysa otomatik bulmaya çalış
        if (cameraTransform == null) cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        ApplyRotation();
        CheckSlopeLogic();

        if (!_isDashing)
        {
            HandleStanceAndSpeed();
            HandleDash();

            // --- YENİ FONKSİYON ÇAĞRISI ---
            HandleAttack();
        }

        ApplyGravity();
        ApplyMovement();
    }

    // --- YENİ EKLENEN FONKSİYON: SALDIRI MANTIĞI ---
    private void HandleAttack()
    {
        bool attackInput = false;

        if (InputManager.instance != null)
        {
            // Input System'de "Punch" adında bir Action oluşturduğunu varsayıyorum.
            attackInput = InputManager.instance.playerInputs.Player.Punch.WasPressedThisFrame();
        }

        // --- Geri kalan mantık aynı ---
        if (attackInput && 
            Time.time >= _lastPunchTime + punchCooldown&&
            InventoryController.instance.CheckSkill(PlayerSkill.Punch) && 
            !_isExhausted && 
            _currentStamina >= punchStaminaCost)
        {
            PerformPunch();
        }
    }

    private void PerformPunch()
    {
        _lastPunchTime = Time.time;

        // Staminayı düşür
        _currentStamina -= punchStaminaCost;
        if (staminaSlider != null) staminaSlider.value = _currentStamina;

        // Animasyonu Oynat (Eğer Animator varsa)
        if (weaponAnimator != null)
        {
            weaponAnimator.SetTrigger("Punch"); // Animator'da "Punch" adında bir Trigger parametresi olmalı
        }
        if (cameraEffects != null)
    {
        // 0.5f hafif sarsıntı, 2f güçlü sarsıntı. 
        // Vuruşun gücüne göre bu sayıyı değiştirebilirsin.
        cameraEffects.ShakeCamera(0.8f); 
    }

        // Raycast (Vuruş Tespiti)
        // Kameranın merkezinden ileriye doğru görünmez bir ışın atıyoruz
        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, punchRange, hitLayers))
        {
            // Konsola neye vurduğumuzu yazdır
            Debug.Log("Vurulan Obje: " + hit.collider.name);


            // Eğer vurulan objenin bir canı varsa hasar ver
            // Örnek: Düşman scriptinde "TakeDamage" fonksiyonu varsa:
            // var enemy = hit.collider.GetComponent<EnemyHealth>();
            // if(enemy != null) enemy.TakeDamage(punchDamage);

            // Vurulan objeye fiziksel güç uygula (İsteğe bağlı - kutuları itmek için)
            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForce(-hit.normal * 5f, ForceMode.Impulse);
            }
        }
    }
    // ---------------------------------------------

    private void CheckSlopeLogic()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, slopeRayLength))
        {
            _hitNormal = hitInfo.normal;
            float slopeAngle = Vector3.Angle(Vector3.up, _hitNormal);

            if (slopeAngle > _characterController.slopeLimit && slopeAngle < 80f)
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
        bool dashInput = false;

        if (InputManager.instance != null)
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
        if (InputManager.instance != null)
            jumpInput = InputManager.instance.playerInputs.Player.Jump.WasPressedThisFrame();

        if (_characterController.isGrounded)
        {
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2.0f;
            }

            if (jumpInput && InventoryController.instance.CheckSkill(PlayerSkill.Jump))
            {
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * _gravity * gravityMultiplier);
            }
        }

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
        float currentTargetSpeed = _targetSpeed;

        Vector3 finalMoveVelocity = inputDirection * currentTargetSpeed;

        if (_isSliding)
        {
            Vector3 slideDirection = new Vector3(_hitNormal.x, -_hitNormal.y, _hitNormal.z);
            Vector3.OrthoNormalize(ref _hitNormal, ref slideDirection);
            finalMoveVelocity += slideDirection * slideSpeed;
        }

        if (_isDashing)
        {
            Vector3 dashMove = finalMoveVelocity.normalized;
            if (dashMove.magnitude < 0.1f) dashMove = transform.forward;
            finalMoveVelocity = dashMove * dashSpeed;
        }

        finalMoveVelocity.y = _verticalVelocity;

        _characterController.Move(finalMoveVelocity * Time.deltaTime);
    }

    // Editörde vuruş menzilini görmek için yardımcı çizgi
    private void OnDrawGizmosSelected()
    {
        if (cameraTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(cameraTransform.position, cameraTransform.forward * punchRange);
        }
    }
}