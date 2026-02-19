using System;
using UnityEngine;

namespace _project.Scripts.GameLogic
{
    public class HitBoxScript : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            PlayerScript playerScript = other.GetComponent<PlayerScript>();
            if (playerScript != null)
            {
                Debug.Log("Hit Player!!");
                // playerScript.TakeDamage()
            }
        }
    }
}