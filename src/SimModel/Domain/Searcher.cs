using SimModel.Config;
using SimModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.OrTools.LinearSolver;
using System.Threading;

namespace SimModel.Domain
{
    internal class Searcher
    {
        // 定数：各制約式のIndex
        const int HeadRowIndex = 0;
        const int BodyRowIndex = 1;
        const int ArmRowIndex = 2;
        const int WaistRowIndex = 3;
        const int LegRowIndex = 4;
        const int CharmRowIndex = 5;
        const int Slot1RowIndex = 6;
        const int Slot2RowIndex = 7;
        const int Slot3RowIndex = 8;
        const int Slot4RowIndex = 9;
        const int SexRowIndex = 10;
        const int DefRowIndex = 11;
        const int FireRowIndex = 12;
        const int WaterRowIndex = 13;
        const int ThunderRowIndex = 14;
        const int IceRowIndex = 15;
        const int DragonRowIndex = 16;
        const int c3RowIndex = 17;
        const int c6RowIndex = 18;
        const int c9RowIndex = 19;
        const int c12RowIndex = 20;
        const int c15RowIndex = 21;

        // 検索条件
        public SearchCondition Condition { get; set; }

        // 検索結果
        public List<EquipSet> ResultSets { get; set; }

        // 失敗結果
        public List<EquipSet> FailureSets { get; set; }

        // 中断フラグ
        public bool IsCanceling { get; set; } = false;

        // 検索対象の頭一覧
        private List<Equipment> Heads { get; set; }

        // 検索対象の胴一覧
        private List<Equipment> Bodys { get; set; }

        // 検索対象の腕一覧
        private List<Equipment> Arms { get; set; }

        // 検索対象の腰一覧
        private List<Equipment> Waists { get; set; }

        // 検索対象の足一覧
        private List<Equipment> Legs { get; set; }

        // スキル条件の制約式の開始Index
        private int FirstSkillRowIndex { get; set; }

        // 検索結果除外条件の制約式の開始Index
        private int FirstResultExcludeRowIndex { get; set; }

        // 除外・固定条件の制約式の開始Index
        private int FirstCludeRowIndex { get; set; }

        // 風雷合一：スキル条件の制約式の開始Index
        private int FirstFuraiSkillRowIndex { get; set; }

        // 風雷合一：フラグ条件の制約式の開始Index
        private int FirstFuraiFlagRowIndex { get; set; }

        // 理想錬成：部位制限つきの理想編成の名前・Index
        private Dictionary<string, int> LimitedIdealAugmentationDictionary { get; set; }

        // 除外・固定条件の制約式の開始Index
        private Dictionary<string, int> AdditionalFixRowIndexDictionary { get; set; }

        // コンストラクタ：検索条件を指定する
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

