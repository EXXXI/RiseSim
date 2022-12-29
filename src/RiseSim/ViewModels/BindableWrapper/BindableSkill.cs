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
    internal class BindableSkill : BindableBase
    {   
        // スキル名
        public string Name { get; }

        // スキルレベル
        public int Level { get; }

        // 追加スキルフラグ
        public bool IsAdditional { get; set; }

        // 説明
        public string Description { get; }

        // オリジナル
        public Skill Original { get; set; }

        // コンストラクタ
        public BindableSkill(Skill skill)
        {
            Name = skill.Name;
            Level = skill.Level;
            IsAdditional = skill.IsAdditional;
            Description = skill.Description;
            Original = skill;
        }

        // リストをまとめてバインド用クラスに変換
        static public ObservableCollection<BindableSkill> BeBindableList(List<Skill> list)
        {
            ObservableCollection<BindableSkill> bindableList = new ObservableCollection<BindableSkill>();
            foreach (var skill in list)
            {
                bindableList.Add(new BindableSkill(skill));
            }
            return bindableList;
        }
    }
}
