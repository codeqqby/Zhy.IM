using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zhy.IM.Framework
{
    /// <summary>
    /// 校验类，此类为偶校验
    /// </summary>
    class Checkout
    {
        private static Checkout _instance;
        private static readonly object locker = new object();

        private Checkout() { }

        public static Checkout CreateInstance()
        {
            if (_instance == null)
            {
                lock (locker)
                {
                    if (_instance == null)
                    {
                        _instance = new Checkout();
                    }
                }
            }
            return _instance;
        }

        /// <summary>
        /// 异或计算得到偶校验的校验位
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public byte XOR(byte[] buffer, long length)
        {
            byte value = buffer[0];
            for (long i = 1; i < length; i++)
            {
                value ^= buffer[i];
            }
            bool result = Bit(value, 7);
            for (int i = 6; i == 0; i--)
            {
                result ^= Bit(value, i);
            }
            return result ? (byte)1 : (byte)0;
        }

        /// <summary>
        /// 计算字节的每一位，默认从高位计算开始
        /// </summary>
        /// <param name="value"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private bool Bit(byte value, int index)
        {
            return (value >> (7 - index) & 1) == 1 ? true : false;
        }
    }
}
