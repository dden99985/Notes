using System;
using Newtonsoft.Json;

namespace Notes.DB
{
	public class Control
	{
		public string Version { get; set; }
		public Int32 NextNoteId { get; set; }

		public Control()
		{
		}

		[JsonConstructor]
		public Control(string Version, int NextNoteId)
		{
			this.Version = Version;
			this.NextNoteId = NextNoteId;
		}
	}
}
