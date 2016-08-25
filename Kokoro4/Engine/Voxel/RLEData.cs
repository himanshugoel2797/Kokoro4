using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Voxel
{
    public class RLEData
    {
        public Dictionary<ushort, uint> CodeToValueMap { get; private set; }
        public Dictionary<uint, ushort> ValueToCodeMap { get; private set; }
        private ushort[] Data;
        private int uncompressed_size = 0;

        public ushort[] EncodedData
        {
            get
            {
                return Data;
            }
        }

        public RLEData(ushort[] dat, uint[] v2cMap)
        {
            CodeToValueMap = new Dictionary<ushort, uint>();
            ValueToCodeMap = new Dictionary<uint, ushort>();
            if (dat != null)
            {
                uncompressed_size = dat.Length;
                Data = dat;
            }

            if(v2cMap != null)
            {
                for(int i = 0; i < v2cMap.Length; i += 2)
                {
                    ValueToCodeMap[v2cMap[i]] = (ushort)v2cMap[i + 1];
                    CodeToValueMap[(ushort)v2cMap[i + 1]] = v2cMap[i];
                }
            }
        }

        public uint this[int pos]
        {
            get
            {
                if (pos >= uncompressed_size)
                    throw new IndexOutOfRangeException();

                int p = 0;

                ushort prevVal = 0;
                for(int i = 0; i < Data.Length; i++)
                {
                    p++;

                    if(prevVal == Data[i])
                    {
                        if(p + Data[i + 1] > pos && p < pos)
                        {
                            return CodeToValueMap[Data[i]];
                        }
                        else
                        {
                            p += Data[i + 1];
                        }
                        i++;
                        prevVal = 0;
                        continue;
                    }

                    prevVal = Data[i];
                }

                throw new Exception("Execution shouldn't be here");
            }
            //set
            //{
                //TODO Implement setter, ideas: have a diff dictionary that stores changed values, decode automatically adjusts the output stream, as does get
            //}
        }

        public void Encode(uint[] data)
        {
            //First classify all the data into a table and generate a huffman code map for it
            //Then RLE encode the data based on the huffman code

            CodeToValueMap.Clear();
            ValueToCodeMap.Clear();

            Dictionary<uint, uint> FrequencyMap = new Dictionary<uint, uint>();

            for (int i = 0; i < data.Length; i++)
            {
                if (!FrequencyMap.ContainsKey(data[i]))
                    FrequencyMap[data[i]] = 0;

                FrequencyMap[data[i]]++;
            }

            //Order the map by decending value of counts
            FrequencyMap.OrderByDescending(a => a.Value);

            for (uint i = 0; i < FrequencyMap.Count; i++)
            {
                CodeToValueMap[(ushort)(i + 1)] = FrequencyMap.ElementAt((int)i).Key;
                ValueToCodeMap[CodeToValueMap[(ushort)(i + 1)]] = (ushort)(i + 1);
            }

            List<ushort> mem = new List<ushort>();

            uint prevVal = 0;

            for (int i = 0; i < data.Length; i++)
            {
                //Write the current value to memory as its code
                mem.Add(ValueToCodeMap[data[i]]);

                if (prevVal == data[i])
                {
                    //Count how many entries in a row still match and write them in
                    ushort cnt = 0;
                    while (i + cnt < data.Length && data[i + cnt] == prevVal) cnt++ ;
                    mem.Add((ushort)(cnt));
                    i += cnt;
                    prevVal = 0;
                    continue;
                }

                prevVal = data[i];
            }

            Data = mem.ToArray();
            uncompressed_size = data.Length;
        }

        public uint[] Decode()
        {
            List<uint> outData = new List<uint>();

            uint prevVal = 0;

            for(int i = 0; i < Data.Length; i++)
            {
                outData.Add(CodeToValueMap[Data[i]]);

                if(prevVal == Data[i])
                {
                    for(int j = 0; j < Data[i + 1]; j++)
                    {
                        outData.Add(CodeToValueMap[Data[i]]);
                    }
                    i++;
                    prevVal = 0;
                    continue;
                }

                prevVal = Data[i];
            }

            return outData.ToArray();
        }

        public uint[] GetMapValueToCode()
        {
            uint[] data = new uint[ValueToCodeMap.Count * 2];
            for(int i = 0; i < ValueToCodeMap.Count; i+=2)
            {
                var element = ValueToCodeMap.ElementAt(i / 2);

                data[i] = element.Key;
                data[i + 1] = element.Value;
            }

            return data;
        }
    }
}
