using System;
using System.Linq;
using MySkype.WpfClient.Models;
using NAudio.Wave;
using WebSocket4Net;

namespace MySkype.WpfClient.Services
{
    public class CallService
    {
        private readonly WebSocketClient _webSocketClient;
        private readonly RestSharpClient _restClient;
        private readonly Guid _friendId;
        private WaveIn _input = new WaveIn { WaveFormat = new WaveFormat(8000, 16, 1), BufferMilliseconds = 50 };
        private BufferedWaveProvider _bufferStream = new BufferedWaveProvider(new WaveFormat(8000, 16, 1));
        private WaveOut _output = new WaveOut();

        public CallService(WebSocketClient webSocketClient, RestSharpClient restClient, Guid friendId)
        {
            _webSocketClient = webSocketClient;
            _restClient = restClient;
            _friendId = friendId;

            _webSocketClient.DataReceived += OnDataReceived;
            _bufferStream.DiscardOnBufferOverflow = true;
            _input.DataAvailable += OnDataAvailable;

            _output.Init(_bufferStream);
        }

        private void OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            _bufferStream.AddSamples(e.Data, 0, e.Data.Length);
        }

        private async void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            var data = e.Buffer.Take(e.BytesRecorded).ToArray();

            await _restClient.SendDataAsync(_friendId, data);
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
