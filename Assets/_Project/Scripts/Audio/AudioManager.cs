using OrbitDash.Core;
using UnityEngine;

namespace OrbitDash.Audio
{
    /// <summary>
    /// Plays SFX for key gameplay moments (tap, dodge, collision) and loops background music.
    /// Entirely event-driven, no references to gameplay scripts. Each clip picks itself up
    /// automatically from a Resources path if the explicit Inspector field is left empty; if
    /// neither is set, playback safely no-ops (no music pack has been chosen yet - see README).
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [Header("SFX")]
        [SerializeField] private AudioClip tapClip;
        [SerializeField] private string tapClipResourcePath = "SFX/tap";

        [SerializeField] private AudioClip dodgeClip;
        [SerializeField] private string dodgeClipResourcePath = "SFX/dodge";

        [SerializeField] private AudioClip collisionClip;
        [SerializeField] private string collisionClipResourcePath = "SFX/collision";

        [Header("Music")]
        [SerializeField] private AudioClip musicClip;
        [SerializeField] private string musicClipResourcePath = "Music/theme";
        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.5f;

        private AudioSource _sfxSource;
        private AudioSource _musicSource;
        private AudioSource _nearMissSource; // own source: its pitch rises with the combo without detuning the other SFX

        private void Awake()
        {
            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;

            _nearMissSource = gameObject.AddComponent<AudioSource>();
            _nearMissSource.playOnAwake = false;

            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.playOnAwake = false;
            _musicSource.loop = true;
            _musicSource.volume = musicVolume;

            tapClip = ResolveClip(tapClip, tapClipResourcePath);
            dodgeClip = ResolveClip(dodgeClip, dodgeClipResourcePath);
            collisionClip = ResolveClip(collisionClip, collisionClipResourcePath);
            musicClip = ResolveClip(musicClip, musicClipResourcePath);
        }

        private void OnEnable()
        {
            GameEvents.RingSwitched += HandleRingSwitched;
            GameEvents.NearMiss += HandleNearMiss;
            GameEvents.PlayerHit += HandlePlayerHit;
            GameEvents.SoundSettingChanged += HandleSoundSettingChanged;
        }

        private void OnDisable()
        {
            GameEvents.RingSwitched -= HandleRingSwitched;
            GameEvents.NearMiss -= HandleNearMiss;
            GameEvents.PlayerHit -= HandlePlayerHit;
            GameEvents.SoundSettingChanged -= HandleSoundSettingChanged;
        }

        private void Start()
        {
            PlayMusic();
        }

        private void HandleRingSwitched() => PlaySfx(tapClip);

        /// <summary>The "dodge" whoosh rewards real close calls only, one semitone higher per combo step.</summary>
        private void HandleNearMiss(int combo, int bonus)
        {
            if (dodgeClip == null || !AudioPreferences.IsSoundEnabled)
            {
                return;
            }
            _nearMissSource.pitch = Mathf.Pow(1.059463f, Mathf.Min(combo - 1, 12));
            _nearMissSource.PlayOneShot(dodgeClip);
        }

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

        private static AudioClip ResolveClip(AudioClip explicitClip, string resourcePath)
        {
            if (explicitClip != null)
            {
                return explicitClip;
            }
            return string.IsNullOrEmpty(resourcePath) ? null : Resources.Load<AudioClip>(resourcePath);
        }
    }
}
