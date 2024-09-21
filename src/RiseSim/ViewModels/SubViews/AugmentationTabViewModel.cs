using Reactive.Bindings;
using RiseSim.Config;
using RiseSim.Util;
using RiseSim.ViewModels.BindableWrapper;
using RiseSim.ViewModels.Controls;
using SimModel.Config;
using SimModel.Domain;
using SimModel.Model;
using SimModel.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace RiseSim.ViewModels.SubViews
{
    /// <summary>
    /// 錬成装備タブのVM
    /// </summary>
    internal class AugmentationTabViewModel : ChildViewModelBase
    {
        /// <summary>
        /// スロットの最大の大きさ
        /// </summary>
        private int MaxSlotSize { get; } = ViewConfig.Instance.MaxSlotSize;

        /// <summary>
        /// 部位指定の選択肢
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<string>> KindMaster { get; } = new();

        /// <summary>
        /// 部位指定
        /// </summary>
        public ReactivePropertySlim<string> Kind { get; } = new();

        /// <summary>
        /// ベース装備一覧
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<BindableEquipment>> Equips { get; } = new();

        /// <summary>
        /// 選択したベース装備
        /// </summary>
        public ReactivePropertySlim<BindableEquipment?> SelectedEquip { get; } = new();

        /// <summary>
        /// 入力中のベース装備名
        /// </summary>
        public ReactivePropertySlim<string> InputBaseEquipName { get; } = new();

        /// <summary>
        /// 名前
        /// </summary>
        public ReactivePropertySlim<string> DispName { get; } = new();

        /// <summary>
        /// 増加防御力
        /// </summary>
        public ReactivePropertySlim<string> Def { get; } = new();

        /// <summary>
        /// スロット選択の選択肢
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<string>> SlotMaster { get; } = new();

        /// <summary>
        /// スロット
        /// </summary>
        public ReactivePropertySlim<string> Slots { get; } = new();

        /// <summary>
        /// 増加火耐性
        /// </summary>
        public ReactivePropertySlim<string> Fire { get; } = new();

        /// <summary>
        /// 増加水耐性
        /// </summary>
        public ReactivePropertySlim<string> Water { get; } = new();

        /// <summary>
        /// 増加雷耐性
        /// </summary>
        public ReactivePropertySlim<string> Thunder { get; } = new();

        /// <summary>
        /// 増加氷耐性
        /// </summary>
        public ReactivePropertySlim<string> Ice { get; } = new();

        /// <summary>
        /// 増加龍耐性
        /// </summary>
        public ReactivePropertySlim<string> Dragon { get; } = new();

        /// <summary>
        /// スキル選択部品のVM
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<SkillSelectorViewModel>> SkillSelectorVMs { get; } = new();

        /// <summary>
        /// 錬成装備一覧
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<BindableAugmentation>> Augmentations { get; } = new();

        /// <summary>
        /// 一覧の選択行データ
        /// </summary>
        public ReactivePropertySlim<BindableAugmentation> SelectedAugmentation { get; } = new();

        /// <summary>
        /// 追加コマンド
        /// </summary>
        public ReactiveCommand AddCommand { get; private set; } = new();

        /// <summary>
        /// 削除コマンド
        /// </summary>
        public ReactiveCommand DeleteCommand { get; private set; } = new();

        /// <summary>
        /// 反映コマンド
        /// </summary>
        public ReactiveCommand InputCommand { get; private set; } = new();

        /// <summary>
        /// 上書きコマンド
        /// </summary>
        public ReactiveCommand UpdateCommand { get; private set; } = new();


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AugmentationTabViewModel()
        {
            // スキル選択部品準備
            ObservableCollection<SkillSelectorViewModel> selectorVMs = new();
            for (int i = 0; i < LogicConfig.Instance.MaxAugmentationSkillCountActual; i++)
            {
                selectorVMs.Add(new SkillSelectorViewModel(SkillSelectorKind.Augmentation));
            }
            SkillSelectorVMs.ChangeCollection(selectorVMs);

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
            Kind.Subscribe(_ => SetEquips());
            SelectedEquip.Subscribe(_ => SetSlots());
            AddCommand.Subscribe(_ => AddAugmentation());
            DeleteCommand.Subscribe(_ => DeleteAugmentation());
            InputCommand.Subscribe(_ => InputAugmentation());
            UpdateCommand.Subscribe(_ => UpdateAugmentation());
        }

        /// <summary>
        /// 上書きコマンド
        /// </summary>
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
            MainVM.LoadEquips();
            Simulator.LoadMySet();
            MySetTabVM.LoadMySets();

            // ログ
            SetStatusBar("錬成装備上書き完了：" + aug.DispName);
        }

        /// <summary>
        /// 反映コマンド
        /// </summary>
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

            // ログ
            SetStatusBar("錬成装備反映完了：" + aug.DispName);
        }

        /// <summary>
        /// 錬成情報を追加
        /// </summary>
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

            MainVM.LoadEquips();

            // ログ
            SetStatusBar("錬成装備追加完了：" + aug.DispName);
        }

        /// <summary>
        /// 錬成情報を削除
        /// </summary>
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
            CludeTabVM.LoadCludes();
            MySetTabVM.LoadMySets();
            MainVM.LoadEquips();

            // ログ
            SetStatusBar("錬成装備削除完了：" + aug.DispName);
        }

        /// <summary>
        /// 指定した錬成装備を使っているマイセットがあったら削除
        /// </summary>
        /// <param name="name">錬成装備名</param>
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

        /// <summary>
        /// ベース装備一覧を変更
        /// </summary>
        private void SetEquips()
        {
            switch (Kind.Value)
            {
                case "頭":
                    Equips.ChangeCollection(BindableEquipment.BeBindableList(Masters.OriginalHeads, null, 8));
                    break;
                case "胴":
                    Equips.ChangeCollection(BindableEquipment.BeBindableList(Masters.OriginalBodys, null, 8));
                    break;
                case "腕":
                    Equips.ChangeCollection(BindableEquipment.BeBindableList(Masters.OriginalArms, null, 8));
                    break;
                case "腰":
                    Equips.ChangeCollection(BindableEquipment.BeBindableList(Masters.OriginalWaists, null, 8));
                    break;
                case "足":
                    Equips.ChangeCollection(BindableEquipment.BeBindableList(Masters.OriginalLegs, null, 8));
                    break;
                case "脚":
                    // 誤記
                    Equips.ChangeCollection(BindableEquipment.BeBindableList(Masters.OriginalLegs, null, 8));
                    break;
                default:
                    break;
            }
            SelectedEquip.Value = null;
        }

        /// <summary>
        /// 錬成装備のマスタ情報をVMにロード
        /// </summary>
        internal void LoadAugmentations()
        {
            // 錬成装備情報の設定
            Augmentations.ChangeCollection(BindableAugmentation.BeBindableList(Masters.Augmentations));
        }

        /// <summary>
        /// スロットの初期値を反映
        /// </summary>
        private void SetSlots()
        {
            if (SelectedEquip?.Value != null)
            {
                Slots.Value = SelectedEquip.Value.SlotStr;
            }
        }
    }
}
