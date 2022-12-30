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
            if (equip.Ideal != null)
            {
                equip = equip.BaseEquipment;
            }
            if (equip == null)
            {
                return null;
            }
            return AddClude(equip.Name, CludeKind.exclude);
        }

        // 固定設定を追加
        static internal Clude? AddInclude(string name)
        {
            Equipment? equip = Masters.GetEquipByName(name);
            if (equip.Ideal != null)
            {
                equip = equip.BaseEquipment;
            }
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

                Equipment? oldEquip = Masters.GetEquipByName(clude.Name, false);
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
            return AddClude(equip.Name, CludeKind.include);
        }

        // 錬成防具を全て除外設定に追加
        static internal bool ExcludeAllAugmentation()
        {
            foreach (var aug in Masters.Augmentations)
            {
                string name = aug.Name;
                Equipment? equip = Masters.GetEquipByName(name, false);
                if (equip == null)
                {
                    return false;
                }

                AddClude(name, CludeKind.exclude);
            }
            return true;
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

        // 除外・固定設定の全削除
        static internal void DeleteAllClude()
        {
            Masters.Cludes.Clear();

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

        // マイセットを再読み込み
        static internal void LoadMySet()
        {
            // マスタへ反映
            CsvOperation.LoadMySetCSV();
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


        // 錬成装備の追加
        static internal void AddAugmentation(Augmentation aug)
        {
            // 追加
            Masters.Augmentations.Add(aug);

            // マスタへ反映
            CsvOperation.SaveAugmentationCSV();
        }

        // 錬成装備の削除
        static internal void DeleteAugmentation(Augmentation aug)
        {
            // 削除
            Masters.Augmentations.Remove(aug);

            // マスタへ反映
            CsvOperation.SaveAugmentationCSV();
        }

        // 錬成装備の更新
        internal static void UpdateAugmentation(Augmentation newAugData)
        {
            foreach (var aug in Masters.Augmentations)
            {
                if (aug.Name == newAugData.Name)
                {
                    aug.DispName = newAugData.DispName;
                    aug.Kind = newAugData.Kind;
                    aug.BaseName = newAugData.BaseName;
                    aug.Slot1 = newAugData.Slot1;
                    aug.Slot2 = newAugData.Slot2;
                    aug.Slot3 = newAugData.Slot3;
                    aug.Def = newAugData.Def;
                    aug.Fire = newAugData.Fire;
                    aug.Water = newAugData.Water;
                    aug.Thunder = newAugData.Thunder;
                    aug.Ice = newAugData.Ice;
                    aug.Dragon = newAugData.Dragon;
                    aug.Skills = newAugData.Skills;

                    // マスタへ反映
                    CsvOperation.SaveAugmentationCSV();

                    return;
                }
            }

            // 万一更新先が見つからなかった場合は新規登録
            AddAugmentation(newAugData);
        }

        // 理想錬成の追加
        internal static void AddIdealAugmentation(IdealAugmentation ideal)
        {
            // 追加
            Masters.Ideals.Add(ideal);

            // マスタへ反映
            CsvOperation.SaveIdealCSV();
        }

        // 理想錬成の削除
        internal static void DeleteIdealAugmentation(IdealAugmentation ideal)
        {
            // 削除
            Masters.Ideals.Remove(ideal);

            // マスタへ反映
            CsvOperation.SaveIdealCSV();
        }

        // 理想錬成の更新
        internal static void UpdateIdealAugmentation(IdealAugmentation newIdeal)
        {
            foreach (var ideal in Masters.Ideals)
            {
                if (ideal.Name == newIdeal.Name)
                {
                    ideal.Table = newIdeal.Table;
                    ideal.DispName = newIdeal.DispName;
                    ideal.SlotIncrement = newIdeal.SlotIncrement;
                    ideal.GenericSkills[0] = newIdeal.GenericSkills[0];
                    ideal.GenericSkills[1] = newIdeal.GenericSkills[1];
                    ideal.GenericSkills[2] = newIdeal.GenericSkills[2];
                    ideal.GenericSkills[3] = newIdeal.GenericSkills[3];
                    ideal.GenericSkills[4] = newIdeal.GenericSkills[4];
                    ideal.Skills = newIdeal.Skills;
                    ideal.SkillMinuses = newIdeal.SkillMinuses;
                    ideal.IsOne = newIdeal.IsOne;

                    // マスタへ反映
                    CsvOperation.SaveIdealCSV();

                    return;
                }
            }

            // 万一更新先が見つからなかった場合は新規登録
            AddIdealAugmentation(newIdeal);
        }
    }
}
