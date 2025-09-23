using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppSage.Infrastructure.Serialization
{
    public class AppSageSerializer
    {
        // Serialize the metrics to JSON


        public static void SerializeToFile<T>(string filePath, T obj)
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Objects,
                NullValueHandling = NullValueHandling.Ignore
            };

            using (var writer = new StreamWriter(filePath))
            {
                using (var jsonWriter = new JsonTextWriter(writer))
                {
                    var serializer = JsonSerializer.Create(settings);
                    serializer.Serialize(jsonWriter, obj);
                }
            }
        }

        public static T DeserializeFromFile<T>(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The file {filePath} does not exist.");
            }

            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Objects,
                NullValueHandling = NullValueHandling.Ignore
            };

            using (var reader = new StreamReader(filePath))
            {
                using (var jsonReader = new JsonTextReader(reader))
                {
                    var serializer = JsonSerializer.Create(settings);
                    return serializer.Deserialize<T>(jsonReader);
                }
            }
        }
    }
}
