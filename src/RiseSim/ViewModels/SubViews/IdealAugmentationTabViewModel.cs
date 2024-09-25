using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RiseSim.Util;
using RiseSim.ViewModels.BindableWrapper;
using RiseSim.ViewModels.Controls;
using SimModel.Config;
using SimModel.Domain;
using SimModel.Model;
using SimModel.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Windows;

namespace RiseSim.ViewModels.SubViews
{
    /// <summary>
    /// 理想錬成タブのVM
    /// </summary>
    internal class IdealAugmentationTabViewModel : ChildViewModelBase
    {
        /// <summary>
        /// 理想装備一覧
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<BindableIdealAugmentation>> Ideals { get; } = new();

        /// <summary>
        /// 選択中の理想装備
        /// </summary>
        public ReactivePropertySlim<BindableIdealAugmentation> SelectedIdeal { get; } = new();

        /// <summary>
        /// テーブル選択の選択肢
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<int>> TableMaster { get; } = new();

        /// <summary>
        /// 選択されたテーブル
        /// </summary>
        public ReactivePropertySlim<int> Table { get; } = new();

        /// <summary>
        /// 下位テーブルを含むか否か
        /// </summary>
        public ReactivePropertySlim<bool> IsIncludeLower { get; } = new(false);

        /// <summary>
        /// 下位テーブルを含むか否か(ラジオボタン表示用の反転したプロパティ)
        /// </summary>
        public ReadOnlyReactivePropertySlim<bool> IsNotIncludeLower { get; set; }

        /// <summary>
        /// 表示名
        /// </summary>
        public ReactivePropertySlim<string> DispName { get; } = new();

        /// <summary>
        /// スロット選択の選択肢
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<int>> SlotMaster { get; } = new();

        /// <summary>
        /// 選択されたスロット
        /// </summary>
        public ReactivePropertySlim<int> Slot { get; } = new();

        /// <summary>
        /// c3スキル追加数
        /// </summary>
        public ReactivePropertySlim<string> C3 { get; } = new("0");

        /// <summary>
        /// c6スキル追加数
        /// </summary>
        public ReactivePropertySlim<string> C6 { get; } = new("0");

        /// <summary>
        /// c9スキル追加数
        /// </summary>
        public ReactivePropertySlim<string> C9 { get; } = new("0");

        /// <summary>
        /// c12スキル追加数
        /// </summary>
        public ReactivePropertySlim<string> C12 { get; } = new("0");

        /// <summary>
        /// c15スキル追加数
        /// </summary>
        public ReactivePropertySlim<string> C15 { get; } = new("0");

        /// <summary>
        /// 部位制限の有無
        /// </summary>
        public ReactivePropertySlim<bool> IsOne { get; } = new(false);

        /// <summary>
        /// 部位制限の有無(ラジオボタン表示用の反転したプロパティ)
        /// </summary>
        public ReadOnlyReactivePropertySlim<bool> IsNotOne { get; set; }

        /// <summary>
        /// スキル選択部品のVM
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<SkillSelectorViewModel>> SkillSelectorVMs { get; } = new();

        /// <summary>
        /// 削除スキル選択部品のVM
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<MinusSelectorViewModel>> MinusSelectorVMs { get; } = new();

        /// <summary>
        /// 説明
        /// </summary>
        public ReactivePropertySlim<string> HowToUse { get; } = new();


        /// <summary>
        /// 追加コマンド
        /// </summary>
        public ReactiveCommand AddCommand { get; private set; } = new();

        /// <summary>
        /// 削除コマンド
        /// </summary>
        public ReactiveCommand DeleteCommand { get; private set; } = new();

        /// <summary>
        /// 反映コマンド
        /// </summary>
        public ReactiveCommand InputCommand { get; private set; } = new();

        /// <summary>
        /// 上書きコマンド
        /// </summary>
        public ReactiveCommand UpdateCommand { get; private set; } = new();

        /// <summary>
        /// 全無効化コマンド
        /// </summary>
        public ReactiveCommand AllDisableCommand { get; private set; } = new();

