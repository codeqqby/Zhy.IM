using System;
using System.IO;
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
        private byte[] _tempBuffer;
        private object _lock = new object();
        private int _offset = 0;

        public Client(Socket socket)
        {
            this._socket = socket;
            this._buffer = new byte[1024 * 1024 * 20];
            this._tempBuffer = new byte[1024 * 1024 * 20];
        }

        public void StartReceiveData()
        {
            ReceiveData();
            HandleData();
        }

        private void ReceiveData()
        {
            byte[] buffer = new byte[1024];
            int len = 0;
            Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        if (_offset < 0)
                        {
                            break;
                        }
                        try
                        {
                            len = this._socket.Receive(buffer);
                            lock (this._lock)
                            {
                                Array.Copy(buffer, 0, this._buffer, this._offset, len);
                                this._offset += len;
                            }
                        }
                        catch { }
                    }
                });
        }

        /// <summary>
        /// 接收来自客户端的数据
        /// </summary>
        private void HandleData()
        {
            byte[] buffer = new byte[1024];
            string fileName = string.Empty;
            bool closed = false;
            Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        try
                        {
                            lock (this._lock)
                            {
                                if (_offset == 0)
                                {
                                    Thread.Sleep(200);
                                    continue;
                                }
                                if (this._offset < buffer.Length)
                                {
                                    Array.Copy(this._buffer, 0, buffer, 0, this._offset);
                                    Array.Copy(this._buffer, this._offset, this._tempBuffer, 0, this._buffer.Length - this._offset);
                                    this._tempBuffer.CopyTo(this._buffer, 0);
                                    this._offset = 0;
                                }
                                else
                                {
                                    Array.Copy(this._buffer, 0, buffer, 0, buffer.Length);
                                    Array.Copy(this._buffer, buffer.Length, this._tempBuffer, 0, this._buffer.Length - buffer.Length);
                                    this._tempBuffer.CopyTo(this._buffer, 0);
                                    this._offset -= buffer.Length;
                                }
                                if (this._offset < 0)
                                {
                                    Console.WriteLine("int");
                                    break;
                                }
                            }
                            switch ((FileCommand)buffer[0])
                            {
                                case FileCommand.BeginSendFile_Req:
                                    fileName = CreateFile(buffer);
                                    break;
                                case FileCommand.SendFile_Req:
                                    ReceiveFile(buffer, fileName);
                                    break;
                                case FileCommand.EndSendFile_Req:
                                    ReceiveComplete(buffer, fileName);
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
        private string CreateFile(byte[] buffer)
        {
            byte[] tempBuffer = new byte[4];
            Array.Copy(buffer, 1, tempBuffer, 0, tempBuffer.Length);
            int len = BitConverter.ToInt32(tempBuffer, 0);
            if (buffer[5 + len] != Checkout.CreateInstance().XOR(buffer, 5 + len))
            {
                //发送指令让客户端重发
                return string.Empty;
            }
            string fileName = Encoding.UTF8.GetString(buffer, 5, len);
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
        private void ReceiveFile(byte[] buffer, string fname)
        {
            byte[] tempBuffer = new byte[4];
            Array.Copy(buffer, 1, tempBuffer, 0, tempBuffer.Length);
            int nameLen = BitConverter.ToInt32(tempBuffer, 0);
            Array.Copy(buffer, 5 + nameLen, tempBuffer, 0, tempBuffer.Length);
            int contentLen = BitConverter.ToInt32(tempBuffer, 0);
            if (buffer[9 + nameLen + contentLen] != Checkout.CreateInstance().XOR(buffer, 9 + nameLen + contentLen))
            {
                Console.WriteLine("aaaaaaa");
                //发送指令让客户端重发
            }
            string fileName = Encoding.UTF8.GetString(buffer, 5, nameLen);
            using (FileStream fs = new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                fs.Write(buffer, 9 + nameLen, contentLen);
            }
            SendData(FileCommand.SendFile_Resp);
        }

        /// <summary>
        /// 文件接收完成
        /// </summary>
        /// <param name="fileName"></param>
        private void ReceiveComplete(byte[] buffer,string fileName)
        {
            byte[] tempBuffer = new byte[8];
            Array.Copy(buffer, 1, tempBuffer, 0, tempBuffer.Length);
            long len = BitConverter.ToInt64(tempBuffer, 0);
            if (buffer[9] != Checkout.CreateInstance().XOR(buffer, 9))
            {
                //发送指令让客户端重发
            }
            FileInfo info = new FileInfo(fileName);
            buffer[0] = len == info.Length ? (byte)FileCommand.EndSendFileSuccess_Resp : (byte)FileCommand.EndSendFileFailed_Resp;
            this._socket.Send(buffer, 1, SocketFlags.None);
        }

        /// <summary>
        /// 发送响应的指令
        /// </summary>
        /// <param name="length"></param>
        private void SendData(FileCommand command)
        {
            this._socket.Send(new byte[] { (byte)command }, 1, SocketFlags.None);
        }
    }
}
