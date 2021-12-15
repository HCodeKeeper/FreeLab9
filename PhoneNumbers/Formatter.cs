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
        private string Pattern;
        private readonly string DefaultHeader; //First line that will be appended before the formatted data //Просто что бы разграничивать

        public Formatter()
        {
            Pattern = @"^(\+38|3?8?)(\(|\s)?\d{3}(\)\s?|\s)?\d{3}(-?\d{2}){2}$"; //@ - дает возможность записывать строку без знаков экранированя "\", в следствии чего не приходится использовать двойной \ 
            DefaultHeader = "\n[Output]";
        }
        

        public void Format(string sourcePath, string destination, string header)
        {
            int WithoutCountryCodeSize = 10; //не учитывая 38
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
                    if (Regex.IsMatch(c.ToString(), "\\d")) //фильтруем и генерим стринг билдер только из цифр номера
                    {
                        sb.Append(c);
                    }
                }

                int beginInd = sb.Length - WithoutCountryCodeSize; // с этого индекса будет начинаться номер без 38
                string resultLine = "+38 " + sb.ToString(beginInd, 3) + sb.ToString(beginInd + 3, 3) // 38 + *** + *** + ** + **
                                    + "-" + sb.ToString(beginInd + 6, 2) + "-" + sb.ToString(beginInd + 8, 2);
                data[i] = resultLine;
                
            }
            WriteData(destination, GetWithHeader(data, header));
        }

        public void Format(string sourcePath, string destination)
        {
            Format(sourcePath, destination, DefaultHeader);
        }

        private List<string> GetWithHeader(List<string> data, string header) //в стилистических целях добавляем строку отделающуюю старые и новые данные в файле
        {
            List<string> headedData = new List<string>(data); //избавляемся от потенциальных проблем с мутабельностью списков
            headedData.Insert(0, header);
            return headedData;
        }

        private List<string> GetData(string sourcePath)
        {
            List<string> data = new List<string>();
            
            using (StreamReader streamReader = new StreamReader(sourcePath)) //используем using resources что бы неявно закрыть поток после выхода из scope'а using
            //могут быть memore leak'и так как исключения не обработаны
            {
                while (true)
                {
                    string? line = streamReader.ReadLine(); //очень важно сформировать список строк для удобности и из-за использования ^abc$ в паттерне
                    if (line == null) //если достигли конца файла
                    {
                        break;
                    }

                    data.Add(line);
                }
            }

            return data;
        }

        private void WriteData(string destination, List<string> data)
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