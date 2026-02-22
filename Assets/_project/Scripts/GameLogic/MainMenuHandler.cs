using System;
using _project.Scripts.GameLogic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuHandler : MonoBehaviour
{
    
    [SerializeField] private TMP_InputField _addressInputField;
    [SerializeField] private TMP_InputField _portInputField;
    
    public void ButtonHostGame()
    {
        SetupConnectionData();
        SceneManager.LoadScene("test-tome-sv", LoadSceneMode.Single);
    }

    private void SetupConnectionData()
    {
        ConnectionParamsData.Instance.IpAddress = _addressInputField.text;
        ConnectionParamsData.Instance.Port = Convert.ToUInt16(_portInputField.text);
    }

    public void ButtonJoinGame()
    {
        // TODO
        // Check val in input
        // Start Client server and try to connect to server
        // Load scene
        SetupConnectionData();
        SceneManager.LoadScene("test-tome-cl", LoadSceneMode.Single);
    }


    public void ImportButton()
    {
        // TODO
    }
}
