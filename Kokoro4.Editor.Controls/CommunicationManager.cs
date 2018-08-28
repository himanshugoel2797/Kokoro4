using Kokoro4.Editor.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Editor.Controls
{
    class CommunicationManager
    {

        public static Connection ControlConnection;

        public CommunicationManager()
        {
            ControlConnection = new Connection(false);
        }

        public void ReadTest()
        {
            Console.WriteLine(ControlConnection.Read());
        }
    }
}
