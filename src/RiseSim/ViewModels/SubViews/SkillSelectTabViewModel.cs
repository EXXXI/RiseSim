﻿using Prism.Mvvm;
using Reactive.Bindings;
using RiseSim.Config;
using RiseSim.Exceptions;
using RiseSim.ViewModels.BindableWrapper;
using RiseSim.ViewModels.Controls;
using RiseSim.Views.SubViews;
using SimModel.Model;
using SimModel.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiseSim.ViewModels.SubViews
{
    internal class SkillSelectTabViewModel : BindableBase
    {
        const int ExSkillTabIndex = 1;

        // MainViewModelから参照を取得
        private Simulator Simulator { get; }
        private ReactivePropertySlim<string> StatusBarText { get; }
        private ReactivePropertySlim<bool> IsBusy { get; }


        // スロットの最大の大きさ
        private int MaxSlotSize { get; } = ViewConfig.Instance.MaxSlotSize;

        // デフォルトの頑張り度
        private string DefaultLimit { get; } = ViewConfig.Instance.DefaultLimit;

        // 追加スキル検索結果用VM
        public ReactivePropertySlim<ObservableCollection<SkillAdderViewModel>> ExtraSkillVMs { get; } = new();

        // 最近使ったスキル用VM
        public ReactivePropertySlim<ObservableCollection<SkillAdderViewModel>> RecentSkillVMs { get; } = new();

        //スキルカテゴリ表示部品のVM
        public ReactivePropertySlim<ObservableCollection<SkillPickerContainerViewModel>> SkillPickerContainerVMs { get; } = new();

        // マイ検索条件表示部品のVM
        public ReactivePropertySlim<ObservableCollection<MyConditionRowViewModel>> MyConditionVMs { get; } = new();

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

        // 頑張り度(検索件数)
        public ReactivePropertySlim<string> Limit { get; } = new();

        // 頑張り度(検索件数)
        public ReactivePropertySlim<int> SelectedTabIndex { get; } = new();

        // スロット選択の選択肢
        public ReactivePropertySlim<ObservableCollection<string>> SlotMaster { get; } = new();

        // 性別選択の選択肢
        public ReactivePropertySlim<ObservableCollection<string>> SexMaster { get; } = new();

        // 理想錬成利用フラグ
        public ReactivePropertySlim<bool> IsIncludeIdeal { get; } = new(false);

        // 通常装備優先フラグ
        public ReactivePropertySlim<bool> IsPrioritizeNoIdeal { get; } = new(false);

        // 通常装備優先フラグ
        public ReactivePropertySlim<bool> IsExcludeAbstract { get; } = new(false);

        // 検索コマンド
        public AsyncReactiveCommand SearchCommand { get; private set; }

        // 追加スキル検索コマンド
        public AsyncReactiveCommand SearchExtraSkillCommand { get; private set; }

        // 検索キャンセルコマンド
        public ReactiveCommand CancelCommand { get; private set; }

        // 検索条件クリアコマンド
        public ReactiveCommand ClearAllCommand { get; } = new ReactiveCommand();

        // マイ検索条件追加コマンド
        public ReactiveCommand AddMyConditionCommand { get; } = new ReactiveCommand();

        // コマンドを設定
        private void SetCommand()
        {
            ReadOnlyReactivePropertySlim<bool> isFree = MainViewModel.Instance.IsFree;
            SearchCommand = isFree.ToAsyncReactiveCommand().WithSubscribe(async () => await Search());
            SearchExtraSkillCommand = isFree.ToAsyncReactiveCommand().WithSubscribe(async () => await SearchExtraSkill());
            CancelCommand = IsBusy.ToReactiveCommand().WithSubscribe(() => Cancel());
            ClearAllCommand.Subscribe(_ => ClearSearchCondition());
            AddMyConditionCommand.Subscribe(_ => AddMyCondition());
        }

        private async Task Search()
        {
            // 頑張り度を整理
            int searchLimit;
            try
            {
                searchLimit = int.Parse(Limit.Value);
            }
            catch (Exception)
            {
                // 数値以外が入力されていたら初期値を利用
                searchLimit = int.Parse(DefaultLimit);
            }

            // 検索条件を整理
            SearchCondition condition = MakeCondition();

            // 開始ログ表示
            //LogSb.Clear();
            //LogSb.Append("■検索開始：\n");
            //LogSb.Append("武器スロ");
            //LogSb.Append(condition.WeaponSlot1);
            //LogSb.Append('-');
            //LogSb.Append(condition.WeaponSlot2);
            //LogSb.Append('-');
            //LogSb.Append(condition.WeaponSlot3);
            //LogSb.Append('\n');
            //foreach (Skill skill in condition.Skills)
            //{
            //    LogSb.Append(skill.Description);
            //    LogSb.Append('\n');
            //}
            //LogBoxText.Value = LogSb.ToString();
            StatusBarText.Value = "検索中・・・";

            // ビジーフラグ
            IsBusy.Value = true;

            // 検索
            List<EquipSet> result = await Task.Run(() => Simulator.Search(condition, searchLimit));
            //SearchResult.Value = BindableEquipSet.BeBindableList(result);

            // ビジーフラグ解除
            IsBusy.Value = false;

            // 最近使ったスキル再読み込み
            LoadRecentSkills();

            // 完了ログ表示
            //if (Simulator.IsCanceling)
            //{
            //    LogSb.Clear();
            //    LogSb.Append("※中断しました\n");
            //    LogSb.Append("※結果は途中経過までを表示しています\n");
            //    LogBoxText.Value = LogSb.ToString();
            //    StatusBarText.Value = "中断";
            //    return;
            //}
            //LogSb.Append("■検索完了：");
            //LogSb.Append(SearchResult.Value.Count);
            //LogSb.Append("件\n");
            //if (Simulator.IsSearchedAll)
            //{
            //    LogSb.Append("これで全件です\n");
            //}
            //else
            //{
            //    LogSb.Append("結果が多いため検索を打ち切りました\n");
            //    LogSb.Append("続きを検索するには「もっと検索」をクリックしてください\n");
            //}
            //LogBoxText.Value = LogSb.ToString();
            StatusBarText.Value = "検索完了";

            MainViewModel.Instance.ShowSearchResult(result, Simulator.IsCanceling || !Simulator.IsSearchedAll, searchLimit);
        }

        private async Task SearchExtraSkill()
        {
            // 開始ログ表示
            //LogSb.Clear();
            //LogSb.Append("■追加スキル検索開始：\n");
            //LogBoxText.Value = LogSb.ToString();
            StatusBarText.Value = "追加スキル検索中・・・";

            // ビジーフラグ
            IsBusy.Value = true;

            // 追加スキル検索
            SearchCondition condition = MakeCondition();
            List<Skill> result = await Task.Run(() => Simulator.SearchExtraSkill(condition));

            var groups = result.GroupBy(skill => skill.Name);

            ExtraSkillVMs.Value = new ObservableCollection<SkillAdderViewModel>(
                groups.Select(group => new SkillAdderViewModel(group.Key, group.Select(skill => skill.Level))));




            //ExtraSkills.Value = BindableSkill.BeBindableList(result);

            // ビジーフラグ解除
            IsBusy.Value = false;

            // ログ表示
            //if (Simulator.IsCanceling)
            //{
            //    LogSb.Append("※中断しました\n");
            //    LogSb.Append("※結果は途中経過までを表示しています\n");
            //}
            //LogSb.Append("■追加スキル検索結果\n");
            //foreach (Skill skill in result)
            //{
            //    LogSb.Append(skill.Description);
            //    LogSb.Append('\n');
            //}
            //LogBoxText.Value = LogSb.ToString();
            StatusBarText.Value = "追加スキル検索完了";

            SelectedTabIndex.Value = ExSkillTabIndex;
        }

        private void Cancel()
        {
            Simulator.Cancel();
        }

        private void ClearSearchCondition()
        {
            foreach (var vm in SkillPickerContainerVMs.Value)
            {
                vm.ClearAll();
            }
            WeaponSlots.Value = "0-0-0";
            Def.Value = string.Empty;
            Fire.Value = string.Empty;
            Water.Value = string.Empty;
            Thunder.Value = string.Empty;
            Ice.Value = string.Empty;
            Dragon.Value = string.Empty;
        }

        // コンストラクタ
        public SkillSelectTabViewModel()
        {
            // MainViewModelから参照を取得
            Simulator = MainViewModel.Instance.Simulator;
            StatusBarText = MainViewModel.Instance.StatusBarText;
            IsBusy = MainViewModel.Instance.IsBusy;

            // 
            SkillPickerContainerVMs.Value = new ObservableCollection<SkillPickerContainerViewModel>(
                Masters.Skills
                    .GroupBy(s => s.Category)
                    .Select(g => new SkillPickerContainerViewModel(g.Key, g))
            );

            // スロットの選択肢を生成し、画面に反映
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

            // 頑張り度を設定
            Limit.Value = DefaultLimit;

            // 最近使ったスキル読み込み
            LoadRecentSkills();

            // マイ検索条件
            LoadMyCondition();

            // コマンドを設定
            SetCommand();
        }

        public void LoadMyCondition()
        {
            List<SearchCondition> conditions = Masters.MyConditions;
            MyConditionVMs.Value = new ObservableCollection<MyConditionRowViewModel>(
                Masters.MyConditions.Select(condition => new MyConditionRowViewModel(condition))
            );
        }

        // 最近使ったスキル読み込み
        private void LoadRecentSkills()
        {
            var recentSkills = Masters.RecentSkillNames.Join(
                Masters.Skills, r => r, s => s.Name,
                (r, s) => new
                {
                    Name = s.Name,
                    Range = Enumerable.Range(1, s.Level)
                });

            RecentSkillVMs.Value = new ObservableCollection<SkillAdderViewModel>(recentSkills.Select(skill => new SkillAdderViewModel(skill.Name,skill.Range)));
        }

        // 検索条件インスタンスを作成
        private SearchCondition MakeCondition()
        {
            SearchCondition condition = new();

            // スキル条件を整理
            condition.Skills = SkillPickerContainerVMs.Value
                .SelectMany(vm => vm.SelectedSkills())
                .ToList();

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

            // 理想錬成の有無
            condition.IncludeIdealAugmentation = IsIncludeIdeal.Value;

            // 通常装備優先の有無
            condition.PrioritizeNoIdeal = IsPrioritizeNoIdeal.Value;

            // 通常装備で組める場合の除外有無
            condition.ExcludeAbstract = IsExcludeAbstract.Value;

            // 名前・ID
            condition.ID = Guid.NewGuid().ToString();
            condition.DispName = "検索条件";

            return condition;
        }

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

        
        internal void AddSkill(string name, int level)
        {

            var isSuccess = SkillPickerContainerVMs.Value
                .Select(vm => vm.TryAddSkill(name, level))
                .Contains(true);
        }

        // マイセットのスキルをシミュ画面の検索条件に反映
        internal void InputMySetCondition(EquipSet? mySet)
        {
            if (mySet == null)
            {
                // マイセットの詳細画面が空の場合何もせず終了
                return;
            }

            // ログ表示用
            //StringBuilder sb = new StringBuilder();

            foreach (var vm in SkillPickerContainerVMs.Value)
            {
                vm.ClearAll();
                vm.TryAddSkill(mySet.Skills);
            }

            //// マイセットの内容を反映したスキル入力部品を用意
            //var vms = new List<SkillSelectorViewModel>();
            //for (int i = 0; i < mySet.Skills.Count; i++)
            //{
            //    if (mySet.Skills[i].Level <= 0)
            //    {
            //        continue;
            //    }

            //    // スキル情報反映
            //    vms.Add(new SkillSelectorViewModel(mySet.Skills[i], SkillSelectorKind.WithFixs));

            //    // ログ表示用
            //    sb.Append(mySet.Skills[i].Description);
            //    sb.Append(',');
            //}
            //for (var i = vms.Count; i < SkillSelectorCount; i++)
            //{
            //    vms.Add(new SkillSelectorViewModel(SkillSelectorKind.WithFixs));
            //}
            //SkillSelectorVMs.Value = new ObservableCollection<SkillSelectorViewModel>(vms);

            // スロット情報反映
            WeaponSlots.Value = mySet.WeaponSlotDisp;

            // ログ表示
            //StatusBarText.Value = "検索条件反映：" + sb.ToString();
        }

        internal void ApplyMyCondition(SearchCondition condition)
        {
            // スキル
            foreach (var vm in SkillPickerContainerVMs.Value)
            {
                vm.ClearAll();
                vm.TryAddSkill(condition.Skills);
            }

            // スロット情報反映
            WeaponSlots.Value = condition.WeaponSlot1 + "-" + condition.WeaponSlot2 + "-" + condition.WeaponSlot3;

            // 性別を反映
            SelectedSex.Value = condition.Sex.Str();

            // 防御力・耐性を反映
            Def.Value = condition.Def?.ToString() ?? string.Empty;
            Fire.Value = condition.Fire?.ToString() ?? string.Empty;
            Water.Value = condition.Water?.ToString() ?? string.Empty;
            Thunder.Value = condition.Thunder?.ToString() ?? string.Empty;
            Ice.Value = condition.Ice?.ToString() ?? string.Empty;
            Dragon.Value = condition.Dragon?.ToString() ?? string.Empty;
        }

        internal void AddMyCondition()
        {
            SearchCondition condition = MakeCondition();
            string condName = string.Empty;
            bool hasSameName = true;
            for (int i = 1; hasSameName; i++)
            {
                condName = "検索条件" + i;
                hasSameName = Masters.MyConditions.Any(cond => cond.DispName == condName);
            }
            condition.DispName = condName;
            Simulator.AddMyCondition(condition);
            LoadMyCondition();
        }
    }
}
