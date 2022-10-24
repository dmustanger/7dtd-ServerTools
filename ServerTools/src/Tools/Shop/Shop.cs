using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEngine;

namespace ServerTools
{
    class Shop
    {
        public static bool IsEnabled = false, IsRunning = false, Inside_Market = false, Inside_Traders = false,
            Panel= false;
        public static int Delay_Between_Uses = 60;
        public static string Command_shop = "shop", Command_shop_buy = "shop buy", 
            Panel_Name = "Super Shop", PanelItems = "", CategoryString = "";

        public static Dictionary<string, int> PanelAccess = new Dictionary<string, int>();
        public static List<string[]> Dict = new List<string[]>();
        public static List<string> Categories = new List<string>();

        private static string LogFilePath = string.Format("{0}/Logs/ShopLogs/Shop_{1}.txt", API.ConfigPath, DateTime.Today.ToString("M-d-yyyy:hh:mm:ss"));
        private static string FilePath = string.Format("{0}/{1}", API.ConfigPath, "Shop.xml");
        private static FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, "Shop.xml");

        private static XmlNodeList OldNodeList;

        public static void Load()
        {
            LoadXml();
            InitFileWatcher();
        }

        public static void Unload()
        {
            Dict.Clear();
            Categories.Clear();
            PanelItems = "";
            CategoryString = "";
            FileWatcher.Dispose();
            IsRunning = false;
        }

