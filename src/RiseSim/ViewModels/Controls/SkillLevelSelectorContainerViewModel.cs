using SimModel.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RiseSim.ViewModels.Controls
{
    /// <summary>
    /// カテゴリごとにスキルレベル選択部品を表示するコンテナ
    /// </summary>
    internal class SkillLevelSelectorContainerViewModel : ChildViewModelBase
    {
        /// <summary>
        /// Expanderに設定するタイトル。スキルカテゴリを設定する想定
        /// </summary>
        public string Header { get; init; }

        /// <summary>
        /// 格納するスキルレベル選択部品
        /// </summary>
        public ObservableCollection<SkillLevelSelectorViewModel> SkillLevelSelectors { get; init; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="categoryName">カテゴリ名</param>
        /// <param name="skills">スキル一覧</param>
        public SkillLevelSelectorContainerViewModel(string categoryName, IEnumerable<Skill> skills)
        {
            Header = categoryName;
            SkillLevelSelectors = new ObservableCollection<SkillLevelSelectorViewModel>(skills.Select(x =>
                new SkillLevelSelectorViewModel(x)));
        }

        /// <summary>
        /// このコンテナにあるSelectorのうち選択されているスキルの配列を返す
        /// </summary>
        /// <returns>選択されているスキルの配列</returns>
        public IReadOnlyList<Skill> SelectedSkills() => 
            SkillLevelSelectors
                .Where(s => s.Selected.Value)
                .Select(s => new Skill(s.SelectedSkill.Value.Name, s.SelectedSkill.Value.Level, isFixed:s.IsFix))
                .ToList();

        /// <summary>
        /// すべてのスキルレベル選択部品をリセット
        /// </summary>
        internal void ClearAll()
        {
            foreach (var vm in SkillLevelSelectors)
            {
                vm.Reset();
            }
        }

        /// <summary>
        /// 指定のスキルに対応するスキルレベル選択部品を持っていた場合それを適用する
        /// </summary>
        /// <param name="name">スキル名</param>
        /// <param name="level">レベル</param>
        /// <param name="isFixed">固定有無</param>
        /// <returns>適用があった場合true</returns>
        internal bool TryAddSkill(string name, int level, bool isFixed = false)
        {
            foreach (var vm in SkillLevelSelectors)
            {
                if (vm.SelectedSkill.Value.Name == name)
                {
                    vm.SelectLevel(level);
                    vm.SetIsFix(isFixed);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 指定のスキルリストについて、対応するスキルレベル選択部品を持っていた場合それを適用する
        /// </summary>
        /// <param name="skills">スキルリスト</param>
        /// <returns>適用が１つでもあった場合true</returns>
        internal bool TryAddSkill(List<Skill> skills)
        {
            bool addedAny = false;
            foreach (var skill in skills)
            {
                bool added = TryAddSkill(skill.Name, skill.Level, skill.IsFixed);
                addedAny = addedAny || added;
            }
            return addedAny;
        }
    }
}
