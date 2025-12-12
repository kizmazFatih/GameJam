using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("Gerekli Bileşenler")]
    public AbilityManager abilityManager;

    [Header("Hız Ayarları")]
    public float walkSpeed = 5.0f;
    public float sprintSpeed = 9.0f;
    public float crouchSpeed = 2.5f;

    [Header("Fizik Ayarları")]
    public float jumpHeight = 1.5f;
    public float gravity = -19.62f;

    [Header("Eğilme (Crouch) Ayarları")]
    public float standHeight = 2.0f;
    public float crouchHeight = 1.0f;
    public float crouchTransitionSpeed = 10f;

    // --- Private Değişkenler ---
    private CharacterController controller;
    private PlayerInputs inputActions;
    private Vector2 moveInput;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isSprinting;
    private bool isCrouching;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputActions = new PlayerInputs();

        // --- INPUT TANIMLAMALARI ---

        // Hareket (WASD)
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        // Zıplama (Space)
        inputActions.Player.Jump.performed += ctx => TryJump();

        // Koşma (Shift)
        inputActions.Player.Sprint.performed += ctx => isSprinting = true;
        inputActions.Player.Sprint.canceled += ctx => isSprinting = false;

        // Eğilme (C)
        inputActions.Player.Crouch.performed += ctx => isCrouching = true;
        inputActions.Player.Crouch.canceled += ctx => isCrouching = false;
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void Update()
    {
        float cameraYRotation = Camera.main.transform.eulerAngles.y;

        transform.rotation = Quaternion.Euler(0, cameraYRotation, 0);

        HandleGravity();
        HandleMovement();
        HandleCrouchState();
    }

    private void HandleGravity()
    {
        isGrounded = controller.isGrounded;

        // Yerdeysek ve aşağı doğru hızlanıyorsak hızı sıfırla (hafif negatif tutarak yere yapıştır)
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Yerçekimi uygula (v = a * t)
        velocity.y += gravity * Time.deltaTime;

        // Yerçekimi hareketini uygula
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleMovement()
    {
        // 1. Yetenek Kontrolü: MOVE
        // Eğer Move yeteneği envanterde yoksa input gelse bile hareket 0 olur.
        if (abilityManager != null && !abilityManager.CanUse(MovementType.Move))
        {
            // Hareket yok
            return;
        }

        // Hız belirleme
        float currentSpeed = walkSpeed;

        // 2. Yetenek Kontrolü: SPRINT
        if (isSprinting && !isCrouching) // Eğilirken koşulmaz
        {
            if (abilityManager == null || abilityManager.CanUse(MovementType.Sprint))
            {
                currentSpeed = sprintSpeed;
            }
        }

        // 3. Yetenek Kontrolü: CROUCH (Hız etkisi)
        if (isCrouching)
        {
            if (abilityManager == null || abilityManager.CanUse(MovementType.Crouch))
            {
                currentSpeed = crouchSpeed;
            }
        }

        // Hareket Vektörü (Local Space -> World Space)
        // transform.right ve forward kullandığımız için karakter nereye dönükse oraya gider.
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        // Hareketi uygula
        controller.Move(move * currentSpeed * Time.deltaTime);
    }

    private void TryJump()
    {
        // Yerde değilsek zıplayamayız
        if (!isGrounded) return;

        // 4. Yetenek Kontrolü: JUMP
        if (abilityManager != null && !abilityManager.CanUse(MovementType.Jump))
        {
            // Zıplama yeteneği yok! (Buraya hata sesi ekleyebilirsin)
            return;
        }

        // Fizik Formülü: v = sqrt(h * -2 * g)
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    private void HandleCrouchState()
    {
        // Yetenek var mı?
        bool canCrouch = abilityManager == null || abilityManager.CanUse(MovementType.Crouch);

        // Hedef yükseklik
        float targetHeight = (canCrouch && isCrouching) ? crouchHeight : standHeight;

        // Boyu yumuşakça değiştir
        if (Mathf.Abs(controller.height - targetHeight) > 0.01f)
        {
            controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);

            // Karakterin pivot noktası genelde altta değil ortada olabilir, 
            // yere gömülmemesi veya havada kalmaması için Center noktasını da orantılı kaydırıyoruz.
            Vector3 targetCenter = new Vector3(0, targetHeight / 2f, 0);
            controller.center = Vector3.Lerp(controller.center, targetCenter, Time.deltaTime * crouchTransitionSpeed);
        }
    }
}
