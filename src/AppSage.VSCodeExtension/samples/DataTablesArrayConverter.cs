namespace AppSage.Web.Pages.Reports.Repository.RepositoryAnalysis
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;



    public sealed class DataTablesExactArrayConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            if (objectType.IsArray && objectType.GetElementType() == typeof(DataTable)) return true;
            if (typeof(IEnumerable<DataTable>).IsAssignableFrom(objectType)) return true;
            return false;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is null) { writer.WriteNull(); return; }

            IEnumerable<DataTable> tables = value switch
            {
                DataTable[] arr => arr,
                IEnumerable<DataTable> seq => seq,
                _ => throw new JsonSerializationException("Unsupported type for DataTablesExactArrayConverter.")
            };

            writer.WriteStartArray();
            foreach (var table in tables)
            {
                if (table is null) { writer.WriteNull(); continue; }

                writer.WriteStartObject();

                // TableName
                writer.WritePropertyName("TableName");
                writer.WriteValue(table.TableName);

                // Columns (Name + DataType)
                writer.WritePropertyName("Columns");
                writer.WriteStartArray();
                foreach (DataColumn col in table.Columns)
                {
                    writer.WriteStartObject();

                    writer.WritePropertyName("Name");
                    writer.WriteValue(col.ColumnName);

                    writer.WritePropertyName("DataType");
                    var t = Nullable.GetUnderlyingType(col.DataType) ?? col.DataType;
                    writer.WriteValue(t.FullName); // e.g., "System.Decimal"

                    writer.WriteEndObject();
                }
                writer.WriteEndArray();

                // Rows (objects keyed by column name)
                writer.WritePropertyName("Rows");
                writer.WriteStartArray();
                foreach (DataRow row in table.Rows)
                {
                    writer.WriteStartObject();
                    foreach (DataColumn col in table.Columns)
                    {
                        writer.WritePropertyName(col.ColumnName);
                        var v = row[col];
                        serializer.Serialize(writer, v == DBNull.Value ? null : v);
                    }
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();

                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            => throw new NotImplementedException("Deserialization not implemented.");

        public override bool CanRead => false;
    }


}
