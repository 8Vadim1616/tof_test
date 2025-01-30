using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Libraries.RSG;
using DG.Tweening;
using UnityEngine;

namespace Assets.Scripts.Core.Sound
{
    public partial class SoundManager : MonoBehaviour
    {
        public const string DefaultMusic = "ms_level_music";
		
        private const float MinVolume = 0f;
        public const float MaxVolume = 1f;
        private const float FadeDuration = 0f;
        private const float LoopSoundLengthOffset = 0.1f; // sec
        private const float MinDelayBetweenSameSounds = 0.1f; // sec
        private float _limitedMusicVolume = 0.75f;

        private AudioSource _musicSource;
        private IPromise _musicIPromise;
        private string _currentMusicSoundClass;
        private AudioSource _soundSource;

        private readonly List<string> _loopedSoundLoading = new List<string>();
        private readonly Dictionary<string, float> _playingSoundsEndTime = new Dictionary<string, float>();

        private string GetMusicUrl(string soundClass) => $"sound/{soundClass}";
        private string GetSoundUrl(string soundClass) => $"sound/sfx/{soundClass}";

        private void Start()
        {
            _musicSource = CreateAudioSource(MinVolume, true);
            _soundSource = CreateAudioSource(MaxVolume, false);

#if UNITY_WEBGL || UNITY_IOS
            Game.GameReloader.ApplicationUnFocus += OnDeactivate;
            Game.GameReloader.ApplicationFocus += OnActivate;
#endif
		}


		public void Init()
		{
		}

		private AudioSource CreateAudioSource(float volume, bool loop)
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.spatialBlend = 0f;
            source.volume = volume;
            source.minDistance = 100f;
            source.maxDistance = 100f;
            source.playOnAwake = false;
            source.loop = loop;
            return source;
        }

		public void PlayMusic(string soundClass = null)
		{
			Debug.Log($"Play music. Enabled = {!IsMusicOff}");
			if (IsMusicOff) return;

			if (string.IsNullOrEmpty(soundClass))
			{
				soundClass = DefaultMusic;
			}

			if (_currentMusicSoundClass == soundClass) return;

            _currentMusicSoundClass = soundClass;
            
            var url = GetMusicUrl(soundClass);
            Debug.Log($"Play music {url}");

            _musicIPromise?.Done();
            _musicIPromise = SwapToSound(url, _musicSource, _limitedMusicVolume);
        }
        
        public void StopMusic(bool doItInstantly)
        {
			Debug.Log("StopMusic " + _musicSource);
            if (_musicSource == null) return;

            if (doItInstantly)
            {
                _musicSource.volume = MinVolume;
                _musicSource.Stop();
                _musicSource.DOKill();

                _musicIPromise?.Done();

                _currentMusicSoundClass = string.Empty;
            }
            else
            {
                FadeVolumeTo(MinVolume, FadeDuration, _musicSource)
                    .Then(() => StopMusic(true));
            }
        }

        public void PlaySound(string soundClass, int repeats = 1, float volumeScale = 1f, bool playOnlyIfNotPlaying = false)
        {
            if (IsSoundOff) return;
            if (repeats <= 0) return;
            if (string.IsNullOrEmpty(soundClass)) return;

            var url = GetSoundUrl(soundClass);

			if (IsNotPlayThisSoundNow(soundClass))
	            LoadClip(url)
	                .Then(clip =>
	                {
	                    if (IsNotPlayThisSoundNow(soundClass))
	                        PlaySoundClip(soundClass, clip, repeats, volumeScale);
	                })
	                .Catch(ex => { Debug.LogWarning(ex.Message); });
        }

        private bool IsNotPlayThisSoundNow(string soundClass)
        {
            if (string.IsNullOrEmpty(soundClass)) return false;
            if (IsSoundOff) return false;
            
            if (_playingSoundsEndTime.ContainsKey(soundClass)
                && _playingSoundsEndTime[soundClass] > Time.time) return false;
            
            return true;
        }

