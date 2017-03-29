using System;
using System.IO;
using System.Runtime.InteropServices;
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

		public string GetDrawingPath()
		{
			var filename = String.Format("{0:0000000000}.drawing", NoteId);
			return Path.Combine(DB.DBPath, "Notes", filename);
		}

		public string GetDrawingPath(string filename)
		{
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
		private BinaryWriter drawingWriter;

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(Note);
		}

		void CGPathApplierFunction(CGPathElement element)
		{
			switch (element.Type)
			{
				case CGPathElementType.AddCurveToPoint:
					drawingWriter.Write((Int32)element.Type);
					drawingWriter.Write(element.Point1.X);
					drawingWriter.Write(element.Point1.Y);
					drawingWriter.Write(element.Point2.X);
					drawingWriter.Write(element.Point2.Y);
					drawingWriter.Write(element.Point3.X);
					drawingWriter.Write(element.Point3.Y);
					break;
				
				case CGPathElementType.AddLineToPoint:
					drawingWriter.Write((Int32)element.Type);
					drawingWriter.Write(element.Point1.X);
					drawingWriter.Write(element.Point1.Y);
					break;
				
				case CGPathElementType.AddQuadCurveToPoint:
					drawingWriter.Write((Int32)element.Type);
					drawingWriter.Write(element.Point1.X);
					drawingWriter.Write(element.Point1.Y);
					drawingWriter.Write(element.Point2.X);
					drawingWriter.Write(element.Point2.Y);
					break;
				
				case CGPathElementType.CloseSubpath:
					drawingWriter.Write((Int32)element.Type);
					break;
				
				case CGPathElementType.MoveToPoint:
					drawingWriter.Write((Int32)element.Type);
					drawingWriter.Write(element.Point1.X);
					drawingWriter.Write(element.Point1.Y);
					break;
			}

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
				// Open the graphics file
				using (FileStream sw = new FileStream(note.GetDrawingPath(), FileMode.Create, FileAccess.Write))
				{
					using (BinaryWriter bw = new BinaryWriter(sw))
					{
						drawingWriter = bw;
						note.Drawing.Apply(CGPathApplierFunction);
					}
				}

				writer.WritePropertyName("Drawing");
				writer.WriteValue(Path.GetFileName(note.GetDrawingPath()));

			}
			writer.WriteEnd();
		}

		protected CGPath readDrawing(String pathname)
		{

			var path = new CGPath();
			var point1 = new CGPoint();
			var point2 = new CGPoint();
			var point3 = new CGPoint();

			try
			{
				// Open the graphics file
				using (FileStream sr = new FileStream(pathname, FileMode.Open, FileAccess.Read))
				{
					using (BinaryReader br = new BinaryReader(sr))
					{
						// Read until eof()
						while (true)
						{
							// Read the element type
							switch ((CGPathElementType)br.ReadInt32())
							{
								case CGPathElementType.AddCurveToPoint:
									point1.X = (nfloat)br.ReadDouble();
									point1.Y = (nfloat)br.ReadDouble();
									point2.X = (nfloat)br.ReadDouble();
									point2.Y = (nfloat)br.ReadDouble();
									point3.X = (nfloat)br.ReadDouble();
									point3.Y = (nfloat)br.ReadDouble();
									path.AddCurveToPoint(point1, point2, point3);
									break;

								case CGPathElementType.AddLineToPoint:
									point1.X = (nfloat)br.ReadDouble();
									point1.Y = (nfloat)br.ReadDouble();
									path.AddLineToPoint(point1);
									break;

								case CGPathElementType.AddQuadCurveToPoint:
									point1.X = (nfloat)br.ReadDouble();
									point1.Y = (nfloat)br.ReadDouble();
									point2.X = (nfloat)br.ReadDouble();
									point2.Y = (nfloat)br.ReadDouble();
									path.AddQuadCurveToPoint(point1.X, point1.Y, point2.X, point2.Y);
									break;

								case CGPathElementType.CloseSubpath:
									path.CloseSubpath();
									break;

								case CGPathElementType.MoveToPoint:
									point1.X = (nfloat)br.ReadDouble();
									point1.Y = (nfloat)br.ReadDouble();
									path.MoveToPoint(point1);
									break;
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				if (e is EndOfStreamException || e is FileNotFoundException)
				{
					// No problem
				}
				else
				{
					throw;
				}
			}

			return path;
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

						case "drawing":
							String filename = reader.ReadAsString();
							note.Drawing = readDrawing(note.GetDrawingPath(filename));
							if (note.Height < note.Drawing.BoundingBox.Height)
							{
								note.Height = note.Drawing.BoundingBox.Height;
							}
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
