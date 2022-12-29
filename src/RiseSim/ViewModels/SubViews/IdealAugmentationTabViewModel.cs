using Prism.Mvvm;
using Reactive.Bindings;
using RiseSim.Config;
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
using System.Threading.Tasks;

namespace RiseSim.ViewModels.SubViews
{
    internal class IdealAugmentationTabViewModel : BindableBase
    {
        // MainViewModelから参照を取得
        private Simulator Simulator { get; }
        private ReactivePropertySlim<string> StatusBarText { get; }


        // 理想装備一覧
        public ReactivePropertySlim<ObservableCollection<BindableIdealAugmentation>> Ideals { get; } = new();

        // 選択中の理想装備
        public ReactivePropertySlim<BindableIdealAugmentation> SelectedIdeal { get; } = new();

        // テーブル選択の選択肢
        public ReactivePropertySlim<ObservableCollection<int>> TableMaster { get; } = new();

        // 選択されたテーブル
        public ReactivePropertySlim<int> Table { get; } = new();

        // 表示名
        public ReactivePropertySlim<string> DispName { get; } = new();

        // スロット選択の選択肢
        public ReactivePropertySlim<ObservableCollection<int>> SlotMaster { get; } = new();

        // 選択されたスロット
        public ReactivePropertySlim<int> Slot { get; } = new();

        // c3スキル追加数
        public ReactivePropertySlim<string> C3 { get; } = new("0");

        // c6スキル追加数
        public ReactivePropertySlim<string> C6 { get; } = new("0");

        // c9スキル追加数
        public ReactivePropertySlim<string> C9 { get; } = new("0");

        // c12スキル追加数
        public ReactivePropertySlim<string> C12 { get; } = new("0");

        // c15スキル追加数
        public ReactivePropertySlim<string> C15 { get; } = new("0");

        // 部位制限の有無
        public ReactivePropertySlim<bool> IsOne { get; } = new(false);

        // 部位制限の有無(ラジオボタン表示用の反転したプロパティ)
        public ReadOnlyReactivePropertySlim<bool> IsNotOne { get; set; }

        // スキル選択部品のVM
        public ReactivePropertySlim<ObservableCollection<SkillSelectorViewModel>> SkillSelectorVMs { get; } = new();

        // 削除スキル選択部品のVM
        public ReactivePropertySlim<ObservableCollection<MinusSelectorViewModel>> MinusSelectorVMs { get; } = new();

        // 説明
        public ReactivePropertySlim<string> HowToUse { get; } = new();


        // 追加コマンド
        public ReactiveCommand AddCommand { get; private set; } = new();

        // 削除コマンド
        public ReactiveCommand DeleteCommand { get; private set; } = new();

        // 反映コマンド
        public ReactiveCommand InputCommand { get; private set; } = new();

        // 上書きコマンド
        public ReactiveCommand UpdateCommand { get; private set; } = new();


        // コマンドを設定
        private void SetCommand()
        {
            IsNotOne = IsOne.Select(x => !x).ToReadOnlyReactivePropertySlim();
            InputCommand.Subscribe(_ => InputIdeal());
            AddCommand.Subscribe(_ => AddIdeal());
            DeleteCommand.Subscribe(_ => DeleteIdeal());
            UpdateCommand.Subscribe(_ => UpdateIdeal());
        }


        // コンストラクタ
        public IdealAugmentationTabViewModel()
        {
            // MainViewModelから参照を取得
            Simulator = MainViewModel.Instance.Simulator;
            StatusBarText = MainViewModel.Instance.StatusBarText;

            // スキル選択部品準備
            ObservableCollection<SkillSelectorViewModel> selectorVMs = new();
            for (int i = 0; i < LogicConfig.Instance.MaxAugmentationSkillCountActual; i++)
            {
                selectorVMs.Add(new SkillSelectorViewModel(SkillSelectorKind.IdealAugmentation));
            }
            SkillSelectorVMs.Value = selectorVMs;

            // 削除スキル選択部品準備
            ObservableCollection<MinusSelectorViewModel> minusVMs = new();
            for (int i = 0; i < LogicConfig.Instance.MaxAugmentationSkillCountActual; i++)
            {
                minusVMs.Add(new MinusSelectorViewModel());
            }
            MinusSelectorVMs.Value = minusVMs;

            // スロットの選択肢を生成
            TableMaster.Value = new() { 1, 2, 3, 4, 5, 6, 13 };
            Table.Value = 6;

            // スロットの選択肢を生成
            SlotMaster.Value = new() { 0, 1, 2, 3, 4, 5, 6 };
            Slot.Value = 0;

            // コマンドを設定
            SetCommand();

            // 説明
            WriteHowToUse();
        }

        // 理想錬成情報を追加
        private void AddIdeal()
        {
            IdealAugmentation ideal = new();
            ideal.Name = Guid.NewGuid().ToString();
            ideal.Table = Table.Value;
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
                if (selector.SkillName.Value != ViewConfig.Instance.NoSkillName)
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

            MainViewModel.Instance.LoadEquips();
        }

