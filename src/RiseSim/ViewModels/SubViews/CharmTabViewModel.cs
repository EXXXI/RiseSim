using Prism.Mvvm;
using Reactive.Bindings;
using RiseSim.Config;
using RiseSim.ViewModels.Controls;
using SimModel.Config;
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
    class CharmTabViewModel : BindableBase
    {
        // MainViewModelから参照を取得
        private Simulator Simulator { get; }
        private ReactivePropertySlim<string> StatusBarText { get; }


        // 護石のスキル個数
        private int MaxCharmSkillCount { get; } = LogicConfig.Instance.MaxCharmSkillCount;

        // スロットの最大の大きさ
        private int MaxSlotSize { get; } = ViewConfig.Instance.MaxSlotSize;

        // スキル未選択時の表示
        private string NoSkillName { get; } = ViewConfig.Instance.NoSkillName;


        // 護石画面のスキル選択部品のVM
        public ReactivePropertySlim<ObservableCollection<SkillSelectorViewModel>> CharmSkillSelectorVMs { get; } = new();

        // 護石画面の一覧用部品のVM
        public ReactivePropertySlim<ObservableCollection<CharmRowViewModel>> CharmRowVMs { get; } = new();

        // 護石画面のスロット指定
        public ReactivePropertySlim<string> CharmWeaponSlots { get; } = new();

        // スロット選択の選択肢
        public ReactivePropertySlim<ObservableCollection<string>> SlotMaster { get; } = new();


        // 護石追加コマンド
        public ReactiveCommand AddCharmCommand { get; } = new ReactiveCommand();

        // コマンドを設定
        private void SetCommand()
        {
            AddCharmCommand.Subscribe(_ => AddCharm());
        }

        public CharmTabViewModel()
        {
            // MainViewModelから参照を取得
            Simulator = MainViewModel.Instance.Simulator;
            StatusBarText = MainViewModel.Instance.StatusBarText;

            // 護石画面のスキル選択部品準備
            ObservableCollection<SkillSelectorViewModel> charmSelectorVMs = new();
            for (int i = 0; i < MaxCharmSkillCount; i++)
            {
                charmSelectorVMs.Add(new SkillSelectorViewModel());
            }
            CharmSkillSelectorVMs.Value = charmSelectorVMs;

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
            SetCommand();
        }

        // 護石追加
        internal void AddCharm()
        {
            // スキルを整理
            List<Skill> skills = new();
            foreach (var vm in CharmSkillSelectorVMs.Value)
            {
                if (!vm.SkillName.Value.Equals(NoSkillName))
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
            MainViewModel.Instance.LoadEquips();

            // ログ表示
            StatusBarText.Value = "護石追加：" + added.DispName;
        }

        // 護石削除
        internal void DeleteCharm(string trueName, string dispName)
        {
            if (string.IsNullOrEmpty(trueName))
            {
                // 装備無しなら何もせず終了
                return;
            }

            // 除外・固定設定があったら削除
            Simulator.DeleteClude(trueName);

            // この護石を使っているマイセットがあったら削除
            DeleteMySetUsingCharm(trueName);

            // 護石削除
            Simulator.DeleteCharm(trueName);

            // マスタをリロード
            MainViewModel.Instance.LoadCludes();
            MainViewModel.Instance.LoadMySets();
            MainViewModel.Instance.LoadEquips();

            // ログ表示
            StatusBarText.Value = "護石削除：" + dispName;
        }

        // 指定した護石を使っているマイセットがあったら削除
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

        // 装備関係のマスタ情報をVMにロード
        internal void LoadEquipsForCharm()
        {

            // 護石画面用のVMの設定
            ObservableCollection<CharmRowViewModel> charmList = new();
            foreach (var charm in Masters.Charms)
            {
                charmList.Add(new CharmRowViewModel(charm));
            }
            CharmRowVMs.Value = charmList;
        }
    }
}
