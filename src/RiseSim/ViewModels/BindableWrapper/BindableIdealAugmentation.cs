using Reactive.Bindings;
using SimModel.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace RiseSim.ViewModels.BindableWrapper
{
    /// <summary>
    /// バインド用理想錬成
    /// </summary>
    internal class BindableIdealAugmentation : ChildViewModelBase
    {
        /// <summary>
        /// 表示用装備名
        /// </summary>
        public ReactivePropertySlim<string> DispName { get; } = new();

        /// <summary>
        /// 表示用テーブル
        /// </summary>
        public ReactivePropertySlim<string> TableDisp { get; } = new();

        /// <summary>
        /// スロット追加数
        /// </summary>
        public ReactivePropertySlim<int> SlotIncrement { get; } = new();

        /// <summary>
        /// 1部位だけか否か
        /// </summary>
        public ReactivePropertySlim<string> IsOneDisp { get; } = new();

        /// <summary>
        /// 表示用スキル一覧
        /// </summary>
        public ReactivePropertySlim<string> SimpleSkillDiscription { get; } = new();

        /// <summary>
        /// 表示用スキルマイナス一覧
        /// </summary>
        public ReactivePropertySlim<string> SimpleSkillMinusDiscription { get; } = new();

        /// <summary>
        /// 有効無効
        /// </summary>
        public ReactivePropertySlim<bool> IsEnabled { get; } = new(true);

        /// <summary>
        /// 必須
        /// </summary>
        public ReactivePropertySlim<bool> IsRequired { get; } = new(false);

        /// <summary>
        /// オリジナル
        /// </summary>
        public IdealAugmentation Original { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="ideal">理想錬成</param>
        public BindableIdealAugmentation(IdealAugmentation ideal)
        {
            DispName.Value = ideal.DispName;
            SlotIncrement.Value = ideal.SlotIncrement;
            IsOneDisp.Value = ideal.IsOne ? "一部位のみ" : "全部位可";
            IsEnabled.Value = ideal.IsEnabled;
            IsRequired.Value = ideal.IsRequired;
            TableDisp.Value = ideal.Table + (ideal.IsIncludeLower ? "以下" : "のみ"); 
            SimpleSkillDiscription.Value = ideal.SimpleSkillDiscription;
            SimpleSkillMinusDiscription.Value = ideal.SimpleSkillMinusDiscription;
            Original = ideal;
            IsEnabled.Subscribe(x => ChangeIsEnabled(x));
            IsRequired.Subscribe(x => ChangeIsRequired(x));
        }

        /// <summary>
        /// 有効無効切り替え
        /// </summary>
        /// <param name="x">有効の場合true</param>
        private void ChangeIsEnabled(bool x)
        {
            if (Original.IsEnabled != x)
            {
                Original.IsEnabled = x;
                IdealAugmentationTabVM.SaveIdeal();
            }
        }

        /// <summary>
        /// 必須切り替え
        /// </summary>
        /// <param name="x">必須の場合true</param>
        private void ChangeIsRequired(bool x)
        {
            if (Original.IsRequired != x)
            {
                Original.IsRequired = x;
                IdealAugmentationTabVM.SaveIdeal();
            }
        }

        /// <summary>
        /// リストをまとめてバインド用クラスに変換
        /// </summary>
        /// <param name="list">変換前リスト</param>
        /// <returns></returns>
        static public ObservableCollection<BindableIdealAugmentation> BeBindableList(List<IdealAugmentation> list)
        {
            ObservableCollection<BindableIdealAugmentation> bindableList = new ObservableCollection<BindableIdealAugmentation>();
            foreach (var ideal in list)
            {
                bindableList.Add(new BindableIdealAugmentation(ideal));
            }
            return bindableList;
        }
    }
}
