using Prism.Mvvm;
using Reactive.Bindings;
using RiseSim.Config;
using RiseSim.ViewModels.BindableWrapper;
using RiseSim.ViewModels.Controls;
using SimModel.Model;
using SimModel.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RiseSim.ViewModels.SubViews
{
    internal class MyConditionTabViewModel : BindableBase
    {
        // MainViewModelから参照を取得
        private Simulator Simulator { get; }

        // スキル選択部品の個数
        private int SkillSelectorCount { get; } = ViewConfig.Instance.SkillSelectorCount;

        // スロットの最大の大きさ
        private int MaxSlotSize { get; } = ViewConfig.Instance.MaxSlotSize;

        // スキル未選択時の表示
        private string NoSkillName { get; } = ViewConfig.Instance.NoSkillName;

        // スキル選択部品のVM
        public ReactivePropertySlim<ObservableCollection<SkillSelectorViewModel>> SkillSelectorVMs { get; } = new();

        // 管理用ID
        private string ID { get; set; } = string.Empty;

        // 表示名
        public ReactivePropertySlim<string> DispName { get; } = new(string.Empty);

        // 武器スロ指定
        public ReactivePropertySlim<string> WeaponSlots { get; } = new();

        // 性別指定
        public ReactivePropertySlim<string> SelectedSex { get; } = new();

        // 防御力指定
        public ReactivePropertySlim<string> Def { get; } = new(string.Empty);

        // 火耐性指定
        public ReactivePropertySlim<string> Fire { get; } = new(string.Empty);

        // 水耐性指定
        public ReactivePropertySlim<string> Water { get; } = new(string.Empty);

        // 雷耐性指定
        public ReactivePropertySlim<string> Thunder { get; } = new(string.Empty);

        // 氷耐性指定
        public ReactivePropertySlim<string> Ice { get; } = new(string.Empty);

        // 龍耐性指定
        public ReactivePropertySlim<string> Dragon { get; } = new(string.Empty);

        // スロット選択の選択肢
        public ReactivePropertySlim<ObservableCollection<string>> SlotMaster { get; } = new();

        // 性別選択の選択肢
        public ReactivePropertySlim<ObservableCollection<string>> SexMaster { get; } = new();

        // マイ検索条件
        public ReactivePropertySlim<ObservableCollection<BindableSearchCondition>> Conditions { get; } = new();

        // 選択中の検索条件
        public ReactivePropertySlim<BindableSearchCondition> SelectedCondition { get; } = new();

        // 説明
        public ReactivePropertySlim<string> HowToUse { get; } = new();


        // 検索条件クリアコマンド
        public ReactiveCommand ClearAllCommand { get; } = new ReactiveCommand();

        // マイ検索条件追加コマンド
        public ReactiveCommand AddCommand { get; } = new ReactiveCommand();

        // マイ検索条件削除コマンド
        public ReactiveCommand DeleteCommand { get; } = new ReactiveCommand();

        // マイ検索条件更新コマンド
        public ReactiveCommand UpdateCommand { get; } = new ReactiveCommand();

        // 入力欄に反映コマンド
        public ReactiveCommand InputCommand { get; } = new ReactiveCommand();

        // シミュ画面に反映コマンド
        public ReactiveCommand InputToSimCommand { get; } = new ReactiveCommand();

        // コマンドを設定
        private void SetCommand()
        {
            ClearAllCommand.Subscribe(_ => ClearSearchCondition());
            AddCommand.Subscribe(_ => AddSearchCondition());
            DeleteCommand.Subscribe(_ => DeleteSearchCondition());
            UpdateCommand.Subscribe(_ => UpdateSearchCondition());
            InputCommand.Subscribe(_ => InputSearchCondition());
            InputToSimCommand.Subscribe(_ => InputSearchConditionToSim());

        }

        // コンストラクタ
        public MyConditionTabViewModel()
        {
            // MainViewModelから参照を取得
            Simulator = MainViewModel.Instance.Simulator;

            // スキル選択部品準備
            ObservableCollection<SkillSelectorViewModel> selectorVMs = new();
            for (int i = 0; i < SkillSelectorCount; i++)
            {
                selectorVMs.Add(new SkillSelectorViewModel());
            }
            SkillSelectorVMs.Value = selectorVMs;

            // スロットの選択肢を生成し反映
            ObservableCollection<string> slots = new();
            for (int i = 0; i <= MaxSlotSize; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    for (int k = 0; k <= j; k++)
                    {
                        slots.Add(string.Join('-', i, j, k));
                    }
                }
            }
            SlotMaster.Value = slots;
            WeaponSlots.Value = "0-0-0";

            // 性別の選択肢
            ObservableCollection<string> sexes = new();
            sexes.Add(Sex.male.Str());
            sexes.Add(Sex.female.Str());
            SexMaster.Value = sexes;
            SelectedSex.Value = ViewConfig.Instance.DefaultSex.Str();

            // 初期表示
            LoadMyConditions();

            // コマンドを設定
            SetCommand();

            // 説明
            WriteHowToUse();
        }

        // マイ検索条件読み込み
        public void LoadMyConditions()
        {
            Conditions.Value = BindableSearchCondition.BeBindableList(Masters.MyConditions);
        }

        // 全ての検索条件をクリア
        private void ClearSearchCondition()
        {
            var vms = new List<SkillSelectorViewModel>();
            for (var i = 0; i < SkillSelectorCount; i++)
            {
                vms.Add(new SkillSelectorViewModel());
            }
            SkillSelectorVMs.Value = new ObservableCollection<SkillSelectorViewModel>(vms);
            WeaponSlots.Value = "0-0-0";
            Def.Value = string.Empty;
            Fire.Value = string.Empty;
            Water.Value = string.Empty;
            Thunder.Value = string.Empty;
            Ice.Value = string.Empty;
            Dragon.Value = string.Empty;
        }

        // スキルを検索条件に追加
        private void AddSkill(BindableSkill? skill)
        {
            if (skill == null)
            {
                return;
            }

            // 同名スキルがあった場合、レベルを上書きして終了
            foreach (var vm in SkillSelectorVMs.Value)
            {
                if (skill.Name.Equals(vm.SkillName.Value))
                {
                    vm.SkillLevel.Value = skill.Level;
                    return;
                }
            }

            // 同名スキルがない場合、空欄にスキルを追加して終了
            foreach (var vm in SkillSelectorVMs.Value)
            {
                if (NoSkillName.Equals(vm.SkillName.Value))
                {
                    vm.SkillName.Value = skill.Name;
                    vm.SkillLevel.Value = skill.Level;
                    return;
                }
            }

            // 同名スキルも空欄もない場合、何もせずに終了
            return;
        }

        // マイ検索条件追加
        private void AddSearchCondition()
        {
            SearchCondition condition = new();

            // スキル条件を整理
            List<Skill> skills = new();
            foreach (var selectorVM in SkillSelectorVMs.Value)
            {
                if (selectorVM.SkillName.Value != NoSkillName)
                {
                    skills.Add(new Skill(selectorVM.SkillName.Value, selectorVM.SkillLevel.Value));
                }
            }
            condition.Skills = skills;

            // 武器スロ条件を整理
            string[] splited = WeaponSlots.Value.Split('-');
            condition.WeaponSlot1 = int.Parse(splited[0]);
            condition.WeaponSlot2 = int.Parse(splited[1]);
            condition.WeaponSlot3 = int.Parse(splited[2]);

            // 性別を整理
            condition.Sex = Sex.male;
            if (SelectedSex.Value.Equals(Sex.female.Str()))
            {
                condition.Sex = Sex.female;
            }

            // 防御力・耐性を整理
            condition.Def = ParseOrNull(Def.Value);
            condition.Fire = ParseOrNull(Fire.Value);
            condition.Water = ParseOrNull(Water.Value);
            condition.Thunder = ParseOrNull(Thunder.Value);
            condition.Ice = ParseOrNull(Ice.Value);
            condition.Dragon = ParseOrNull(Dragon.Value);

            // 名前・ID
            condition.ID = Guid.NewGuid().ToString();
            condition.DispName = DispName.Value;

            AddSearchCondition(condition);
        }

        // マイ検索条件追加
        internal void AddSearchCondition(SearchCondition condition)
        {
            Simulator.AddMyCondition(condition);
            LoadMyConditions();
        }

        // マイ検索条件削除
        private void DeleteSearchCondition()
        {
            MessageBoxResult result = MessageBox.Show(
                $"マイ検索条件「{SelectedCondition.Value.DispName}」を削除します。\nよろしいですか？",
                "マイ検索条件削除",
                MessageBoxButton.YesNo);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            Simulator.DeleteMyCondition(SelectedCondition.Value.Original);
            LoadMyConditions();
        }

        // マイ検索条件更新
        private void UpdateSearchCondition()
        {
            if (SelectedCondition.Value == null)
            {
                return;
            }

            SearchCondition condition = new();

            // スキル条件を整理
            List<Skill> skills = new();
            foreach (var selectorVM in SkillSelectorVMs.Value)
            {
                if (selectorVM.SkillName.Value != NoSkillName)
                {
                    skills.Add(new Skill(selectorVM.SkillName.Value, selectorVM.SkillLevel.Value));
                }
            }
            condition.Skills = skills;

            // 武器スロ条件を整理
            string[] splited = WeaponSlots.Value.Split('-');
            condition.WeaponSlot1 = int.Parse(splited[0]);
            condition.WeaponSlot2 = int.Parse(splited[1]);
            condition.WeaponSlot3 = int.Parse(splited[2]);

            // 性別を整理
            condition.Sex = Sex.male;
            if (SelectedSex.Value.Equals(Sex.female.Str()))
            {
                condition.Sex = Sex.female;
            }

            // 防御力・耐性を整理
            condition.Def = ParseOrNull(Def.Value);
            condition.Fire = ParseOrNull(Fire.Value);
            condition.Water = ParseOrNull(Water.Value);
            condition.Thunder = ParseOrNull(Thunder.Value);
            condition.Ice = ParseOrNull(Ice.Value);
            condition.Dragon = ParseOrNull(Dragon.Value);

            // 名前・ID
            condition.ID = SelectedCondition.Value.ID;
            condition.DispName = DispName.Value;

            Simulator.UpdateMyCondition(condition);
            LoadMyConditions();
        }

        // 選択中のマイ検索条件を入力欄に反映
        private void InputSearchCondition()
        {
            if (SelectedCondition.Value == null)
            {
                return;
            }

            SearchCondition selected = SelectedCondition.Value.Original;

            // マイ検索条件の内容を反映したスキル入力部品を用意
            var vms = new List<SkillSelectorViewModel>();
            for (int i = 0; i < selected.Skills.Count; i++)
            {
                if (selected.Skills[i].Level <= 0)
                {
                    continue;
                }
                // スキル情報反映
                vms.Add(new SkillSelectorViewModel(selected.Skills[i]));
            }
            for (var i = vms.Count; i < SkillSelectorCount; i++)
            {
                vms.Add(new SkillSelectorViewModel());
            }
            SkillSelectorVMs.Value = new ObservableCollection<SkillSelectorViewModel>(vms);

            // スロット情報反映
            WeaponSlots.Value = selected.WeaponSlot1 + "-" + selected.WeaponSlot2 + "-" + selected.WeaponSlot3;

            // 性別反映
            SelectedSex.Value = selected.Sex.Str();

            // 防御力・耐性を反映
            Def.Value = selected.Def?.ToString() ?? string.Empty;
            Fire.Value = selected.Fire?.ToString() ?? string.Empty;
            Water.Value = selected.Water?.ToString() ?? string.Empty;
            Thunder.Value = selected.Thunder?.ToString() ?? string.Empty;
            Ice.Value = selected.Ice?.ToString() ?? string.Empty;
            Dragon.Value = selected.Dragon?.ToString() ?? string.Empty;

            // 名前・ID
            ID = selected.ID;
            DispName.Value = selected.DispName;
        }

        // 選択中のマイ検索条件を入力欄に反映
        private void InputSearchConditionToSim()
        {
            if (SelectedCondition.Value == null)
            {
                return;
            }

            MainViewModel.Instance.InputMyCondition(SelectedCondition.Value.Original);
        }

        // 説明
        private void WriteHowToUse()
        {
            StringBuilder sb = new();
            sb.Append("■ナニコレ\n");
            sb.Append("よく使う検索条件を登録しておき、シミュレータ画面に反映させることができます。\n");
            sb.Append("この画面で登録・削除・編集ができるほか、シミュレータ画面の検索条件の右クリックメニューからも登録可能です。\n");
            HowToUse.Value = sb.ToString();
        }

        // TODO:共通化したい
        // int.Parseを実施
        // 変換できなかった場合nullを返す
        private int? ParseOrNull(string param)
        {
            if (int.TryParse(param, out int result))
            {
                return result;
            }
            return null;
        }

    }
}
