using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Zhy.IM.Framework.Tcp
{
    /// <summary>
    /// 处理来自客户端的数据，主要用于文件传输
    /// </summary>
    class Client
    {
        private Socket _socket;
        private byte[] _buffer;

        public Client(Socket socket)
        {
            this._socket = socket;
            this._buffer = new byte[1024];
        }

        /// <summary>
        /// 接收来自客户端的数据
        /// </summary>
        public void StartReceive()
        {
            int count = 0;
            string fileName = string.Empty;
            bool closed = false;
            ThreadPool.QueueUserWorkItem((object o) =>
                {
                    while (true)
                    {
                        try
                        {
                            count = this._socket.Receive(this._buffer);
                            switch ((FileCommand)this._buffer[0])
                            {
                                case FileCommand.BeginSendFile_Req:
                                    fileName = CreateFile();
                                    break;
                                case FileCommand.SendFile_Req:
                                    ReceiveFile(fileName);
                                    break;
                                case FileCommand.EndSendFile_Req:
                                    ReceiveComplete(fileName);
                                    break;
                                case FileCommand.Close:
                                    Close();
                                    closed = true;
                                    break;
                                default:
                                    break;
                            }
                            if (closed)
                            {
                                break;
                            }
                        }
                        catch
                        {
                            Close();
                            break;
                        }
                    }
                });
        }

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

        /// <summary>
        /// 新建文件
        /// </summary>
        /// <returns></returns>
        private string CreateFile()
        {
            byte[] buffer = new byte[4];
            Array.Copy(this._buffer, 1, buffer, 0, buffer.Length);
            int len = BitConverter.ToInt32(buffer, 0);
            if (this._buffer[5 + len] != Checkout.CreateInstance().XOR(this._buffer, 5 + len))
            {
                //发送指令让客户端重发
                return string.Empty;
            }
            buffer = new byte[len];
            Array.Copy(this._buffer, 5, buffer, 0, buffer.Length);
            string fileName = Encoding.UTF8.GetString(buffer);
            if (!File.Exists(fileName))
            {
                File.Create(fileName).Close();
            }
            SendData(FileCommand.BeginSendFile_Resp);
            return fileName;
        }

        /// <summary>
        /// 接收文件
        /// </summary>
        /// <param name="fileName"></param>
        private void ReceiveFile(string fileName)
        {
            byte[] buffer = new byte[4];
            Array.Copy(this._buffer, 1, buffer, 0, buffer.Length);
            int len = BitConverter.ToInt32(buffer, 0);
            if (this._buffer[5 + len] != Checkout.CreateInstance().XOR(this._buffer, 5 + len))
            {
                //发送指令让客户端重发
            }
            buffer = new byte[len];
            Array.Copy(this._buffer, 5, buffer, 0, buffer.Length);
            using (FileStream fs = new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                fs.Write(buffer, 0, buffer.Length);
            }
            SendData(FileCommand.SendFile_Resp);
        }

        /// <summary>
        /// 文件接收完成
        /// </summary>
        /// <param name="fileName"></param>
        private void ReceiveComplete(string fileName)
        {
            byte[] buffer = new byte[8];
            Array.Copy(this._buffer, 1, buffer, 0, buffer.Length);
            long len = BitConverter.ToInt64(buffer, 0);
            if (this._buffer[9] != Checkout.CreateInstance().XOR(this._buffer, 9))
            {
                //发送指令让客户端重发
            }
            FileInfo info = new FileInfo(fileName);
            this._buffer[0] = len == info.Length ? (byte)FileCommand.EndSendFileSuccess_Resp : (byte)FileCommand.EndSendFileFailed_Resp;
            this._socket.Send(this._buffer, 1, SocketFlags.None);
        }

        /// <summary>
        /// 发送响应的指令
        /// </summary>
        /// <param name="length"></param>
        private void SendData(FileCommand command)
        {
            this._buffer[0] = (byte)command;
            this._socket.Send(this._buffer, 1, SocketFlags.None);
        }
    }
}
