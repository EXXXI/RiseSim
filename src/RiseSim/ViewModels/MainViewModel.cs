using Prism.Mvvm;
using Reactive.Bindings;
using RiseSim.ViewModels.SubViews;
using SimModel.Model;
using SimModel.Service;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace RiseSim.ViewModels
{
    internal class MainViewModel : BindableBase
    {
        // 結果画面のタブIndex
        const int SimulatorTabIndex = 1;

        // MainViewインスタンス：子VMからの参照用
        static public MainViewModel Instance { get; set; }

        // シミュ本体
        public Simulator Simulator { get; set; }

        // 選択しているタブのIndex
        public ReactivePropertySlim<int> SelectedTabIndex { get; set; } = new();

        // シミュ画面のVM
        public ReactivePropertySlim<SkillSelectTabViewModel> SkillSelectTabVM { get; } = new();

        // シミュ画面のVM
        public ReactivePropertySlim<SimulatorTabViewModel> SimulatorTabVM { get; } = new();

        // 除外・固定画面のVM
        public ReactivePropertySlim<CludeTabViewModel> CludeTabVM { get; } = new();

        // 護石画面のVM
        public ReactivePropertySlim<CharmTabViewModel> CharmTabVM { get; } = new();

        // 傀異錬成画面のVM
        public ReactivePropertySlim<AugmentationTabViewModel> AugmentationTabVM { get; } = new();

        // 理想錬成画面のVM
        public ReactivePropertySlim<IdealAugmentationTabViewModel> IdealAugmentationTabVM { get; } = new();

        // マイセット画面のVM
        public ReactivePropertySlim<MySetTabViewModel> MySetTabVM { get; } = new();

        // ライセンス画面のVM
        public ReactivePropertySlim<LicenseTabViewModel> LicenseTabVM { get; } = new();
 
        // マイ検索条件画面のVM
        public ReactivePropertySlim<MyConditionTabViewModel> MyConditionTabVM { get; } = new();

        // ビジー判定
        public ReactivePropertySlim<bool> IsBusy { get; } = new(false);
        public ReadOnlyReactivePropertySlim<bool> IsFree { get; private set; }

        // ステータスバー表示
        public ReactivePropertySlim<string> StatusBarText { get; } = new();


        // コンストラクタ：起動時処理
        public MainViewModel()
        {
            // 子VMからの参照用にstaticにインスタンスを登録
            Instance = this;

            // シミュ本体のインスタンス化
            Simulator = new Simulator();
            Simulator.LoadData();

            // ビジー状態のプロパティを紐づけ
            IsFree = IsBusy.Select(x => !x).ToReadOnlyReactivePropertySlim();

            // 各タブのVMを設定
            SkillSelectTabVM.Value = new SkillSelectTabViewModel();
            SimulatorTabVM.Value = new SimulatorTabViewModel();
            CludeTabVM.Value = new CludeTabViewModel();
            CharmTabVM.Value = new CharmTabViewModel();
            AugmentationTabVM.Value = new AugmentationTabViewModel();
            IdealAugmentationTabVM.Value = new IdealAugmentationTabViewModel();
            MySetTabVM.Value = new MySetTabViewModel();
            LicenseTabVM.Value = new LicenseTabViewModel();
            MyConditionTabVM.Value = new MyConditionTabViewModel();

            // マスタファイル読み込み
            LoadMasters();

            // ログ表示
            StatusBarText.Value = "モンハンライズスキルシミュレータ for Windows";
        }

        // 理想錬成更新 処理本体は理想錬成VM
        internal void SaveIdeal()
        {
            IdealAugmentationTabVM.Value.SaveIdeal();
        }

        // 除外装備設定　処理本体は除外・固定画面VM
        internal void AddExclude(string trueName, string dispName)
        {
            CludeTabVM.Value.AddExclude(trueName, dispName);
        }

        // 固定装備設定　処理本体は除外・固定画面VM
        internal void AddInclude(string trueName, string dispName)
        {
            CludeTabVM.Value.AddInclude(trueName, dispName);
        }

        // 除外・固定の解除　処理本体は除外・固定画面VM
        internal void DeleteClude(string trueName, string dispName)
        {
            CludeTabVM.Value.DeleteClude(trueName, dispName);
        }

        // 護石削除　処理本体は護石画面VM
        internal void DeleteCharm(string trueName, string dispName)
        {
            CharmTabVM.Value.DeleteCharm(trueName, dispName);
        }

        // マイセットのスキルをシミュ画面の検索条件に反映　処理本体はシミュ画面VM
        internal void InputMySetCondition(EquipSet? set)
        {
            SimulatorTabVM.Value.InputMySetCondition(set);
        }

        // マイ検索条件をシミュ画面の検索条件に反映　処理本体はシミュ画面VM
        internal void InputMyCondition(SearchCondition condition)
        {
            SimulatorTabVM.Value.InputMyCondition(condition);
        }

        // シミュ画面の検索条件をマイ検索条件に反映　処理本体はマイ検索条件画面VM

        internal void AddMyCondition(SearchCondition searchCondition)
        {
            MyConditionTabVM.Value.AddSearchCondition(searchCondition);
        }

        // マスタ情報を全てVMにロード
        internal void LoadMasters()
        {
            LoadEquips();
            LoadCludes();
            LoadMySets();
        }

        // 装備関係のマスタ情報をVMにロード
        internal void LoadEquips()
        {
            // 錬成情報を反映
            Simulator.RefreshEquipmentMasters();

            // 錬成画面用のVM設定
            AugmentationTabVM.Value.LoadAugmentations();

            // 錬成画面用のVM設定
            IdealAugmentationTabVM.Value.LoadIdealAugmentations();

            // 除外固定画面用のVMの設定
            CludeTabVM.Value.LoadEquipsForClude();

            // 護石画面用のVMの設定
            CharmTabVM.Value.LoadEquipsForCharm();
        }

        // 除外固定のマスタ情報をVMにロード
        internal void LoadCludes()
        {
            // 除外固定画面用のVMの設定
            CludeTabVM.Value.LoadCludes();
        }

        // マイセットのマスタ情報をVMにロード
        internal void LoadMySets()
        {
            // マイセット画面用のVMの設定
            MySetTabVM.Value.LoadMySets();
        }

        internal void ShowSearchResult(List<EquipSet> result)
        {
            SimulatorTabVM.Value.ShowSearchResult(result);
            SelectedTabIndex.Value = SimulatorTabIndex;
        }

        internal void AddSkill(string name, int level)
        {
            SkillSelectTabVM.Value.AddSkill(name, level);
        }
    }
}
