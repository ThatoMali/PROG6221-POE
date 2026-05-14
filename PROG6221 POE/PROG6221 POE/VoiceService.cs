using System;
using System.Media;
using System.IO;

namespace PROG6221_POE
{
    public class VoiceService : IDisposable
    {
        private SoundPlayer _soundPlayer;

        public void PlayGreeting()
        {
            try
            {
                string wavPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "greeting.wav");
                if (File.Exists(wavPath))
                {
                    _soundPlayer = new SoundPlayer(wavPath);
                    _soundPlayer.Play();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Voice greeting failed: {ex.Message}");
            }
        }

        public void SpeakResponse(string text)
        {
            // Text-to-speech would go here
            System.Diagnostics.Debug.WriteLine($"Voice would say: {text}");
        }

        public void Dispose()
        {
            _soundPlayer?.Dispose();
        }
    }
}