        public static void LoadXml()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    UpdateXml();
                }
                XmlDocument xmlDoc = new XmlDocument();
                try
                {
                    xmlDoc.Load(FilePath);
                }
                catch (XmlException e)
                {
                    Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", "Shop.xml", e.Message));
                    return;
                }
                bool upgrade = true;
                XmlNodeList childNodes = xmlDoc.DocumentElement.ChildNodes;
                if (childNodes != null)
                {
                    Dict.Clear();
                    Categories.Clear();
                    PanelItems = "";
                    CategoryString = "";
                    string qualityCategories = "";
                    Dictionary<string, int> itemList = new Dictionary<string, int>();
                    List<string[]> shopLog = PersistentContainer.Instance.ShopLog;
                    for (int i = 0; i < childNodes.Count; i++)
                    {
                        if (childNodes[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement line = (XmlElement)childNodes[i];
                            if (line.HasAttributes)
                            {
                                if (line.HasAttribute("Version") && line.GetAttribute("Version") == Config.Version)
                                {
                                    upgrade = false;
                                    continue;
                                }
                                else if (line.HasAttribute("Name") && line.HasAttribute("Count") && line.HasAttribute("Quality") &&
                                    line.HasAttribute("Price") && line.HasAttribute("Category") && line.HasAttribute("PanelMessage"))
                                {
                                    int count = int.Parse(line.GetAttribute("Count"));
                                    int quality = int.Parse(line.GetAttribute("Quality"));
                                    int price = int.Parse(line.GetAttribute("Price"));
                                    string name = line.GetAttribute("Name");
                                    string secondaryname;
                                    if (line.HasAttribute("SecondaryName"))
                                    {
                                        secondaryname = line.GetAttribute("SecondaryName");
                                    }
                                    else
                                    {
                                        secondaryname = name;
                                    }
                                    ItemValue itemValue = ItemClass.GetItem(name, false);
                                    if (itemValue.type == ItemValue.None.type)
                                    {
                                        Log.Out(string.Format("[SERVERTOOLS] Ignoring Shop.xml entry. Item could not be found: {0}", name));
                                        continue;
                                    }
                                    name = itemValue.ItemClass.Name;
                                    if (count > itemValue.ItemClass.Stacknumber.Value)
                                    {
                                        count = itemValue.ItemClass.Stacknumber.Value;
                                    }
                                    if (quality < 1)
                                    {
                                        quality = 1;
                                    }
                                    if (itemValue.HasQuality)
                                    {
                                        itemValue.Quality = quality;
                                    }
                                    string category = line.GetAttribute("Category").ToLower();
                                    string panelMessage = line.GetAttribute("PanelMessage");
                                    int id = Dict.Count;
                                    string[] item = new string[] { id.ToString(), name, secondaryname, count.ToString(), quality.ToString(), price.ToString(), category, panelMessage };
                                    if (!Dict.Contains(item))
                                    {
                                        if (category.Contains(","))
                                        {
                                            string[] categories = category.Split(',');
                                            for (int j = 0; j < categories.Length; j++)
                                            {
                                                if (!Categories.Contains(categories[j]))
                                                {
                                                    Categories.Add(categories[j]);
                                                    CategoryString += categories[j] + "§";
                                                }
                                            }
                                        }
                                        else if (!Categories.Contains(category))
                                        {
                                            Categories.Add(category);
                                            CategoryString += category + "§";
                                        }
                                        if (!qualityCategories.Contains("Quality:" + quality.ToString()))
                                        {
                                            qualityCategories += "Quality:" + quality.ToString() + "§";
                                        }
                                        Dict.Add(item);
                                        string stats = "";
                                        switch (itemValue.ItemClass.DisplayType)
                                        {
                                            case "rangedGunNoMag":
                                            int rangedDamage1 = (int)EffectManager.GetValue(PassiveEffects.EntityDamage, itemValue, quality - 1);
                                            if (rangedDamage1 != 0)
                                            {
                                                if (stats.Length > 0)
                                                {
                                                    stats += " </br> ";
                                                }
                                                stats += "Ranged Damage: " + rangedDamage1;
                                            }
                                            int magazineSize1 = (int)EffectManager.GetValue(PassiveEffects.MagazineSize, itemValue, quality - 1);
                                            if (magazineSize1 != 0)
                                            {
                                                if (stats.Length > 0)
                                                {
                                                    stats += " </br> ";
                                                }
                                                stats += "Magazine Size: " + magazineSize1;
                                            }
                                            int range1 = (int)EffectManager.GetValue(PassiveEffects.MaxRange, itemValue, quality - 1);
                                            if (range1 != 0)
                                            {
                                                if (stats.Length > 0)
                                                {
                                                    stats += " </br> ";
                                                }
                                                stats += "Effective Range: " + range1;
                                            }
                                            int durability1 = (int)EffectManager.GetValue(PassiveEffects.DegradationMax, itemValue, quality - 1);
                                            if (durability1 != 0)
                                            {
                                                if (stats.Length > 0)
                                                {
                                                    stats += " </br> ";
                                                }
                                                stats += "Max Durability: " + durability1;
                                            }
                                                break;
                                            case "rangedGun":
                                                int rangedDamage2 = (int)EffectManager.GetValue(PassiveEffects.EntityDamage, itemValue, quality - 1);
                                                if (rangedDamage2 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Ranged Damage: " + rangedDamage2;
                                                }
                                                int magazineSize2 = (int)EffectManager.GetValue(PassiveEffects.MagazineSize, itemValue, quality - 1);
                                                if (magazineSize2 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Magazine Size: " + magazineSize2;
                                                }
                                                int roundsPerMin2 = (int)EffectManager.GetValue(PassiveEffects.RoundsPerMinute, itemValue, quality - 1);
                                                if (roundsPerMin2 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Rounds Per Minute: " + roundsPerMin2;
                                                }
                                                int range2 = (int)EffectManager.GetValue(PassiveEffects.MaxRange, itemValue, quality - 1);
                                                if (range2 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Effective Range: " + range2;
                                                }
                                                int durability2 = (int)EffectManager.GetValue(PassiveEffects.DegradationMax, itemValue, quality - 1);
                                                if (durability2 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Max Durability: " + durability2;
                                                }
                                                break;
                                            case "rangedShotgunNoMag":
                                                int rangedDamage3 = (int)EffectManager.GetValue(PassiveEffects.EntityDamage, itemValue, quality - 1);
                                                if (rangedDamage3 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Ranged Damage: " + rangedDamage3;
                                                }
                                                int pellets3 = (int)EffectManager.GetValue(PassiveEffects.RoundRayCount, itemValue, quality - 1);
                                                if (pellets3 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Pellets: " + pellets3;
                                                }
                                                int magazineSize3 = (int)EffectManager.GetValue(PassiveEffects.MagazineSize, itemValue, quality - 1);
                                                if (magazineSize3 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Magazine Size: " + magazineSize3;
                                                }
                                                int range3 = (int)EffectManager.GetValue(PassiveEffects.MaxRange, itemValue, quality - 1);
                                                if (range3 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Effective Range: " + range3;
                                                }
                                                int durability3 = (int)EffectManager.GetValue(PassiveEffects.DegradationMax, itemValue, quality - 1);
                                                if (durability3 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Max Durability: " + durability3;
                                                }
                                                break;
                                            case "rangedShotgun":
                                                int rangedDamage4 = (int)EffectManager.GetValue(PassiveEffects.EntityDamage, itemValue, quality - 1);
                                                if (rangedDamage4 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Ranged Damage: " + rangedDamage4;
                                                }
                                                int pellets4 = (int)EffectManager.GetValue(PassiveEffects.RoundRayCount, itemValue, quality - 1);
                                                if (pellets4 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Pellets: " + pellets4;
                                                }
                                                int magazineSize4 = (int)EffectManager.GetValue(PassiveEffects.MagazineSize, itemValue, quality - 1);
                                                if (magazineSize4 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Magazine Size: " + magazineSize4;
                                                }
                                                int roundsPerMin4 = (int)EffectManager.GetValue(PassiveEffects.RoundsPerMinute, itemValue, quality - 1);
                                                if (roundsPerMin4 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Rounds Per Minute: " + roundsPerMin4;
                                                }
                                                int range4 = (int)EffectManager.GetValue(PassiveEffects.MaxRange, itemValue, quality - 1);
                                                if (range4 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Effective Range: " + range4;
                                                }
                                                int durability4 = (int)EffectManager.GetValue(PassiveEffects.DegradationMax, itemValue, quality - 1);
                                                if (durability4 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Max Durability: " + durability4;
                                                }
                                                break;
                                            case "meleeRepairTool":
                                                int meleeDamage5 = (int)EffectManager.GetValue(PassiveEffects.EntityDamage, itemValue, quality - 1);
                                                if (meleeDamage5 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Melee Damage: " + meleeDamage5;
                                                }
                                                int repairAmount5 = (int)EffectManager.GetValue(PassiveEffects.RepairAmount, itemValue, quality - 1);
                                                if (repairAmount5 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Repair Amount: " + repairAmount5;
                                                }
                                                int blockDamage5 = (int)EffectManager.GetValue(PassiveEffects.BlockDamage, itemValue, quality - 1);
                                                if (blockDamage5 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Block Damage: " + blockDamage5;
                                                }
                                                int stamina5 = (int)EffectManager.GetValue(PassiveEffects.StaminaLoss, itemValue, quality - 1);
                                                if (stamina5 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Stamina Cost: " + stamina5;
                                                }
                                                int attacksPerMin5 = (int)EffectManager.GetValue(PassiveEffects.AttacksPerMinute, itemValue, quality - 1);
                                                if (attacksPerMin5 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Attacks Per Min: " + attacksPerMin5;
                                                }
                                                int durability5 = (int)EffectManager.GetValue(PassiveEffects.DegradationMax, itemValue, quality - 1);
                                                if (durability5 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Max Durability: " + durability5;
                                                }
                                                break;
                                            case "rangedRepairTool":
                                                int meleeDamage6 = (int)EffectManager.GetValue(PassiveEffects.EntityDamage, itemValue, quality - 1);
                                                if (meleeDamage6 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Melee Damage: " + meleeDamage6;
                                                }
                                                int repairAmount6 = (int)EffectManager.GetValue(PassiveEffects.RepairAmount, itemValue, quality - 1);
                                                if (repairAmount6 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Repair Amount: " + repairAmount6;
                                                }
                                                int blockDamage6 = (int)EffectManager.GetValue(PassiveEffects.BlockDamage, itemValue, quality - 1);
                                                if (blockDamage6 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Block Damage: " + blockDamage6;
                                                }
                                                int range6 = (int)EffectManager.GetValue(PassiveEffects.MaxRange, itemValue, quality - 1);
                                                if (range6 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Effective Range: " + range6;
                                                }
                                                int attacksPerMin6 = (int)EffectManager.GetValue(PassiveEffects.AttacksPerMinute, itemValue, quality - 1);
                                                if (attacksPerMin6 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Attacks Per Min: " + attacksPerMin6;
                                                }
                                                int durability6 = (int)EffectManager.GetValue(PassiveEffects.DegradationMax, itemValue, quality - 1);
                                                if (durability6 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Max Durability: " + durability6;
                                                }
                                                break;
                                            case "melee":
                                                int meleeDamage7 = (int)EffectManager.GetValue(PassiveEffects.EntityDamage, itemValue, quality - 1);
                                                if (meleeDamage7 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Melee Damage: " + meleeDamage7;
                                                }
                                                int bonusDamage7 = (int)EffectManager.GetValue(PassiveEffects.DamageBonus, itemValue, quality - 1);
                                                if (bonusDamage7 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Power Attack Damage: " + bonusDamage7;
                                                }
                                                int blockDamage7 = (int)EffectManager.GetValue(PassiveEffects.BlockDamage, itemValue, quality - 1);
                                                if (blockDamage7 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Block Damage: " + blockDamage7;
                                                }
                                                int stamina7 = (int)EffectManager.GetValue(PassiveEffects.StaminaLoss, itemValue, quality - 1);
                                                if (stamina7 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Stamina Cost: " + stamina7;
                                                }
                                                int attacksPerMin7 = (int)EffectManager.GetValue(PassiveEffects.AttacksPerMinute, itemValue, quality - 1);
                                                if (attacksPerMin7 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Attacks Per Min: " + attacksPerMin7;
                                                }
                                                int durability7 = (int)EffectManager.GetValue(PassiveEffects.DegradationMax, itemValue, quality - 1);
                                                if (durability7 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Max Durability: " + durability7;
                                                }
                                                break;
                                            case "motorTool":
                                                int meleeDamage8 = (int)EffectManager.GetValue(PassiveEffects.EntityDamage, itemValue, quality - 1);
                                                if (meleeDamage8 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Melee Damage: " + meleeDamage8;
                                                }
                                                int blockDamage8 = (int)EffectManager.GetValue(PassiveEffects.BlockDamage, itemValue, quality - 1);
                                                if (blockDamage8 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Block Damage: " + blockDamage8;
                                                }
                                                int attacksPerMin8 = (int)EffectManager.GetValue(PassiveEffects.AttacksPerMinute, itemValue, quality - 1);
                                                if (attacksPerMin8 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Attacks Per Min: " + attacksPerMin8;
                                                }
                                                int durability8 = (int)EffectManager.GetValue(PassiveEffects.DegradationMax, itemValue, quality - 1);
                                                if (durability8 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Max Durability: " + durability8;
                                                }
                                                break;
                                            case "meleeSpear":
                                                int meleeDamage9 = (int)EffectManager.GetValue(PassiveEffects.EntityDamage, itemValue, quality - 1);
                                                if (meleeDamage9 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Melee Damage: " + meleeDamage9;
                                                }
                                                int bonusDamage9 = (int)EffectManager.GetValue(PassiveEffects.DamageBonus, itemValue, quality - 1);
                                                if (bonusDamage9 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Power Attack Damage: " + bonusDamage9;
                                                }
                                                int targetArmor = (int)EffectManager.GetValue(PassiveEffects.TargetArmor, itemValue, quality - 1);
                                                if (targetArmor != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Target Armor: " + targetArmor;
                                                }
                                                int blockDamage9 = (int)EffectManager.GetValue(PassiveEffects.BlockDamage, itemValue, quality - 1);
                                                if (blockDamage9 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Block Damage: " + blockDamage9;
                                                }
                                                int stamina9 = (int)EffectManager.GetValue(PassiveEffects.StaminaLoss, itemValue, quality - 1);
                                                if (stamina9 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Stamina Cost: " + stamina9;
                                                }
                                                int attacksPerMin9 = (int)EffectManager.GetValue(PassiveEffects.AttacksPerMinute, itemValue, quality - 1);
                                                if (attacksPerMin9 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Attacks Per Min: " + attacksPerMin9;
                                                }
                                                int durability9 = (int)EffectManager.GetValue(PassiveEffects.DegradationMax, itemValue, quality - 1);
                                                if (durability9 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Max Durability: " + durability9;
                                                }
                                                break;
                                            case "meleeHeavy":
                                                int meleeDamage10 = (int)EffectManager.GetValue(PassiveEffects.EntityDamage, itemValue, quality - 1);
                                                if (meleeDamage10 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Melee Damage: " + meleeDamage10;
                                                }
                                                int bonusDamage10 = (int)EffectManager.GetValue(PassiveEffects.DamageBonus, itemValue, quality - 1);
                                                if (bonusDamage10 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Power Attack Damage: " + bonusDamage10;
                                                }
                                                int blockDamage10 = (int)EffectManager.GetValue(PassiveEffects.BlockDamage, itemValue, quality - 1);
                                                if (blockDamage10 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Block Damage: " + blockDamage10;
                                                }
                                                int stamina10 = (int)EffectManager.GetValue(PassiveEffects.StaminaLoss, itemValue, quality - 1);
                                                if (stamina10 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Stamina Cost: " + stamina10;
                                                }
                                                int attacksPerMin10 = (int)EffectManager.GetValue(PassiveEffects.AttacksPerMinute, itemValue, quality - 1);
                                                if (attacksPerMin10 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Attacks Per Min: " + attacksPerMin10;
                                                }
                                                int durability10 = (int)EffectManager.GetValue(PassiveEffects.DegradationMax, itemValue, quality - 1);
                                                if (durability10 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Max Durability: " + durability10;
                                                }
                                                break;
                                            case "rangedBow":
                                                int meleeDamage11 = (int)EffectManager.GetValue(PassiveEffects.EntityDamage, itemValue, quality - 1);
                                                if (meleeDamage11 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Ranged Damage: " + meleeDamage11;
                                                }
                                                int projectileVelocity11 = (int)EffectManager.GetValue(PassiveEffects.ProjectileVelocity, itemValue, quality - 1);
                                                if (projectileVelocity11 > 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Projectile Velocity: " + projectileVelocity11;
                                                }
                                                int durability11 = (int)EffectManager.GetValue(PassiveEffects.DegradationMax, itemValue, quality - 1);
                                                if (durability11 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Max Durability: " + durability11;
                                                }
                                                break;
                                            case "meleeTurret":
                                                int meleeDamage12 = (int)EffectManager.GetValue(PassiveEffects.EntityDamage, itemValue, quality - 1);
                                                if (meleeDamage12 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Melee Damage: " + meleeDamage12;
                                                }
                                                int blockDamage12 = (int)EffectManager.GetValue(PassiveEffects.BlockDamage, itemValue, quality - 1);
                                                if (blockDamage12 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Block Damage: " + blockDamage12;
                                                }
                                                int attacksPerMin12 = (int)EffectManager.GetValue(PassiveEffects.AttacksPerMinute, itemValue, quality - 1);
                                                if (attacksPerMin12 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Attacks Per Min: " + attacksPerMin12;
                                                }
                                                int durability12 = (int)EffectManager.GetValue(PassiveEffects.DegradationMax, itemValue, quality - 1);
                                                if (durability12 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Health/Max Durability: " + durability12;
                                                }
                                                break;
                                            case "armorLight":
                                                int damageResistence13 = (int)EffectManager.GetValue(PassiveEffects.PhysicalDamageResist, itemValue, 0f);
                                                if (damageResistence13 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Light Armor Rating: " + damageResistence13;
                                                }
                                                int hypothermalResistence13 = (int)EffectManager.GetValue(PassiveEffects.HypothermalResist, itemValue, 0f);
                                                if (hypothermalResistence13 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Hypothermal Resistance: " + hypothermalResistence13;
                                                }
                                                break;
                                            case "armorHeavy":
                                                int damageResistence14 = (int)EffectManager.GetValue(PassiveEffects.PhysicalDamageResist, itemValue, 0f);
                                                if (damageResistence14 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Heavy Armor Rating: " + damageResistence14;
                                                }
                                                int hypothermalResistence14 = (int)EffectManager.GetValue(PassiveEffects.HypothermalResist, itemValue, 0f);
                                                if (hypothermalResistence14 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Hypothermal Resistance: " + hypothermalResistence14;
                                                }
                                                break;
                                            case "ammoLauncher":
                                                int explosionDamage15 = (int)EffectManager.GetValue(PassiveEffects.EntityDamage, itemValue, quality - 1);
                                                if (explosionDamage15 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Explosion Damage: " + explosionDamage15;
                                                }
                                                int blockDamage15 = (int)EffectManager.GetValue(PassiveEffects.BlockDamage, itemValue, quality - 1);
                                                if (blockDamage15 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Block Damage: " + blockDamage15;
                                                }
                                                int explosionRadius15 = (int)EffectManager.GetValue(PassiveEffects.ExplosionRadius, itemValue, quality - 1);
                                                if (explosionRadius15 != 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Explosion Radius: " + explosionRadius15;
                                                }
                                                break;
                                            case "medical":
                                                int healthUp16 = (int)EffectManager.GetValue(PassiveEffects.HealthGain, itemValue, quality - 1);
                                                if (healthUp16 > 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Health Gain: " + healthUp16;
                                                }
                                                int healthDown16 = (int)EffectManager.GetValue(PassiveEffects.HealthLoss, itemValue, quality - 1);
                                                if (healthDown16 < 0)
                                                {
                                                    if (stats.Length > 0)
                                                    {
                                                        stats += " </br> ";
                                                    }
                                                    stats += "Health Loss: " + healthDown16;
                                                }
                                                break;
                                        }
                                        if (itemValue.ItemClass.GetItemName().ToLower().Contains("arrow") || itemValue.ItemClass.DisplayType.ToLower().Contains("arrow"))
                                        {
                                            int meleeDamage = (int)EffectManager.GetValue(PassiveEffects.EntityDamage, itemValue, quality - 1);
                                            if (meleeDamage != 0)
                                            {
                                                if (stats.Length > 0)
                                                {
                                                    stats += " </br> ";
                                                }
                                                stats += "Ranged Damage: " + meleeDamage;
                                            }
                                            int blockDamage = (int)EffectManager.GetValue(PassiveEffects.BlockDamage, itemValue, quality - 1);
                                            if (blockDamage != 0)
                                            {
                                                if (stats.Length > 0)
                                                {
                                                    stats += " </br> ";
                                                }
                                                stats += "Block Damage: " + blockDamage;
                                            }
                                            int projectileVelocity = (int)EffectManager.GetValue(PassiveEffects.ProjectileVelocity, itemValue, quality - 1);
                                            if (projectileVelocity > 0)
                                            {
                                                if (stats.Length > 0)
                                                {
                                                    stats += " </br> ";
                                                }
                                                stats += "Projectile Velocity: " + projectileVelocity;
                                            }
                                        }
                                        else if (itemValue.ItemClass.GetItemName().ToLower().Contains("food") || itemValue.ItemClass.DisplayType.ToLower().Contains("food"))
                                        {
                                            int foodUp = (int)EffectManager.GetValue(PassiveEffects.FoodGain, itemValue, quality - 1);
                                            if (foodUp > 0)
                                            {
                                                if (stats.Length > 0)
                                                {
                                                    stats += " </br> ";
                                                }
                                                stats += "Food: " + foodUp;
                                            }
                                            int foodDown = (int)EffectManager.GetValue(PassiveEffects.FoodLoss, itemValue, quality - 1);
                                            if (foodDown < 0)
                                            {
                                                if (stats.Length > 0)
                                                {
                                                    stats += " </br> ";
                                                }
                                                stats += "Food: " + foodDown;
                                            }
                                            int healthUp = (int)EffectManager.GetValue(PassiveEffects.HealthGain, itemValue, quality - 1);
                                            if (healthUp > 0)
                                            {
                                                if (stats.Length > 0)
                                                {
                                                    stats += " </br> ";
                                                }
                                                stats += "Health: " + healthUp;
                                            }
                                            int healthDown = (int)EffectManager.GetValue(PassiveEffects.HealthLoss, itemValue, quality - 1);
                                            if (healthDown < 0)
                                            {
                                                if (stats.Length > 0)
                                                {
                                                    stats += " </br> ";
                                                }
                                                stats += "Health: " + healthDown;
                                            }
                                            int waterUp = (int)EffectManager.GetValue(PassiveEffects.WaterGain, itemValue, quality - 1);
                                            if (waterUp > 0)
                                            {
                                                if (stats.Length > 0)
                                                {
                                                    stats += " </br> ";
                                                }
                                                stats += "Water: " + waterUp;
                                            }
                                            int waterDown = (int)EffectManager.GetValue(PassiveEffects.WaterLoss, itemValue, quality - 1);
                                            if (waterDown < 0)
                                            {
                                                if (stats.Length > 0)
                                                {
                                                    stats += " </br> ";
                                                }
                                                stats += "Water: " + waterDown;
                                            }
                                            int staminaMax = (int)EffectManager.GetValue(PassiveEffects.StaminaMax, itemValue, quality - 1);
                                            if (staminaMax != 0)
                                            {
                                                if (stats.Length > 0)
                                                {
                                                    stats += " </br> ";
                                                }
                                                stats += "Max Stamina Bonus: " + staminaMax;
                                            }
                                        }

                                        if (stats.Length > 0)
                                        {
                                            stats += " </br> ";
                                        }
                                        stats += "Price: " + price + " </br> ";
                                        string sold = "Sold: 0";
                                        if (shopLog.Count > 0)
                                        {
                                            int sellCount = 0;
                                            for (int j = 0; j < shopLog.Count; j++)
                                            {
                                                if (shopLog[j].Contains(name))
                                                {
                                                    sellCount += 1;
                                                }
                                            }
                                            if (sellCount > 0)
                                            {
                                                sold = "Sold: " + sellCount;
                                            }
                                        }
                                        stats += sold;
                                        itemList.Add(category + "§" + name + "§" + secondaryname + "§" + itemValue.ItemClass.GetIconName() + "§" + count + "§" +
                                            quality + "§" + price + "§" + stats + "§" + id + "§" + panelMessage + "╚", price);
                                    }
                                }
                                else
                                {
                                    Log.Out(string.Format("[SERVERTOOLS] Shop.xml entry has missing data. 'Item Name=\"\" SecondaryName=\"\" Count=\"\" Quality=\"\" Price=\"\" Category=\"\" PanelMessage=\"\"'"));
                                }
                            }
                        }
                    }
                    foreach (var item in itemList.OrderBy(key => key.Value))
                    {
                        PanelItems += item.Key;
                    }
                    if (PanelItems.Length > 0)
                    {
                        PanelItems = PanelItems.Remove(PanelItems.Length - 1);
                    }
                    if (qualityCategories.Length > 0)
                    {
                        qualityCategories = qualityCategories.Remove(qualityCategories.Length - 1);
                        CategoryString += qualityCategories;
                    }
                    else if (CategoryString.Length > 0)
                    {
                        CategoryString = CategoryString.Remove(CategoryString.Length - 1);
                    }
                }
                if (upgrade)
                {
                    XmlNodeList nodeList = xmlDoc.DocumentElement.ChildNodes;
                    XmlNode node = nodeList[0];
                    XmlElement line = (XmlElement)nodeList[0];
                    if (line != null)
                    {
                        if (line.HasAttributes)
                        {
                            OldNodeList = nodeList;
                            File.Delete(FilePath);
                            UpgradeXml();
                            return;
                        }
                        else
                        {
                            nodeList = node.ChildNodes;
                            line = (XmlElement)nodeList[0];
                            if (line != null)
                            {
                                if (line.HasAttributes)
                                {
                                    OldNodeList = nodeList;
                                    File.Delete(FilePath);
                                    UpgradeXml();
                                    return;
                                }
                            }
                            File.Delete(FilePath);
                            UpdateXml();
                            Log.Out(string.Format("[SERVERTOOLS] The existing Shop.xml was too old or misconfigured. File deleted and rebuilt for version {0}", Config.Version));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message == "Specified cast is not valid.")
                {
                    File.Delete(FilePath);
                    UpdateXml();
                }
                else
                {
                    Log.Out(string.Format("[SERVERTOOLS] Error in Shop.LoadXml: {0}", e.Message));
                }
            }
        }

        public static void UpdateXml()
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<Shop>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("    <!-- Secondary name is what will show in chat instead of the item name -->");
                    sw.WriteLine("    <!-- Items with no quality should be set to 0 -->");
                    sw.WriteLine("    <!-- <Item Name=\"\" SecondaryName=\"\" Count=\"\" Quality=\"\" Price=\"\" Category=\"\" PanelMessage=\"\" /> -->");
                    sw.WriteLine();
                    sw.WriteLine();
                    if (Dict.Count > 0)
                    {
                        for (int i = 0; i < Dict.Count; i++)
                        {
                            sw.WriteLine(string.Format("    <Item Name=\"{0}\" SecondaryName=\"{1}\" Count=\"{2}\" Quality=\"{3}\" Price=\"{4}\" Category=\"{5}\" PanelMessage=\"{6}\" />", Dict[i][1], Dict[i][2], Dict[i][3], Dict[i][4], Dict[i][5], Dict[i][6], Dict[i][7]));
                        }
                    }
                    sw.WriteLine("</Shop>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Shop.UpdateXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
        }

        private static void InitFileWatcher()
        {
            FileWatcher.Changed += new FileSystemEventHandler(OnFileChanged);
            FileWatcher.Created += new FileSystemEventHandler(OnFileChanged);
            FileWatcher.Deleted += new FileSystemEventHandler(OnFileChanged);
            FileWatcher.EnableRaisingEvents = true;
            IsRunning = true;
        }

        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            if (!File.Exists(FilePath))
            {
                UpdateXml();
            }
            LoadXml();
        }

        public static void SetLink()
        {
            try
            {
                if (File.Exists(GeneralFunction.XPathDir + "XUi/windows.xml"))
                {
                    string link = string.Format("http://{0}:{1}/shop.html", WebAPI.BaseAddress, WebAPI.Port);
                    List<string> lines = File.ReadAllLines(GeneralFunction.XPathDir + "XUi/windows.xml").ToList();
                    for (int i = 0; i < lines.Count; i++)
                    {
                        if (lines[i].Contains("browserShop"))
                        {
                            if (!lines[i + 7].Contains(link))
                            {
                                lines[i + 7] = string.Format("          <label depth=\"2\" pos=\"0,-40\" height=\"32\" width=\"200\" name=\"ServerWebsiteURL\" text=\"{0}\" justify=\"center\" style=\"press,hover\" font_size=\"1\" upper_case=\"false\" sound=\"[paging_click]\" />", link);
                                File.WriteAllLines(GeneralFunction.XPathDir + "XUi/windows.xml", lines.ToArray());
                            }
                            return;
                        }
                    }
                    for (int i = 0; i < lines.Count; i++)
                    {
                        if (lines[i].Contains("/append"))
                        {
                            lines.RemoveRange(i, 3);
                            lines.Add("  <window name=\"browserShop\" controller=\"ServerInfo\">");
                            lines.Add("      <panel name=\"header\" pos=\"-300,0\" height=\"40\" depth=\"1\" backgroundspritename=\"ui_game_panel_header\" >");
                            lines.Add("          <label style=\"header.name\" pos=\"0,0\" width=\"197\" justify=\"center\" text=\"Shop\" />");
                            lines.Add("      </panel>");
                            lines.Add("      <panel name=\"\" pos=\"-300,0\" height=\"63\">");
                            lines.Add("          <sprite depth=\"5\" pos=\"0,0\" height=\"33\" width=\"200\" name=\"background\" color=\"[darkGrey]\" type=\"sliced\" />");
                            lines.Add("          <label name=\"ServerDescription\" />");
                            lines.Add(string.Format("          <label depth=\"2\" pos=\"0,-40\" height=\"32\" width=\"200\" name=\"ServerWebsiteURL\" text=\"{0}\" justify=\"center\" style=\"press,hover\" font_size=\"1\" upper_case=\"false\" sound=\"[paging_click]\" />", link));
                            lines.Add("          <sprite depth=\"3\" pos=\"0,-40\" height=\"32\" width=\"200\" name=\"URLMask\" color=\"[white]\" foregroundlayer=\"true\" fillcenter=\"true\" />");
                            lines.Add("          <sprite depth=\"4\" name=\"shoppingCartIcon\" style=\"icon30px\" pos=\"5,-40\" color=\"[black]\" sprite=\"ui_game_symbol_shopping_cart\" />");
                            lines.Add("          <label depth=\"4\" style=\"header.name\" pos=\"0,-40\" height=\"32\" width=\"200\" justify=\"center\" color=\"[black]\" text=\"Click Here\" />");
                            lines.Add("          <!-- Change the text IP and Port to the one needed by ServerTools web api -->");
                            lines.Add("      </panel>");
                            lines.Add("  </window>");
                            lines.Add("");
                            lines.Add("</append>");
                            lines.Add("");
                            lines.Add("</configs>");
                            File.WriteAllLines(GeneralFunction.XPathDir + "XUi/windows.xml", lines.ToArray());
                            return;
                        }
                    }
                }
            }
            catch (XmlException e)
            {
                Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", GeneralFunction.XPathDir + "XUi/windows.xml", e.Message));
            }
        }

        public static void PosCheck(ClientInfo _cInfo, string _categoryOrItem, int _form, int _count)
        {
            try
            {
                if (Dict.Count > 0)
                {
                    EntityPlayer player = GeneralFunction.GetEntityPlayer(_cInfo.entityId);
                    if (player != null)
                    {
                        if (Market.IsEnabled && Inside_Market)
                        {
                            if (Inside_Traders)
                            {
                                string[] cords = Market.Market_Position.Split(',');
                                int.TryParse(cords[0], out int x);
                                int.TryParse(cords[1], out int y);
                                int.TryParse(cords[2], out int z);
                                if (Market.IsMarket(player.position) && GameManager.Instance.World.IsWithinTraderArea(new Vector3i(player.position.x, player.position.y, player.position.z)))
                                {
                                    Form(_cInfo, _categoryOrItem, _form, _count);
                                }
                                else
                                {
                                    Phrases.Dict.TryGetValue("Shop9", out string _phrase);
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                            else
                            {
                                string[] cords = Market.Market_Position.Split(',');
                                int.TryParse(cords[0], out int x);
                                int.TryParse(cords[1], out int y);
                                int.TryParse(cords[2], out int z);
                                if (Market.IsMarket(player.position))
                                {
                                    Form(_cInfo, _categoryOrItem, _form, _count);
                                }
                                else
                                {
                                    Phrases.Dict.TryGetValue("Shop10", out string phrase);
                                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                                }
                            }
                        }
                        else if (Inside_Traders)
                        {
                            string[] cords = Market.Market_Position.Split(',');
                            int.TryParse(cords[0], out int x);
                            int.TryParse(cords[1], out int y);
                            int.TryParse(cords[2], out int z);
                            if (GameManager.Instance.World.IsWithinTraderArea(new Vector3i(player.position.x, player.position.y, player.position.z)))
                            {
                                Form(_cInfo, _categoryOrItem, _form, _count);
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue("Shop9", out string phrase);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                        else
                        {
                            Form(_cInfo, _categoryOrItem, _form, _count);
                        }
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("Shop8", out string _phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + _phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Shop.PosCheck: {0}", e.Message));
            }
        }

        public static void Form(ClientInfo _cInfo, string _categoryOrItem, int _form, int _count)
        {
            if (_form == 1)
            {
                ListCategories(_cInfo);
            }
            else if (_form == 2)
            {
                ShowCategory(_cInfo, _categoryOrItem);
            }
            else if (int.TryParse(_categoryOrItem, out int _id))
            {
                Walletcheck(_cInfo, _id, _count);
            }
        }

        public static void ListCategories(ClientInfo _cInfo)
        {
            try
            {
                if (Panel && WebAPI.IsEnabled && WebAPI.Connected && !PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Overlay)
                {
                    string ip = _cInfo.ip;
                    bool duplicate = false;
                    List<ClientInfo> clientList = GeneralFunction.ClientList();
                    if (clientList != null && clientList.Count > 1)
                    {
                        for (int i = 0; i < clientList.Count; i++)
                        {
                            ClientInfo cInfo = clientList[i];
                            if (cInfo != null && cInfo.entityId != _cInfo.entityId && ip == cInfo.ip)
                            {
                                duplicate = true;
                                break;
                            }
                        }
                    }
                    long ipLong = GeneralFunction.ConvertIPToLong(_cInfo.ip);
                    if (duplicate || (ipLong >= GeneralFunction.ConvertIPToLong("10.0.0.0") && ipLong <= GeneralFunction.ConvertIPToLong("10.255.255.255")) ||
                        (ipLong >= GeneralFunction.ConvertIPToLong("172.16.0.0") && ipLong <= GeneralFunction.ConvertIPToLong("172.31.255.255")) ||
                        (ipLong >= GeneralFunction.ConvertIPToLong("192.168.0.0") && ipLong <= GeneralFunction.ConvertIPToLong("192.168.255.255")) ||
                        _cInfo.ip == "127.0.0.1")
                    {
                        string securityId = "";
                        for (int i = 0; i < 10; i++)
                        {
                            string pass = CreatePassword(4);
                            if (pass != "DBUG" && !PanelAccess.ContainsKey(pass))
                            {
                                securityId = pass;
                                if (!PanelAccess.ContainsValue(_cInfo.entityId))
                                {
                                    PanelAccess.Add(securityId, _cInfo.entityId);
                                }
                                else
                                {
                                    if (PanelAccess.Count > 0)
                                    {
                                        foreach (var client in PanelAccess)
                                        {
                                            if (client.Value == _cInfo.entityId)
                                            {
                                                PanelAccess.Remove(client.Key);
                                                break;
                                            }
                                        }
                                    }
                                    PanelAccess.Add(securityId, _cInfo.entityId);
                                }
                                break;
                            }
                        }
                        Phrases.Dict.TryGetValue("Shop4", out string phrase1);
                        phrase1 = phrase1.Replace("{Value}", securityId);
                        ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui open browserShop", true));
                    }
                    else
                    {
                        if (PanelAccess.Count > 0 && PanelAccess.ContainsValue(_cInfo.entityId))
                        {
                            var clients = PanelAccess.ToArray();
                            for (int i = 0; i < clients.Length; i++)
                            {
                                if (clients[i].Value == _cInfo.entityId && clients[i].Key != ip)
                                {
                                    PanelAccess.Remove(clients[i].Key);
                                    PanelAccess.Add(ip, _cInfo.entityId);
                                    break;
                                }
                            }
                        }
                        else if (PanelAccess.ContainsKey(ip))
                        {
                            PanelAccess[ip] = _cInfo.entityId;
                        }
                        else
                        {
                            PanelAccess.Add(ip, _cInfo.entityId);
                        }
                        _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("xui open browserShop", true));
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("Shop1", out string phrase2);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase2 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    string categories = "";
                    if (Categories.Count > 1)
                    {
                        categories = string.Join(", ", Categories.ToArray());
                    }
                    else
                    {
                        categories = Categories[0];
                    }
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + categories + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    Phrases.Dict.TryGetValue("Shop2", out string phrase3);
                    phrase3 = phrase3.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                    phrase3 = phrase3.Replace("{Command_shop}", Command_shop);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase3 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                    Phrases.Dict.TryGetValue("Shop13", out phrase3);
                    phrase3 = phrase3.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                    phrase3 = phrase3.Replace("{Command_shop_buy}", Command_shop_buy);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase3 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Shop.ListCategories: {0}", e.Message));
            }
        }

        public static void ShowCategory(ClientInfo _cInfo, string _category)
        {
            try
            {
                if (GeneralFunction.Debug)
                {
                    Log.Out(string.Format("[SERVERTOOLS] DEBUG: Shop: ShowCategory: _cInfo.playerName = '{0}' / _category = '{1}'", _cInfo.playerName, _category));
                }
                if (Categories.Contains(_category))
                {
                    for (int i = 0; i < Dict.Count; i++)
                    {
                        string[] itemData = Dict[i];
                        if (itemData[6].Contains(_category))
                        {
                            if (int.Parse(itemData[4]) > 1)
                            {
                                Phrases.Dict.TryGetValue("Shop11", out string phrase);
                                phrase = phrase.Replace("{Id}", itemData[0]);
                                phrase = phrase.Replace("{Count}", itemData[3]);
                                phrase = phrase.Replace("{Item}", itemData[2]);
                                phrase = phrase.Replace("{Quality}", itemData[4]);
                                phrase = phrase.Replace("{Price}", itemData[5]);
                                phrase = phrase.Replace("{Name}", Wallet.Currency_Name);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                            else
                            {
                                Phrases.Dict.TryGetValue("Shop12", out string phrase);
                                phrase = phrase.Replace("{Id}", itemData[0]);
                                phrase = phrase.Replace("{Count}", itemData[3]);
                                phrase = phrase.Replace("{Item}", itemData[2]);
                                phrase = phrase.Replace("{Price}", itemData[5]);
                                phrase = phrase.Replace("{Name}", Wallet.Currency_Name);
                                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                            }
                        }
                    }
                }
                else
                {
                    Phrases.Dict.TryGetValue("Shop14", out string phrase);
                    phrase = phrase.Replace("{Command_Prefix1}", ChatHook.Chat_Command_Prefix1);
                    phrase = phrase.Replace("{Command_shop}", Command_shop);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Shop.ShowCategory: {0}", e.Message));
            }
        }

        public static void Walletcheck(ClientInfo _cInfo, int _item, int _count)
        {
            try
            {
                for (int i = 0; i < Dict.Count; i++)
                {
                    string[] itemData = Dict[i];
                    if (int.Parse(itemData[0]) == _item)
                    {
                        int currency = 0;
                        if (Wallet.IsEnabled)
                        {
                            currency = Wallet.GetCurrency(_cInfo.CrossplatformId.CombinedString);
                        }
                        int cost = int.Parse(itemData[5]) * _count;
                        if (currency >= cost)
                        {
                            _count = int.Parse(itemData[3]) * _count;
                            ShopPurchase(_cInfo, itemData[1], itemData[2], _count, int.Parse(itemData[4]), cost);
                        }
                        else
                        {
                            Phrases.Dict.TryGetValue("Shop5", out string phrase);
                            phrase = phrase.Replace("{CoinName}", Wallet.Currency_Name);
                            phrase = phrase.Replace("{Value}", currency.ToString());
                            ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                        }
                        return;
                    }
                }
                Phrases.Dict.TryGetValue("Shop15", out string phrase1);
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase1 + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Shop.Walletcheck: {0}", e.Message));
            }
        }

        public static void ShopPurchase(ClientInfo _cInfo, string _itemName, string _secondaryName, int _count, int _quality, int _price)
        {
            try
            {
                if (_quality < 1)
                {
                    _quality = 1;
                }
                ItemValue itemValue = new ItemValue(ItemClass.GetItem(_itemName, false).type);
                if (itemValue.HasQuality)
                {
                    itemValue.Quality = 1;
                    if (_quality > 1)
                    {
                        itemValue.Quality = _quality;
                    }
                    itemValue.Modifications = new ItemValue[(int)EffectManager.GetValue(PassiveEffects.ModSlots, itemValue, itemValue.Quality - 1)];
                    itemValue.CosmeticMods = new ItemValue[itemValue.ItemClass.HasAnyTags(ItemClassModifier.CosmeticItemTags) ? 1 : 0];
                }
                World world = GameManager.Instance.World;
                var entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                {
                    entityClass = EntityClass.FromString("item"),
                    id = EntityFactory.nextEntityID++,
                    itemStack = new ItemStack(itemValue, _count),
                    pos = world.Players.dict[_cInfo.entityId].position,
                    rot = new Vector3(20f, 0f, 20f),
                    lifetime = 60f,
                    belongsPlayerId = _cInfo.entityId
                });
                world.SpawnEntityInWorld(entityItem);
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, _cInfo.entityId));
                world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Despawned);
                if (_price >= 1)
                {
                    Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, _price);
                }
                PersistentContainer.Instance.ShopLog.Add(new string[] { itemValue.ItemClass.Name, _count.ToString(), _cInfo.PlatformId.CombinedString, _cInfo.CrossplatformId.CombinedString, _cInfo.playerName, DateTime.Now.ToString() });
                PersistentContainer.DataChange = true;

                Phrases.Dict.TryGetValue("Shop16", out string phrase);
                phrase = phrase.Replace("{Count}", _count.ToString());
                if (_secondaryName != "")
                {
                    phrase = phrase.Replace("{Item}", _secondaryName);
                }
                else
                {
                    phrase = phrase.Replace("{Item}", itemValue.ItemClass.GetLocalizedItemName() ?? itemValue.ItemClass.GetItemName());
                }
                ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Shop.ShopPurchase: {0}", e.Message));
            }
        }

        public static string CreatePassword(int _length)
        {
            string pass = string.Empty;
            System.Random rnd = new System.Random();
            for (int i = 0; i < _length; i++)
            {
                pass += GeneralFunction.NumSet.ElementAt(rnd.Next(0, 10));
            }
            return pass;
        }

        private static void UpgradeXml()
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<Shop>");
                    sw.WriteLine(string.Format("<ST Version=\"{0}\" />", Config.Version));
                    sw.WriteLine("    <!-- Secondary name is what will show in chat instead of the item name -->");
                    sw.WriteLine("    <!-- Items with no quality should be set to 0 -->");
                    sw.WriteLine("    <!-- <Item Name=\"\" SecondaryName=\"\" Count=\"\" Quality=\"\" Price=\"\" Category=\"\" PanelMessage=\"\" /> -->");
                    for (int i = 0; i < OldNodeList.Count; i++)
                    {
                        if (OldNodeList[i].NodeType == XmlNodeType.Comment && !OldNodeList[i].OuterXml.Contains("<!-- Secondary name is") && 
                            !OldNodeList[i].OuterXml.Contains("<!-- <Item Name=\"\"") && !OldNodeList[i].OuterXml.Contains("<!-- Items with no"))
                        {
                            sw.WriteLine(OldNodeList[i].OuterXml);
                        }
                    }
                    sw.WriteLine();
                    sw.WriteLine();
                    for (int i = 0; i < OldNodeList.Count; i++)
                    {
                        if (OldNodeList[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement line = (XmlElement)OldNodeList[i];
                            if (line.HasAttributes && (line.Name == "Shop" || line.Name == "Item"))
                            {
                                string name = "", secondaryName = "", count = "", quality = "", price = "", category = "", panelMessage = "";
                                if (line.HasAttribute("Name"))
                                {
                                    name = line.GetAttribute("Name");
                                }
                                if (line.HasAttribute("SecondaryName"))
                                {
                                    secondaryName = line.GetAttribute("SecondaryName");
                                }
                                if (line.HasAttribute("Count"))
                                {
                                    count = line.GetAttribute("Count");
                                }
                                if (line.HasAttribute("Quality"))
                                {
                                    quality = line.GetAttribute("Quality");
                                }
                                if (line.HasAttribute("Price"))
                                {
                                    price = line.GetAttribute("Price");
                                }
                                if (line.HasAttribute("Category"))
                                {
                                    category = line.GetAttribute("Category");
                                }
                                if (line.HasAttribute("PanelMessage"))
                                {
                                    panelMessage = line.GetAttribute("PanelMessage");
                                }
                                sw.WriteLine(string.Format("    <Item Name=\"{0}\" SecondaryName=\"{1}\" Count=\"{2}\" Quality=\"{3}\" Price=\"{4}\" Category=\"{5}\" PanelMessage=\"{6}\" />", name, secondaryName, count, quality, price, category, panelMessage));
                            }
                        }
                    }

                    sw.WriteLine("</Shop>");
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in Shop.UpgradeXml: {0}", e.Message));
            }
            FileWatcher.EnableRaisingEvents = true;
            LoadXml();
        }

        public static void Writer(string _entry)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(LogFilePath, true, Encoding.UTF8))
                {
                    sw.WriteLine(_entry);
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();
                    sw.Dispose();
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("Error in Shop.Writer: {0}", e.Message));
            }
        }
    }
}