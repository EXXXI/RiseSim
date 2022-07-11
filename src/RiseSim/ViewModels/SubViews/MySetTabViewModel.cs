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

namespace RiseSim.ViewModels.SubViews
{
    class MySetTabViewModel : BindableBase
    {
        // MainViewModelから参照を取得
        private Simulator Simulator { get; }
        private ReactivePropertySlim<string> StatusBarText { get; }


        // マイセット一覧
        public ReactivePropertySlim<ObservableCollection<BindableEquipSet>> MySetList { get; } = new();

        // マイセットの選択行データ
        public ReactivePropertySlim<BindableEquipSet> MyDetailSet { get; } = new();

        // マイセット画面の装備詳細の各行のVM
        public ReactivePropertySlim<ObservableCollection<EquipRowViewModel>> MyEquipRowVMs { get; } = new();

        // マイセット名前入力欄
        public ReactivePropertySlim<string> MyDetailName { get; } = new();


        // マイセット削除コマンド
        public ReactiveCommand DeleteMySetCommand { get; } = new ReactiveCommand();

        // マイセットの内容を検索条件に指定するコマンド
        public ReactiveCommand InputMySetConditionCommand { get; } = new ReactiveCommand();

        // マイセットの名前変更を保存するコマンド
        public ReactiveCommand ChangeNameCommand { get; } = new ReactiveCommand();

        // コマンドを設定
        private void SetCommand()
        {
            DeleteMySetCommand.Subscribe(_ => DeleteMySet());
            InputMySetConditionCommand.Subscribe(_ => InputMySetCondition());
            ChangeNameCommand.Subscribe(_ => ChangeName());
        }

        // マイセットの名前変更
        private void ChangeName()
        {
            if (MyDetailSet.Value == null)
            {
                return;
            }

            // TODO: これだけだと不安だからID欲しくない？
            // 選択状態復帰用
            string description = MyDetailSet.Value.Description;
            string name = MyDetailName.Value;

            // 変更
            MyDetailSet.Value.Original.Name = name;
            Simulator.SaveMySet();

            // マイセットマスタのリロード
            MainViewModel.Instance.LoadMySets();

            // 選択状態が解除されてしまうのでDetailSetを再選択
            foreach (var mySet in MySetList.Value)
            {
                if (mySet.Name == name && mySet.Description == description)
                {
                    MyDetailSet.Value = mySet;
                }
            }

            // ログ表示
            StatusBarText.Value = "マイセット名前変更：" + MyDetailName.Value;
        }

        public MySetTabViewModel()
        {
            // MainViewModelから参照を取得
            Simulator = MainViewModel.Instance.Simulator;
            StatusBarText = MainViewModel.Instance.StatusBarText;

            // マイセット画面の一覧と装備詳細を紐づけ
            MyDetailSet.Subscribe(set => {
                MyEquipRowVMs.Value = EquipRowViewModel.SetToEquipRows(set);
                MyDetailName.Value = MyDetailSet.Value?.Name;
            });

            // コマンドを設定
            SetCommand();
        }

        // マイセットを削除
        internal void DeleteMySet()
        {
            EquipSet? set = MyDetailSet.Value?.Original;
            if (set == null)
            {
                // 詳細画面が空の状態で実行したなら何もせず終了
                return;
            }

            // 削除
            Simulator.DeleteMySet(set);

            // マイセットマスタのリロード
            MainViewModel.Instance.LoadMySets();

            // ログ表示
            StatusBarText.Value = "マイセット解除：" + set.SimpleSetName;
        }

        // マイセットのスキルをシミュ画面の検索条件に反映
        internal void InputMySetCondition()
        {
            MainViewModel.Instance.InputMySetCondition(MyDetailSet.Value?.Original);
        }

        // マイセットのマスタ情報をVMにロード
        internal void LoadMySets()
        {
            // マイセット画面用のVMの設定
            MySetList.Value = BindableEquipSet.BeBindableList(Masters.MySets);
        }
    }
}
