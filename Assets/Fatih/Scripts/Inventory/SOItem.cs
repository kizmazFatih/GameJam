using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "ScriptableObjects/Item")]
public class SOItem : ScriptableObject
{
    [Header("Tooltip / Metin Ayarları")]
    public string skillName;
    [TextArea]
    public string pickupMessage;

    public PlayerSkill player_skill;
    //public bool isPlaceable;
    public Texture my_image;
    public GameObject my_prefab;

    [Header("Stok Bilgisi")]
    public int my_amount;
    public int max_stack;



}
