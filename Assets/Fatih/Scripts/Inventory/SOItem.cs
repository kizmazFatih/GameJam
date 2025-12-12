using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "ScriptableObjects/Item")]
public class SOItem : ScriptableObject
{

    public bool isPlaceable;
    public Texture my_image;
    public GameObject my_prefab;
    public int my_amount;
    public int max_stack;

}
