namespace d4rkpl4y3r.Factorio.QualityCalculator;

public class QualitySettings
{
    public required List<(string name, int bonusLevel, double chanceModifier)> QualityLevels { get; set; }
    public double BaseChance { get; set; } = 0.0;
    public double RecyclerReturnRate { get; set; } = 0.25;
    public double ProductivityCap { get; set; } = 4.0;

    public const double SkipChance = 0.1;
    public const double ModuleBonusModifierPerQualityLevel = 0.3;

    public double[] RollQualities(double qualityChance, double productivityBonus, int targetLevel, double[] itemAmounts)
    {
        qualityChance += BaseChance;
        double productivity = Math.Min(1 + productivityBonus, ProductivityCap);
        var newAmounts = new double[QualityLevels.Count];
        for (int i = 0; i < targetLevel; i++)
        {
            double amount = itemAmounts[i] * productivity;
            double chance = QualityLevels[i].chanceModifier * qualityChance;
            // code can't handle greater than 100% chance right now
            if (chance > 1.0)
            {
                throw new InvalidOperationException($"Chance for quality level {QualityLevels[i].name} is {chance*100:F1}%, which is greater than 100%.");
            }
            for (int j = i; j < itemAmounts.Length; j++)
            {
                double skipToNextLevel = amount * ((j == itemAmounts.Length - 1) ? 0 : (j == i) ? chance : QualityLevels[j].chanceModifier * SkipChance);
                newAmounts[j] += amount - skipToNextLevel;
                amount = skipToNextLevel;
            }
        }
        for (int i = targetLevel; i < itemAmounts.Length; i++)
        {
            newAmounts[i] += itemAmounts[i];
        }
        return newAmounts;
    }

    public double[] RollQualities(double qualityChance, double productivityBonus, string targetLevel, double[] itemAmounts)
    {
        int targetIndex = QualityLevels.FindIndex(x => x.name == targetLevel);
        return RollQualities(qualityChance, productivityBonus, targetIndex, itemAmounts);
    }

    public double[] RollQualities(double qualityChance, double productivityBonus, double[] itemAmounts)
    {
        return RollQualities(qualityChance, productivityBonus, itemAmounts.Length - 1, itemAmounts);
    }

    public double[] Recycle(double qualityChance, int targetLevel, double[] itemAmounts)
    {
        return RollQualities(qualityChance, RecyclerReturnRate - 1, targetLevel, itemAmounts);
    }

    public double[] Recycle(double qualityChance, string targetLevel, double[] itemAmounts)
    {
        int targetIndex = QualityLevels.FindIndex(x => x.name == targetLevel);
        return Recycle(qualityChance, targetIndex, itemAmounts);
    }

    public double[] Recycle(double qualityChance, double[] itemAmounts)
    {
        return Recycle(qualityChance, itemAmounts.Length - 1, itemAmounts);
    }

    public (string name, int bonusLevel, double chanceModifier) this[string name]
    {
        get
        {
            return QualityLevels.Find(x => x.name == name);
        }
    }

    public static QualitySettings Vanilla = new QualitySettings
    {
        QualityLevels =
        [
            ("common", 0, 1.0),
            ("uncommon", 1, 1.0),
            ("rare", 2, 1.0),
            ("epic", 3, 1.0),
            ("legendary", 5, 0.0)
        ]
    };

    private const double uncommonUpgradeChance = 1.0;
    private const double upgradeChanceDecay = 0.05;
    public static QualitySettings MyEverythingHasQualityRun = new QualitySettings
    {
        QualityLevels =
        [
            ("common", 0, 1.0),
            ("uncommon", 2, uncommonUpgradeChance),
            ("rare", 3, uncommonUpgradeChance - upgradeChanceDecay * 1),
            ("epic", 4, uncommonUpgradeChance - upgradeChanceDecay * 2),
            ("legendary", 6, uncommonUpgradeChance - upgradeChanceDecay * 3),
            ("unique", 9, uncommonUpgradeChance - upgradeChanceDecay * 4),
            ("mystic", 13, uncommonUpgradeChance - upgradeChanceDecay * 5),
            ("unbreakable", 18, uncommonUpgradeChance - upgradeChanceDecay * 6),
            ("infernal", 24, uncommonUpgradeChance - upgradeChanceDecay * 7),
            ("demonic", 31, uncommonUpgradeChance - upgradeChanceDecay * 8),
            ("godly", 39, 0.0)
        ],
        BaseChance = 0.1,
        RecyclerReturnRate = 0.10,
        ProductivityCap = 10.0
    };
}
