using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Zhy.IM.IFramework;

namespace Zhy.IM.Framework.Tcp
{
    /// <summary>
    /// TCP客户端，主要用于传输文件
    /// </summary>
    public class TcpClient : IConnect
    {
        private static TcpClient _instance;
        private static readonly object locker = new object();
        private Socket _socket;
        private byte[] _buffer;
        public delegate void PrintSendingFileInfoEventHandler(string fileName, double length, double convertedLength, Capacity flag);
        public PrintSendingFileInfoEventHandler PrintSendingFileInfo;
        public delegate void PrintSendingProgressEventHandler(string fileName, double convertedLength, double currentLength, double convertedCurrentLength, Capacity flag);
        public PrintSendingProgressEventHandler PrintSendingProgress;
        public delegate void PrintSendedResultEventHandler(string result);
        public PrintSendedResultEventHandler PrintSendedResult;

        private TcpClient()
        {
            this._socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this._buffer = new byte[1024];
        }

        public static TcpClient CreateInstance()
        {
            if (_instance == null)
            {
                lock (locker)
                {
                    if (_instance == null)
                    {
                        _instance = new TcpClient();
                    }
                }
            }
            return _instance;
        }

        #region connect
        public bool Connect(IPAddress ip, int port)
        {
            try
            {
                IPEndPoint ipe = new IPEndPoint(ip, port);
                this._socket.Connect(ipe);
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region receive
        private bool Receive(FileCommand command)
        {
            try
            {
                this._socket.Receive(this._buffer);
                this._socket.ReceiveTimeout = 30000;
                return (FileCommand)this._buffer[0] == command;
            }
            catch
            {
                return false;
            }
        }

        private bool ReceiveComplete()
        {
            try
            {
                this._socket.Receive(this._buffer);
                this._socket.ReceiveTimeout = 30000;
                bool result = (FileCommand)this._buffer[0] == FileCommand.EndSendFileSuccess_Resp ? true : false;
                if (this.PrintSendedResult != null)
                {
                    this.PrintSendedResult(result ? "发送成功" : "发送失败");
                }
                return result;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region send
        /// <summary>
        /// 发送文件
        /// </summary>
        /// <param name="fileName"></param>
        public void SendFile(string fileName)
        {
            ThreadPool.QueueUserWorkItem((object o) =>
                {
                    try
                    {
                        FileInfo info = new FileInfo(fileName);
                        long len = info.Length;
                        string name = info.Name;
                        CapacityInfo ci = GetCapacity(len);
                        if (this.PrintSendingFileInfo != null)
                        {
                            this.PrintSendingFileInfo(name, len, ci.Value, ci.Flag);
                        }
                        SendData(name);
                        if (!Receive(FileCommand.BeginSendFile_Resp))
                        {
                            return;
                        }
                        using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            long total = 0;
                            int count = 0;
                            double currentLength = 0;
                            byte[] buffer = new byte[this._buffer.Length - 6];
                            do
                            {
                                count = fs.Read(buffer, 0, buffer.Length);
                                SendData(buffer, count);
                                if (!Receive(FileCommand.SendFile_Resp))
                                {
                                    return;
                                }
                                total += count;
                                if (this.PrintSendingProgress != null)
                                {
                                    currentLength = GetCurrentLength(total, ci.Flag);
                                    this.PrintSendingProgress(name, ci.Value, total, currentLength, ci.Flag);
                                }
                            } while (total < len);
                        }
                        SendData(len);
                        ReceiveComplete();
                    }
                    catch { }
                });
        }

        private void Send(int length)
        {
            this._socket.Send(this._buffer, length, SocketFlags.None);
        }

        /// <summary>
        /// 发送文件的名称
        /// </summary>
        /// <param name="content"></param>
        private void SendData(string content)
        {
            byte[] cnt = Encoding.UTF8.GetBytes(content);
            byte[] len = BitConverter.GetBytes(cnt.Length);
            this._buffer[0] = (byte)FileCommand.BeginSendFile_Req;
            Array.Copy(len, 0, this._buffer, 1, 4);
            Array.Copy(cnt, 0, this._buffer, 5, cnt.Length);
            this._buffer[5 + cnt.Length] = Checkout.CreateInstance().XOR(this._buffer, 5 + cnt.Length);
            Send(6 + cnt.Length);
        }



        /// <summary>
        /// 发送文件的内容
        /// </summary>
        /// <param name="buffer"></param>
        private void SendData(byte[] buffer, int length)
        {
            byte[] len = BitConverter.GetBytes(length);
            this._buffer[0] = (byte)FileCommand.SendFile_Req;
            Array.Copy(len, 0, this._buffer, 1, 4);
            Array.Copy(buffer, 0, this._buffer, 5, length);
            this._buffer[5 + length] = Checkout.CreateInstance().XOR(this._buffer, 5 + length);
            Send(6 + length);
        }

        /// <summary>
        /// 发送文件的结束标记
        /// </summary>
        /// <param name="length"></param>
        private void SendData(long length)
        {
            byte[] len = BitConverter.GetBytes(length);
            this._buffer[0] = (byte)FileCommand.EndSendFile_Req;
            Array.Copy(len, 0, this._buffer, 1, 8);
            this._buffer[9] = Checkout.CreateInstance().XOR(this._buffer, 9);
            Send(10);
        }
        #endregion

        #region 判断文件大小
        const int GB = 1024 * 1024 * 1024;
        const int MB = 1024 * 1024;
        const int KB = 1024;

        /// <summary>
        /// 获取文件大小
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        private CapacityInfo GetCapacity(long length)
        {
            CapacityInfo info = new CapacityInfo();
            if (length / GB >= 1)
            {
                info.Flag = Capacity.GB;
                info.Value = Math.Round(length * 1.0 / GB, 2);
            }
            else if (length / MB >= 1)
            {
                info.Flag = Capacity.MB;
                info.Value = Math.Round(length * 1.0 / MB, 2);
            }
            else if (length / KB >= 1)
            {
                info.Flag = Capacity.KB;
                info.Value = Math.Round(length * 1.0 / KB, 2);
            }
            else
            {
                info.Flag = Capacity.B;
                info.Value = length;
            }
            return info;
        }

        /// <summary>
        /// 获取已发送的文件大小
        /// </summary>
        /// <param name="currentLength"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        private double GetCurrentLength(long currentLength, Capacity flag)
        {
            double result = 0;
            switch (flag)
            {
                case Capacity.GB:
                    result = Math.Round(currentLength * 1.0 / GB, 2);
                    break;
                case Capacity.MB:
                    result = Math.Round(currentLength * 1.0 / MB, 2);
                    break;
                case Capacity.KB:
                    result = Math.Round(currentLength * 1.0 / KB, 2);
                    break;
                default:
                    result = currentLength;
                    break;
            }
            return result;
        }
        #endregion
    }

    class CapacityInfo
    {
        public Capacity Flag { get; set; }
        public double Value { get; set; }
    }

    public enum Capacity
    {
        B,
        KB,
        MB,
        GB
    }
}
