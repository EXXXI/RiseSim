using Prism.Mvvm;
using Reactive.Bindings;
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
    class CludeTabViewModel : BindableBase
    {
        // MainViewModelから参照を取得
        private Simulator Simulator { get; }
        private ReactivePropertySlim<string> StatusBarText { get; }


        // 除外・固定画面の登録部品のVM
        public ReactivePropertySlim<ObservableCollection<EquipSelectRowViewModel>> EquipSelectRowVMs { get; } = new();

        // 除外・固定画面の一覧表示の各行のVM
        public ReactivePropertySlim<ObservableCollection<CludeRowViewModel>> CludeRowVMs { get; } = new();


        public CludeTabViewModel()
        {
            // MainViewModelから参照を取得
            Simulator = MainViewModel.Instance.Simulator;
            StatusBarText = MainViewModel.Instance.StatusBarText;
        }

        // 装備のマスタ情報をVMにロード
        internal void LoadEquipsForClude()
        {
            // 除外固定画面用のVMの設定
            ObservableCollection<EquipSelectRowViewModel> equipList = new();
            equipList.Add(new EquipSelectRowViewModel("頭：", Masters.Heads));
            equipList.Add(new EquipSelectRowViewModel("胴：", Masters.Bodys));
            equipList.Add(new EquipSelectRowViewModel("腕：", Masters.Arms));
            equipList.Add(new EquipSelectRowViewModel("腰：", Masters.Waists));
            equipList.Add(new EquipSelectRowViewModel("足：", Masters.Legs));
            equipList.Add(new EquipSelectRowViewModel("護石：", Masters.Charms));
            equipList.Add(new EquipSelectRowViewModel("装飾品：", Masters.Decos));
            EquipSelectRowVMs.Value = equipList;
        }

        // 除外固定のマスタ情報をVMにロード
        internal void LoadCludes()
        {
            // 除外固定画面用のVMの設定
            ObservableCollection<CludeRowViewModel> cludeList = new();
            foreach (var clude in Masters.Cludes)
            {
                cludeList.Add(new CludeRowViewModel(clude));
            }
            CludeRowVMs.Value = cludeList;
        }

        // 除外装備設定
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
            StatusBarText.Value = "除外：" + dispName;
        }

        // 固定装備設定
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
            StatusBarText.Value = "固定：" + dispName;
        }

        // 除外・固定の解除
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
            StatusBarText.Value = "解除：" + dispName;
        }
    }
}
