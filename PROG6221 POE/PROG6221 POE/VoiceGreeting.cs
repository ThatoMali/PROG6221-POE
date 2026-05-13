using System;
using System.Media;
using System.Speech.Synthesis;
using System.Threading.Tasks;

namespace PROG6221_POE
{
    public class VoiceService : IDisposable
    {
        private SpeechSynthesizer _synthesizer;
        private SoundPlayer _soundPlayer;

        public VoiceService()
        {
            try
            {
                _synthesizer = new SpeechSynthesizer();
                _synthesizer.SetOutputToDefaultAudioDevice();
                _synthesizer.Rate = 1; // Slightly faster
                _synthesizer.Volume = 80;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Speech synthesis not available: {ex.Message}");
            }
        }

        public void PlayGreeting()
        {
            try
            {
                // Question 1: Try to play WAV file if exists
                string wavPath = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "greeting.wav");

                if (System.IO.File.Exists(wavPath))
                {
                    _soundPlayer = new SoundPlayer(wavPath);
                    _soundPlayer.Play();
                }
                else
                {
                    // Fallback to text-to-speech
                    SpeakResponse("Welcome to SecureCore, your cybersecurity awareness chatbot!");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Voice greeting failed: {ex.Message}");
            }
        }

        public async void SpeakResponse(string text)
        {
            if (_synthesizer == null) return;

            try
            {
                await Task.Run(() => _synthesizer.SpeakAsync(text));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Speech failed: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _synthesizer?.Dispose();
            _soundPlayer?.Dispose();
        }
    }
}