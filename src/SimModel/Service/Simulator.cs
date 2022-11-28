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
using SimModel.Config;
using SimModel.Domain;
using SimModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimModel.Service
{
    // シミュ本体
    public class Simulator
    {
        // 検索インスタンス
        private ISearcher Searcher { get; set; }

        // 全件検索完了フラグ
        public bool IsSearchedAll { get; set; }

        // データ読み込み
        public void LoadData()
        {
            // マスタデータ類の読み込み
            CsvOperation.LoadSkillCSV();
            CsvOperation.LoadHeadCSV();
            CsvOperation.LoadBodyCSV();
            CsvOperation.LoadArmCSV();
            CsvOperation.LoadWaistCSV();
            CsvOperation.LoadLegCSV();
            CsvOperation.LoadDecoCSV();

            // セーブデータ類の読み込み
            CsvOperation.LoadCludeCSV();
            CsvOperation.LoadCharmCSV();
            CsvOperation.LoadAugmentationCSV();
            CsvOperation.LoadRecentSkillCSV();

            // 錬成装備込みのマスタデータ作成&マイセット読み込み
            Masters.RefreshEquipmentMasters();
            CsvOperation.LoadMySetCSV();
        }

        // 新規検索
        public List<EquipSet> Search(
            List<Skill> skillList, int weaponSlot1, int weaponSlot2, int weaponSlot3, int limit, Sex sex, int? def, int? fire, int? water, int? thunder, int? ice, int? dragon)
        {
            // 検索条件を整理
            SearchCondition condition = new();
            condition.Skills = new List<Skill>();
            foreach (var skill in skillList)
            {
                condition.AddSkill(skill);
            }
            condition.WeaponSlot1 = weaponSlot1;
            condition.WeaponSlot2 = weaponSlot2;
            condition.WeaponSlot3 = weaponSlot3;
            condition.Sex = sex;
            condition.Fire = fire;
            condition.Water = water;
            condition.Thunder = thunder;
            condition.Ice = ice;
            condition.Dragon = dragon;
            condition.Def = def;

            // 検索
            if (IntPtr.Size == 4)
            {
                Searcher = new Searcher_x86(condition);
            }
            else
            {
                Searcher = new Searcher(condition);
            }
            IsSearchedAll = Searcher.ExecSearch(limit);

            // 最近使ったスキル更新
            UpdateRecentSkill(condition.Skills);

            return Searcher.ResultSets;
        }

        // 条件そのまま追加検索
        public List<EquipSet> SearchMore(int limit)
        {
            // まだ検索がされていない場合、0件で返す
            if(Searcher == null)
            {
                return new List<EquipSet>();
            }

            IsSearchedAll = Searcher.ExecSearch(limit);

            return Searcher.ResultSets;
        }

        // 追加スキル検索
        public List<Skill> SearchExtraSkill()
        {
            // まだ検索がされていない場合、0件で返す
            if (Searcher == null)
            {
                return new List<Skill>();
            }

            List<Skill> exSkills = new();

            // 全スキル全レベルを走査
            Parallel.ForEach(Masters.Skills,
                new ParallelOptions { MaxDegreeOfParallelism = LogicConfig.Instance.MaxDegreeOfParallelism },
                () => new List<Skill>(),
                (skill, loop, subResult) =>
                {
                    for (int i = 1; i <= skill.Level; i++)
                    {
                        // 現在の検索条件をコピー
                        SearchCondition condition = new(Searcher.Condition);

                        // スキルを検索条件に追加
                        Skill exSkill = new(skill.Name, i);
                        bool isNewSkill = condition.AddSkill(new Skill(skill.Name, i));

                        // 新規スキルor既存だが上位Lvのスキルの場合のみ検索を実行
                        if (isNewSkill)
                        {
                            // 頑張り度1で検索
                            ISearcher exSearcher;
                            if (IntPtr.Size == 4)
                            {
                                exSearcher = new Searcher_x86(condition);
                            }
                            else
                            {
                                exSearcher = new Searcher(condition);
                            }
                            exSearcher.ExecSearch(1);

                            // 1件でもヒットすれば追加スキル一覧に追加
                            if (exSearcher.ResultSets.Count > 0)
                            {
                                subResult.Add(exSkill);
                            }
                        }
                    }
                    return subResult;
                },
                (finalResult) =>
                {
                    lock (exSkills)
                    {
                        exSkills.AddRange(finalResult);
                    }
                }
            );

            // skill.csv順にソート
            List<Skill> sortedSkills = new();
            foreach (var skill in Masters.Skills)
            {
                foreach (var result in exSkills)
                {
                    if (skill.Name == result.Name)
                    {
                        sortedSkills.Add(result);
                    }
                }
            }

            return sortedSkills;
        }

        // 除外装備登録
        public Clude? AddExclude(string name)
        {
            return DataManagement.AddExclude(name);
        }

        // 固定装備登録
        public Clude? AddInclude(string name)
        {
            return DataManagement.AddInclude(name);
        }

        // 除外・固定解除
        public void DeleteClude(string name)
        {
            DataManagement.DeleteClude(name);
        }

        // 除外・固定全解除
        public void DeleteAllClude()
        {
            DataManagement.DeleteAllClude();
        }

        // 錬成防具を全除外
        public void ExcludeAllAugmentation()
        {
            DataManagement.ExcludeAllAugmentation();
        }

        // 護石追加
        public Equipment AddCharm(List<Skill> skill, int slot1, int slot2, int slot3)
        {
            return DataManagement.AddCharm(skill, slot1, slot2, slot3);
        }

        // 護石削除
        public void DeleteCharm(string guid)
        {
            DataManagement.DeleteCharm(guid);
        }

        // マイセット登録
        public EquipSet AddMySet(EquipSet set)
        {
            return DataManagement.AddMySet(set);
        }

        // マイセット削除
        public void DeleteMySet(EquipSet set)
        {
            DataManagement.DeleteMySet(set);
        }

        // マイセット更新
        public void SaveMySet()
        {
            DataManagement.SaveMySet();
        }

        // マイセット再読み込み
        public void LoadMySet()
        {
            DataManagement.LoadMySet();
        }

        // 最近使ったスキル更新
        public void UpdateRecentSkill(List<Skill> skills)
        {
            DataManagement.UpdateRecentSkill(skills);
        }

        // 錬成装備登録
        public void AddAugmentation(Augmentation aug)
        {
            DataManagement.AddAugmentation(aug);
        }

        // 錬成装備削除
        public void DeleteAugmentation(Augmentation aug)
        {
            DataManagement.DeleteAugmentation(aug);
        }

        // 錬成装備更新
        public void UpdateAugmentation(Augmentation aug)
        {
            DataManagement.UpdateAugmentation(aug);
        }

        // 装備マスタリロード
        public void RefreshEquipmentMasters()
        {
            Masters.RefreshEquipmentMasters();
        }
    }
}
