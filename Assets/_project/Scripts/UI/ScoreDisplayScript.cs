using System;
using System.Collections.Generic;
using _project.Scripts.GameLogic;
using UnityEngine;

namespace _project.Scripts.UI
{
    public class ScoreDisplayScript : MonoBehaviour
    {
        [SerializeField] private List<ScoreEntryDisplayScript> _entries = new();
        
        public void CleanEntries()
        {
            foreach (ScoreEntryDisplayScript scoreEntryDisplayScript in _entries)
            {
                scoreEntryDisplayScript.Hide();
            }
        }

        public void UpdateEntries(List<PlayerData> playerScores, PlayerData self)
        {
            CleanEntries();
            playerScores.Sort((x, y) => x.Score.CompareTo(y.Score));

            int min = Math.Min(playerScores.Count, _entries.Count-1);
            Debug.Log(min);

            bool scoreOfSelfDrawn = false;
            for (int i = 0; i < min; i++)
            {
                PlayerData playerData = playerScores[i];

                bool isScoreOfSelf = Equals(playerData, self);
                scoreOfSelfDrawn |= isScoreOfSelf;
                _entries[i].Show();
                _entries[i].UpdateDisplay(playerData.Name, playerData.Score, isScoreOfSelf);
            }

            if (!scoreOfSelfDrawn)
            {
                _entries[_entries.Count-1].Show();
                _entries[_entries.Count-1].UpdateDisplay(self.Name, self.Score, true);
            }
        }
    }
}