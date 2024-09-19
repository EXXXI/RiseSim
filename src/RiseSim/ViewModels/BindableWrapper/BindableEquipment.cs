using SimModel.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RiseSim.ViewModels.BindableWrapper
{
    /// <summary>
    /// バインド用装備
    /// </summary>
    internal class BindableEquipment : ChildViewModelBase
    {
        /// <summary>
        /// 管理用装備名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 性別制限
        /// </summary>
        public Sex Sex { get; set; }

        /// <summary>
        /// レア度
        /// </summary>
        public int Rare { get; set; }

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
        /// スロットの文字列表現
        /// </summary>
        public string SlotStr { get { return Slot1 + "-" + Slot2 + "-" + Slot3; } }

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
        /// スキル
        /// </summary>
        public ObservableCollection<BindableSkill> Skills { get; set; } = new();

        /// <summary>
        /// 装備種類
        /// </summary>
        public EquipKind Kind { get; set; }

        /// <summary>
        /// 表示用装備名(護石のみ特殊処理)
        /// </summary>
        public string DispName { get; set; }

        // TODO:仮実装
        /// <summary>
        /// 一覧での詳細表示用
        /// </summary>
        public string DetailDispName { get; set; }

        /// <summary>
        /// 装備の説明
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 装備の簡易説明(名前とスロットのみ)
        /// </summary>
        public string SimpleDescription { get; set; }

        /// <summary>
        /// オリジナル
        /// </summary>
        public Equipment Original { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="equip">装備情報</param>
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
            DetailDispName = equip.DetailDispName;
            Description = equip.Description;
            SimpleDescription = equip.SimpleDescription;
            Original = equip;
        }

        /// <summary>
        /// リストをまとめてバインド用クラスに変換
        /// </summary>
        /// <param name="list">変換前リスト</param>
        /// <param name="filter">フィルタ文字列</param>
        /// <param name="minRare">最低レア度</param>
        /// <returns></returns>
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

        /// <summary>
        /// リストをまとめてバインド用クラスに変換
        /// </summary>
        /// <param name="list">変換前リスト</param>
        /// <param name="filter">フィルタ文字列</param>
        /// <returns></returns>
        static public ObservableCollection<BindableEquipment> BeBindableList(List<Equipment> list, string? filter)
        {
            return BeBindableList(list, null, 0);
        }

        /// <summary>
        /// リストをまとめてバインド用クラスに変換
        /// </summary>
        /// <param name="list">変換前リスト</param>
        /// <returns></returns>
        static public ObservableCollection<BindableEquipment> BeBindableList(List<Equipment> list)
        {
            return BeBindableList(list, null);
        }
    }
}