        // 検索 全件検索完了した場合trueを返す
        public bool ExecSearch(int limit)
        {
            // 目標検索件数
            int target = ResultSets.Count + limit;

            while (ResultSets.Count < target)
            {
                using Solver solver = Solver.CreateSolver("SCIP");

                // 変数設定
                Variable[] x = SetVariables(solver);

                // 制約式設定
                Constraint[] y = SetConstraints(solver);

                // 目的関数設定(防御力)
                SetObjective(solver, x);

                // 係数設定(防具データ)
                SetDatas(solver, x, y);

                // 計算
                var result = solver.Solve();
                if (!result.Equals(Solver.ResultStatus.OPTIMAL))
                {
                    // もう結果がヒットしない場合終了
                    return true;
                }

                // 計算結果整理
                bool hasData = MakeSet(x);
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

        // 制約式設定
        private Constraint[] SetConstraints(Solver solver)
        {

            int numConstraints = 0;
            numConstraints += 5; // 防具5部位
            numConstraints += 1; // 護石
            numConstraints += 4; // スロットLv
            numConstraints += 1; // 性別
            numConstraints += 1; // 防御力
            numConstraints += 5; // 耐性
            numConstraints += 5; // 理想錬成スキルコスト
            numConstraints += Condition.Skills.Count; // スキル数
            numConstraints += Condition.Skills.Count; // 風雷合一：防具スキル存在条件
            numConstraints += Condition.Skills.Count; // 風雷合一：各スキル用フラグ条件
            numConstraints += Masters.Ideals.Count; // 理想錬成：部位制限
            numConstraints += ResultSets.Count + FailureSets.Count; // 検索済み結果の除外
            numConstraints += Masters.Cludes.Count; // 除外固定装備設定
            if (Condition.AdditionalFixData != null)
            {
                numConstraints += Condition.AdditionalFixData.Count(); // 錬成パターン検索用、追加固定情報
            }

            Constraint[] y = new Constraint[numConstraints];

            int index = 0;
            // 各部位に装着できる防具は1つまで
            y[index++] = solver.MakeConstraint(0, 1, "head");
            y[index++] = solver.MakeConstraint(0, 1, "body");
            y[index++] = solver.MakeConstraint(0, 1, "arm");
            y[index++] = solver.MakeConstraint(0, 1, "waist");
            y[index++] = solver.MakeConstraint(0, 1, "leg");
            y[index++] = solver.MakeConstraint(0, 1, "charm");

            // 武器スロ計算
            int[] slotCond = SlotCalc(Condition.WeaponSlot1, Condition.WeaponSlot2, Condition.WeaponSlot3);

            // 残りスロット数は0以上
            y[index++] = solver.MakeConstraint(0.0 - slotCond[0], double.PositiveInfinity, "Slot1");
            y[index++] = solver.MakeConstraint(0.0 - slotCond[1], double.PositiveInfinity, "Slot2");
            y[index++] = solver.MakeConstraint(0.0 - slotCond[2], double.PositiveInfinity, "Slot3");
            y[index++] = solver.MakeConstraint(0.0 - slotCond[3], double.PositiveInfinity, "Slot4");

            // 性別(自分と違う方を除外する)
            y[index++] = solver.MakeConstraint(0, 0, "Sex");

            // 防御・耐性
            y[index++] = solver.MakeConstraint(Condition.Def ?? 0.0, double.PositiveInfinity, "Def");
            y[index++] = solver.MakeConstraint(Condition.Fire ?? double.NegativeInfinity, double.PositiveInfinity, "Fire");
            y[index++] = solver.MakeConstraint(Condition.Water ?? double.NegativeInfinity, double.PositiveInfinity, "Water");
            y[index++] = solver.MakeConstraint(Condition.Thunder ?? double.NegativeInfinity, double.PositiveInfinity, "Thunder");
            y[index++] = solver.MakeConstraint(Condition.Ice ?? double.NegativeInfinity, double.PositiveInfinity, "Ice");
            y[index++] = solver.MakeConstraint(Condition.Dragon ?? double.NegativeInfinity, double.PositiveInfinity, "Dragon");

            // 理想錬成スキルコスト
            y[index++] = solver.MakeConstraint(0.0, double.PositiveInfinity, "c3");
            y[index++] = solver.MakeConstraint(0.0, double.PositiveInfinity, "c6");
            y[index++] = solver.MakeConstraint(0.0, double.PositiveInfinity, "c9");
            y[index++] = solver.MakeConstraint(0.0, double.PositiveInfinity, "c12");
            y[index++] = solver.MakeConstraint(0.0, double.PositiveInfinity, "c15");

            // スキル条件
            FirstSkillRowIndex = index;
            foreach (var skill in Condition.Skills)
            {
                if (skill.IsFixed)
                {
                    y[index++] = solver.MakeConstraint(skill.Level, skill.Level, skill.Name);
                }
                else
                {
                    y[index++] = solver.MakeConstraint(skill.Level, double.PositiveInfinity, skill.Name);
                }
            }

            // 風雷合一：防具スキル存在条件
            FirstFuraiSkillRowIndex = index;
            foreach (var skill in Condition.Skills)
            {
                y[index++] = solver.MakeConstraint(0.0, double.PositiveInfinity, skill.Name + "風雷スキル存在");
            }

            // 風雷合一：各スキル用フラグ条件
            FirstFuraiFlagRowIndex = index;
            foreach (var skill in Condition.Skills)
            {
                y[index++] = solver.MakeConstraint(0.0, double.PositiveInfinity, skill.Name + "風雷フラグ");
            }

            LimitedIdealAugmentationDictionary = new();
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
                LimitedIdealAugmentationDictionary.Add(ideal.Name, index);
                y[index++] = solver.MakeConstraint(min, max, ideal.Name);
            }

            // 検索済み結果の除外
            FirstResultExcludeRowIndex = index;
            foreach (var set in ResultSets)
            {
                var equipIndexes = set.EquipIndexsWithOutDecos(Condition.IncludeIdealAugmentation);
                y[index++] = solver.MakeConstraint(0.0, equipIndexes.Count - 1, set.GlpkRowName);
            }
            foreach (var set in FailureSets)
            {
                var equipIndexes = set.EquipIndexsWithOutDecos(Condition.IncludeIdealAugmentation);
                y[index++] = solver.MakeConstraint(0.0, equipIndexes.Count - 1, set.GlpkRowName);
            }

            // 除外固定装備設定
            FirstCludeRowIndex = index;
            foreach (var clude in Masters.Cludes)
            {
                string nameSuffix = "_ex";
                int fix = 0;
                if (clude.Kind.Equals(CludeKind.include))
                {
                    nameSuffix = "_in";
                    fix = 1;
                }
                y[index++] = solver.MakeConstraint(fix, fix, clude.Name + nameSuffix);
            }

            // 錬成パターン検索用、追加固定情報
            if (Condition.AdditionalFixData != null)
            {
                AdditionalFixRowIndexDictionary = new();
                foreach (var fixData in Condition.AdditionalFixData)
                {
                    string nameSuffix = "_ex_additional";
                    int fix = fixData.Value;
                    AdditionalFixRowIndexDictionary.Add(fixData.Key, index);
                    y[index++] = solver.MakeConstraint(fix, fix, fixData.Key + nameSuffix);
                }
            }


            return y;
        }

        // 変数設定
        private Variable[] SetVariables(Solver solver)
        {
            // 変数の数
            int numVars = 0;
            numVars += Heads.Count;
            numVars += Bodys.Count;
            numVars += Arms.Count;
            numVars += Waists.Count;
            numVars += Legs.Count;
            numVars += Masters.Charms.Count;
            numVars += Masters.Decos.Count;
            numVars += Masters.GenericSkills.Count;
            numVars += Condition.Skills.Count;
            numVars += Condition.Skills.Count;

            Variable[] x = new Variable[numVars];

            // 各装備は0個以上で整数
            int index = 0;
            foreach (var equip in Heads)
            {
                x[index++] = solver.MakeIntVar(0.0, double.PositiveInfinity, equip.Name);
            }
            foreach (var equip in Bodys)
            {
                x[index++] = solver.MakeIntVar(0.0, double.PositiveInfinity, equip.Name);
            }
            foreach (var equip in Arms)
            {
                x[index++] = solver.MakeIntVar(0.0, double.PositiveInfinity, equip.Name);
            }
            foreach (var equip in Waists)
            {
                x[index++] = solver.MakeIntVar(0.0, double.PositiveInfinity, equip.Name);
            }
            foreach (var equip in Legs)
            {
                x[index++] = solver.MakeIntVar(0.0, double.PositiveInfinity, equip.Name);
            }
            foreach (var equip in Masters.Charms)
            {
                x[index++] = solver.MakeIntVar(0.0, double.PositiveInfinity, equip.Name);
            }
            foreach (var equip in Masters.Decos)
            {
                x[index++] = solver.MakeIntVar(0.0, double.PositiveInfinity, equip.Name);
            }
            foreach (var equip in Masters.GenericSkills)
            {
                x[index++] = solver.MakeIntVar(0.0, double.PositiveInfinity, equip.Name);
            }

            // 風雷合一：Lv4
            foreach (var skill in Condition.Skills)
            {
                x[index++] = solver.MakeIntVar(0.0, double.PositiveInfinity, skill.Name + "風雷4");
            }

            // 風雷合一：Lv5
            foreach (var skill in Condition.Skills)
            {
                x[index++] = solver.MakeIntVar(0.0, double.PositiveInfinity, skill.Name + "風雷5");
            }

            return x;
        }

        // TODO: 目的関数、防御力以外も対応する？
        // 目的関数設定(防御力)
        private void SetObjective(Solver solver, Variable[] x)
        {
            Objective objective = solver.Objective();

            // 各装備の防御力が、目的関数における各装備の項の係数となる
            int index = 0;
            foreach (var equip in Heads)
            {
                objective.SetCoefficient(x[index++], Score(equip));
            }
            foreach (var equip in Bodys)
            {
                objective.SetCoefficient(x[index++], Score(equip));
            }
            foreach (var equip in Arms)
            {
                objective.SetCoefficient(x[index++], Score(equip));
            }
            foreach (var equip in Waists)
            {
                objective.SetCoefficient(x[index++], Score(equip));
            }
            foreach (var equip in Legs)
            {
                objective.SetCoefficient(x[index++], Score(equip));
            }
            foreach (var equip in Masters.Charms)
            {
                objective.SetCoefficient(x[index++], Score(equip));
            }
            foreach (var equip in Masters.Decos)
            {
                objective.SetCoefficient(x[index++], Score(equip));
            }
            foreach (var equip in Masters.GenericSkills)
            {
                objective.SetCoefficient(x[index++], Score(equip));
            }
            objective.SetMaximization();
        }

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
                score -= 200;
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

        // 係数設定(防具データ)
        private void SetDatas(Solver solver, Variable[] x, Constraint[] y)
        {
            // 防具データ
            int columnIndex = 0;
            foreach (var equip in Heads)
            {
                SetEquipData(x[columnIndex], y, equip);
                columnIndex++;
            }
            foreach (var equip in Bodys)
            {
                SetEquipData(x[columnIndex], y, equip);
                columnIndex++;
            }
            foreach (var equip in Arms)
            {
                SetEquipData(x[columnIndex], y, equip);
                columnIndex++;
            }
            foreach (var equip in Waists)
            {
                SetEquipData(x[columnIndex], y, equip);
                columnIndex++;
            }
            foreach (var equip in Legs)
            {
                SetEquipData(x[columnIndex], y, equip);
                columnIndex++;
            }
            foreach (var equip in Masters.Charms)
            {
                SetEquipData(x[columnIndex], y, equip);
                columnIndex++;
            }
            foreach (var equip in Masters.Decos)
            {
                SetEquipData(x[columnIndex], y, equip);
                columnIndex++;
            }
            foreach (var equip in Masters.GenericSkills)
            {
                SetEquipData(x[columnIndex], y, equip);
                columnIndex++;
            }

            // 風雷合一：各スキルLv4
            foreach (var skill in Condition.Skills)
            {
                if (!LogicConfig.Instance.FuraiName.Equals(skill.Name))
                {
                    // スキル値追加
                    y[FirstSkillRowIndex + Condition.Skills.IndexOf(skill)].SetCoefficient(x[columnIndex], 1);

                    // スキル存在値減少
                    y[FirstFuraiSkillRowIndex + Condition.Skills.IndexOf(skill)].SetCoefficient(x[columnIndex], -1);

                    // スキルフラグ値減少
                    y[FirstFuraiFlagRowIndex + Condition.Skills.IndexOf(skill)].SetCoefficient(x[columnIndex], -4);
                }
                columnIndex++;
            }

            // 風雷合一：各スキルLv5
            foreach (var skill in Condition.Skills)
            {
                if (!LogicConfig.Instance.FuraiName.Equals(skill.Name))
                {
                    // スキル値追加
                    y[FirstSkillRowIndex + Condition.Skills.IndexOf(skill)].SetCoefficient(x[columnIndex], 2);

                    // スキル存在値減少
                    y[FirstFuraiSkillRowIndex + Condition.Skills.IndexOf(skill)].SetCoefficient(x[columnIndex], -1);

                    // スキルフラグ値減少
                    y[FirstFuraiFlagRowIndex + Condition.Skills.IndexOf(skill)].SetCoefficient(x[columnIndex], -5);

                }
                columnIndex++;
            }

            // 検索済みデータ
            int resultExcludeRowIndex = FirstResultExcludeRowIndex;
            foreach (var set in ResultSets)
            {
                List<int> indexList = set.EquipIndexsWithOutDecos(Condition.IncludeIdealAugmentation);
                foreach (var index in indexList)
                {
                    // 各装備に対応する係数を1とする
                    y[resultExcludeRowIndex].SetCoefficient(x[index], 1);
                }
                resultExcludeRowIndex++;
            }
            foreach (var set in FailureSets)
            {
                List<int> indexList = set.EquipIndexsWithOutDecos(Condition.IncludeIdealAugmentation);
                foreach (var index in indexList)
                {
                    // 各装備に対応する係数を1とする
                    y[resultExcludeRowIndex].SetCoefficient(x[index], 1);
                }
                resultExcludeRowIndex++;
            }

            // 除外固定データ
            int cludeRowIndex = FirstCludeRowIndex;
            foreach (var clude in Masters.Cludes)
            {
                // 装備に対応する係数を1とする
                List<int> indexs = Masters.GetCludeIndexsByName(clude.Name, Condition.IncludeIdealAugmentation);
                foreach (var index in indexs)
                {
                    y[cludeRowIndex].SetCoefficient(x[index], 1);
                }
                cludeRowIndex++;
            }
        }

        // 装備のデータを係数として登録
        private void SetEquipData(Variable xvar, Constraint[] y, Equipment equip)
        {
            // 部位情報
            int kindIndex = 0;
            bool isDecoOrGSkill = false;
            switch (equip.Kind)
            {
                case EquipKind.head:
                    kindIndex = HeadRowIndex;
                    break;
                case EquipKind.body:
                    kindIndex = BodyRowIndex;
                    break;
                case EquipKind.arm:
                    kindIndex = ArmRowIndex;
                    break;
                case EquipKind.waist:
                    kindIndex = WaistRowIndex;
                    break;
                case EquipKind.leg:
                    kindIndex = LegRowIndex;
                    break;
                case EquipKind.charm:
                    kindIndex = CharmRowIndex;
                    break;
                default:
                    isDecoOrGSkill
                        = true;
                    break;
            }
            if (!isDecoOrGSkill)
            {
                y[kindIndex].SetCoefficient(xvar, 1);
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
            y[Slot1RowIndex].SetCoefficient(xvar, slotCond[0]);
            y[Slot2RowIndex].SetCoefficient(xvar, slotCond[1]);
            y[Slot3RowIndex].SetCoefficient(xvar, slotCond[2]);
            y[Slot4RowIndex].SetCoefficient(xvar, slotCond[3]);

            // 性別情報(自分と違う方を除外する)
            if (!equip.Sex.Equals(Sex.all) && !equip.Sex.Equals(Condition.Sex))
            {
                y[SexRowIndex].SetCoefficient(xvar, 1);
            }

            // 防御・耐性情報
            y[DefRowIndex].SetCoefficient(xvar, equip.Maxdef);
            y[FireRowIndex].SetCoefficient(xvar, equip.Fire);
            y[WaterRowIndex].SetCoefficient(xvar, equip.Water);
            y[ThunderRowIndex].SetCoefficient(xvar, equip.Thunder);
            y[IceRowIndex].SetCoefficient(xvar, equip.Ice);
            y[DragonRowIndex].SetCoefficient(xvar, equip.Dragon);

            // 理想錬成スキルコスト情報

            int[] costCond = CostCalc(equip.GenericSkills);
            if (isDecoOrGSkill)
            {
                for (int i = 0; i < costCond.Length; i++)
                {
                    costCond[i] = costCond[i] * -1;
                }
            }
            y[c3RowIndex].SetCoefficient(xvar, costCond[0]);
            y[c6RowIndex].SetCoefficient(xvar, costCond[1]);
            y[c9RowIndex].SetCoefficient(xvar, costCond[2]);
            y[c12RowIndex].SetCoefficient(xvar, costCond[3]);
            y[c15RowIndex].SetCoefficient(xvar, costCond[4]);

            // スキル情報
            foreach (var condSkill in Condition.Skills)
            {
                foreach (var equipSkill in equip.MargedSkills)
                {
                    if (equipSkill.Name.Equals(condSkill.Name))
                    {
                        y[FirstSkillRowIndex + Condition.Skills.IndexOf(condSkill)].SetCoefficient(xvar, equipSkill.Level);
                    }
                }
            }

            // 風雷合一：スキル存在情報
            if(equip.Kind != EquipKind.deco && equip.Kind != EquipKind.charm)
            {
                foreach (var condSkill in Condition.Skills)
                {
                    foreach (var equipSkill in equip.Skills)
                    {
                        // 錬成分は除外
                        if (equipSkill.Name.Equals(condSkill.Name) && !equipSkill.IsAdditional)
                        {
                            y[FirstFuraiSkillRowIndex + Condition.Skills.IndexOf(condSkill)].SetCoefficient(xvar, 1);
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
                    y[FirstFuraiFlagRowIndex + Condition.Skills.IndexOf(condSkill)].SetCoefficient(xvar, 1);
                }
            }

            // 理想錬成：部位制限・有効無効制限
            if (equip.Ideal != null)
            {
                int index = LimitedIdealAugmentationDictionary[equip.Ideal.Name];
                y[index].SetCoefficient(xvar, 1);
            }


            // 錬成パターン検索用、追加固定情報
            if (Condition.AdditionalFixData != null && Condition.AdditionalFixData.ContainsKey(equip.Name))
            {
                int index = AdditionalFixRowIndexDictionary[equip.Name];
                y[index].SetCoefficient(xvar, 1);
            }
        }

        // 計算結果整理
        private bool MakeSet(Variable[] x)
        {
            EquipSet equipSet = new();
            bool hasData = false;
            for (int i = 0; i < x.Length; i++)
            {
                if (x[i].SolutionValue() > 0)
                {
                    // 装備名
                    string name = x[i].Name();

                    // 存在チェック
                    Equipment? equip = Masters.GetEquipByName(name);
                    if (equip == null || string.IsNullOrWhiteSpace(equip.Name))
                    {
                        // 存在しない装備名の場合無視
                        // 護石削除関係でバグっていた場合の対策
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
                            for (int j = 0; j < x[i].SolutionValue(); j++)
                            {
                                // 装飾品は個数を確認し、その数追加
                                equipSet.Decos.Add(equip);
                            }
                            break;
                        case EquipKind.charm:
                            equipSet.Charm = equip;
                            break;
                        case EquipKind.gskill:
                            for (int j = 0; j < x[i].SolutionValue(); j++)
                            {
                                // 錬成スキルは個数を確認し、その数追加
                                equipSet.GenericSkills.Add(equip);
                            }
                            break;
                        default:
                            break;
                    }
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

        // 重複する結果(今回の結果に無駄な装備を加えたもの)が既に見つかっていた場合、それを削除
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

        // 重複判定
        private bool IsDuplicateEquipName(string newName, string oldName)
        {
            return string.IsNullOrWhiteSpace(newName) || newName.Equals(oldName);
        }


        // 理想錬成を含む装備セットについて、同ベース装備の既存の錬成で作成できた場合、理想錬成での結果を除外
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

        // 抽象判定
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


        // 理想錬成のスキルが実現可能か確認
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

        // planの組み合わせで理想錬成のスキルが実現可能か確認
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

        // 組み合わせ一覧作成
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




        // スロットの計算
        // 例：3-1-1→1スロ以下2個2スロ以下2個3スロ以下3個
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

        // コスト計算
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
