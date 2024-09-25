using Reactive.Bindings;
using SimModel.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RiseSim.ViewModels.BindableWrapper
{
    /// <summary>
    /// バインド用装備
    /// </summary>
    internal class BindableEquipment : ChildViewModelBase
    {
        /// <summary>
        /// 表示用装備名(護石のみ特殊処理)
        /// </summary>
        public ReactiveProperty<string> DispName { get; } = new();

        /// <summary>
        /// 一覧での詳細表示用
        /// </summary>
        public ReactiveProperty<string> DetailDispName { get; } = new();

        /// <summary>
        /// 装備の説明
        /// </summary>
        public ReactiveProperty<string> Description { get; } = new();

        /// <summary>
        /// オリジナル
        /// </summary>
        public Equipment Original { get; set; }

        /// <summary>
        /// 防具を除外するコマンド
        /// </summary>
        public ReactiveCommand ExcludeCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// 防具を固定するコマンド
        /// </summary>
        public ReactiveCommand IncludeCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="equip">装備情報</param>
        public BindableEquipment(Equipment equip)
        {
            DispName.Value = equip.DispName;
            DetailDispName.Value = equip.DetailDispName;
            Description.Value = equip.Description;
            Original = equip;

            ExcludeCommand.Subscribe(() => Exclude());
            IncludeCommand.Subscribe(() => Include());
        }

        /// <summary>
        /// 装備除外
        /// </summary>
        /// <param name="equip">対象装備</param>
        private void Exclude()
        {
            CludeTabVM.AddExclude(Original);
        }

        /// <summary>
        /// 装備固定
        /// </summary>
        /// <param name="equip">対象装備</param>
        private void Include()
        {
            CludeTabVM.AddInclude(Original);
        }

        /// <summary>
        /// リストをまとめてバインド用クラスに変換
        /// </summary>
        /// <param name="list">変換前リスト</param>
        /// <param name="filter">フィルタ文字列</param>
        /// <param name="minRare">最低レア度</param>
        /// <returns></returns>
        static public ObservableCollection<BindableEquipment> BeBindableList(List<Equipment> list, string? filter, int minRare)
        {
            ObservableCollection<BindableEquipment> bindableList = new ObservableCollection<BindableEquipment>();
            foreach (var equip in list)
            {
                if ((equip.Rare == 0 || equip.Rare >= minRare) &&
                    (string.IsNullOrWhiteSpace(filter) || equip.DispName.Contains(filter)))
                {
                    bindableList.Add(new BindableEquipment(equip));
                }
            }

            // フィルタ結果が0件の場合フィルタを無効化して再計算
            if (!string.IsNullOrWhiteSpace(filter) && bindableList.Count == 0)
            {
                return BeBindableList(list, null);
            }

            // 返却
            return bindableList;
        }

        /// <summary>
        /// リストをまとめてバインド用クラスに変換
        /// </summary>
        /// <param name="list">変換前リスト</param>
        /// <param name="filter">フィルタ文字列</param>
        /// <returns></returns>
        static public ObservableCollection<BindableEquipment> BeBindableList(List<Equipment> list, string? filter)
        {
            return BeBindableList(list, null, 0);
        }

        /// <summary>
        /// リストをまとめてバインド用クラスに変換
        /// </summary>
        /// <param name="list">変換前リスト</param>
        /// <returns></returns>
        static public ObservableCollection<BindableEquipment> BeBindableList(List<Equipment> list)
        {
            return BeBindableList(list, null);
        }
    }
}
