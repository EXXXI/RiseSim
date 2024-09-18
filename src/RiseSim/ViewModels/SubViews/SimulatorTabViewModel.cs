using Prism.Mvvm;
using Reactive.Bindings;
using RiseSim.ViewModels.BindableWrapper;
using RiseSim.ViewModels.Controls;
using SimModel.Model;
using SimModel.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace RiseSim.ViewModels.SubViews
{
    /// <summary>
    /// 検索結果タブのVM
    /// </summary>
    class SimulatorTabViewModel : ChildViewModelBase
    {
        /// <summary>
        /// 検索結果一覧
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<BindableEquipSet>> SearchResult { get; } = new();

        /// <summary>
        /// 検索結果の選択行
        /// </summary>
        public ReactivePropertySlim<BindableEquipSet> DetailSet { get; } = new();

        /// <summary>
        /// 装備詳細の各行のVM
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<EquipRowViewModel>> EquipRowVMs { get; } = new();

        /// <summary>
        /// 頑張り度(前回の検索の値を保存)
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// もっと検索可能フラグ
        /// </summary>
        public ReactivePropertySlim<bool> IsRemaining { get; } = new(false);

        /// <summary>
        /// もっと検索コマンド
        /// </summary>
        public AsyncReactiveCommand SearchMoreCommand { get; private set; }

        /// <summary>
        /// 錬成パターン検索コマンド
        /// </summary>
        public AsyncReactiveCommand SearchPatternCommand { get; private set; }
        
        /// <summary>
        /// 検索キャンセルコマンド
        /// </summary>
        public ReactiveCommand CancelCommand { get; private set; }

        /// <summary>
        /// マイセット追加コマンド
        /// </summary>
        public ReactiveCommand AddMySetCommand { get; } = new ReactiveCommand();

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
        public SimulatorTabViewModel()
        {
            // シミュ画面の検索結果と装備詳細を紐づけ
            DetailSet.Subscribe(set => {
                EquipRowVMs.Value = EquipRowViewModel.SetToEquipRows(set);
            });

            // コマンドを設定
            ReadOnlyReactivePropertySlim<bool> isFree = MainVM.IsFree;
            SearchMoreCommand = isFree.ToAsyncReactiveCommand().WithSubscribe(async () => await SearchMore());
            SearchPatternCommand = isFree.ToAsyncReactiveCommand().WithSubscribe(async () => await SearchPattern());
            CancelCommand = IsBusy.ToReactiveCommand().WithSubscribe(() => Cancel());
            AddMySetCommand.Subscribe(() => AddMySet());
            ExcludeCommand.Subscribe(x => Exclude(x as BindableEquipment));
            IncludeCommand.Subscribe(x => Include(x as BindableEquipment));
        }

        /// <summary>
        /// 検索結果表示
        /// </summary>
        /// <param name="result">検索結果</param>
        /// <param name="remain">続きの有無(もっと検索の可否)</param>
        /// <param name="limit">頑張り度</param>
        internal void ShowSearchResult(List<EquipSet> result, bool remain, int limit)
        {
            SearchResult.Value = BindableEquipSet.BeBindableList(result);
            IsRemaining.Value = remain;
            Limit = limit;
        }

        /// <summary>
        /// もっと検索
        /// </summary>
        /// <returns></returns>
        async private Task SearchMore()
        {
            // ビジーフラグ
            IsBusy.Value = true;

            // もっと検索
            List<EquipSet> result = await Task.Run(() => Simulator.SearchMore(Limit));
            SearchResult.Value = BindableEquipSet.BeBindableList(result);

            // ビジーフラグ解除
            IsBusy.Value = false;

            // もっと検索可否を設定
            IsRemaining.Value = Simulator.IsCanceling || !Simulator.IsSearchedAll;

            // ステータスバー
            SetStatusBar("もっと検索完了");
        }

        /// <summary>
        /// 錬成パターン検索
        /// </summary>
        /// <returns>Task</returns>
        async private Task SearchPattern()
        {
            if (DetailSet.Value == null)
            {
                return;
            }

            // 開始ログ表示
            SetStatusBar("錬成パターン検索中・・・");

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
            SetStatusBar("錬成パターン検索完了");
        }

        /// <summary>
        /// 検索キャンセル
        /// </summary>
        private void Cancel()
        {
            Simulator.Cancel();
        }

        /// <summary>
        /// 装備除外
        /// </summary>
        /// <param name="equip">除外対象</param>
        private void Exclude(BindableEquipment? equip)
        {
            if (equip != null)
            {
                CludeTabVM.AddExclude(equip.Name, equip.DispName);
            }
        }

        /// <summary>
        /// 装備固定
        /// </summary>
        /// <param name="equip">固定対象</param>
        private void Include(BindableEquipment? equip)
        {
            if (equip != null)
            {
                CludeTabVM.AddInclude(equip.Name, equip.DispName);
            }
        }

        /// <summary>
        /// マイセットを追加
        /// </summary>
        private void AddMySet()
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
            MySetTabVM.LoadMySets();

            // ログ表示
            SetStatusBar("マイセット登録：" + set.SimpleSetName);
        }
    }
}
