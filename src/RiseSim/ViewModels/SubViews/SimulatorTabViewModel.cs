using Prism.Mvvm;
using Reactive.Bindings;
using RiseSim.Const;
using RiseSim.ViewModels.Controls;
using SimModel.model;
using SimModel.service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiseSim.ViewModels.SubViews
{
    class SimulatorTabViewModel : BindableBase
    {

        // MainViewModelから参照を取得
        private Simulator Simulator { get; }
        private ReactivePropertySlim<string> StatusBarText { get; }
        private ReactivePropertySlim<bool> IsBusy { get; }


        // スキル選択部品の個数
        private int SkillSelectorCount { get; } = ViewConfig.Instance.SkillSelectorCount;

        // スロットの最大の大きさ
        private int MaxSlotSize { get; } = ViewConfig.Instance.MaxSlotSize;

        // デフォルトの頑張り度
        private string DefaultLimit { get; } = ViewConfig.Instance.DefaultLimit;

        // スキル未選択時の表示
        private string NoSkillName { get; } = ViewConfig.Instance.NoSkillName;


        // ログ用StringBuilderインスタンス
        private StringBuilder LogSb { get; } = new StringBuilder();


        // スキル選択部品のVM
        public ReactivePropertySlim<ObservableCollection<SkillSelectorViewModel>> SkillSelectorVMs { get; } = new();

        // 検索結果一覧
        public ReactivePropertySlim<ObservableCollection<EquipSet>> SearchResult { get; } = new();

        // 検索結果の選択行
        public ReactivePropertySlim<EquipSet> DetailSet { get; } = new();

        // 追加スキル検索結果
        public ReactivePropertySlim<List<Skill>> ExtraSkills { get; } = new();

        // 最近使ったスキル
        public ReactivePropertySlim<List<string>> RecentSkillNames { get; } = new();

        // 装備詳細の各行のVM
        public ReactivePropertySlim<ObservableCollection<EquipRowViewModel>> EquipRowVMs { get; } = new();

        // 武器スロ指定
        public ReactivePropertySlim<string> WeaponSlots { get; } = new();

        // 頑張り度(検索件数)
        public ReactivePropertySlim<string> Limit { get; } = new();

        // ログ表示
        public ReactivePropertySlim<string> LogBoxText { get; } = new();

        // スロット選択の選択肢
        public ReactivePropertySlim<ObservableCollection<string>> SlotMaster { get; } = new();


        // コマンド
        public AsyncReactiveCommand SearchCommand { get; private set; }
        public AsyncReactiveCommand SearchMoreCommand { get; private set; }
        public AsyncReactiveCommand SearchExtraSkillCommand { get; private set; }
        public ReactiveCommand AddMySetCommand { get; } = new ReactiveCommand();
        public ReactiveCommand ClearAllCommand { get; } = new ReactiveCommand();
        public ReactiveCommand AddExtraSkillCommand { get; } = new ReactiveCommand();
        public ReactiveCommand AddRecentSkillCommand { get; } = new ReactiveCommand();
        // コマンドを設定
        private void SetCommand()
        {
            ReadOnlyReactivePropertySlim<bool> isFree = MainViewModel.Instance.IsFree;
            SearchCommand = isFree.ToAsyncReactiveCommand().WithSubscribe(async () => await Search());
            SearchMoreCommand = isFree.ToAsyncReactiveCommand().WithSubscribe(async () => await SearchMore());
            SearchExtraSkillCommand = isFree.ToAsyncReactiveCommand().WithSubscribe(async () => await SearchExtraSkill());
            AddMySetCommand.Subscribe(_ => AddMySet());
            ClearAllCommand.Subscribe(_ => ClearSearchCondition());
            AddExtraSkillCommand.Subscribe(x => AddSkill(x as Skill));
            AddRecentSkillCommand.Subscribe(x => AddSkill(x as string));
        }

        public SimulatorTabViewModel()
        {
            // MainViewModelから参照を取得
            Simulator = MainViewModel.Instance.Simulator;
            StatusBarText = MainViewModel.Instance.StatusBarText;
            IsBusy = MainViewModel.Instance.IsBusy;

            // シミュ画面のスキル選択部品準備
            ObservableCollection<SkillSelectorViewModel> selectorVMs = new();
            for (int i = 0; i < SkillSelectorCount; i++)
            {
                selectorVMs.Add(new SkillSelectorViewModel());
            }
            SkillSelectorVMs.Value = selectorVMs;

            // シミュ画面の検索結果と装備詳細を紐づけ
            DetailSet.Subscribe(set => EquipRowVMs.Value = EquipRowViewModel.SetToEquipRows(set));

            // スロットの選択肢を生成し、シミュ画面と護石画面に反映
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

            // 頑張り度を設定
            Limit.Value = DefaultLimit;

            // 最近使ったスキル読み込み
            LoadRecentSkills();

            // コマンドを設定
            SetCommand();
        }


        // 検索
        async internal Task Search()
        {
            // スキル条件を整理
            List<Skill> skills = new();
            foreach (var selectorVM in SkillSelectorVMs.Value)
            {
                if (selectorVM.SkillName.Value != NoSkillName)
                {
                    skills.Add(new Skill(selectorVM.SkillName.Value, selectorVM.SkillLevel.Value));
                }
            }

            // 武器スロ条件を整理
            string[] splited = WeaponSlots.Value.Split('-');
            int weaponSlot1 = int.Parse(splited[0]);
            int weaponSlot2 = int.Parse(splited[1]);
            int weaponSlot3 = int.Parse(splited[2]);

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

            // 開始ログ表示
            LogSb.Clear();
            LogSb.Append("■検索開始：\n");
            LogSb.Append("武器スロ");
            LogSb.Append(weaponSlot1);
            LogSb.Append('-');
            LogSb.Append(weaponSlot2);
            LogSb.Append('-');
            LogSb.Append(weaponSlot3);
            LogSb.Append('\n');
            foreach (Skill skill in skills)
            {
                LogSb.Append(skill.Description);
                LogSb.Append('\n');
            }
            LogBoxText.Value = LogSb.ToString();
            StatusBarText.Value = "検索中・・・";

            // ビジーフラグ
            IsBusy.Value = true;

            // 検索
            List<EquipSet> result = await Task.Run(() => Simulator.Search(skills, weaponSlot1, weaponSlot2, weaponSlot3, searchLimit));
            SearchResult.Value = new ObservableCollection<EquipSet>(result);

            // ビジーフラグ解除
            IsBusy.Value = false;

            // 最近使ったスキル再読み込み
            LoadRecentSkills();

            // 完了ログ表示
            LogSb.Append("■検索完了：");
            LogSb.Append(SearchResult.Value.Count);
            LogSb.Append("件\n");
            LogBoxText.Value = LogSb.ToString();
            StatusBarText.Value = "検索完了";
        }

        // もっと検索
        async internal Task SearchMore()
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

            // 開始ログ表示
            LogSb.Clear();
            LogSb.Append("■もっと検索開始：\n");
            LogBoxText.Value = LogSb.ToString();
            StatusBarText.Value = "もっと検索中・・・";

            // ビジーフラグ
            IsBusy.Value = true;

            // もっと検索
            List<EquipSet> result = await Task.Run(() => Simulator.SearchMore(searchLimit));
            SearchResult.Value = new ObservableCollection<EquipSet>(result);

            // ビジーフラグ解除
            IsBusy.Value = false;

            // 完了ログ表示
            LogSb.Append("■もっと検索完了：");
            LogSb.Append(SearchResult.Value.Count);
            LogSb.Append("件\n");
            LogBoxText.Value = LogSb.ToString();
            StatusBarText.Value = "もっと検索完了";
        }

        // 追加スキル検索
        async internal Task SearchExtraSkill()
        {
            // 開始ログ表示
            LogSb.Clear();
            LogSb.Append("■追加スキル検索開始：\n");
            LogBoxText.Value = LogSb.ToString();
            StatusBarText.Value = "追加スキル検索中・・・";

            // ビジーフラグ
            IsBusy.Value = true;

            // 追加スキル検索
            List<Skill> result = await Task.Run(() => Simulator.SearchExtraSkill());
            ExtraSkills.Value = result;

            // ビジーフラグ解除
            IsBusy.Value = false;

            // TODO: 追加スキルの一覧、もっといい方法はないか？
            // ログ表示
            LogSb.Append("■追加スキル検索完了\n");
            foreach (Skill skill in result)
            {
                LogSb.Append(skill.Description);
                LogSb.Append('\n');
            }
            LogBoxText.Value = LogSb.ToString();
            StatusBarText.Value = "追加スキル検索完了";
        }

        // マイセットを追加
        internal void AddMySet()
        {
            EquipSet set = DetailSet.Value;
            if (set == null)
            {
                // 詳細画面が空の状態で実行したなら何もせず終了
                return;
            }

            // 追加
            Simulator.AddMySet(set);

            // マイセットマスタのリロード
            MainViewModel.Instance.LoadMySets();

            // ログ表示
            StatusBarText.Value = "マイセット登録：" + set.SimpleSetName;
        }

        // マイセットのスキルをシミュ画面の検索条件に反映
        internal void InputMySetCondition(EquipSet mySet)
        {
            if (mySet == null)
            {
                // マイセットの詳細画面が空の場合何もせず終了
                return;
            }

            // ログ表示用
            StringBuilder sb = new StringBuilder();

            // スキル入力部品の数以上には実行しない(できない)
            int count = Math.Min(SkillSelectorCount, mySet.Skills.Count);
            for (int i = 0; i < count; i++)
            {
                // スキル情報反映
                SkillSelectorVMs.Value[i].SkillName.Value = mySet.Skills[i].Name;
                SkillSelectorVMs.Value[i].SkillLevel.Value = mySet.Skills[i].Level;

                // ログ表示用
                sb.Append(mySet.Skills[i].Description);
                sb.Append(',');

            }
            for (int i = count; i < SkillSelectorCount; i++)
            {
                // 使わない行は選択状態をリセット
                SkillSelectorVMs.Value[i].SetDefault();
            }

            // スロット情報反映
            WeaponSlots.Value = mySet.WeaponSlotDisp;

            // ログ表示
            StatusBarText.Value = "検索条件反映：" + sb.ToString();
        }

        // 最近使ったスキル読み込み
        private void LoadRecentSkills()
        {
            RecentSkillNames.Value = Masters.RecentSkillNames;
        }

        // 全ての検索条件をクリア
        private void ClearSearchCondition()
        {
            foreach (var vm in SkillSelectorVMs.Value)
            {
                vm.SetDefault();
            }
            WeaponSlots.Value = "0-0-0";
        }

        // スキルを検索条件に追加
        private void AddSkill(Skill? skill)
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
        
        // スキルを検索条件に追加
        private void AddSkill(string? name)
        {
            if (name == null)
            {
                return;
            }

            // 同名スキルがあった場合終了
            foreach (var vm in SkillSelectorVMs.Value)
            {
                if (name.Equals(vm.SkillName.Value))
                {
                    return;
                }
            }

            // 同名スキルがない場合、空欄にスキルを追加して終了
            foreach (var vm in SkillSelectorVMs.Value)
            {
                if (NoSkillName.Equals(vm.SkillName.Value))
                {
                    vm.SkillName.Value = name;
                    return;
                }
            }

            // 同名スキルも空欄もない場合、何もせずに終了
            return;
        }
    }
}
