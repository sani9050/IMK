// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved. 

using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Kinect.Samples.KinectPaint
{
    /// <summary>
    /// Interaction logic for Tutorial.xaml
    /// </summary>
    public partial class Tutorial : UserControl
    {
        public Tutorial()
        {
            InitializeComponent();
        }

        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }
    }
}
