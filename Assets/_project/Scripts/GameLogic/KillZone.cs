using _project.Scripts.GameLogic;
using UnityEngine;

public class KillZone : MonoBehaviour
{
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            Debug.Log("Killbox detects player");
            PlayerMovementScript player = collision.gameObject.GetComponent<PlayerMovementScript>();
            player.RecordDeath();
        }
    }

}
