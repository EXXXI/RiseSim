using SimModel.Config;
using SimModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.OrTools.LinearSolver;
using System.Threading;
using System.Windows.Annotations;

namespace SimModel.Domain
{
    internal class Searcher
    {
        /// <summary>
        /// 制約式名や変数名の重複を防ぐための疑似特殊文字
        /// 本来は入力不可文字を規定してそれを使うべきだが
        /// </summary>
        private string SeparateGUID { get; } = $"__{Guid.NewGuid().ToString()}__";
        // 定数：各制約式の名称
        const string HeadRowName = "head";
        const string BodyRowName = "body";
        const string ArmRowName = "arm";
        const string WaistRowName = "waist";
        const string LegRowName = "leg";
        const string CharmRowName = "charm";
        const string Slot1RowName = "slot1";
        const string Slot2RowName = "slot2";
        const string Slot3RowName = "slot3";
        const string Slot4RowName = "slot4";
        const string SexRowName = "sex";
        const string DefRowName = "def";
        const string FireRowName = "fire";
        const string WaterRowName = "water";
        const string ThunderRowName = "thunder";
        const string IceRowName = "ice";
        const string DragonRowName = "dragon";
        const string C3RowName = "c3";
        const string C6RowName = "c6";
        const string C9RowName = "c9";
        const string C12RowName = "c12";
        const string C15RowName = "c15";
        const string SkillRowPrefix = "skill_";
        const string FuraiExistRowPrefix = "furaiExist_";
        const string FuraiFlagRowPrefix = "furaiFlag_";
        const string IdealRowPrefix = "ideal_";
        const string SetRowPrefix = "set_";
        const string CludeRowPrefix = "clude_";
        const string AdditionalFixRowPrefix = "additionalFix_";
        const string EquipColPrefix = "equip_";
        const string Furai4ColPrefix = "furai4_";
        const string Furai5ColPrefix = "furai5_";

        /// <summary>
        /// 検索条件
        /// </summary>
        public SearchCondition Condition { get; set; }

        /// <summary>
        /// 検索結果
        /// </summary>
        public List<EquipSet> ResultSets { get; set; }

        /// <summary>
        /// 失敗結果
        /// </summary>
        public List<EquipSet> FailureSets { get; set; }

        /// <summary>
        /// 中断フラグ
        /// </summary>
        public bool IsCanceling { get; set; } = false;

        /// <summary>
        /// 検索対象の頭一覧
        /// </summary>
        private List<Equipment> Heads { get; set; }

        /// <summary>
        /// 検索対象の胴一覧
        /// </summary>
        private List<Equipment> Bodys { get; set; }

        /// <summary>
        /// 検索対象の腕一覧
        /// </summary>
        private List<Equipment> Arms { get; set; }

        /// <summary>
        /// 検索対象の腰一覧
        /// </summary>
        private List<Equipment> Waists { get; set; }

        /// <summary>
        /// 検索対象の足一覧
        /// </summary>
        private List<Equipment> Legs { get; set; }

        /// <summary>
        /// コンストラクタ：検索条件を指定する
        /// </summary>
        /// <param name="condition"></param>
        public Searcher(SearchCondition condition)
        {
            Condition = condition;
            ResultSets = new List<EquipSet>();
            FailureSets = new List<EquipSet>();

            if (condition.IncludeIdealAugmentation)
            {
                Heads = Masters.IdealHeads;
                Bodys = Masters.IdealBodys;
                Arms = Masters.IdealArms;
                Waists = Masters.IdealWaists;
                Legs = Masters.IdealLegs;
            }
            else
            {
                Heads = Masters.Heads;
                Bodys = Masters.Bodys;
                Arms = Masters.Arms;
                Waists = Masters.Waists;
                Legs = Masters.Legs;
            }
        }

