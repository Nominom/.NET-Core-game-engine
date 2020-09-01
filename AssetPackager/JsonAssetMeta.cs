using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace AssetPackager
{
	public class JsonAssetMeta : IAssetMeta
	{
		private Dictionary<string, string> dictionary = new Dictionary<string, string>();

		public T GetValue<T>(string key, T defaultValue){
			var lowerCaseKey = key.ToLower();
			if (dictionary.TryGetValue(lowerCaseKey, out var json)) {
				if (typeof(T).IsEnum) {
					string enumString = JsonSerializer.Deserialize<string>(json);
					if (EnumParser<T>.TryParse(enumString, out T value)) {
						return value;
					}
				}
				else {
					return JsonSerializer.Deserialize<T>(json);
				}
			}
			return defaultValue;
		}

		public bool TryGetValue<T>(string key, out T value)
		{
			var lowerCaseKey = key.ToLower();
			if (dictionary.TryGetValue(lowerCaseKey, out var json)) {
				if (typeof(T).IsEnum) {
					string enumString = JsonSerializer.Deserialize<string>(json);
					if (!EnumParser<T>.TryParse(enumString, out value)) {
						value = default;
						return false;
					}
				}
				else {
					value = JsonSerializer.Deserialize<T>(json);
				}
				return true;
			}
			else {
				value = default;
				return false;
			}
		}

		public void SetValue<T>(string key, T value) {
			var lowerCaseKey = key.ToLower();
			var json = JsonSerializer.Serialize(value);
			if (typeof(T).IsEnum) {
				string enumString = Enum.GetName(typeof(T), value);
				json = JsonSerializer.Serialize(enumString);
			}
			dictionary[lowerCaseKey] = json;
		}

		public void WriteToStream(Stream stream) {

			var writerOptions = new JsonWriterOptions
			{
				Indented = true,
				Encoder = JavaScriptEncoder.Create(new TextEncoderSettings(UnicodeRanges.All)),
				SkipValidation = false
			};

			using var writer = new Utf8JsonWriter(stream, options: writerOptions);

			writer.WriteStartObject();

			foreach (var element in dictionary.OrderBy(x => x.Key)) {
				writer.WritePropertyName(element.Key);
				using JsonDocument document = JsonDocument.Parse(element.Value);
				document.WriteTo(writer);
			}

			writer.WriteEndObject();
		}

		public void ReadFromStream(Stream stream) {
			using JsonDocument document = JsonDocument.Parse(stream, new JsonDocumentOptions() {
				AllowTrailingCommas = true,
				CommentHandling = JsonCommentHandling.Skip,
				MaxDepth = 0
			});

			var rootElement = document.RootElement;
			if (rootElement.ValueKind != JsonValueKind.Object) {
				throw new FormatException("Json Meta file root should be an object.");
			}

			foreach (JsonProperty jsonProperty in rootElement.EnumerateObject()) {
				dictionary[jsonProperty.Name.ToLower()] = jsonProperty.Value.GetRawText();
			}
		}
	}
}