        /// <summary>
        /// 全有効化コマンド
        /// </summary>
        public ReactiveCommand AllEnableCommand { get; private set; } = new();


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public IdealAugmentationTabViewModel()
        {
            // スキル選択部品準備
            ObservableCollection<SkillSelectorViewModel> selectorVMs = new();
            for (int i = 0; i < LogicConfig.Instance.MaxAugmentationSkillCountActual; i++)
            {
                selectorVMs.Add(new SkillSelectorViewModel(SkillSelectorKind.IdealAugmentation));
            }
            SkillSelectorVMs.ChangeCollection(selectorVMs);

            // 削除スキル選択部品準備
            ObservableCollection<MinusSelectorViewModel> minusVMs = new();
            for (int i = 0; i < LogicConfig.Instance.MaxAugmentationSkillCountActual; i++)
            {
                minusVMs.Add(new MinusSelectorViewModel());
            }
            MinusSelectorVMs.ChangeCollection(minusVMs);

            // スロットの選択肢を生成
            TableMaster.Value = new() { 1, 2, 3, 4, 5, 6, 13 };
            Table.Value = 6;

            // スロットの選択肢を生成
            SlotMaster.Value = new() { 0, 1, 2, 3, 4, 5, 6 };
            Slot.Value = 0;

            // コマンドを設定
            IsNotIncludeLower = IsIncludeLower.Select(x => !x).ToReadOnlyReactivePropertySlim().AddTo(Disposable);
            IsNotOne = IsOne.Select(x => !x).ToReadOnlyReactivePropertySlim().AddTo(Disposable);
            InputCommand.Subscribe(_ => InputIdeal());
            AddCommand.Subscribe(_ => AddIdeal());
            DeleteCommand.Subscribe(_ => DeleteIdeal());
            UpdateCommand.Subscribe(_ => UpdateIdeal());
            AllDisableCommand.Subscribe(_ => AllDisable());
            AllEnableCommand.Subscribe(_ => AllEnable());

            // 説明
            WriteHowToUse();
        }

        /// <summary>
        /// 理想錬成情報を追加
        /// </summary>
        private void AddIdeal()
        {
            IdealAugmentation ideal = new();
            ideal.Name = Guid.NewGuid().ToString();
            ideal.Table = Table.Value;
            ideal.IsIncludeLower = IsIncludeLower.Value;
            string dispName = DispName.Value;
            if (string.IsNullOrWhiteSpace(dispName))
            {
                dispName = Masters.MakeIdealAugmentaionDefaultDispName(Table.Value);
            }
            ideal.DispName = dispName;

            ideal.SlotIncrement = Slot.Value;
            ideal.GenericSkills[0] = ParseUtil.Parse(C3.Value);
            ideal.GenericSkills[1] = ParseUtil.Parse(C6.Value);
            ideal.GenericSkills[2] = ParseUtil.Parse(C9.Value);
            ideal.GenericSkills[3] = ParseUtil.Parse(C12.Value);
            ideal.GenericSkills[4] = ParseUtil.Parse(C15.Value);
            foreach (var selector in SkillSelectorVMs.Value)
            {
                if (Masters.IsSkillName(selector.SkillName.Value))
                {
                    Skill skill = new(selector.SkillName.Value, selector.SkillLevel.Value, true);
                    ideal.Skills.Add(skill);
                }
            }
            foreach (var selector in MinusSelectorVMs.Value)
            {
                if (selector.SelectedIndex != -1)
                {
                    ideal.SkillMinuses.Add(selector.SelectedIndex);
                }
            }
            ideal.IsOne = IsOne.Value;

            Simulator.AddIdealAugmentation(ideal);

            MainVM.LoadEquips();

            // ログ
            SetStatusBar("理想錬成追加完了：" + ideal.DispName);
        }

