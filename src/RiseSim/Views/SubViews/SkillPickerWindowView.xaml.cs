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
using System.ComponentModel;
using System.Windows;

namespace RiseSim.Views.SubViews
{
    /// <summary>
    /// SkillPickerWindowView.xaml の相互作用ロジック
    /// </summary>
    public partial class SkillPickerWindowView : Window
    {
        public SkillPickerWindowView()
        {
            InitializeComponent();
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
		private void SaveWindowBounds()
		{
			var settings = Settings.Default;
			if (WindowState == WindowState.Maximized)
			{
				settings.PickerMaximized = true;
			}
			else
			{
				settings.PickerMaximized = false;
			}

			// サイズを保存するため、一旦最大化を解除
			WindowState = WindowState.Normal;
			settings.PickerLeft = Left;
			settings.PickerTop = Top;
			settings.PickerWidth = Width;
			settings.PickerHeight = Height;

			// 保存
			settings.Save();
		}

		// ウィンドウサイズ復元
		private void RecoverWindowBounds()
		{
			var settings = Settings.Default;
			// 左
			if (settings.PickerLeft >= 0 &&
				(settings.PickerLeft + settings.PickerWidth) < SystemParameters.VirtualScreenWidth)
			{ Left = settings.PickerLeft; }
			// 上
			if (settings.PickerTop >= 0 &&
				(settings.PickerTop + settings.PickerHeight) < SystemParameters.VirtualScreenHeight)
			{ Top = settings.PickerTop; }
			// 幅
			if (settings.PickerWidth > 0 &&
				settings.PickerWidth <= SystemParameters.WorkArea.Width)
			{ Width = settings.PickerWidth; }
			// 高さ
			if (settings.PickerHeight > 0 &&
				settings.PickerHeight <= SystemParameters.WorkArea.Height)
			{ Height = settings.PickerHeight; }
			// 最大化
			if (settings.PickerMaximized)
			{
				// ロード後に最大化
				Loaded += (o, e) => WindowState = WindowState.Maximized;
			}
		}
    }
}
