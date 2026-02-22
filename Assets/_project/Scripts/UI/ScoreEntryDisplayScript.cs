using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _project.Scripts.UI
{
    public class ScoreEntryDisplayScript : MonoBehaviour
    {
        [SerializeField] private TMP_Text _playerNameText;
        [SerializeField] private TMP_Text _playerScoreText;
        private Image _img;
        private Color _imgDefaultColor;
        private Color _imgSelfColor = Color.green;

        private void Awake()
        {
            _img = GetComponent<Image>();
            _imgDefaultColor = _img.color;
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
        
        public void UpdateDisplay(string playerName, uint playerScore, bool isScoreOfSelf)
        {
            _playerNameText.text = playerName;
            _playerScoreText.text = playerScore.ToString();
            _img.color = isScoreOfSelf ? _imgSelfColor : _imgDefaultColor;
        }
    }
}