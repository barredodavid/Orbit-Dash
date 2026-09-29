using OrbitDash.Core;
using OrbitDash.Save;
using UnityEngine;

namespace OrbitDash.Gameplay
{
    /// <summary>
    /// Ticks the live score (+1 per second survived) while the game is playing, and submits the
    /// final score to <see cref="SaveSystem"/> the moment a run ends. Entirely event-driven: it never
    /// references GameManager or the UI directly.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        private float _elapsedSeconds;
        private int _currentScore;
        private bool _isCounting;
        private int _bonusPoints;

        private void OnEnable()
        {
            GameEvents.GameStarted += HandleGameStarted;
            GameEvents.StateChanged += HandleStateChanged;
            GameEvents.NearMiss += HandleNearMiss;
        }

        private void OnDisable()
        {
            GameEvents.GameStarted -= HandleGameStarted;
            GameEvents.StateChanged -= HandleStateChanged;
            GameEvents.NearMiss -= HandleNearMiss;
        }

        private void Update()
        {
            if (!_isCounting)
            {
                return;
            }

            _elapsedSeconds += Time.deltaTime;
            UpdateScore();
        }

        private void HandleNearMiss(int combo, int bonus)
        {
            _bonusPoints += bonus;
            UpdateScore();
        }

        /// <summary>Score = seconds survived + near-miss bonuses.</summary>
        private void UpdateScore()
        {
            int newScore = Mathf.FloorToInt(_elapsedSeconds) + _bonusPoints;
            if (newScore == _currentScore)
            {
                return;
            }

            _currentScore = newScore;
            GameEvents.RaiseScoreChanged(_currentScore);
        }

        private void HandleGameStarted()
        {
            _elapsedSeconds = 0f;
            _bonusPoints = 0;
            _currentScore = 0;
            GameEvents.RaiseScoreChanged(_currentScore);
        }

        private void HandleStateChanged(GameState state)
        {
            _isCounting = state == GameState.Playing;

            if (state == GameState.GameOver)
            {
                SaveSystem.SubmitScore(_currentScore);
            }
        }
    }
}
