using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ApartmentsProject.Models;

namespace ApartmentsProject.ViewModel.Utilities
{
    public class ParameterMappingFunctions
    {
        public static List<Data> Parse(string fileName)
        {
            List<Data> parameters = new List<Data>();

            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = $"ApartmentsProject.Resources.{fileName}";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new FileNotFoundException("Resource file not found.");

                using (StreamReader reader = new StreamReader(stream))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.StartsWith("PARAM"))
                        {
                            // Разбиваем строку на части с разделителем табуляции
                            string[] parts = line.Split('\t');
                            if (parts.Length < 9)
                            {
                                continue; // значит невалидная строка
                            }

                            Data parameter = new Data()
                            {
                                Name = parts[2],
                                DataType = parts[3],
                                Description = parts[7],
                            };
                            parameters.Add(parameter);
                        }
                    }
                }
            }
            return parameters;
        }
    }
}
