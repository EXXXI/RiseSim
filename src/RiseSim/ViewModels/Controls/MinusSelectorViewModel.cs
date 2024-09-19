using Reactive.Bindings;
using System.Collections.ObjectModel;

namespace RiseSim.ViewModels.Controls
{
    /// <summary>
    /// 理想錬成の削除スキル選択部品のVM
    /// </summary>
    internal class MinusSelectorViewModel : ChildViewModelBase
    {
        /// <summary>
        /// 選択肢一覧
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<MinusSelectorItem>> ItemMaster { get; } = new();

        /// <summary>
        /// 選択中項目
        /// </summary>
        public ReactivePropertySlim<MinusSelectorItem> SelectedItem { get; } = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MinusSelectorViewModel()
        {
            ItemMaster.Value = MinusSelectorItem.MakeMinusSelectorItems();
            SelectedItem.Value = ItemMaster.Value[0];
        }

        /// <summary>
        /// クラス外部とintで選択状態をやり取りする
        /// </summary>
        public int SelectedIndex { 
            get
            {
                return SelectedItem.Value.Value;
            } 
            set
            {
                foreach (var item in ItemMaster.Value)
                {
                    if (item.Value == value)
                    {
                        SelectedItem.Value = item;
                    }
                }
            }
        }
    }
}
