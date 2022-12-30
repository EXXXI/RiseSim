using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimModel.Model
{
    // 除外・固定情報
    public class Clude
    {
        // 装備名
        public string Name { get; set; } = string.Empty;

        // 除外or固定
        public CludeKind Kind { get; set; }
    }
}
