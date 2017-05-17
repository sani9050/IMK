// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved. 

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Diagnostics;
using Coding4Fun.Kinect.Wpf;

using Nui = Microsoft.Kinect;

namespace Microsoft.Kinect.Samples.KinectPaint
{
    /// <summary>
    /// Displays the cursor on the screen, and provides functionality allowing the cursor to interact with other elements
    /// </summary>
    public partial class KinectCursor : UserControl
    {
        #region Data

        Visual _currentlyOver;
        Storyboard _hoverStoryboard;
        bool _isHovering;
        bool _isPainting;

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public KinectCursor()
        {
            InitializeComponent();

            _hoverStoryboard = FindResource("PART_HoverStoryboard") as Storyboard;
            _hoverStoryboard.Completed += OnHoverStoryboardComplete;

            // capture the mouse once loaded, so we can make sure parts of the UI only respond to 
            // this cursor, whether it's controlled via the Kinect or the mouse. It's a Kinect app after all :)
			if(!DesignerProperties.GetIsInDesignMode(this))
			{
				Loaded += (s, e) =>
					{
						//Mouse.Capture(this, CaptureMode.SubTree);
						if (MainWindow.Instance.sensor == null)
							CompositionTarget.Rendering += (s2, e2) => OnUpdate(null, null);
						else
							MainWindow.Instance.sensor.AllFramesReady += OnUpdate;
					};
			}
        }

        #region Events

        /// <summary>
        /// Fired when a hover is completed
        /// </summary>
        public event EventHandler HoverFinished;

        /// <summary>
        /// Fired on an element when the cursor enters that element's visible boundary
        /// </summary>
        public static readonly RoutedEvent CursorEnterEvent = EventManager.RegisterRoutedEvent("CursorEnter", RoutingStrategy.Direct, typeof(EventHandler<CursorEventArgs>), typeof(DependencyObject));

        /// <summary>
        /// Fired on an element when the cursor leaves that element's visible boundary
        /// </summary>
        public static readonly RoutedEvent CursorLeaveEvent = EventManager.RegisterRoutedEvent("CursorLeave", RoutingStrategy.Direct, typeof(EventHandler<CursorEventArgs>), typeof(DependencyObject));

        /// <summary>
        /// Adds a handler for the CursorEnter event on a particular element
        /// </summary>
        /// <param name="target">The element to handle the event on</param>
        /// <param name="handler">The handler of the event</param>
        public static void AddCursorEnterHandler(DependencyObject target, EventHandler<CursorEventArgs> handler)
        {
            IInputElement ie = target as IInputElement;

            if (ie != null)
                ie.AddHandler(CursorEnterEvent, handler);
        }

        /// <summary>
        /// Removes a handler for the CursorEnter event on a particular element
        /// </summary>
        /// <param name="target">The element to handle the event on</param>
        /// <param name="handler">The handler of the event</param>
        public static void RemoveCursorEnterHandler(DependencyObject target, EventHandler<CursorEventArgs> handler)
        {
            IInputElement ie = target as IInputElement;

            if (ie != null)
                ie.RemoveHandler(CursorEnterEvent, handler);
        }

        /// <summary>
        /// Adds a handler for the CursorLeave event on a particular element
        /// </summary>
        /// <param name="target">The element to handle the event on</param>
        /// <param name="handler">The handler of the event</param>
        public static void AddCursorLeaveHandler(DependencyObject target, EventHandler<CursorEventArgs> handler)
        {
            IInputElement ie = target as IInputElement;

            if (ie != null)
                ie.AddHandler(CursorLeaveEvent, handler);
        }

