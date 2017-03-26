using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Notes.Data
{
	public class Control
	{
		private static object locker = new object();

		[JsonProperty]
		public string Version { get; set; }

		[JsonProperty]
		protected int NextNoteId { get; set; }


		/// <summary>
		/// Initializes this object by reading from the control file.
		/// If the control file doesn't exist, this object is initialized with default values and the
		/// control file is written to the file system. 
		/// </summary>
		private void initialize()
		{
			// Check if database already exists
			if (!File.Exists(DB.ControlPath))
			{
				// Create new database
				lock (locker)
				{
					this.Version = DB.version;
					this.NextNoteId = 1;
					DB.WriteObject(this, DB.ControlPath);
				}
			}
			else
			{
				// Read the control info
				lock (locker)
				{
					DB.ReadObject(DB.ControlPath, this);
				}
			}
		}

		/// <summary>
		/// Write this object to the control file.
		/// </summary>
		public void Write()
		{
			lock(locker)
			{
				DB.WriteObject(this, DB.ControlPath);
			}
		}


		/// <summary>
		/// Get the next NoteId, updating the NextNoteId in the control file.
		/// </summary>
		/// <returns>The next available NoteId.</returns>
		public int GetNextNoteId()
		{
			int noteId;

			lock(locker)
			{
				noteId = this.NextNoteId++;
				Write();
			}
			return noteId;
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="T:Notes.Data.Control"/> class.
		/// </summary>
		public Control()
		{
			initialize();
		}

	}
}
