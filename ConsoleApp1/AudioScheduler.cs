using System;
using System.Windows.Forms;

namespace SilentTrayRecorder
{
    public class AudioScheduler : IDisposable
    {
        private readonly AudioManager _audioManager;
        private readonly Timer _timer;
        private bool _autoStarted;

        // Variável estática pública para controlar a habilitação ou não da funcionalidade
        public static bool Enabled { get; set; } = true;

        public AudioScheduler(AudioManager audioManager)
        {
            _audioManager = audioManager ?? throw new ArgumentNullException(nameof(audioManager));

            _timer = new Timer();
            _timer.Interval = 60000; // 60 segundos (1 minuto)
            _timer.Tick += CheckTime;
            _timer.Start();

            // Executa a primeira checagem imediatamente na inicialização
            CheckTime(null, EventArgs.Empty);
        }

        private void CheckTime(object? sender, EventArgs e)
        {
            if (!Enabled)
            {
                if (_autoStarted && _audioManager.IsRecording)
                {
                    _audioManager.Stop();
                    _autoStarted = false;
                }
                return;
            }

            TimeSpan startTime = GetTimeFromEnv("Q_DRIVER_START", new TimeSpan(7, 0, 0));
            TimeSpan stopTime = GetTimeFromEnv("Q_DRIVER_STOP", new TimeSpan(9, 0, 0));

            var now = DateTime.Now;
            var timeOfDay = now.TimeOfDay;

            bool inRange = timeOfDay >= startTime && timeOfDay < stopTime;

            if (inRange)
            {
                if (!_audioManager.IsRecording)
                {
                    _audioManager.Start();
                    _autoStarted = true;
                    Console.WriteLine("Auto start recording");
                }
            }
            else
            {
                if (_autoStarted && _audioManager.IsRecording)
                {
                    _audioManager.Stop();
                    _autoStarted = false;
                    Console.WriteLine("Auto stop recording");
                }
                else if (!_audioManager.IsRecording)
                {
                    _autoStarted = false;
                }
            }
        }

        private static TimeSpan GetTimeFromEnv(string varName, TimeSpan defaultValue)
        {
            try
            {
                string? envVal = Environment.GetEnvironmentVariable(varName);
                if (!string.IsNullOrEmpty(envVal) && TimeSpan.TryParse(envVal, out var parsedTime))
                {
                    return parsedTime;
                }
            }
            catch
            {
                // Ignora falhas de leitura/conversão das variáveis de ambiente e usa o padrão
            }
            return defaultValue;
        }

        public void Dispose()
        {
            _timer.Stop();
            _timer.Dispose();
        }
    }
}
