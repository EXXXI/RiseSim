using Reactive.Bindings;
using RiseSim.Util;
using RiseSim.ViewModels.Controls;
using SimModel.Model;
using SimModel.Service;
using System;
using System.Collections.ObjectModel;

namespace RiseSim.ViewModels.SubViews
{
    /// <summary>
    /// 除外固定設定タブのVM
    /// </summary>
    class CludeTabViewModel : ChildViewModelBase
    {
        /// <summary>
        /// 除外・固定画面の登録部品のVM
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<EquipSelectRowViewModel>> EquipSelectRowVMs { get; } = new();

        /// <summary>
        /// 除外・固定画面の一覧表示の各行のVM
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<CludeRowViewModel>> CludeRowVMs { get; } = new();

        /// <summary>
        /// 除外固定をすべて解除するコマンド
        /// </summary>
        public ReactiveCommand DeleteAllCludeCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// 防具を除外するコマンド
        /// </summary>
        public ReactiveCommand ExcludeAllAugmentationCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// レア9以下防具を除外するコマンド
        /// </summary>
        public ReactiveCommand ExcludeRare9Command { get; } = new ReactiveCommand(); 

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CludeTabViewModel()
        {
            // コマンドを設定
            DeleteAllCludeCommand.Subscribe(_ => DeleteAllClude());
            ExcludeAllAugmentationCommand.Subscribe(_ => ExcludeAllAugmentation());
            ExcludeRare9Command.Subscribe(_ => ExcludeRare9());
        }

        /// <summary>
        /// 除外装備設定
        /// </summary>
        /// <param name="trueName">物理名</param>
        /// <param name="dispName">表示名</param>
        internal void AddExclude(string trueName, string dispName)
        {
            if (string.IsNullOrEmpty(trueName))
            {
                // 装備無しなら何もせず終了
                return;
            }

            // 除外
            Simulator.AddExclude(trueName);

            // 除外固定のマスタをリロード
            LoadCludes();

            // ログ表示
            SetStatusBar("除外：" + dispName);
        }

        /// <summary>
        /// 固定装備設定
        /// </summary>
        /// <param name="trueName">物理名</param>
        /// <param name="dispName">表示名</param>
        internal void AddInclude(string trueName, string dispName)
        {
            if (string.IsNullOrEmpty(trueName))
            {
                // 装備無しなら何もせず終了
                return;
            }

            // 固定
            Simulator.AddInclude(trueName);

            // 除外固定のマスタをリロード
            LoadCludes();

            // ログ表示
            SetStatusBar("固定：" + dispName);
        }

        /// <summary>
        /// 除外・固定の解除
        /// </summary>
        /// <param name="trueName">物理名</param>
        /// <param name="dispName">表示名</param>
        internal void DeleteClude(string trueName, string dispName)
        {
            if (string.IsNullOrEmpty(trueName))
            {
                // 装備無しなら何もせず終了
                return;
            }

            // 解除
            Simulator.DeleteClude(trueName);

            // 除外固定のマスタをリロード
            LoadCludes();

            // ログ表示
            SetStatusBar("解除：" + dispName);
        }

        /// <summary>
        /// 除外・固定の全解除
        /// </summary>
        private void DeleteAllClude()
        {
            // 解除
            Simulator.DeleteAllClude();

            // 除外固定のマスタをリロード
            LoadCludes();

            // ログ表示
            SetStatusBar("固定・除外を全解除");
        }

        /// <summary>
        /// 錬成防具を全て除外
        /// </summary>
        private void ExcludeAllAugmentation()
        {
            // 除外
            Simulator.ExcludeAllAugmentation();

            // 除外固定のマスタをリロード
            LoadCludes();

            // ログ表示
            SetStatusBar("錬成防具を全て除外");
        }

        /// <summary>
        /// レア9以下を全て除外
        /// </summary>
        private void ExcludeRare9()
        {
            // 除外
            Simulator.ExcludeByRare(9);

            // 除外固定のマスタをリロード
            LoadCludes();

            // ログ表示
            SetStatusBar("錬成防具を全て除外");
        }

        /// <summary>
        /// 装備のマスタ情報をVMにロード
        /// </summary>
        internal void LoadEquipsForClude()
        {
            // 除外固定画面用のVMの設定
            ObservableCollection<EquipSelectRowViewModel> equipList = new();
            equipList.Add(new EquipSelectRowViewModel(EquipKind.head.StrWithColon(), Masters.Heads));
            equipList.Add(new EquipSelectRowViewModel(EquipKind.body.StrWithColon(), Masters.Bodys));
            equipList.Add(new EquipSelectRowViewModel(EquipKind.arm.StrWithColon(), Masters.Arms));
            equipList.Add(new EquipSelectRowViewModel(EquipKind.waist.StrWithColon(), Masters.Waists));
            equipList.Add(new EquipSelectRowViewModel(EquipKind.leg.StrWithColon(), Masters.Legs));
            equipList.Add(new EquipSelectRowViewModel(EquipKind.charm.StrWithColon(), Masters.Charms));
            equipList.Add(new EquipSelectRowViewModel(EquipKind.deco.StrWithColon(), Masters.Decos));
            EquipSelectRowVMs.ChangeCollection(equipList);
        }

        /// <summary>
        /// 除外固定のマスタ情報をVMにロード
        /// </summary>
        internal void LoadCludes()
        {
            // TODO: 旧データのDispose

            // 除外固定画面用のVMの設定
            ObservableCollection<CludeRowViewModel> cludeList = new();
            foreach (var clude in Masters.Cludes)
            {
                cludeList.Add(new CludeRowViewModel(clude));
            }
            CludeRowVMs.ChangeCollection(cludeList);
        }
    }
}
