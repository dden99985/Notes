using Foundation;
using System;
using UIKit;
using CoreGraphics;

namespace Notes
{
    public partial class NotesTableViewCell : UITableViewCell
    {
		CGPath path;

		public WeakReference ViewController{get; set;}
		public NSIndexPath indexPath;
//		bool touchesCaptured;

        public NotesTableViewCell (IntPtr handle) : base (handle)
        {
			path = new CGPath();
//			touchesCaptured = false;

			path.AddLines(new CGPoint[]{
					new CGPoint(5,5),
					new CGPoint(20,20),
					new CGPoint(5,20)});
			path.CloseSubpath();
        }

/*		public override UIView HitTest(CGPoint point, UIEvent uievent)
		{
			if (touchesCaptured)
			{
				Console.WriteLine("HitTest: TouchCaptured:true");
				return this;
			}
			else
			{
				Console.WriteLine("HitTest: TouchCaptured:false");
				return base.HitTest(point, uievent);
			}
		}
*/	
		public override void TouchesBegan(NSSet touches, UIEvent evt)
		{
			base.TouchesBegan(touches, evt);

			Console.WriteLine("TouchesBegan: touchesCaptured:true");
//			touchesCaptured = true;
			UITouch touch = touches.AnyObject as UITouch;
			if (touch != null)
			{
				path.AddLineToPoint(touch.LocationInView(this));
				SetNeedsDisplayInRect(CGRect.Inflate(path.BoundingBox, 5f, 5f));
			}
		}

		/*
		public override bool ShouldReceiveTouch(UIGestureRecognizer recognizer, UITouch touch)
		{
			if (touchesCaptured)
			{
				Console.WriteLine("ShouldReceiveTouch: TouchCaptured:true");
				return true;
			}
			else
			{
				Console.WriteLine("ShouldReceiveTouch: TouchCaptured:false");
				return this.Frame.Contains(touch.LocationInView(this));
			}
		}
		*/

		public override void TouchesMoved(NSSet touches, UIEvent evt)
		{
			base.TouchesMoved(touches, evt);
			Console.WriteLine("TouchesMoved: touchesCaptured:true");

			UITouch touch = touches.AnyObject as UITouch;
			if (touch != null)
			{
				path.AddLineToPoint(touch.LocationInView(this));
				if (path.BoundingBox.Y < 0)
				{
					path = path.CopyByTransformingPath(CGAffineTransform.MakeTranslation(0f, path.BoundingBox.Y * -1));
				}

				NotesTableViewController vc = (NotesTableViewController)ViewController.Target;
				if (path.BoundingBox.Height + 2 > vc.RowHeights[indexPath.Row])
				{
					vc.TableView.BeginUpdates();
					vc.RowHeights[indexPath.Row] = path.BoundingBox.Height + 2;
					//vc.UpdateViewConstraints();
					vc.TableView.EndUpdates();
				}
				SetNeedsDisplayInRect(CGRect.Inflate(path.BoundingBox, 5f, 5f));
			}
		}

		public override void TouchesEnded(NSSet touches, UIEvent evt)
		{
			base.TouchesEnded(touches, evt);
			Console.WriteLine("TouchesEnded: touchesCaptured:false");
			//			touchesCaptured = false;
			SetNeedsDisplayInRect(CGRect.Inflate(path.BoundingBox, 5f, 5f));
//			((NotesTableViewController)ViewController.Target).View.UpdateConstraints();
		}

		public override void TouchesCancelled(NSSet touches, UIEvent evt)
		{
			base.TouchesCancelled(touches, evt);
			Console.WriteLine("TouchesCancelled: touchesCaptured:false");
//			touchesCaptured = false;
		}

		public override void Draw(CoreGraphics.CGRect rect)
		{
			base.Draw(rect);

			using (CGContext g = UIGraphics.GetCurrentContext())
			{
				g.SetLineWidth(2);
				UIColor.Blue.SetFill();
				UIColor.Red.SetStroke();

				g.AddPath(path);
				g.DrawPath(CGPathDrawingMode.FillStroke);
			}
		}
    }
}