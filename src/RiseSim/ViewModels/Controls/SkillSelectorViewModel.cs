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
using Reactive.Bindings;
using RiseSim.Config;
using SimModel.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiseSim.ViewModels.Controls
{
    class SkillSelectorViewModel : BindableBase
    {
        // スキル未選択時の表示
        private string NoSkillName { get; } = ViewConfig.Instance.NoSkillName;

        // スキル名一覧
        public ReactivePropertySlim<ObservableCollection<string>> Skills { get; } = new();

        // 選択中スキルのレベル一覧
        public ReactivePropertySlim<ObservableCollection<int>> SkillLevels { get; } = new();

        // 選択中スキル名
        public ReactivePropertySlim<string> SkillName { get; } = new();

        // 選択中スキルレベル
        public ReactivePropertySlim<int> SkillLevel { get; } = new();

        // クリアコマンド
        public ReactiveCommand ClearCommand { get; } = new ReactiveCommand();

        // コマンドを設定
        private void SetCommand()
        {
            ClearCommand.Subscribe(_ => SetDefault());
        }


        // 空行(「スキル選択」の行)を追加してスキルマスタを読み込み
        public SkillSelectorViewModel()
        {
            ObservableCollection<string> skillList = new();
            skillList.Add(NoSkillName);
            foreach (var skill in Masters.Skills)
            {
                skillList.Add(skill.Name);
            }
            Skills.Value = skillList;
            SkillName.Value = NoSkillName;

            // スキル名変更時にレベル一覧を変更するように紐づけ
            SkillName.Subscribe(_ => SetLevels());

            // コマンドを設定
            SetCommand();
        }

        // 選択中スキル名にあわせてスキルレベルの選択肢を変更
        internal void SetLevels()
        {
            ObservableCollection<int> list = new();
            int maxLevel = 0;
            foreach (var skill in Masters.Skills)
            {
                if (skill.Name.Equals(SkillName.Value))
                {
                    maxLevel = skill.Level;
                }
            }
            for (int i = maxLevel; i > 0; i--)
            {
                list.Add(i);
            }
            SkillLevels.Value = list;

            if (list.Count == 0)
            {
                // スキルが選択されていないときは0とする
                list.Add(0);
            }
            // 初期値は最大レベルとする
            SkillLevel.Value = maxLevel;
        }

        internal void SetDefault()
        {
            SkillName.Value = NoSkillName;
        }
    }
}
