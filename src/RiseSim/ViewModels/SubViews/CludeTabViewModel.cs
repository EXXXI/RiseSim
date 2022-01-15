/*    RiseSim : MHRise skill simurator for Windows
 *    Copyright (C) 2022  EXXXI
 *
 *    This program is free software: you can redistribute it and/or modify
 *    it under the terms of the GNU General Public License as published by
 *    the Free Software Foundation, either version 3 of the License, or
 *    (at your option) any later version.
 *
 *    This program is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 *    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *    GNU General Public License for more details.
 *
 *    You should have received a copy of the GNU General Public License
 *    along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
using Prism.Mvvm;
using Reactive.Bindings;
using RiseSim.ViewModels.Controls;
using SimModel.Model;
using SimModel.Service;
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


        // コンストラクタ
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
            equipList.Add(new EquipSelectRowViewModel(EquipKind.head.StrWithColon(), Masters.Heads));
            equipList.Add(new EquipSelectRowViewModel(EquipKind.body.StrWithColon(), Masters.Bodys));
            equipList.Add(new EquipSelectRowViewModel(EquipKind.arm.StrWithColon(), Masters.Arms));
            equipList.Add(new EquipSelectRowViewModel(EquipKind.waist.StrWithColon(), Masters.Waists));
            equipList.Add(new EquipSelectRowViewModel(EquipKind.leg.StrWithColon(), Masters.Legs));
            equipList.Add(new EquipSelectRowViewModel(EquipKind.charm.StrWithColon(), Masters.Charms));
            equipList.Add(new EquipSelectRowViewModel(EquipKind.deco.StrWithColon(), Masters.Decos));
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
