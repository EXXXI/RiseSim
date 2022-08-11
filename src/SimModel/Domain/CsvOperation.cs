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
using Csv;
using SimModel.Model;
using SimModel.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimModel.Domain
{
    static internal class CsvOperation
    {
        // 定数：ファイルパス
        private const string SkillCsv = "MHR_SKILL.csv";
        private const string HeadCsv = "MHR_EQUIP_HEAD.csv";
        private const string BodyCsv = "MHR_EQUIP_BODY.csv";
        private const string ArmCsv = "MHR_EQUIP_ARM.csv";
        private const string WaistCsv = "MHR_EQUIP_WST.csv";
        private const string LegCsv = "MHR_EQUIP_LEG.csv";
        private const string DecoCsv = "MHR_DECO.csv";
        private const string CludeCsv = "save/clude.csv";
        private const string CharmCsv = "save/charm.csv";
        private const string MySetCsv = "save/myset.csv";
        private const string RecentSkillCsv = "save/recentSkill.csv";
        private const string AugmentationCsv = "save/augmentation.csv";

        // スキルマスタ読み込み
        static internal void LoadSkillCSV()
        {
            Masters.Skills = new();

            string csv = ReadAllText(SkillCsv);

            foreach (ICsvLine line in CsvReader.ReadFromText(csv))
            {
                string skillName = line[@"スキル系統"];
                int skillLevel = Parse(line[@"必要ポイント"]);
                AddSkill(skillName, skillLevel);
            }
        }

        // 頭防具マスタ読み込み
        static internal void LoadHeadCSV()
        {
            Masters.OriginalHeads = new();
            LoadEquipCSV(HeadCsv, Masters.OriginalHeads, EquipKind.head);
        }

        // 胴防具マスタ読み込み
        static internal void LoadBodyCSV()
        {
            Masters.OriginalBodys = new();
            LoadEquipCSV(BodyCsv, Masters.OriginalBodys, EquipKind.body);
        }

        // 腕防具マスタ読み込み
        static internal void LoadArmCSV()
        {
            Masters.OriginalArms = new();
            LoadEquipCSV(ArmCsv, Masters.OriginalArms, EquipKind.arm);
        }

        // 腰防具マスタ読み込み
        static internal void LoadWaistCSV()
        {
            Masters.OriginalWaists = new();
            LoadEquipCSV(WaistCsv, Masters.OriginalWaists, EquipKind.waist);
        }

        // 足防具マスタ読み込み
        static internal void LoadLegCSV()
        {
            Masters.OriginalLegs = new();
            LoadEquipCSV(LegCsv, Masters.OriginalLegs, EquipKind.leg);
        }

        // 防具マスタ読み込み
        static private void LoadEquipCSV(string fileName, List<Equipment> equipments, EquipKind kind)
        {

            string csv = ReadAllText(fileName);
            var x = CsvReader.ReadFromText(csv);
            foreach (ICsvLine line in x)
            {
                Equipment equip = new Equipment(kind);
                equip.Name = line[@"名前"];
                equip.Sex = (Sex)Parse(line[@"性別(0=両,1=男,2=女)"]);
                equip.Rare = Parse(line[@"レア度"]);
                equip.Slot1 = Parse(line[@"スロット1"]);
                equip.Slot2 = Parse(line[@"スロット2"]);
                equip.Slot3 = Parse(line[@"スロット3"]);
                equip.Mindef = Parse(line[@"初期防御力"]);
                equip.Maxdef = Parse(line[@"最終防御力"], equip.Mindef); // 読み込みに失敗した場合は初期防御力と同値とみなす
                equip.Fire = Parse(line[@"火耐性"]);
                equip.Water = Parse(line[@"水耐性"]);
                equip.Thunder = Parse(line[@"雷耐性"]);
                equip.Ice = Parse(line[@"氷耐性"]);
                equip.Dragon = Parse(line[@"龍耐性"]);
                List<Skill> skills = new List<Skill>();
                for (int i = 1; i <= LogicConfig.Instance.MaxEquipSkillCount; i++)
                {
                    string skill = line[@"スキル系統" + i];
                    string level = line[@"スキル値" + i];
                    if (string.IsNullOrWhiteSpace(skill))
                    {
                        break;
                    }
                    skills.Add(new Skill(skill, Parse(level)));
                }
                equip.Skills = skills; 

                equipments.Add(equip);
            }
        }

        // 装飾品マスタ読み込み
        static internal void LoadDecoCSV()
        {
            Masters.Decos = new();

            string csv = ReadAllText(DecoCsv);

            foreach (ICsvLine line in CsvReader.ReadFromText(csv))
            {
                Equipment equip = new Equipment(EquipKind.deco);
                equip.Name = line[@"名前"];
                equip.Sex = Sex.all;
                equip.Rare = Parse(line[@"レア度"]);
                equip.Slot1 = Parse(line[@"スロットサイズ"]);
                equip.Slot2 = 0;
                equip.Slot3 = 0;
                equip.Mindef = 0;
                equip.Maxdef = 0;
                equip.Fire = 0;
                equip.Water = 0;
                equip.Thunder = 0;
                equip.Ice = 0;
                equip.Dragon = 0;
                List<Skill> skills = new List<Skill>();
                for (int i = 1; i <= LogicConfig.Instance.MaxDecoSkillCount; i++)
                {
                    string skill = line[@"スキル系統" + i];
                    string level = line[@"スキル値" + i];
                    if (string.IsNullOrWhiteSpace(skill))
                    {
                        break;
                    }
                    skills.Add(new Skill(skill, Parse(level)));
                }
                equip.Skills = skills;

                Masters.Decos.Add(equip);
            }
        }

        // 除外固定マスタ書き込み
        static internal void SaveCludeCSV()
        {
            List<string[]> body = new List<string[]>();
            foreach (var clude in Masters.Cludes)
            {
                string kind = "0";
                if (clude.Kind.Equals(CludeKind.include))
                {
                    kind = "1";
                }
                body.Add(new string[] { clude.Name, kind });
            }

            string export = CsvWriter.WriteToText(new string[] { "対象", "種別" }, body);
            File.WriteAllText(CludeCsv, export);

        }

        // 除外固定マスタ読み込み
        static internal void LoadCludeCSV()
        {
            Masters.Cludes = new();

            string csv = ReadAllText(CludeCsv);

            foreach (ICsvLine line in CsvReader.ReadFromText(csv))
            {
                Clude clude = new Clude
                {
                    Name = line[@"対象"],
                    Kind = (CludeKind)Parse(line[@"種別"])
                };

                Masters.Cludes.Add(clude);
            }
        }

        // 護石マスタ書き込み
        static internal void SaveCharmCSV()
        {
            List<string[]> body = new List<string[]>();
            foreach (var charm in Masters.Charms)
            {
                List<string> bodyStrings = new List<string>();
                for (int i = 0; i < LogicConfig.Instance.MaxCharmSkillCount; i++)
                {
                    bodyStrings.Add(charm.Skills.Count > i ? charm.Skills[i].Name : string.Empty);
                    bodyStrings.Add(charm.Skills.Count > i ? charm.Skills[i].Level.ToString() : string.Empty);
                }
                bodyStrings.Add(charm.Slot1.ToString());
                bodyStrings.Add(charm.Slot2.ToString());
                bodyStrings.Add(charm.Slot3.ToString());
                bodyStrings.Add(charm.Name);
                body.Add(bodyStrings.ToArray());
            }
            List<string> headStrings = new List<string>();
            for (int i = 1; i <= LogicConfig.Instance.MaxCharmSkillCount; i++)
            {
                headStrings.Add("スキル系統" + i);
                headStrings.Add("スキル値" + i);
            }
            headStrings.Add("スロット1");
            headStrings.Add("スロット2");
            headStrings.Add("スロット3");
            headStrings.Add("内部管理ID");
            string[] header = headStrings.ToArray();
            string export = CsvWriter.WriteToText(header, body);
            File.WriteAllText(CharmCsv, export);
        }

        // 護石マスタ読み込み
        static internal void LoadCharmCSV()
        {
            Masters.Charms = new();

            string csv = ReadAllText(CharmCsv);

            foreach (ICsvLine line in CsvReader.ReadFromText(csv))
            {
                Equipment charm = new Equipment(EquipKind.charm);
                charm.Slot1 = Parse(line[@"スロット1"]);
                charm.Slot2 = Parse(line[@"スロット2"]);
                charm.Slot3 = Parse(line[@"スロット3"]);
                charm.Skills = new List<Skill>();
                for (int i = 1; i <= LogicConfig.Instance.MaxCharmSkillCount; i++)
                {
                    Skill skill = new Skill(line[@"スキル系統" + i], Parse(line[@"スキル値" + i]));
                    if (!string.IsNullOrWhiteSpace(skill.Name))
                    {
                        charm.Skills.Add(skill);
                    }
                }

                // 内部管理IDがない場合は付与する
                try
                {
                    charm.Name = line[@"内部管理ID"];
                }
                catch (InvalidOperationException)
                {
                    charm.Name = Guid.NewGuid().ToString();
                }

                Masters.Charms.Add(charm);
            }

            // 内部管理IDを保存するため、書き込みを実施
            SaveCharmCSV();
        }

        // マイセットマスタ書き込み
        static internal void SaveMySetCSV()
        {
            List<string[]> body = new List<string[]>();
            foreach (var set in Masters.MySets)
            {
                string weaponSlot1 = set.WeaponSlot1.ToString();
                string weaponSlot2 = set.WeaponSlot2.ToString();
                string weaponSlot3 = set.WeaponSlot3.ToString();
                body.Add(new string[] { weaponSlot1, weaponSlot2, weaponSlot3, set.Head.Name, set.Body.Name, set.Arm.Name, set.Waist.Name, set.Leg.Name, set.Charm.Name, set.DecoNameCSV, set.Name });
            }
            string[] header = new string[] { "武器スロ1", "武器スロ2", "武器スロ3", "頭", "胴", "腕", "腰", "足", "護石", "装飾品", "名前" };
            string export = CsvWriter.WriteToText(header, body);
            File.WriteAllText(MySetCsv, export);
        }

        // マイセットマスタ読み込み
        static internal void LoadMySetCSV()
        {
            Masters.MySets = new();

            string csv = ReadAllText(MySetCsv);

            foreach (ICsvLine line in CsvReader.ReadFromText(csv))
            {
                EquipSet set = new EquipSet();
                set.WeaponSlot1 = Parse(line[@"武器スロ1"]);
                set.WeaponSlot2 = Parse(line[@"武器スロ2"]);
                set.WeaponSlot3 = Parse(line[@"武器スロ3"]);
                set.Head = Masters.GetEquipByName(line[@"頭"]);
                set.Body = Masters.GetEquipByName(line[@"胴"]);
                set.Arm = Masters.GetEquipByName(line[@"腕"]);
                set.Waist = Masters.GetEquipByName(line[@"腰"]);
                set.Leg = Masters.GetEquipByName(line[@"足"]);
                set.Charm = Masters.GetEquipByName(line[@"護石"]);
                set.DecoNameCSV = line[@"装飾品"];
                // 前バージョンとの互換性のため存在確認
                // TODO: 次回作でこのシミュが使えそうならその時は消そう
                if (line.Headers.Contains(@"名前"))
                {
                    set.Name = line[@"名前"];
                }
                else
                {
                    set.Name = LogicConfig.Instance.DefaultMySetName;
                }
                Masters.MySets.Add(set);
            }
        }

        // 最近使ったスキル書き込み
        internal static void SaveRecentSkillCSV()
        {
            List<string[]> body = new List<string[]>();
            foreach (var name in Masters.RecentSkillNames)
            {
                body.Add(new string[] { name });
            }
            string[] header = new string[] { "スキル名" };
            string export = CsvWriter.WriteToText(header, body);
            File.WriteAllText(RecentSkillCsv, export);
        }

        // 最近使ったスキル読み込み
        internal static void LoadRecentSkillCSV()
        {
            Masters.RecentSkillNames = new();

            string csv = ReadAllText(RecentSkillCsv);

            foreach (ICsvLine line in CsvReader.ReadFromText(csv))
            {
                Masters.RecentSkillNames.Add(line[@"スキル名"]);
            }
        }

        // 錬成装備マスタ読み込み
        static internal void LoadAugmentationCSV()
        {
            Masters.Augmentations = new();

            string csv = ReadAllText(AugmentationCsv);
            var x = CsvReader.ReadFromText(csv);
            foreach (ICsvLine line in x)
            {
                Augmentation aug = new Augmentation();
                aug.DispName = line[@"名前"];
                aug.BaseName = line[@"ベース装備"];
                aug.Kind = ToEquipKind(line[@"種類"]);
                aug.Slot1 = Parse(line[@"スロット1"]);
                aug.Slot2 = Parse(line[@"スロット2"]);
                aug.Slot3 = Parse(line[@"スロット3"]);
                aug.Def = Parse(line[@"防御力増減"]);
                aug.Fire = Parse(line[@"火耐性増減"]);
                aug.Water = Parse(line[@"水耐性増減"]);
                aug.Thunder = Parse(line[@"雷耐性増減"]);
                aug.Ice = Parse(line[@"氷耐性増減"]);
                aug.Dragon = Parse(line[@"龍耐性増減"]);
                List<Skill> skills = new List<Skill>();
                for (int i = 1; i <= LogicConfig.Instance.MaxAugmentationSkillCount; i++)
                {
                    string skill = line[@"スキル系統" + i];
                    string level = line[@"スキル値" + i];
                    if (string.IsNullOrWhiteSpace(skill))
                    {
                        break;
                    }
                    skills.Add(new Skill(skill, Parse(level), true));
                }
                aug.Skills = skills;
                aug.Name = line[@"管理用ID"];

                Masters.Augmentations.Add(aug);
            }
        }

        // 錬成装備マスタ書き込み
        static internal void SaveAugmentationCSV()
        {
            List<string[]> body = new List<string[]>();
            foreach (var aug in Masters.Augmentations)
            {
                List<string> bodyStrings = new List<string>();
                bodyStrings.Add(aug.DispName ?? string.Empty);
                bodyStrings.Add(aug.BaseName);
                bodyStrings.Add(aug.Kind.Str());
                bodyStrings.Add(aug.Slot1.ToString());
                bodyStrings.Add(aug.Slot2.ToString());
                bodyStrings.Add(aug.Slot3.ToString());
                bodyStrings.Add(aug.Def.ToString());
                bodyStrings.Add(aug.Fire.ToString());
                bodyStrings.Add(aug.Water.ToString());
                bodyStrings.Add(aug.Thunder.ToString());
                bodyStrings.Add(aug.Ice.ToString());
                bodyStrings.Add(aug.Dragon.ToString());
                for (int i = 0; i < LogicConfig.Instance.MaxAugmentationSkillCount; i++)
                {
                    bodyStrings.Add(aug.Skills.Count > i ? aug.Skills[i].Name : string.Empty);
                    bodyStrings.Add(aug.Skills.Count > i ? aug.Skills[i].Level.ToString() : string.Empty);
                }
                bodyStrings.Add(aug.Name);
                body.Add(bodyStrings.ToArray());
            }

            string[] header = new string[] { "名前", "ベース装備", "種類", "スロット1", "スロット2", "スロット3", "防御力増減", "火耐性増減", "水耐性増減", "雷耐性増減", "氷耐性増減", "龍耐性増減", "スキル系統1", "スキル値1", "スキル系統2", "スキル値2", "スキル系統3", "スキル値3", "スキル系統4", "スキル値4", "管理用ID" };
            string export = CsvWriter.WriteToText(header, body);
            File.WriteAllText(AugmentationCsv, export);
        }

        // ファイル読み込み
        static private string ReadAllText(string fileName)
        {
            string csv = File.ReadAllText(fileName);

            // ライブラリの仕様に合わせてヘッダーを修正
            // ヘッダー行はコメントアウトしない
            if (csv.StartsWith('#'))
            {
                csv = csv.Substring(1);
            }
            // 同名のヘッダーは利用不可なので小細工
            csv = csv.Replace("生産素材1,個数", "生産素材1,生産素材個数1");
            csv = csv.Replace("生産素材2,個数", "生産素材2,生産素材個数2");
            csv = csv.Replace("生産素材3,個数", "生産素材3,生産素材個数3");
            csv = csv.Replace("生産素材4,個数", "生産素材4,生産素材個数4");

            return csv;
        }

        // スキルマスタへのスキルの追加
        static private void AddSkill(string skillName, int skillLevel)
        {
            foreach (var skill in Masters.Skills)
            {
                if (skill.Name.Equals(skillName))
                {
                    // 同名スキルはスキルレベルが高いものだけを残す(マスタには最大レベルを保持する)
                    if (skill.Level < skillLevel)
                    {
                        skill.Level = skillLevel;
                    }
                    return;
                }
            }
            Masters.Skills.Add(new Skill(skillName, skillLevel));
        }

        // int.Parseを実行
        // 失敗した場合は0として扱う
        static private int Parse(string str)
        {
            return Parse(str, 0);
        }
        // int.Parseを実行
        // 失敗した場合は指定したデフォルト値として扱う
        static private int Parse(string str, int def)
        {
            if (int.TryParse(str, out int num))
            {
                return num;
            }
            else
            {
                return def;
            }       
        }

        // TODO: 別の場所に定義したい
        // 文字列をEquipKindに変換
        static private EquipKind ToEquipKind(string str)
        {
            switch (str)
            {
                case "頭":
                    return EquipKind.head;
                case "胴":
                    return EquipKind.body;
                case "腕":
                    return EquipKind.arm;
                case "腰":
                    return EquipKind.waist;
                case "足":
                    return EquipKind.leg;
                case "脚":
                    // 誤記
                    return EquipKind.leg;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
