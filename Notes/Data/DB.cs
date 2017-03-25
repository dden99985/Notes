using System;
using System.IO;
using System.Runtime;
using Newtonsoft.Json;

/*
 * Library/NotesDB
 *   control.json {Version, NextNoteId}
 *   Notes/
 *     0000000001
 *     0000000002
 */


namespace Notes.Data
{
	public interface IDBObject
	{
		/// <summary>
		/// Returns the path for this object, relative to the NotesDB folder.
		/// </summary>
		/// <returns>The Path for this object.</returns>
		string GetPath();
	}


	public class DB
	{
		#region properties

		/// <summary>
		/// Current version of the database
		/// </summary>
		public static string version = "1.0";

		/// <summary>
		/// Path to the NotesDB folder
		/// </summary>
		private static string dbPath = null;

		/// <summary>
		/// Path to the control file
		/// </summary>
		private static string controlPath = null;

		/// <summary>
		/// Access to the database control file
		/// </summary>
		public static Control control;

		#endregion

		/// <summary>
		/// Used to lock access to the control file
		/// </summary>
		private static object ControlLocker = new object();


		/// <summary>
		/// Path to the NotesDB folder
		/// </summary>
		/// <value>Path to the NotesDB folder.</value>
		public static string DBPath
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
		public static string ControlPath
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

		/// <summary>
		/// Initialize the database by opening/creating the Library/NoteDB folder and control file.
		/// If the DB version doesn't match the class version, upgrade() is called to attempt to upgrade the database.
		/// </summary>
		private void Initialize()
		{
			// Load/create the control file
			control = new Control();

			// Check if the db version is compatible with this version
			if (control.Version != version)
			{
				upgrade(control.Version);
			}

		}

		/// <summary>
		/// Upgrade the database from dbVersion to the current version if possible.  Otherwise throw exception
		/// Note: This currently always throws an exception.
		/// </summary>
		/// <param name="dbVersion">Version from the database.</param>
		private void upgrade(string dbVersion)
		{
			throw new Exception(String.Format("Database version {0} is not compatible with version {1}", dbVersion, version));
		}


		/// <summary>
		/// Write obj to the file system.
		/// </summary>
		/// <param name="obj">Object to write.</param>
		/// <param name="filename">Filename to write to, including full path.</param>
		public static void WriteObject(Object obj, string filename)
		{
			var serializer = new JsonSerializer();
			serializer.NullValueHandling = NullValueHandling.Ignore;

			// Create the directory if it doesn't exist
			Directory.CreateDirectory(Path.GetDirectoryName(filename));

			using (StreamWriter sw = new StreamWriter(filename))
			{
				using (JsonWriter writer = new JsonTextWriter(sw))
				{
					serializer.Serialize(writer, obj, obj.GetType());
				}
			}
		}

		/// <summary>
		/// Write a IDBObject to the file system.
		/// </summary>
		/// <param name="obj">IDBOject to write to the file system.</param>
		public static void WriteObject(IDBObject obj)
		{
			string path = Path.Combine(DBPath, obj.GetPath());
			DB.WriteObject(obj, path);
		}

		/// <summary>
		/// Read an object from the file system."/>
		/// </summary>
		/// <returns>The object read from the file system.</returns>
		/// <param name="filename">The filename, including path.</param>
		/// <typeparam name="T">The type of object to read.</typeparam>
		public T ReadObject<T>(string filename)
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

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Notes.Data.DB"/> class.
		/// </summary>
		public DB()
		{
			// Setup control
			Initialize();
		}


	}
}
