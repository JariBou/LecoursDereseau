using UnityEngine;

namespace _project.Scripts.GameLogic
{
    public class HitBoxScript : MonoBehaviour
    {
        private Transform _transformParent;

        private void Awake()
        {
            _transformParent = transform.parent;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.transform == _transformParent)
            {
                return;
            }
            Debug.Log("HitBox");
            ReplicatedPlayerScriptBase replicatedPlayerScriptBase = other.GetComponent<ReplicatedPlayerScriptBase>();
            if (replicatedPlayerScriptBase != null)
            {
                Vector3 direction = replicatedPlayerScriptBase.transform.position - _transformParent.position;
                replicatedPlayerScriptBase.Hurt(direction);
            }
            // PlayerMovementScript playerMovementScript = other.GetComponent<PlayerMovementScript>();
            // if (playerMovementScript != null)
            // {
            //     Debug.Log("Hit Player!!");
            //     playerMovementScript.RecordDamage();
            // }
        }
    }
}