        /// <summary>
        /// 理想錬成情報を削除
        /// </summary>
        private void DeleteIdeal()
        {
            BindableIdealAugmentation ideal = SelectedIdeal.Value;

            if (ideal == null)
            {
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                $"理想錬成「{ideal.DispName}」を削除します。\nよろしいですか？",
                "理想錬成削除",
                MessageBoxButton.YesNo);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            // 錬成情報を削除
            Simulator.DeleteIdealAugmentation(SelectedIdeal.Value.Original);

            // マスタをリロード
            // マイセットが変更になる可能性があるためそちらもリロード
            MySetTabVM.LoadMySets();
            MainVM.LoadEquips();

            // ログ
            SetStatusBar("理想錬成削除完了：" + ideal.DispName);
        }

        /// <summary>
        /// 反映コマンド
        /// </summary>
        private void InputIdeal()
        {
            if (SelectedIdeal.Value == null)
            {
                return;
            }
            IdealAugmentation ideal = SelectedIdeal.Value.Original;
            Table.Value = ideal.Table;
            IsIncludeLower.Value = ideal.IsIncludeLower;
            DispName.Value = ideal.DispName;
            Slot.Value = ideal.SlotIncrement;
            C3.Value = ideal.GenericSkills[0].ToString();
            C6.Value = ideal.GenericSkills[1].ToString();
            C9.Value = ideal.GenericSkills[2].ToString();
            C12.Value = ideal.GenericSkills[3].ToString();
            C15.Value = ideal.GenericSkills[4].ToString();
            for (int i = 0; i < LogicConfig.Instance.MaxAugmentationSkillCountActual; i++)
            {
                if (i < ideal.Skills.Count)
                {
                    SkillSelectorVMs.Value[i].SkillName.Value = ideal.Skills[i].Name;
                    SkillSelectorVMs.Value[i].SkillLevel.Value = ideal.Skills[i].Level;
                }
                else
                {
                    SkillSelectorVMs.Value[i].SkillName.Value = string.Empty;
                    SkillSelectorVMs.Value[i].SkillLevel.Value = 0;
                }
            }
            for (int i = 0; i < LogicConfig.Instance.MaxAugmentationSkillCountActual; i++)
            {
                if (i < ideal.SkillMinuses.Count)
                {
                    MinusSelectorVMs.Value[i].SelectedIndex = ideal.SkillMinuses[i];
                }
                else
                {
                    MinusSelectorVMs.Value[i].SelectedIndex = -1;
                }
            }
            IsOne.Value = ideal.IsOne;

            // ログ
            SetStatusBar("理想錬成反映完了：" + ideal.DispName);
        }

        /// <summary>
        /// 上書きコマンド
        /// </summary>
        public void UpdateIdeal()
        {
            if (SelectedIdeal.Value == null)
            {
                return;
            }

            IdealAugmentation ideal = new();
            ideal.Name = SelectedIdeal.Value.Original.Name;
            ideal.Table = Table.Value;
            ideal.IsIncludeLower = IsIncludeLower.Value;
            ideal.IsEnabled = SelectedIdeal.Value.IsEnabled.Value;
            ideal.IsRequired = SelectedIdeal.Value.IsRequired.Value;
            string dispName = DispName.Value;
            if (string.IsNullOrWhiteSpace(dispName))
            {
                dispName = Masters.MakeIdealAugmentaionDefaultDispName(Table.Value);
            }
            ideal.DispName = dispName;

            ideal.SlotIncrement = Slot.Value;
            ideal.GenericSkills[0] = ParseUtil.Parse(C3.Value);
            ideal.GenericSkills[1] = ParseUtil.Parse(C6.Value);
            ideal.GenericSkills[2] = ParseUtil.Parse(C9.Value);
            ideal.GenericSkills[3] = ParseUtil.Parse(C12.Value);
            ideal.GenericSkills[4] = ParseUtil.Parse(C15.Value);
            foreach (var selector in SkillSelectorVMs.Value)
            {
                if (Masters.IsSkillName(selector.SkillName.Value))
                {
                    Skill skill = new(selector.SkillName.Value, selector.SkillLevel.Value, true);
                    ideal.Skills.Add(skill);
                }
            }
            foreach (var selector in MinusSelectorVMs.Value)
            {
                if (selector.SelectedIndex != -1)
                {
                    ideal.SkillMinuses.Add(selector.SelectedIndex);
                }
            }
            ideal.IsOne = IsOne.Value;

            Simulator.UpdateIdealAugmentation(ideal);

            MainVM.LoadEquips();

            // マイセット内容の更新
            Simulator.LoadMySet();
            MySetTabVM.LoadMySets();

            // ログ
            SetStatusBar("理想錬成上書き完了：" + ideal.DispName);
        }

