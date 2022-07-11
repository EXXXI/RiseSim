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
using SimModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimModel.Domain
{
    static internal class DataManagement
    {
        // 最近使ったスキルの記憶容量
        static public int MaxRecentSkillCount { get; } = LogicConfig.Instance.MaxRecentSkillCount;

        // 除外設定を追加
        static internal Clude? AddExclude(string name)
        {
            Equipment? equip = Masters.GetEquipByName(name);
            if (equip == null)
            {
                return null;
            }
            return AddClude(name, CludeKind.exclude);
        }

        // 固定設定を追加
        static internal Clude? AddInclude(string name)
        {
            Equipment? equip = Masters.GetEquipByName(name);
            if (equip == null || equip.Kind == EquipKind.deco)
            {
                // 装飾品は固定しない
                return null;
            }

            // 同じ装備種類の固定装備があった場合、固定を解除する
            string? toDelete = null;
            foreach (var clude in Masters.Cludes)
            {
                if (clude.Kind == CludeKind.exclude)
                {
                    continue;
                }

                Equipment? oldEquip = Masters.GetEquipByName(clude.Name);
                if (oldEquip == null || oldEquip.Kind.Equals(equip.Kind))
                {
                    toDelete = clude.Name;
                }
            }
            if(toDelete != null)
            {
                DeleteClude(toDelete);
            }

            // 追加
            return AddClude(name, CludeKind.include);
        }

        // 除外・固定の追加
        static private Clude? AddClude(string name, CludeKind kind)
        {
            Clude? ret = null;

            bool existClude = false;
            foreach (var clude in Masters.Cludes)
            {
                if (clude.Name.Equals(name))
                {
                    // 既に設定がある場合は上書き
                    clude.Kind = kind;
                    existClude = true;
                    ret = clude;
                }
            }
            if (!existClude)
            {
                // 設定がない場合は新規作成
                Clude clude = new();
                clude.Name = name;
                clude.Kind = kind;
                // 追加
                Masters.Cludes.Add(clude);
                ret = clude;
            }

            // マスタへ反映
            CsvOperation.SaveCludeCSV();

            // 追加した設定
            return ret;
        }

        // 除外・固定設定の削除
        static internal void DeleteClude(string name)
        {
            foreach (var clude in Masters.Cludes)
            {
                if (clude.Name.Equals(name))
                {
                    // 削除
                    Masters.Cludes.Remove(clude);
                    break;
                }
            }

            // マスタへ反映
            CsvOperation.SaveCludeCSV();
        }

        // 護石の追加
        static internal Equipment AddCharm(List<Skill> skills, int slot1, int slot2, int slot3)
        {
            Equipment equipment = new(EquipKind.charm);
            equipment.Name = Guid.NewGuid().ToString();
            equipment.Slot1 = slot1;
            equipment.Slot2 = slot2;
            equipment.Slot3 = slot3;
            equipment.Skills = skills;

            // 追加
            Masters.Charms.Add(equipment);

            // マスタへ反映
            CsvOperation.SaveCharmCSV();

            // 追加した護石
            return equipment;
        }

        // 護石の削除
        static internal void DeleteCharm(string guid)
        {
            foreach (var charm in Masters.Charms)
            {
                if (charm.Name.Equals(guid))
                {
                    // 削除
                    Masters.Charms.Remove(charm);
                    break;
                }
            }

            // マスタへ反映
            CsvOperation.SaveCharmCSV();
        }

        // マイセットの追加
        static internal EquipSet AddMySet(EquipSet set)
        {
            // 追加
            Masters.MySets.Add(set);

            // マスタへ反映
            CsvOperation.SaveMySetCSV();

            return set;
        }

        // マイセットの削除
        static internal void DeleteMySet(EquipSet set)
        {
            // 削除
            Masters.MySets.Remove(set);

            // マスタへ反映
            CsvOperation.SaveMySetCSV();
        }

        // マイセットの変更を保存
        static internal void SaveMySet()
        {
            // マスタへ反映
            CsvOperation.SaveMySetCSV();
        }

        // 最近使ったスキルの更新
        internal static void UpdateRecentSkill(List<Skill> skills)
        {
            List<string> newNames = new List<string>();

            // 今回の検索条件をリストに追加
            foreach (var skill in skills)
            {
                newNames.Add(skill.Name);
            }

            // 今までの検索条件をリストに追加
            foreach (var oldName in Masters.RecentSkillNames)
            {
                bool isDuplicate = false;
                foreach (var newName in newNames)
                {
                    if (newName.Equals(oldName))
                    {
                        isDuplicate = true;
                        break;
                    }
                }
                if (!isDuplicate)
                {
                    newNames.Add(oldName);
                }

                // 最大数に達したらそこで終了
                if (MaxRecentSkillCount <= newNames.Count)
                {
                    break;
                }
            }

            // 新しいリストに入れ替え
            Masters.RecentSkillNames = newNames;

            // マスタへ反映
            CsvOperation.SaveRecentSkillCSV();
        }
    }
}
