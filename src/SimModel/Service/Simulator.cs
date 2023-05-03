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
        private Searcher Searcher { get; set; }

        // 全件検索完了フラグ
        public bool IsSearchedAll { get; set; }

        // 中断フラグ
        public bool IsCanceling { get; private set; } = false;

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
            CsvOperation.LoadIdealCSV();
            CsvOperation.LoadRecentSkillCSV();
            CsvOperation.LoadMyConditionCSV();

            // 錬成装備込みのマスタデータ作成&マイセット読み込み
            Masters.RefreshEquipmentMasters();
            CsvOperation.LoadMySetCSV();
        }

        // 新規検索
        public List<EquipSet> Search(SearchCondition condition, int limit)
        {
            ResetIsCanceling();

            // 検索
            Searcher = new Searcher(condition);
            IsSearchedAll = Searcher.ExecSearch(limit);

            // 最近使ったスキル更新
            UpdateRecentSkill(condition.Skills);

            return Searcher.ResultSets;
        }

        // 条件そのまま追加検索
        public List<EquipSet> SearchMore(int limit)
        {
            ResetIsCanceling();

            // まだ検索がされていない場合、0件で返す
            if (Searcher == null)
            {
                return new List<EquipSet>();
            }

            IsSearchedAll = Searcher.ExecSearch(limit);

            return Searcher.ResultSets;
        }

        // 追加スキル検索
        public List<Skill> SearchExtraSkill()
        {
            ResetIsCanceling();

            // まだ検索がされていない場合、0件で返す
            if (Searcher == null)
            {
                return new List<Skill>();
            }

            List<Skill> exSkills = new();


            // 全スキル全レベルを走査
            Parallel.ForEach(Masters.Skills,
                new ParallelOptions { 
                    MaxDegreeOfParallelism = LogicConfig.Instance.MaxDegreeOfParallelism 
                },
                () => new List<Skill>(),
                (skill, loop, subResult) =>
                {
                    // 中断チェック
                    // TODO: もし時間がかかるようならCancelToken等でちゃんとループを終了させること
                    if (IsCanceling)
                    {
                        return subResult;
                    }

                    for (int i = 1; i <= skill.Level; i++)
                    {
                        // 現在の検索条件をコピー&処理を軽くするため一部オプションを無効化
                        SearchCondition condition = new(Searcher.Condition);
                        condition.PrioritizeNoIdeal = false;
                        condition.ExcludeAbstract = false;

                        // スキルを検索条件に追加
                        Skill exSkill = new(skill.Name, i);
                        bool isNewSkill = condition.AddSkill(new Skill(skill.Name, i));

                        // 新規スキルor既存だが上位Lvのスキルの場合のみ検索を実行
                        if (isNewSkill)
                        {
                            // 頑張り度1で検索
                            Searcher exSearcher;
                            exSearcher = new Searcher(condition);
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


        // 錬成の別パターン検索
        public List<EquipSet> SearchOtherGenericSkillPattern(EquipSet set)
        {
            ResetIsCanceling();

            // まだ検索がされていない場合、0件で返す
            if (Searcher == null)
            {
                return new List<EquipSet>();
            }

            // 検索スキルの中で錬成可能なものをリストアップ
            List<string> targetEquipNames = new();
            foreach (var skill in Searcher.Condition.Skills)
            {
                foreach (var gskill in Masters.GenericSkills)
                {
                    if (skill.Name == gskill.Skills[0].Name)
                    {
                        targetEquipNames.Add(gskill.Name);
                    }
                }
            }

            // 錬成スキルの追加可能数をカウントアップ
            int gSkillCount = 0;
            for (int i = 0; i < 5; i++)
            {
                gSkillCount += set.Head.GenericSkills[i];
            }
            for (int i = 0; i < 5; i++)
            {
                gSkillCount += set.Body.GenericSkills[i];
            }
            for (int i = 0; i < 5; i++)
            {
                gSkillCount += set.Arm.GenericSkills[i];
            }
            for (int i = 0; i < 5; i++)
            {
                gSkillCount += set.Waist.GenericSkills[i];
            }
            for (int i = 0; i < 5; i++)
            {
                gSkillCount += set.Leg.GenericSkills[i];
            }

            // 錬成スキルの追加パターンを洗い出し
            List<SortedDictionary<string, int>> duplicatingPatterns = MakeGskillPattern(targetEquipNames, gSkillCount);

            // 重複削除
            IEnumerable<SortedDictionary<string, int>> patterns = duplicatingPatterns.GroupBy(p =>
            {
                StringBuilder sb = new StringBuilder();
                foreach (var dic in p)
                {
                    sb.Append(dic.Key);
                    sb.Append(dic.Value.ToString());
                }
                return sb.ToString();
            }).Select(group => group.First()).Where(p => IsValidCost(set, p));

            var x = patterns.Count();

            // 全パターンを走査
            List<EquipSet> result = new();
            Parallel.ForEach(patterns,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = LogicConfig.Instance.MaxDegreeOfParallelism
                },
                () => new List<EquipSet>(),
                (pattern, loop, subResult) =>
                {
                    // 中断チェック
                    // TODO: もし時間がかかるようならCancelToken等でちゃんとループを終了させること
                    if (IsCanceling)
                    {
                        return subResult;
                    }

                    // 現在の検索条件をコピー&処理を軽くするため一部オプションを無効化
                    SearchCondition condition = new(Searcher.Condition);
                    condition.PrioritizeNoIdeal = false;
                    condition.ExcludeAbstract = false;

                    // 検索条件に固定条件を追加
                    if (!string.IsNullOrEmpty(set.Head.Name))
                    {
                        pattern.Add(set.Head.Name, 1);
                    }
                    if (!string.IsNullOrEmpty(set.Body.Name))
                    {
                        pattern.Add(set.Body.Name, 1);
                    }
                    if (!string.IsNullOrEmpty(set.Arm.Name))
                    {
                        pattern.Add(set.Arm.Name, 1);
                    }
                    if (!string.IsNullOrEmpty(set.Waist.Name))
                    {
                        pattern.Add(set.Waist.Name, 1);
                    }
                    if (!string.IsNullOrEmpty(set.Leg.Name))
                    {
                        pattern.Add(set.Leg.Name, 1);
                    }
                    if (!string.IsNullOrEmpty(set.Charm.Name))
                    {
                        pattern.Add(set.Charm.Name, 1);
                    }
                    condition.AdditionalFixData = pattern;

                    // 頑張り度1で検索
                    Searcher exSearcher;
                    exSearcher = new Searcher(condition);
                    exSearcher.ExecSearch(1);

                    // 1件でもヒットすればパターン一覧に追加
                    if (exSearcher.ResultSets.Count > 0)
                    {
                        subResult.Add(exSearcher.ResultSets[0]);
                    }

                    return subResult;
                },
                (finalResult) =>
                {
                    lock (result)
                    {
                        result.AddRange(finalResult);
                    }
                }
            );

            return result;
        }

        private bool IsValidCost(EquipSet set, SortedDictionary<string, int> pattern)
        {
            int[] reqGSkills = { 0, 0, 0, 0, 0 }; // 要求
            int[] hasGSkills = { 0, 0, 0, 0, 0 }; // 所持
            int[] restGSkills = { 0, 0, 0, 0, 0 }; // 空き



            foreach (var pair in pattern)
            {
                int[] cost = Masters.SkillCostByGSkillEquipName(pair.Key);
                if (cost == null)
                {
                    return false;
                }
                for (int i = 0; i < 5; i++)
                {
                    reqGSkills[i] += cost[i];
                }
            }
            for (int i = 0; i < 5; i++)
            {
                hasGSkills[i] += set.Head.GenericSkills[i];
            }
            for (int i = 0; i < 5; i++)
            {
                hasGSkills[i] += set.Body.GenericSkills[i];
            }
            for (int i = 0; i < 5; i++)
            {
                hasGSkills[i] += set.Arm.GenericSkills[i];
            }
            for (int i = 0; i < 5; i++)
            {
                hasGSkills[i] += set.Waist.GenericSkills[i];
            }
            for (int i = 0; i < 5; i++)
            {
                hasGSkills[i] += set.Leg.GenericSkills[i];
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

            return true;
        }

        private List<SortedDictionary<string, int>> MakeGskillPattern(List<string> targetEquipNames, int nestCount)
        {
            if (nestCount < 1)
            {
                List<SortedDictionary<string, int>> emptyList = new();
                emptyList.Add(new SortedDictionary<string, int>());
                return emptyList;
            }
            else
            {
                List<SortedDictionary<string, int>> newPatterns = new();
                foreach (var oldPattern in MakeGskillPattern(targetEquipNames, nestCount - 1))
                {
                    foreach (var target in targetEquipNames)
                    {
                        SortedDictionary<string, int> newPattern = new(oldPattern);
                        if (newPattern.ContainsKey(target))
                        {
                            newPattern[target] += 1;
                        }
                        else
                        {
                            newPattern.Add(target, 1);
                        }
                        newPatterns.Add(newPattern);
                    }
                }
                return newPatterns;
            }
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

        // 指定レア度以下を全除外
        public void ExcludeByRare(int rare)
        {
            DataManagement.ExcludeByRare(rare);
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

        // 理想錬成登録
        public void AddIdealAugmentation(IdealAugmentation ideal)
        {
            DataManagement.AddIdealAugmentation(ideal);
        }

        // 理想錬成削除
        public void DeleteIdealAugmentation(IdealAugmentation ideal)
        {
            DataManagement.DeleteIdealAugmentation(ideal);
        }

        // 理想錬成更新
        public void UpdateIdealAugmentation(IdealAugmentation ideal)
        {
            DataManagement.UpdateIdealAugmentation(ideal);
        }

        // 装備マスタリロード
        public void RefreshEquipmentMasters()
        {
            Masters.RefreshEquipmentMasters();
        }

        // 中断フラグをオン
        public void Cancel()
        {
            IsCanceling = true;
            if (Searcher != null)
            {
                Searcher.IsCanceling = true;
            }
        }

        // 中断フラグをリセット
        public void ResetIsCanceling()
        {
            IsCanceling = false;
            if (Searcher != null)
            {
                Searcher.IsCanceling = false;
            }
        }

        // マイ検索条件登録
        public void AddMyCondition(SearchCondition condition)
        {
            DataManagement.AddMyCondition(condition);
        }

        // マイ検索条件削除
        public void DeleteMyCondition(SearchCondition condition)
        {
            DataManagement.DeleteMyCondition(condition);
        }

        // マイ検索条件更新
        public void UpdateMyCondition(SearchCondition condition)
        {
            DataManagement.UpdateMyCondition(condition);
        }

        // TODO:名前が良くない　というか設計が良くない
        // 理想錬成更新
        public void SaveIdeal()
        {
            DataManagement.SaveIdeal();
        }
    }
}
