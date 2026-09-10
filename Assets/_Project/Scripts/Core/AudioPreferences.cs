using UnityEngine;

namespace OrbitDash.Core
{
    /// <summary>Single source of truth for the sound on/off preference, shared by MenuScreen and AudioManager.</summary>
    public static class AudioPreferences
    {
        private const string SoundEnabledPrefKey = "OrbitDash.SoundEnabled";

        public static bool IsSoundEnabled => PlayerPrefs.GetInt(SoundEnabledPrefKey, 1) != 0;

        public static void SetSoundEnabled(bool isEnabled)
        {
            PlayerPrefs.SetInt(SoundEnabledPrefKey, isEnabled ? 1 : 0);
            PlayerPrefs.Save();
            GameEvents.RaiseSoundSettingChanged(isEnabled);
        }
    }
}
