/*    RiseSim : MHRise skill simurator for Windows
 *    Copyright (C) 2022  EXXXI
 *
 *    This program is free software: you can redistribute it and/or modify
 *    it under the terms of the GNU General Public License as published by
 *    the Free Software Foundation, either version 3 of the License, or
 *    (at your option) any later version.
 *
 *    This program is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 *    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *    GNU General Public License for more details.
 *
 *    You should have received a copy of the GNU General Public License
 *    along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
using RiseSim.Properties;
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
using System.Windows.Shapes;

namespace RiseSim.Views
{
    /// <summary>
    /// MainView.xaml の相互作用ロジック
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();

			// ウィンドウサイズ復元
			RecoverWindowBounds();
		}

		// 終了時
		protected override void OnClosing(CancelEventArgs e)
		{
			// ウィンドウサイズ保存
			SaveWindowBounds();

			base.OnClosing(e);
		}

		// ウィンドウサイズ保存
		void SaveWindowBounds()
		{
			var settings = Settings.Default;
			if (WindowState == WindowState.Maximized)
            {
				settings.WindowMaximized = true;
            }
            else
            {
				settings.WindowMaximized = false;
			}

			// サイズを保存するため、一旦最大化を解除
			WindowState = WindowState.Normal;
			settings.WindowLeft = Left;
			settings.WindowTop = Top;
			settings.WindowWidth = Width;
			settings.WindowHeight = Height;

			// 保存
			settings.Save();
		}

		// ウィンドウサイズ復元
		void RecoverWindowBounds()
		{
			var settings = Settings.Default;
			// 左
			if (settings.WindowLeft >= 0 &&
				(settings.WindowLeft + settings.WindowWidth) < SystemParameters.VirtualScreenWidth)
			{ Left = settings.WindowLeft; }
			// 上
			if (settings.WindowTop >= 0 &&
				(settings.WindowTop + settings.WindowHeight) < SystemParameters.VirtualScreenHeight)
			{ Top = settings.WindowTop; }
			// 幅
			if (settings.WindowWidth > 0 &&
				settings.WindowWidth <= SystemParameters.WorkArea.Width)
			{ Width = settings.WindowWidth; }
			// 高さ
			if (settings.WindowHeight > 0 &&
				settings.WindowHeight <= SystemParameters.WorkArea.Height)
			{ Height = settings.WindowHeight; }
			// 最大化
			if (settings.WindowMaximized)
			{
				// ロード後に最大化
				Loaded += (o, e) => WindowState = WindowState.Maximized;
			}
		}
	}
}
