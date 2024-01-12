using System;

namespace SQLModel
{
    public enum ForeignKeyRule
    {
        Cascade,
        SetNull,
        Restrict,
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
                case ForeignKeyRule.Restrict:
                    return "RESTRICT";
                default:
                    throw new ArgumentOutOfRangeException(nameof(rule), $"Unsupported foreign key rule: {rule}");
            }
        }
    }
}
