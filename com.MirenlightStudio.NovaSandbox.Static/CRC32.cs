using System.Text;

namespace com.MirenlightStudio.NovaSandbox.Static
{
    public static class CRC32
    {
        private static readonly uint[] _table = InitializeTable();

        private static uint[] InitializeTable()
        {
            uint[] table = new uint[256];
            for (uint i = 0; i < 256; i++)
            {
                uint crc = i;
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 1) != 0)
                    {
                        crc = (crc >> 1) ^ 0xEDB88320;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
                table[i] = crc;
            }
            return table;
        }

        public static uint Compute(byte[] buffer)
        {
            uint crc = 0xFFFFFFFF;
            foreach (byte b in buffer)
            {
                crc = (crc >> 8) ^ _table[(crc ^ b) & 0xFF];
            }
            return ~crc;
        }

        public static uint ComputeString(string input)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            return Compute(bytes);
        }
    }
}
