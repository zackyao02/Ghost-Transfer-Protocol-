public sealed class SkillCooldowns
{
    public float GetCooldown(SkillType skill)
    {
        switch (skill)
        {
            case SkillType.SwordQi:
                return 0.5f;
            case SkillType.FireTalisman:
                return 1.5f;
            case SkillType.DeitySummon:
                return 999f;
            default:
                return 0f;
        }
    }
}
