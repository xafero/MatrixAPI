using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace libMatrix.Helpers
{
    class JsonHelper
    {
        public static string Serialize(object instance)
        {
            using (var stream = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(instance.GetType());
                serializer.WriteObject(stream, instance);
                return Encoding.UTF8.GetString(stream.ToArray(), 0, (int)stream.Length);
            }
        }
    }
}
