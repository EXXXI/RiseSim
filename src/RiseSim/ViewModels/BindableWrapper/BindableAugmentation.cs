using Reactive.Bindings;
using SimModel.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace RiseSim.ViewModels.BindableWrapper
{
    /// <summary>
    /// バインド用錬成
    /// </summary>
    internal class BindableAugmentation : ChildViewModelBase
    {
        /// <summary>
        /// 表示用装備名
        /// </summary>
        public ReactivePropertySlim<string> DispName { get; } = new();

        /// <summary>
        /// 装備種類
        /// </summary>
        public ReactivePropertySlim<string> KindStr { get; } = new();

        /// <summary>
        /// ベース装備名
        /// </summary>
        public ReactivePropertySlim<string> BaseName { get; } = new();

        /// <summary>
        /// スロット表示
        /// </summary>
        public ReactivePropertySlim<string> SlotDisp { get; } = new();

        /// <summary>
        /// 防御力増減
        /// </summary>
        public ReactivePropertySlim<int> Def { get; } = new();

        /// <summary>
        /// 火耐性増減
        /// </summary>
        public ReactivePropertySlim<int> Fire { get; } = new();

        /// <summary>
        /// 水耐性増減
        /// </summary>
        public ReactivePropertySlim<int> Water { get; } = new ();

        /// <summary>
        /// 雷耐性増減
        /// </summary>
        public ReactivePropertySlim<int> Thunder { get; } = new();

        /// <summary>
        /// 氷耐性増減
        /// </summary>
        public ReactivePropertySlim<int> Ice { get; } = new();

        /// <summary>
        /// 龍耐性増減
        /// </summary>
        public ReactivePropertySlim<int> Dragon { get; } = new();

        /// <summary>
        /// スキルのCSV形式
        /// </summary>
        public ReactivePropertySlim<string> SkillsDisp { get; } = new();

        /// <summary>
        /// 装備としての説明
        /// </summary>
        public ReactivePropertySlim<string> EquipDescription { get; } = new();

        /// <summary>
        /// オリジナル
        /// </summary>
        public Augmentation Original { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="aug">元の錬成クラス</param>
        public BindableAugmentation(Augmentation aug)
        {
            DispName.Value = aug.DispName;
            KindStr.Value = aug.KindStr;
            BaseName.Value = aug.BaseName;
            SlotDisp.Value = aug.SlotDisp;
            Def.Value = aug.Def;
            Fire.Value = aug.Fire;
            Water.Value = aug.Water;
            Thunder.Value = aug.Thunder;
            Ice.Value = aug.Ice;
            Dragon.Value = aug.Dragon;
            SkillsDisp.Value = aug.SkillsDisp;
            EquipDescription.Value = Masters.GetEquipByName(aug.Name, false)?.Description ?? string.Empty;
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
