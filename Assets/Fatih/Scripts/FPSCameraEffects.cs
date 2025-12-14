using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CinemachineImpulseSource))] // Bu satır objeye otomatik bileşen ekler
public class FPSCameraEffects : MonoBehaviour
{
    [Header("Referanslar")]
    public CharacterController playerController;
    public CinemachineVirtualCamera virtualCamera;
    public Transform cameraRoot;

    [Header("Efektler")]
    public ParticleSystem speedLines;
    [SerializeField] private CinemachineImpulseSource _impulseSource; // Sarsıntı kaynağı

    [Header("Hız Ayarları")]
    public float defaultSpeed = 5.0f;
    public float sprintSpeed = 9.0f;

    [Header("FOV (Hız Hissi)")]
    public float baseFOV = 60f;
    public float sprintFOV = 75f;
    public float fovChangeSpeed = 5f;

    [Header("Head Bob (Sallantı)")]
    public float bobFrequency = 10f;
    public float bobAmplitude = 0.05f;
    public float sprintBobMultiplier = 2f;

    private float _defaultYPos;
    private float _timer = 0;

    private void Start()
    {

        if (cameraRoot != null) _defaultYPos = cameraRoot.localPosition.y;
        if (virtualCamera != null) virtualCamera.m_Lens.FieldOfView = baseFOV;

        if (speedLines != null && speedLines.isPlaying) speedLines.Stop();
    }

    private void Update()
    {
        if (playerController == null || virtualCamera == null || cameraRoot == null) return;

        Vector3 horizontalVelocity = new Vector3(playerController.velocity.x, 0, playerController.velocity.z);
        float currentSpeed = horizontalVelocity.magnitude;

        HandleFOV(currentSpeed);
        HandleHeadBob(currentSpeed);
        HandleSpeedLines(currentSpeed);
    }

    // --- YENİ EKLENEN FONKSİYON: KAMERA SARSINTISI ---
    public void ShakeCamera(float force = 1f)
    {
        // Kameraya bir "dürtme" (Impulse) gönderir
        if (_impulseSource != null)
        {
            // Vector3.down * force: Kamera hafifçe aşağı ve rastgele titrer (vuruş hissi için)
            // Velocity parametresi sarsıntının yönünü ve şiddetini belirler.
            _impulseSource.GenerateImpulse(new Vector3(Random.Range(-0.5f, 0.5f), -1f, 0) * force);
        }
    }

    private void HandleFOV(float speed)
    {
        float targetFOV = (speed > defaultSpeed + 1f) ? sprintFOV : baseFOV;
        virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(virtualCamera.m_Lens.FieldOfView, targetFOV, fovChangeSpeed * Time.deltaTime);
    }

    private void HandleHeadBob(float speed)
    {
        if (speed > 0.1f && playerController.isGrounded)
        {
            float speedMultiplier = (speed > defaultSpeed + 1f) ? sprintBobMultiplier : 1f;
            _timer += Time.deltaTime * bobFrequency * speedMultiplier;
            float newY = _defaultYPos + Mathf.Sin(_timer) * bobAmplitude * speedMultiplier;
            cameraRoot.localPosition = new Vector3(cameraRoot.localPosition.x, newY, cameraRoot.localPosition.z);
        }
        else
        {
            _timer = 0;
            float newY = Mathf.Lerp(cameraRoot.localPosition.y, _defaultYPos, Time.deltaTime * 10f);
            cameraRoot.localPosition = new Vector3(cameraRoot.localPosition.x, newY, cameraRoot.localPosition.z);
        }
    }

    private void HandleSpeedLines(float speed)
    {
        if (speedLines == null) return;

        if (speed > defaultSpeed + 1f)
        {
            if (!speedLines.isPlaying) speedLines.Play();
        }
        else
        {
            if (speedLines.isPlaying) speedLines.Stop();
        }
    }
}