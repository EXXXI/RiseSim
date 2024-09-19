using System.Collections.ObjectModel;

namespace RiseSim.ViewModels.Controls
{
    /// <summary>
    /// 理想錬成のスキル削除の選択肢
    /// </summary>
    internal class MinusSelectorItem : ChildViewModelBase
    {
        /// <summary>
        /// 表示
        /// </summary>
        public string Disp { get; set; }

        /// <summary>
        /// 内部で使う値
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="disp">表示</param>
        /// <param name="value">内部で使う値</param>
        public MinusSelectorItem(string disp, int value)
        {
            Disp = disp;
            Value = value;
        }

        /// <summary>
        /// 選択肢一覧の作成
        /// </summary>
        /// <returns></returns>
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
