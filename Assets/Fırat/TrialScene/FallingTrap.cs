using UnityEngine;

public class FallingTrap : MonoBehaviour
{
    private Rigidbody rb;
    private bool dustuMu = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Is Trigger olan collider'a değdiğimiz an burası çalışır
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !dustuMu)
        {
            dustuMu = true;
            Debug.Log("Oyuncu bastı, düşüş başlıyor!");
            
            // Hemen düşürme, 0.1 sn bekle ki karakter tam üstüne basmış olsun
            Invoke("DropPlatform", 0.1f); 
        }
    }

    void DropPlatform()
    {
        rb.isKinematic = false; // Yerçekimi açılır
        Destroy(gameObject, 3f); // Temizlik
    }
}