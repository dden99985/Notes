using System;
using System.IO;
using Mono.Data.Sqlite;

namespace Notes
{
	public class DB
	{
		private static string version = "1.0";

		/// <summary>
		/// Used to serialize access to the database
		/// </summary>
		private object locker;

		/// <summary>
		/// Path to the library folder
		/// </summary>
		private string libraryPath;

		/// <summary>
		/// Full pathname for the databae
		/// </summary>
		private string dbName;

		/// <summary>
		/// Path to the library folder
		/// </summary>
		/// <value>Path to the library folder.</value>
		public string LibraryPath
		{
			get
			{
				if (libraryPath == null)
				{
					// Get Library folder
					string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
					libraryPath = Path.Combine(documentsPath, "..", "Library"); // Library folder instead
				}
				return libraryPath;
			}
		}

		/// <summary>
		/// Filename of the database
		/// </summary>
		/// <value>The name of the database file.</value>
		public string FileName
		{
			get
			{
				return "notes.db3";
			}
		}

		/// <summary>
		/// Full pathname of the database
		/// </summary>
		/// <value>Full path of the database.</value>
		public string DBName
		{
			get
			{
				if (dbName == null)
				{
					dbName = Path.Combine(LibraryPath, FileName);
				}
				return dbName;
			}
		}

		public DB()
		{
			locker = new object();
			libraryPath = null;
			dbName = null;
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
		private void setupSchema(SqliteConnection connection)
		{
			var datatable = connection.GetSchema("Tables");
			var rows = datatable.Select("table_name = 'control'");
			if(rows.Length == 0)
			{
				// Create control table
				using (var command = connection.CreateCommand())
				{
					command.CommandText = "CREATE TABLE control (tag TEXT NOT NULL PRIMARY KEY, value TEXT);";
					command.ExecuteNonQuery();
					command.CommandText = "INSERT INTO control (tag, value) VALUES ('VERSION', @version);";
					command.Parameters.AddWithValue("@version", version);
					command.ExecuteNonQuery();
				}
			}

			// Verify database version
			using (var command = connection.CreateCommand())
			{
				command.CommandText = "SELECT value FROM control WHERE tag = @tag;";
				command.Parameters.AddWithValue("@tag", "VERSION");
				var dbVersion = command.ExecuteScalar().ToString();
				if (dbVersion.CompareTo(version) != 0)
				{
					upgrade(dbVersion);
				}
			}



				


			int bob = 3;
		}


		public void Initialize()
		{
			// Check if database already exists
			if (!File.Exists(DBName))
			{
				// Create new database
				SqliteConnection.CreateFile(DBName);
			}

			// Create the connection
			using (var connection = new SqliteConnection("Data Source=" + DBName))
			{
				// SQLLite is not thread safe
				lock(locker)
				{
					connection.Open();

					setupSchema(connection);

					connection.Close();
				}
			}

		}


	}
}
