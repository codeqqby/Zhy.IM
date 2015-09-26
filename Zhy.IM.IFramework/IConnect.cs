using System.Net;

namespace Zhy.IM.IFramework
{
    public interface IConnect
    {
        bool Connect(IPAddress ip, int port);
    }
}
