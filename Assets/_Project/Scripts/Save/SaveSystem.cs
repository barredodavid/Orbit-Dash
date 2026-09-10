using System;
using System.Collections.Generic;
using UnityEngine;

namespace OrbitDash.Save
{
    /// <summary>
    /// Persists the top 5 scores locally as JSON inside PlayerPrefs. JsonUtility can't serialize a
    /// bare list, so it's wrapped in a small data class.
    /// </summary>
    public static class SaveSystem
    {
        private const string TopScoresKey = "OrbitDash.TopScores";
        private const int MaxEntries = 5;

        [Serializable]
        private class ScoreBoardData
        {
            public List<int> scores = new();
        }

        /// <summary>Top scores, best first. Empty if nothing has been saved yet.</summary>
        public static IReadOnlyList<int> LoadTopScores()
        {
            string json = PlayerPrefs.GetString(TopScoresKey, string.Empty);
            if (string.IsNullOrEmpty(json))
            {
                return Array.Empty<int>();
            }

            ScoreBoardData data = JsonUtility.FromJson<ScoreBoardData>(json);
            return (IReadOnlyList<int>)data?.scores ?? Array.Empty<int>();
        }

        public static int GetBestScore()
        {
            IReadOnlyList<int> scores = LoadTopScores();
            return scores.Count > 0 ? scores[0] : 0;
        }

        /// <summary>Inserts a new score into the board, keeping only the top 5. Returns the updated board.</summary>
        public static IReadOnlyList<int> SubmitScore(int score)
        {
            var scores = new List<int>(LoadTopScores()) { score };
            scores.Sort((a, b) => b.CompareTo(a));

            if (scores.Count > MaxEntries)
            {
                scores.RemoveRange(MaxEntries, scores.Count - MaxEntries);
            }

            var data = new ScoreBoardData { scores = scores };
            PlayerPrefs.SetString(TopScoresKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
            return scores;
        }
    }
}
