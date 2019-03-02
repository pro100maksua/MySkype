using System;
using NAudio.Wave;

namespace MySkype.WpfClient.Services
{
    public class CallService
    {
        private readonly WebSocketClient _webSocketClient;
        private readonly Guid _friendId; 

        private WaveIn _input = new WaveIn { WaveFormat = new WaveFormat(8000, 16, 1), BufferMilliseconds = 100 };
        private BufferedWaveProvider _bufferStream = new BufferedWaveProvider(new WaveFormat(8000, 16, 1));
        private readonly Codec _codec = new Codec();
        private WaveOut _output = new WaveOut();

        public CallService(WebSocketClient webSocketClient, Guid friendId)
        {
            _webSocketClient = webSocketClient;
            _friendId = friendId;

            _webSocketClient.DataReceived += OnDataReceived;
            _bufferStream.DiscardOnBufferOverflow = true;
            _input.DataAvailable += OnDataAvailable;

            _output.Init(_bufferStream);
        }

        private void OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            var decoded = _codec.Decode(e.Data, 0, e.Data.Length);

            _bufferStream.AddSamples(decoded, 0, decoded.Length);
        }

        private async void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            var encoded = _codec.Encode(e.Buffer, 0, e.BytesRecorded);

            await _webSocketClient.SendDataAsync(_friendId, encoded);
        }

        public void PauseRecording()
        {
            _input.StopRecording();
        }

        public void ContinueRecording()
        {
            _input.StartRecording();
        }

        public void PausePlaying()
        {
            _output.Pause();
        }

        public void ContinuePlaying()
        {
            _bufferStream.ClearBuffer();

            _output.Play();
        }

        public void StartCall()
        {
            _input.StartRecording();

            _output.Play();
        }

        public void StopCall()
        {
            _webSocketClient.DataReceived -= OnDataReceived;

            _output?.Stop();
            _output?.Dispose();
            _input?.StopRecording();
            _input?.Dispose();

            _input = null;
            _output = null;
            _bufferStream = null;
        }
    }
}
