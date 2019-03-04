using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using NAudio.Wave;

namespace MySkype.WpfClient.Services
{
    public class CallService
    {
        private readonly WebSocketClient _webSocketClient;
        private List<KeyValuePair<Guid, Player>> _players = new List<KeyValuePair<Guid, Player>>();
        private WaveIn _input = new WaveIn {WaveFormat = new WaveFormat(8000, 16, 1), BufferMilliseconds = 50};
        private readonly Codec _codec = new Codec();


        public CallService(WebSocketClient webSocketClient)
        {
            _webSocketClient = webSocketClient;
            _webSocketClient.DataReceived += OnDataReceived;
            _input.DataAvailable += OnDataAvailable;
        }

        private void OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            var decoded = _codec.Decode(e.Data, 0, e.Data.Length);

            Application.Current.Dispatcher.Invoke(() =>
            {
                var player = _players.SingleOrDefault(p => p.Key == e.SenderId);
                if (player.Key == Guid.Empty)
                {
                    _players.Add(new KeyValuePair<Guid, Player>(e.SenderId, new Player(decoded)));
                }
                else
                {
                    var index = _players.IndexOf(player);
                    _players[index].Value.AddSamples(decoded);
                }
            });
        }

        private async void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            var encoded = _codec.Encode(e.Buffer, 0, e.BytesRecorded);

            await _webSocketClient.SendDataAsync(encoded);
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
            foreach (var pair in _players)
            {
                pair.Value.Output.Pause();
            }
        }

        public void ContinuePlaying()
        {
            foreach (var pair in _players)
            {
                pair.Value.BufferStream.ClearBuffer();
                pair.Value.Output.Play();
            }
        }

        public void StartCall()
        {
            _input.StartRecording();
        }

        public void StopCall()
        {
            _webSocketClient.DataReceived -= OnDataReceived;

            foreach (var pair in _players)
            {
                pair.Value.Output?.Stop();
                pair.Value.Output?.Dispose();
            }

            _input?.StopRecording();
            _input?.Dispose();

            _input = null;
            _players = new List<KeyValuePair<Guid, Player>>();
        }
    }

    public class Player
    {
        public WaveOut Output { get; } = new WaveOut();

        public BufferedWaveProvider BufferStream { get; } = new BufferedWaveProvider(new WaveFormat(8000, 16, 1));

        public Player()
        {
            BufferStream.DiscardOnBufferOverflow = true;
            Output.Init(BufferStream);
            Output.Play();
        }
        public Player(byte[] sample) : this()
        {
            AddSamples(sample);
        }

        public void AddSamples(byte[] data)
        {
            BufferStream.AddSamples(data, 0, data.Length);
        }
    }
}
