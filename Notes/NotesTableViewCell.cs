using Foundation;
using System;
using UIKit;
using CoreGraphics;

//  Note:  The approach taken in this view was initially based on the helpful
//         "Smooth Freehand Drawing in iOS" article by Akiel Khan:
//         http://mobile.tutsplus.com/tutorials/iphone/ios-sdk_freehand-drawing/

namespace Notes
{
	public partial class NotesTableViewCell : UITableViewCell
	{
		public Data.Note Note { get; set; }


		public WeakReference ViewController { get; set; }
		public NSIndexPath indexPath;

		private int nextPoint;
		private CGPoint[] bezierPoints = new CGPoint[4];
		CGPath path;
		CGPath partPath;

		public NotesTableViewCell(IntPtr handle) : base(handle)
		{
			path = new CGPath();
			partPath = new CGPath();

			path.AddLines(new CGPoint[]{
					new CGPoint(5,5),
					new CGPoint(20,20),
					new CGPoint(5,20)});
			path.CloseSubpath();
		}

		public override void TouchesBegan(NSSet touches, UIEvent evt)
		{
			base.TouchesBegan(touches, evt);

			Console.WriteLine("TouchesBegan: touchesCaptured:true");

			UITouch touch = touches.AnyObject as UITouch;
			if (touch != null)
			{
			    nextPoint = 0;
				addPoint(touch.LocationInView(this));
			}
		}

		public override void TouchesMoved(NSSet touches, UIEvent evt)
		{
			base.TouchesMoved(touches, evt);
			Console.WriteLine("TouchesMoved: touchesCaptured:true");

			UITouch touch = touches.AnyObject as UITouch;
			if (touch != null)
			{
				addPoint(touch.LocationInView(this));
			}

			nfloat minY = path.BoundingBox.Y < partPath.BoundingBox.Y ? path.BoundingBox.Y : partPath.BoundingBox.Y;
			nfloat maxHeight = path.BoundingBox.Height > partPath.BoundingBox.Height ? path.BoundingBox.Height : partPath.BoundingBox.Height;

			// Check if we've drawn past the top of the cell
			if (path.BoundingBox.Y < 0f)
			{
				// Shift the paths down
				if (!path.IsEmpty)
				{
					path = path.CopyByTransformingPath(CGAffineTransform.MakeTranslation(0f, minY * -1f));
				}
				if (!partPath.IsEmpty)
				{
					partPath = partPath.CopyByTransformingPath(CGAffineTransform.MakeTranslation(0f, minY * -1f));
				}
			}

			// Check if we've drawn past the bottom of the cell
			NotesTableViewController vc = (NotesTableViewController)ViewController.Target;
			if (maxHeight + 2.0 > Note.Height)
			{
				// Increase the height of the cell
				vc.TableView.BeginUpdates();
				Note.Height = maxHeight + 2;
				//vc.UpdateViewConstraints();
				vc.TableView.EndUpdates();
			}
		}

		public override void TouchesEnded(NSSet touches, UIEvent evt)
		{
			base.TouchesEnded(touches, evt);
			Console.WriteLine("TouchesEnded: touchesCaptured:false");

			UITouch touch = touches.AnyObject as UITouch;
			if (touch != null)
			{
				finishPoint(touch.LocationInView(this));
			}

			Note.Drawing = path;
			Note.Write();

			//			((NotesTableViewController)ViewController.Target).View.UpdateConstraints();
		}

		public override void TouchesCancelled(NSSet touches, UIEvent evt)
		{
			base.TouchesCancelled(touches, evt);
			Console.WriteLine("TouchesCancelled: touchesCaptured:false");
		}

		public override void Draw(CoreGraphics.CGRect rect)
		{
			base.Draw(rect);

			using (CGContext g = UIGraphics.GetCurrentContext())
			{
				g.SetLineWidth(2);

				// Stroke the current path
				UIColor.Blue.SetStroke();
				g.AddPath(path);
				g.StrokePath();

				// And the partial path
				if (partPath != null)
				{
					UIColor.Red.SetStroke();
					g.AddPath(partPath);
					g.StrokePath();
				}

				//				g.AddPath(path);
				//				g.DrawPath(CGPathDrawingMode.FillStroke);

				//// Now stroke the current path
				//	[path stroke];

				//// And the partial path
				//CGContextSetStrokeColorWithColor(context, self.tempPenColor);
				//	[partPath stroke];


				//NSLog(@"Curves %d", curveCount);

			}
		}


