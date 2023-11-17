using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MrSquash.Controls
{
    public class ReservationControl : Control
    {
        private Border _button;

        private const string ButtonPartName = "PART_ActiveBorder";

        public static readonly DependencyProperty IsReservedProperty =
            DependencyProperty.Register(nameof(IsReserved), typeof(bool), typeof(ReservationControl), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(ReservationControl));

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(ReservationControl));

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(ReservationControl),
                new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty SelectedBorderBrushProperty =
            DependencyProperty.Register(nameof(SelectedBorderBrush), typeof(Brush), typeof(ReservationControl));

        public static readonly DependencyProperty SelectedBorderThicknessProperty =
            DependencyProperty.Register(nameof(SelectedBorderThickness), typeof(Thickness), typeof(ReservationControl), new PropertyMetadata(new Thickness(1)));

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(ReservationControl));

        public static readonly DependencyProperty ReservedBackgroundProperty =
            DependencyProperty.Register(nameof(ReservedBackground), typeof(Brush), typeof(ReservationControl));

        public static readonly DependencyProperty ReservedBorderBrushProperty =
            DependencyProperty.Register(nameof(ReservedBorderBrush), typeof(Brush), typeof(ReservationControl));

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(ReservationControl), new PropertyMetadata(new CornerRadius(0)));

        public static readonly RoutedEvent CheckedClickEvent =
            EventManager.RegisterRoutedEvent(nameof(CheckClick), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ReservationControl));

        [Category("Behavior")]
        public event RoutedEventHandler CheckClick { add { AddHandler(CheckedClickEvent, value); } remove { RemoveHandler(CheckedClickEvent, value); } }

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public bool IsReserved
        {
            get => (bool)GetValue(IsReservedProperty);
            set => SetValue(IsReservedProperty, value);
        }

        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public Brush SelectedBorderBrush
        {
            get => (Brush)GetValue(SelectedBorderBrushProperty);
            set => SetValue(SelectedBorderBrushProperty, value);
        }

        public Thickness SelectedBorderThickness
        {
            get => (Thickness)GetValue(SelectedBorderThicknessProperty);
            set => SetValue(SelectedBorderThicknessProperty, value);
        }

        public Brush ReservedBackground
        {
            get => (Brush)GetValue(ReservedBackgroundProperty);
            set => SetValue(ReservedBackgroundProperty, value);
        }

        public Brush ReservedBorderBrush
        {
            get => (Brush)GetValue(ReservedBorderBrushProperty);
            set => SetValue(ReservedBorderBrushProperty, value);
        }

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        static ReservationControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ReservationControl), new FrameworkPropertyMetadata(typeof(ReservationControl)));
        }

        public override void OnApplyTemplate()
        {
            if (_button != null)
            {
                _button.MouseLeftButtonDown -= OnLeftClick;
                _button.MouseRightButtonDown -= OnRightClick;
            }

            _button = GetTemplateChild(ButtonPartName) as Border;
            _button.MouseLeftButtonDown += OnLeftClick;
            _button.MouseRightButtonDown += OnRightClick;

            base.OnApplyTemplate();
        }

        private void OnLeftClick(object sender, MouseButtonEventArgs e)
        {
            var newEvent = new RoutedEventArgs(CheckedClickEvent, this);
            RaiseEvent(newEvent);

            if (this.Command != null)
            {
                if (this.Command.CanExecute(CommandParameter))
                    this.Command.Execute(CommandParameter);
            }
        }

        private void OnRightClick(object sender, MouseButtonEventArgs e)
        {
            this.IsSelected = !this.IsSelected;
        }
    }
}
