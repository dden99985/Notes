using Foundation;
using System;
using UIKit;

namespace Notes
{
	public partial class NotesTableViewController : UITableViewController
	{
		
		static NSString NoteCellId = new NSString ("NotesTableViewCell");

		//public nfloat[] RowHeights;

		protected NotesTableViewController(IntPtr handle) : base(handle)
		{
			TableView.RegisterClassForCellReuse(typeof(NotesTableViewCell), NoteCellId);
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			// Perform any additional setup after loading the view, typically from a nib

			Data.DB database = new Data.DB();
			var source = new NotesTableSource();
			source.ViewController = new WeakReference(this);
			TableView.Source = source;
		}

		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
			// Release any cached data, images, etc that aren't in use.
		}

		//public override nint RowsInSection(UITableView tableview, nint section)
		//{
		//	return 6;
		//}

		//public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
		//{
		//	return RowHeights[indexPath.Row];
			
		//	//return base.GetHeightForRow(tableView, indexPath);
		//}
	
		//public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		//{
		//	var cell = (NotesTableViewCell)tableView.DequeueReusableCell(NoteCellId, indexPath);
		//	cell.ViewController = new WeakReference(this);
		//	cell.indexPath = indexPath;
		//	return cell;
		//}

	}
}