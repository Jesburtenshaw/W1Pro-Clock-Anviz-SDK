using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockTransactionsTransmiter.Helpers
{
    public static class BytesStringHelper
    {
        public static Encoding Encoding { get; set; } = Encoding.UTF8;

        public static string BytesToIpAddr(byte[] bytes)
        {
            return bytes[0] + "." + bytes[1] + "." + bytes[2] + "." + bytes[3];
        }

        public static byte[] IpAddrToBytes(string ipAddr)
        {
            byte[] bytes = new byte[4];
            var arr = ipAddr.Split('.');
            for (int i = 0; i < 4; i++)
            {
                bytes[i] = Byte.Parse(arr[i]);
            }
            return bytes;
        }

        public static string BytesToMacAddr(byte[] bytes)
        {
            return bytes[0].ToString("X2") + "-" + bytes[1].ToString("X2") + "-" + bytes[2].ToString("X2") + "-" + bytes[3].ToString("X2") + "-" + bytes[4].ToString("X2") + "-" + bytes[5].ToString("X2");
        }

        public static string BytesToUnicodeString(byte[] bytes)
        {
            int i;
            byte[] buff = new byte[bytes.Length];

            for (i = 0; i + 1 < buff.Length; i += 2)
            {
                buff[i] = bytes[i + 1];
                buff[i + 1] = bytes[i];
            }
            return Encoding.Unicode.GetString(buff);
        }

        public static uint SwapInt32(uint value)
        {
            return ((value & 0x000000FF) << 24) |
               ((value & 0x0000FF00) << 8) |
               ((value & 0x00FF0000) >> 8) |
               ((value & 0xFF000000) >> 24);
        }

        public static string GetEmployeeId(byte[] employeeId)
        {
            byte[] temp = new byte[8];
            int i;
            for (i = 0; i < 5; i++)
            {
                temp[8 - 4 - i] = employeeId[i];
            }
            return BitConverter.ToInt64(temp, 0).ToString();
        }
    }
}
