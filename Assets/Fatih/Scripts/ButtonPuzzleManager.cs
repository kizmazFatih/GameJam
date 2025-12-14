using System.Collections.Generic;
using UnityEngine;

public class ButtonPuzzleManager : MonoBehaviour
{
    [Header("Ayarlar")]
    // Doğru sıralama (Örn: 0, 2, 1, 3 -> 1.buton, 3.buton, 2.buton, 4.buton)
    public int[] dogruSifre; 
    
    [Header("Kapı")]
    public GameObject kapiObjesi; // Açılacak kapı
    public Animator kapiAnimator; // Animasyon varsa

    // Oyuncunun şu ana kadar doğru bastığı sayısı
    private int currentSequenceIndex = 0;

    public void ButonaBasildi(int buttonID)
    {
        // 1. Oyuncu doğru sıradaki butona mı bastı?
        if (buttonID == dogruSifre[currentSequenceIndex])
        {
            Debug.Log("Doğru buton: " + buttonID);
            currentSequenceIndex++; // Bir sonraki aşamaya geç

            // 2. Şifre tamamlandı mı?
            if (currentSequenceIndex >= dogruSifre.Length)
            {
                KapiyiAc();
            }
        }
        else
        {
            // 3. Yanlış butona bastı -> SIFIRLA
            Debug.Log("Yanlış buton! Sıfırlandı.");
            Sifirla();
        }
    }

    void KapiyiAc()
    {
        Debug.Log("ŞİFRE DOĞRU! KAPI AÇILIYOR.");
        
        // Yöntem A: Obje yok etme (Basit)
        // Destroy(kapiObjesi); 

        // Yöntem B: Animasyon çalıştırma (Önerilen)
        if (kapiAnimator != null)
        {
            kapiAnimator.SetTrigger("Open");
        }
        // Yöntem C: Fiziksel olarak döndürme/kaydırma
        else if (kapiObjesi != null)
        {
            // Basitçe kapıyı yukarı kaydırır veya yok eder
            kapiObjesi.SetActive(false); 
        }
    }

    void Sifirla()
    {
        currentSequenceIndex = 0;
        // İstersen burada "Hata Sesi" çalabilirsin.
    }
}