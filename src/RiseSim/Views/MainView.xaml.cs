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
		/// <summary>
		/// 起動時処理
		/// </summary>
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

        /// <summary>
		/// 終了時処理
		/// </summary>
		/// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
		{
			// ウィンドウサイズ保存
			SaveWindowBounds();

			// DataGridの列順保存
			SaveDataGridColumnIndex();

			base.OnClosing(e);
		}

		/// <summary>
		/// ウィンドウサイズ保存
		/// </summary>
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

		/// <summary>
		/// ウィンドウサイズ復元
		/// </summary>
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

		/// <summary>
		/// DataGridの列順保存
		/// </summary>
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

		/// <summary>
		/// DataGridの列順をcsv形式の文字列に変換
		/// </summary>
		/// <param name="grid">グリッドのインスタンス</param>
		/// <returns></returns>
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

		/// <summary>
		/// DataGridの列順復元
		/// </summary>
		private void RecoverDataGridColumnIndex()
		{
			var settings = Settings.Default;

			// 各DataGridに列情報を復元
			SetColumnIndexes(simulator.grid, settings.SimulatorIndexes);
			SetColumnIndexes(augmentation.grid, settings.AugmentationIndexes);
			SetColumnIndexes(myset.grid, settings.MySetIndexes);
		}

		/// <summary>
		/// 前回終了時の列順をDataGridに設定
		/// </summary>
		/// <param name="grid">対象Grid</param>
		/// <param name="indexes">列順</param>
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

		/// <summary>
		/// 標準の列順に設定する
		/// データに不備がある場合に呼び出されるメソッド
		/// </summary>
		/// <param name="grid">対象Grid</param>
		private void SetDefaultColumnIndexes(DataGrid grid)
        {
			for (int i = 0; i < grid.Columns.Count; i++)
			{
				grid.Columns[i].DisplayIndex = i;
			}
		}
	}
}
