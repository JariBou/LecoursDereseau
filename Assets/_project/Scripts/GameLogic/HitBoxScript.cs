using System;
using UnityEngine;

namespace _project.Scripts.GameLogic
{
    public class HitBoxScript : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            PlayerMovementScript playerMovementScript = other.GetComponent<PlayerMovementScript>();
            if (playerMovementScript != null)
            {
                Debug.Log("Hit Player!!");
                playerMovementScript.RecordDamage();
            }
        }
    }
}