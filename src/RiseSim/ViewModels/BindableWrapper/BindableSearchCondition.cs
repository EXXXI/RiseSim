using Reactive.Bindings;
using SimModel.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiseSim.ViewModels.BindableWrapper
{
    internal class BindableSearchCondition
    {
        // スキルリスト
        public ObservableCollection<BindableSkill> Skills { get; set; } = new();

        // 武器スロ
        public ReactivePropertySlim<string> WeaponSlot { get; set; } = new();

        // 防御力
        public ReactivePropertySlim<string> Def { get; set; } = new();

        // 火耐性
        public ReactivePropertySlim<string> Fire { get; set; } = new();

        // 水耐性
        public ReactivePropertySlim<string> Water { get; set; } = new();

        // 雷耐性
        public ReactivePropertySlim<string> Thunder { get; set; } = new();

        // 氷耐性
        public ReactivePropertySlim<string> Ice { get; set; } = new();

        // 龍耐性
        public ReactivePropertySlim<string> Dragon { get; set; } = new();

        // 性別
        public ReactivePropertySlim<string> SexCond { get; set; } = new();

        // マイ検索条件保存用ID
        public string ID { get; set; }

        // マイ検索条件保存用名前
        public ReactivePropertySlim<string> DispName { get; set; } = new();

        // CSV用スキル形式
        public ReactivePropertySlim<string> SkillCSV { get; set; } = new();

        public SearchCondition Original { get; set; }

        // コンストラクタ
        public BindableSearchCondition(SearchCondition condition)
        {
            Skills = BindableSkill.BeBindableList(condition.Skills);
            WeaponSlot.Value = condition.WeaponSlot1 + "-" + condition.WeaponSlot2 + "-" + condition.WeaponSlot3;
            Def.Value = condition.Def?.ToString() ?? "なし";
            Fire.Value = condition.Fire?.ToString() ?? "なし";
            Water.Value = condition.Water?.ToString() ?? "なし";
            Thunder.Value = condition.Thunder?.ToString() ?? "なし";
            Ice.Value = condition.Ice?.ToString() ?? "なし";
            Dragon.Value = condition.Dragon?.ToString() ?? "なし";
            SexCond.Value = condition.Sex == Sex.male ? Sex.male.Str() : Sex.female.Str();
            ID = condition.ID;
            DispName.Value = condition.DispName;
            SkillCSV.Value = condition.SkillCSV;
            Original = condition;
        }

        // リストをまとめてバインド用クラスに変換
        static public ObservableCollection<BindableSearchCondition> BeBindableList(List<SearchCondition> list)
        {
            ObservableCollection<BindableSearchCondition> bindableList = new ObservableCollection<BindableSearchCondition>();
            foreach (var condition in list)
            {
                bindableList.Add(new BindableSearchCondition(condition));
            }
            return bindableList;
        }
    }
}
