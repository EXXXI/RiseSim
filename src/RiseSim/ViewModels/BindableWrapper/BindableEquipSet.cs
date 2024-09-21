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
        public BindableEquipment Head { get; set; }

        /// <summary>
        /// 胴装備
        /// </summary>
        public BindableEquipment Body { get; set; }

        /// <summary>
        /// 腕装備
        /// </summary>
        public BindableEquipment Arm { get; set; }

        /// <summary>
        /// 腰装備
        /// </summary>
        public BindableEquipment Waist { get; set; }

        /// <summary>
        /// 足装備
        /// </summary>
        public BindableEquipment Leg { get; set; }

        /// <summary>
        /// 護石
        /// </summary>
        public BindableEquipment Charm { get; set; }

        /// <summary>
        /// 装飾品(リスト)
        /// </summary>
        public ObservableCollection<BindableEquipment> Decos { get; set; } = new();

        /// <summary>
        /// 理想錬成スキル(リスト)
        /// </summary>
        public ObservableCollection<BindableEquipment> GenericSkills { get; set; } = new();

        /// <summary>
        /// 武器スロ1つ目
        /// </summary>
        public int WeaponSlot1 { get; set; }

        /// <summary>
        /// 武器スロ2つ目
        /// </summary>
        public int WeaponSlot2 { get; set; }

        /// <summary>
        /// 武器スロ3つ目
        /// </summary>
        public int WeaponSlot3 { get; set; }

        /// <summary>
        /// マイセット用名前
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 初期防御力
        /// </summary>
        public int Mindef { get; set; }

        /// <summary>
        /// 最大防御力
        /// </summary>
        public int Maxdef { get; set; }

        /// <summary>
        /// 火耐性
        /// </summary>
        public int Fire { get; set; }

        /// <summary>
        /// 水耐性
        /// </summary>
        public int Water { get; set; }

        /// <summary>
        /// 雷耐性
        /// </summary>
        public int Thunder { get; set; }

        /// <summary>
        /// 氷耐性
        /// </summary>
        public int Ice { get; set; }

        /// <summary>
        /// 龍耐性
        /// </summary>
        public int Dragon { get; set; }

        /// <summary>
        /// スキル(リスト)
        /// </summary>
        public ObservableCollection<BindableSkill> Skills { get; set; } = new();

        /// <summary>
        /// 表示用CSV表記
        /// </summary>
        public string SimpleSetName { get; set; }

        /// <summary>
        /// 装飾品のCSV表記 Set可能
        /// </summary>
        public string DecoNameCSV { get; set; }

        /// <summary>
        /// 装飾品のCSV表記 3行
        /// </summary>
        public string DecoNameCSVMultiLine { get; set; }

        /// <summary>
        /// 武器スロの表示用形式(2-2-0など)
        /// </summary>
        public string WeaponSlotDisp { get; set; }

        /// <summary>
        /// スキルのCSV形式
        /// </summary>
        public string SkillsDisp { get; set; }

        /// <summary>
        /// スキルのCSV形式 3行
        /// </summary>
        public string SkillsDispMultiLine { get; set; }

        /// <summary>
        /// オリジナル
        /// </summary>
        public EquipSet Original { get; set; }

        /// <summary>
        /// 説明
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 空きスロット数
        /// </summary>
        public string EmptySlotNum { get; set; }

        /// <summary>
        /// 理想錬成防具を含むかどうか
        /// </summary>
        public bool HasIdeal { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="set"></param>
        public BindableEquipSet(EquipSet set)
        {
            Head = new BindableEquipment(set.Head);
            Body = new BindableEquipment(set.Body);
            Arm = new BindableEquipment(set.Arm);
            Waist = new BindableEquipment(set.Waist);
            Leg = new BindableEquipment(set.Leg);
            Charm = new BindableEquipment(set.Charm);
            Decos = BindableEquipment.BeBindableList(set.Decos);
            WeaponSlot1 = set.WeaponSlot1;
            WeaponSlot2 = set.WeaponSlot2;
            WeaponSlot3 = set.WeaponSlot3;
            Name = set.Name;
            Mindef = set.Mindef;
            Maxdef = set.Maxdef;
            Fire = set.Fire;
            Water = set.Water;
            Thunder = set.Thunder;
            Ice = set.Ice;
            Dragon = set.Dragon;
            Skills = BindableSkill.BeBindableList(set.Skills);
            SimpleSetName = set.SimpleSetName;
            DecoNameCSV = set.DecoNameCSV;
            DecoNameCSVMultiLine = set.DecoNameCSVMultiLine;
            WeaponSlotDisp = set.WeaponSlotDisp;
            SkillsDisp = set.SkillsDisp;
            SkillsDispMultiLine = set.SkillsDispMultiLine;
            Description = set.Description;
            EmptySlotNum = set.EmptySlotNum;
            GenericSkills = BindableEquipment.BeBindableList(set.GenericSkills);
            HasIdeal = set.HasIdeal;
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
