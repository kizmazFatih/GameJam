using UnityEngine;
using TMPro;

public class SkillTooltipManager : MonoBehaviour
{
    public static SkillTooltipManager Instance;

    public GameObject tooltipPanel;
    public TextMeshProUGUI tooltipText;

    private Transform targetObject; 
    public float yukseklikAyari = 3f; 

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
        
        tooltipPanel.SetActive(false);
    }

    private void Update()
    {
        // Eğer panel açıksa ve bir hedefimiz varsa
        if (tooltipPanel.activeSelf && targetObject != null)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(targetObject.position + (Vector3.up * yukseklikAyari));
            
            tooltipPanel.transform.position = screenPos;
        }
    }

    public void ShowTooltip(string skillIsmi, string ozelMesaj, Transform target)
    {
        targetObject = target; 
        tooltipPanel.SetActive(true);
        tooltipText.text = $"<color=yellow><b>{skillIsmi}</b></color>\n<size=80%>{ozelMesaj}</size>";
    }

    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
        targetObject = null; 
    }
}