        private void PlaySoundClip(string soundClass, AudioClip clip, int repeats, float volumeScale = 1f)
        {
            if (clip == null) return;

            var delay = clip.length - LoopSoundLengthOffset;
            var blockRepeatTime = Time.time + MinDelayBetweenSameSounds + delay * (repeats - 1);
            
            if (_playingSoundsEndTime.ContainsKey(soundClass))
                _playingSoundsEndTime[soundClass] = blockRepeatTime;
            else
                _playingSoundsEndTime.Add(soundClass, blockRepeatTime);

            if (repeats == 1)
            {
                PlaySoundClipOneShot(clip, null, volumeScale);
                //Debug.Log($"Play sound: {soundClass}");
            }
            else
            {
                //Debug.Log($"Play sound {repeats} times: {soundClass}");
                DOTween.Sequence()
                    .AppendCallback(() => PlaySoundClipOneShot(clip))
                    .AppendInterval(delay)
                    .SetLoops(repeats, LoopType.Restart);
            }
        }

        private void PlaySoundClipOneShot(AudioClip clip, AudioSource source = null, float volumeScale = 1f)
        {
            if (IsSoundOff) return;

			if (source == null)
				source = _soundSource;
			source.PlayOneShot(clip, volumeScale);
        }

		private readonly Dictionary<string, AudioSource> _loopedSoundAudioSources = new Dictionary<string, AudioSource>();

		private bool StopLoopSoundInternal(string soundClass)
		{
			if (_loopedSoundAudioSources.ContainsKey(soundClass))
			{
				_loopedSoundAudioSources[soundClass].Stop();
				Destroy(_loopedSoundAudioSources[soundClass]);
				_loopedSoundAudioSources.Remove(soundClass);
				return true;
			}

			return false;
		}


		private void StartLoopPlay(string soundClass, AudioClip clip)
        {
            if (IsSoundOff) return;
            if (soundClass == null || clip == null) return;
            if (WasLoopClipStoppedWhenLoading(soundClass)) return;
            ForgetLoopClipLoadStarted(soundClass);
            
			StopLoopSoundInternal(soundClass);
			
			var source = CreateAudioSource(MaxVolume, true);
			source.clip = clip;
			source.Play();

			_loopedSoundAudioSources[soundClass] = source;
		}
		
		private void StopLoopSound(string soundClass)
		{
			if (soundClass == null) return;

			StopLoopClipWhenItLoading(soundClass);
			if (StopLoopSoundInternal(soundClass))
				Debug.Log($"Stop loop sound: {soundClass}");
			else
				Debug.Log($"Has not looped sound to stop: {soundClass}");
		}
		
		public void StopAllLoopedSounds()
		{
			var keys = _loopedSoundAudioSources.Keys.ToList();

			foreach (var key in keys)
				StopLoopSoundInternal(key);

			_loopedSoundAudioSources.Clear();
			Debug.Log("All looped sounds stopped.");
		}
        
        private bool IsUserSettingsNull => Game.User == null || Game.User.Settings == null;
        
        private bool IsSoundOff => IsUserSettingsNull || Game.User.Settings.IsSound == false;

        private bool IsMusicOff => IsUserSettingsNull || Game.User.Settings.IsMusic == false; 
        
        /// <summary>
        /// останавливает текущее проигрывание звуков
        /// не останавливает LoopSounds
        /// не останавливает PlaySound с Repeat > 1, используется только в Action, непонятно надо ли
        /// </summary>
        public void StopAllPlayingSounds()
        {
            _soundSource.Stop();
        }

        private void PlayLoopSound(string soundClass)
        {
            if (IsSoundOff) return;
            if (_loopedSoundLoading.Contains(soundClass)) return;
            if (string.IsNullOrEmpty(soundClass)) return;

            var url = GetSoundUrl(soundClass);
            Debug.Log($"Play loop sound {url}");

            RememberLoopClipLoadHasStarted(soundClass);

            LoadClip(url)
                .Then(clip => { StartLoopPlay(soundClass, clip); })
                .Catch(ex => { Debug.LogWarning(ex.Message); });
        }
        
