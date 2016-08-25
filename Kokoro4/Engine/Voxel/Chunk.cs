using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Voxel
{
    public class Chunk
    {
        public RLEData[] Data;
        public const int Side = 250;

        public Chunk()
        {
            Data = new RLEData[Side];
        }

        public void LoadFromFile(string file)
        {
            //Parse chunk data from the provided file and load it into the RLEData 

        }

        public void SaveToFile(string file)
        {
            FileStream f = File.OpenWrite(file);
            for (int i = 0; i < Data.Length; i++)
            {
                if (Data[i] == null) continue;

                //Write the buffer index
                f.Write(BitConverter.GetBytes(i), 0, sizeof(int));

                //Write out the encoded data length
                f.Write(BitConverter.GetBytes(Data[i].EncodedData.Length), 0, sizeof(int));

                //Write out the encoded data
                for (int j = 0; j < Data[i].EncodedData.Length; j++)
                {
                    f.Write(BitConverter.GetBytes(Data[i].EncodedData[j]), 0, sizeof(ushort));
                }

                //Obtain the value to code map
                var map = Data[i].GetMapValueToCode();

                //Write out the value to code map length
                f.Write(BitConverter.GetBytes(map.Length), 0, sizeof(int));

                //Write out the value to code map
                for(int j = 0; j < map.Length; j++)
                {
                    f.Write(BitConverter.GetBytes(map[j]), 0, sizeof(uint));
                }
            }

            f.Flush();
            f.Close();
        }
    }
}
