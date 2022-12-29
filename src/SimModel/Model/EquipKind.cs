using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimModel.Model
{
    // 装備種類
    public enum EquipKind
    {
        head,
        body,
        arm,
        waist,
        leg,
        deco,
        charm,
        gskill,
        error
    }
    public static class EquipKindExt
    {
        // 日本語名を返す
        public static string Str(this EquipKind kind)
        {
            switch (kind)
            {
                case EquipKind.head:
                    return "頭";
                case EquipKind.body:
                    return "胴";
                case EquipKind.arm:
                    return "腕";
                case EquipKind.waist:
                    return "腰";
                case EquipKind.leg:
                    return "足";
                case EquipKind.deco:
                    return "装飾品";
                case EquipKind.charm:
                    return "護石";
                case EquipKind.gskill:
                    return "錬成";
                default:
                    return string.Empty;
            }
        }

        // 日本語名と区切り用のコロンを返す
        public static string StrWithColon(this EquipKind kind)
        {
            return Str(kind) + '：';
        }

        // 文字列をEquipKindに
        public static EquipKind ToEquipKind(this string? str)
        {
            return str switch
            {
                "頭" => EquipKind.head,
                "胴" => EquipKind.body,
                "腕" => EquipKind.arm,
                "腰" => EquipKind.waist,
                "足" => EquipKind.leg,
                "脚" => EquipKind.leg,// 誤記
                _ => EquipKind.error,
            };
        }
    }
}
