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
    // BÖLÜM 1: BAŞLANGIÇ (INTRO) - BURASI "ShowCube" KULLANIYOR
    // ========================================================================
    [Header("BÖLÜM 1: İntro Ayarları")]
    public Animator introPlatformAnimator; 
    public Transform introSkillPoint;      
    public SOItem introRewardItem;         
    
    public float introAnimasyonSuresi = 1.5f; 
    public float introYukseklikOffset = 1.5f;

    // ========================================================================
    // BÖLÜM 2: PUZZLE SONRASI - BURASI ARTIK "ShowCubePuzzle" KULLANIYOR
    // ========================================================================
    [Header("BÖLÜM 2: Puzzle Sonu Ayarları")]
    public Animator level2PlatformAnimator; 
    public Transform level2SkillPoint;      
    public SOItem level2RewardItem;         
    
    public float level2AnimasyonSuresi = 2.0f; 
    public float level2YukseklikOffset = 1.5f;

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

    // --- 1. BÖLÜM (Eski Animasyon) ---
    IEnumerator PlayIntroSequence()
    {
        yield return new WaitForSeconds(2f);
        dialogueText.text = "Hareket edemiyorsun...";
        yield return new WaitForSeconds(3f);
        dialogueText.text = "Sana ihtiyacın olanı veriyorum.";
        yield return new WaitForSeconds(2f);

        if (introPlatformAnimator != null)
            introPlatformAnimator.SetTrigger("ShowCube"); // BURASI ESKİSİ GİBİ KALDI

        yield return new WaitForSeconds(introAnimasyonSuresi);

        SpawnItem(introRewardItem, introSkillPoint, introPlatformAnimator.transform, introYukseklikOffset);

        dialogueText.text = "";
    }

    // --- 2. BÖLÜM (YENİ ANİMASYON TRIGGER'I) ---
    public void StartLevel2Event()
    {
        StartCoroutine(PlayLevel2Sequence());
    }

    IEnumerator PlayLevel2Sequence()
    {
        dialogueText.text = "Tebrikler! Zorlu yolu aştın. İşte yeni yeteneğin...";
        yield return new WaitForSeconds(3f); 

        if (level2PlatformAnimator != null)
        {
            // DEĞİŞİKLİK BURADA: Artık yeni trigger ismini çağırıyoruz
            level2PlatformAnimator.SetTrigger("ShowCubePuzzle");
        }

        yield return new WaitForSeconds(level2AnimasyonSuresi);

        SpawnItem(level2RewardItem, level2SkillPoint, level2PlatformAnimator.transform, level2YukseklikOffset);

        dialogueText.text = "";
    }

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

        // 2. Eğer varsa, geçici olarak kapat
        if (cc != null)
        {
            cc.enabled = false;
        }

        // 3. Pozisyonu değiştir (Işınla)
        player.transform.position = currentSpawnPoint.position;

        // 4. CharacterController'ı tekrar aç
        if (cc != null)
        {
            cc.enabled = true;
        }
    }

    #endregion
}