        // 理想錬成情報を削除
        private void DeleteIdeal()
        {
            BindableIdealAugmentation ideal = SelectedIdeal.Value;

            if (ideal == null)
            {
                return;
            }

            // 該当の理想錬成を使っているマイセットがある場合削除
            DeleteMySetUsingIdeal(ideal.Name);

            // 錬成情報を削除
            Simulator.DeleteIdealAugmentation(SelectedIdeal.Value.Original);

            // マスタをリロード
            // MainViewModel.Instance.LoadCludes();
            MainViewModel.Instance.LoadMySets();
            MainViewModel.Instance.LoadEquips();
        }

        // 指定した理想錬成装備を使っているマイセットがあったら削除
        private void DeleteMySetUsingIdeal(string name)
        {
            List<EquipSet> delMySets = new();
            foreach (var set in Masters.MySets)
            {
                if ((set.Head.Ideal?.Name != null && set.Head.Ideal?.Name == name) ||
                    (set.Body.Ideal?.Name != null && set.Body.Ideal?.Name == name) ||
                    (set.Arm.Ideal?.Name != null && set.Arm.Ideal?.Name == name) ||
                    (set.Waist.Ideal?.Name != null && set.Waist.Ideal?.Name == name) ||
                    (set.Leg.Ideal?.Name != null && set.Leg.Ideal?.Name == name) )
                {
                    delMySets.Add(set);
                }
            }
            foreach (var set in delMySets)
            {
                Simulator.DeleteMySet(set);
            }
        }

        // 反映コマンド
        private void InputIdeal()
        {
            if (SelectedIdeal.Value == null)
            {
                return;
            }
            IdealAugmentation ideal = SelectedIdeal.Value.Original;
            Table.Value = ideal.Table;
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
                    SkillSelectorVMs.Value[i].SkillName.Value = ViewConfig.Instance.NoSkillName;
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
        }



        // 上書きコマンド
        private void UpdateIdeal()
        {
            if (SelectedIdeal.Value == null)
            {
                return;
            }

            IdealAugmentation ideal = new();
            ideal.Name = SelectedIdeal.Value.Name;
            ideal.Table = Table.Value;
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
                if (selector.SkillName.Value != ViewConfig.Instance.NoSkillName)
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

            MainViewModel.Instance.LoadEquips();

            // マイセット内容の更新
            Simulator.LoadMySet();
            MainViewModel.Instance.LoadMySets();
        }

        // 錬成装備のマスタ情報をVMにロード
        internal void LoadIdealAugmentations()
        {
            // 錬成装備情報の設定
            Ideals.Value = BindableIdealAugmentation.BeBindableList(Masters.Ideals);
        }

        // 説明
        private void WriteHowToUse()
        {
            StringBuilder sb = new();
            sb.Append("■ナニコレ\n");
            sb.Append("テーブル単位で、「ここまでなら引くまで錬成する」のような、自分が理想とする錬成を登録できます。\n");
            sb.Append("シミュレータタブにて「理想錬成を使う」をオンにすると、登録した理想錬成情報を使って検索します。\n");
            sb.Append('\n');
            sb.Append("■注意\n");
            sb.Append("・「理想錬成を使う」をオンにしないと機能しません。\n");
            sb.Append("・理想錬成単位での固定・除外はできません(検索時はベース装備の固定・除外設定に準じます)。\n");
            sb.Append("・装飾品と錬成追加スキルのパターンは1種類しか提示されません(つまり、入れ替えが可能な場合があります)。\n");
            sb.Append("・本来存在しない錬成も登録できてしまいます。\n");
            sb.Append('\n');
            sb.Append("■使用例１\n");
            sb.Append("「T6で『スキル1つ欠けのs3』『スキル1つ欠けのs2c3』『スキル1つ欠けのs1c15』『s2』『c3s1』『c15』までなら頑張る。何を錬成するといい？」\n");
            sb.Append("部位制限を『全部位可』にして、これらを登録してシミュしてください。\n");
            sb.Append("(スキルの欠けは、『どれか1つをLv-1』を選択してください。)\n");
            sb.Append("候補に出てきた装備が狙い目です。\n");
            sb.Append('\n');
            sb.Append("■使用例２\n");
            sb.Append("「T6で6番目欠けのs4引いたけど何に移植すればいい？」\n");
            sb.Append("部位制限を『一部位限定』にして、これを登録してシミュしてください。\n");
            sb.Append("候補に出てきた装備が狙い目です。\n");
            sb.Append("ただし、検索結果は『移植が成功した場合』の結果であり、移植が成功することを保証するものではありません。\n");
            sb.Append('\n');
            sb.Append("■備考\n");
            sb.Append("・指定したコストより低コストのスキルも計算対象です(c9を1つ指定した場合に、その枠に回復量UP(c3)が入ることもあります)。\n");
            HowToUse.Value = sb.ToString();
        }
    }
}
