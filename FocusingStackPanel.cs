// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;



namespace Microsoft.Kinect.Samples.KinectPaint
{
    /// <summary>
    /// A Grid customized to work like a StackPanel that assigns equal space to each child item except for the one in "focus"
    /// </summary>
    public class FocusingStackPanel : Grid
    {
        #region Properties

        /// <summary>
        /// Gets or sets the index of the focused child
        /// </summary>
        public int FocusedIndex 
        {
            get { return _focusedIndex; }
            set
            {
                if (_focusedIndex == value) return;

                _focusedIndex = value;

                ApplyFocus();
            }
        }
        private int _focusedIndex;

        /// <summary>
        /// Gets or sets the size of the focused item relative to all other items
        /// </summary>
        public double FocusedQuantity 
        {
            get { return _focusedQuantity; }
            set
            {
                if (_focusedQuantity == value) return;

                _focusedQuantity = value;

                ApplyFocus();
            }
        }
        private double _focusedQuantity;

        #endregion

        #region Internal

        protected override System.Windows.Size MeasureOverride(System.Windows.Size constraint)
        {
            if (Children.Count != ColumnDefinitions.Count)
                FixColumnDefinitions();

            return base.MeasureOverride(constraint);
        }

        // Makes changes to the width of the items so that FocusedIndex and FocusedQuantity stay in sync with what is visible
        private void ApplyFocus()
        {
            if (Children.Count != ColumnDefinitions.Count)
                FixColumnDefinitions();

            for (int i = 0; i < Children.Count; i++)
                ColumnDefinitions[i].Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star);

            ColumnDefinitions[FocusedIndex].Width = new System.Windows.GridLength(FocusedQuantity, System.Windows.GridUnitType.Star);
        }

        // If children are added/removed, the ColumnDefinitions collection can get out of sync. This fixes that.
        private void FixColumnDefinitions()
        {
            ColumnDefinitions.Clear();

            for (int i = 0; i < Children.Count; i++)
            {
                ColumnDefinitions.Add(new ColumnDefinition() { Width = new System.Windows.GridLength(1.0, System.Windows.GridUnitType.Star) });
                SetColumn(Children[i], i);
            }
        }

        #endregion
    }
}
