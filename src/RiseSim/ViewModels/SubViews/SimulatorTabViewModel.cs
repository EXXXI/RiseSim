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
using RiseSim.Views.SubViews;
using System.Windows;

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

        // ログ用StringBuilderインスタンス
        private StringBuilder LogSb { get; } = new StringBuilder();


        // スキル選択部品のVM
        public ReactivePropertySlim<ObservableCollection<SkillSelectorViewModel>> SkillSelectorVMs { get; } = new();

        // 検索結果一覧
        public ReactivePropertySlim<ObservableCollection<BindableEquipSet>> SearchResult { get; } = new();

        // 検索結果の選択行
        public ReactivePropertySlim<BindableEquipSet> DetailSet { get; } = new();

        // 追加スキル検索結果
        public ReactivePropertySlim<ObservableCollection<BindableSkill>> ExtraSkills { get; } = new();

        // 最近使ったスキル
        public ReactivePropertySlim<ObservableCollection<string>> RecentSkillNames { get; } = new();

        // 装備詳細の各行のVM
        public ReactivePropertySlim<ObservableCollection<EquipRowViewModel>> EquipRowVMs { get; } = new();

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

        // 頑張り度(前回の検索の値を保存)
        public int Limit { get; set; }

        // ログ表示
        public ReactivePropertySlim<string> LogBoxText { get; } = new();

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

        // もっと検索可能フラグ
        public ReactivePropertySlim<bool> IsRemaining { get; } = new(false);

        // 検索コマンド
        public AsyncReactiveCommand SearchCommand { get; private set; }

        // もっと検索コマンド
        public AsyncReactiveCommand SearchMoreCommand { get; private set; }

        // 追加スキル検索コマンド
        public AsyncReactiveCommand SearchExtraSkillCommand { get; private set; }

        // 錬成パターン検索コマンド
        public AsyncReactiveCommand SearchPatternCommand { get; private set; }
        
        // 検索キャンセルコマンド
        public ReactiveCommand CancelCommand { get; private set; }

        // マイセット追加コマンド
        public ReactiveCommand AddMySetCommand { get; } = new ReactiveCommand();

        // マイ検索条件追加コマンド
        public ReactiveCommand AddMyConditionCommand { get; } = new ReactiveCommand();

        // 検索条件クリアコマンド
        public ReactiveCommand ClearAllCommand { get; } = new ReactiveCommand();

        // 追加スキル検索結果から検索条件へスキルを追加するコマンド
        public ReactiveCommand AddExtraSkillCommand { get; } = new ReactiveCommand();

        // 最近使ったスキルから検索条件へスキルを追加するコマンド
        public ReactiveCommand AddRecentSkillCommand { get; } = new ReactiveCommand();

        // 防具を除外するコマンド
        public ReactiveCommand ExcludeCommand { get; } = new ReactiveCommand();

        // 防具を固定するコマンド
        public ReactiveCommand IncludeCommand { get; } = new ReactiveCommand();

        // スキルピッカー起動コマンド
        public ReactiveCommand LaunchSkillPickerCommand { get; } = new();

        // コマンドを設定
        private void SetCommand()
        {
            ReadOnlyReactivePropertySlim<bool> isFree = MainViewModel.Instance.IsFree;
            SearchCommand = isFree.ToAsyncReactiveCommand().WithSubscribe(async () => await Search());
            SearchMoreCommand = isFree.ToAsyncReactiveCommand().WithSubscribe(async () => await SearchMore());
            SearchExtraSkillCommand = isFree.ToAsyncReactiveCommand().WithSubscribe(async () => await SearchExtraSkill());
            SearchPatternCommand = isFree.ToAsyncReactiveCommand().WithSubscribe(async () => await SearchPattern());
            CancelCommand = IsBusy.ToReactiveCommand().WithSubscribe(() => Cancel());
            AddMySetCommand.Subscribe(() => AddMySet());
            AddMyConditionCommand.Subscribe(() => AddMyCondition());
            ClearAllCommand.Subscribe(_ => ClearSearchCondition());
            AddExtraSkillCommand.Subscribe(x => AddSkill(x as BindableSkill));
            AddRecentSkillCommand.Subscribe(x => AddSkill(x as string));
            ExcludeCommand.Subscribe(x => Exclude(x as BindableEquipment));
            IncludeCommand.Subscribe(x => Include(x as BindableEquipment));
            IsIncludeIdeal.Subscribe(x =>
            {
                if (x == false)
                {
                    IsPrioritizeNoIdeal.Value = false;
                }
            });
            IsPrioritizeNoIdeal.Subscribe(x =>
            {
                if (x == false)
                {
                    IsExcludeAbstract.Value = false;
                }
            });
            LaunchSkillPickerCommand.Subscribe(_ =>
            {
                var picker = new SkillPickerWindowView();
                using var pickerViewModel = new SkillPickerWindowViewModel(
                    // SkillSelectorVMsですでに選択しているスキルをスキルピッカーに反映
                    SkillSelectorVMs.Value
                        .Where(vm => Masters.IsSkillName(vm.SkillName.Value))
                        .Select(vm => vm.GetSelectedSkill())
                );
                pickerViewModel.OnAccept += skills =>
                {
                    // ピッカーで選んだスキルをSkillSelectorVMsに反映する
                    var vms = new List<SkillSelectorViewModel>(skills.Select(s => new SkillSelectorViewModel(s, SkillSelectorKind.WithFixs)));
                    for (var i = vms.Count; i < SkillSelectorCount; i++)
                    { 
                        vms.Add(new SkillSelectorViewModel(SkillSelectorKind.WithFixs));
                    }
                    SkillSelectorVMs.Value = new ObservableCollection<SkillSelectorViewModel>(vms);

                    picker.Close();
                };

                pickerViewModel.OnCancel += () => picker.Close();
                picker.DataContext = pickerViewModel;

                picker.ShowDialog();
            });
        }

        // 装備除外
        private void Exclude(BindableEquipment? equip)
        {
            if (equip != null)
            {
                MainViewModel.Instance.AddExclude(equip.Name, equip.DispName);
            }
        }

        // 装備固定
        private void Include(BindableEquipment? equip)
        {
            if (equip != null)
            {
                MainViewModel.Instance.AddInclude(equip.Name, equip.DispName);
            }
        }

        // コンストラクタ
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
                selectorVMs.Add(new SkillSelectorViewModel(SkillSelectorKind.WithFixs));
            }
            SkillSelectorVMs.Value = selectorVMs;

            // シミュ画面の検索結果と装備詳細を紐づけ
            DetailSet.Subscribe(set => {
                EquipRowVMs.Value = EquipRowViewModel.SetToEquipRows(set);
            });

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

            // 性別の選択肢
            ObservableCollection<string> sexes = new();
            sexes.Add(Sex.male.Str());
            sexes.Add(Sex.female.Str());
            SexMaster.Value = sexes;
            SelectedSex.Value = ViewConfig.Instance.DefaultSex.Str();

            // 頑張り度を設定
            //Limit.Value = DefaultLimit;

            // 最近使ったスキル読み込み
            LoadRecentSkills();

            // コマンドを設定
            SetCommand();
        }


        // 検索
        async internal Task Search()
        {
            // 頑張り度を整理
            int searchLimit;
            try
            {
                //searchLimit = int.Parse(Limit.Value);
            }
            catch (Exception)
            {
                // 数値以外が入力されていたら初期値を利用
                searchLimit = int.Parse(DefaultLimit);
            }

            // 検索条件を整理
            SearchCondition condition = MakeCondition();

            // 開始ログ表示
            LogSb.Clear();
            LogSb.Append("■検索開始：\n");
            LogSb.Append("武器スロ");
            LogSb.Append(condition.WeaponSlot1);
            LogSb.Append('-');
            LogSb.Append(condition.WeaponSlot2);
            LogSb.Append('-');
            LogSb.Append(condition.WeaponSlot3);
            LogSb.Append('\n');
            foreach (Skill skill in condition.Skills)
            {
                LogSb.Append(skill.Description);
                LogSb.Append('\n');
            }
            LogBoxText.Value = LogSb.ToString();
            StatusBarText.Value = "検索中・・・";

            // ビジーフラグ
            IsBusy.Value = true;

            // 検索
            //List<EquipSet> result = await Task.Run(() => Simulator.Search(condition, searchLimit));
            //SearchResult.Value = BindableEquipSet.BeBindableList(result);

            // ビジーフラグ解除
            IsBusy.Value = false;

            // 最近使ったスキル再読み込み
            LoadRecentSkills();

            // 完了ログ表示
            if (Simulator.IsCanceling)
            {
                LogSb.Clear();
                LogSb.Append("※中断しました\n");
                LogSb.Append("※結果は途中経過までを表示しています\n");
                LogBoxText.Value = LogSb.ToString();
                StatusBarText.Value = "中断";
                return;
            }
            LogSb.Append("■検索完了：");
            LogSb.Append(SearchResult.Value.Count);
            LogSb.Append("件\n");
            if (Simulator.IsSearchedAll)
            {
                LogSb.Append("これで全件です\n");
            }
            else
            {
                LogSb.Append("結果が多いため検索を打ち切りました\n");
                LogSb.Append("続きを検索するには「もっと検索」をクリックしてください\n");
            }
            LogBoxText.Value = LogSb.ToString();
            StatusBarText.Value = "検索完了";
        }

        // もっと検索
        async internal Task SearchMore()
        {
            // 開始ログ表示
            LogSb.Clear();
            LogSb.Append("■もっと検索開始：\n");
            LogBoxText.Value = LogSb.ToString();
            StatusBarText.Value = "もっと検索中・・・";

            // ビジーフラグ
            IsBusy.Value = true;

            // もっと検索
            List<EquipSet> result = await Task.Run(() => Simulator.SearchMore(Limit));
            SearchResult.Value = BindableEquipSet.BeBindableList(result);

            // ビジーフラグ解除
            IsBusy.Value = false;

            // 完了ログ表示
            if (Simulator.IsCanceling)
            {
                LogSb.Clear();
                LogSb.Append("※中断しました\n");
                LogSb.Append("※結果は途中経過までを表示しています\n");
                LogBoxText.Value = LogSb.ToString();
                StatusBarText.Value = "中断";
                return;
            }
            LogSb.Append("■もっと検索完了：");
            LogSb.Append(SearchResult.Value.Count);
            LogSb.Append("件\n");
            if (Simulator.IsSearchedAll)
            {
                LogSb.Append("これで全件です\n");
            }
            else
            {
                LogSb.Append("結果が多いため検索を打ち切りました\n");
                LogSb.Append("続きを検索するには「もっと検索」をクリックしてください\n");
            }
            LogBoxText.Value = LogSb.ToString();

            IsRemaining.Value = Simulator.IsCanceling || !Simulator.IsSearchedAll;

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
            ExtraSkills.Value = BindableSkill.BeBindableList(result);

            // ビジーフラグ解除
            IsBusy.Value = false;

            // ログ表示
            if (Simulator.IsCanceling)
            {
                LogSb.Append("※中断しました\n");
                LogSb.Append("※結果は途中経過までを表示しています\n");
            }
            LogSb.Append("■追加スキル検索結果\n");
            foreach (Skill skill in result)
            {
                LogSb.Append(skill.Description);
                LogSb.Append('\n');
            }
            LogBoxText.Value = LogSb.ToString();
            StatusBarText.Value = "追加スキル検索完了";
        }


        // 錬成パターン検索
        async internal Task SearchPattern()
        {
            if (DetailSet.Value == null)
            {
                return;
            }

            // 開始ログ表示
            LogSb.Clear();
            LogSb.Append("■錬成パターン検索：\n");
            LogBoxText.Value = LogSb.ToString();
            StatusBarText.Value = "錬成パターン検索中・・・";

            // ビジーフラグ
            IsBusy.Value = true;

            // 錬成スキル枠のカウントアップ
            int gskillCount = 0;
            for (int i = 0; i < 5; i++)
            {
                gskillCount += DetailSet.Value.Head.Original.GenericSkills[i];
            }
            for (int i = 0; i < 5; i++)
            {
                gskillCount += DetailSet.Value.Body.Original.GenericSkills[i];
            }
            for (int i = 0; i < 5; i++)
            {
                gskillCount += DetailSet.Value.Arm.Original.GenericSkills[i];
            }
            for (int i = 0; i < 5; i++)
            {
                gskillCount += DetailSet.Value.Waist.Original.GenericSkills[i];
            }
            for (int i = 0; i < 5; i++)
            {
                gskillCount += DetailSet.Value.Leg.Original.GenericSkills[i];
            }

            // 注意
            bool canceled = false;
            if (gskillCount < 1)
            {
                MessageBoxResult mesResult = MessageBox.Show(
                    $"錬成スキルの枠がないので計算できません。",
                    "錬成パターン検索",
                    MessageBoxButton.OK);
                IsBusy.Value = false;
                return;
            }
            else if (gskillCount == 3)
            {
                MessageBoxResult mesResult = MessageBox.Show(
                    $"錬成スキルの枠が多いので少し時間がかかります。\nよろしいですか？",
                    "錬成パターン検索",
                    MessageBoxButton.YesNo);
                if (mesResult == MessageBoxResult.No)
                {
                    IsBusy.Value = false;
                    return;
                }
            }
            else if (gskillCount > 3)
            {
                MessageBoxResult mesResult = MessageBox.Show(
                    $"錬成スキルの枠が多いのでかなり時間がかかります。\nよろしいですか？",
                    "錬成パターン検索",
                    MessageBoxButton.YesNo);
                if (mesResult == MessageBoxResult.No)
                {
                    IsBusy.Value = false;
                    return;
                }
            }

            // 錬成パターン検索
            List<EquipSet> result = await Task.Run(() => Simulator.SearchOtherGenericSkillPattern(DetailSet.Value.Original));
            SearchResult.Value = BindableEquipSet.BeBindableList(result);

            // ビジーフラグ解除
            IsBusy.Value = false;

            // ログ表示
            if (Simulator.IsCanceling)
            {
                LogSb.Append("※中断しました\n");
                LogSb.Append("※結果は途中経過までを表示しています\n");
            }
            LogSb.Append("■錬成パターン検索完了\n");
            LogBoxText.Value = LogSb.ToString();
            StatusBarText.Value = "錬成パターン検索完了";
        }

        // マイセットを追加
        internal void AddMySet()
        {
            EquipSet? set = DetailSet.Value?.Original;
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
        internal void InputMySetCondition(EquipSet? mySet)
        {
            if (mySet == null)
            {
                // マイセットの詳細画面が空の場合何もせず終了
                return;
            }

            // ログ表示用
            StringBuilder sb = new StringBuilder();

            // マイセットの内容を反映したスキル入力部品を用意
            var vms = new List<SkillSelectorViewModel>();
            for (int i = 0; i < mySet.Skills.Count; i++)
            {
                if (mySet.Skills[i].Level <= 0)
                {
                    continue;
                }

                // スキル情報反映
                vms.Add(new SkillSelectorViewModel(mySet.Skills[i], SkillSelectorKind.WithFixs));

                // ログ表示用
                sb.Append(mySet.Skills[i].Description);
                sb.Append(',');
            }
            for (var i = vms.Count; i < SkillSelectorCount; i++)
            {
                vms.Add(new SkillSelectorViewModel(SkillSelectorKind.WithFixs));
            }
            SkillSelectorVMs.Value = new ObservableCollection<SkillSelectorViewModel>(vms);

            // スロット情報反映
            WeaponSlots.Value = mySet.WeaponSlotDisp;

            // ログ表示
            StatusBarText.Value = "検索条件反映：" + sb.ToString();
        }

        // マイ検索条件をシミュ画面の検索条件に反映
        internal void InputMyCondition(SearchCondition condition)
        {
            if (condition == null)
            {
                // 空の場合何もせず終了
                return;
            }

            // ログ表示用
            StringBuilder sb = new StringBuilder();

            // マイ検索条件の内容を反映したスキル入力部品を用意
            var vms = new List<SkillSelectorViewModel>();
            for (int i = 0; i < condition.Skills.Count; i++)
            {
                if (condition.Skills[i].Level <= 0)
                {
                    continue;
                }
                // スキル情報反映
                vms.Add(new SkillSelectorViewModel(condition.Skills[i], SkillSelectorKind.WithFixs));

                // ログ表示用
                sb.Append(condition.Skills[i].Description);
                sb.Append(',');
            }
            for (var i = vms.Count; i < SkillSelectorCount; i++)
            {
                vms.Add(new SkillSelectorViewModel(SkillSelectorKind.WithFixs));
            }
            SkillSelectorVMs.Value = new ObservableCollection<SkillSelectorViewModel>(vms);

            // スロット情報反映
            WeaponSlots.Value = condition.WeaponSlot1 + "-" + condition.WeaponSlot2 + "-" + condition.WeaponSlot3;

            // 性別反映
            SelectedSex.Value = condition.Sex.Str();

            // 防御力・耐性を反映
            Def.Value = condition.Def?.ToString() ?? string.Empty;
            Fire.Value = condition.Fire?.ToString() ?? string.Empty;
            Water.Value = condition.Water?.ToString() ?? string.Empty;
            Thunder.Value = condition.Thunder?.ToString() ?? string.Empty;
            Ice.Value = condition.Ice?.ToString() ?? string.Empty;
            Dragon.Value = condition.Dragon?.ToString() ?? string.Empty;

            // ログ表示
            StatusBarText.Value = "検索条件反映：" + sb.ToString();
        }

        internal void Cancel()
        {
            Simulator.Cancel();
        }

        // 最近使ったスキル読み込み
        private void LoadRecentSkills()
        {
            RecentSkillNames.Value = new ObservableCollection<string>(Masters.RecentSkillNames);
        }

        // 全ての検索条件をクリア
        private void ClearSearchCondition()
        {
            var vms = new List<SkillSelectorViewModel>();
            for (var i = 0; i < SkillSelectorCount; i++)
            {
                vms.Add(new SkillSelectorViewModel(SkillSelectorKind.WithFixs));
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

        // マイ検索条件を追加
        private void AddMyCondition()
        {
            MainViewModel.Instance.AddMyCondition(MakeCondition());
        }

        // 検索条件インスタンスを作成
        private SearchCondition MakeCondition()
        {
            SearchCondition condition = new();

            // スキル条件を整理
            condition.Skills = new();
            foreach (var selectorVM in SkillSelectorVMs.Value)
            {
                if (Masters.IsSkillName(selectorVM.SkillName.Value) &&
                    (selectorVM.SkillLevel.Value != 0 || selectorVM.IsFix))
                {
                    condition.AddSkill(new Skill(selectorVM.SkillName.Value, selectorVM.SkillLevel.Value, false, selectorVM.IsFix));
                }
            }

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
                if (string.IsNullOrEmpty(vm.SkillName.Value))
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
                if (string.IsNullOrEmpty(vm.SkillName.Value))
                {
                    vm.SkillName.Value = name;
                    return;
                }
            }

            // 同名スキルも空欄もない場合、何もせずに終了
            return;
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

        internal void ShowSearchResult(List<EquipSet> result, bool remain, int limit)
        {
            SearchResult.Value = BindableEquipSet.BeBindableList(result);
            IsRemaining.Value = remain;
            Limit = limit;
        }
    }
}
