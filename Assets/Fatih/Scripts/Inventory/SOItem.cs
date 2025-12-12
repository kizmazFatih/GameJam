using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "ScriptableObjects/Item")]
public class SOItem : ScriptableObject
{
    [Header("Tooltip")]
    public string skillName;
    [TextArea]
    public string pickupMessage;
    public PlayerSkill player_skill;

    [Header("Görsel")]
    public Texture my_image;
    public GameObject my_prefab;

    [Header("Stok")]
    public int my_amount;
    public int max_stack;



}
