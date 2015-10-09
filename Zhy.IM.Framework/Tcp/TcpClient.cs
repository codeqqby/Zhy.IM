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

        private bool ReceiveComplete(TcpFile file)
        {
            try
            {
                this._socket.Receive(this._buffer);
                this._socket.ReceiveTimeout = 30000;
                bool result = (FileCommand)this._buffer[0] == FileCommand.EndSendFileSuccess_Resp ? true : false;
                file.ProgressText = result ? "发送成功" : "发送失败";
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
        public void SendFile(TcpFile file)
        {
            ThreadPool.QueueUserWorkItem((object o) =>
            {
                try
                {
                    FileInfo info = new FileInfo(file.FileName);
                    long len = info.Length;
                    file.FileName = System.IO.Path.GetFileName(info.Name);
                    FileCapacityInfo fileCapacity = GetFileCapacityInfo(len);
                    file.ProgressMaximum=len;
                    file.ProgressText = string.Format("({0}{1})", fileCapacity.Value.ToString("F2"), fileCapacity.Type.ToString());
                    SendData(file.FileName);
                    if (!Receive(FileCommand.BeginSendFile_Resp))
                    {
                        return;
                    }

                    byte[] name = Encoding.UTF8.GetBytes(file.FileName);
                    byte[] nameLength = BitConverter.GetBytes(name.Length);
                    using (FileStream fs = new FileStream(info.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        long total = 0;
                        int count = 0;
                        double currentLength = 0;
                        byte[] buffer = new byte[this._buffer.Length - 10 - name.Length];
                        do
                        {
                            count = fs.Read(buffer, 0, buffer.Length);
                            SendData(file.FileName,buffer, count);
                            if (!Receive(FileCommand.SendFile_Resp))
                            {
                                Console.WriteLine("aaaabbb");
                                return;
                            }
                            total += count;
                            currentLength = GetCurrentLength(total, fileCapacity.Type);
                            file.ProgressText = string.Format("({0}/{1}{2})", fileCapacity.Value.ToString("F2"), currentLength.ToString("F2"), fileCapacity.Type.ToString());
                            file.ProgressValue = total;
                        } while (total < len);
                    }
                    SendData(len);
                    ReceiveComplete(file);
                }
                catch(Exception ex) 
                {
                    Console.WriteLine(ex.Message);
                }
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
        private void SendData( string fileName, byte[] buffer, int length)
        {
            this._buffer[0] = (byte)FileCommand.SendFile_Req;

            byte[] name = Encoding.UTF8.GetBytes(fileName);
            byte[] nameLength = BitConverter.GetBytes(name.Length);
            Array.Copy(nameLength, 0, this._buffer, 1, 4);
            Array.Copy(name, 0, this._buffer, 5, name.Length);

            byte[] len = BitConverter.GetBytes(length);
            Array.Copy(len, 0, this._buffer, 5 + name.Length, 4);
            Array.Copy(buffer, 0, this._buffer, 9 + name.Length, length);
            this._buffer[9 + name.Length + length] = Checkout.CreateInstance().XOR(this._buffer, 9 + name.Length + length);
            Send(10 + name.Length + length);
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
        private FileCapacityInfo GetFileCapacityInfo(long length)
        {
            FileCapacityInfo file = new FileCapacityInfo();
            if (length / GB >= 1)
            {
                file.Type = FileCapacityType.GB;
                file.Value = Math.Round(length * 1.0 / GB, 2);
            }
            else if (length / MB >= 1)
            {
                file.Type = FileCapacityType.MB;
                file.Value = Math.Round(length * 1.0 / MB, 2);
            }
            else if (length / KB >= 1)
            {
                file.Type = FileCapacityType.KB;
                file.Value = Math.Round(length * 1.0 / KB, 2);
            }
            else
            {
                file.Type = FileCapacityType.B;
                file.Value = length;
            }
            return file;
        }

        /// <summary>
        /// 获取已发送的文件大小
        /// </summary>
        /// <param name="currentLength"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        private double GetCurrentLength(long currentLength, FileCapacityType type)
        {
            double result = 0;
            switch (type)
            {
                case FileCapacityType.GB:
                    result = Math.Round(currentLength * 1.0 / GB, 2);
                    break;
                case FileCapacityType.MB:
                    result = Math.Round(currentLength * 1.0 / MB, 2);
                    break;
                case FileCapacityType.KB:
                    result = Math.Round(currentLength * 1.0 / KB, 2);
                    break;
                default:
                    result = currentLength;
                    break;
            }
            return result;
        }
        #endregion

        /// <summary>
        /// 关闭连接
        /// </summary>
        private void Close()
        {
            try
            {
                this._socket.Shutdown(SocketShutdown.Both);
                this._socket.Close();
            }
            catch { }
        }
    }

    class FileCapacityInfo
    {
        public FileCapacityType Type { get; set; }
        public double Value { get; set; }
    }

    public enum FileCapacityType
    {
        B,
        KB,
        MB,
        GB
    }
}
