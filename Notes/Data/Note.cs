using System;
using System.IO;
using Newtonsoft.Json;
using UIKit;

namespace Notes.Data
{
	public class Note : IDBObject
	{
		string IDBObject.GetPath()
		{
			string filename = String.Format("{0:0000000000}.json", NoteId);
			return Path.Combine("Notes", filename);
		}

		public int NoteId { get; set; }
		public DateTime Date { get; set; }
		public UIBezierPath BezierPath { get; set; }

		public void Initialize()
		{
			NoteId = DB.control.GetNextNoteId();
			Date = DateTime.Now;
		}

		public Note()
		{
			Initialize();
		}

		[JsonConstructor]
		public Note(int NoteId, DateTime Date, UIBezierPath BezierPath)
		{
			this.NoteId = NoteId;
			this.Date = Date;
			this.BezierPath = BezierPath;
		}

	}
}
