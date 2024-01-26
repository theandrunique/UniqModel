using System;

namespace UniqModel
{
    public enum ForeignKeyRule
    {
        Cascade,
        SetNull,
        NoAction,
    }

    internal static class ForeignKeyRuleMapper
    {
        public static string MapToSql(ForeignKeyRule rule)
        {
            switch (rule)
            {
                case ForeignKeyRule.Cascade:
                    return "CASCADE";
                case ForeignKeyRule.SetNull:
                    return "SET NULL";
                case ForeignKeyRule.NoAction:
                    return "NO ACTION";
                default:
                    throw new ArgumentOutOfRangeException(nameof(rule), $"Unsupported foreign key rule: {rule}");
            }
        }
    }
}
