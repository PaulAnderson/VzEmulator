using System;
using System.Windows.Forms;

namespace VzEmulate2
{
    internal class TextboxValidation
    {
        internal void ValueTextBox_Validate(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var ctrl = sender as Control;
            if (ctrl != null)
            {
                try
                {
                    MemUtils.StringToUShort(ctrl.Text);
                }
                catch (Exception)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
