using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DroneBullet : MonoBehaviour
{
    public float speed = 20f;
    public float lifeTime = 3f; // Iskalarsa 3 saniye sonra yok olsun

    private void Start()
    {
        // Mermi doğduğu an ileri doğru fırlasın
        GetComponent<Rigidbody>().velocity = transform.forward * speed;
        
        // Yerçekiminden etkilenmesin ki düz gitsin
        GetComponent<Rigidbody>().useGravity = false;

        // Sonsuza kadar gitmesin, süre dolunca yok olsun
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. OYUNCUYA ÇARPARSA
        if (other.CompareTag("Player"))
        {
            Debug.Log("VURULDUN! Drone seni tek mermide indirdi.");
            
            // Burada oyuncuyu yok edebilir veya oyun bitti ekranını açabilirsin
            // Şimdilik oyuncuyu yok ediyoruz:
            Destroy(other.gameObject); 
            
            // Mermiyi de yok et
            Destroy(gameObject);
        }
        // 2. CAMA ÇARPARSA
        else if (other.CompareTag("Cam")) // Cam tag'i önemli!
        {
            Debug.Log("Cam kırıldı!");
            Destroy(other.gameObject); // Camı yok et
            Destroy(gameObject);       // Mermiyi de yok et (Cam mermiyi durdurur)
            
            // NOT: Eğer merminin camı delip geçmesini ve arkadaki adamı da vurmasını
            // istiyorsan, sadece 'Destroy(gameObject)' satırını silmen yeterli.
        }
        // 3. DUVARA VEYA YERE ÇARPARSA
        else if (!other.CompareTag("Enemy") && !other.CompareTag("Bullet"))
        {
            // Başka bir şeye çarparsa sadece mermi yok olsun
            Destroy(gameObject);
        }
    }
}