        /// <summary>
        /// 検索
        /// </summary>
        /// <param name="limit">頑張り度</param>
        /// <returns>全件検索完了した場合true</returns>
        public bool ExecSearch(int limit)
        {
            // 目標検索件数
            int target = ResultSets.Count + limit;

            while (ResultSets.Count < target)
            {
                using Solver solver = Solver.CreateSolver("SCIP");

                // 変数設定
                SetVariables(solver);

                // 制約式設定
                SetConstraints(solver);

                // 目的関数設定(防御力)
                SetObjective(solver);

                // 係数設定(防具データ)
                SetDatas(solver);

                // 計算
                var result = solver.Solve();
                if (!result.Equals(Solver.ResultStatus.OPTIMAL))
                {
                    // もう結果がヒットしない場合終了
                    return true;
                }

                // 計算結果整理
                bool hasData = MakeSet(solver);
                if (!hasData)
                {
                    // TODO: 計算結果の空データ、何故発生する？
                    // 空データが出現したら終了
                    return true;
                }

                // 中断確認
                if (IsCanceling)
                {
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// 制約式設定
        /// </summary>
        /// <param name="solver">ソルバ</param>
        private void SetConstraints(Solver solver)
        {
            // 各部位に装着できる防具は1つまで
            solver.MakeConstraint(0, 1, HeadRowName);
            solver.MakeConstraint(0, 1, BodyRowName);
            solver.MakeConstraint(0, 1, ArmRowName);
            solver.MakeConstraint(0, 1, WaistRowName);
            solver.MakeConstraint(0, 1, LegRowName);
            solver.MakeConstraint(0, 1, CharmRowName);

            // 武器スロ計算
            int[] slotCond = SlotCalc(Condition.WeaponSlot1, Condition.WeaponSlot2, Condition.WeaponSlot3);

            // 残りスロット数は0以上
            solver.MakeConstraint(0.0 - slotCond[0], double.PositiveInfinity, Slot1RowName);
            solver.MakeConstraint(0.0 - slotCond[1], double.PositiveInfinity, Slot2RowName);
            solver.MakeConstraint(0.0 - slotCond[2], double.PositiveInfinity, Slot3RowName);
            solver.MakeConstraint(0.0 - slotCond[3], double.PositiveInfinity, Slot4RowName);

            // 性別(自分と違う方を除外する)
            solver.MakeConstraint(0, 0, SexRowName);

            // 防御・耐性
            solver.MakeConstraint(Condition.Def ?? 0.0, double.PositiveInfinity, DefRowName);
            solver.MakeConstraint(Condition.Fire ?? double.NegativeInfinity, double.PositiveInfinity, FireRowName);
            solver.MakeConstraint(Condition.Water ?? double.NegativeInfinity, double.PositiveInfinity, WaterRowName);
            solver.MakeConstraint(Condition.Thunder ?? double.NegativeInfinity, double.PositiveInfinity, ThunderRowName);
            solver.MakeConstraint(Condition.Ice ?? double.NegativeInfinity, double.PositiveInfinity, IceRowName);
            solver.MakeConstraint(Condition.Dragon ?? double.NegativeInfinity, double.PositiveInfinity, DragonRowName);

            // 理想錬成スキルコスト
            solver.MakeConstraint(0.0, double.PositiveInfinity, C3RowName);
            solver.MakeConstraint(0.0, double.PositiveInfinity, C6RowName);
            solver.MakeConstraint(0.0, double.PositiveInfinity, C9RowName);
            solver.MakeConstraint(0.0, double.PositiveInfinity, C12RowName);
            solver.MakeConstraint(0.0, double.PositiveInfinity, C15RowName);

            // スキル条件
            foreach (var skill in Condition.Skills)
            {
                if (skill.IsFixed)
                {
                    solver.MakeConstraint(skill.Level, skill.Level, SkillRowPrefix + skill.Name);
                }
                else
                {
                    solver.MakeConstraint(skill.Level, double.PositiveInfinity, SkillRowPrefix + skill.Name);
                }
            }

            // 風雷合一：防具スキル存在条件
            foreach (var skill in Condition.Skills)
            {
                solver.MakeConstraint(0.0, double.PositiveInfinity, FuraiExistRowPrefix + skill.Name);
            }

            // 風雷合一：各スキル用フラグ条件
            foreach (var skill in Condition.Skills)
            {
                solver.MakeConstraint(0.0, double.PositiveInfinity, FuraiFlagRowPrefix + skill.Name);
            }

            // 理想錬成：部位制限や有効無効制限
            foreach (var ideal in Masters.Ideals)
            {
                double min = 0.0;
                if (ideal.IsRequired)
                {
                    min = 1.0;
                }
                double max = 5.0;
                if (ideal.IsOne)
                {
                    max = 1.0;
                }
                if (!ideal.IsEnabled)
                {
                    max = 0.0;
                }
                solver.MakeConstraint(min, max, IdealRowPrefix + ideal.Name);
            }

            // 検索済み結果の除外
            foreach (var set in ResultSets)
            {
                int equipCount = 0;
                equipCount += string.IsNullOrWhiteSpace(set.Head?.Name) ? 0 : 1;
                equipCount += string.IsNullOrWhiteSpace(set.Body?.Name) ? 0 : 1;
                equipCount += string.IsNullOrWhiteSpace(set.Arm?.Name) ? 0 : 1;
                equipCount += string.IsNullOrWhiteSpace(set.Waist?.Name) ? 0 : 1;
                equipCount += string.IsNullOrWhiteSpace(set.Leg?.Name) ? 0 : 1;
                equipCount += string.IsNullOrWhiteSpace(set.Charm?.Name) ? 0 : 1;
                solver.MakeConstraint(0.0, equipCount - 1, SetRowPrefix + set.GlpkRowName);
            }
            foreach (var set in FailureSets)
            {
                int equipCount = 0;
                equipCount += string.IsNullOrWhiteSpace(set.Head?.Name) ? 0 : 1;
                equipCount += string.IsNullOrWhiteSpace(set.Body?.Name) ? 0 : 1;
                equipCount += string.IsNullOrWhiteSpace(set.Arm?.Name) ? 0 : 1;
                equipCount += string.IsNullOrWhiteSpace(set.Waist?.Name) ? 0 : 1;
                equipCount += string.IsNullOrWhiteSpace(set.Leg?.Name) ? 0 : 1;
                equipCount += string.IsNullOrWhiteSpace(set.Charm?.Name) ? 0 : 1;
                solver.MakeConstraint(0.0, equipCount - 1, SetRowPrefix + set.GlpkRowName);
            }

            // 除外固定装備設定
            foreach (var clude in Masters.Cludes)
            {
                int fix = 0;
                if (clude.Kind.Equals(CludeKind.include))
                {
                    fix = 1;
                }
                solver.MakeConstraint(fix, fix, CludeRowPrefix + clude.Name);
            }

            // 錬成パターン検索用、追加固定情報
            if (Condition.AdditionalFixData != null)
            {
                foreach (var fixData in Condition.AdditionalFixData)
                {
                    int fix = fixData.Value;
                    solver.MakeConstraint(fix, fix, AdditionalFixRowPrefix + fixData.Key);
                }
            }
        }

        /// <summary>
        /// 変数設定
        /// </summary>
        /// <param name="solver">ソルバ</param>
        private void SetVariables(Solver solver)
        {
            // 各装備は0個以上で整数
            foreach (var equip in Heads)
            {
                solver.MakeIntVar(0.0, double.PositiveInfinity, EquipColPrefix + equip.Name);
            }
            foreach (var equip in Bodys)
            {
                solver.MakeIntVar(0.0, double.PositiveInfinity, EquipColPrefix + equip.Name);
            }
            foreach (var equip in Arms)
            {
                solver.MakeIntVar(0.0, double.PositiveInfinity, EquipColPrefix + equip.Name);
            }
            foreach (var equip in Waists)
            {
                solver.MakeIntVar(0.0, double.PositiveInfinity, EquipColPrefix + equip.Name);
            }
            foreach (var equip in Legs)
            {
                solver.MakeIntVar(0.0, double.PositiveInfinity, EquipColPrefix + equip.Name);
            }
            foreach (var equip in Masters.Charms)
            {
                solver.MakeIntVar(0.0, double.PositiveInfinity, EquipColPrefix + equip.Name);
            }
            foreach (var equip in Masters.Decos)
            {
                solver.MakeIntVar(0.0, double.PositiveInfinity, EquipColPrefix + equip.Name);
            }
            foreach (var equip in Masters.GenericSkills)
            {
                solver.MakeIntVar(0.0, double.PositiveInfinity, EquipColPrefix + equip.Name);
            }

            // 風雷合一：Lv4
            foreach (var skill in Condition.Skills)
            {
                solver.MakeIntVar(0.0, double.PositiveInfinity, Furai4ColPrefix + skill.Name);
            }

            // 風雷合一：Lv5
            foreach (var skill in Condition.Skills)
            {
                solver.MakeIntVar(0.0, double.PositiveInfinity, Furai5ColPrefix + skill.Name);
            }
        }

        /// <summary>
        /// 目的関数設定(防御力)
        /// </summary>
        /// <param name="solver">ソルバ</param>
        private void SetObjective(Solver solver)
        {
            Objective objective = solver.Objective();

            // 各装備の防御力が、目的関数における各装備の項の係数となる
            int index = 0;
            foreach (var equip in Heads)
            {
                var v = solver.variables().First(v => v.Name() == (EquipColPrefix + equip.Name));
                objective.SetCoefficient(v, Score(equip));
            }
            foreach (var equip in Bodys)
            {
                var v = solver.variables().First(v => v.Name() == (EquipColPrefix + equip.Name));
                objective.SetCoefficient(v, Score(equip));
            }
            foreach (var equip in Arms)
            {
                var v = solver.variables().First(v => v.Name() == (EquipColPrefix + equip.Name));
                objective.SetCoefficient(v, Score(equip));
            }
            foreach (var equip in Waists)
            {
                var v = solver.variables().First(v => v.Name() == (EquipColPrefix + equip.Name));
                objective.SetCoefficient(v, Score(equip));
            }
            foreach (var equip in Legs)
            {
                var v = solver.variables().First(v => v.Name() == (EquipColPrefix + equip.Name));
                objective.SetCoefficient(v, Score(equip));
            }
            foreach (var equip in Masters.Charms)
            {
                var v = solver.variables().First(v => v.Name() == (EquipColPrefix + equip.Name));
                objective.SetCoefficient(v, Score(equip));
            }
            foreach (var equip in Masters.Decos)
            {
                var v = solver.variables().First(v => v.Name() == (EquipColPrefix + equip.Name));
                objective.SetCoefficient(v, Score(equip));
            }
            foreach (var equip in Masters.GenericSkills)
            {
                var v = solver.variables().First(v => v.Name() == (EquipColPrefix + equip.Name));
                objective.SetCoefficient(v, Score(equip));
            }
            objective.SetMaximization();
        }

        /// <summary>
        /// 装備の評価スコアを返す
        /// </summary>
        /// <param name="equip">装備</param>
        /// <returns>スコア</returns>
        private int Score(Equipment equip)
        {
            int slot1 = 0;
            int slot2 = 0;
            int slot3 = 0;

            if (equip.Kind != EquipKind.deco)
            {
                slot1 = equip.Slot1;
                slot2 = equip.Slot2;
                slot3 = equip.Slot3;
            }
            else
            {
                slot1 = -equip.Slot1;
            }


            int score = 0;

            // 理想錬成以外を優先
            if (Condition.PrioritizeNoIdeal && equip.Ideal != null)
            {
                score -= 100;
            }

            // 防御力
            score += equip.Maxdef;

            // 錬成コスト
            score *= 100;
            if (equip.Kind == EquipKind.gskill)
            {
                for (int i = 0; i < 5; i++)
                {
                    score -= equip.GenericSkills[i] * (i + 1) * 3;
                }
            }

            // スロット数
            score *= 20;
            score += Math.Sign(slot1);
            score += Math.Sign(slot2);
            score += Math.Sign(slot3);

            // スロット大きさ
            score *= 80;
            score += slot1 + slot2 + slot3;

            return score;
        }

        /// <summary>
        /// 係数設定(防具データ)
        /// </summary>
        /// <param name="solver">ソルバ</param>
        private void SetDatas(Solver solver)
        {
            // 防具データ
            foreach (var equip in Heads)
            {
                SetEquipData(solver, equip);
            }
            foreach (var equip in Bodys)
            {
                SetEquipData(solver, equip);
            }
            foreach (var equip in Arms)
            {
                SetEquipData(solver, equip);
            }
            foreach (var equip in Waists)
            {
                SetEquipData(solver, equip);
            }
            foreach (var equip in Legs)
            {
                SetEquipData(solver, equip);
            }
            foreach (var equip in Masters.Charms)
            {
                SetEquipData(solver, equip);
            }
            foreach (var equip in Masters.Decos)
            {
                SetEquipData(solver, equip);
            }
            foreach (var equip in Masters.GenericSkills)
            {
                SetEquipData(solver, equip);
            }

            // 風雷合一：各スキルLv4
            foreach (var skill in Condition.Skills)
            {
                if (!LogicConfig.Instance.FuraiName.Equals(skill.Name))
                {
                    var v = solver.variables().First(v => v.Name() == (Furai4ColPrefix + skill.Name));

                    // スキル値追加
                    solver.constraints().First(c => c.Name() == (SkillRowPrefix + skill.Name)).SetCoefficient(v, 1);

                    // スキル存在値減少
                    solver.constraints().First(c => c.Name() == (FuraiExistRowPrefix + skill.Name)).SetCoefficient(v, -1);

                    // スキルフラグ値減少
                    solver.constraints().First(c => c.Name() == (FuraiFlagRowPrefix + skill.Name)).SetCoefficient(v, -4);
                }
            }

            // 風雷合一：各スキルLv5
            foreach (var skill in Condition.Skills)
            {
                if (!LogicConfig.Instance.FuraiName.Equals(skill.Name))
                {
                    var v = solver.variables().First(v => v.Name() == (Furai5ColPrefix + skill.Name));

                    // スキル値追加
                    solver.constraints().First(c => c.Name() == (SkillRowPrefix + skill.Name)).SetCoefficient(v, 2);

                    // スキル存在値減少
                    solver.constraints().First(c => c.Name() == (FuraiExistRowPrefix + skill.Name)).SetCoefficient(v, -1);

                    // スキルフラグ値減少
                    solver.constraints().First(c => c.Name() == (FuraiFlagRowPrefix + skill.Name)).SetCoefficient(v, -5);
                }
            }

            // 検索済みデータ
            foreach (var set in ResultSets)
            {
                var y = solver.constraints().First(c => c.Name() == (SetRowPrefix + set.GlpkRowName));

                // 各装備に対応する係数を1とする
                if (!string.IsNullOrWhiteSpace(set.Head?.Name))
                {
                    y.SetCoefficient(solver.variables().First(v => v.Name() == (EquipColPrefix + set.Head.Name)), 1);
                }
                if (!string.IsNullOrWhiteSpace(set.Body?.Name))
                {
                    y.SetCoefficient(solver.variables().First(v => v.Name() == (EquipColPrefix + set.Body.Name)), 1);
                }
                if (!string.IsNullOrWhiteSpace(set.Arm?.Name))
                {
                    y.SetCoefficient(solver.variables().First(v => v.Name() == (EquipColPrefix + set.Arm.Name)), 1);
                }
                if (!string.IsNullOrWhiteSpace(set.Waist?.Name))
                {
                    y.SetCoefficient(solver.variables().First(v => v.Name() == (EquipColPrefix + set.Waist.Name)), 1);
                }
                if (!string.IsNullOrWhiteSpace(set.Leg?.Name))
                {
                    y.SetCoefficient(solver.variables().First(v => v.Name() == (EquipColPrefix + set.Leg.Name)), 1);
                }
                if (!string.IsNullOrWhiteSpace(set.Charm?.Name))
                {
                    y.SetCoefficient(solver.variables().First(v => v.Name() == (EquipColPrefix + set.Charm.Name)), 1);
                }
            }
            foreach (var set in FailureSets)
            {
                var y = solver.constraints().First(c => c.Name() == (SetRowPrefix + set.GlpkRowName));

                // 各装備に対応する係数を1とする
                if (!string.IsNullOrWhiteSpace(set.Head?.Name))
                {
                    y.SetCoefficient(solver.variables().First(v => v.Name() == (EquipColPrefix + set.Head.Name)), 1);
                }
                if (!string.IsNullOrWhiteSpace(set.Body?.Name))
                {
                    y.SetCoefficient(solver.variables().First(v => v.Name() == (EquipColPrefix + set.Body.Name)), 1);
                }
                if (!string.IsNullOrWhiteSpace(set.Arm?.Name))
                {
                    y.SetCoefficient(solver.variables().First(v => v.Name() == (EquipColPrefix + set.Arm.Name)), 1);
                }
                if (!string.IsNullOrWhiteSpace(set.Waist?.Name))
                {
                    y.SetCoefficient(solver.variables().First(v => v.Name() == (EquipColPrefix + set.Waist.Name)), 1);
                }
                if (!string.IsNullOrWhiteSpace(set.Leg?.Name))
                {
                    y.SetCoefficient(solver.variables().First(v => v.Name() == (EquipColPrefix + set.Leg.Name)), 1);
                }
                if (!string.IsNullOrWhiteSpace(set.Charm?.Name))
                {
                    y.SetCoefficient(solver.variables().First(v => v.Name() == (EquipColPrefix + set.Charm.Name)), 1);
                }
            }

            // 除外固定データ
            foreach (var clude in Masters.Cludes)
            {
                var y = solver.constraints().First(c => c.Name() == (CludeRowPrefix + clude.Name));

                // 装備に対応する係数を1とする
                List<Equipment> equips = Masters.GetCludeEquipsByName(clude.Name, Condition.IncludeIdealAugmentation);
                foreach (var equip in equips)
                {
                    y.SetCoefficient(solver.variables().First(v => v.Name() == (EquipColPrefix + equip.Name)), 1);
                }
            }
        }

        /// <summary>
        /// 装備のデータを係数として登録
        /// </summary>
        /// <param name="solver">ソルバ</param>
        /// <param name="equip">装備</param>
        private void SetEquipData(Solver solver, Equipment equip)
        {
            var xvar = solver.variables().First(v => v.Name() == (EquipColPrefix + equip.Name));

            // 部位情報
            string kindRowName = string.Empty;
            bool isDecoOrGSkill = false;
            switch (equip.Kind)
            {
                case EquipKind.head:
                    kindRowName = HeadRowName;
                    break;
                case EquipKind.body:
                    kindRowName = BodyRowName;
                    break;
                case EquipKind.arm:
                    kindRowName = ArmRowName;
                    break;
                case EquipKind.waist:
                    kindRowName = WaistRowName;
                    break;
                case EquipKind.leg:
                    kindRowName = LegRowName;
                    break;
                case EquipKind.charm:
                    kindRowName = CharmRowName;
                    break;
                default:
                    isDecoOrGSkill = true;
                    break;
            }
            if (!isDecoOrGSkill)
            {
                solver.constraints().First(c => c.Name() == kindRowName).SetCoefficient(xvar, 1);
            }

            // スロット情報
            int[] slotCond = SlotCalc(equip.Slot1, equip.Slot2, equip.Slot3);
            if (isDecoOrGSkill)
            {
                for (int i = 0; i < slotCond.Length; i++)
                {
                    slotCond[i] = slotCond[i] * -1;
                }
            }
            solver.constraints().First(c => c.Name() == Slot1RowName).SetCoefficient(xvar, slotCond[0]);
            solver.constraints().First(c => c.Name() == Slot2RowName).SetCoefficient(xvar, slotCond[1]);
            solver.constraints().First(c => c.Name() == Slot3RowName).SetCoefficient(xvar, slotCond[2]);
            solver.constraints().First(c => c.Name() == Slot4RowName).SetCoefficient(xvar, slotCond[3]);

            // 性別情報(自分と違う方を除外する)
            if (!equip.Sex.Equals(Sex.all) && !equip.Sex.Equals(Condition.Sex))
            {
                solver.constraints().First(c => c.Name() == SexRowName).SetCoefficient(xvar, 1);
            }

            // 防御・耐性情報
            solver.constraints().First(c => c.Name() == DefRowName).SetCoefficient(xvar, equip.Maxdef);
            solver.constraints().First(c => c.Name() == FireRowName).SetCoefficient(xvar, equip.Fire);
            solver.constraints().First(c => c.Name() == WaterRowName).SetCoefficient(xvar, equip.Water);
            solver.constraints().First(c => c.Name() == ThunderRowName).SetCoefficient(xvar, equip.Thunder);
            solver.constraints().First(c => c.Name() == IceRowName).SetCoefficient(xvar, equip.Ice);
            solver.constraints().First(c => c.Name() == DragonRowName).SetCoefficient(xvar, equip.Dragon);

            // 理想錬成スキルコスト情報

            int[] costCond = CostCalc(equip.GenericSkills);
            if (isDecoOrGSkill)
            {
                for (int i = 0; i < costCond.Length; i++)
                {
                    costCond[i] = costCond[i] * -1;
                }
            }
            solver.constraints().First(c => c.Name() == C3RowName).SetCoefficient(xvar, costCond[0]);
            solver.constraints().First(c => c.Name() == C6RowName).SetCoefficient(xvar, costCond[1]);
            solver.constraints().First(c => c.Name() == C9RowName).SetCoefficient(xvar, costCond[2]);
            solver.constraints().First(c => c.Name() == C12RowName).SetCoefficient(xvar, costCond[3]);
            solver.constraints().First(c => c.Name() == C15RowName).SetCoefficient(xvar, costCond[4]);

            // スキル情報
            foreach (var condSkill in Condition.Skills)
            {
                foreach (var equipSkill in equip.MargedSkills)
                {
                    if (equipSkill.Name.Equals(condSkill.Name))
                    {
                        solver.constraints().First(c => c.Name() == (SkillRowPrefix + condSkill.Name)).SetCoefficient(xvar, equipSkill.Level);
                    }
                }
            }

            // 風雷合一：スキル存在情報
            if(equip.Kind != EquipKind.deco && equip.Kind != EquipKind.charm && equip.Kind != EquipKind.gskill)
            {
                foreach (var condSkill in Condition.Skills)
                {
                    foreach (var equipSkill in equip.Skills)
                    {
                        // 錬成分は除外
                        if (equipSkill.Name.Equals(condSkill.Name) && !equipSkill.IsAdditional)
                        {
                            solver.constraints().First(c => c.Name() == (FuraiExistRowPrefix + condSkill.Name)).SetCoefficient(xvar, 1);
                        }
                    }
                }
            }

            // 風雷合一：スキル用フラグ情報
            bool hasFurai = false;
            foreach (var skill in equip.Skills)
            {
                if (LogicConfig.Instance.FuraiName.Equals(skill.Name))
                {
                    hasFurai = true;
                    break;
                }
            }
            if (hasFurai)
            {
                foreach (var condSkill in Condition.Skills)
                {
                    solver.constraints().First(c => c.Name() == (FuraiFlagRowPrefix + condSkill.Name)).SetCoefficient(xvar, 1);
                }
            }

            // 理想錬成：部位制限・有効無効制限
            if (equip.Ideal != null)
            {
                solver.constraints().First(c => c.Name() == (IdealRowPrefix + equip.Ideal.Name)).SetCoefficient(xvar, 1);
            }


            // 錬成パターン検索用、追加固定情報
            if (Condition.AdditionalFixData != null && Condition.AdditionalFixData.ContainsKey(equip.Name))
            {
                solver.constraints().First(c => c.Name() == (AdditionalFixRowPrefix + equip.Name)).SetCoefficient(xvar, 1);
            }
        }

        /// <summary>
        /// 計算結果整理
        /// </summary>
        /// <param name="solver">ソルバ</param>
        /// <returns>成功でtrue</returns>
        private bool MakeSet(Solver solver)
        {
            EquipSet equipSet = new();
            bool hasData = false;
            var hittedVal = solver.variables().Where(v => v.SolutionValue() > 0);
            foreach (var val in hittedVal)
            {
                // 装備名
                string name = val.Name().Replace(EquipColPrefix, string.Empty);

                // 存在チェック
                Equipment? equip = Masters.GetEquipByName(name);
                if (equip == null || string.IsNullOrWhiteSpace(equip.Name))
                {
                    // 存在しない装備名の場合無視
                    continue;
                }
                hasData = true;

                // 装備種類確認
                switch (equip.Kind)
                {
                    case EquipKind.head:
                        equipSet.Head = equip;
                        break;
                    case EquipKind.body:
                        equipSet.Body = equip;
                        break;
                    case EquipKind.arm:
                        equipSet.Arm = equip;
                        break;
                    case EquipKind.waist:
                        equipSet.Waist = equip;
                        break;
                    case EquipKind.leg:
                        equipSet.Leg = equip;
                        break;
                    case EquipKind.deco:
                        for (int i = 0; i < val.SolutionValue(); i++)
                        {
                            // 装飾品は個数を確認し、その数追加
                            equipSet.Decos.Add(equip);
                        }
                        break;
                    case EquipKind.charm:
                        equipSet.Charm = equip;
                        break;
                    case EquipKind.gskill:
                        for (int i = 0; i < val.SolutionValue(); i++)
                        {
                            // 錬成スキルは個数を確認し、その数追加
                            equipSet.GenericSkills.Add(equip);
                        }
                        break;
                    default:
                        break;
                }
            }

            if (hasData)
            {
                // 装備セットにスロット情報を付加
                equipSet.WeaponSlot1 = Condition.WeaponSlot1;
                equipSet.WeaponSlot2 = Condition.WeaponSlot2;
                equipSet.WeaponSlot3 = Condition.WeaponSlot3;

                bool isValid = true;

                // 理想錬成のスキルが実現可能か確認
                isValid = isValid && IsValidGenericSkill(equipSet);

                // 既存防具での具体的な検索結果が既にあるかどうか確認
                if (Condition.ExcludeAbstract)
                {
                    isValid = isValid && !IsAbstractSet(equipSet);
                }

                if (isValid)
                {
                    // 重複する結果(今回の結果に無駄な装備を加えたもの)が既に見つかっていた場合、それを削除
                    RemoveDuplicateSet(equipSet);

                    // 装飾品・理想錬成スキルをソート
                    equipSet.SortDecos();
                    equipSet.SortGSkills();

                    // 検索結果に追加
                    ResultSets.Add(equipSet);
                }
                else
                {
                    // 除外用結果に追加
                    FailureSets.Add(equipSet);
                }

                // 成功
                return true;
            }

            // 失敗
            return false;
        }

        /// <summary>
        /// 重複する結果(今回の結果に無駄な装備を加えたもの)が既に見つかっていた場合、それを削除
        /// </summary>
        /// <param name="newSet">新しい検索結果</param>
        private void RemoveDuplicateSet(EquipSet newSet)
        {
            List<EquipSet> removeList = new();
            foreach (var set in ResultSets)
            {
                if (!IsDuplicateEquipName(newSet.Head.Name, set.Head.Name))
                {
                    continue;
                }
                if (!IsDuplicateEquipName(newSet.Body.Name, set.Body.Name))
                {
                    continue;
                }
                if (!IsDuplicateEquipName(newSet.Arm.Name, set.Arm.Name))
                {
                    continue;
                }
                if (!IsDuplicateEquipName(newSet.Waist.Name, set.Waist.Name))
                {
                    continue;
                }
                if (!IsDuplicateEquipName(newSet.Leg.Name, set.Leg.Name))
                {
                    continue;
                }
                if (!IsDuplicateEquipName(newSet.Charm.Name, set.Charm.Name))
                {
                    continue;
                }

                // 全ての部位で重複判定を満たしたため削除
                removeList.Add(set);
            }

            foreach (var set in removeList)
            {
                ResultSets.Remove(set);
            }
        }

        /// <summary>
        /// 重複判定
        /// </summary>
        /// <param name="newName">新セットの防具名</param>
        /// <param name="oldName">旧セットの防具名</param>
        /// <returns></returns>
        private bool IsDuplicateEquipName(string newName, string oldName)
        {
            return string.IsNullOrWhiteSpace(newName) || newName.Equals(oldName);
        }

        /// <summary>
        /// 理想錬成を含む装備セットについて、同ベース装備の既存の錬成で作成できるか否か
        /// </summary>
        /// <param name="newSet">新セット</param>
        /// <returns>既存の錬成で作成できる場合true</returns>
        private bool IsAbstractSet(EquipSet newSet)
        {
            foreach (var set in ResultSets)
            {
                if (!IsAbstractEquip(newSet.Head, set.Head))
                {
                    continue;
                }
                if (!IsAbstractEquip(newSet.Body, set.Body))
                {
                    continue;
                }
                if (!IsAbstractEquip(newSet.Arm, set.Arm))
                {
                    continue;
                }
                if (!IsAbstractEquip(newSet.Waist, set.Waist))
                {
                    continue;
                }
                if (!IsAbstractEquip(newSet.Leg, set.Leg))
                {
                    continue;
                }
                if (!IsAbstractEquip(newSet.Charm, set.Charm))
                {
                    continue;
                }

                // 全ての部位で抽象判定を満たした
                return true;
            }
            return false;
        }

        /// <summary>
        /// 抽象判定
        /// </summary>
        /// <param name="newEquip">新防具</param>
        /// <param name="oldEquip">旧防具</param>
        /// <returns>既存の錬成で置き換え可能ならtrue</returns>
        private bool IsAbstractEquip(Equipment newEquip, Equipment oldEquip)
        {
            // 同じ防具
            if (newEquip.Name == oldEquip.Name)
            {
                return true;
            }

            // 理想vs理想以外の時のみ判定する
            if (newEquip.Ideal == null || oldEquip.Ideal != null)
            {
                return false;
            }

            // ベース防具が同じ時のみ判定する
            string newBaseName = newEquip.BaseEquipment?.Name ?? string.Empty;
            string oldBaseName = oldEquip.BaseEquipment?.Name ?? oldEquip.Name;
            if (newBaseName != oldBaseName)
            {
                return false;
            }

            // スロット確認
            if (newEquip.Slot1 != oldEquip.Slot1 ||
                newEquip.Slot2 != oldEquip.Slot2 ||
                newEquip.Slot3 != oldEquip.Slot3)
            {
                return false;
            }

            // スキル差分計算
            List<Skill> skillDiff = new List<Skill>();
            foreach (var oldSkill in oldEquip.Skills)
            {
                bool isExist = false;
                foreach (var skill in skillDiff)
                {
                    if (skill.Name == oldSkill.Name)
                    {
                        isExist = true;
                        skill.Level += oldSkill.Level;
                        break;
                    }
                }
                if (!isExist)
                {
                    skillDiff.Add(new Skill(oldSkill.Name, oldSkill.Level));
                }
            }
            foreach (var newSkill in newEquip.Skills)
            {
                bool isExist = false;
                foreach (var skill in skillDiff)
                {
                    if (skill.Name == newSkill.Name)
                    {
                        isExist = true;
                        skill.Level -= newSkill.Level;
                        break;
                    }
                }
                if (!isExist)
                {
                    skillDiff.Add(new Skill(newSkill.Name, -newSkill.Level));
                }
            }

            // 差分を実現可能か確認
            int[] reqGSkills = { 0, 0, 0, 0, 0 }; // 要求
            int[] hasGSkills = { 0, 0, 0, 0, 0 }; // 所持
            int[] restGSkills = { 0, 0, 0, 0, 0 }; // 空き
            foreach (var skill in skillDiff)
            {
                if (skill.Level <= 0)
                {
                    continue;
                }
                int[]? gSkills = Masters.SkillCost(skill.Name);
                if (gSkills == null)
                {
                    // 錬成対象外のスキルを要求された
                    return false; 
                }
                for (int i = 0; i < gSkills.Length; i++)
                {
                    reqGSkills[i] += gSkills[i] * skill.Level;
                }
            }
            for (int i = 0; i < newEquip.GenericSkills.Length; i++)
            {
                hasGSkills[i] += newEquip.GenericSkills[i];
            }

            // 空き算出
            for (int i = 0; i < 5; i++)
            {
                restGSkills[i] = hasGSkills[i] - reqGSkills[i];
            }

            // 足りない分は1Lv上を消費する
            for (int i = 0; i < 4; i++)
            {
                if (restGSkills[i] < 0)
                {
                    restGSkills[i + 1] += restGSkills[i];
                    restGSkills[i] = 0;
                }
            }

            if (restGSkills[4] < 0)
            {
                // 不足
                return false;
            }

            return true;
        }


        /// <summary>
        /// 理想錬成のスキルが実現可能か確認
        /// </summary>
        /// <param name="equipSet">セット</param>
        /// <returns>可能ならtrue</returns>
        private bool IsValidGenericSkill(EquipSet equipSet)
        {
            // 組み合わせ一覧作成
            List<List<int>> plans = MakeValidatePlans(equipSet.GenericSkills.Count);

            if (plans.Count < 1)
            {
                return true;
            }

            foreach (var plan in plans)
            {
                // planの組み合わせで理想錬成のスキルが実現可能か確認
                bool isValid = ValidateGenericSkill(equipSet, plan);
                if (isValid)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// planの組み合わせで理想錬成のスキルが実現可能か確認
        /// </summary>
        /// <param name="equipSet">装備セット</param>
        /// <param name="plan">組み合わせ</param>
        /// <returns>実現可能ならtrue</returns>
        private bool ValidateGenericSkill(EquipSet equipSet, List<int> plan)
        {
            List<Equipment> gSkillEquips = equipSet.GenericSkills;

            // 頭
            if (!ValidateEquipSkill(gSkillEquips, plan, 0, equipSet.Head))
            {
                return false;
            }
            // 胴
            if (!ValidateEquipSkill(gSkillEquips, plan, 1, equipSet.Body))
            {
                return false;
            }
            // 腕
            if (!ValidateEquipSkill(gSkillEquips, plan, 2, equipSet.Arm))
            {
                return false;
            }
            // 腰
            if (!ValidateEquipSkill(gSkillEquips, plan, 3, equipSet.Waist))
            {
                return false;
            }
            // 足
            if (!ValidateEquipSkill(gSkillEquips, plan, 4, equipSet.Leg))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 指定防具に指定のスキルが実装可能かどうか確認
        /// </summary>
        /// <param name="gSkillEquips">錬成スキル一覧</param>
        /// <param name="plan">組み合わせ</param>
        /// <param name="kindIndex">plan上の表記に対応する部位番号</param>
        /// <param name="equip">装備</param>
        /// <returns>実装可能ならtrue</returns>
        private static bool ValidateEquipSkill(List<Equipment> gSkillEquips, List<int> plan, int kindIndex, Equipment equip)
        {

            // コスト条件
            int[] reqGSkills = { 0, 0, 0, 0, 0 }; // 要求
            int[] hasGSkills = { 0, 0, 0, 0, 0 }; // 所持
            int[] restGSkills = { 0, 0, 0, 0, 0 }; // 空き
            for (int i = 0; i < plan.Count; i++)
            {
                if (plan[i] == kindIndex)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        reqGSkills[j] += gSkillEquips[i].GenericSkills[j];
                    }
                }
            }

            for (int i = 0; i < 5; i++)
            {
                hasGSkills[i] += equip.GenericSkills[i];
            }

            // 空き算出
            for (int i = 0; i < 5; i++)
            {
                restGSkills[i] = hasGSkills[i] - reqGSkills[i];
            }

            // 足りない分は1Lv上を消費する
            for (int i = 0; i < 4; i++)
            {
                if (restGSkills[i] < 0)
                {
                    restGSkills[i + 1] += restGSkills[i];
                    restGSkills[i] = 0;
                }
            }

            if (restGSkills[4] < 0)
            {
                // スロット不足
                return false;
            }

            // スキル数
            // スキル数が6以上の場合、そのパターンは却下
            List<string> skillList = new();
            // ベース防具スキルは全てカウント
            foreach (var skill in equip.Skills)
            {
                if (skill.Level > 0 && !skillList.Contains(skill.Name))
                {
                    skillList.Add(skill.Name);
                }
            }
            // 適用後のスキルをカウント
            for (int i = 0; i < plan.Count; i++)
            {
                if (plan[i] == kindIndex)
                {
                    foreach (var skill in gSkillEquips[i].Skills)
                    {
                        if (skill.Level > 0 && !skillList.Contains(skill.Name))
                        {
                            skillList.Add(skill.Name);
                        }
                    }
                }
            }
            if (skillList.Count > 5)
            {
                // スキルオーバー
                return false;
            }

            return true;
        }

        /// <summary>
        /// 組み合わせ一覧作成
        /// </summary>
        /// <param name="count">残りスキル数</param>
        /// <returns>組み合わせ一覧(0～4(頭～足)の順列のリスト)</returns>
        private List<List<int>> MakeValidatePlans(int count)
        {
            if (count < 1)
            {
                return new();
            }

            if (count == 1)
            {
                List<List<int>> firstList = new();
                for (int i = 0; i < 5; i++)
                {
                    List<int> item = new();
                    item.Add(i);
                    firstList.Add(item);
                }
                return firstList;
            }

            List<List<int>> oldList = MakeValidatePlans(count - 1);
            List<List<int>> newList = new();

            foreach (var oldItem in oldList)
            {
                for (int i = 0; i < 5; i++)
                {
                    List<int> newItem = new(oldItem);
                    newItem.Add(i);
                    newList.Add(newItem);
                }
            }

            return newList;
        }

        /// <summary>
        /// GLPK用のスロット計算
        /// 例：3-1-1→1スロ以下2個2スロ以下2個3スロ以下3個
        /// </summary>
        /// <param name="slot1">スロット1</param>
        /// <param name="slot2">スロット2</param>
        /// <param name="slot3">スロット3</param>
        /// <returns>GLPK用のスロット値</returns>
        private int[] SlotCalc(int slot1, int slot2, int slot3)
        {
            int[] slotCond = new int[4];
            for (int i = 0; i < slot1; i++)
            {
                slotCond[i]++;
            }
            for (int i = 0; i < slot2; i++)
            {
                slotCond[i]++;
            }
            for (int i = 0; i < slot3; i++)
            {
                slotCond[i]++;
            }
            return slotCond;
        }

        /// <summary>
        /// GLPK用のコスト計算
        /// </summary>
        /// <param name="genericSkills">錬成スキル一覧</param>
        /// <returns>GLPK用のコスト値</returns>
        private int[] CostCalc(int[] genericSkills)
        {
            int[] result = new int[5];
            result[4] = genericSkills[4];
            result[3] = result[4] + genericSkills[3];
            result[2] = result[3] + genericSkills[2];
            result[1] = result[2] + genericSkills[1];
            result[0] = result[1] + genericSkills[0];

            return result;
        }
    }
}
