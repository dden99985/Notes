using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Notes.Data
{

	[JsonConverter(typeof(NotesListConverter))]
	public class NotesList : List<Note>, IDBObject
	{
		public string GetPath()
		{
			return GetPath(1);
		}

		public void Write()
		{
			DB.WriteObject(this);
		}

		public void Read()
		{
			// unfortunately, JsonSerializer.Populate doesn't use the JasonConverter...
			throw new NotImplementedException();
		}

		/// <summary>
		/// Static versio of GetPath.
		/// </summary>
		/// <returns>The path.</returns>
		/// <param name="staticVersion">Ignored.  Indicates that you want the static version.</param>
		public static string GetPath(int staticVersion)
		{
			return Path.Combine(DB.DBPath, "Notes", "contents.json");
		}
	}

	public class NotesListConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(NotesList);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var list = (NotesList)value;
			writer.WriteStartArray();
			foreach (Note note in list)
			{
				writer.WriteValue(note.NoteId);
			}
			writer.WriteEndArray();
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var list = new NotesList();

			var id = reader.ReadAsInt32();
			while (id != null)
			{
				var note = DB.ReadObject<Note>(Path.Combine(DB.DBPath, Note.GetPath(id.Value)));
				list.Add(note);
				id = reader.ReadAsInt32();
			}

			return list;
		}
	}

}

