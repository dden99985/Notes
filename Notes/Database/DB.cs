using System;
using System.IO;
using System.Runtime;
using Newtonsoft.Json;

/*
 * Library/NotesDB
 *   control.json {version, nextid}
 *   Notes/
 *     YYYY.MM.DD/
 *       0000000001
 *       0000000002
 */


namespace Notes.DB
{
	public class DB
	{
		private static string version = "1.0";

		/// <summary>
		/// Used to serialize access to the database
		/// </summary>
		private object locker;

		/// <summary>
		/// Path to the NotesDB folder
		/// </summary>
		private string dbPath;

		/// <summary>
		/// Path to the control file
		/// </summary>
		private string controlPath;


		/// <summary>
		/// Path to the NotesDB folder
		/// </summary>
		/// <value>Path to the NotesDB folder.</value>
		public string DBPath
		{
			get
			{
				if (dbPath == null)
				{
					// Get Library folder
					string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
					dbPath = Path.Combine(documentsPath, "..", "Library/NotesDB"); // Library folder instead
				}
				return dbPath;
			}
		}

		/// <summary>
		/// Path to the control file
		/// </summary>
		/// <value>Path to the control file.</value>
		public string ControlPath
		{
			get
			{
				if (controlPath == null)
				{
					// Get Library folder
					controlPath = Path.Combine(DBPath, "control.json");
				}
				return controlPath;
			}
		}

		public DB()
		{
			locker = new object();
			dbPath = null;
			controlPath = null;
		}

		/// <summary>
		/// Upgrade the database from dbVersion to the current version if possible.  Otherwise throw exception
		/// </summary>
		/// <returns>void.</returns>
		/// <param name="dbVersion">Version from the database.</param>
		private void upgrade(string dbVersion)
		{
			throw new Exception(String.Format("Database version {0} is not compatible with version {1}", dbVersion, version));
		}

		/// <summary>
		/// Create the schema if it doesn't already exist
		/// </summary>
		private void setupSchema()
		{

			int bob = 3;
		}

		protected void writeObject<T>(T obj, string filename)
		{
			JsonSerializer serializer = new JsonSerializer();
			serializer.NullValueHandling = NullValueHandling.Ignore;

			// Create the directory if it doesn't exist
			string path = Path.GetDirectoryName(filename);
			Directory.CreateDirectory(Path.GetDirectoryName(filename));

			using (StreamWriter sw = new StreamWriter(filename))
			{
				using (JsonWriter writer = new JsonTextWriter(sw))
				{
					serializer.Serialize(writer, obj);
				}
			}
		}

		protected T readObject<T>(string filename)
		{
			JsonSerializer serializer = new JsonSerializer();
			serializer.NullValueHandling = NullValueHandling.Ignore;

			using (StreamReader sr = new StreamReader(filename))
			{
				using (JsonReader reader = new JsonTextReader(sr))
				{
					return serializer.Deserialize<T>(reader);
				}
			}
		}


		public void Initialize()
		{
			Control control;

			// Check if database already exists
			if (!File.Exists(ControlPath))
			{
				// Create new database
				lock (locker)
				{
					control = new Control(version, 1);
					writeObject(control, ControlPath);
					//					string output = JsonConvert.SerializeObject(cnt)
				}
			}
			else
			{
				control = readObject<Control>(ControlPath);
				int bob = 3;
			}

		}


	}
}