        private void RememberLoopClipLoadHasStarted(string soundClass)
        {
            if (!_loopedSoundLoading.Contains(soundClass)) 
                _loopedSoundLoading.Add(soundClass);
        }
        
        private void ForgetLoopClipLoadStarted(string soundClass)
        {
            if (_loopedSoundLoading.Contains(soundClass)) 
                _loopedSoundLoading.Remove(soundClass);
        }
        
        private void StopLoopClipWhenItLoading(string soundClass)
        {
            if (_loopedSoundLoading.Contains(soundClass)) 
                _loopedSoundLoading.Remove(soundClass);
        }
                
        private bool WasLoopClipStoppedWhenLoading(string soundClass)
        {
            return !_loopedSoundLoading.Contains(soundClass);
        }

        private void OnDestroy()
        {
            _musicSource.DOKill();
            StopAllLoopedSounds();
            
#if UNITY_WEBGL || UNITY_IOS
            Game.GameReloader.ApplicationUnFocus -= OnDeactivate;
            Game.GameReloader.ApplicationFocus -= OnActivate;
#endif
        }

        private static IPromise SwapToSound(string url, AudioSource source, float volume = MaxVolume)
        {
            source.DOKill();

            var result = FadeVolumeTo(MinVolume, FadeDuration, source)
                .Then(() => LoadClipToSource(url, source))
                .Then(() => source.Play())
                .Then(() => FadeVolumeTo(volume, FadeDuration, source))
                .Catch(ex => { Debug.LogWarning(ex.Message); });

            return result;
        }

        private static IPromise FadeVolumeTo(float volume, float duration, AudioSource source,
            bool proportionalDuration = true)
		{
			source.volume = volume;
			return Promise.Resolved();
            // процент длительности в зависимости от текущей громкости
			/*
            if (proportionalDuration)
                duration *= Math.Abs(source.volume - volume) / (MaxVolume - MinVolume);

            var result = new Promise();
            source
                .DOFade(volume, duration)
                .OnComplete(() => result.Resolve());
            return result;
            */
        }

        private static IPromise LoadClipToSource(string url, AudioSource source)
        {
            var result = new Promise();

            LoadClip(url)
                .Then(clip =>
                {
                    source.clip = clip;
                    result.Resolve();
                })
                .Catch(ex => { result.Reject(ex); });

            return result;
        }

        private static IPromise<AudioClip> LoadClip(string url)
        {
            var result = new Promise<AudioClip>();
            AssetsManager.AssetsManager.Instance.Loader.LoadAndCache<AudioClip>(url)
                .Then(clip =>
                {
                    if (clip == null)
                        result.Reject(
                            new NullReferenceException(
                                $"Can't load {url}\nCheck: 1) File exist? 2) Addressable selected?"));
                    else
                        result.Resolve(clip);
                })
                .Catch(ex => { result.Reject(ex); });
            return result;
        }

		public void ChangeSound(bool isSound)
		{
			if (isSound)
			{
			}
			else
			{
				Game.Sound.StopAllLoopedSounds();
				Game.Sound.StopAllPlayingSounds();
			}
            
            //Game.HUD?.Content?.SettingsPanel?.UpdateSoundAndMusic();
		}

		public void ChangeMusic(bool isMusic)
		{
			if (isMusic)
			{
				PlayMusic();
			}
			else
			{
				Game.Sound.StopMusic(true);
			}

            //Game.HUD?.Content?.SettingsPanel?.UpdateSoundAndMusic();
        }

		private float _lastMusicVolume;
		private float _lastSoundVolume;

		public void OnActivate()
		{
			if (Game.User?.Settings == null)
				return;

			ChangeMusic(Game.User.Settings.IsMusic);
			ChangeSound(Game.User.Settings.IsSound);
		}

		public void OnDeactivate()
		{
			ChangeMusic(false);
			ChangeSound(false);
		}
	}
}