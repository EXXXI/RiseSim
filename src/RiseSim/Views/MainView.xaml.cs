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
using RiseSim.Config;
using RiseSim.Properties;
using RiseSim.ViewModels;
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

            // DataGridの列順復元
            if (ViewConfig.Instance.UseSavedColumnIndexes)
            {
				RecoverDataGridColumnIndex();
			}
		}

        // 終了時
        protected override void OnClosing(CancelEventArgs e)
		{
			// ウィンドウサイズ保存
			SaveWindowBounds();

			// DataGridの列順保存
			SaveDataGridColumnIndex();

			base.OnClosing(e);
		}

		// ウィンドウサイズ保存
		private void SaveWindowBounds()
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
		private void RecoverWindowBounds()
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

		// DataGridの列順保存
		private void SaveDataGridColumnIndex()
		{
			var settings = Settings.Default;

			// 各DataGridの情報を保存
			settings.SimulatorIndexes = GetColumnIndexes(simulator.grid);
			settings.AugmentationIndexes = GetColumnIndexes(augmentation.grid);
			settings.MySetIndexes = GetColumnIndexes(myset.grid);

			// 保存
			settings.Save();
		}

		// DataGridの列順をcsv形式の文字列に変換
		private string GetColumnIndexes(DataGrid grid)
        {
			bool isFirst = true;
			StringBuilder indexes = new();
			foreach (var column in grid.Columns)
            {
				if (isFirst)
                {
					isFirst = false;
                }
                else
                {
					indexes.Append(',');
				}
				indexes.Append(column.DisplayIndex);
			}
			return indexes.ToString();
        }
		// DataGridの列順復元
		private void RecoverDataGridColumnIndex()
		{
			var settings = Settings.Default;

			// 各DataGridに列情報を復元
			SetColumnIndexes(simulator.grid, settings.SimulatorIndexes);
			SetColumnIndexes(augmentation.grid, settings.AugmentationIndexes);
			SetColumnIndexes(myset.grid, settings.MySetIndexes);
		}

		// 前回終了時の列順をDataGridに設定
		private void SetColumnIndexes(DataGrid grid, string indexes)
		{
			if (string.IsNullOrWhiteSpace(indexes))
			{
				return;
			}

			string[] indexArray = indexes.Split(',');

			try
			{
				for (int i = 0; i < grid.Columns.Count; i++)
				{
					int index = int.Parse(indexArray[i]);
                    if (index < 0 || index >= grid.Columns.Count)
                    {
						SetDefaultColumnIndexes(grid);
					}

					grid.Columns[i].DisplayIndex = index;
				}
			}
			catch (Exception)
			{
				SetDefaultColumnIndexes(grid);
			}
		}

		private void SetDefaultColumnIndexes(DataGrid grid)
        {
			for (int i = 0; i < grid.Columns.Count; i++)
			{
				grid.Columns[i].DisplayIndex = i;
			}
		}
	}
}
