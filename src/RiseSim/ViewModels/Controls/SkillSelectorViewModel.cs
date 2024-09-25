using Reactive.Bindings;
using RiseSim.Config;
using SimModel.Model;
using System;
using System.Collections.ObjectModel;

namespace RiseSim.ViewModels.Controls
{
    /// <summary>
    /// スキル選択部品(スキル種類選択可)
    /// </summary>
    class SkillSelectorViewModel : ChildViewModelBase
    {
        /// <summary>
        /// 「固定」文字列
        /// </summary>
        const string FixStr = "固定";

        /// <summary>
        /// 「以上」文字列
        /// </summary>
        const string NotFixStr = "以上";

        /// <summary>
        /// スキル名一覧
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<string>> Skills { get; } = new();

        /// <summary>
        /// 選択中スキルのレベル一覧
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<int>> SkillLevels { get; } = new();

        /// <summary>
        /// 選択中スキル名
        /// </summary>
        public ReactivePropertySlim<string> SkillName { get; } = new();

        /// <summary>
        /// 選択中スキルレベル
        /// </summary>
        public ReactivePropertySlim<int> SkillLevel { get; } = new();

        /// <summary>
        /// 選択中スキル番号
        /// </summary>
        public ReactivePropertySlim<int> SkillIndex { get; } = new(-1);

        /// <summary>
        /// 画面の種類
        /// </summary>
        public SkillSelectorKind Kind { get; set; } = SkillSelectorKind.Normal;

        /// <summary>
        /// スキル値固定の表示有無
        /// </summary>
        public ReactivePropertySlim<bool> IsWithFix { get; } = new(false);

        /// <summary>
        /// スキル値固定の表示候補
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<string>> IsFixDisps { get; } = new();

        /// <summary>
        /// スキル値固定の表示内容
        /// </summary>
        public ReactivePropertySlim<string> IsFixDisp { get; } = new();

        /// <summary>
        /// スキル値固定状態
        /// </summary>
        public bool IsFix { get; set; } = false;

        /// <summary>
        /// プレースホルダーの内容
        /// </summary>
        public ReactivePropertySlim<string> PlaceHolderText { get; } = new(ViewConfig.Instance.NoSkillName);

        /// <summary>
        /// クリアコマンド
        /// </summary>
        public ReactiveCommand ClearCommand { get; } = new ReactiveCommand();


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="kind">部品の種類</param>
        public SkillSelectorViewModel(SkillSelectorKind kind = SkillSelectorKind.Normal)
        {
            Kind = kind;
            IsWithFix.Value = Kind == SkillSelectorKind.WithFixs;

            // スキル値固定関連準備
            IsFixDisps.Value = new();
            IsFixDisps.Value.Add(NotFixStr);
            IsFixDisps.Value.Add(FixStr);
            IsFixDisp.Value = NotFixStr;

            // スキルの一覧
            ObservableCollection<string> skillList = new();
            foreach (var skill in Masters.Skills)
            {
                skillList.Add(skill.Name);
            }
            Skills.Value = skillList;

            // スキル名変更時にレベル一覧を変更するように紐づけ
            SkillName.Subscribe(_ => SetLevels());

            // コマンドを設定
            ClearCommand.Subscribe(_ => SetDefault());
            IsFixDisp.Subscribe(_ => { IsFix = IsFixDisp.Value == FixStr; });
        }

        /// <summary>
        /// 選択中スキル名にあわせてスキルレベルの選択肢を変更
        /// </summary>
        private void SetLevels()
        {
            ObservableCollection<int> list = new();

            int maxLevel = Masters.SkillMaxLevel(SkillName.Value);

            for (int i = 0; i < Skills.Value.Count; i++)
            {
                if (Skills.Value[i].Equals(SkillName.Value))
                {
                    SkillIndex.Value = i;
                }
            }

            if (maxLevel == 0)
            {
                // スキルが選択されていないときは0とする
                list.Add(0);
            }
            else if (Kind == SkillSelectorKind.Augmentation)
            {
                // スキルが存在して傀異錬成画面の場合は固定
                list.Add(3);
                list.Add(2);
                list.Add(1);
                list.Add(-1);
                list.Add(-2);
                list.Add(-3);
            }
            else if (Kind == SkillSelectorKind.IdealAugmentation)
            {
                // スキルが存在して理想錬成画面の場合は固定
                list.Add(3);
                list.Add(2);
                list.Add(1);
            }
            else
            {
                // 通常の場合
                for (int i = maxLevel; i >= 0; i--)
                {
                    list.Add(i);
                }
            }
            SkillLevels.Value = list;

            // 初期値は通常は最大レベル、傀異錬成・理想錬成時は1とする
            if (maxLevel != 0 && Kind != SkillSelectorKind.Normal && Kind != SkillSelectorKind.WithFixs)
            {
                SkillLevel.Value = 1;
            }
            else
            {
                SkillLevel.Value = maxLevel;
            }
        }

        /// <summary>
        /// 選択状態をリセット
        /// </summary>
        private void SetDefault()
        {
            SkillName.Value = string.Empty;
            IsFixDisp.Value = NotFixStr;
        }
    }
}
