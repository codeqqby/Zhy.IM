using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zhy.IM.Framework.Tcp
{
    public enum FileCommand
    {
        BeginSendFile_Req = 0,
        BeginSendFile_Resp,
        SendFile_Req,
        SendFile_Resp,
        EndSendFile_Req,
        EndSendFileSuccess_Resp,
        EndSendFileFailed_Resp,
        Close
    }


}
