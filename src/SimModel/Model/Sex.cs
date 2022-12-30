using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimModel.Model
{
    // 性別
    public enum Sex
    {
        all = 0,
        male = 1,
        female = 2
    }

    public static class SexExt
    {
        // 日本語名を返す
        public static string Str(this Sex kind)
        {
            switch (kind)
            {
                case Sex.male:
                    return "男性";
                case Sex.female:
                    return "女性";
                default:
                    return string.Empty;
            }
        }

        // 日本語名をSex型に変換
        public static Sex StrToSex(this string sex)
        {
            if (Sex.male.Str().Equals(sex))
            {
                return Sex.male;
            }
            if (Sex.female.Str().Equals(sex))
            {
                return Sex.female;
            }
            return Sex.all;
        }
    }
}
