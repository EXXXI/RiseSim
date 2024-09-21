using SimModel.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RiseSim.ViewModels.BindableWrapper
{
    /// <summary>
    /// バインド用Skill
    /// </summary>
    internal class BindableSkill : ChildViewModelBase
    {   
        /// <summary>
        /// スキル名
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// スキルレベル
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 固定フラグ
        /// </summary>
        public bool IsFixed { get; set; }

        /// <summary>
        /// 追加スキルフラグ
        /// </summary>
        public bool IsAdditional { get; set; }

        /// <summary>
        /// 説明
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// オリジナル
        /// </summary>
        public Skill Original { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="skill"></param>
        public BindableSkill(Skill skill)
        {
            Name = skill.Name;
            Level = skill.Level;
            IsAdditional = skill.IsAdditional;
            Description = skill.Description;
            IsFixed = skill.IsFixed;
            Original = skill;
        }

        /// <summary>
        /// リストをまとめてバインド用クラスに変換
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        static public ObservableCollection<BindableSkill> BeBindableList(List<Skill> list)
        {
            ObservableCollection<BindableSkill> bindableList = new ObservableCollection<BindableSkill>();
            foreach (var skill in list)
            {
                bindableList.Add(new BindableSkill(skill));
            }
            return bindableList;
        }

        /// <summary>
        /// レベルを含めた表示に使う文字列を返す
        /// </summary>
        public string LevelDisplayName => Level switch
        {
            0 => Name,
            _ => $"{Name}Lv{Level}"
        };
    }
}
