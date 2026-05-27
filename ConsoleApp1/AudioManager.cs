using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;

namespace SilentTrayRecorder
{
    public class AudioManager : IDisposable
    {
        private WaveInEvent _waveIn;
        private WaveFileWriter? _writer;
        public static int deviceID = 0;

        // Propriedade pública para a interface/bandeja consultar o estado atual
        public bool IsRecording { get; private set; } = false;

        public List<AudioDevice> ListarMicrofones()
        {
            var list = new List<AudioDevice>();

            int deviceCount = WaveIn.DeviceCount;

            for (int i = 0; i < deviceCount; i++)
            {
                WaveInCapabilities waveInDevice = WaveIn.GetCapabilities(i);
                list.Add(new AudioDevice(i,waveInDevice.ProductName));
            }

            return list;
        }

        private void ConfigureRecording()
        {
            //configurar path
            string musicFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            string fileName = $"!gravacao_{DateTime.Now:yyyyMMdd_HHmmss}.wav";
            string fullPath = Path.Combine(musicFolder, fileName);

            // Configura o gravador de áudio
            _waveIn = new WaveInEvent();
            _waveIn.WaveFormat = new WaveFormat(44100, 1); // 44.1kHz, Mono

            // Configura o escritor de arquivos para salvar a gravação
            _writer = new WaveFileWriter(fullPath, _waveIn.WaveFormat);

            // Vincula o recebimento de bytes ao arquivo em disco
            _waveIn.DataAvailable += (sender, eventArgs) =>
            {
                _writer?.Write(eventArgs.Buffer, 0, eventArgs.BytesRecorded);
            };

            // Vincula o encerramento físico à limpeza dos ponteiros
            _waveIn.RecordingStopped += (sender, eventArgs) =>
            {
                CleanupResources();
            };
        }

        public void Start()
        {
            if (IsRecording) return;

            // Configura o gravador e os caminhos de arquivo antes de iniciar
            ConfigureRecording();
            Console.WriteLine("Gravação iniciada... Dispositivo " + AudioManager.deviceID);

            _waveIn?.StartRecording();
            IsRecording = true;
        }

        public void Stop()
        {
            if (!IsRecording) return;

            // Solicita a parada ao hardware. O evento 'RecordingStopped' cuidará do Dispose
            _waveIn?.StopRecording();
            IsRecording = false;
        }

        private void CleanupResources()
        {
            _writer?.Dispose();
            _writer = null;
            _waveIn?.Dispose();
            _waveIn = null;
        }

        public void Dispose()
        {
            // Garante que o arquivo feche se o objeto for destruído inesperadamente
            if (IsRecording)
            {
                Stop();
            }
            else
            {
                CleanupResources();
            }
        }
    }
}
