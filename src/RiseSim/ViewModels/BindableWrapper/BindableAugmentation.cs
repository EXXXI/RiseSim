using SimModel.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RiseSim.ViewModels.BindableWrapper
{
    /// <summary>
    /// バインド用錬成
    /// </summary>
    internal class BindableAugmentation : ChildViewModelBase
    {
        /// <summary>
        /// 管理用装備名(GUID)
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 表示用装備名
        /// </summary>
        public string DispName { get; set; } = string.Empty;

        /// <summary>
        /// 装備種類
        /// </summary>
        public EquipKind Kind { get; set; }

        /// <summary>
        /// 装備種類(文字列)
        /// </summary>
        public string KindStr { get; set; }

        /// <summary>
        /// ベース装備名
        /// </summary>
        public string BaseName { get; set; } = string.Empty;

        /// <summary>
        /// スロット1つ目
        /// </summary>
        public int Slot1 { get; set; }

        /// <summary>
        /// スロット2つ目
        /// </summary>
        public int Slot2 { get; set; }

        /// <summary>
        /// スロット3つ目
        /// </summary>
        public int Slot3 { get; set; }

        /// <summary>
        /// スロット表示
        /// </summary>
        public string SlotDisp { get; set; }

        /// <summary>
        /// 防御力増減
        /// </summary>
        public int Def { get; set; }

        /// <summary>
        /// 火耐性増減
        /// </summary>
        public int Fire { get; set; }

        /// <summary>
        /// 水耐性増減
        /// </summary>
        public int Water { get; set; }

        /// <summary>
        /// 雷耐性増減
        /// </summary>
        public int Thunder { get; set; }

        /// <summary>
        /// 氷耐性増減
        /// </summary>
        public int Ice { get; set; }

        /// <summary>
        /// 龍耐性増減
        /// </summary>
        public int Dragon { get; set; }

        /// <summary>
        /// 追加スキル
        /// </summary>
        public ObservableCollection<BindableSkill> Skills { get; set; } = new();

        /// <summary>
        /// スキルのCSV形式
        /// </summary>
        public string SkillsDisp { get; set; }

        /// <summary>
        /// オリジナル
        /// </summary>
        public Augmentation Original { get; set; }

        /// <summary>
        /// 防具データ(ベース含)
        /// </summary>
        public BindableEquipment Equip { 
            get
            {
                return new BindableEquipment(Masters.GetEquipByName(Name, false));
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="aug">元の錬成クラス</param>
        public BindableAugmentation(Augmentation aug)
        {
            Name = aug.Name;
            DispName = aug.DispName;
            Kind = aug.Kind;
            KindStr = aug.KindStr;
            BaseName = aug.BaseName;
            Slot1 = aug.Slot1;
            Slot2 = aug.Slot2;
            Slot3 = aug.Slot3;
            SlotDisp = aug.SlotDisp;
            Def = aug.Def;
            Fire = aug.Fire;
            Water = aug.Water;
            Thunder = aug.Thunder;
            Ice = aug.Ice;
            Dragon = aug.Dragon;
            Skills = BindableSkill.BeBindableList(aug.Skills);
            SkillsDisp = aug.SkillsDisp;
            Original = aug;
        }

        /// <summary>
        /// リストをまとめてバインド用クラスに変換
        /// </summary>
        /// <param name="list">変換前リスト</param>
        /// <returns></returns>
        static public ObservableCollection<BindableAugmentation> BeBindableList(List<Augmentation> list)
        {
            ObservableCollection<BindableAugmentation> bindableList = new ObservableCollection<BindableAugmentation>();
            foreach (var aug in list)
            {
                bindableList.Add(new BindableAugmentation(aug));
            }
            return bindableList;
        }
    }
}
