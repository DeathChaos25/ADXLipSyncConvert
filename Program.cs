using System.Globalization;

namespace ADXLipSyncConvert
{
    struct FILE
    {
        public HEADER header;
        public List<RECORD> records;

        public struct HEADER
        {
            public int count;
        }

        public struct RECORD
        {
            public int Msec;
            public float width;
            public float height;
            public float tongue;
            public float a;
            public float i;
            public float u;
            public float e;
            public float o;
            public float vol;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("ADXLipSyncConvert\nUsage: Drag and Drop a .csv, .txt or .adxlip output file from ADX LipSync program to convert to a binary file.\nThe binary file format targets Fire Emblem: Engage for the Nintendo Switch family of systems.\n\nPress Any Key to exit...");
                Console.ReadKey();
                return;
            }

            string inputFile = args[0];
            string fileExtension = Path.GetExtension(inputFile).ToLower();

            if (fileExtension == ".csv" || fileExtension == ".txt" || fileExtension == ".adxlip")
            {
                string outputBin = Path.ChangeExtension(inputFile, ".bin");
                FILE fileData = CsvToBinary(inputFile);
                WriteBinary(outputBin, fileData);
            }
            else if (fileExtension == ".bin")
            {
                string outputCsv = Path.ChangeExtension(inputFile, ".csv");
                FILE fileData = ReadBinary(inputFile);
                BinaryToCsv(fileData, outputCsv);
            }
            else
            {
                Console.WriteLine("https://www.youtube.com/watch?v=lMEt3RdqB9Y");
            }
        }

        static FILE CsvToBinary(string csvFile)
        {
            var lines = File.ReadAllLines(csvFile);
            var records = new List<FILE.RECORD>();

            int totalEntries = 0;

            for (int j = 0; j < lines.Length; j++)
            {
                if (lines[j].StartsWith("//")) continue; // skip header comments

                totalEntries += 1;

                var values = lines[j].Split(',').Select(v => v.Trim()).ToArray();
                records.Add(new FILE.RECORD
                {
                    Msec = int.Parse(values[1]), // Skip frame count (index 0)
                    width = float.Parse(values[2], CultureInfo.InvariantCulture),
                    height = float.Parse(values[3], CultureInfo.InvariantCulture),
                    tongue = float.Parse(values[4], CultureInfo.InvariantCulture),
                    a = float.Parse(values[5], CultureInfo.InvariantCulture),
                    i = float.Parse(values[6], CultureInfo.InvariantCulture),
                    u = float.Parse(values[7], CultureInfo.InvariantCulture),
                    e = float.Parse(values[8], CultureInfo.InvariantCulture),
                    o = float.Parse(values[9], CultureInfo.InvariantCulture),
                    vol = float.Parse(values[10], CultureInfo.InvariantCulture),
                });
            }

            return new FILE
            {
                header = new FILE.HEADER { count = totalEntries },
                records = records
            };
        }


        static void WriteBinary(string filePath, FILE fileData)
        {
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(fs))
            {
                writer.Write(fileData.header.count);

                foreach (var data in fileData.records)
                {
                    writer.Write(data.Msec);
                    writer.Write(data.width);
                    writer.Write(data.height);
                    writer.Write(data.tongue);
                    writer.Write(data.a);
                    writer.Write(data.i);
                    writer.Write(data.u);
                    writer.Write(data.e);
                    writer.Write(data.o);
                    writer.Write(data.vol);
                }
            }
        }

        static FILE ReadBinary(string filePath)
        {
            FILE fileData;

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(fs))
            {
                fileData.header = new FILE.HEADER { count = reader.ReadInt32() };
                fileData.records = new List<FILE.RECORD>();

                for (int k = 0; k < fileData.header.count; k++)
                {
                    fileData.records.Add(new FILE.RECORD
                    {
                        Msec = reader.ReadInt32(),
                        width = reader.ReadSingle(),
                        height = reader.ReadSingle(),
                        tongue = reader.ReadSingle(),
                        a = reader.ReadSingle(),
                        i = reader.ReadSingle(),
                        u = reader.ReadSingle(),
                        e = reader.ReadSingle(),
                        o = reader.ReadSingle(),
                        vol = reader.ReadSingle()
                    });
                }
            }

            return fileData;
        }

        static void BinaryToCsv(FILE fileData, string csvFile)
        {
            using (var writer = new StreamWriter(csvFile))
            {
                writer.WriteLine($"// input: {Path.GetFileNameWithoutExtension(csvFile)}.wav");
                writer.WriteLine("// framerate: 60 [fps] (assumed)");
                writer.WriteLine("// frame count, msec, width(0-1 def=0.583), height(0-1 def=0.000), tongue(0-1 def=0.000), A(0-1), I(0-1), U(0-1), E(0-1), O(0-1), Vol(dB)");

                for (int frameCount = 0; frameCount < fileData.records.Count; frameCount++)
                {
                    var record = fileData.records[frameCount];
                    writer.WriteLine($"{frameCount},{record.Msec},{record.width},{record.height},{record.tongue},{record.a},{record.i},{record.u},{record.e},{record.o},{record.vol}");
                }
            }
        }

    }

}
