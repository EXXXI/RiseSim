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
    internal class BindableEquipment : BindableBase
    {
        // 管理用装備名
        public string Name { get; set; }

        // 性別制限
        public Sex Sex { get; set; }

        // レア度
        public int Rare { get; set; }

        // スロット1つ目
        public int Slot1 { get; set; }

        // スロット2つ目
        public int Slot2 { get; set; }

        // スロット3つ目
        public int Slot3 { get; set; }

        // スロットの文字列表現
        public string SlotStr { get { return Slot1 + "-" + Slot2 + "-" + Slot3; } }

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

        // スキル
        public ObservableCollection<BindableSkill> Skills { get; set; } = new();

        // 装備種類
        public EquipKind Kind { get; set; }

        // 表示用装備名(護石のみ特殊処理)
        public string DispName { get; set; }

        // 装備の説明
        public string Description { get; set; }

        // 装備の簡易説明(名前とスロットのみ)
        public string SimpleDescription { get; set; }

        // オリジナル
        public Equipment Original { get; set; }

        // コンストラクタ
        public BindableEquipment(Equipment equip)
        {
            Name = equip.Name;
            Sex = equip.Sex;
            Rare = equip.Rare;
            Slot1 = equip.Slot1;
            Slot2 = equip.Slot2;
            Slot3 = equip.Slot3;
            Mindef = equip.Mindef;
            Maxdef = equip.Maxdef;
            Fire = equip.Fire;
            Water = equip.Water;
            Thunder = equip.Thunder;
            Ice = equip.Ice;
            Dragon = equip.Dragon;
            Skills = BindableSkill.BeBindableList(equip.Skills);
            Kind = equip.Kind;
            DispName = equip.DispName;
            Description = equip.Description;
            SimpleDescription = equip.SimpleDescription;
            Original = equip;
        }

        // リストをまとめてバインド用クラスに変換
        static public ObservableCollection<BindableEquipment> BeBindableList(List<Equipment> list, string? filter, int minRare)
        {
            ObservableCollection<BindableEquipment> bindableList = new ObservableCollection<BindableEquipment>();
            foreach (var equip in list)
            {
                if ((equip.Rare == 0 || equip.Rare >= minRare) &&
                    (string.IsNullOrWhiteSpace(filter) || equip.DispName.Contains(filter)))
                {
                    bindableList.Add(new BindableEquipment(equip));
                }
            }

            // フィルタ結果が0件の場合フィルタを無効化して再計算
            if (!string.IsNullOrWhiteSpace(filter) && bindableList.Count == 0)
            {
                return BeBindableList(list, null);
            }

            // 返却
            return bindableList;
        }

        // リストをまとめてバインド用クラスに変換
        static public ObservableCollection<BindableEquipment> BeBindableList(List<Equipment> list, string? filter)
        {
            return BeBindableList(list, null, 0);
        }

        // リストをまとめてバインド用クラスに変換
        static public ObservableCollection<BindableEquipment> BeBindableList(List<Equipment> list)
        {
            return BeBindableList(list, null);
        }
    }
}