        /// <summary>
        /// Removes a handler for the CursorLeave event on a particular element
        /// </summary>
        /// <param name="target">The element to handle the event on</param>
        /// <param name="handler">The handler of the event</param>
        public static void RemoveCursorLeaveHandler(DependencyObject target, EventHandler<CursorEventArgs> handler)
        {
            IInputElement ie = target as IInputElement;

            if (ie != null)
                ie.RemoveHandler(CursorLeaveEvent, handler);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Tells the KinectCursor to start a hover operation
        /// </summary>
        public void BeginHover()
        {
            Debug.Assert(!_isHovering);

            WaitCursor.Visibility = Visibility.Visible;
            DrawCursor.Visibility = Visibility.Collapsed;

            _hoverStoryboard.Begin();
            _isHovering = true;
        }

        /// <summary>
        /// Tells the Kinect to end a hover operation prematurely, if one is still taking place
        /// </summary>
        public void EndHover()
        {
            if (!_isHovering) return;

            WaitCursor.Visibility = Visibility.Collapsed;
            DrawCursor.Visibility = Visibility.Visible;

            _hoverStoryboard.Stop();
            _isHovering = false;
        }

        /// <summary>
        /// Gets the position of the cursor relative to the given element
        /// </summary>
        /// <param name="visual">The element</param>
        /// <returns>The cursor's position, in the coordinate space of the element</returns>
        public Point GetPosition(Visual visual)
        {
            if (visual == MainWindow.Instance) return CursorPosition;

            return MainWindow.Instance.TransformToDescendant(visual).Transform(CursorPosition);
        }

        /// <summary>
        /// Gets the previous position of the cursor relative to the given element
        /// </summary>
        /// <param name="visual">The element</param>
        /// <returns>The cursor's previousposition, in the coordinate space of the element</returns>
        public Point GetPreviousPosition(Visual visual)
        {
            if (visual == MainWindow.Instance) return CursorPosition;

            return MainWindow.Instance.TransformToDescendant(visual).Transform(PreviousCursorPosition);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current cursor position relative to the window's client area
        /// </summary>
        public Point CursorPosition
        {
            get { return _cursorPosition; }
            private set
            {
                _cursorPosition = value;

                Canvas.SetLeft(PART_Cursor, value.X);
                Canvas.SetTop(PART_Cursor, value.Y);
            }
        }
        private Point _cursorPosition;

        /// <summary>
        /// Gets the previous cursor position
        /// </summary>
        public Point PreviousCursorPosition { get; private set; }

        /// <summary>
        /// Gets or sets whether the cursor is currently passive. If true, CursorEnter and CursorLeave events will not fire.
        /// </summary>
        public bool Passive
        {
            get { return _passive; }
            set
            {
                if (_passive == value) return;

                _passive = value;

                UpdateElementOver();
            }
        }
        private bool _passive;

        #endregion

        #region Internal

        void OnUpdate(object sender, AllFramesReadyEventArgs e)
        {
            // KINECT Add code to get joint data and smooth it
            if (Application.Current.MainWindow == null || MainWindow.Instance == null) return;


            MainWindow.Instance.NuiRuntime_VideoFrameReady(this, e); 
           


            Nui.Skeleton skeleton = null;

            PreviousCursorPosition = CursorPosition;

            if (e == null)
            {
                CursorPosition = Mouse.GetPosition(Application.Current.MainWindow);
            }
            else
            {
                using (SkeletonFrame skeletonFrameData = e.OpenSkeletonFrame())
                {
                    if (skeletonFrameData != null)
                    {
                        Skeleton[] allSkeletons = new Skeleton[skeletonFrameData.SkeletonArrayLength];

                        skeletonFrameData.CopySkeletonDataTo(allSkeletons); 

                        foreach (Skeleton sd in allSkeletons)
                            if (sd.TrackingState == Nui.SkeletonTrackingState.Tracked)
                            {
                                skeleton = sd;
                                break;
                            }

                        if (skeleton == null)
                        {
                            return;
                        }


                        var nuiv = skeleton.Joints[JointType.HandRight].ScaleTo((int)ActualWidth, (int)ActualHeight, 0.50f, 0.30f).Position;
                        CursorPosition = new Point(nuiv.X, nuiv.Y);


                    }
                }

            }

            // Update which graphical element the cursor is currently over
            if(!Passive)
                UpdateElementOver();

            if (MainWindow.Instance.sensor == null)
            {
                // For mouse, see if the right mouse button is down.
                if (_isPainting)
                {
                    if (Mouse.LeftButton == MouseButtonState.Released)
                        MainWindow.Instance.StopPainting();
                }
                else
                {
                    if (Mouse.LeftButton == MouseButtonState.Pressed)
                        MainWindow.Instance.StartPainting();
                }

                _isPainting = Mouse.LeftButton == MouseButtonState.Pressed;
            }
            else
            {

                if (skeleton == null)
                {
                    return; 
                }
                // To begin painting w/ Kinect, raise your left hand above your shoulder.
                // To stop painting, lower it.

                Nui.Joint lh = skeleton.Joints[Nui.JointType.HandLeft];
                Nui.Joint ls = skeleton.Joints[Nui.JointType.ShoulderLeft];

                bool isup = lh.Position.Y > ls.Position.Y;

                if (isup != _isPainting)
                {
                    _isPainting = isup;

                    if (_isPainting)
                        MainWindow.Instance.StartPainting();
                    else
                        MainWindow.Instance.StopPainting();

                }
            }
        }

        // Perform hit testing and fire enter/leave events so controls can properly respond
        private void UpdateElementOver()
        {
            Visual visualOver = null;
            DependencyObject commonAncestor = null;

            if(!Passive)
                visualOver = Application.Current.MainWindow.InputHitTest(_cursorPosition) as Visual;

            if(_currentlyOver == visualOver) return;

            if (_currentlyOver != null && visualOver != null)
                commonAncestor = visualOver.FindCommonVisualAncestor(_currentlyOver);

            for (DependencyObject leaving = _currentlyOver; leaving != commonAncestor; leaving = VisualTreeHelper.GetParent(leaving))
            {
                if (!(leaving is IInputElement))
                {
                    continue;
                }

                ((IInputElement)leaving).RaiseEvent(new CursorEventArgs(this, CursorLeaveEvent, leaving));
            }

            for (DependencyObject entering = visualOver; entering != commonAncestor; entering = VisualTreeHelper.GetParent(entering))
            {
                if (!(entering is IInputElement))
                {
                    continue;
                }

                ((IInputElement)entering).RaiseEvent(new CursorEventArgs(this, CursorEnterEvent, entering));
            }

            _currentlyOver = visualOver;
        }

        // Fires when the hover animation is finished, so we can inform any listeners (like buttons) so they can do their thing.
        private void OnHoverStoryboardComplete(object sender, EventArgs e)
        {
            if (HoverFinished != null)
                HoverFinished(this, new EventArgs());

            EndHover();
            _isHovering = false;
        }

        #endregion
    }
}
