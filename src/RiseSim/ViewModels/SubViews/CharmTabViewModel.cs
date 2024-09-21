using Reactive.Bindings;
using RiseSim.Config;
using RiseSim.Util;
using RiseSim.ViewModels.Controls;
using SimModel.Config;
using SimModel.Model;
using SimModel.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

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
        /// 護石画面の一覧用部品のVM
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<CharmRowViewModel>> CharmRowVMs { get; } = new();

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

            // 除外・固定設定があったら削除
            Simulator.DeleteClude(trueName);

            // この護石を使っているマイセットがあったら削除
            DeleteMySetUsingCharm(trueName);

            // 護石削除
            Simulator.DeleteCharm(trueName);

            // マスタをリロード
            CludeTabVM.LoadCludes();
            MySetTabVM.LoadMySets();
            MainVM.LoadEquips();

            // ログ表示
            SetStatusBar("護石削除完了：" + dispName);
        }

        /// <summary>
        /// 指定した護石を使っているマイセットがあったら削除
        /// </summary>
        /// <param name="name">護石名</param>
        private void DeleteMySetUsingCharm(string name)
        {
            List<EquipSet> delMySets = new();
            foreach (var set in Masters.MySets)
            {
                if (set.Charm.Name != null && set.Charm.Name.Equals(name))
                {
                    delMySets.Add(set);
                }
            }
            foreach (var set in delMySets)
            {
                Simulator.DeleteMySet(set);
            }
        }

        /// <summary>
        /// 装備関係のマスタ情報をVMにロード
        /// </summary>
        internal void LoadEquipsForCharm()
        {

            // 護石画面用のVMの設定
            ObservableCollection<CharmRowViewModel> charmList = new();
            foreach (var charm in Masters.Charms)
            {
                charmList.Add(new CharmRowViewModel(charm));
            }
            CharmRowVMs.ChangeCollection(charmList);
        }
    }
}
