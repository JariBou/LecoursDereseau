using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuHandler : MonoBehaviour
{
    public void ButtonHostGame()
    {
        SceneManager.LoadScene("test-tome-sv", LoadSceneMode.Additive);
    }

    public void ButtonJoinGame()
    {
        // TODO
        // Check val in input
        // Start Client server and try to connect to server
        // Load scene
        SceneManager.LoadScene("test-tome-cl", LoadSceneMode.Additive);
    }


    public void ImportButton()
    {
        // TODO
    }
}
