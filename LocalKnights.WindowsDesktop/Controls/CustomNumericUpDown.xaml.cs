using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LocalKnights.WindowsDesktop.Controls
{
    /// <summary>
    /// A reusable numeric input control with up/down buttons and direct text editing.
    /// Supports minimum, maximum, increment, and decimal precision configuration.
    /// </summary>
    public partial class CustomNumericUpDown : UserControl, INotifyPropertyChanged
    {
        private bool _updatingText;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomNumericUpDown"/> class.
        /// </summary>
        public CustomNumericUpDown()
        {
            InitializeComponent();
            DisplayValue = FormatValue(Value);
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Identifies the Value dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(CustomNumericUpDown),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged, CoerceValue));

        /// <summary>
        /// Identifies the Minimum dependency property.
        /// </summary>
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            nameof(Minimum),
            typeof(double),
            typeof(CustomNumericUpDown),
            new PropertyMetadata(0.0, OnRangePropertyChanged));

        /// <summary>
        /// Identifies the Maximum dependency property.
        /// </summary>
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            nameof(Maximum),
            typeof(double),
            typeof(CustomNumericUpDown),
            new PropertyMetadata(100.0, OnRangePropertyChanged));

        /// <summary>
        /// Identifies the Increment dependency property.
        /// </summary>
        public static readonly DependencyProperty IncrementProperty = DependencyProperty.Register(
            nameof(Increment),
            typeof(double),
            typeof(CustomNumericUpDown),
            new PropertyMetadata(1.0));

        /// <summary>
        /// Identifies the DecimalPlaces dependency property.
        /// </summary>
        public static readonly DependencyProperty DecimalPlacesProperty = DependencyProperty.Register(
            nameof(DecimalPlaces),
            typeof(int),
            typeof(CustomNumericUpDown),
            new PropertyMetadata(0, OnDecimalPlacesPropertyChanged));

        /// <summary>
        /// Identifies the DisplayValue dependency property.
        /// </summary>
        public static readonly DependencyProperty DisplayValueProperty = DependencyProperty.Register(
            nameof(DisplayValue),
            typeof(string),
            typeof(CustomNumericUpDown),
            new FrameworkPropertyMetadata("0", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnDisplayValueChanged));

        /// <summary>
        /// Gets or sets the current value of the control.
        /// </summary>
        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        /// <summary>
        /// Gets or sets the minimum allowed value.
        /// </summary>
        public double Minimum
        {
            get => (double)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        /// <summary>
        /// Gets or sets the maximum allowed value.
        /// </summary>
        public double Maximum
        {
            get => (double)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        /// <summary>
        /// Gets or sets the increment used by the up and down buttons.
        /// </summary>
        public double Increment
        {
            get => (double)GetValue(IncrementProperty);
            set => SetValue(IncrementProperty, value);
        }

        /// <summary>
        /// Gets or sets the number of decimal places displayed.
        /// </summary>
        public int DecimalPlaces
        {
            get => (int)GetValue(DecimalPlacesProperty);
            set => SetValue(DecimalPlacesProperty, value);
        }

        /// <summary>
        /// Gets or sets the text displayed in the inner text box.
        /// </summary>
        public string DisplayValue
        {
            get => (string)GetValue(DisplayValueProperty);
            set => SetValue(DisplayValueProperty, value);
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CustomNumericUpDown control)
            {
                control.UpdateDisplayValue();
            }
        }

        private static object CoerceValue(DependencyObject d, object baseValue)
        {
            if (d is CustomNumericUpDown control && baseValue is double value)
            {
                if (value < control.Minimum)
                {
                    return control.Minimum;
                }

                if (value > control.Maximum)
                {
                    return control.Maximum;
                }

                return Math.Round(value, control.DecimalPlaces);
            }

            return baseValue;
        }

        private static void OnRangePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CustomNumericUpDown control)
            {
                control.CoerceValue(ValueProperty);
                control.UpdateDisplayValue();
            }
        }

        private static void OnDecimalPlacesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CustomNumericUpDown control)
            {
                control.CoerceValue(ValueProperty);
                control.UpdateDisplayValue();
            }
        }

        private static void OnDisplayValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CustomNumericUpDown control && !control._updatingText)
            {
                if (control.ValueTextBox?.IsFocused != true)
                {
                    control.CommitDisplayValue();
                }
            }
        }

        private void UpdateDisplayValue()
        {
            _updatingText = true;
            DisplayValue = FormatValue(Value);
            _updatingText = false;
        }

        private string FormatValue(double value)
        {
            return value.ToString(GetFormatString(), CultureInfo.CurrentCulture);
        }

        private string GetFormatString()
        {
            return DecimalPlaces > 0 ? "F" + DecimalPlaces : "F0";
        }

        private void CommitDisplayValue()
        {
            if (_updatingText)
            {
                return;
            }

            if (double.TryParse(DisplayValue, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out double parsedValue))
            {
                parsedValue = Math.Round(parsedValue, DecimalPlaces);
                if (parsedValue < Minimum)
                {
                    parsedValue = Minimum;
                }

                if (parsedValue > Maximum)
                {
                    parsedValue = Maximum;
                }

                Value = parsedValue;
                UpdateDisplayValue();
            }
            else
            {
                UpdateDisplayValue();
            }
        }

        private void SetValue(double newValue)
        {
            Value = Math.Round(Math.Max(Minimum, Math.Min(Maximum, newValue)), DecimalPlaces);
        }

        private void IncrementButton_Click(object sender, RoutedEventArgs e)
        {
            SetValue(Value + Increment);
        }

        private void DecrementButton_Click(object sender, RoutedEventArgs e)
        {
            SetValue(Value - Increment);
        }

        private void ValueTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            CommitDisplayValue();
        }

        private void ValueTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            string currentText = ValueTextBox.Text;
            int selectionStart = ValueTextBox.SelectionStart;
            int selectionLength = ValueTextBox.SelectionLength;
            string newText = currentText.Remove(selectionStart, selectionLength).Insert(selectionStart, e.Text);
            e.Handled = !IsTextValid(newText);
        }

        private void ValueTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                SetValue(Value + Increment);
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                SetValue(Value - Increment);
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                CommitDisplayValue();
                e.Handled = true;
            }
        }

        private bool IsTextValid(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return true;
            }

            NumberFormatInfo numberFormat = CultureInfo.CurrentCulture.NumberFormat;
            string decimalSeparator = numberFormat.NumberDecimalSeparator;
            string negativeSign = numberFormat.NegativeSign;

            if (text == negativeSign)
            {
                return Minimum < 0;
            }

            if (text.EndsWith(decimalSeparator, StringComparison.CurrentCulture))
            {
                text = text.TrimEnd(decimalSeparator.ToCharArray());
            }

            return double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out _);
        }

        /// <summary>
        /// Raises the PropertyChanged event for the specified property name.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
