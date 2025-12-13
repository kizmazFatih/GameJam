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

    private float _currentVelocity;
    private float _gravity = -9.81f;
    private float _verticalVelocity;

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
        
        if (!_isDashing) 
        {
            HandleStanceAndSpeed();
            HandleDash(); // Dash inputunu kontrol et
        }
        
        ApplyGravity();
        ApplyMovement();
    }

    private void HandleDash()
    {
        
        bool dashInput = false;
        try {
             dashInput = InputManager.instance.playerInputs.Player.Dash.WasPressedThisFrame();
        } catch { 
             dashInput = Keyboard.current.leftShiftKey.wasPressedThisFrame; 
        }

        if (dashInput && Time.time >= _lastDashTime + dashCooldown && InventoryController.instance.CheckSkill(PlayerSkill.Dash)) // PlayerSkill.Dash enum'ına eklemelisin!
        {
            StartCoroutine(PerformDash());
        }
    }

    private IEnumerator PerformDash()
    {
        _isDashing = true;
        _lastDashTime = Time.time;
        
        yield return new WaitForSeconds(dashDuration);

        _isDashing = false;
        
    }
  

    private void HandleStanceAndSpeed()
    {
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
        bool jumpInput = InputManager.instance.playerInputs.Player.Jump.WasPressedThisFrame();

        if (_characterController.isGrounded && InventoryController.instance.CheckSkill(PlayerSkill.Jump))
        {
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2.0f;
            }

            if (jumpInput)
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
        Vector2 input = InputManager.instance.playerInputs.Player.Move.ReadValue<Vector2>();

        if (InventoryController.instance.CheckSkill(PlayerSkill.Vertical) == false) input.y = 0f;
        if (InventoryController.instance.CheckSkill(PlayerSkill.Horizontal) == false) input.x = 0f;

        Vector3 move = (transform.right * input.x) + (transform.forward * input.y);

        float currentSpeed;
        
        if (_isDashing)
        {
            if (move.magnitude < 0.1f) move = transform.forward;
            
            currentSpeed = dashSpeed;
        }
        else
        {
            currentSpeed = _targetSpeed;
        }

        Vector3 finalMovement = (move.normalized * currentSpeed) + (Vector3.up * _verticalVelocity);

        _characterController.Move(finalMovement * Time.deltaTime);
    }
}