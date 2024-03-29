﻿using Prism.Mvvm;
using SimModel.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiseSim.ViewModels.BindableWrapper
{
    internal class BindableEquipSet : BindableBase
    {
        // 頭装備
        public BindableEquipment Head { get; set; }

        // 胴装備
        public BindableEquipment Body { get; set; }

        // 腕装備
        public BindableEquipment Arm { get; set; }

        // 腰装備
        public BindableEquipment Waist { get; set; }

        // 足装備
        public BindableEquipment Leg { get; set; }

        // 護石
        public BindableEquipment Charm { get; set; }

        // 装飾品(リスト)
        public ObservableCollection<BindableEquipment> Decos { get; set; } = new();

        // 理想錬成スキル(リスト)
        public ObservableCollection<BindableEquipment> GenericSkills { get; set; } = new();

        // 武器スロ1つ目
        public int WeaponSlot1 { get; set; }

        // 武器スロ2つ目
        public int WeaponSlot2 { get; set; }

        // 武器スロ3つ目
        public int WeaponSlot3 { get; set; }

        // マイセット用名前
        public string Name { get; set; }

        // 初期防御力
        public int Mindef { get; set; }

        // 最大防御力
        public int Maxdef { get; set; }

        // 火耐性
        public int Fire { get; set; }

        // 水耐性
        public int Water { get; set; }

        // 雷耐性
        public int Thunder { get; set; }

        // 氷耐性
        public int Ice { get; set; }

        // 龍耐性
        public int Dragon { get; set; }

        // スキル(リスト)
        public ObservableCollection<BindableSkill> Skills { get; set; } = new();

        // 表示用CSV表記
        public string SimpleSetName { get; set; }

        // 装飾品のCSV表記 Set可能
        public string DecoNameCSV { get; set; }

        // 装飾品のCSV表記 3行
        public string DecoNameCSVMultiLine { get; set; }

        // 武器スロの表示用形式(2-2-0など)
        public string WeaponSlotDisp { get; set; }

        // スキルのCSV形式
        public string SkillsDisp { get; set; }

        // スキルのCSV形式 3行
        public string SkillsDispMultiLine { get; set; }

        // オリジナル
        public EquipSet Original { get; set; }

        // 説明
        public string Description { get; set; }

        // 空きスロット数
        public string EmptySlotNum { get; set; }

        // 理想錬成防具を含むかどうか
        public bool HasIdeal { get; set; }

        // コンストラクタ
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

        // リストをまとめてバインド用クラスに変換
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
