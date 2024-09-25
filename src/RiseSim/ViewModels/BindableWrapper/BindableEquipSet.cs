using Reactive.Bindings;
using SimModel.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RiseSim.ViewModels.BindableWrapper
{
    /// <summary>
    /// バインド用装備セット
    /// </summary>
    internal class BindableEquipSet : ChildViewModelBase
    {
        /// <summary>
        /// 頭装備
        /// </summary>
        public ReactivePropertySlim<BindableEquipment> Head { get; } = new();

        /// <summary>
        /// 胴装備
        /// </summary>
        public ReactivePropertySlim<BindableEquipment> Body { get; } = new();

        /// <summary>
        /// 腕装備
        /// </summary>
        public ReactivePropertySlim<BindableEquipment> Arm { get; } = new();

        /// <summary>
        /// 腰装備
        /// </summary>
        public ReactivePropertySlim<BindableEquipment> Waist { get; } = new();

        /// <summary>
        /// 足装備
        /// </summary>
        public ReactivePropertySlim<BindableEquipment> Leg { get; } = new();

        /// <summary>
        /// 護石
        /// </summary>
        public ReactivePropertySlim<BindableEquipment> Charm { get; } = new();

        /// <summary>
        /// マイセット用名前
        /// </summary>
        public ReactivePropertySlim<string> Name { get; } = new();

        /// <summary>
        /// 最大防御力
        /// </summary>
        public ReactivePropertySlim<int> Maxdef { get; } = new();

        /// <summary>
        /// 火耐性
        /// </summary>
        public ReactivePropertySlim<int> Fire { get; } = new();

        /// <summary>
        /// 水耐性
        /// </summary>
        public ReactivePropertySlim<int> Water { get; } = new();

        /// <summary>
        /// 雷耐性
        /// </summary>
        public ReactivePropertySlim<int> Thunder { get; } = new();

        /// <summary>
        /// 氷耐性
        /// </summary>
        public ReactivePropertySlim<int> Ice { get; } = new();

        /// <summary>
        /// 龍耐性
        /// </summary>
        public ReactivePropertySlim<int> Dragon { get; } = new();

        /// <summary>
        /// 装飾品のCSV表記 3行
        /// </summary>
        public ReactivePropertySlim<string> DecoNameCSV { get; } = new();

        /// <summary>
        /// 武器スロの表示用形式(2-2-0など)
        /// </summary>
        public ReactivePropertySlim<string> WeaponSlotDisp { get; } = new();

        /// <summary>
        /// スキルのCSV形式 3行
        /// </summary>
        public ReactivePropertySlim<string> SkillsDisp { get; } = new();

        /// <summary>
        /// 説明
        /// </summary>
        public ReactivePropertySlim<string> Description { get; } = new();

        /// <summary>
        /// 空きスロット数
        /// </summary>
        public ReactivePropertySlim<string> EmptySlotNum { get; set; } = new();

        /// <summary>
        /// オリジナル
        /// </summary>
        public EquipSet Original { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="set"></param>
        public BindableEquipSet(EquipSet set)
        {
            Head.Value = new BindableEquipment(set.Head);
            Body.Value = new BindableEquipment(set.Body);
            Arm.Value = new BindableEquipment(set.Arm);
            Waist.Value = new BindableEquipment(set.Waist);
            Leg.Value = new BindableEquipment(set.Leg);
            Charm.Value = new BindableEquipment(set.Charm);
            Name.Value = set.Name;
            Maxdef.Value = set.Maxdef;
            Fire.Value = set.Fire;
            Water.Value = set.Water;
            Thunder.Value = set.Thunder;
            Ice.Value = set.Ice;
            Dragon.Value = set.Dragon;
            DecoNameCSV.Value = set.DecoNameCSVMultiLine;
            WeaponSlotDisp.Value = set.WeaponSlotDisp;
            SkillsDisp.Value = set.SkillsDispMultiLine;
            Description.Value = set.Description;
            EmptySlotNum.Value = set.EmptySlotNum;
            Original = set;
        }

        /// <summary>
        /// リストをまとめてバインド用クラスに変換
        /// </summary>
        /// <param name="list">変換前リスト</param>
        /// <returns></returns>
        static public ObservableCollection<BindableEquipSet> BeBindableList(List<EquipSet> list)
        {
            ObservableCollection<BindableEquipSet> bindableList = new ObservableCollection<BindableEquipSet>();
            foreach (var set in list)
            {
                bindableList.Add(new BindableEquipSet(set));
            }
            return bindableList;
        }
    }
}
