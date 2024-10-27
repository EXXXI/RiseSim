using RiseSim.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// RowDraggableDataGrid.xaml の相互作用ロジック
    /// </summary>
    public partial class RowDraggableDataGrid : DataGrid
    {

        private bool IsRowDragging { get; set; }
        private ChildViewModelBase DraggedItem { get; set; }

        Point MyPoint { get; set; }



        public ICommand RowChangedCommand
        {
            get { return (ICommand)GetValue(RowChangedCommandProperty); }
            set { SetValue(RowChangedCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RowChangedCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RowChangedCommandProperty =
            DependencyProperty.Register("RowChangedCommand", typeof(ICommand), typeof(RowDraggableDataGrid));



        //public ICommand RowChangedCommand
        //{
        //    get { return (ICommand)GetValue(RowChangedProperty); }
        //    set { SetValue(RowChangedProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for RowChangedCommand.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty RowChangedProperty =
        //    DependencyProperty.Register("RowChangedCommand", typeof(ICommand), typeof(RowDraggableDataGrid), new PropertyMetadata(0));

        private void DataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IsRowDragging = true;
            //クリック位置取得
            MyPoint = e.GetPosition(this);
            var row = FindVisualParent<DataGridRow>(e.OriginalSource as DependencyObject);
            if (row != null)
            {
                DraggedItem = row.Item as ChildViewModelBase;
            }
        }

        private void DataGrid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (IsRowDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(draggableGrid, DraggedItem, DragDropEffects.Move);
            }

        }

        private void DataGrid_DragOver(object sender, DragEventArgs e)
        {
            if (IsRowDragging)
            {
                // ドラッグ中にリアルタイムでスクロール
                var scrollViewer = FindVisualChild<ScrollViewer>(draggableGrid);
                if (scrollViewer != null)
                {
                    //今のマウスの座標
                    var mouseP = e.GetPosition(this);

                    //スクロール位置の指定
                    if (MyPoint.Y < mouseP.Y)
                    {
                        scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + 1);
                    }
                    else
                    {
                        scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - 1);
                    }
                }
            }
        }

        private void DataGrid_Drop(object sender, DragEventArgs e)
        {
            //if (e.Data.GetDataPresent(typeof(object)))
            //{
            var target = FindVisualParent<DataGridRow>(e.OriginalSource as DependencyObject);
            if (target != null)
            {
                var targetItem = target.Item as ChildViewModelBase;
                int targetIndex = Items.IndexOf(targetItem);
                if (targetItem != null)
                {
                    int dropIndex = Items.IndexOf(DraggedItem);

                    if (e.GetPosition(draggableGrid).Y < 0)
                    {
                        // ドロップ先が DataGrid の上にある場合、一番上に挿入
                        targetIndex = 0;
                    }
                    else if (e.GetPosition(draggableGrid).Y > draggableGrid.ActualHeight)
                    {
                        // ドロップ先が DataGrid の下にある場合、一番下に挿入
                        targetIndex = Items.Count - 1;
                    }

                    if (targetIndex != -1 && targetIndex != dropIndex)
                    {
                        RowChangedCommand?.Execute((dropIndex, targetIndex));
                    }
                }
            }
            //}
            IsRowDragging = false;
        }

        private void DataGrid_PreviewDrop(object sender, DragEventArgs e)
        {            //if (e.Data.GetDataPresent(typeof(object)))
                     //{
            var target = FindVisualParent<DataGridRow>(e.OriginalSource as DependencyObject);
            if (target != null)
            {
                var targetItem = target.Item as ChildViewModelBase;
                int targetIndex = Items.IndexOf(targetItem);
                if (targetItem != null)
                {
                    int dropIndex = Items.IndexOf(DraggedItem);

                    if (e.GetPosition(draggableGrid).Y < 0)
                    {
                        // ドロップ先が DataGrid の上にある場合、一番上に挿入
                        targetIndex = 0;
                    }
                    else if (e.GetPosition(draggableGrid).Y > draggableGrid.ActualHeight)
                    {
                        // ドロップ先が DataGrid の下にある場合、一番下に挿入
                        targetIndex = Items.Count - 1;
                    }

                    if (targetIndex != -1 && targetIndex != dropIndex)
                    {
                        RowChangedCommand?.Execute((dropIndex, targetIndex));
                    }
                }
            }
            //}
            IsRowDragging = false;
        }

        private T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null) return null;

            if (parentObject is T parent)
            {
                return parent;
            }
            else
            {
                return FindVisualParent<T>(parentObject);
            }
        }

        private T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T found)
                {
                    return found;
                }
                else
                {
                    T childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }




        public RowDraggableDataGrid()
        {
            PreviewMouseMove += DataGrid_PreviewMouseMove;
            Drop += DataGrid_Drop;
            InitializeComponent();
        }
    }
}
