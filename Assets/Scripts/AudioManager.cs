using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Tower Sounds")]
    [SerializeField] private AudioClip archerShootSound;
    [SerializeField] private AudioClip magicShootSound;
    [SerializeField] private AudioClip iceFreezeSound;
    [SerializeField] private AudioClip stoneShootSound;
    [SerializeField] private AudioClip explosionSound;

    [Header("UI Sounds")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip towerPlaceSound;
    [SerializeField] private AudioClip towerUpgradeSound;
    [SerializeField] private AudioClip notEnoughGoldSound;
    [SerializeField] private AudioClip towerSellSound; // ADD THIS LINE
    [SerializeField] private AudioClip waveCompleteSound;
    [SerializeField] private AudioClip gameOverSound;
    [SerializeField] private AudioClip victorySound;

    [Header("Music")]
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip inGameMusic;
    [SerializeField] private float musicFadeDuration = 1.5f;

    // Music volume settings with defaults
    [Header("Music Volumes")]
    [SerializeField] private float mainMenuMusicVolume = 0.4f;
    [SerializeField] private float inGameMusicVolume = 0.2f;

    // Track if victory sound is already playing to prevent duplicates
    private bool _isVictorySoundPlaying = false;
    private bool _isGameOverSoundPlaying = false;

    // Music state tracking
    private AudioClip _currentMusic;
    private Coroutine _musicFadeCoroutine;

    private void Awake()
    {
        // Improved singleton pattern
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple AudioManager instances detected! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Reset flags when AudioManager is created
        _isVictorySoundPlaying = false;
        _isGameOverSoundPlaying = false;

        // Configure music source
        if (musicSource != null)
        {
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }
    }

    private void Start()
    {
        // Start with main menu music if we're in the main menu scene
        if (IsMainMenuScene())
        {
            PlayMainMenuMusic();
        }
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    // ========== MUSIC METHODS ==========

    // Main menu music with customizable volume
    public void PlayMainMenuMusic(float volume = -1f)
    {
        if (mainMenuMusic != null && musicSource != null)
        {
            float targetVolume = (volume >= 0) ? volume : mainMenuMusicVolume;
            PlayMusic(mainMenuMusic, targetVolume);
        }
    }

    // In-game music with customizable volume
    public void PlayInGameMusic(float volume = -1f)
    {
        if (inGameMusic != null && musicSource != null)
        {
            float targetVolume = (volume >= 0) ? volume : inGameMusicVolume;
            PlayMusic(inGameMusic, targetVolume);
        }
    }

    // Quick methods with specific volumes (like your tower sounds)
    public void PlayMainMenuMusicDefault() => PlayMainMenuMusic(mainMenuMusicVolume);
    public void PlayMainMenuMusicQuiet() => PlayMainMenuMusic(0.3f);
    public void PlayMainMenuMusicLoud() => PlayMainMenuMusic(0.4f);

    public void PlayInGameMusicDefault() => PlayInGameMusic(inGameMusicVolume);
    public void PlayInGameMusicQuiet() => PlayInGameMusic(0.2f);
    public void PlayInGameMusicLoud() => PlayInGameMusic(0.4f);

    private void PlayMusic(AudioClip music, float volume)
    {
        if (_currentMusic == music && musicSource.isPlaying) return;

        // Stop any ongoing fade coroutine
        if (_musicFadeCoroutine != null)
        {
            StopCoroutine(_musicFadeCoroutine);
        }

        _musicFadeCoroutine = StartCoroutine(FadeToNewMusic(music, volume));
    }

    private IEnumerator FadeToNewMusic(AudioClip newMusic, float targetVolume)
    {
        // Fade out current music if playing
        if (musicSource.isPlaying)
        {
            float startVolume = musicSource.volume;
            float timer = 0f;

            while (timer < musicFadeDuration)
            {
                timer += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, timer / musicFadeDuration);
                yield return null;
            }
        }

        // Switch to new music
        _currentMusic = newMusic;
        musicSource.clip = newMusic;
        musicSource.volume = 0f; // Start at 0 volume for fade in
        musicSource.Play();

        // Fade in new music
        float fadeTimer = 0f;
        while (fadeTimer < musicFadeDuration)
        {
            fadeTimer += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, targetVolume, fadeTimer / musicFadeDuration);
            yield return null;
        }

        musicSource.volume = targetVolume;
        _musicFadeCoroutine = null;
    }

    public void StopMusic()
    {
        if (_musicFadeCoroutine != null)
        {
            StopCoroutine(_musicFadeCoroutine);
        }
        StartCoroutine(FadeOutMusic());
    }

    private IEnumerator FadeOutMusic()
    {
        float startVolume = musicSource.volume;
        float timer = 0f;

        while (timer < musicFadeDuration)
        {
            timer += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, timer / musicFadeDuration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = startVolume; // Reset volume for next time
    }

    public void PauseMusic()
    {
        if (musicSource != null)
        {
            musicSource.Pause();
        }
    }

    public void ResumeMusic()
    {
        if (musicSource != null && _currentMusic != null)
        {
            musicSource.Play();
        }
    }

    // ========== VOLUME CONTROL METHODS ==========

    // Set music volume (affects currently playing music)
    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = volume;
        }
    }

    // Set SFX volume
    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
        {
            sfxSource.volume = volume;
        }
    }

    // Set default volumes (useful for resetting)
    public void SetMainMenuMusicVolume(float volume) => mainMenuMusicVolume = volume;
    public void SetInGameMusicVolume(float volume) => inGameMusicVolume = volume;

    // Get current volumes
    public float GetMainMenuMusicVolume() => mainMenuMusicVolume;
    public float GetInGameMusicVolume() => inGameMusicVolume;
    public float GetCurrentMusicVolume() => musicSource != null ? musicSource.volume : 0f;
    public float GetSFXVolume() => sfxSource != null ? sfxSource.volume : 0f;

    private bool IsMainMenuScene()
    {
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainMenu";
    }

    // ========== TOWER SOUND METHODS ==========
    public void PlayArcherShoot() => PlaySFX(archerShootSound, 0.31f);
    public void PlayMagicShoot() => PlaySFX(magicShootSound, 0.13f);
    public void PlayIceFreeze() => PlaySFX(iceFreezeSound, 0.33f);
    public void PlayStoneShoot() => PlaySFX(stoneShootSound, 0.35f);
    public void PlayExplosion() => PlaySFX(explosionSound, 0.35f);

    // ========== UI SOUND METHODS ==========
    public void PlayButtonClick() => PlaySFX(buttonClickSound, 0.2f);
    public void PlayTowerPlace() => PlaySFX(towerPlaceSound, 0.5f);
    public void PlayTowerUpgrade() => PlaySFX(towerUpgradeSound, 0.4f);
    public void PlayNotEnoughGold() => PlaySFX(notEnoughGoldSound, 0.25f);
    public void PlayTowerSell() => PlaySFX(towerSellSound, 0.33f); // ADD THIS METHOD

    public void PlayWaveComplete() => PlaySFX(waveCompleteSound, 0.8f);

    // Improved game over and victory sounds with protection
    public void PlayGameOver()
    {
        if (!_isGameOverSoundPlaying)
        {
            _isGameOverSoundPlaying = true;
            PlaySFX(gameOverSound, 0.3f);
            StartCoroutine(ResetGameOverFlagAfterDelay(gameOverSound.length));
        }
    }

    public void PlayVictory()
    {
        if (!_isVictorySoundPlaying)
        {
            _isVictorySoundPlaying = true;
            PlaySFX(victorySound, 0.3f);
            StartCoroutine(ResetVictoryFlagAfterDelay(victorySound.length));
        }
    }

    // Coroutines to reset flags after sound finishes
    private IEnumerator ResetVictoryFlagAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay + 0.1f); // Small buffer
        _isVictorySoundPlaying = false;
    }

    private IEnumerator ResetGameOverFlagAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay + 0.1f); // Small buffer
        _isGameOverSoundPlaying = false;
    }

    // Public method to manually reset flags (useful when changing scenes)
    public void ResetAllFlags()
    {
        _isVictorySoundPlaying = false;
        _isGameOverSoundPlaying = false;
    }
}