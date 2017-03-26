using System;
using Notes.Data;
using UIKit;
using Foundation;
using System.IO;

namespace Notes
{
	public class NotesTableSource : UITableViewSource
	{
		static NSString NoteCellId = new NSString("NotesTableViewCell");
		public WeakReference ViewController { get; set; }

		NotesList NotesList
		{
			get
			{
				return DB.AllNotes;
			}
		}

		public NotesTableSource()
		{

		}

		public override nint RowsInSection(UITableView tableview, nint section)
		{
			return NotesList.Count;
		}

		public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
		{
			return NotesList[indexPath.Row].Height;
		}

		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{
			var cell = (NotesTableViewCell)tableView.DequeueReusableCell(NoteCellId, indexPath);
			cell.Note = NotesList[indexPath.Row];
			cell.ViewController = ViewController;
			cell.indexPath = indexPath;
			return cell;
		}


	}
}
