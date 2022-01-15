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
using RiseSim.ViewModels.SubViews;
using SimModel.Model;
using SimModel.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiseSim.ViewModels
{
    internal class MainViewModel : BindableBase
    {
        // MainViewインスタンス：子VMからの参照用
        static public MainViewModel Instance { get; set; }

        // シミュ本体
        public Simulator Simulator { get; set; }

        // シミュ画面のVM
        public ReactivePropertySlim<SimulatorTabViewModel> SimulatorTabVM { get; } = new();

        // 除外・固定画面のVM
        public ReactivePropertySlim<CludeTabViewModel> CludeTabVM { get; } = new();

        // 護石画面のVM
        public ReactivePropertySlim<CharmTabViewModel> CharmTabVM { get; } = new();

        // マイセット画面のVM
        public ReactivePropertySlim<MySetTabViewModel> MySetTabVM { get; } = new();

        // ライセンス画面のVM
        public ReactivePropertySlim<LicenseTabViewModel> LicenseTabVM { get; } = new();

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
            SimulatorTabVM.Value = new SimulatorTabViewModel();
            CludeTabVM.Value = new CludeTabViewModel();
            CharmTabVM.Value = new CharmTabViewModel();
            MySetTabVM.Value = new MySetTabViewModel();
            LicenseTabVM.Value = new LicenseTabViewModel();

            // マスタファイル読み込み
            LoadMasters();

            // ログ表示
            StatusBarText.Value = "モンハンライズスキルシミュレータ for Windows";
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
    }
}
