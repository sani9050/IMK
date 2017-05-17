// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace Microsoft.Kinect.Samples.KinectPaint
{
    /// <summary>
    /// Represents the different kinds of 'brushes' available to the user
    /// </summary>
    public enum KinectPaintbrush
    {
        Brush,
        Marker,
        Airbrush,
        Eraser
    }

    /// <summary>
    /// Contains information about a particular kind of brush
    /// </summary>
    public class BrushSelection
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="icon">URI of the icon representing the brush</param>
        /// <param name="iconSelected">URI of the icon representing the brush when it is selected</param>
        /// <param name="brush">The type of brush</param>
        /// <param name="friendlyName">The user-friendly name of the brush</param>
        public BrushSelection(Uri icon, Uri iconSelected, KinectPaintbrush brush, string friendlyName)
        {
            Icon = icon;
            IconSelected = iconSelected;
            Brush = brush;
            FriendlyName = friendlyName;
        }

        #region Properties

        /// <summary>
        /// URI of the icon representing the brush
        /// </summary>
        public Uri Icon { get; private set; }

        /// <summary>
        /// URI of the icon representing the brush when the tool is selected
        /// </summary>
        public Uri IconSelected { get; private set; }

        /// <summary>
        /// The type of brush
        /// </summary>
        public KinectPaintbrush Brush { get; private set; }

        /// <summary>
        /// The user-friendly name of the brush
        /// </summary>
        public string FriendlyName { get; private set; }

        #endregion
    }
}
