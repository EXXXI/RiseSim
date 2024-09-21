using Reactive.Bindings;
using SimModel.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace RiseSim.ViewModels.BindableWrapper
{
    /// <summary>
    /// バインド用マイ検索条件
    /// </summary>
    internal class BindableSearchCondition : ChildViewModelBase
    {
        /// <summary>
        /// スキルリスト
        /// </summary>
        public ObservableCollection<BindableSkill> Skills { get; set; } = new();

        /// <summary>
        /// 武器スロ
        /// </summary>
        public ReactivePropertySlim<string> WeaponSlot { get; set; } = new();

        /// <summary>
        /// 防御力
        /// </summary>
        public ReactivePropertySlim<string> Def { get; set; } = new();

        /// <summary>
        /// 火耐性
        /// </summary>
        public ReactivePropertySlim<string> Fire { get; set; } = new();

        /// <summary>
        /// 水耐性
        /// </summary>
        public ReactivePropertySlim<string> Water { get; set; } = new();

        /// <summary>
        /// 雷耐性
        /// </summary>
        public ReactivePropertySlim<string> Thunder { get; set; } = new();

        /// <summary>
        /// 氷耐性
        /// </summary>
        public ReactivePropertySlim<string> Ice { get; set; } = new();

        /// <summary>
        /// 龍耐性
        /// </summary>
        public ReactivePropertySlim<string> Dragon { get; set; } = new();

        /// <summary>
        /// 性別
        /// </summary>
        public ReactivePropertySlim<string> SexCond { get; set; } = new();

        /// <summary>
        /// マイ検索条件保存用ID
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// マイ検索条件保存用名前
        /// </summary>
        public ReactivePropertySlim<string> DispName { get; set; } = new();

        /// <summary>
        /// CSV用スキル形式
        /// </summary>
        public ReactivePropertySlim<string> SkillCSV { get; set; } = new();

        /// <summary>
        /// 表示用詳細
        /// </summary>
        public ReactivePropertySlim<string> Description { get; set; } = new();

        /// <summary>
        /// オリジナル
        /// </summary>
        public SearchCondition Original { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="condition"></param>
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

            Description.Value = MakeDescription(condition);
        }

        /// <summary>
        /// 表示用説明
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns>表示用説明文字列</returns>
        private string MakeDescription(SearchCondition condition)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"武器スロ:{WeaponSlot.Value}");
            sb.AppendLine($"防御力:{Def.Value}, 性別:{SexCond.Value}");
            sb.AppendLine($"火:{Fire.Value},水:{Water.Value},雷:{Thunder.Value},氷:{Ice.Value},龍:{Dragon.Value}");
            foreach (var skill in Skills)
            {
                sb.Append(skill.LevelDisplayName);
                sb.AppendLine(skill.IsFixed ? "(固定)" : string.Empty);
            }

            return sb.ToString();
        }

        /// <summary>
        /// リストをまとめてバインド用クラスに変換
        /// </summary>
        /// <param name="list">リスト</param>
        /// <returns></returns>
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
