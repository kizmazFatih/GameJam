using UnityEngine;

public class Level3Trigger : MonoBehaviour
{
    // Bu değişken trigger'ın sadece bir kere çalışmasını sağlar
    private bool hasTriggered = false;
    
    [SerializeField] private Transform spawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        // Çarpan obje "Player" etiketine sahipse VE daha önce tetiklenmediyse
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true; // Tekrar çalışmasını engelle

            // StoryManager'daki fonksiyonu çağır
            if (StoryEventManager.Instance != null)
            {
                StoryEventManager.Instance.StartLevel3Event();
                StoryEventManager.Instance.currentSpawnPoint = spawnPoint;
            }

            // İstersen bu Trigger objesini tamamen yok edebilirsin:
            Destroy(gameObject);
        }
    }
}