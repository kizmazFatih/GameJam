using UnityEngine;

public class SafePlatform : MonoBehaviour
{
    [Header("Atanacak Materyal")]
    public Material aktifMateryal; 
    
    private MeshRenderer meshRenderer;
    private bool basildiMi = false;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    // YÖNTEM 1: Fiziksel Çarpışma (Bazen çalışmaz)
    private void OnCollisionEnter(Collision collision)
    {
        KontrolEt(collision.gameObject);
    }

    // YÖNTEM 2: Trigger Girişi (Garanti çalışır)
    private void OnTriggerEnter(Collider other)
    {
        KontrolEt(other.gameObject);
    }

    // Ortak Kontrol Fonksiyonu
    void KontrolEt(GameObject oyuncu)
    {
        // Sadece Player ise ve daha önce basılmadıysa
        if (oyuncu.CompareTag("Player") && !basildiMi)
        {
            Debug.Log("TEMAS ALGILANDI! Renk değiştiriliyor..."); // Bunu Console'da görmelisin
            basildiMi = true;

            if (meshRenderer != null && aktifMateryal != null)
            {
                meshRenderer.material = aktifMateryal;
            }
        }
    }
}