		protected void addPoint(CGPoint aNewPoint)
		{

			if (nextPoint < 4)
			{
				bezierPoints[nextPoint] = aNewPoint;

				// Draw this partial segment so it doesn't appear that the line is always lagging a bit behind the penn
				partPath = new CGPath();
				partPath.MoveToPoint(bezierPoints[0]);
				partPath.AddCurveToPoint(bezierPoints[nextPoint],
										 bezierPoints[nextPoint < 1 ? 0 : 1],
										 bezierPoints[nextPoint < 2 ? nextPoint : 2]);
				nextPoint++;

				//        [partPath removeAllPoints];
				//        [partPath moveToPoint:bezierPoints[0]];
				//        [partPath addCurveToPoint:bezierPoints[nextPoint]
				//		controlPoint1:bezierPoints[nextPoint < 1 ? 0 : 1] controlPoint2:bezierPoints[nextPoint < 2 ? nextPoint : 2]];

				//        nextPoint++;
				//        [self setNeedsDisplayInRect:CGRectInset([partPath bounds], -10, -10)];
			}

			else
			{
				// Calculate the endpoint of this curve (and the start point of the next curve) to the middle of an
				// imaginary line between the second control point of the first curve and the first control point
				// of the next curve
				bezierPoints[3] = new CGPoint((bezierPoints[2].X + aNewPoint.X) / 2.0,
											  (bezierPoints[2].Y + aNewPoint.Y) / 2.0);
				partPath = new CGPath();
				if (path.IsEmpty)
				{
					// set the starting point
					path.MoveToPoint(bezierPoints[0]);
				}

				// Add the new curve to the path
				path.AddCurveToPoint(bezierPoints[3], bezierPoints[1], bezierPoints[2]);

				//        [partPath removeAllPoints];
				//        if ([path isEmpty])
				//        {
				//            // Set the starting point
				//            [path moveToPoint:bezierPoints[0]];
				//        }
				//// Add a new curve to the path
				//[path addCurveToPoint:bezierPoints[3]
				//controlPoint1:bezierPoints[1] controlPoint2:bezierPoints[2]];
				//        pathSize++;
				//        curveCount++;

				//        if (pathSize >= MAX_PATH_LENGTH)
				//        {
				//            [self movePathToBuffer];
				//        }

				// Setup the first points of the next curve
				bezierPoints[0] = bezierPoints[3];
				bezierPoints[1] = aNewPoint;
				nextPoint = 2;

				//        // Draw only the part that changed (with a bit of slack to handle bounds being a bit tight sometimes)

			}

			// Draw the part that changed (with a bit of slack to handle bounds being a bit tight sometimes)
			SetNeedsDisplayInRect(partPath.BoundingBox.Inset(-10, -10));

		}

		/// <summary>
		/// Add the LastPoint to the path
		/// </summary>
		/// <param name="aLastPoint">Last point.</param>
		protected void finishPoint(CGPoint aLastPoint)
		{
			if (nextPoint < 4)
			{
				// capture the final point
				bezierPoints[nextPoint] = aLastPoint;
			}
			else
			{;
				// Add the pending pointt
				addPoint(aLastPoint);
			}

			// Add this final point to the end of the current path
			if (path.IsEmpty)
			{
				// Set the starting point
				path.MoveToPoint(bezierPoints[0]);
			}

			// Add a new curve to the path
			path.AddCurveToPoint(bezierPoints[nextPoint],
								 bezierPoints[nextPoint < 1 ? nextPoint : 1],
								 bezierPoints[nextPoint < 2 ? nextPoint : 2]);

			//    pathSize++;

			// Clean upp
			partPath = null;

			//    // Now render the path to the buffer to keep the length reasonable
			//    [self movePathToBuffer];
			//}
		}

	}
}