using RiseSim.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace RiseSim.Views.Controls
{
    /// <summary>
    /// RowDraggableDataGrid.xaml の相互作用ロジック
    /// 行をドラッグした際に、どの行がどこへドラッグされたかをイベントで通知する
    /// </summary>
    public partial class RowDraggableDataGrid : DataGrid
    {
        /// <summary>
        /// ドラッグ中に触れるとスクロールする領域の幅
        /// </summary>
        private const int ScrollAreaHeight = 30;

        /// <summary>
        /// ドラッグ中か否か
        /// </summary>
        private bool IsRowDragging { get; set; }

        /// <summary>
        /// ドラッグ中の項目
        /// </summary>
        private ChildViewModelBase? DraggedItem { get; set; }

        /// <summary>
        /// ドラッグを通知するコマンド(公開用プロパティ)
        /// </summary>
        public ICommand RowChangedCommand
        {
            get { return (ICommand)GetValue(RowChangedCommandProperty); }
            set { SetValue(RowChangedCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RowChangedCommand.  This enables animation, styling, binding, etc...
        /// <summary>
        /// ドラッグを通知するコマンド(本体)
        /// </summary>
        public static readonly DependencyProperty RowChangedCommandProperty =
            DependencyProperty.Register("RowChangedCommand", typeof(ICommand), typeof(RowDraggableDataGrid));

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public RowDraggableDataGrid()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ソート時の挙動をオーバーライド
        /// 昇順降順のあとにソート解除を挟む
        /// </summary>
        /// <param name="eventArgs"></param>
        protected override void OnSorting(DataGridSortingEventArgs eventArgs)
        {
            if (eventArgs.Column.SortDirection == ListSortDirection.Descending)
            {
                eventArgs.Handled = true;
                eventArgs.Column.SortDirection = null;
                Items.SortDescriptions.Clear();
                return;
            }
            base.OnSorting(eventArgs);
        }

        /// <summary>
        /// ドラッグ用
        /// 左クリック時にドラッグ開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // ソート中は禁止
            if (Items.SortDescriptions.Count > 0)
            {
                return;
            }
            if (e.OriginalSource is not DependencyObject source)
            {
                return;
            }
            if (FindVisualParent<DataGridRow>(source)?.Item is ChildViewModelBase item)
            {
                DraggedItem = item;
                IsRowDragging = true;
            }
        }

        /// <summary>
        /// ドラッグせずに左ボタンを離した時
        /// ドラッグ用情報を破棄する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            EndDrag();
        }

        /// <summary>
        /// ドラッグ中の表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGrid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!IsRowDragging)
            {
                return;
            }
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(draggableGrid, DraggedItem, DragDropEffects.Move);
            }
        }

        /// <summary>
        /// ドラッグ中のスクロール処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGrid_DragOver(object sender, DragEventArgs e)
        {
            if (!IsRowDragging)
            {
                return;
            }

            // スクロール
            var scrollViewer = FindVisualChild<ScrollViewer>(draggableGrid);
            if (scrollViewer != null)
            {
                // 上下ともにScrollAreaHeightの範囲に入ったらスクロール
                if (e.GetPosition(draggableGrid).Y < ScrollAreaHeight)
                {
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - 1);
                }
                else if (e.GetPosition(draggableGrid).Y > draggableGrid.ActualHeight - ScrollAreaHeight)
                {
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + 1);
                }
            }
        }

        /// <summary>
        /// ドラッグ終了時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGrid_Drop(object sender, DragEventArgs e)
        {
            if (!IsRowDragging)
            {
                return;
            }
            if (e.OriginalSource is not DependencyObject targetSource)
            {
                return;
            }
            
            // ドラッグ先を取得
            DataGridRow? targetRow = FindVisualParent<DataGridRow>(targetSource);
            DataGridColumnHeader? targetHeader = FindVisualParent<DataGridColumnHeader>(targetSource);

            int targetIndex;
            if (targetRow != null)
            {
                // 行が取得できた場合、ドラッグ先の行番号を取得
                var targetItem = targetRow.Item as ChildViewModelBase;
                targetIndex = Items.IndexOf(targetItem);
            }
            else if (targetHeader != null)
            {
                // ヘッダーが取得できた場合、0行目をドラッグ先の行番号とする
                targetIndex = 0;
            }
            else
            {
                // 上記以外は余白部分へのドラッグと判断し、最終行をドラッグ先の行番号とする
                targetIndex = Items.Count - 1;
            }

            // ドロップ先の行番号が取得され、ドロップ元の行番号と違う場合、それぞれの行番号をパラメータとしてコマンドを実行
            int dropIndex = Items.IndexOf(DraggedItem);
            if (targetIndex != -1 && targetIndex != dropIndex)
            {
                RowChangedCommand?.Execute((dropIndex, targetIndex));
            }

            // ドラッグ用情報の破棄
            EndDrag();
        }

        /// <summary>
        /// 親要素からジェネリック指定の型のコントロールを取得
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="child"></param>
        /// <returns></returns>
        private T? FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject? parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null)
            {
                return null;
            }
            if (parentObject is T parent)
            {
                return parent;
            }
            else
            {
                return FindVisualParent<T>(parentObject);
            }
        }

        /// <summary>
        /// 子要素からジェネリック指定の型のコントロールを取得
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent"></param>
        /// <returns></returns>
        private T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject? child = VisualTreeHelper.GetChild(parent, i);
                if (child != null)
                {
                    if (child is T found)
                    {
                        return found;
                    }
                    else
                    {
                        T? childOfChild = FindVisualChild<T>(child);
                        if (childOfChild != null)
                            return childOfChild;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// ドラッグ用情報の破棄
        /// </summary>
        private void EndDrag()
        {
            IsRowDragging = false;
            DraggedItem = null;
        }
    }
}
