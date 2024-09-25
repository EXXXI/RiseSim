namespace SimModel.Model
{
    /// <summary>
    /// 除外・固定情報
    /// </summary>
    public class Clude
    {
        /// <summary>
        /// 装備名
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 除外or固定
        /// </summary>
        public CludeKind Kind { get; set; }
    }
}
