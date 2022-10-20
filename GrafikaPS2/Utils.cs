//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace GrafikaPS2
//{
//    public static class Utils
//    {
//        public static bool IsRightFormat(StreamReader sr, params string[] formats)
//        {
//            var magicNumber = new char[2];
//            sr.Read(magicNumber, 0, 2);
//            var magicNumberStr = $"{magicNumber[0]}{magicNumber[1]}";

//            foreach (var item in formats)
//            {
//                if (magicNumberStr == item)
//                {
//                    return true;
//                }
//            }
//            return false;
//        }

//        public static string[] ReadAttributes(StreamReader sr, int count)
//        {
//            var result = new string[count];
//            var line = sr.ReadLine();
//            var splitted = line?.Split(' ', '\t').Where(v => v != "" && v != "\t");

//            for (int i = 0; i < count; i++)
//            {

//                result[i]
//            }
//        }

//        public static Bitmap GetBitmapFromFile(string fileName, Stream fileStream)
//        {
//            Bitmap bitmap = null;

//            using (var streamReader = new StreamReader(fileName))
//            {
//                var line = streamReader.ReadLine();
//                var ppmFormat = line;
//                if (ppmFormat != "P3" && ppmFormat != "P6")
//                    return;

//                Format = line;

//                while (MaxColor == -1)
//                {
//                    line = streamReader.ReadLine();

//                    if (line.StartsWith("#") || line.Length == 0)
//                        continue;

//                    if (line.Contains("#"))
//                        line = line.Substring(0, line.IndexOf('#') - 1);

//                    var values = line.Split(new char[] { ' ', '\t' }).Where(v => v != "" && v != "\t");

//                    foreach (var value in values)
//                    {
//                        if (Int32.TryParse(value, out var val))
//                        {
//                            if (Width == 0)
//                                Width = val;
//                            else if (Height == 0)
//                                Height = val;
//                            else if (MaxColor == -1)
//                                MaxColor = val;
//                            else
//                                arg = val;
//                        }
//                        else
//                        {
//                            throw new Exception();
//                        }
//                    }
//                }
//            }


//        }
//    }
