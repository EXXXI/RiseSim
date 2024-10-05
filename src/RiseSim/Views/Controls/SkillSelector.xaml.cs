using DotNetKit.Windows.Controls;
using RiseSim.ViewModels.Controls;
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
    /// SkillSelector.xaml の相互作用ロジック
    /// </summary>
    public partial class SkillSelector : UserControl
    {
        public SkillSelector()
        {
            InitializeComponent();

            combobox.GotFocus += (obj, args) => {
                placeholder.Visibility = Visibility.Hidden;
            };
            combobox.LostFocus += (obj, args) => {
                bool isEmpty = string.IsNullOrEmpty(combobox.Text);
                if (isEmpty)
                {
                    placeholder.Visibility = Visibility.Visible;
                }
                else
                {
                    placeholder.Visibility = Visibility.Hidden;
                }
            };
            combobox.SelectionChanged += (obj, args) =>
            {
                if (!combobox.EditableTextBox.IsFocused)
                {
                    if (args.AddedItems.Count > 0)
                    {
                        placeholder.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        var x = combobox.EditableTextBox.Text;
                        placeholder.Visibility = Visibility.Visible;
                    }
                }
            };

        }
    }
}
