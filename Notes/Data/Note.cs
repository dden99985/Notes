﻿using System;
using System.IO;
using CoreGraphics;
using Newtonsoft.Json;
using UIKit;

namespace Notes.Data
{
	[JsonConverter(typeof(NoteConverter))]
	public class Note : IDBObject
	{
		
		public string GetPath()
		{
			return Note.GetPath(NoteId);
		}

		public void Read()
		{
			// unfortunately, JsonSerializer.Populate doesn't use the JasonConverter...
			throw new NotImplementedException();
		}

		public void Write()
		{
			DB.WriteObject(this);
		}


		public static string GetPath(int NoteId)
		{
			var filename = String.Format("{0:0000000000}.json", NoteId);
			return Path.Combine(DB.DBPath, "Notes", filename);
		}

		public static string GetDrawingPath(int NoteId)
		{
			var filename = String.Format("{0:0000000000}.drawing", NoteId);
			return Path.Combine(DB.DBPath, "Notes", filename);
		}


		public int NoteId { get; set; }
		public DateTime Date { get; set; }
		public nfloat Height { get; set; }
		public CGPath Drawing { get; set; }

		public void Initialize()
		{
			NoteId = DB.control.GetNextNoteId();
			Date = DateTime.Now;
			Height = 0f;
		}


		public Note()
		{
		}

		public Note(nfloat Height, CGPath Path)
		{
			Initialize();
			this.Height = Height;
			this.Drawing = Path;
		}

		[JsonConstructor]
		public Note(int NoteId, DateTime Date, nfloat Height, CGPath Path)
		{
			this.NoteId = NoteId;
			this.Date = Date;
			this.Height = Height;
			this.Drawing = Path;
		}

	}

	public class NoteConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(Note);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var note = (Note)value;

			writer.WriteStartObject();
			writer.WritePropertyName("NoteId");
			writer.WriteValue(note.NoteId);
			writer.WritePropertyName("Date");
			writer.WriteValue(note.Date);
			writer.WritePropertyName("Height");
			writer.WriteValue((Double)note.Height);
			if (note.Drawing != null && !note.Drawing.IsEmpty)
			{
				writer.WritePropertyName("Drawing");
				writer.WriteValue(note.NoteId);
			}
			writer.WriteEnd();
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var note = new Note();

			while (reader.Read())
			{
				var type = reader.TokenType;
				if (type == JsonToken.PropertyName)
				{
					switch (reader.Value.ToString().ToLower())
					{
						case "noteid":
							note.NoteId = reader.ReadAsInt32().Value;
							break;

						case "date":
							note.Date = reader.ReadAsDateTime().Value;
							break;

						case "height":
							note.Height = (nfloat)reader.ReadAsDouble().Value;
							break;

					}
				}
			}

			return note;
		}
	}


	public class nfloatConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(nfloat);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			writer.WriteValue((Double)value);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			return (nfloat)reader.ReadAsDouble();
		}

	}
}
