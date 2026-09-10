using OrbitDash.Core;
using UnityEngine;

namespace OrbitDash.Audio
{
    /// <summary>
    /// Plays SFX for key gameplay moments (tap, dodge, collision) and loops background music.
    /// Entirely event-driven, no references to gameplay scripts. Every clip field is left empty
    /// until an audio pack is imported (see README, step 7) - PlayOneShot/Play safely no-op on null.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [Header("SFX")]
        [SerializeField] private AudioClip tapClip;
        [SerializeField] private AudioClip dodgeClip;
        [SerializeField] private AudioClip collisionClip;

        [Header("Music")]
        [SerializeField] private AudioClip musicClip;
        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.5f;

        private AudioSource _sfxSource;
        private AudioSource _musicSource;

        private void Awake()
        {
            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;

            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.playOnAwake = false;
            _musicSource.loop = true;
            _musicSource.volume = musicVolume;
        }

        private void OnEnable()
        {
            GameEvents.RingSwitched += HandleRingSwitched;
            GameEvents.ObstacleDodged += HandleObstacleDodged;
            GameEvents.PlayerHit += HandlePlayerHit;
            GameEvents.SoundSettingChanged += HandleSoundSettingChanged;
        }

        private void OnDisable()
        {
            GameEvents.RingSwitched -= HandleRingSwitched;
            GameEvents.ObstacleDodged -= HandleObstacleDodged;
            GameEvents.PlayerHit -= HandlePlayerHit;
            GameEvents.SoundSettingChanged -= HandleSoundSettingChanged;
        }

        private void Start()
        {
            PlayMusic();
        }

        private void HandleRingSwitched() => PlaySfx(tapClip);

        private void HandleObstacleDodged() => PlaySfx(dodgeClip);

        private void HandlePlayerHit() => PlaySfx(collisionClip);

        private void HandleSoundSettingChanged(bool isEnabled) => _musicSource.mute = !isEnabled;

        private void PlaySfx(AudioClip clip)
        {
            if (clip == null || !AudioPreferences.IsSoundEnabled)
            {
                return;
            }
            _sfxSource.PlayOneShot(clip);
        }

        private void PlayMusic()
        {
            if (musicClip == null)
            {
                return;
            }
            _musicSource.clip = musicClip;
            _musicSource.mute = !AudioPreferences.IsSoundEnabled;
            _musicSource.Play();
        }
    }
}
