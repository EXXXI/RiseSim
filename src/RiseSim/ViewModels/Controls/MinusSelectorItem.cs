using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiseSim.ViewModels.Controls
{
    // 理想錬成のスキル削除の選択肢
    internal class MinusSelectorItem : BindableBase
    {
        // 表示
        public string Disp { get; set; }

        // 内部で使う値
        public int Value { get; set; }

        // コンストラクタ
        MinusSelectorItem(string disp, int value)
        {
            Disp = disp;
            Value = value;
        }

        // 選択肢一覧の作成
        public static ObservableCollection<MinusSelectorItem> MakeMinusSelectorItems()
        {
            ObservableCollection<MinusSelectorItem> items = new();
            items.Add(new MinusSelectorItem("-", -1));
            items.Add(new MinusSelectorItem("どれか1つをLv-1", 0));
            for (int i = 1; i <= 12; i++)
            {
                items.Add(new MinusSelectorItem(i + "番目をLv-1", i));
            }

            return items;
        }
    }
}
