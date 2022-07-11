/*    RiseSim : MHRise skill simurator for Windows
 *    Copyright (C) 2022  EXXXI
 *
 *    This program is free software: you can redistribute it and/or modify
 *    it under the terms of the GNU General Public License as published by
 *    the Free Software Foundation, either version 3 of the License, or
 *    (at your option) any later version.
 *
 *    This program is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 *    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *    GNU General Public License for more details.
 *
 *    You should have received a copy of the GNU General Public License
 *    along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
using Prism.Mvvm;
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

        // 武器スロの表示用形式(2-2-0など)
        public string WeaponSlotDisp { get; set; }

        // スキルのCSV形式
        public string SkillsDisp { get; set; }

        // オリジナル
        public EquipSet Original { get; set; }

        // 説明
        public string Description { get; set; }

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
            WeaponSlotDisp = set.WeaponSlotDisp;
            SkillsDisp = set.SkillsDisp;
            Description = set.Description;
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
