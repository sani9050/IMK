// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Automation.Provider;
using System.Windows.Automation.Peers;




namespace Microsoft.Kinect.Samples.KinectPaint
{
    /// <summary>
    /// Extends a standard checkbox with the ability to respond to Kinect cursor hover-over events
    /// </summary>
    public class KinectPaintCheckBox : CheckBox
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public KinectPaintCheckBox()
        {
            KinectCursor.AddCursorEnterHandler(this, OnCursorEnter);
            KinectCursor.AddCursorLeaveHandler(this, OnCursorLeave);
        }

        #region Internal

        // Called when the cursor enters this checkbox's visible area
        private void OnCursorEnter(object sender, CursorEventArgs args)
        {
            args.Cursor.BeginHover();

            args.Cursor.HoverFinished += Cursor_HoverFinished;
        }

        // Called when the cursor leaves this checkbox's visible area
        private void OnCursorLeave(object sender, CursorEventArgs args)
        {
            args.Cursor.EndHover();

            args.Cursor.HoverFinished -= Cursor_HoverFinished;
        }

        // Called when the hover action has finished, and the button should be pressed
        private void Cursor_HoverFinished(object sender, EventArgs e)
        {
            IsChecked = !IsChecked;

            ((KinectCursor)sender).HoverFinished -= Cursor_HoverFinished;
        }

        #endregion
    }
}
