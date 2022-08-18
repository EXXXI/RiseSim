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
using Google.OrTools.LinearSolver;

namespace SimModel.Domain
{
    internal class Searcher : ISearcher
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

        // 検索条件
        public SearchCondition Condition { get; set; }

        // 検索結果
        public List<EquipSet> ResultSets { get; set; }

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

        
        // コンストラクタ：検索条件を指定する
        public Searcher(SearchCondition condition)
        {
            Condition = condition;
            ResultSets = new List<EquipSet>();
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
            numConstraints += Condition.Skills.Count; // スキル数
            numConstraints += Condition.Skills.Count; // 風雷合一：防具スキル存在条件
            numConstraints += Condition.Skills.Count; // 風雷合一：各スキル用フラグ条件
            numConstraints += ResultSets.Count; // 検索済み結果の除外
            numConstraints += Masters.Cludes.Count; // 除外固定装備設定

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

            // スキル条件
            FirstSkillRowIndex = index;
            foreach (var skill in Condition.Skills)
            {

                y[index++] = solver.MakeConstraint(skill.Level, double.PositiveInfinity, skill.Name);
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

            // 検索済み結果の除外
            FirstResultExcludeRowIndex = index;
            foreach (var set in ResultSets)
            {
                y[index++] = solver.MakeConstraint(0.0, set.EquipIndexsWithOutDecos.Count - 1, set.GlpkRowName);
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

            return y;
        }

        // 変数設定
        private Variable[] SetVariables(Solver solver)
        {
            // 変数の数
            int numVars = 0;
            numVars += Masters.Heads.Count;
            numVars += Masters.Bodys.Count;
            numVars += Masters.Arms.Count;
            numVars += Masters.Waists.Count;
            numVars += Masters.Legs.Count;
            numVars += Masters.Charms.Count;
            numVars += Masters.Decos.Count;
            numVars += Condition.Skills.Count;
            numVars += Condition.Skills.Count;

            Variable[] x = new Variable[numVars];

            // 各装備は0個以上で整数
            int index = 0;
            foreach (var equip in Masters.Heads)
            {
                x[index++] = solver.MakeIntVar(0.0, double.PositiveInfinity, equip.Name);
            }
            foreach (var equip in Masters.Bodys)
            {
                x[index++] = solver.MakeIntVar(0.0, double.PositiveInfinity, equip.Name);
            }
            foreach (var equip in Masters.Arms)
            {
                x[index++] = solver.MakeIntVar(0.0, double.PositiveInfinity, equip.Name);
            }
            foreach (var equip in Masters.Waists)
            {
                x[index++] = solver.MakeIntVar(0.0, double.PositiveInfinity, equip.Name);
            }
            foreach (var equip in Masters.Legs)
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
        private static void SetObjective(Solver solver, Variable[] x)
        {
            Objective objective = solver.Objective();

            // 各装備の防御力が、目的関数における各装備の項の係数となる
            int index = 0;
            foreach (var equip in Masters.Heads)
            {
                objective.SetCoefficient(x[index++], Score(equip));
            }
            foreach (var equip in Masters.Bodys)
            {
                objective.SetCoefficient(x[index++], Score(equip));
            }
            foreach (var equip in Masters.Arms)
            {
                objective.SetCoefficient(x[index++], Score(equip));
            }
            foreach (var equip in Masters.Waists)
            {
                objective.SetCoefficient(x[index++], Score(equip));
            }
            foreach (var equip in Masters.Legs)
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
            objective.SetMaximization();
        }

        private static int Score(Equipment equip)
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

            // 防御力
            score += equip.Maxdef;

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
            foreach (var equip in Masters.Heads)
            {
                SetEquipData(x[columnIndex], y, equip);
                columnIndex++;
            }
            foreach (var equip in Masters.Bodys)
            {
                SetEquipData(x[columnIndex], y, equip);
                columnIndex++;
            }
            foreach (var equip in Masters.Arms)
            {
                SetEquipData(x[columnIndex], y, equip);
                columnIndex++;
            }
            foreach (var equip in Masters.Waists)
            {
                SetEquipData(x[columnIndex], y, equip);
                columnIndex++;
            }
            foreach (var equip in Masters.Legs)
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
                List<int> indexList = set.EquipIndexsWithOutDecos;
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
                int index = Masters.GetEquipIndexByName(clude.Name);
                y[cludeRowIndex].SetCoefficient(x[index], 1);
                cludeRowIndex++;
            }
        }

        // 装備のデータを係数として登録
        private void SetEquipData(Variable xvar, Constraint[] y, Equipment equip)
        {
            // 部位情報
            int kindIndex = 0;
            bool isDeco = false;
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
                    isDeco = true;
                    break;
            }
            if (!isDeco)
            {
                y[kindIndex].SetCoefficient(xvar, 1);
            }

            // スロット情報
            int[] slotCond = SlotCalc(equip.Slot1, equip.Slot2, equip.Slot3);
            if (isDeco)
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

                // 重複する結果(今回の結果に無駄な装備を加えたもの)が既に見つかっていた場合、それを削除
                RemoveDuplicateSet(equipSet);

                // 検索結果に追加
                ResultSets.Add(equipSet);

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
    }
}
