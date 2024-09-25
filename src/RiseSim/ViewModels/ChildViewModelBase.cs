using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Disposables;
using RiseSim.ViewModels.SubViews;
using SimModel.Service;
using System;

namespace RiseSim.ViewModels
{
    /// <summary>
    /// MainViewModel以外のViewModelの基底クラス
    /// VM同士の接続の補助と、一部MainViewModelメンバの参照を共有する
    /// </summary>
    internal class ChildViewModelBase : BindableBase, IDisposable
    {
        /// <summary>
        /// MainViewModel
        /// </summary>
        protected MainViewModel MainVM { get => MainViewModel.Instance; }

        /// <summary>
        /// スキル選択画面のVM
        /// </summary>
        protected SkillSelectTabViewModel SkillSelectTabVM { get => MainVM.SkillSelectTabVM.Value; }

        /// <summary>
        /// 検索結果画面のVM
        /// </summary>
        protected SimulatorTabViewModel SimulatorTabVM { get => MainVM.SimulatorTabVM.Value; }

        /// <summary>
        /// 除外・固定画面のVM
        /// </summary>
        protected CludeTabViewModel CludeTabVM { get => MainVM.CludeTabVM.Value; }

        /// <summary>
        /// 護石画面のVM
        /// </summary>
        protected CharmTabViewModel CharmTabVM { get => MainVM.CharmTabVM.Value; }

        /// <summary>
        /// 傀異錬成画面のVM
        /// </summary>
        protected AugmentationTabViewModel AugmentationTabVM { get => MainVM.AugmentationTabVM.Value; }

        /// <summary>
        /// 理想錬成画面のVM
        /// </summary>
        protected IdealAugmentationTabViewModel IdealAugmentationTabVM { get => MainVM.IdealAugmentationTabVM.Value; }

        /// <summary>
        /// マイセット画面のVM
        /// </summary>
        protected MySetTabViewModel MySetTabVM { get => MainVM.MySetTabVM.Value; }

        /// <summary>
        /// ライセンス画面のVM
        /// </summary>
        protected LicenseTabViewModel LicenseTabVM { get => MainVM.LicenseTabVM.Value; }

        /// <summary>
        /// シミュ本体
        /// MainViewModelから参照を取得
        /// </summary>
        protected Simulator Simulator { get => MainVM.Simulator; }

        /// <summary>
        /// ビジーフラグ
        /// MainViewModelから参照を取得
        /// </summary>
        protected ReactivePropertySlim<bool> IsBusy { get => MainVM.IsBusy; }

        /// <summary>
        /// ステータスバーに指定のテキストを表示
        /// </summary>
        /// <param name="text">表示対象</param>
        protected void SetStatusBar(string text)
        {
            MainVM.StatusBarText.Value = text;
        }

        #region Dispose関連

        protected CompositeDisposable Disposable { get; } = new CompositeDisposable();

        /// <summary>
        /// disposeフラグ
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing">disposeフラグ</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Disposable.Dispose();
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// ファイナライザ
        /// </summary>
        ~ChildViewModelBase()
        {
            Dispose(false);
        }

        #endregion
    }
}
