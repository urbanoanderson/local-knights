using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace LocalKnights.WindowsDesktop.Controls
{
    /// <summary>
    /// Displays a button that opens a customizable list of command items.
    /// </summary>
    public class CustomDropDownButton : HeaderedItemsControl
    {
        private const string ButtonPartName = "PART_Button";
        private ButtonBase? _button;

        static CustomDropDownButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(CustomDropDownButton),
                new FrameworkPropertyMetadata(typeof(CustomDropDownButton)));
        }

        /// <summary>
        /// Identifies the <see cref="IsDropDownOpen"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register(
            nameof(IsDropDownOpen),
            typeof(bool),
            typeof(CustomDropDownButton),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// Identifies the <see cref="ButtonStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ButtonStyleProperty = DependencyProperty.Register(
            nameof(ButtonStyle),
            typeof(Style),
            typeof(CustomDropDownButton),
            new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets whether the dropdown menu is open.
        /// </summary>
        public bool IsDropDownOpen
        {
            get => (bool)GetValue(IsDropDownOpenProperty);
            set => SetValue(IsDropDownOpenProperty, value);
        }

        /// <summary>
        /// Gets or sets the style applied to the main button.
        /// </summary>
        public Style? ButtonStyle
        {
            get => (Style?)GetValue(ButtonStyleProperty);
            set => SetValue(ButtonStyleProperty, value);
        }

        /// <inheritdoc/>
        public override void OnApplyTemplate()
        {
            if (_button != null)
            {
                _button.Click -= Button_Click;
            }

            base.OnApplyTemplate();

            _button = GetTemplateChild(ButtonPartName) as ButtonBase;
            if (_button != null)
            {
                _button.Click += Button_Click;
            }
        }

        /// <inheritdoc/>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new CustomDropDownButtonItem();
        }

        /// <inheritdoc/>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is CustomDropDownButtonItem;
        }

        /// <inheritdoc/>
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (object oldItem in e.OldItems)
                {
                    if (oldItem is CustomDropDownButtonItem dropDownItem && dropDownItem.Owner == this)
                    {
                        dropDownItem.Owner = null;
                    }
                }
            }

            base.OnItemsChanged(e);

            if (e.NewItems != null)
            {
                foreach (object newItem in e.NewItems)
                {
                    if (newItem is CustomDropDownButtonItem dropDownItem)
                    {
                        dropDownItem.Owner = this;
                    }
                }
            }
        }

        /// <inheritdoc/>
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            if (element is CustomDropDownButtonItem dropDownItem)
            {
                dropDownItem.Owner = this;
            }
        }

        /// <inheritdoc/>
        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            if (element is CustomDropDownButtonItem dropDownItem)
            {
                dropDownItem.Owner = null;
            }

            base.ClearContainerForItemOverride(element, item);
        }

        /// <inheritdoc/>
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape && IsDropDownOpen)
            {
                IsDropDownOpen = false;
                e.Handled = true;
                return;
            }

            if ((e.Key == Key.Down || e.Key == Key.Enter || e.Key == Key.Space) && !IsDropDownOpen)
            {
                IsDropDownOpen = true;
                e.Handled = true;
                return;
            }

            base.OnPreviewKeyDown(e);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            IsDropDownOpen = !IsDropDownOpen;
        }
    }

    /// <summary>
    /// Represents a clickable item displayed by a <see cref="CustomDropDownButton"/>.
    /// </summary>
    public class CustomDropDownButtonItem : Button
    {
        internal CustomDropDownButton? Owner { get; set; }

        static CustomDropDownButtonItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(CustomDropDownButtonItem),
                new FrameworkPropertyMetadata(typeof(CustomDropDownButtonItem)));
        }

        /// <inheritdoc/>
        protected override void OnClick()
        {
            base.OnClick();
            Owner?.SetCurrentValue(CustomDropDownButton.IsDropDownOpenProperty, false);
        }
    }
}