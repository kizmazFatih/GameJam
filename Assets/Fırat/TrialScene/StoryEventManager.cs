using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StoryEventManager : MonoBehaviour
{
    public static StoryEventManager Instance;

    [Header("GENEL UI")]
    public TextMeshProUGUI dialogueText;

    // ========================================================================
    // BÖLÜM 1: BAŞLANGIÇ (INTRO)
    // ========================================================================
    [Header("BÖLÜM 1: İntro Ayarları")]
    public Animator introPlatformAnimator; 
    public Transform introSkillPoint;      
    public SOItem introRewardItem;         
    public float introAnimasyonSuresi = 1.5f; 
    public float introYukseklikOffset = 1.5f;

    // ========================================================================
    // BÖLÜM 2: PUZZLE SONRASI
    // ========================================================================
    [Header("BÖLÜM 2: Puzzle Sonu Ayarları")]
    public Animator level2PlatformAnimator; 
    public Transform level2SkillPoint;      
    public SOItem level2RewardItem;         
    public float level2AnimasyonSuresi = 2.0f; 
    public float level2YukseklikOffset = 1.5f;

    // ========================================================================
    // BÖLÜM 3: İPUCU TRIGGER'I (YENİ EKLENEN KISIM)
    // ========================================================================
    [Header("BÖLÜM 3: İpucu Ayarları")]
    public float level3TextDuration = 5.0f; // Yazının ekranda kalma süresi

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        dialogueText.text = "";
        StartCoroutine(PlayIntroSequence());
    }

    // --- 1. BÖLÜM ---
    IEnumerator PlayIntroSequence()
    {
        // Envanter kontrolü vs. (Önceki kodlardan varsa buraya eklersin)
        yield return new WaitForSeconds(2f);
        dialogueText.text = "Hareket edemiyorsun...";
        yield return new WaitForSeconds(3f);
        dialogueText.text = "Sana ihtiyacin olani veriyorum.";
        yield return new WaitForSeconds(2f);

        if (introPlatformAnimator != null)
            introPlatformAnimator.SetTrigger("ShowCube");

        yield return new WaitForSeconds(introAnimasyonSuresi);
        SpawnItem(introRewardItem, introSkillPoint, introPlatformAnimator.transform, introYukseklikOffset);
        dialogueText.text = "";
    }

    // --- 2. BÖLÜM ---
    public void StartLevel2Event()
    {
        StartCoroutine(PlayLevel2Sequence());
    }

    IEnumerator PlayLevel2Sequence()
    {
        // 1. Önce tebrik mesajını yazdır
        dialogueText.text = "Tebrikler! Zorlu yolu aştın. İşte yeni yeteneğin...";
        
        // Oyuncunun ilk cümleyi okuması için biraz bekle
        yield return new WaitForSeconds(3f); 

        // 2. İkinci mesajı SİLMEDEN EKLE (+= operatörü)
        // \n\n ifadesi iki satır aşağı inmesini sağlar, böylece metinler karışmaz.
        dialogueText.text += "\n\nÖrümcekler yakınına gelip patlar. Patladıklarında envanterinden rastgele bir yeteneği 10 saniye etkisiz bırakır.";
        
        // Uzun metnin okunması için süre tanı (5 saniye)
        yield return new WaitForSeconds(5f); 

        // Platform animasyonunu tetikle
        if (level2PlatformAnimator != null)
        {
            level2PlatformAnimator.SetTrigger("ShowCubePuzzle");
        }

        yield return new WaitForSeconds(level2AnimasyonSuresi);

        SpawnItem(level2RewardItem, level2SkillPoint, level2PlatformAnimator.transform, level2YukseklikOffset);

        // En sonda yazıyı temizle
        dialogueText.text = "";
    }

    // --- 3. BÖLÜM (YENİ EKLENEN FONKSİYON) ---
    public void StartLevel3Event()
    {
        // Eğer zaten bir yazı varsa üstüne yazmasın veya karışmasın diye durdurabilirsin
        StopAllCoroutines(); 
        StartCoroutine(PlayLevel3Sequence());
    }

    IEnumerator PlayLevel3Sequence()
    {
        dialogueText.text = "Yeni yeteneğini kendin bulman gerekicek. İpuçlarini takip et.";
        
        // Belirlenen süre kadar bekle
        yield return new WaitForSeconds(level3TextDuration);

        // Yazıyı sil
        dialogueText.text = "";
    }

    // --- YARDIMCI FONKSİYONLAR ---
    void SpawnItem(SOItem item, Transform spawnPoint, Transform parentPlatform, float offset)
    {
        if (item != null && item.my_prefab != null && spawnPoint != null)
        {
            Vector3 finalPos = spawnPoint.position + (Vector3.up * offset);
            GameObject spawnedObj = Instantiate(item.my_prefab, finalPos, Quaternion.identity);
            
            if (parentPlatform != null)
            {
                spawnedObj.transform.SetParent(parentPlatform);
            }
        }
    }

    #region Die
    [SerializeField] private GameObject player;
    public Transform currentSpawnPoint;

    public void Die()
    {
        Debug.Log("Die");
        CharacterController cc = player.GetComponent<CharacterController>();

        if (cc != null) cc.enabled = false;
        player.transform.position = currentSpawnPoint.position;
        if (cc != null) cc.enabled = true;
    }
    #endregion
}