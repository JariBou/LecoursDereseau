using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuHandler : MonoBehaviour
{
    public void ButtonHostGame()
    {
        SceneManager.LoadScene("test-tome-sv", LoadSceneMode.Single);
    }

    public void ButtonJoinGame()
    {
        // TODO
        // Check val in input
        // Start Client server and try to connect to server
        // Load scene
        SceneManager.LoadScene("test-tome-cl", LoadSceneMode.Single);
    }


    public void ImportButton()
    {
        // TODO
    }
}
