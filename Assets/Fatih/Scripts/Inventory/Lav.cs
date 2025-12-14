using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lav : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            StoryEventManager.Instance.Die();
        }
    }
}
