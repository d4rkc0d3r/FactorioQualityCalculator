using System.Globalization;

namespace d4rkpl4y3r.Factorio.QualityCalculator;

public class Program
{
    public static void Main()
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

        Dictionary<int, (double quality, double productivity)> moduleSettings = new()
        {
            { 1, (0.01, 0.04) },
            { 2, (0.02, 0.06) },
            { 3, (0.025, 0.1) },
        };
        Dictionary<string, (int moduleSlots, double productivity)> machineData = new()
        {
            { "assembling-machine-1", (0, 0) },
            { "assembling-machine-2", (2, 0) },
            { "assembling-machine-3", (4, 0) },
            { "oil-refinery", (3, 0) },
            { "chemical-plant", (3, 0) },
            { "centrifuge", (2, 0) },
            { "electrical-furnace", (2, 0) },
            { "drill", (3, 0) },
            { "big-drill", (4, 0) },
            { "foundry", (4, 0.5) },
            { "bio-chamber", (4, 0.5) },
            { "electromagnetic-plant", (5, 0.5) },
            { "cryogenic-plant", (8, 0)},
            
            { "ass1" , (0, 0) },
            { "ass2" , (2, 0) },
            { "ass3" , (4, 0) },
            { "ref", (3, 0) },
            { "chem", (3, 0) },
            { "furnace", (2, 0) },
            { "emp", (5, 0.5) },
            { "cryo", (8, 0) }
        };
        
        #if true
            var settings = QualitySettings.MyEverythingHasQualityRun;
            (string machineName, int moduleTier, string moduleQuality) machineSettings = ("emp", 2, "godly");
            string targetQuality = "godly";
            bool canUseProductivity = true;
        #else
            var settings = QualitySettings.Vanilla;
            (string machineName, int moduleTier, string moduleQuality) machineSettings = ("emp", 3, "legendary");
            string targetQuality = "legendary";
            bool canUseProductivity = true;
        #endif

        Console.WriteLine("Quality levels:");
        for (int i = 0; i < settings.QualityLevels.Count; i++)
        {
            Console.WriteLine($"  {settings.QualityLevels[i].name}: {settings.QualityLevels[i].bonusLevel}, {settings.QualityLevels[i].chanceModifier*100:F1}%");
        }

        double moduleQualityFactor = 1 + settings[machineSettings.moduleQuality].bonusLevel * QualitySettings.ModuleBonusModifierPerQualityLevel;
        var modules = moduleSettings[machineSettings.moduleTier];
        modules.quality = Math.Floor(modules.quality * moduleQualityFactor * 1000) / 1000;
        modules.productivity = Math.Floor(modules.productivity * moduleQualityFactor * 100) / 100;
        Console.WriteLine($"Module quality: {modules.quality*100:F1}%");
        Console.WriteLine($"Module productivity: {modules.productivity*100:F0}%");
        var machine = machineData[machineSettings.machineName];

        int targetQualityIndex = settings.QualityLevels.FindIndex(x => x.name == targetQuality);
        double[] bestItemAmounts = new double[settings.QualityLevels.Count];
        int bestProdModules = 0;
        for (int prodModules = 0; prodModules <= machine.moduleSlots; prodModules++)
        {
            double qualityChance = (machine.moduleSlots - prodModules) * modules.quality;
            double productivityBonus = machine.productivity + prodModules * modules.productivity;
            double[] itemAmounts = new double[settings.QualityLevels.Count];
            itemAmounts[0] = 1.0;

            //itemAmounts = settings.RollQualities(3 * modules.quality, 0 * modules.productivity, itemAmounts);  // mine coal
            //itemAmounts = settings.RollQualities(3 * modules.quality, 0 * modules.productivity, itemAmounts);  // craft plastic
            //itemAmounts = settings.RollQualities(qualityChance, productivityBonus, itemAmounts);               // craft red circuits
            //itemAmounts = settings.RollQualities(qualityChance, productivityBonus, itemAmounts);               // craft module
            itemAmounts = settings.RollQualities(qualityChance, productivityBonus, itemAmounts); // initial craft
            for (int i = 0; i < 1000; i++)
            {
                itemAmounts = settings.Recycle(4 * modules.quality, targetQualityIndex, itemAmounts);
                itemAmounts = settings.RollQualities(qualityChance, productivityBonus, targetQualityIndex, itemAmounts);
            }
            if (itemAmounts[targetQualityIndex] > bestItemAmounts[targetQualityIndex])
            {
                bestItemAmounts = itemAmounts;
                bestProdModules = prodModules;
            }
            if (!canUseProductivity)
            {
                break;
            }
        }
        Console.WriteLine($"Best module config: {machine.moduleSlots - bestProdModules} quality modules, {bestProdModules} productivity modules");
        Console.WriteLine($"  Quality chance: {((machine.moduleSlots - bestProdModules) * modules.quality + settings.BaseChance) * 100:F2}%");
        Console.WriteLine($"  Productivity bonus: {(machine.productivity + bestProdModules * modules.productivity) * 100:F2}%");
        Console.WriteLine($"  Recycler quality chance: {(4 * modules.quality + settings.BaseChance) * 100:F2}% | return rate: {settings.RecyclerReturnRate * 100:F2}%");
        Console.WriteLine($"Resulting item amounts:");

        for (int i = 0; i < bestItemAmounts.Length; i++)
        {
            string rcpValue = bestItemAmounts[i] >= 0.25 || bestItemAmounts[i] <= 1e-50 ? "" : $"  |  ~1/{Math.Round(1/bestItemAmounts[i])}";
            Console.WriteLine($"  {settings.QualityLevels[i].name}: {bestItemAmounts[i]*100:F2}%{rcpValue}");
        }
    }
}