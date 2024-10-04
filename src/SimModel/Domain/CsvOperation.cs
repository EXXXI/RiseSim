using Csv;
using NLog;
using SimModel.Config;
using SimModel.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace SimModel.Domain
{
    /// <summary>
    /// CSV操作クラス
    /// </summary>
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
        private const string IdealCsv = "save/ideal.csv";
        private const string ConditionCsv = "save/condition.csv";

        private const string SkillMasterHeaderName = @"スキル系統";
        private const string SkillMasterHeaderRequiredPoints = @"必要ポイント";
        private const string SkillMasterHeaderCategory = @"カテゴリ";
        private const string SkillMasterHeaderCost = @"コスト";
        private const string SkillMasterHeaderSpecificName = @"発動スキル";

        static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// スキルマスタ読み込み
        /// </summary>
        static internal void LoadSkillCSV()
        {
            string csv = ReadAllText(SkillCsv);

            Masters.Skills = CsvReader.ReadFromText(csv)
                .Select(line => new
                {
                    Name = line[SkillMasterHeaderName],
                    Level = ParseUtil.Parse(line[SkillMasterHeaderRequiredPoints]),
                    Category = line[SkillMasterHeaderCategory]
                })
                // マスタのCSVにある同名スキルのうち、スキルレベルが最大のものだけを選ぶ
                .GroupBy(x => new { x.Name, x.Category })
                .Select(group => new Skill(group.Key.Name, group.Max(x => x.Level), group.Key.Category))
                .ToList();

            // 特殊な名称のデータを保持
            var hasSpecificNames = CsvReader.ReadFromText(csv)
                .Select(line => new
                {
                    Name = line[SkillMasterHeaderName],
                    Level = ParseUtil.Parse(line[SkillMasterHeaderRequiredPoints]),
                    Specific = line[SkillMasterHeaderSpecificName]
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.Specific));
            foreach (var item in hasSpecificNames)
            {
                Skill skill = Masters.Skills.First(s => s.Name == item.Name);
                skill.SpecificNames.Add(item.Level, item.Specific);
            }

            // 理想錬成用スキル
            foreach (ICsvLine line in CsvReader.ReadFromText(csv))
            {
                // 既に登録済みの場合パス
                bool isExist = false;
                foreach (var gSkill in Masters.GenericSkills)
                {
                    if (gSkill.Skills[0].Name == line[SkillMasterHeaderName])
                    {
                        isExist = true;
                        break;
                    }
                }
                if (isExist)
                {
                    continue;
                }
                Equipment equip = new Equipment();
                equip.Kind = EquipKind.gskill;
                equip.Name = line[SkillMasterHeaderName] + "(c" + line[SkillMasterHeaderCost] + ")";
                equip.DispName = equip.Name;
                equip.Skills.Add(new Skill(line[SkillMasterHeaderName], 1));
                switch (ParseUtil.Parse(line[SkillMasterHeaderCost]))
                {
                    case 3:
                        equip.GenericSkills[0] = 1;
                        break;
                    case 6:
                        equip.GenericSkills[1] = 1;
                        break;
                    case 9:
                        equip.GenericSkills[2] = 1;
                        break;
                    case 12:
                        equip.GenericSkills[3] = 1;
                        break;
                    case 15:
                        equip.GenericSkills[4] = 1;
                        break;
                    default:
                        continue;
                }
                Masters.GenericSkills.Add(equip);
            }
        }

        /// <summary>
        /// 頭防具マスタ読み込み
        /// </summary>
        static internal void LoadHeadCSV()
        {
            Masters.OriginalHeads = new();
            LoadEquipCSV(HeadCsv, Masters.OriginalHeads, EquipKind.head);
            Masters.Heads = Masters.OriginalHeads;
        }

        /// <summary>
        /// 胴防具マスタ読み込み
        /// </summary>
        static internal void LoadBodyCSV()
        {
            Masters.OriginalBodys = new();
            LoadEquipCSV(BodyCsv, Masters.OriginalBodys, EquipKind.body);
            Masters.Bodys = Masters.OriginalBodys;
        }

        /// <summary>
        /// 腕防具マスタ読み込み
        /// </summary>
        static internal void LoadArmCSV()
        {
            Masters.OriginalArms = new();
            LoadEquipCSV(ArmCsv, Masters.OriginalArms, EquipKind.arm);
            Masters.Arms = Masters.OriginalArms;
        }

        /// <summary>
        /// 腰防具マスタ読み込み
        /// </summary>
        static internal void LoadWaistCSV()
        {
            Masters.OriginalWaists = new();
            LoadEquipCSV(WaistCsv, Masters.OriginalWaists, EquipKind.waist);
            Masters.Waists = Masters.OriginalWaists;
        }

        /// <summary>
        /// 足防具マスタ読み込み
        /// </summary>
        static internal void LoadLegCSV()
        {
            Masters.OriginalLegs = new();
            LoadEquipCSV(LegCsv, Masters.OriginalLegs, EquipKind.leg);
            Masters.Legs = Masters.OriginalLegs;
        }

        /// <summary>
        /// 防具マスタ読み込み
        /// </summary>
        /// <param name="fileName">CSVファイル名</param>
        /// <param name="equipments">格納先</param>
        /// <param name="kind">部位</param>
        static private void LoadEquipCSV(string fileName, List<Equipment> equipments, EquipKind kind)
        {

            string csv = ReadAllText(fileName);
            var x = CsvReader.ReadFromText(csv);
            foreach (ICsvLine line in x)
            {
                Equipment equip = new Equipment(kind);
                equip.Name = line[@"名前"];
                equip.Sex = (Sex)ParseUtil.Parse(line[@"性別(0=両,1=男,2=女)"]);
                equip.Rare = ParseUtil.Parse(line[@"レア度"]);
                equip.Slot1 = ParseUtil.Parse(line[@"スロット1"]);
                equip.Slot2 = ParseUtil.Parse(line[@"スロット2"]);
                equip.Slot3 = ParseUtil.Parse(line[@"スロット3"]);
                equip.Mindef = ParseUtil.Parse(line[@"初期防御力"]);
                equip.Maxdef = ParseUtil.Parse(line[@"最終防御力"], equip.Mindef); // 読み込みに失敗した場合は初期防御力と同値とみなす
                equip.Fire = ParseUtil.Parse(line[@"火耐性"]);
                equip.Water = ParseUtil.Parse(line[@"水耐性"]);
                equip.Thunder = ParseUtil.Parse(line[@"雷耐性"]);
                equip.Ice = ParseUtil.Parse(line[@"氷耐性"]);
                equip.Dragon = ParseUtil.Parse(line[@"龍耐性"]);
                equip.RowNo = ParseUtil.Parse(line[@"仮番号"], int.MaxValue);
                List<Skill> skills = new List<Skill>();
                for (int i = 1; i <= LogicConfig.Instance.MaxEquipSkillCount; i++)
                {
                    string skill = line[@"スキル系統" + i];
                    string level = line[@"スキル値" + i];
                    if (string.IsNullOrWhiteSpace(skill))
                    {
                        break;
                    }
                    skills.Add(new Skill(skill, ParseUtil.Parse(level)));
                }
                equip.Skills = skills;
                equip.AugmentationTable = ParseUtil.Parse(line[@"錬成テーブル"]);

                equipments.Add(equip);
            }
        }

        /// <summary>
        /// 装飾品マスタ読み込み
        /// </summary>
        static internal void LoadDecoCSV()
        {
            Masters.Decos = new();

            string csv = ReadAllText(DecoCsv);

            foreach (ICsvLine line in CsvReader.ReadFromText(csv))
            {
                Equipment equip = new Equipment(EquipKind.deco);
                equip.Name = line[@"名前"];
                equip.Sex = Sex.all;
                equip.Rare = ParseUtil.Parse(line[@"レア度"]);
                equip.Slot1 = ParseUtil.Parse(line[@"スロットサイズ"]);
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
                    skills.Add(new Skill(skill, ParseUtil.Parse(level)));
                }
                equip.Skills = skills;

                Masters.Decos.Add(equip);
            }
        }

        /// <summary>
        /// 除外固定マスタ書き込み
        /// </summary>
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

        /// <summary>
        /// 除外固定マスタ読み込み
        /// </summary>
        static internal void LoadCludeCSV()
        {
            Masters.Cludes = new();

            string csv = ReadAllText(CludeCsv);

            foreach (ICsvLine line in CsvReader.ReadFromText(csv))
            {
                Clude clude = new Clude
                {
                    Name = line[@"対象"],
                    Kind = (CludeKind)ParseUtil.Parse(line[@"種別"])
                };

                Masters.Cludes.Add(clude);
            }
        }

        /// <summary>
        /// 護石マスタ書き込み
        /// </summary>
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

        /// <summary>
        /// 護石マスタ読み込み
        /// </summary>
        static internal void LoadCharmCSV()
        {
            Masters.Charms = new();

            string csv = ReadAllText(CharmCsv);

            foreach (ICsvLine line in CsvReader.ReadFromText(csv))
            {
                Equipment charm = new Equipment(EquipKind.charm);
                charm.Slot1 = ParseUtil.Parse(line[@"スロット1"]);
                charm.Slot2 = ParseUtil.Parse(line[@"スロット2"]);
                charm.Slot3 = ParseUtil.Parse(line[@"スロット3"]);
                charm.Skills = new List<Skill>();
                for (int i = 1; i <= LogicConfig.Instance.MaxCharmSkillCount; i++)
                {
                    Skill skill = new Skill(line[@"スキル系統" + i], ParseUtil.Parse(line[@"スキル値" + i]));
                    if (!string.IsNullOrWhiteSpace(skill.Name))
                    {
                        charm.Skills.Add(skill);
                    }
                }

                // TODO: 例外処理だと遅いから後で何か別の方法を考える
                // 内部管理IDがない場合は付与する
                try
                {
                    charm.Name = line[@"内部管理ID"];
                    if (string.IsNullOrWhiteSpace(charm.Name))
                    {
                        charm.Name = Guid.NewGuid().ToString();
                    }
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

        /// <summary>
        /// マイセットマスタ書き込み
        /// </summary>
        static internal void SaveMySetCSV()
        {
            List<string[]> body = new List<string[]>();
            foreach (var set in Masters.MySets)
            {
                string weaponSlot1 = set.WeaponSlot1.ToString();
                string weaponSlot2 = set.WeaponSlot2.ToString();
                string weaponSlot3 = set.WeaponSlot3.ToString();
                body.Add(new string[] { weaponSlot1, weaponSlot2, weaponSlot3, set.Head.Name, set.Body.Name, set.Arm.Name, set.Waist.Name, set.Leg.Name, set.Charm.Name, set.DecoNameCSV, set.GSkillNameCSV, set.Name });
            }
            string[] header = new string[] { "武器スロ1", "武器スロ2", "武器スロ3", "頭", "胴", "腕", "腰", "足", "護石", "装飾品", "錬成スキル", "名前" };
            string export = CsvWriter.WriteToText(header, body);
            File.WriteAllText(MySetCsv, export);
        }

        /// <summary>
        /// マイセットマスタ読み込み
        /// </summary>
        static internal void LoadMySetCSV()
        {
            Masters.MySets = new();

            string csv = ReadAllText(MySetCsv);

            foreach (ICsvLine line in CsvReader.ReadFromText(csv))
            {
                EquipSet set = new EquipSet();
                set.WeaponSlot1 = ParseUtil.Parse(line[@"武器スロ1"]);
                set.WeaponSlot2 = ParseUtil.Parse(line[@"武器スロ2"]);
                set.WeaponSlot3 = ParseUtil.Parse(line[@"武器スロ3"]);
                set.Head = Masters.GetEquipByName(line[@"頭"]);
                set.Body = Masters.GetEquipByName(line[@"胴"]);
                set.Arm = Masters.GetEquipByName(line[@"腕"]);
                set.Waist = Masters.GetEquipByName(line[@"腰"]);
                set.Leg = Masters.GetEquipByName(line[@"足"]);
                set.Charm = Masters.GetEquipByName(line[@"護石"]);
                set.Head.Kind = EquipKind.head;
                set.Body.Kind = EquipKind.body;
                set.Arm.Kind = EquipKind.arm;
                set.Waist.Kind = EquipKind.waist;
                set.Leg.Kind = EquipKind.leg;
                set.Charm.Kind = EquipKind.charm;
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
                // 前バージョンとの互換性のため存在確認
                if (line.Headers.Contains(@"錬成スキル"))
                {
                    set.GSkillNameCSV = line[@"錬成スキル"];
                }
                Masters.MySets.Add(set);
            }
        }

        /// <summary>
        /// 最近使ったスキル書き込み
        /// </summary>
        internal static void SaveRecentSkillCSV()
        {
            List<string[]> body = new List<string[]>();
            foreach (var name in Masters.RecentSkillNames)
            {
                body.Add(new string[] { name });
            }
            string[] header = new string[] { "スキル名" };
            string export = CsvWriter.WriteToText(header, body);
            try
            {
                File.WriteAllText(RecentSkillCsv, export);
            }
            catch (Exception e)
            {
                logger.Warn(e, "エラーが発生しました。");
            }
        }

        /// <summary>
        /// 最近使ったスキル読み込み
        /// </summary>
        internal static void LoadRecentSkillCSV()
        {
            Masters.RecentSkillNames = new();

            string csv = ReadAllText(RecentSkillCsv);

            foreach (ICsvLine line in CsvReader.ReadFromText(csv))
            {
                Masters.RecentSkillNames.Add(line[@"スキル名"]);
            }
        }

        /// <summary>
        /// 錬成装備マスタ読み込み
        /// </summary>
        static internal void LoadAugmentationCSV()
        {

            // 一時置き場
            List<Augmentation> augList = new();

            string csv = ReadAllText(AugmentationCsv);
            var x = CsvReader.ReadFromText(csv);
            bool isFirst = true;
            int skillCount = LogicConfig.Instance.MaxAugmentationSkillCountActual;
            foreach (ICsvLine line in x)
            {
                // スキル数確認
                // 1行目でのみ実行
                if (isFirst)
                {
                    for (int i = 1; true; i++)
                    {
                        if (!line.Headers.Contains(@"スキル系統" + i) || !line.Headers.Contains(@"スキル値" + i))
                        {
                            skillCount = Math.Max(skillCount, i - 1);
                            break;
                        }
                    }
                    isFirst = false;
                }

                Augmentation aug = new Augmentation();
                aug.BaseName = line[@"ベース装備"];
                // TODO: 例外処理は重いから別の方法で判別できないか
                try
                {
                    aug.Kind = line[@"種類"].ToEquipKind();
                }
                catch (InvalidOperationException)
                {
                    aug.Kind = EquipKind.error;
                }
                // 種類の指定がない場合泣きシミュデータ読み込みモード
                if (aug.Kind == EquipKind.error)
                {
                    Equipment baseEquip = Masters.GetEquipByName(aug.BaseName, false);
                    aug.Kind = baseEquip.Kind;
                    aug.Slot1 = baseEquip.Slot1 + ParseUtil.Parse(line[@"泣読込用1"]);
                    aug.Slot2 = baseEquip.Slot2 + ParseUtil.Parse(line[@"泣読込用2"]);
                    aug.Slot3 = baseEquip.Slot3 + ParseUtil.Parse(line[@"泣読込用3"]);

                    aug.Name = Guid.NewGuid().ToString();
                }
                else
                {
                    aug.Slot1 = ParseUtil.Parse(line[@"スロット1"]);
                    aug.Slot2 = ParseUtil.Parse(line[@"スロット2"]);
                    aug.Slot3 = ParseUtil.Parse(line[@"スロット3"]);
                    aug.DispName = line[@"名前"];
                    aug.Name = line[@"管理用ID"];
                    if (string.IsNullOrEmpty(aug.Name))
                    {
                        aug.Name = Guid.NewGuid().ToString();
                    }
                }
                aug.Def = ParseUtil.Parse(line[@"防御力増減"]);
                aug.Fire = ParseUtil.Parse(line[@"火耐性増減"]);
                aug.Water = ParseUtil.Parse(line[@"水耐性増減"]);
                aug.Thunder = ParseUtil.Parse(line[@"雷耐性増減"]);
                aug.Ice = ParseUtil.Parse(line[@"氷耐性増減"]);
                aug.Dragon = ParseUtil.Parse(line[@"龍耐性増減"]);
                List<Skill> skills = new List<Skill>();
                for (int i = 1; i <= skillCount; i++)
                {
                    if (!line.Headers.Contains(@"スキル系統" + i) || !line.Headers.Contains(@"スキル値" + i))
                    {
                        break;
                    }
                    try
                    {
                        string skill = line[@"スキル系統" + i];
                        string level = line[@"スキル値" + i];
                        if (string.IsNullOrWhiteSpace(skill))
                        {
                            break;
                        }
                        skills.Add(new Skill(skill, ParseUtil.Parse(level), true));
                    }
                    catch (Exception)
                    {
                        // カラムが存在しない場合
                        break;
                    }
                }
                aug.Skills = skills;

                augList.Add(aug);
            }

            // Masters.Augmentationsに移し替え
            // このとき、名前が空欄のものはデフォルト名をつける
            Masters.Augmentations = new();
            foreach (var aug in augList)
            {
                if (string.IsNullOrWhiteSpace(aug.DispName))
                {
                    aug.DispName = Masters.MakeAugmentaionDefaultDispName(aug.BaseName);
                }
                Masters.Augmentations.Add(aug);
            }

            // スキル数再確認
            // Column数ではなく実際のスキル数を数える(設定値と大きい方を優先)
            skillCount = LogicConfig.Instance.MaxAugmentationSkillCountActual;
            foreach (var aug in Masters.Augmentations)
            {
                if (skillCount < aug.Skills.Count)
                {
                    skillCount = aug.Skills.Count;
                }
            }
            LogicConfig.Instance.MaxAugmentationSkillCountActual = skillCount;

            // GUID等保存のためにSaveを呼び出し
            SaveAugmentationCSV();
        }

        /// <summary>
        /// 錬成装備マスタ書き込み
        /// </summary>
        static internal void SaveAugmentationCSV()
        {
            List<string[]> body = new List<string[]>();
            foreach (var aug in Masters.Augmentations)
            {
                Equipment baseEquip = Masters.GetEquipByName(aug.BaseName, false);
                List<string> bodyStrings = new List<string>();
                bodyStrings.Add(aug.BaseName);
                bodyStrings.Add(aug.Def.ToString());
                bodyStrings.Add(aug.Fire.ToString());
                bodyStrings.Add(aug.Water.ToString());
                bodyStrings.Add(aug.Thunder.ToString());
                bodyStrings.Add(aug.Ice.ToString());
                bodyStrings.Add(aug.Dragon.ToString());
                bodyStrings.Add((aug.Slot1 - baseEquip.Slot1).ToString());
                bodyStrings.Add((aug.Slot2 - baseEquip.Slot2).ToString());
                bodyStrings.Add((aug.Slot3 - baseEquip.Slot3).ToString());
                for (int i = 0; i < LogicConfig.Instance.MaxAugmentationSkillCountActual; i++)
                {
                    bodyStrings.Add(aug.Skills.Count > i ? aug.Skills[i].Name : string.Empty);
                    bodyStrings.Add(aug.Skills.Count > i ? aug.Skills[i].Level.ToString() : string.Empty);
                }
                bodyStrings.Add(aug.DispName ?? string.Empty);
                bodyStrings.Add(aug.Kind.Str());
                bodyStrings.Add(aug.Slot1.ToString());
                bodyStrings.Add(aug.Slot2.ToString());
                bodyStrings.Add(aug.Slot3.ToString());
                bodyStrings.Add(aug.Name);
                body.Add(bodyStrings.ToArray());
            }

            string[] header1 = new string[] { "ベース装備", "防御力増減", "火耐性増減", "水耐性増減", "雷耐性増減", "氷耐性増減", "龍耐性増減", "泣読込用1", "泣読込用2", "泣読込用3" };
            List<string> header2List = new();
            for (int i = 1; i <= LogicConfig.Instance.MaxAugmentationSkillCountActual; i++)
            {
                header2List.Add(@"スキル系統" + i);
                header2List.Add(@"スキル値" + i);
            }
            string[] header2 = header2List.ToArray();
            string[] header3 = new string[] { "名前", "種類", "スロット1", "スロット2", "スロット3", "管理用ID" };
            string[] header = header1.Concat(header2).Concat(header3).ToArray();
            string export = CsvWriter.WriteToText(header, body);
            File.WriteAllText(AugmentationCsv, export);
        }

        /// <summary>
        /// 理想錬成装備マスタ読み込み
        /// </summary>
        static internal void LoadIdealCSV()
        {

            // 一時置き場
            List<IdealAugmentation> idealList = new();

            string csv = ReadAllText(IdealCsv);
            var x = CsvReader.ReadFromText(csv);
            bool isFirst = true;
            int skillCount = LogicConfig.Instance.MaxAugmentationSkillCountActual;
            foreach (ICsvLine line in x)
            {
                // スキル数確認
                // 1行目でのみ実行
                if (isFirst)
                {
                    for (int i = 1; true; i++)
                    {
                        if (!line.Headers.Contains(@"スキル系統" + i) || !line.Headers.Contains(@"スキル値" + i))
                        {
                            skillCount = Math.Max(skillCount, i - 1);
                            break;
                        }
                    }
                    isFirst = false;
                }

                IdealAugmentation ideal = new IdealAugmentation();
                ideal.Table = ParseUtil.Parse(line[@"テーブル"]);
                ideal.IsIncludeLower = false;
                if (line.Headers.Contains(@"下位テーブル含フラグ"))
                {
                    ideal.IsIncludeLower = line[@"下位テーブル含フラグ"].Equals("1");
                }
                ideal.IsEnabled = true;
                if (line.Headers.Contains(@"有効無効フラグ"))
                {
                    ideal.IsEnabled = line[@"有効無効フラグ"].Equals("1");
                }
                ideal.IsRequired = false;
                if (line.Headers.Contains(@"必須フラグ"))
                {
                    ideal.IsRequired = line[@"必須フラグ"].Equals("1");
                }
                ideal.IsOne = line[@"部位制限フラグ"].Equals("1");
                ideal.SlotIncrement = ParseUtil.Parse(line[@"スロット"]);
                ideal.DispName = line[@"名前"];
                ideal.Name = line[@"管理用ID"];
                /*
                ideal.Def = ParseUtil.Parse(line[@"防御力増減"]);
                ideal.Fire = ParseUtil.Parse(line[@"火耐性増減"]);
                ideal.Water = ParseUtil.Parse(line[@"水耐性増減"]);
                ideal.Thunder = ParseUtil.Parse(line[@"雷耐性増減"]);
                ideal.Ice = ParseUtil.Parse(line[@"氷耐性増減"]);
                ideal.Dragon = ParseUtil.Parse(line[@"龍耐性増減"]);
                */
                List<Skill> skills = new List<Skill>();
                for (int i = 1; i <= skillCount; i++)
                {
                    if (!line.Headers.Contains(@"スキル系統" + i) || !line.Headers.Contains(@"スキル値" + i))
                    {
                        break;
                    }
                    try
                    {
                        string skill = line[@"スキル系統" + i];
                        string level = line[@"スキル値" + i];
                        if (string.IsNullOrWhiteSpace(skill))
                        {
                            break;
                        }
                        skills.Add(new Skill(skill, ParseUtil.Parse(level), true));
                    }
                    catch (Exception)
                    {
                        // カラムが存在しない場合
                        break;
                    }
                }
                ideal.Skills = skills;
                List<int> skillMinuses = new();
                string originalMinuses = line[@"スキルマイナス位置"];
                string[] splitedMinuses = originalMinuses.Split('-');
                foreach (var minusIndex in splitedMinuses)
                {
                    if (!string.IsNullOrWhiteSpace(minusIndex))
                    {
                        skillMinuses.Add(ParseUtil.Parse(minusIndex));
                    }
                }
                ideal.SkillMinuses = skillMinuses;
                ideal.GenericSkills[0] = ParseUtil.Parse(line[@"c3スキル数"]);
                ideal.GenericSkills[1] = ParseUtil.Parse(line[@"c6スキル数"]);
                ideal.GenericSkills[2] = ParseUtil.Parse(line[@"c9スキル数"]);
                ideal.GenericSkills[3] = ParseUtil.Parse(line[@"c12スキル数"]);
                ideal.GenericSkills[4] = ParseUtil.Parse(line[@"c15スキル数"]);

                idealList.Add(ideal);
            }

            // Masters.Idealsに移し替え
            // このとき、名前が空欄のものはデフォルト名をつける
            Masters.Ideals = new();
            foreach (var ideal in idealList)
            {
                if (string.IsNullOrWhiteSpace(ideal.DispName))
                {
                    ideal.DispName = Masters.MakeIdealAugmentaionDefaultDispName(ideal.Table);
                }
                Masters.Ideals.Add(ideal);
            }

            // スキル数再確認
            // Column数ではなく実際のスキル数を数える(設定値と大きい方を優先)
            skillCount = LogicConfig.Instance.MaxAugmentationSkillCountActual;
            foreach (var ideal in Masters.Ideals)
            {
                if (skillCount < ideal.Skills.Count)
                {
                    skillCount = ideal.Skills.Count;
                }
                if (skillCount < ideal.SkillMinuses.Count)
                {
                    skillCount = ideal.SkillMinuses.Count;
                }

            }
            LogicConfig.Instance.MaxAugmentationSkillCountActual = skillCount;

            // GUID等保存のためにSaveを呼び出し
            SaveIdealCSV();
        }

        /// <summary>
        /// 理想錬成装備マスタ書き込み
        /// </summary>
        static internal void SaveIdealCSV()
        {
            List<string[]> body = new List<string[]>();
            foreach (var ideal in Masters.Ideals)
            {
                List<string> bodyStrings = new List<string>();
                bodyStrings.Add(ideal.Table.ToString());
                bodyStrings.Add(ideal.IsIncludeLower ? "1" : "0");
                /*
                bodyStrings.Add(ideal.Def.ToString());
                bodyStrings.Add(ideal.Fire.ToString());
                bodyStrings.Add(ideal.Water.ToString());
                bodyStrings.Add(ideal.Thunder.ToString());
                bodyStrings.Add(ideal.Ice.ToString());
                bodyStrings.Add(ideal.Dragon.ToString());
                */
                bodyStrings.Add(ideal.IsOne ? "1" : "0");
                bodyStrings.Add(ideal.IsEnabled ? "1" : "0");
                bodyStrings.Add(ideal.IsRequired ? "1" : "0");
                bodyStrings.Add(ideal.SlotIncrement.ToString());
                bodyStrings.Add(ideal.DispName ?? string.Empty);
                bodyStrings.Add(ideal.Name);
                bodyStrings.Add(ideal.GenericSkills[0].ToString());
                bodyStrings.Add(ideal.GenericSkills[1].ToString());
                bodyStrings.Add(ideal.GenericSkills[2].ToString());
                bodyStrings.Add(ideal.GenericSkills[3].ToString());
                bodyStrings.Add(ideal.GenericSkills[4].ToString());
                bodyStrings.Add(string.Join('-', ideal.SkillMinuses));
                for (int i = 0; i < LogicConfig.Instance.MaxAugmentationSkillCountActual; i++)
                {
                    bodyStrings.Add(ideal.Skills.Count > i ? ideal.Skills[i].Name : string.Empty);
                    bodyStrings.Add(ideal.Skills.Count > i ? ideal.Skills[i].Level.ToString() : string.Empty);
                }
                body.Add(bodyStrings.ToArray());
            }

            string[] header1 = new string[] { "テーブル", "下位テーブル含フラグ", "部位制限フラグ", "有効無効フラグ", "必須フラグ", "スロット", "名前", "管理用ID", "c3スキル数", "c6スキル数", "c9スキル数", "c12スキル数", "c15スキル数", "スキルマイナス位置" };
            List<string> header2List = new();
            for (int i = 1; i <= LogicConfig.Instance.MaxAugmentationSkillCountActual; i++)
            {
                header2List.Add(@"スキル系統" + i);
                header2List.Add(@"スキル値" + i);
            }
            string[] header2 = header2List.ToArray();
            string[] header = header1.Concat(header2).ToArray();
            string export = CsvWriter.WriteToText(header, body);
            File.WriteAllText(IdealCsv, export);
        }

        /// <summary>
        /// マイ検索条件書き込み
        /// </summary>
        internal static void SaveMyConditionCSV()
        {
            List<string[]> body = new();
            foreach (var condition in Masters.MyConditions)
            {
                List<string> bodyStrings = new();
                bodyStrings.Add(condition.ID);
                bodyStrings.Add(condition.DispName);
                bodyStrings.Add(condition.WeaponSlot1.ToString());
                bodyStrings.Add(condition.WeaponSlot2.ToString());
                bodyStrings.Add(condition.WeaponSlot3.ToString());
                bodyStrings.Add(condition.Sex.ToString());
                bodyStrings.Add(condition.Def?.ToString() ?? "null");
                bodyStrings.Add(condition.Fire?.ToString() ?? "null");
                bodyStrings.Add(condition.Water?.ToString() ?? "null");
                bodyStrings.Add(condition.Thunder?.ToString() ?? "null");
                bodyStrings.Add(condition.Ice?.ToString() ?? "null");
                bodyStrings.Add(condition.Dragon?.ToString() ?? "null");
                bodyStrings.Add(condition.SkillCSV);
                body.Add(bodyStrings.ToArray());
            }

            string[] header = new string[] { "ID", "名前", "武器スロ1", "武器スロ2", "武器スロ3", "性別", "防御力", "火耐性", "水耐性", "雷耐性", "氷耐性", "龍耐性", "スキル"};
            string export = CsvWriter.WriteToText(header, body);
            File.WriteAllText(ConditionCsv, export);
        }

        /// <summary>
        /// マイ検索条件読み込み
        /// </summary>
        internal static void LoadMyConditionCSV()
        {
            Masters.MyConditions = new();

            string csv = ReadAllText(ConditionCsv);

            foreach (ICsvLine line in CsvReader.ReadFromText(csv))
            {
                SearchCondition condition = new();

                condition.ID = line[@"ID"];
                condition.DispName = line[@"名前"];
                condition.WeaponSlot1 = ParseUtil.Parse(line[@"武器スロ1"]);
                condition.WeaponSlot2 = ParseUtil.Parse(line[@"武器スロ2"]);
                condition.WeaponSlot3 = ParseUtil.Parse(line[@"武器スロ3"]);
                condition.Sex = line[@"性別"] == "male" ? Sex.male : Sex.female;
                condition.Def = line[@"防御力"] == "null" ? null : ParseUtil.Parse(line[@"防御力"]);
                condition.Fire = line[@"火耐性"] == "null" ? null : ParseUtil.Parse(line[@"火耐性"]);
                condition.Water = line[@"水耐性"] == "null" ? null : ParseUtil.Parse(line[@"水耐性"]);
                condition.Thunder = line[@"雷耐性"] == "null" ? null : ParseUtil.Parse(line[@"雷耐性"]);
                condition.Ice = line[@"氷耐性"] == "null" ? null : ParseUtil.Parse(line[@"氷耐性"]);
                condition.Dragon = line[@"龍耐性"] == "null" ? null : ParseUtil.Parse(line[@"龍耐性"]);
                condition.SkillCSV = line[@"スキル"];

                Masters.MyConditions.Add(condition);
            }
        }

        /// <summary>
        /// ファイル読み込み
        /// </summary>
        /// <param name="fileName">CSVファイル名</param>
        /// <returns>CSVの内容</returns>
        static private string ReadAllText(string fileName)
        {
            try
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
            catch (Exception e) when (e is DirectoryNotFoundException or FileNotFoundException)
            {
                return string.Empty;
            }
        }
    }
}
