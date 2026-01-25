using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using MonoStereo.Sources.Songs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using YoshiLand.Models;

namespace YoshiLand.Systems
{
    public static class SongSystem
    {
        private static ContentManager _content;
        private static Dictionary<string, Song> _songs;
        private static float previousVolume = 1.0f;
        private static bool isMute = false;

        private static MonoStereo.Song songPlayer;
        private static SongReader songReader;

        public static void Initialize(ContentManager content)
        {
            _content = content;
            _songs = new Dictionary<string, Song>();
            LoadConfig();
        }

        private static void LoadConfig()
        {
            string filePath = Path.Combine(_content.RootDirectory, "Audio", "Audio.xml");
            using Stream stream = TitleContainer.OpenStream(filePath);
            using XmlReader xmlReader = XmlReader.Create(stream);
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlReader);
            XmlNodeList songNodes = doc.SelectNodes("//Audio/Songs/Song");
            if (songNodes != null)
            {
                foreach (XmlNode songNode in songNodes)
                {
                    var song = new Song
                    {
                        Name = songNode.Attributes["name"]?.Value,
                        File = songNode.Attributes["file"]?.Value,
                        IsLooping = bool.Parse(songNode.Attributes["repeat"]?.Value ?? "false")
                    };

                    if (songNode.Attributes["volume"] != null)
                    {
                        song.Volume = float.Parse(songNode.Attributes["volume"].Value);
                    }

                    if (!string.IsNullOrEmpty(song.Name))
                    {
                        _songs[song.Name] = song;
                    }
                }
            }
        }

        public static void Play(string songName)
        {
            if (_songs.TryGetValue(songName, out Song song))
            {
                Stop();
                songReader?.Dispose();
                songPlayer?.Dispose();
                string songPath = Path.Combine(_content.RootDirectory, "Audio", "Song", song.File);
                songReader = new SongReader(songPath);
                songReader.IsLooped = song.IsLooping;
                
                songPlayer = MonoStereo.Song.CreateBuffered(songReader);
                songPlayer.IsLooped = song.IsLooping;
                songPlayer.Play();
            }
        }

        public static void Stop()
        {
            //if (soundPlayer != null)
            //{
            //    soundPlayer.Stop();
            //    soundPlayer.Dispose();
            //    soundPlayer = null;
            //    stream?.Dispose();
            //}
            songPlayer?.Stop();
        }

        public static void Pause()
        {
            songPlayer?.Pause();
        }

        public static void Resume()
        {
            songPlayer?.Resume();
        }

        public static void SetVolume(float volume)
        {
            songPlayer?.Volume = volume;
        }

        public static void Mute()
        {
            if (!isMute)
            {
                previousVolume = songPlayer?.Volume ?? 1.0f;
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

        public static void Dispose()
        {
            Stop();
            _songs?.Clear();
            songPlayer?.Dispose();
            songReader?.Dispose();
        }
    }
}