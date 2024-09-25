using SimModel.Config;
using SimModel.Domain;
using SimModel.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimModel.Service
{
    /// <summary>
    /// シミュ本体
    /// </summary>
    public class Simulator
    {
        /// <summary>
        /// 検索インスタンス
        /// </summary>
        private Searcher Searcher { get; set; }

        /// <summary>
        /// 全件検索完了フラグ
        /// </summary>
        public bool IsSearchedAll { get; set; }

        /// <summary>
        /// 中断フラグ
        /// </summary>
        public bool IsCanceling { get; private set; } = false;

        /// <summary>
        /// データ読み込み
        /// </summary>
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

        /// <summary>
        /// 新規検索
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <param name="limit">頑張り度</param>
        /// <returns>検索結果</returns>
        public List<EquipSet> Search(SearchCondition condition, int limit)
        {
            ResetIsCanceling();

            // 検索
            if (Searcher != null)
            {
                Searcher.Dispose();
            }
            Searcher = new Searcher(condition);
            IsSearchedAll = Searcher.ExecSearch(limit);

            // 最近使ったスキル更新
            UpdateRecentSkill(condition.Skills);

            return Searcher.ResultSets;
        }

        /// <summary>
        /// 条件そのまま追加検索
        /// </summary>
        /// <param name="limit">頑張り度</param>
        /// <returns>検索結果</returns>
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

        /// <summary>
        /// 追加スキル検索
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns>検索結果</returns>
        public List<Skill> SearchExtraSkill(SearchCondition condition, Reactive.Bindings.ReactivePropertySlim<double>? progress = null)
        {
            ResetIsCanceling();

            List<Skill> exSkills = new();

            // プログレスバー
            if (progress != null)
            {
                progress.Value = 0.0;
            }

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
                        // 検索条件をコピー&処理を軽くするため一部オプションを無効化
                        SearchCondition exCondition = new(condition);
                        exCondition.PrioritizeNoIdeal = false;
                        exCondition.ExcludeAbstract = false;

                        // スキルを検索条件に追加
                        Skill exSkill = new(skill.Name, i);
                        bool isNewSkill = exCondition.AddSkill(new Skill(skill.Name, i));

                        // 新規スキルor既存だが上位Lvのスキルの場合のみ検索を実行
                        if (isNewSkill)
                        {
                            // 頑張り度1で検索
                            using Searcher exSearcher = new Searcher(exCondition);
                            exSearcher.ExecSearch(1);

                            // 1件でもヒットすれば追加スキル一覧に追加
                            if (exSearcher.ResultSets.Count > 0)
                            {
                                subResult.Add(exSkill);
                            }
                        }
                    }

                    // プログレスバー
                    if (progress != null)
                    {
                        lock (progress)
                        {
                            progress.Value += 1.0 / Masters.Skills.Count;
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


        /// <summary>
        /// 錬成の別パターン検索
        /// </summary>
        /// <param name="set">装備セット</param>
        /// <returns>検索結果</returns>
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
                    using Searcher exSearcher = new Searcher(condition);
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

        /// <summary>
        /// 錬成スキルが適正かどうか判断
        /// </summary>
        /// <param name="set">装備セット</param>
        /// <param name="pattern">錬成パターン</param>
        /// <returns>適正ならtrue</returns>
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

        /// <summary>
        /// 錬成パターン作成
        /// </summary>
        /// <param name="targetEquipNames">錬成対象スキル</param>
        /// <param name="nestCount">残り錬成可能数</param>
        /// <returns></returns>
        private List<SortedDictionary<string, int>> MakeGskillPattern(List<string> targetEquipNames, int nestCount)
        {
            if (nestCount < 1)
            {
                // 錬成可能数が残ってない場合、取得パターンは空
                List<SortedDictionary<string, int>> emptyList = new();
                emptyList.Add(new SortedDictionary<string, int>());
                return emptyList;
            }
            else
            {
                List<SortedDictionary<string, int>> newPatterns = new();
                // １つ少ない錬成可能数の錬成パターンを先に計算し、それに1スキル追加したパターンを総当たりで作る
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

        /// <summary>
        /// 除外装備登録
        /// </summary>
        /// <param name="name">対象装備名</param>
        /// <returns>追加できた場合その設定、追加できなかった場合null</returns>
        public Clude? AddExclude(string name)
        {
            return DataManagement.AddExclude(name);
        }

        /// <summary>
        /// 固定装備登録
        /// </summary>
        /// <param name="name">対象装備名</param>
        /// <returns>追加できた場合その設定、追加できなかった場合null</returns>
        public Clude? AddInclude(string name)
        {
            return DataManagement.AddInclude(name);
        }

        /// <summary>
        /// 除外・固定解除
        /// </summary>
        /// <param name="name">対象装備名</param>
        public void DeleteClude(string name)
        {
            DataManagement.DeleteClude(name);
        }

        /// <summary>
        /// 除外・固定全解除
        /// </summary>
        public void DeleteAllClude()
        {
            DataManagement.DeleteAllClude();
        }

        /// <summary>
        /// 錬成防具を全除外
        /// </summary>
        public void ExcludeAllAugmentation()
        {
            DataManagement.ExcludeAllAugmentation();
        }

        /// <summary>
        /// 指定レア度以下を全除外
        /// </summary>
        /// <param name="rare">レア度</param>
        public void ExcludeByRare(int rare)
        {
            DataManagement.ExcludeByRare(rare);
        }

        /// <summary>
        /// 護石追加
        /// </summary>
        /// <param name="skill">スキルリスト</param>
        /// <param name="slot1">スロット1</param>
        /// <param name="slot2">スロット2</param>
        /// <param name="slot3">スロット3</param>
        /// <returns>登録装備</returns>
        public Equipment AddCharm(List<Skill> skill, int slot1, int slot2, int slot3)
        {
            return DataManagement.AddCharm(skill, slot1, slot2, slot3);
        }

        /// <summary>
        /// 護石削除
        /// </summary>
        /// <param name="guid">削除対象</param>
        public void DeleteCharm(string guid)
        {
            // 除外・固定設定があったら削除
            DeleteClude(guid);

            // この護石を使っているマイセットがあったら削除
            List<EquipSet> delMySets = new();
            foreach (var set in Masters.MySets)
            {
                if (set.Charm.Name != null && set.Charm.Name.Equals(guid))
                {
                    delMySets.Add(set);
                }
            }
            foreach (var set in delMySets)
            {
                DeleteMySet(set);
            }

            // 削除
            DataManagement.DeleteCharm(guid);
        }

        /// <summary>
        /// マイセット登録
        /// </summary>
        /// <param name="set">マイセット</param>
        /// <returns>登録セット</returns>
        public EquipSet AddMySet(EquipSet set)
        {
            return DataManagement.AddMySet(set);
        }

        /// <summary>
        /// マイセット削除
        /// </summary>
        /// <param name="set">削除対象</param>
        public void DeleteMySet(EquipSet set)
        {
            DataManagement.DeleteMySet(set);
        }

        /// <summary>
        /// マイセット更新
        /// </summary>
        public void SaveMySet()
        {
            DataManagement.SaveMySet();
        }

        /// <summary>
        /// マイセット再読み込み
        /// </summary>
        public void LoadMySet()
        {
            DataManagement.LoadMySet();
        }

        /// <summary>
        /// 最近使ったスキル更新
        /// </summary>
        /// <param name="skills">検索で使ったスキル</param>
        public void UpdateRecentSkill(List<Skill> skills)
        {
            DataManagement.UpdateRecentSkill(skills);
        }

        /// <summary>
        /// 錬成装備登録
        /// </summary>
        /// <param name="aug">登録対象</param>
        public void AddAugmentation(Augmentation aug)
        {
            DataManagement.AddAugmentation(aug);
        }

        /// <summary>
        /// 錬成装備削除
        /// </summary>
        /// <param name="aug">削除対象</param>
        public void DeleteAugmentation(Augmentation aug)
        {
            // 除外・固定設定があったら削除
            DeleteClude(aug.Name);

            // この装備を使っているマイセットがあったら削除
                List<EquipSet> delMySets = new();
                foreach (var set in Masters.MySets)
                {
                    if ((set.Head.Name != null && set.Head.Name.Equals(aug.Name)) ||
                        (set.Body.Name != null && set.Body.Name.Equals(aug.Name)) ||
                        (set.Arm.Name != null && set.Arm.Name.Equals(aug.Name)) ||
                        (set.Waist.Name != null && set.Waist.Name.Equals(aug.Name)) ||
                        (set.Leg.Name != null && set.Leg.Name.Equals(aug.Name)))
                    {
                        delMySets.Add(set);
                    }
                }
                foreach (var set in delMySets)
                {
                    DeleteMySet(set);
                }
            
            DataManagement.DeleteAugmentation(aug);
        }

        /// <summary>
        /// 錬成装備更新
        /// </summary>
        /// <param name="aug">更新対象</param>
        public void UpdateAugmentation(Augmentation aug)
        {
            DataManagement.UpdateAugmentation(aug);
        }

        /// <summary>
        /// 理想錬成登録
        /// </summary>
        /// <param name="ideal">登録対象</param>
        public void AddIdealAugmentation(IdealAugmentation ideal)
        {
            DataManagement.AddIdealAugmentation(ideal);
        }

        /// <summary>
        /// 理想錬成削除
        /// </summary>
        /// <param name="ideal">削除対象</param>
        public void DeleteIdealAugmentation(IdealAugmentation ideal)
        {
            // 該当の理想錬成を使っているマイセットがある場合削除
            List<EquipSet> delMySets = new();
            foreach (var set in Masters.MySets)
            {
                if ((set.Head.Ideal?.Name != null && set.Head.Ideal?.Name == ideal.Name) ||
                    (set.Body.Ideal?.Name != null && set.Body.Ideal?.Name == ideal.Name) ||
                    (set.Arm.Ideal?.Name != null && set.Arm.Ideal?.Name == ideal.Name) ||
                    (set.Waist.Ideal?.Name != null && set.Waist.Ideal?.Name == ideal.Name) ||
                    (set.Leg.Ideal?.Name != null && set.Leg.Ideal?.Name == ideal.Name))
                {
                    delMySets.Add(set);
                }
            }
            foreach (var set in delMySets)
            {
                DeleteMySet(set);
            }

            // 削除
            DataManagement.DeleteIdealAugmentation(ideal);
        }

        /// <summary>
        /// 理想錬成更新
        /// </summary>
        /// <param name="ideal">更新対象</param>
        public void UpdateIdealAugmentation(IdealAugmentation ideal)
        {
            DataManagement.UpdateIdealAugmentation(ideal);
        }

        /// <summary>
        /// 装備マスタリロード
        /// </summary>
        public void RefreshEquipmentMasters()
        {
            Masters.RefreshEquipmentMasters();
        }

        /// <summary>
        /// 中断フラグをオン
        /// </summary>
        public void Cancel()
        {
            IsCanceling = true;
            if (Searcher != null)
            {
                Searcher.IsCanceling = true;
            }
        }

        /// <summary>
        /// 中断フラグをリセット
        /// </summary>
        public void ResetIsCanceling()
        {
            IsCanceling = false;
            if (Searcher != null)
            {
                Searcher.IsCanceling = false;
            }
        }

        /// <summary>
        /// マイ検索条件登録
        /// </summary>
        /// <param name="condition">登録対象</param>
        public void AddMyCondition(SearchCondition condition)
        {
            DataManagement.AddMyCondition(condition);
        }

        /// <summary>
        /// マイ検索条件削除
        /// </summary>
        /// <param name="condition">削除対象</param>
        public void DeleteMyCondition(SearchCondition condition)
        {
            DataManagement.DeleteMyCondition(condition);
        }

        /// <summary>
        /// マイ検索条件更新
        /// </summary>
        /// <param name="condition">更新対象</param>
        public void UpdateMyCondition(SearchCondition condition)
        {
            DataManagement.UpdateMyCondition(condition);
        }

        /// <summary>
        /// 理想錬成更新
        /// </summary>
        public void SaveIdeal()
        {
            DataManagement.SaveIdeal();
        }
    }
}
