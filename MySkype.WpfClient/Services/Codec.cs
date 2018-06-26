using System;
using NAudio.Codecs;

namespace MySkype.WpfClient.Services
{
    public class Codec
    {
        public byte[] Encode(byte[] data, int offset, int length)
        {
            var encoded = new byte[length / 2];
            var outIndex = 0;
            for (var i = 0; i < length; i += 2)
            {
                encoded[outIndex++] = ALawEncoder.LinearToALawSample(BitConverter.ToInt16(data, offset + i));
            }
            return encoded;
        }

        public byte[] Decode(byte[] data, int offset, int length)
        {
            var decoded = new byte[length * 2];
            var outIndex = 0;
            for (var i = 0; i < length; i++)
            {
                var decodedSample = ALawDecoder.ALawToLinearSample(data[i + offset]);
                decoded[outIndex++] = (byte)(decodedSample & 0xFF);
                decoded[outIndex++] = (byte)(decodedSample >> 8);
            }
            return decoded;
        }
    }
}
