using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MrSquash.Controls
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:MrSquash.Controls"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:MrSquash.Controls;assembly=MrSquash.Controls"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:CustomControl1/>
    ///
    /// </summary>
    public class ReservationControl : Control
    {
        private Border _button;

        private const string ButtonPartName = "PART_ActiveBorder";

        public static readonly DependencyProperty IsBusyProperty =
            DependencyProperty.Register(nameof(IsBusy), typeof(bool), typeof(ReservationControl), new PropertyMetadata(default(bool)));

        public bool IsBusy
        {
            get => (bool)GetValue(IsBusyProperty);
            set => SetValue(IsBusyProperty, value);
        }

        public static readonly DependencyProperty WatchingChangedCommandProperty =
            DependencyProperty.Register("WatchingChangedCommand", typeof(ICommand), typeof(ReservationControl));

        public ICommand WatchingChangedCommand
        {
            get => (ICommand)GetValue(WatchingChangedCommandProperty);
            set => SetValue(WatchingChangedCommandProperty, value);
        }

        public static readonly DependencyProperty WatchingChangedCommandParameterProperty =
            DependencyProperty.Register("WatchingChangedCommandParameter", typeof(object), typeof(ReservationControl));

        public object WatchingChangedCommandParameter
        {
            get => GetValue(WatchingChangedCommandParameterProperty);
            set => SetValue(WatchingChangedCommandParameterProperty, value);
        }

        public static readonly DependencyProperty IsWatchingProperty =
            DependencyProperty.Register(nameof(IsWatching), typeof(bool), typeof(ReservationControl),
                new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public bool IsWatching
        {
            get => (bool)GetValue(IsWatchingProperty);
            set => SetValue(IsWatchingProperty, value);
        }

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(ReservationControl));

        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        public static readonly DependencyProperty BusyBackgroundProperty =
            DependencyProperty.Register(nameof(BusyBackground), typeof(Brush), typeof(ReservationControl));

        public Brush BusyBackground
        {
            get => (Brush)GetValue(BusyBackgroundProperty);
            set => SetValue(BusyBackgroundProperty, value);
        }

        public static readonly DependencyProperty BusyBorderBrushProperty =
            DependencyProperty.Register(nameof(BusyBorderBrush), typeof(Brush), typeof(ReservationControl));

        public Brush BusyBorderBrush
        {
            get => (Brush)GetValue(BusyBorderBrushProperty);
            set => SetValue(BusyBorderBrushProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(ReservationControl), new PropertyMetadata(new CornerRadius(0)));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static readonly RoutedEvent CheckedClickEvent =
            EventManager.RegisterRoutedEvent("CheckClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ReservationControl));

        [Category("Behavior")]
        public event RoutedEventHandler CheckClick { add { AddHandler(CheckedClickEvent, value); } remove { RemoveHandler(CheckedClickEvent, value); } }

        static ReservationControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ReservationControl), new FrameworkPropertyMetadata(typeof(ReservationControl)));
        }

        public override void OnApplyTemplate()
        {
            if (_button != null)
                _button.MouseLeftButtonDown -= OnLeftClick;

            _button = GetTemplateChild(ButtonPartName) as Border;
            if (_button != null)
                _button.MouseLeftButtonDown += OnLeftClick;

            base.OnApplyTemplate();
        }

        private void OnLeftClick(object sender, MouseButtonEventArgs e)
        {
            this.IsWatching = !this.IsWatching;

            var newEvent = new RoutedEventArgs(CheckedClickEvent, this);
            RaiseEvent(newEvent);

            if (this.WatchingChangedCommand != null)
            {
                if (this.WatchingChangedCommand.CanExecute(WatchingChangedCommandParameter))
                    this.WatchingChangedCommand.Execute(WatchingChangedCommandParameter);
            }
        }
    }
}
