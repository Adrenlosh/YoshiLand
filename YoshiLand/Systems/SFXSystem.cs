using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using MonoStereo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using YoshiLand.Models;

namespace YoshiLand.Systems
{
    public static class SFXSystem //FIXME: ???
    {
        private static ContentManager _content;
        private static Dictionary<string, SFX> _SFXs;
        private static List<SoundEffect> _SFXPlayers;
        private static float previousVolume = 1.0f;
        private static bool isMute = false;

        public static void Initialize(ContentManager content)
        {
            _content = content;
            _SFXs = new Dictionary<string, SFX>();
            _SFXPlayers = new List<SoundEffect>();
            LoadConfig();
        }

        private static void LoadConfig()
        {
            string filePath = Path.Combine(_content.RootDirectory, "Audio", "Audio.xml");
            using Stream stream = TitleContainer.OpenStream(filePath);
            using XmlReader xmlReader = XmlReader.Create(stream);
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlReader);
            XmlNodeList sfxNodes = doc.SelectNodes("//Audio/SoundEffects/SoundEffect");
            if (sfxNodes != null)
            {
                foreach (XmlNode sfxNode in sfxNodes)
                {
                    var sfx = new SFX
                    {
                        Name = sfxNode.Attributes["name"]?.Value,
                        File = sfxNode.Attributes["file"]?.Value,
                    };

                    if (sfxNode.Attributes["volume"] != null)
                    {
                        sfx.Volume = float.Parse(sfxNode.Attributes["volume"].Value);
                    }

                    if (sfxNode.Attributes["singleInstance"] != null)
                    {
                        sfx.SingleInstance = bool.Parse(sfxNode.Attributes["singleInstance"].Value);
                    }

                    if (!string.IsNullOrEmpty(sfx.File))
                    {
                        string sfxPath = Path.Combine(_content.RootDirectory, "Audio", "SFX", sfx.File);
                        sfx.Cached = CachedSoundEffect.Create(sfxPath);
                    }

                    if (!string.IsNullOrEmpty(sfx.Name))
                    {
                        _SFXs[sfx.Name] = sfx;
                    }
                }
            }
        }

        public static void Update(GameTime gameTime)
        {
            for (int i = _SFXPlayers.Count - 1; i >= 0; i--)
            {
                var player = _SFXPlayers[i];
                if (player.PlaybackState == NAudio.Wave.PlaybackState.Stopped)
                {
                    try { player.Dispose(); } catch { }
                    _SFXPlayers.RemoveAt(i);
                }
            }
        }

        public static void Play(string sfxName)
        {
            if (_SFXs.ContainsKey(sfxName) && _SFXs[sfxName].SingleInstance)
                return;

            if (_SFXs.TryGetValue(sfxName, out SFX sfx))
            {
                if (sfx.Cached != null)
                {
                    SoundEffect soundEffect = sfx.Cached.GetInstance();
                    soundEffect.Volume = sfx.Volume;
                    soundEffect.Play();
                    _SFXPlayers.Add(soundEffect);
                }
                else
                {
                    string sfxPath = Path.Combine(_content.RootDirectory, "Audio", "SFX", sfx.File);
                    var cached = CachedSoundEffect.Create(sfxPath);
                    SoundEffect soundEffect = cached.GetInstance();
                    soundEffect.Volume = sfx.Volume;
                    soundEffect.Play();
                    _SFXPlayers.Add(soundEffect);
                }
            }
            else
            {
                throw new ArgumentException(nameof(sfxName));
            }
        }

        public static void Stop(string sfxName)
        {

        }

        public static void StopAll()
        {
            for (int i = _SFXPlayers.Count - 1; i >= 0; i--)
            {
                try { _SFXPlayers[i].Stop(); } catch { }
                try { _SFXPlayers[i].Dispose(); } catch { }
            }
            _SFXPlayers.Clear();
        }

        public static void SetVolume(float volume)
        {
            foreach (var instance in _SFXPlayers)
            {
                try { instance.Volume = volume; } catch { }
            }
        }

        public static void Mute()
        {
            if (!isMute)
            {
                SetVolume(0f);
                isMute = true;
            }
        }

        public static void Unmute()
        {
            if (isMute)
            {
                SetVolume(previousVolume);
                isMute = false;
            }
        }

        public static void ToggleMute()
        {
            if (isMute)
            {
                Unmute();
            }
            else
            {
                Mute();
            }
        }

        public static bool IsPlaying(string sfxName)
        {
            return true;
        }

        public static void Dispose()
        {
            StopAll();
            if (_SFXs != null)
            {
                foreach (var sfx in _SFXs.Values)
                {
                    try { sfx.Cached?.Dispose(); } catch { }
                }
                _SFXs.Clear();
            }
        }
    }
}
