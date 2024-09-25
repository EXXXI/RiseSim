﻿using Google.OrTools.LinearSolver;
using Google.Protobuf.Reflection;
using SimModel.Config;
using SimModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimModel.Domain
{
    internal class Searcher : IDisposable
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

        /// <summary>
        /// 検索条件
        /// </summary>
        public SearchCondition Condition { get; set; }

        /// <summary>
        /// ソルバ
        /// </summary>
        public Solver SimSolver { get; set; }

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
        /// スキル条件の制約式の開始Index
        /// </summary>
        private int FirstSkillRowIndex { get; set; }

        /// <summary>
        /// 検索結果除外条件の制約式の開始Index
        /// </summary>
        private int FirstResultExcludeRowIndex { get; set; }

        /// <summary>
        /// 除外・固定条件の制約式の開始Index
        /// </summary>
        private int FirstCludeRowIndex { get; set; }

        /// <summary>
        /// 風雷合一：スキル条件の制約式の開始Index
        /// </summary>
        private int FirstFuraiSkillRowIndex { get; set; }

        /// <summary>
        /// 風雷合一：フラグ条件の制約式の開始Index
        /// </summary>
        private int FirstFuraiFlagRowIndex { get; set; }

        /// <summary>
        /// 理想錬成：部位制限つきの理想編成の名前・Index
        /// </summary>
        private Dictionary<string, int> LimitedIdealAugmentationDictionary { get; set; }

        /// <summary>
        /// 除外・固定条件の制約式の開始Index
        /// </summary>
        private Dictionary<string, int> AdditionalFixRowIndexDictionary { get; set; }

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

            SimSolver = Solver.CreateSolver("SCIP");

            // 変数設定
            Variable[] x = SetVariables(SimSolver);

            // 制約式設定
            Constraint[] y = SetConstraints(SimSolver);

            // 目的関数設定(防御力)
            SetObjective(SimSolver, x);

            // 係数設定(防具データ)
            SetDatas(SimSolver, x, y);
        }

        /// <summary>
        /// 検索
        /// </summary>
        /// <param name="limit">頑張り度</param>
        /// <returns>全件検索完了した場合true</returns>
        public bool ExecSearch(int limit, bool isForExSearch = false)
        {
            // 目標検索件数
            int target = ResultSets.Count + limit;

            while (ResultSets.Count < target)
            {
                // 計算
                var result = SimSolver.Solve();
                if (!result.Equals(Solver.ResultStatus.OPTIMAL))
                {
                    // もう結果がヒットしない場合終了
                    return true;
                }

                // 計算結果整理
                EquipSet? set = MakeSet(SimSolver.variables());
                if (set == null)
                {
                    // TODO: 計算結果の空データ、何故発生する？
                    // 空データが出現したら終了
                    return true;
                }

                if (!isForExSearch) 
                {
                    // 次回検索時用
                    // 検索済み結果の除外
                    var equipIndexes = set.EquipIndexsWithOutDecos(Condition.IncludeIdealAugmentation);
                    var ny = SimSolver.MakeConstraint(0.0, equipIndexes.Count - 1, set.GlpkRowName);
                    // 検索済みデータ
                    List<int> indexList = set.EquipIndexsWithOutDecos(Condition.IncludeIdealAugmentation);
                    foreach (var index in indexList)
                    {
                        // 各装備に対応する係数を1とする
                        ny.SetCoefficient(SimSolver.variables()[index], 1);
                    }
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
        /// 検索
        /// </summary>
        /// <param name="limit">頑張り度</param>
        /// <returns>全件検索完了した場合true</returns>
        public List<Skill> ExecExSearch(Reactive.Bindings.ReactivePropertySlim<double>? progress = null)
        {
            List<Skill> result = new List<Skill>();

            // まず素の計算
            ExecSearch(1, true);
            if (ResultSets.Count < 1)
            {
                return result;
            }

            // TODO: 保持すべき？
            // 制約式
            var constraints = SimSolver.constraints();

            // 追加スキル検索
            foreach (var exSkill in Condition.Skills)
            {
                // 固定されているスキルは追加スキル検索しない
                if (exSkill.IsFixed)
                {
                    continue;
                }

                // 必要な情報を取得
                int originalLevel = exSkill.Level;
                int maxLevel = Masters.SkillMaxLevel(exSkill.Name);
                var y = constraints[FirstSkillRowIndex + Condition.Skills.IndexOf(exSkill)];

                // 追加スキル検索
                for (int level = originalLevel + 1; level <= maxLevel; level++)
                {
                    y.SetLb(level);
                    ResultSets.Clear();
                    FailureSets.Clear();
                    ExecSearch(1, true);
                    if (ResultSets.Count > 0)
                    {
                        result.Add(new Skill(exSkill.Name, level));
                    }
                    else
                    {
                        break;
                    }
                }

                // 制約式を元に戻す
                y.SetLb(originalLevel);

                // TODO: プログレスバーここじゃない 
                if (progress != null)
                {
                    lock (progress)
                    {
                        progress.Value += 1.0 / Masters.Skills.Count;
                    }

                }
            }

            return result;
        }

        /// <summary>
        /// 制約式設定
        /// </summary>
        /// <param name="solver">ソルバ</param>
        /// <returns>制約式の配列</returns>
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

        /// <summary>
        /// 変数設定
        /// </summary>
        /// <param name="solver">ソルバ</param>
        /// <returns>変数の配列</returns>
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

        /// <summary>
        /// 目的関数設定(防御力)
        /// </summary>
        /// <param name="solver">ソルバ</param>
        /// <param name="x">変数の配列</param>
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
        /// <param name="x">変数の配列</param>
        /// <param name="y">制約式の配列</param>
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

        /// <summary>
        /// 装備のデータを係数として登録
        /// </summary>
        /// <param name="xvar">変数</param>
        /// <param name="y">制約式の配列</param>
        /// <param name="equip">装備</param>
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
            if(equip.Kind != EquipKind.deco && equip.Kind != EquipKind.charm && equip.Kind != EquipKind.gskill)
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

        /// <summary>
        /// 計算結果整理
        /// </summary>
        /// <param name="x">変数の配列</param>
        /// <returns>成功時EquipSet、失敗時null</returns>
        private EquipSet? MakeSet(Variable[] x)
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
                return equipSet;
            }

            // 失敗
            return null;
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


        #region Dispose関連

        /// <summary>
        /// disposeフラグ
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing">disposeフラグ</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    SimSolver.Dispose();
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// ファイナライザ
        /// </summary>
        ~Searcher()
        {
            Dispose(false);
        }

        #endregion
    }
}
