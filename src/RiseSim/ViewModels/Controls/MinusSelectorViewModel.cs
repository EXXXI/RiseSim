using Prism.Mvvm;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiseSim.ViewModels.Controls
{
    // 理想錬成の削除スキル選択部品のVM
    internal class MinusSelectorViewModel : BindableBase
    {

        // 選択肢一覧
        public ReactivePropertySlim<ObservableCollection<MinusSelectorItem>> ItemMaster { get; } = new();

        // 選択中項目
        public ReactivePropertySlim<MinusSelectorItem> SelectedItem { get; } = new();

        // コンストラクタ
        public MinusSelectorViewModel()
        {
            ItemMaster.Value = MinusSelectorItem.MakeMinusSelectorItems();
            SelectedItem.Value = ItemMaster.Value[0];
        }

        // クラス外部とintで選択状態をやり取りする
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
