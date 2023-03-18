using System;
using System.Collections.Generic;
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

namespace RiseSim.Views.Controls
{
    /// <summary>
    /// FocusTextBox.xaml の相互作用ロジック
    /// </summary>
    public partial class FocusTextBox : TextBox
    {
        public FocusTextBox()
        {
            InitializeComponent();
            this.GotFocus += (s, e) =>
            {
                this.SelectAll();
            };
            this.PreviewMouseLeftButtonDown += (s, e) =>
            {
                if (IsFocused)
                {
                    return;
                }
                e.Handled = true;
                this.Focus();
            };
        }

    }
}
