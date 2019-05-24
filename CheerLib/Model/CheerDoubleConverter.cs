using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheerLib.Model
{
    public class CheerDoubleConverter : CustomCreationConverter<double>
    {

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public override double Create(Type objectType)
        {
            return 0.0;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value==null)
            {
                writer.WriteNull();
                return;
            }

            var formatVal = ((double)value).ToString("F64");
            formatVal = formatVal.TrimEnd('0');
            writer.WriteValue(formatVal);
        }

    }
}
