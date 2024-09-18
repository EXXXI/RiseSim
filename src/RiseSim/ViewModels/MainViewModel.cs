using Prism.Mvvm;
using Reactive.Bindings;
using RiseSim.ViewModels.SubViews;
using SimModel.Model;
using SimModel.Service;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace RiseSim.ViewModels
{
    /// <summary>
    /// MainViewのViewModel
    /// 各タブのVMの設定や、VMを跨ぐ処理の仲介を行う
    /// </summary>
    internal class MainViewModel : BindableBase
    {
        // TODO: 名称指定か何かにしたい

        /// <summary>
        /// スキル選択画面のタブIndex
        /// </summary>
        private const int SkillSelectorTabIndex = 0;

        /// <summary>
        /// 検索結果画面のタブIndex
        /// </summary>
        private const int SimulatorTabIndex = 1;

        /// <summary>
        /// MainViewインスタンス：子VMからの参照用
        /// </summary>
        static internal MainViewModel Instance { get; set; }

        /// <summary>
        /// ビジー判定
        /// </summary>
        public ReactivePropertySlim<bool> IsBusy { get; } = new(false);

        /// <summary>
        /// ビジー判定の反転
        /// </summary>
        public ReadOnlyReactivePropertySlim<bool> IsFree { get; private set; }

        /// <summary>
        /// ステータスバー表示
        /// </summary>
        public ReactivePropertySlim<string> StatusBarText { get; } = new();

        /// <summary>
        /// シミュ本体
        /// </summary>
        internal Simulator Simulator { get; set; }

        /// <summary>
        /// スキル選択画面のVM
        /// </summary>
        public ReactivePropertySlim<SkillSelectTabViewModel> SkillSelectTabVM { get; } = new();

        /// <summary>
        /// 検索結果画面のVM
        /// </summary>
        public ReactivePropertySlim<SimulatorTabViewModel> SimulatorTabVM { get; } = new();

        /// <summary>
        /// 除外・固定画面のVM
        /// </summary>
        public ReactivePropertySlim<CludeTabViewModel> CludeTabVM { get; } = new();

        /// <summary>
        /// 護石画面のVM
        /// </summary>
        public ReactivePropertySlim<CharmTabViewModel> CharmTabVM { get; } = new();

        /// <summary>
        /// 傀異錬成画面のVM
        /// </summary>
        public ReactivePropertySlim<AugmentationTabViewModel> AugmentationTabVM { get; } = new();

        /// <summary>
        /// 理想錬成画面のVM
        /// </summary>
        public ReactivePropertySlim<IdealAugmentationTabViewModel> IdealAugmentationTabVM { get; } = new();

        /// <summary>
        /// マイセット画面のVM
        /// </summary>
        public ReactivePropertySlim<MySetTabViewModel> MySetTabVM { get; } = new();

        /// <summary>
        /// ライセンス画面のVM
        /// </summary>
        public ReactivePropertySlim<LicenseTabViewModel> LicenseTabVM { get; } = new();

        /// <summary>
        /// 選択しているタブのIndex
        /// </summary>
        public ReactivePropertySlim<int> SelectedTabIndex { get; set; } = new();


        /// <summary>
        /// コンストラクタ：起動時処理
        /// </summary>
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

            // マスタファイル読み込み
            LoadMasters();

            // ログ表示
            StatusBarText.Value = "モンハンライズスキルシミュレータ for Windows";
        }


        /// <summary>
        /// スキル選択画面へスキルを反映　処理本体はスキル選択画面
        /// </summary>
        /// <param name="name">スキル名</param>
        /// <param name="level">レベル</param>
        internal void AddSkill(string name, int level)
        {
            SkillSelectTabVM.Value.AddSkill(name, level);
        }

        /// <summary>
        /// マイ検索条件の再読み込み　処理本体はスキル選択画面
        /// </summary>
        internal void LoadMyCondition()
        {
            SkillSelectTabVM.Value.LoadMyCondition();
        }

        /// <summary>
        /// マイ検索条件のスキル選択への適用　処理本体はスキル選択画面
        /// </summary>
        /// <param name="condition"></param>
        internal void ApplyMyCondition(SearchCondition condition)
        {
            SkillSelectTabVM.Value.ApplyMyCondition(condition);
        }

        /// <summary>
        /// 検索結果画面を表示
        /// </summary>
        internal void ShowSimulatorTab()
        {
            SelectedTabIndex.Value = SimulatorTabIndex;
        }

        /// <summary>
        /// 除外・固定の解除　処理本体は除外・固定画面VM
        /// </summary>
        /// <param name="trueName">物理名</param>
        /// <param name="dispName">表示名</param>
        internal void DeleteClude(string trueName, string dispName)
        {
            CludeTabVM.Value.DeleteClude(trueName, dispName);
        }

        /// <summary>
        /// 護石削除　処理本体は護石画面VM
        /// </summary>
        /// <param name="trueName">物理名</param>
        /// <param name="dispName">表示名</param>
        internal void DeleteCharm(string trueName, string dispName)
        {
            CharmTabVM.Value.DeleteCharm(trueName, dispName);
        }

        /// <summary>
        /// スキル選択画面を表示
        /// </summary>
        internal void ShowSkillSelectorTab()
        {
            SelectedTabIndex.Value = SkillSelectorTabIndex;
        }

        /// <summary>
        /// 理想錬成更新 処理本体は理想錬成VM
        /// </summary>
        internal void SaveIdeal()
        {
            IdealAugmentationTabVM.Value.SaveIdeal();
        }


        /// <summary>
        /// マスタ情報を全てVMにロード
        /// </summary>
        internal void LoadMasters()
        {
            LoadEquips();
            CludeTabVM.Value.LoadCludes();
            MySetTabVM.Value.LoadMySets();
        }

        /// <summary>
        /// 装備関係のマスタ情報をVMにロード
        /// </summary>
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
    }
}
