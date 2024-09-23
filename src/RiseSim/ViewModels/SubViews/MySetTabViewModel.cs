using Reactive.Bindings;
using RiseSim.Util;
using RiseSim.ViewModels.BindableWrapper;
using RiseSim.ViewModels.Controls;
using SimModel.Model;
using SimModel.Service;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace RiseSim.ViewModels.SubViews
{
    /// <summary>
    /// マイセット画面のVM
    /// </summary>
    class MySetTabViewModel : ChildViewModelBase
    {
        /// <summary>
        /// マイセット一覧
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<BindableEquipSet>> MySetList { get; } = new();

        /// <summary>
        /// マイセットの選択行データ
        /// </summary>
        public ReactivePropertySlim<BindableEquipSet> MyDetailSet { get; } = new();

        /// <summary>
        /// マイセット画面の装備詳細の各行のVM
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<EquipRowViewModel>> MyEquipRowVMs { get; } = new();

        /// <summary>
        /// マイセット名前入力欄
        /// </summary>
        public ReactivePropertySlim<string> MyDetailName { get; } = new();

        /// <summary>
        /// マイセット削除コマンド
        /// </summary>
        public ReactiveCommand DeleteMySetCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// マイセットの内容を検索条件に指定するコマンド
        /// </summary>
        public ReactiveCommand InputMySetConditionCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// マイセットの名前変更を保存するコマンド
        /// </summary>
        public ReactiveCommand ChangeNameCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MySetTabViewModel()
        {
            // マイセット画面の一覧と装備詳細を紐づけ
            MyDetailSet.Subscribe(set => {
                if (set != null)
                {
                    MyEquipRowVMs.ChangeCollection(EquipRowViewModel.SetToEquipRows(set.Original));
                    MyDetailName.Value = MyDetailSet.Value?.Name.Value ?? string.Empty;
                }
            });

            // コマンドを設定
            DeleteMySetCommand.Subscribe(_ => DeleteMySet());
            InputMySetConditionCommand.Subscribe(_ => InputMySetCondition());
            ChangeNameCommand.Subscribe(_ => ChangeName());
        }

        /// <summary>
        /// マイセットの名前変更
        /// </summary>
        private void ChangeName()
        {
            if (MyDetailSet.Value == null)
            {
                return;
            }

            // TODO: これだけだと不安だからID欲しくない？
            // 選択状態復帰用
            string description = MyDetailSet.Value.Description.Value;
            string name = MyDetailName.Value;

            // 変更
            MyDetailSet.Value.Original.Name = name;
            Simulator.SaveMySet();

            // マイセットマスタのリロード
            LoadMySets();

            // 選択状態が解除されてしまうのでDetailSetを再選択
            foreach (var mySet in MySetList.Value)
            {
                if (mySet.Name.Value == name && mySet.Description.Value == description)
                {
                    MyDetailSet.Value = mySet;
                }
            }

            // ログ表示
            SetStatusBar("マイセット名前変更完了：" + MyDetailName.Value);
        }

        /// <summary>
        /// マイセットを削除
        /// </summary>
        private void DeleteMySet()
        {
            EquipSet? set = MyDetailSet.Value?.Original;
            if (set == null)
            {
                // 詳細画面が空の状態で実行したなら何もせず終了
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                $"マイセット「{set.Name}」を削除します。\nよろしいですか？",
                "マイセット削除",
                MessageBoxButton.YesNo);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            // 削除
            Simulator.DeleteMySet(set);

            // マイセットマスタのリロード
            LoadMySets();

            // ログ表示
            SetStatusBar("マイセット削除完了：" + set.Name);
        }

        /// <summary>
        /// マイセットのスキルをシミュ画面の検索条件に反映
        /// </summary>
        private void InputMySetCondition()
        {
            EquipSet? set = MyDetailSet.Value?.Original;
            if (set == null)
            {
                // 詳細画面が空の状態で実行したなら何もせず終了
                return;
            }

            SkillSelectTabVM.InputMySetCondition(set);
            MainVM.ShowSkillSelectorTab();

            // ログ表示
            SetStatusBar("マイセット反映完了：" + set.Name);
        }

        /// <summary>
        /// マイセットのマスタ情報をVMにロード
        /// </summary>
        internal void LoadMySets()
        {
            // マイセット画面用のVMの設定
            MySetList.ChangeCollection(BindableEquipSet.BeBindableList(Masters.MySets));
        }
    }
}
