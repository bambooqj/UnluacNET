using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using UnluacNET.IO;

namespace UnluacNET
{
    public class LStringType : BObjectType<LString>
    {
        public override LString Parse(Stream stream, BHeader header)
        {
            var sizeT = header.SizeT.Parse(stream, header);
            var xx = sizeT.AsInteger();
            var sb = new StringBuilder();
            //stream.ReadChars();
            Decoder dec = Encoding.Default.GetDecoder();
            var charData = new char[xx];
            var tempChars = stream.ReadBytes(xx);
            dec.GetChars(tempChars, 0, tempChars.Length, charData, 0);
            String tempSring = new String(charData);
            var str = tempSring.Replace("\0","").Split('?')[0]+"\0";
            if (header.Debug)
                Console.WriteLine("-- parsed <string> \"" + str + "\"");

            return new LString(sizeT, str);
        }

        public static string ASCII2Str(string textAscii)
        {
            try
            {
                int k = 0;//字节移动偏移量

                byte[] buffer = new byte[textAscii.Length / 2];//存储变量的字节

                for (int i = 0; i < textAscii.Length / 2; i++)
                {
                    //每两位合并成为一个字节
                    buffer[i] = byte.Parse(textAscii.Substring(k, 2), System.Globalization.NumberStyles.HexNumber);
                    k = k + 2;
                }
                //将字节转化成汉字
                return Encoding.Default.GetString(buffer);
            }
            catch (Exception ex)
            {

                Console.WriteLine("ASCII转含中文字符串异常" + ex.Message);
            }
            return "";
        }
    }
}
