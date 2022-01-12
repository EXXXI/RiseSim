using Prism.Mvvm;
using Reactive.Bindings;
using RiseSim.ViewModels.BindableWrapper;
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


        // コマンド
        public ReactiveCommand DeleteMySetCommand { get; } = new ReactiveCommand();
        public ReactiveCommand InputMySetConditionCommand { get; } = new ReactiveCommand();

        // コマンドを設定
        private void SetCommand()
        {
            DeleteMySetCommand.Subscribe(_ => DeleteMySet());
            InputMySetConditionCommand.Subscribe(_ => InputMySetCondition());
        }


        public MySetTabViewModel()
        {
            // MainViewModelから参照を取得
            Simulator = MainViewModel.Instance.Simulator;
            StatusBarText = MainViewModel.Instance.StatusBarText;

            // マイセット画面の一覧と装備詳細を紐づけ
            MyDetailSet.Subscribe(set => MyEquipRowVMs.Value = EquipRowViewModel.SetToEquipRows(set));

            // コマンドを設定
            SetCommand();
        }

        // マイセットを削除
        internal void DeleteMySet()
        {
            EquipSet set = MyDetailSet.Value.Original;
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
            MainViewModel.Instance.InputMySetCondition(MyDetailSet.Value.Original);
        }

        // マイセットのマスタ情報をVMにロード
        internal void LoadMySets()
        {
            // マイセット画面用のVMの設定
            MySetList.Value = BindableEquipSet.BeBindableList(Masters.MySets);
        }
    }
}
