using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    public static AbilityManager instance;

    public int maxSlots = 3;
    HashSet<PlayerSkill> activeSkills = new HashSet<PlayerSkill>();


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        AddAbility(PlayerSkill.Vertical);
        AddAbility(PlayerSkill.Horizontal);
        AddAbility(PlayerSkill.Jump);
        AddAbility(PlayerSkill.Sprint);

    }

    public void AddAbility(PlayerSkill type)
    {
        if (activeSkills.Count < maxSlots)
        {
            activeSkills.Add(type);
        }

    }
}
