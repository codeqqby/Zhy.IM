using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Zhy.IM.IFramework;

namespace Zhy.IM.Framework.Tcp
{
    public class TcpServer : IListen, IReceive
    {
        private Socket _socket;

        public TcpServer()
        {
            this._socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Listen()
        {
            IPEndPoint ipe = new IPEndPoint(IPAddress.Any, 8010);
            this._socket.Bind(ipe);
            this._socket.Listen(10);

            Receive();
        }

        public void Receive()
        {
            this._socket.BeginAccept(AcceptCallBack, null);
        }

        private void AcceptCallBack(IAsyncResult state)
        {
            Socket socket = this._socket.EndAccept(state);
            Client client = new Client(socket);
            client.StartReceive();
            Receive();
        }
    }
}
