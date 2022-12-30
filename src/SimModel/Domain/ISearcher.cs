using SimModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimModel.Domain
{
    internal interface ISearcher
    {
        // 検索 全件検索完了した場合trueを返す
        public bool ExecSearch(int limit);

        // 検索条件
        public SearchCondition Condition { get; set; }

        // 検索結果
        public List<EquipSet> ResultSets { get; set; }
    }
}
