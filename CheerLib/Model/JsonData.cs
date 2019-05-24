using Newtonsoft.Json;
using System;

namespace CheerLib.Model
{
    public class JsonData
    {
        public string ToJson()
        {
            string data = string.Empty;

            try
            {
                data = JsonConvert.SerializeObject(this,new JsonConverter[] { new CheerDoubleConverter()});
            }
            catch (Exception ex)
            {
                CheerLib.LogWriter.Log(ex.ToString());
            }

            return data;
        }

        public static T FromJson<T>(string jsonData) where T : JsonData, new()
        {
            var data = new T();

            try
            {
                data = JsonConvert.DeserializeObject<T>(jsonData);
            }
            catch (Exception ex)
            {
                CheerLib.LogWriter.Log(ex.ToString());
            }

            return data;
        }
    }
}