        /// <summary>
        /// 理想錬成の変更状態を保存
        /// </summary>
        internal void SaveIdeal()
        {
            Simulator.SaveIdeal();
        }

        /// <summary>
        /// 全有効化
        /// </summary>
        private void AllEnable()
        {
            SetAllIsEnabled(true);
            SaveIdeal();

            // ログ
            SetStatusBar("理想錬成の全有効化完了");
        }

        /// <summary>
        /// 全無効化
        /// </summary>
        private void AllDisable()
        {
            SetAllIsEnabled(false);
            SaveIdeal();

            // ログ
            SetStatusBar("理想錬成の全無効化完了");
        }

        /// <summary>
        /// 全ての理想錬成に有効無効を設定
        /// </summary>
        /// <param name="isEnabled"></param>
        private void SetAllIsEnabled(bool isEnabled)
        {
            foreach (var bindableIdeal in Ideals.Value)
            {
                bindableIdeal.IsEnabled.Value = isEnabled;
            }
        }

        /// <summary>
        /// 錬成装備のマスタ情報をVMにロード
        /// </summary>
        internal void LoadIdealAugmentations()
        {
            // 錬成装備情報の設定
            Ideals.ChangeCollection(BindableIdealAugmentation.BeBindableList(Masters.Ideals));
        }

        /// <summary>
        /// 説明
        /// </summary>
        private void WriteHowToUse()
        {
            StringBuilder sb = new();
            sb.Append("■ナニコレ\n");
            sb.Append("テーブル単位で、「ここまでなら引くまで錬成する」のような、自分が理想とする錬成を登録できます。\n");
            sb.Append("シミュレータタブにて「理想錬成を使う」をオンにすると、登録した理想錬成情報を使って検索します。\n");
            sb.Append('\n');
            sb.Append("■注意\n");
            sb.Append("・「理想錬成を使う」をオンにしないと機能しません。\n");
            sb.Append("・理想錬成を適用した装備の固定・除外設定はベース防具の固定・除外設定に準じます。\n");
            sb.Append("・特定の理想錬成自体を有効・無効にしたい場合はこの画面で行ってください。\n");
            sb.Append("・装飾品と錬成追加スキルのパターンは1種類しか提示されません(つまり、入れ替えが可能な場合があります)。\n");
            sb.Append("・本来存在しない錬成も登録できてしまいます。\n");
            sb.Append('\n');
            sb.Append("■使用例１\n");
            sb.Append("「T6で『スキル1つ欠けのs3』『スキル1つ欠けのs2c3』『スキル1つ欠けのs1c15』『s2』『c3s1』『c15』までなら頑張る。何を錬成するといい？」\n");
            sb.Append("部位制限を『全部位可』にして、これらを登録してシミュしてください。\n");
            sb.Append("(スキルの欠けは、『どれか1つをLv-1』を選択すれば全パターン検索します。)\n");
            sb.Append("候補に出てきた装備が狙い目です。\n");
            sb.Append('\n');
            sb.Append("■使用例２\n");
            sb.Append("「T6で6番目欠けのs4引いたけど何に移植すればいい？」\n");
            sb.Append("部位制限を『一部位限定』にして、これを登録してシミュしてください。\n");
            sb.Append("候補に出てきた装備が狙い目です。\n");
            sb.Append("ただし、検索結果は『移植が成功した場合』の結果であり、移植が成功することを保証するものではありません。\n");
            sb.Append('\n');
            sb.Append("■備考\n");
            sb.Append("・指定したコストより低コストのスキルも計算対象です(例えばc9を1つ指定した場合、その枠に回復量UP(c3)が入ることもあります)。\n");
            HowToUse.Value = sb.ToString();
        }
    }
}
