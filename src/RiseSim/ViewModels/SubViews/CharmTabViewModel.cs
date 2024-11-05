using Reactive.Bindings;
using RiseSim.Config;
using RiseSim.Util;
using RiseSim.ViewModels.BindableWrapper;
using RiseSim.ViewModels.Controls;
using SimModel.Config;
using SimModel.Model;
using SimModel.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace RiseSim.ViewModels.SubViews
{
    /// <summary>
    /// 護石タブのVM
    /// </summary>
    class CharmTabViewModel : ChildViewModelBase
    {
        /// <summary>
        /// 護石のスキル個数
        /// </summary>
        private int MaxCharmSkillCount { get; } = LogicConfig.Instance.MaxCharmSkillCount;

        /// <summary>
        /// スロットの最大の大きさ
        /// </summary>
        private int MaxSlotSize { get; } = ViewConfig.Instance.MaxSlotSize;

        /// <summary>
        /// 護石画面のスキル選択部品のVM
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<SkillSelectorViewModel>> CharmSkillSelectorVMs { get; } = new();

        /// <summary>
        /// 表示用護石一覧
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<BindableCharm>> Charms { get; } = new();

        /// <summary>
        /// 護石画面のスロット指定
        /// </summary>
        public ReactivePropertySlim<string> CharmWeaponSlots { get; } = new();

        /// <summary>
        /// スロット選択の選択肢
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<string>> SlotMaster { get; } = new();

        /// <summary>
        /// 護石追加コマンド
        /// </summary>
        public ReactiveCommand AddCharmCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// ドラッグコマンド
        /// </summary>
        public ReactiveCommand RowChangedCommand { get; private set; } = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CharmTabViewModel()
        {
            // 護石画面のスキル選択部品準備
            ObservableCollection<SkillSelectorViewModel> charmSelectorVMs = new();
            for (int i = 0; i < MaxCharmSkillCount; i++)
            {
                charmSelectorVMs.Add(new SkillSelectorViewModel());
            }
            CharmSkillSelectorVMs.ChangeCollection(charmSelectorVMs);

            // スロットの選択肢を生成し、画面に反映
            ObservableCollection<string> slots = new();
            for (int i = 0; i <= MaxSlotSize; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    for (int k = 0; k <= j; k++)
                    {
                        slots.Add(string.Join('-', i, j, k));
                    }
                }
            }
            SlotMaster.Value = slots;
            CharmWeaponSlots.Value = "0-0-0";

            // コマンドを設定
            AddCharmCommand.Subscribe(_ => AddCharm());
            RowChangedCommand.Subscribe(indexpair => RowChanged(indexpair as (int, int)?));
        }

        /// <summary>
        /// 護石追加
        /// </summary>
        internal void AddCharm()
        {
            // スキルを整理
            List<Skill> skills = new();
            foreach (var vm in CharmSkillSelectorVMs.Value)
            {
                if (Masters.IsSkillName(vm.SkillName.Value))
                {
                    skills.Add(new Skill(vm.SkillName.Value, vm.SkillLevel.Value));
                }
            }

            // スロットを整理
            string[] splited = CharmWeaponSlots.Value.Split('-');
            int weaponSlot1 = int.Parse(splited[0]);
            int weaponSlot2 = int.Parse(splited[1]);
            int weaponSlot3 = int.Parse(splited[2]);

            // 護石追加
            Equipment added = Simulator.AddCharm(skills, weaponSlot1, weaponSlot2, weaponSlot3);

            // 装備マスタをリロード
            MainVM.LoadEquips();

            // ログ表示
            SetStatusBar("護石追加完了：" + added.DispName);
        }

        /// <summary>
        /// 順番入れ替え
        /// </summary>
        /// <param name="indexpair">(int dropIndex, int targetIndex)</param>
        private void RowChanged((int dropIndex, int targetIndex)? indexpair)
        {
            if (indexpair != null)
            {
                Charms.Value.Move(indexpair.Value.dropIndex, indexpair.Value.targetIndex);
                Simulator.MoveCharm(indexpair.Value.dropIndex, indexpair.Value.targetIndex);
            }
        }

        /// <summary>
        /// 護石削除
        /// </summary>
        /// <param name="trueName">物理名</param>
        /// <param name="dispName">表示名</param>
        internal void DeleteCharm(string trueName, string dispName)
        {
            if (string.IsNullOrEmpty(trueName))
            {
                // 装備無しなら何もせず終了
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                $"護石「{dispName}」を削除します。\nよろしいですか？",
                "護石削除",
                MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            // この護石を使っているマイセットがあったら再度確認する
            if (Masters.MySets.Where(set => trueName.Equals(set.Charm.Name)).Any())
            {
                MessageBoxResult setConfirm = MessageBox.Show(
                    $"護石「{dispName}」を利用しているマイセットが存在します。\n本当に削除してよろしいですか？\n(該当のマイセットも同時に削除されます。)",
                    "護石削除",
                    MessageBoxButton.YesNo);
                if (setConfirm != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            // 護石削除
            Simulator.DeleteCharm(trueName);

            // マスタをリロード
            // マイセットが変更になる可能性があるためそちらもリロード
            MySetTabVM.LoadMySets();
            MainVM.LoadEquips();

            // ログ表示
            SetStatusBar("護石削除完了：" + dispName);
        }

        /// <summary>
        /// 装備関係のマスタ情報をVMにロード
        /// </summary>
        internal void LoadEquipsForCharm()
        {
            // 護石画面用のVMの設定
            ObservableCollection<BindableCharm> charmList = BindableCharm.BeBindableList(Masters.Charms);
            Charms.ChangeCollection(charmList);
        }
    }
}
