using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro4.Editor.Communication
{
    public class Connection
    {

        private bool isViewport;

        #region Communication Management
        NamedPipeClientStream Client;
        NamedPipeServerStream Server;

        StreamReader ClientReader;
        StreamWriter ServerWriter;

        public const string ViewportToControls = "Kokoro4_VP_2_CTRLS";
        public const string ControlsToViewport = "Kokoro4_CTRLS_2_VP";

        public bool HasData()
        {
            return ClientReader.EndOfStream;
        }

        public string Read()
        {
            return ClientReader.ReadLine();
        }

        public void Write(string cmd)
        {
            ServerWriter.WriteLine(cmd);
        }

        #endregion

        public Connection(bool isViewport)
        {
            this.isViewport = isViewport;

            PipeSecurity pipeSecurity = new PipeSecurity();
            pipeSecurity.AddAccessRule(new PipeAccessRule("Users", PipeAccessRights.Read, System.Security.AccessControl.AccessControlType.Allow));

            if (isViewport)
            {
                //Is Viewport
                Client = new NamedPipeClientStream(".", ControlsToViewport, PipeDirection.In, PipeOptions.Asynchronous);
                Client.Connect();
                Server = new NamedPipeServerStream(ViewportToControls, PipeDirection.Out, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous, 4096, 4096, pipeSecurity);
                Server.WaitForConnection();
            }
            else
            {
                //Is Controls
                Server = new NamedPipeServerStream(ControlsToViewport, PipeDirection.Out, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous, 4096, 4096, pipeSecurity);
                Server.WaitForConnection();

                Client = new NamedPipeClientStream(".", ViewportToControls, PipeDirection.In, PipeOptions.Asynchronous);
                Client.Connect();
            }

            ServerWriter = new StreamWriter(Server);
            ClientReader = new StreamReader(Client);
        }

    }
}
