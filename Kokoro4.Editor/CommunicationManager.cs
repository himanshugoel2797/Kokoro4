using Kokoro4.Editor.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

namespace Kokoro.Editor
{
    class CommunicationManager
    {
        public Connection ControlConnection;
        private ConcurrentQueue<string> Commands;

        private bool exitThread;
        private Thread commThread;

        public CommunicationManager()
        {
            ControlConnection = new Connection(true);
            Commands = new ConcurrentQueue<string>();

            exitThread = false;
            commThread = new Thread(CommThread);
            commThread.Start();
        }

        private void CommThread()
        {
            while (!exitThread)
            {
                while (!Commands.IsEmpty && Commands.TryDequeue(out var cmd))
                    ControlConnection.Write(cmd);

                while (Commands.IsEmpty)
                    Thread.Sleep(1);
            }
        }

        public void WriteEchoCMD(string cmd)
        {
            Commands.Enqueue($"CMD:ECHO {cmd}");
        }
    }
}
