using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace LudumDare57Web.Managers
{
    internal class AudioManager
    {
        private readonly SoundEffect _theme;
        private readonly SoundEffectInstance _themeInstance;
        public AudioManager(ContentManager Content)
        {
            SoundEffect.MasterVolume = .25f;

            _theme = Content.Load<SoundEffect>("Audio/Theme");
            _themeInstance = _theme.CreateInstance();
            _themeInstance.Volume = .05f;
            _themeInstance.IsLooped = true;
        }

        public void Stop()
        {
            _themeInstance?.Stop();
        }

        public bool IsPlaying()
        {
            return _themeInstance.State == SoundState.Playing;
        }

        public void Start()
        {
            _themeInstance?.Play();
        }
    }
}