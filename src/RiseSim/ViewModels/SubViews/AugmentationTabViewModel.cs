using Prism.Mvvm;
using Reactive.Bindings;
using RiseSim.Config;
using RiseSim.ViewModels.BindableWrapper;
using RiseSim.ViewModels.Controls;
using SimModel.Config;
using SimModel.Domain;
using SimModel.Model;
using SimModel.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RiseSim.ViewModels.SubViews
{
    internal class AugmentationTabViewModel : BindableBase
    {
        // MainViewModelから参照を取得
        private Simulator Simulator { get; }
        private ReactivePropertySlim<string> StatusBarText { get; }


        // スロットの最大の大きさ
        private int MaxSlotSize { get; } = ViewConfig.Instance.MaxSlotSize;


        // 部位指定の選択肢
        public ReactivePropertySlim<ObservableCollection<string>> KindMaster { get; } = new();

        // 部位指定
        public ReactivePropertySlim<string> Kind { get; } = new();

        // ベース装備一覧
        public ReactivePropertySlim<ObservableCollection<BindableEquipment>> Equips { get; } = new();

        // 選択したベース装備
        public ReactivePropertySlim<BindableEquipment> SelectedEquip { get; } = new();

        // 入力中のベース装備名
        public ReactivePropertySlim<string> InputBaseEquipName { get; } = new();

        // 名前
        public ReactivePropertySlim<string> DispName { get; } = new();

        // 増加防御力
        public ReactivePropertySlim<string> Def { get; } = new();

        // スロット選択の選択肢
        public ReactivePropertySlim<ObservableCollection<string>> SlotMaster { get; } = new();

        // スロット
        public ReactivePropertySlim<string> Slots { get; } = new();

        // 増加火耐性
        public ReactivePropertySlim<string> Fire { get; } = new();

        // 増加水耐性
        public ReactivePropertySlim<string> Water { get; } = new();

        // 増加雷耐性
        public ReactivePropertySlim<string> Thunder { get; } = new();

        // 増加氷耐性
        public ReactivePropertySlim<string> Ice { get; } = new();

        // 増加龍耐性
        public ReactivePropertySlim<string> Dragon { get; } = new();

        // スキル選択部品のVM
        public ReactivePropertySlim<ObservableCollection<SkillSelectorViewModel>> SkillSelectorVMs { get; } = new();

        // 錬成装備一覧
        public ReactivePropertySlim<ObservableCollection<BindableAugmentation>> Augmentations { get; } = new();

        // 一覧の選択行データ
        public ReactivePropertySlim<BindableAugmentation> SelectedAugmentation { get; } = new();


        // 追加コマンド
        public ReactiveCommand AddCommand { get; private set; } = new();

        // 削除コマンド
        public ReactiveCommand DeleteCommand { get; private set; } = new();

        // 反映コマンド
        public ReactiveCommand InputCommand { get; private set; } = new();

        // 上書きコマンド
        public ReactiveCommand UpdateCommand { get; private set; } = new();


        // コマンドを設定
        private void SetCommand()
        {
            Kind.Subscribe(_ => SetEquips());
            SelectedEquip.Subscribe(_ => SetSlots());
            AddCommand.Subscribe(_ => AddAugmentation());
            DeleteCommand.Subscribe(_ => DeleteAugmentation());
            InputCommand.Subscribe(_ => InputAugmentation());
            UpdateCommand.Subscribe(_ => UpdateAugmentation());
        }

        // 上書きコマンド
        private void UpdateAugmentation()
        {
            if (SelectedAugmentation.Value == null)
            {
                return;
            }

            if (SelectedEquip.Value == null)
            {
                foreach (var equip in Equips.Value)
                {
                    if (equip.DispName == InputBaseEquipName.Value)
                    {
                        SelectedEquip.Value = equip;
                        break;
                    }
                }
            }

            if (SelectedEquip.Value == null)
            {
                return;
            }

            // ベースが変わっている場合は新規に回す
            if (Kind.Value.ToEquipKind() != SelectedAugmentation.Value.Kind ||
                SelectedEquip.Value.Name != SelectedAugmentation.Value.BaseName)
            {
                AddAugmentation();
                return;
            }

            Augmentation aug = new();
            aug.Name = SelectedAugmentation.Value.Name;
            string dispName = DispName.Value;
            if (string.IsNullOrWhiteSpace(dispName))
            {
                dispName = Masters.MakeAugmentaionDefaultDispName(SelectedEquip.Value.Name);
            }
            aug.DispName = dispName;
            aug.Kind = Kind.Value.ToEquipKind();
            aug.BaseName = SelectedEquip.Value.Name;
            string[] slots = Slots.Value.Split('-');
            aug.Slot1 = ParseUtil.Parse(slots[0]);
            aug.Slot2 = ParseUtil.Parse(slots[1]);
            aug.Slot3 = ParseUtil.Parse(slots[2]);
            aug.Def = ParseUtil.Parse(Def.Value);
            aug.Fire = ParseUtil.Parse(Fire.Value);
            aug.Water = ParseUtil.Parse(Water.Value);
            aug.Thunder = ParseUtil.Parse(Thunder.Value);
            aug.Ice = ParseUtil.Parse(Ice.Value);
            aug.Dragon = ParseUtil.Parse(Dragon.Value);
            foreach (var selector in SkillSelectorVMs.Value)
            {
                if (Masters.IsSkillName(selector.SkillName.Value))
                {
                    Skill skill = new(selector.SkillName.Value, selector.SkillLevel.Value, true);
                    aug.Skills.Add(skill);
                }
            }
            Simulator.UpdateAugmentation(aug);

            // 装備情報修正・マイセットの内容を更新
            MainViewModel.Instance.LoadEquips();
            Simulator.LoadMySet();
            MainViewModel.Instance.LoadMySets();
        }

        // 反映コマンド
        private void InputAugmentation()
        {
            if (SelectedAugmentation.Value == null)
            {
                return;
            }

            BindableAugmentation aug = SelectedAugmentation.Value;
            DispName.Value = aug.DispName;
            Kind.Value = aug.Kind.Str();
            foreach (var equip in Equips.Value)
            {
                if (equip.Name == aug.BaseName)
                {
                    SelectedEquip.Value = equip;
                }
            }
            Slots.Value = aug.Slot1 + "-" + aug.Slot2 + "-" + aug.Slot3;
            Def.Value = aug.Def.ToString();
            Fire.Value = aug.Fire.ToString();
            Water.Value = aug.Water.ToString();
            Thunder.Value = aug.Thunder.ToString();
            Ice.Value = aug.Ice.ToString();
            Dragon.Value = aug.Dragon.ToString();

            for (int i = 0; i < LogicConfig.Instance.MaxAugmentationSkillCountActual; i++)
            {
                if (i < aug.Skills.Count)
                {
                    SkillSelectorVMs.Value[i].SkillName.Value = aug.Skills[i].Name;
                    SkillSelectorVMs.Value[i].SkillLevel.Value = aug.Skills[i].Level;
                }
                else
                {
                    SkillSelectorVMs.Value[i].SkillName.Value = string.Empty;
                    SkillSelectorVMs.Value[i].SkillLevel.Value = 0;
                }
            }

        }

        // コンストラクタ
        public AugmentationTabViewModel()
        {
            // MainViewModelから参照を取得
            Simulator = MainViewModel.Instance.Simulator;
            StatusBarText = MainViewModel.Instance.StatusBarText;

            // スキル選択部品準備
            ObservableCollection<SkillSelectorViewModel> selectorVMs = new();
            for (int i = 0; i < LogicConfig.Instance.MaxAugmentationSkillCountActual; i++)
            {
                selectorVMs.Add(new SkillSelectorViewModel(SkillSelectorKind.Augmentation));
            }
            SkillSelectorVMs.Value = selectorVMs;

            // スロットの選択肢を生成
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
            Slots.Value = "0-0-0";

            // 部位の選択肢を生成
            ObservableCollection<string> kinds = new();
            kinds.Add(EquipKind.head.Str());
            kinds.Add(EquipKind.body.Str());
            kinds.Add(EquipKind.arm.Str());
            kinds.Add(EquipKind.waist.Str());
            kinds.Add(EquipKind.leg.Str());
            KindMaster.Value = kinds;
            Kind.Value = EquipKind.head.Str();

            // コマンドを設定
            SetCommand();
        }

        // 錬成情報を追加
        private void AddAugmentation()
        {


            if (SelectedEquip.Value == null)
            {
                foreach (var equip in Equips.Value)
                {
                    if (equip.DispName == InputBaseEquipName.Value)
                    {
                        SelectedEquip.Value = equip;
                        break;
                    }
                }
            }

            if (SelectedEquip.Value == null)
            {
                return;
            }

            Augmentation aug = new();
            aug.Name = Guid.NewGuid().ToString();
            string dispName = DispName.Value;
            if (string.IsNullOrWhiteSpace(dispName))
            {
                dispName = Masters.MakeAugmentaionDefaultDispName(SelectedEquip.Value.Name);
            }
            aug.DispName = dispName;
            aug.Kind = Kind.Value.ToEquipKind();
            aug.BaseName = SelectedEquip.Value.Name;
            string[] slots = Slots.Value.Split('-');
            aug.Slot1 = ParseUtil.Parse(slots[0]);
            aug.Slot2 = ParseUtil.Parse(slots[1]);
            aug.Slot3 = ParseUtil.Parse(slots[2]);
            aug.Def = ParseUtil.Parse(Def.Value);
            aug.Fire = ParseUtil.Parse(Fire.Value);
            aug.Water = ParseUtil.Parse(Water.Value);
            aug.Thunder = ParseUtil.Parse(Thunder.Value);
            aug.Ice = ParseUtil.Parse(Ice.Value);
            aug.Dragon = ParseUtil.Parse(Dragon.Value);
            foreach (var selector in SkillSelectorVMs.Value)
            {
                if (Masters.IsSkillName(selector.SkillName.Value))
                {
                    Skill skill = new(selector.SkillName.Value, selector.SkillLevel.Value, true);
                    aug.Skills.Add(skill);
                }
            }

            Simulator.AddAugmentation(aug);

            MainViewModel.Instance.LoadEquips();
        }

        // 錬成情報を削除
        private void DeleteAugmentation()
        {
            BindableAugmentation aug = SelectedAugmentation.Value;

            if (aug == null)
            {
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                $"錬成防具「{aug.DispName}」を削除します。\nよろしいですか？",
                "錬成防具削除",
                MessageBoxButton.YesNo);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            // 除外・固定設定があったら削除
            Simulator.DeleteClude(aug.Name);

            // この装備を使っているマイセットがあったら削除
            DeleteMySetUsingAugmentation(aug.Name);

            // 錬成情報を削除
            Simulator.DeleteAugmentation(SelectedAugmentation.Value.Original);

            // マスタをリロード
            MainViewModel.Instance.LoadCludes();
            MainViewModel.Instance.LoadMySets();
            MainViewModel.Instance.LoadEquips();
        }

        // 指定した錬成装備を使っているマイセットがあったら削除
        private void DeleteMySetUsingAugmentation(string name)
        {
            List<EquipSet> delMySets = new();
            foreach (var set in Masters.MySets)
            {
                if ((set.Head.Name != null && set.Head.Name.Equals(name)) ||
                    (set.Body.Name != null && set.Body.Name.Equals(name)) ||
                    (set.Arm.Name != null && set.Arm.Name.Equals(name)) ||
                    (set.Waist.Name != null && set.Waist.Name.Equals(name)) ||
                    (set.Leg.Name != null && set.Leg.Name.Equals(name)))
                {
                    delMySets.Add(set);
                }
            }
            foreach (var set in delMySets)
            {
                Simulator.DeleteMySet(set);
            }
        }

        // ベース装備一覧を変更
        private void SetEquips()
        {
            switch (Kind.Value)
            {
                case "頭":
                    Equips.Value = BindableEquipment.BeBindableList(Masters.OriginalHeads, null, 8);
                    break;
                case "胴":
                    Equips.Value = BindableEquipment.BeBindableList(Masters.OriginalBodys, null, 8);
                    break;
                case "腕":
                    Equips.Value = BindableEquipment.BeBindableList(Masters.OriginalArms, null, 8);
                    break;
                case "腰":
                    Equips.Value = BindableEquipment.BeBindableList(Masters.OriginalWaists, null, 8);
                    break;
                case "足":
                    Equips.Value = BindableEquipment.BeBindableList(Masters.OriginalLegs, null, 8);
                    break;
                case "脚":
                    // 誤記
                    Equips.Value = BindableEquipment.BeBindableList(Masters.OriginalLegs, null, 8);
                    break;
                default:
                    break;
            }
            SelectedEquip.Value = null;
        }

        // 錬成装備のマスタ情報をVMにロード
        internal void LoadAugmentations()
        {
            // 錬成装備情報の設定
            Augmentations.Value = BindableAugmentation.BeBindableList(Masters.Augmentations);
        }

        // スロットの初期値を反映
        private void SetSlots()
        {
            if (SelectedEquip?.Value != null)
            {
                Slots.Value = SelectedEquip.Value.SlotStr;
            }
        }
    }
}
