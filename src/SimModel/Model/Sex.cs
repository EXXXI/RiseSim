namespace SimModel.Model
{
    /// <summary>
    /// 性別
    /// </summary>
    public enum Sex
    {
        all = 0,
        male = 1,
        female = 2
    }

    /// <summary>
    /// Sex拡張メソッド用クラス
    /// </summary>
    public static class SexExt
    {
        /// <summary>
        /// 日本語名を返す
        /// </summary>
        /// <param name="kind">Sex</param>
        /// <returns>日本語名</returns>
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

        /// <summary>
        /// 日本語名をSex型に変換
        /// </summary>
        /// <param name="sex">日本語名</param>
        /// <returns>Sex</returns>
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
