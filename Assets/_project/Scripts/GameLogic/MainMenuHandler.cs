using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuHandler : MonoBehaviour
{
    public void ButtonHostGame()
    {
        SceneManager.LoadScene("PipotamPremiumServer", LoadSceneMode.Additive);
    }

    public void ButtonJoinGame()
    {
        // TODO
        // Check val in input
        // Start Client server and try to connect to server
        // Load scene
    }


    public void ImportButton()
    {
        // TODO
    }
}
