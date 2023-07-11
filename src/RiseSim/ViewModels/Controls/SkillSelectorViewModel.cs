using Prism.Mvvm;
using Reactive.Bindings;
using RiseSim.Config;
using SimModel.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using RiseSim.Exceptions;
using RiseSim.Views.Controls;

namespace RiseSim.ViewModels.Controls
{
    class SkillSelectorViewModel : BindableBase
    {
        const string FixStr = "固定";
        const string NotFixStr = "以上";

        // TODO: あまりやりたくないのでいい方法を考えたい
        // 画面インスタンス
        public SkillSelector Instance { get; set; }

        // スキル名一覧
        public ReactivePropertySlim<ObservableCollection<string>> Skills { get; } = new();

        // 選択中スキルのレベル一覧
        public ReactivePropertySlim<ObservableCollection<int>> SkillLevels { get; } = new();

        // 選択中スキル名
        public ReactivePropertySlim<string> SkillName { get; } = new();

        // 選択中スキルレベル
        public ReactivePropertySlim<int> SkillLevel { get; } = new();

        // 選択中スキル番号
        public ReactivePropertySlim<int> SkillIndex { get; } = new(-1);

        // 画面の種類
        public SkillSelectorKind Kind { get; set; } = SkillSelectorKind.Normal;

        // スキル値固定の表示有無
        public ReactivePropertySlim<bool> IsWithFix { get; } = new(false);

        // スキル値固定の表示候補
        public ReactivePropertySlim<ObservableCollection<string>> IsFixDisps { get; } = new();

        // スキル値固定の表示内容
        public ReactivePropertySlim<string> IsFixDisp { get; } = new();

        // スキル値固定状態
        public bool IsFix { get; set; } = false;

        // プレースホルダーの表示フラグ
        public ReactivePropertySlim<bool> ShowPlaceHolder { get; } = new();

        // プレースホルダーの内容
        public ReactivePropertySlim<string> PlaceHolderText { get; } = new(ViewConfig.Instance.NoSkillName);


        // クリアコマンド
        public ReactiveCommand ClearCommand { get; } = new ReactiveCommand();

        // コマンドを設定
        private void SetCommand()
        {
            ClearCommand.Subscribe(_ => SetDefault());
            IsFixDisp.Subscribe(_ => { IsFix = IsFixDisp.Value == FixStr; });
        }


        // スキルマスタを読み込み
        public SkillSelectorViewModel(SkillSelectorKind kind = SkillSelectorKind.Normal)
        {
            Kind = kind;
            IsWithFix.Value = Kind == SkillSelectorKind.WithFixs;

            // スキル値固定関連準備
            IsFixDisps.Value = new();
            IsFixDisps.Value.Add(NotFixStr);
            IsFixDisps.Value.Add(FixStr);
            IsFixDisp.Value = NotFixStr;


            ObservableCollection<string> skillList = new();
            foreach (var skill in Masters.Skills)
            {
                skillList.Add(skill.Name);
            }
            Skills.Value = skillList;

            // スキル名変更時にレベル一覧を変更するように紐づけ
            SkillName.Subscribe(_ => SetLevels());

            // コマンドを設定
            SetCommand();
        }

        /// <summary>
        /// 特定のスキルを選択した状態のSkillSelectorViewModelを作って返す
        /// </summary>
        /// <param name="skill"></param>
        public SkillSelectorViewModel(Skill skill, SkillSelectorKind kind = SkillSelectorKind.Normal) : this(kind)
        {
            SkillName.Value = skill.Name;
            SkillLevel.Value = skill.Level;
        }

        /// <summary>
        /// このSkillSelectorが選択しているスキルをSkillにして返す
        /// </summary>
        public Skill GetSelectedSkill()
        {
            var sameNameSkills = Masters.Skills.Where(s => s.Name == SkillName.Value).ToList();

            if (!sameNameSkills.Any())
            {
                throw new SkillNotFoundException(SkillName.Value);
            }

            return sameNameSkills.First() with
            {
                Level = Math.Min(sameNameSkills.Max(s => s.Level), SkillLevel.Value)
            };
        }

        // 選択中スキル名にあわせてスキルレベルの選択肢を変更
        internal void SetLevels()
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

        // 選択状態をリセット
        internal void SetDefault()
        {
            SkillName.Value = string.Empty;
            IsFixDisp.Value = NotFixStr;
        }
    }
}
