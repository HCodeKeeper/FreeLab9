using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks.Dataflow;

namespace PhoneNumbers
{
    public class Formatter
    {
        private int FullPhoneNumberSize;
        private string Pattern;
        private readonly string DefaultHeader; //First line that will be appended before the formatted data //Просто что бы разграничивать

        public Formatter()
        {
            FullPhoneNumberSize = 12;
            Pattern = @"^(\+38|3?8?)(\(|\s)?\d{3}(\)\s?|\s)?\d{3}(-?\d{2}){2}$"; //@ - дает возможность записывать строку без знаков экранированя "\", в следствии чего не приходится использовать двойной \ 
            DefaultHeader = "[Output]";
        }
        

        public void Format(string sourcePath, string destination, string header)
        {
            List<string> data = GetData(sourcePath);
            for (int i=0; i < data.Count; i++)
            {
                if (!Regex.IsMatch(data[i], Pattern))
                {
                    data[i] = "Failed!";
                    continue;
                }
                
                StringBuilder sb = new StringBuilder();
                foreach (char c in data[i].ToCharArray())
                {
                    if (Regex.IsMatch(c.ToString(), "\\d"))
                    {
                        sb.Append(c);
                    }
                }

                int beginInd = FullPhoneNumberSize - sb.Length; // с этого индекса будет начинатся часть номера, полученного из входных данных
            }
        }

        public void Format(string sourcePath, string destination)
        {
            Format(sourcePath, destination, DefaultHeader);
        }

        private List<string> GetWithHeader(List<string> data, string header)
        {
            List<string> headedData = new List<string>(data);
            headedData.Insert(0, header);
            return headedData;
        }

        private List<string> GetData(string sourcePath)
        {
            List<string> data = new List<string>();
            
            using (StreamReader streamReader = new StreamReader(sourcePath))
            {
                while (true)
                {
                    string? line = streamReader.ReadLine();
                    if (line == null)
                    {
                        break;
                    }

                    data.Add(line);
                }
            }

            return data;
        }

        private void WriteData(string destination, string[] data)
        {
            using (StreamWriter streamWriter = new StreamWriter(destination, true))
            {
                foreach (string line in data)
                {
                    streamWriter.WriteLine(line);
                }
            }
        }
    }
}