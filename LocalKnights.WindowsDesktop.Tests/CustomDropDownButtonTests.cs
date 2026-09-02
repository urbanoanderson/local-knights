using System.Runtime.ExceptionServices;
using System.Threading;
using LocalKnights.WindowsDesktop.Controls;
using LocalKnights.WindowsDesktop.Helpers;

namespace LocalKnights.WindowsDesktop.Tests;

/// <summary>
/// Tests the reusable dropdown button behavior.
/// </summary>
public sealed class CustomDropDownButtonTests
{
    /// <summary>
    /// Verifies that activating an item executes its command and closes the dropdown.
    /// </summary>
    [Fact]
    public void ItemClick_ExecutesCommandAndClosesDropDown()
    {
        Exception? threadException = null;
        Thread thread = new Thread(() =>
        {
            try
            {
                bool commandExecuted = false;
                CustomDropDownButton dropDownButton = new CustomDropDownButton
                {
                    IsDropDownOpen = true,
                };
                TestDropDownButtonItem item = new TestDropDownButtonItem
                {
                    Command = new RelayCommand(_ => commandExecuted = true),
                };

                dropDownButton.Items.Add(item);
                item.InvokeClick();

                Assert.True(commandExecuted);
                Assert.False(dropDownButton.IsDropDownOpen);
            }
            catch (Exception ex)
            {
                threadException = ex;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (threadException != null)
        {
            ExceptionDispatchInfo.Capture(threadException).Throw();
        }
    }

    private sealed class TestDropDownButtonItem : CustomDropDownButtonItem
    {
        public void InvokeClick()
        {
            OnClick();
        }
    }
}