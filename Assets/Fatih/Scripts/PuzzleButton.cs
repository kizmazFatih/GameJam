using UnityEngine;
using UnityEngine.Events;

public class PuzzleButton : MonoBehaviour,IInteractable
{
    [Header("Buton Ayarları")]
    public int buttonID; // Bu butonun numarası kaç? (0, 1, 2, 3...)
    public ButtonPuzzleManager puzzleManager; // Yöneticiye haber vereceğiz

    [Header("Görsel Efekt")]
    public float pressDepth = 0.1f; // Basılma derinliği
    private Vector3 initialPos;

    private void Start()
    {
        initialPos = transform.localPosition;
    }

    // Bu fonksiyonu Raycast ile veya tıklayarak çağıracağız
    public void Interact()
    {
        // 1. Görsel olarak butonu içeri göm (Basit animasyon)
        transform.localPosition = initialPos - (transform.up * pressDepth);
        Invoke("ResetPosition", 0.2f); // 0.2 saniye sonra geri çıksın

        // 2. Yöneticiye haber ver
        if (puzzleManager != null)
        {
            puzzleManager.ButonaBasildi(buttonID);
        }
    }

    void ResetPosition()
    {
        transform.localPosition = initialPos;
    }

   
}