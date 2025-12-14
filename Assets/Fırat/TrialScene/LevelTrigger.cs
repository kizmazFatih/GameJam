using UnityEngine;

public class LevelTrigger : MonoBehaviour
{
    private bool calisti = false; // Zil sadece 1 kere çalsın

    private void OnTriggerEnter(Collider other)
    {
        // 1. Çarpan şey Oyuncu mu? (Tag kontrolü)
        // 2. Daha önce çalıştı mı?
        if (other.CompareTag("Player") && !calisti)
        {
            calisti = true; // Kilitledik, bir daha çalışmayacak
            
            Debug.Log("Oyuncu Trigger'a girdi! Bölüm 2 başlıyor...");

            // Yöneticiye (StoryEventManager) haber veriyoruz
            if (StoryEventManager.Instance != null)
            {
                StoryEventManager.Instance.StartLevel2Event();
            }

            // İşimiz bitti, trigger kutusunu yok edebiliriz
            Destroy(gameObject);
        }
    }
}