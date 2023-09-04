using Platform;
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
            Panel_Name = "Super Shop", CategoryString = "";

        public static Dictionary<string, int> PanelAccess = new Dictionary<string, int>();
        public static List<string[]> Dict = new List<string[]>();
        public static List<string> Categories = new List<string>();

        private static readonly string file = string.Format("Shop_{0}.txt", DateTime.Today.ToString("M-d-yyyy"));
        private static readonly string LogFilePath = string.Format("{0}/Logs/ShopLogs/{1}", API.ConfigPath, file);
        private static string FilePath = string.Format("{0}/{1}", API.ConfigPath, "Shop.xml");
        private static FileSystemWatcher FileWatcher = new FileSystemWatcher(API.ConfigPath, "Shop.xml");

        public static void Load()
        {
            LoadXml();
            InitFileWatcher();
        }

        public static void Unload()
        {
            Dict.Clear();
            Categories.Clear();
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
                XmlNodeList childNodes = xmlDoc.DocumentElement.ChildNodes;
                if (childNodes != null && childNodes[0] != null && childNodes[0].OuterXml.Contains("Version") && childNodes[0].OuterXml.Contains(Config.Version))
                {
                    Dict.Clear();
                    Categories.Clear();
                    CategoryString = "";
                    string qualityCategories = "";
                    for (int i = 0; i < childNodes.Count; i++)
                    {
                        if (childNodes[i].NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        XmlElement line = (XmlElement)childNodes[i];
                        if (!line.HasAttributes)
                        {
                            continue;
                        }
                        if (line.HasAttribute("Name") && line.HasAttribute("SecondaryName") && line.HasAttribute("Count") && line.HasAttribute("Quality") &&
                            line.HasAttribute("Price") && line.HasAttribute("Category") && line.HasAttribute("PanelMessage"))
                        {
                            string name = "";
                            string secondaryname;
                            if (line.HasAttribute("Name"))
                            {
                                name = line.GetAttribute("Name");
                            }
                            if (name == "")
                            {
                                continue;
                            }
                            if (line.HasAttribute("SecondaryName"))
                            {
                                secondaryname = line.GetAttribute("SecondaryName");
                            }
                            else
                            {
                                secondaryname = name;
                            }
                            int count = 0;
                            if (line.HasAttribute("Count"))
                            {
                                count = int.Parse(line.GetAttribute("Count"));
                            }
                            int quality = 0;
                            if (line.HasAttribute("Quality"))
                            {
                                quality = int.Parse(line.GetAttribute("Quality"));
                            }
                            int price = 0;
                            if (line.HasAttribute("Price"))
                            {
                                price = int.Parse(line.GetAttribute("Price"));
                            }
                            string category = "";
                            if (line.HasAttribute("Category"))
                            {
                                category = line.GetAttribute("Category").ToLower();
                            }
                            string panelMessage = "";
                            if (line.HasAttribute("PanelMessage"))
                            {
                                panelMessage = line.GetAttribute("PanelMessage");
                            }
                            ItemValue itemValue = ItemClass.GetItem(name, false);
                            if (itemValue.type == ItemValue.None.type || itemValue.ItemClass == null)
                            {
                                Log.Out(string.Format("[SERVERTOOLS] Ignoring Shop.xml entry. Item could not be found: {0}", name));
                                continue;
                            }
                            name = itemValue.ItemClass.GetItemName();
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
                            int id = Dict.Count;
                            string[] item = new string[] { id.ToString(), name, secondaryname, count.ToString(), quality.ToString(), price.ToString(), category, panelMessage };
                            if (!Dict.Contains(item))
                            {
                                Dict.Add(item);
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

                            }
                        }
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
                else
                {
                    XmlNodeList nodeList = xmlDoc.DocumentElement.ChildNodes;
                    if (nodeList != null)
                    {
                        Timers.UpgradeShopXml(nodeList);
                        //UpgradeXml(nodeList);
                        return;
                    }
                    File.Delete(FilePath);
                    UpdateXml();
                    return;
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
                    Log.Out("[SERVERTOOLS] Error in Shop.LoadXml: {0}", e.Message);
                }
            }
        }

        public static string GetPanelItems(ClientInfo _cInfo)
        {
            EntityPlayer player = null;
            if (_cInfo != null)
            {
                player = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
            }
            string panelItems = "";
            string[] item;
            string name;
            string secondaryName;
            string count;
            int quality = 1;
            int price = 0;
            Dictionary<string, int> itemList = new Dictionary<string, int>();
            List<string[]> shopLog = new List<string[]>();
            for (int i = 0; i < Dict.Count; i++)
            {
                item = Dict[i];
                name = item[1];
                secondaryName = item[2];
                count = item[3];
                quality = 1;
                int.TryParse(item[4], out quality);
                ItemValue itemValue = ItemClass.GetItem(name, false);
                if (itemValue.type == ItemValue.None.type || itemValue.ItemClass == null)
                {
                    continue;
                }
                if (itemValue.HasQuality)
                {
                    itemValue.Quality = quality;
                }
                string stats = "";
                if (itemValue.ItemClass.DisplayType == null)
                {
                    continue;
                }
                switch (itemValue.ItemClass.DisplayType)
                {
                    case "rangedGunNoMag":
                        float rangedDamage1 = EffectManager.GetValue(PassiveEffects.EntityDamage, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (rangedDamage1 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Ranged Damage: " + rangedDamage1;
                        }
                        int magazineSize1 = (int)EffectManager.GetValue(PassiveEffects.MagazineSize, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (magazineSize1 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Magazine Size: " + magazineSize1;
                        }
                        int range1 = (int)EffectManager.GetValue(PassiveEffects.MaxRange, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (range1 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Effective Range: " + range1;
                        }
                        int durability1 = (int)EffectManager.GetValue(PassiveEffects.DegradationMax, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
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
                        float rangedDamage2 = EffectManager.GetValue(PassiveEffects.EntityDamage, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (rangedDamage2 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Ranged Damage: " + rangedDamage2;
                        }
                        int magazineSize2 = (int)EffectManager.GetValue(PassiveEffects.MagazineSize, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (magazineSize2 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Magazine Size: " + magazineSize2;
                        }
                        int roundsPerMin2 = (int)EffectManager.GetValue(PassiveEffects.RoundsPerMinute, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (roundsPerMin2 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Rounds Per Minute: " + roundsPerMin2;
                        }
                        int range2 = (int)EffectManager.GetValue(PassiveEffects.MaxRange, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (range2 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Effective Range: " + range2;
                        }
                        int durability2 = (int)EffectManager.GetValue(PassiveEffects.DegradationMax, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
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
                        float rangedDamage3 = EffectManager.GetValue(PassiveEffects.EntityDamage, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (rangedDamage3 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Ranged Damage: " + rangedDamage3;
                        }
                        int pellets3 = (int)EffectManager.GetValue(PassiveEffects.RoundRayCount, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (pellets3 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Pellets: " + pellets3;
                        }
                        int magazineSize3 = (int)EffectManager.GetValue(PassiveEffects.MagazineSize, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (magazineSize3 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Magazine Size: " + magazineSize3;
                        }
                        int range3 = (int)EffectManager.GetValue(PassiveEffects.MaxRange, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (range3 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Effective Range: " + range3;
                        }
                        int durability3 = (int)EffectManager.GetValue(PassiveEffects.DegradationMax, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
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
                        float rangedDamage4 = EffectManager.GetValue(PassiveEffects.EntityDamage, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (rangedDamage4 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Ranged Damage: " + rangedDamage4;
                        }
                        int pellets4 = (int)EffectManager.GetValue(PassiveEffects.RoundRayCount, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (pellets4 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Pellets: " + pellets4;
                        }
                        int magazineSize4 = (int)EffectManager.GetValue(PassiveEffects.MagazineSize, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (magazineSize4 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Magazine Size: " + magazineSize4;
                        }
                        int roundsPerMin4 = (int)EffectManager.GetValue(PassiveEffects.RoundsPerMinute, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (roundsPerMin4 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Rounds Per Minute: " + roundsPerMin4;
                        }
                        int range4 = (int)EffectManager.GetValue(PassiveEffects.MaxRange, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (range4 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Effective Range: " + range4;
                        }
                        int durability4 = (int)EffectManager.GetValue(PassiveEffects.DegradationMax, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
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
                        int meleeDamage5 = (int)EffectManager.GetValue(PassiveEffects.EntityDamage, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (meleeDamage5 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Melee Damage: " + meleeDamage5;
                        }
                        int repairAmount5 = (int)EffectManager.GetValue(PassiveEffects.RepairAmount, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (repairAmount5 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Repair Amount: " + repairAmount5;
                        }
                        int blockDamage5 = (int)EffectManager.GetValue(PassiveEffects.BlockDamage, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (blockDamage5 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Block Damage: " + blockDamage5;
                        }
                        int stamina5 = (int)EffectManager.GetValue(PassiveEffects.StaminaLoss, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (stamina5 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Stamina Cost: " + stamina5;
                        }
                        int attacksPerMin5 = (int)EffectManager.GetValue(PassiveEffects.AttacksPerMinute, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (attacksPerMin5 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Attacks Per Min: " + attacksPerMin5;
                        }
                        int durability5 = (int)EffectManager.GetValue(PassiveEffects.DegradationMax, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
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
                        int meleeDamage6 = (int)EffectManager.GetValue(PassiveEffects.EntityDamage, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (meleeDamage6 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Melee Damage: " + meleeDamage6;
                        }
                        int repairAmount6 = (int)EffectManager.GetValue(PassiveEffects.RepairAmount, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (repairAmount6 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Repair Amount: " + repairAmount6;
                        }
                        int blockDamage6 = (int)EffectManager.GetValue(PassiveEffects.BlockDamage, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (blockDamage6 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Block Damage: " + blockDamage6;
                        }
                        int range6 = (int)EffectManager.GetValue(PassiveEffects.MaxRange, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (range6 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Effective Range: " + range6;
                        }
                        int attacksPerMin6 = (int)EffectManager.GetValue(PassiveEffects.AttacksPerMinute, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (attacksPerMin6 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Attacks Per Min: " + attacksPerMin6;
                        }
                        int durability6 = (int)EffectManager.GetValue(PassiveEffects.DegradationMax, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
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
                        int meleeDamage7 = (int)EffectManager.GetValue(PassiveEffects.EntityDamage, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (meleeDamage7 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Melee Damage: " + meleeDamage7;
                        }
                        int bonusDamage7 = (int)EffectManager.GetValue(PassiveEffects.DamageBonus, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (bonusDamage7 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Power Attack Damage: " + bonusDamage7;
                        }
                        int blockDamage7 = (int)EffectManager.GetValue(PassiveEffects.BlockDamage, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (blockDamage7 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Block Damage: " + blockDamage7;
                        }
                        int stamina7 = (int)EffectManager.GetValue(PassiveEffects.StaminaLoss, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (stamina7 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Stamina Cost: " + stamina7;
                        }
                        int attacksPerMin7 = (int)EffectManager.GetValue(PassiveEffects.AttacksPerMinute, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (attacksPerMin7 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Attacks Per Min: " + attacksPerMin7;
                        }
                        int durability7 = (int)EffectManager.GetValue(PassiveEffects.DegradationMax, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
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
                        int meleeDamage8 = (int)EffectManager.GetValue(PassiveEffects.EntityDamage, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (meleeDamage8 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Melee Damage: " + meleeDamage8;
                        }
                        int blockDamage8 = (int)EffectManager.GetValue(PassiveEffects.BlockDamage, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (blockDamage8 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Block Damage: " + blockDamage8;
                        }
                        int attacksPerMin8 = (int)EffectManager.GetValue(PassiveEffects.AttacksPerMinute, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (attacksPerMin8 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Attacks Per Min: " + attacksPerMin8;
                        }
                        int durability8 = (int)EffectManager.GetValue(PassiveEffects.DegradationMax, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
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
                        int meleeDamage9 = (int)EffectManager.GetValue(PassiveEffects.EntityDamage, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (meleeDamage9 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Melee Damage: " + meleeDamage9;
                        }
                        int bonusDamage9 = (int)EffectManager.GetValue(PassiveEffects.DamageBonus, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (bonusDamage9 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Power Attack Damage: " + bonusDamage9;
                        }
                        int targetArmor = (int)EffectManager.GetValue(PassiveEffects.TargetArmor, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (targetArmor != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Target Armor: " + targetArmor;
                        }
                        int blockDamage9 = (int)EffectManager.GetValue(PassiveEffects.BlockDamage, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (blockDamage9 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Block Damage: " + blockDamage9;
                        }
                        int stamina9 = (int)EffectManager.GetValue(PassiveEffects.StaminaLoss, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (stamina9 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Stamina Cost: " + stamina9;
                        }
                        int attacksPerMin9 = (int)EffectManager.GetValue(PassiveEffects.AttacksPerMinute, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (attacksPerMin9 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Attacks Per Min: " + attacksPerMin9;
                        }
                        int durability9 = (int)EffectManager.GetValue(PassiveEffects.DegradationMax, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
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
                        int meleeDamage10 = (int)EffectManager.GetValue(PassiveEffects.EntityDamage, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (meleeDamage10 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Melee Damage: " + meleeDamage10;
                        }
                        int bonusDamage10 = (int)EffectManager.GetValue(PassiveEffects.DamageBonus, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (bonusDamage10 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Power Attack Damage: " + bonusDamage10;
                        }
                        int blockDamage10 = (int)EffectManager.GetValue(PassiveEffects.BlockDamage, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (blockDamage10 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Block Damage: " + blockDamage10;
                        }
                        int stamina10 = (int)EffectManager.GetValue(PassiveEffects.StaminaLoss, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (stamina10 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Stamina Cost: " + stamina10;
                        }
                        int attacksPerMin10 = (int)EffectManager.GetValue(PassiveEffects.AttacksPerMinute, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (attacksPerMin10 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Attacks Per Min: " + attacksPerMin10;
                        }
                        int durability10 = (int)EffectManager.GetValue(PassiveEffects.DegradationMax, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
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
                        int meleeDamage11 = (int)EffectManager.GetValue(PassiveEffects.EntityDamage, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (meleeDamage11 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Ranged Damage: " + meleeDamage11;
                        }
                        int projectileVelocity11 = (int)EffectManager.GetValue(PassiveEffects.ProjectileVelocity, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (projectileVelocity11 > 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Projectile Velocity: " + projectileVelocity11;
                        }
                        int durability11 = (int)EffectManager.GetValue(PassiveEffects.DegradationMax, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
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
                        int meleeDamage12 = (int)EffectManager.GetValue(PassiveEffects.EntityDamage, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (meleeDamage12 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Melee Damage: " + meleeDamage12;
                        }
                        int blockDamage12 = (int)EffectManager.GetValue(PassiveEffects.BlockDamage, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (blockDamage12 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Block Damage: " + blockDamage12;
                        }
                        int attacksPerMin12 = (int)EffectManager.GetValue(PassiveEffects.AttacksPerMinute, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (attacksPerMin12 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Attacks Per Min: " + attacksPerMin12;
                        }
                        int durability12 = (int)EffectManager.GetValue(PassiveEffects.DegradationMax, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
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
                        float damageResistance13 = EffectManager.GetValue(PassiveEffects.PhysicalDamageResist, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (damageResistance13 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Light Armor Rating: " + Math.Round(damageResistance13, 1);
                        }
                        float explosionResistance13 = EffectManager.GetValue(PassiveEffects.ElementalDamageResist, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (explosionResistance13 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Explosion Resistance: " + Math.Round(explosionResistance13, 1);
                        }
                        float critResistence13 = EffectManager.GetValue(PassiveEffects.BuffResistance, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (critResistence13 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Crit Resistance: " + Math.Round(critResistence13, 1);
                        }
                        float stamina13 = EffectManager.GetValue(PassiveEffects.StaminaChangeOT, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (stamina13 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Stamina /s: -" + stamina13;
                        }
                        float mobility13 = EffectManager.GetValue(PassiveEffects.Mobility, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (mobility13 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Mobility: -" + Math.Round(mobility13, 1);
                        }
                        float noise13 = EffectManager.GetValue(PassiveEffects.NoiseMultiplier, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (noise13 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Noise Increase: " + Math.Round(noise13, 1);
                        }
                        int durability13 = (int)EffectManager.GetValue(PassiveEffects.DegradationMax, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (durability13 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Max Durability: " + durability13;
                        }
                        int hypothermalResistence13 = (int)EffectManager.GetValue(PassiveEffects.HypothermalResist, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
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
                        float damageResistance14 = EffectManager.GetValue(PassiveEffects.PhysicalDamageResist, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (damageResistance14 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Heavy Armor Rating: " + Math.Round(damageResistance14, 1);
                        }
                        float explosionResistance14 = EffectManager.GetValue(PassiveEffects.ElementalDamageResist, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (explosionResistance14 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Explosion Resistance: " + Math.Round(explosionResistance14, 1);
                        }
                        float critResistance14 = EffectManager.GetValue(PassiveEffects.BuffResistance, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (critResistance14 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Crit Resistance: " + Math.Round(critResistance14, 1);
                        }
                        float stamina14 = EffectManager.GetValue(PassiveEffects.StaminaChangeOT, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (stamina14 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Stamina /s: -" + stamina14;
                        }
                        float mobility14 = EffectManager.GetValue(PassiveEffects.Mobility, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (mobility14 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Mobility: -" + Math.Round(mobility14, 1);
                        }
                        float noise14 = EffectManager.GetValue(PassiveEffects.NoiseMultiplier, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (noise14 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Noise Increase: " + Math.Round(noise14, 1);
                        }
                        int durability14 = (int)EffectManager.GetValue(PassiveEffects.DegradationMax, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (durability14 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Max Durability: " + durability14;
                        }
                        int hypothermalResistence14 = (int)EffectManager.GetValue(PassiveEffects.HypothermalResist, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
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
                        int explosionDamage15 = (int)EffectManager.GetValue(PassiveEffects.EntityDamage, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (explosionDamage15 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Explosion Damage: " + explosionDamage15;
                        }
                        int blockDamage15 = (int)EffectManager.GetValue(PassiveEffects.BlockDamage, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (blockDamage15 != 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Block Damage: " + blockDamage15;
                        }
                        int explosionRadius15 = (int)EffectManager.GetValue(PassiveEffects.ExplosionRadius, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
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
                        int healthUp16 = (int)EffectManager.GetValue(PassiveEffects.HealthGain, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (healthUp16 > 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Health Gain: " + healthUp16;
                        }
                        int healthDown16 = (int)EffectManager.GetValue(PassiveEffects.HealthLoss, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                        if (healthDown16 < 0)
                        {
                            if (stats.Length > 0)
                            {
                                stats += " </br> ";
                            }
                            stats += "Health Loss: " + healthDown16;
                        }
                        break;
                    case null:
                        break;
                }
                if (itemValue.ItemClass.GetItemName().ToLower().Contains("arrow") || itemValue.ItemClass.DisplayType.ToLower().Contains("arrow"))
                {
                    int meleeDamage = (int)EffectManager.GetValue(PassiveEffects.EntityDamage, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                    if (meleeDamage != 0)
                    {
                        if (stats.Length > 0)
                        {
                            stats += " </br> ";
                        }
                        stats += "Ranged Damage: " + meleeDamage;
                    }
                    int blockDamage = (int)EffectManager.GetValue(PassiveEffects.BlockDamage, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                    if (blockDamage != 0)
                    {
                        if (stats.Length > 0)
                        {
                            stats += " </br> ";
                        }
                        stats += "Block Damage: " + blockDamage;
                    }
                    int projectileVelocity = (int)EffectManager.GetValue(PassiveEffects.ProjectileVelocity, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
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
                    int foodUp = (int)EffectManager.GetValue(PassiveEffects.FoodGain, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                    if (foodUp > 0)
                    {
                        if (stats.Length > 0)
                        {
                            stats += " </br> ";
                        }
                        stats += "Food: " + foodUp;
                    }
                    int foodDown = (int)EffectManager.GetValue(PassiveEffects.FoodLoss, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                    if (foodDown < 0)
                    {
                        if (stats.Length > 0)
                        {
                            stats += " </br> ";
                        }
                        stats += "Food: " + foodDown;
                    }
                    int healthUp = (int)EffectManager.GetValue(PassiveEffects.HealthGain, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                    if (healthUp > 0)
                    {
                        if (stats.Length > 0)
                        {
                            stats += " </br> ";
                        }
                        stats += "Health: " + healthUp;
                    }
                    int healthDown = (int)EffectManager.GetValue(PassiveEffects.HealthLoss, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                    if (healthDown < 0)
                    {
                        if (stats.Length > 0)
                        {
                            stats += " </br> ";
                        }
                        stats += "Health: " + healthDown;
                    }
                    int waterUp = (int)EffectManager.GetValue(PassiveEffects.WaterGain, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                    if (waterUp > 0)
                    {
                        if (stats.Length > 0)
                        {
                            stats += " </br> ";
                        }
                        stats += "Water: " + waterUp;
                    }
                    int waterDown = (int)EffectManager.GetValue(PassiveEffects.WaterLoss, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                    if (waterDown < 0)
                    {
                        if (stats.Length > 0)
                        {
                            stats += " </br> ";
                        }
                        stats += "Water: " + waterDown;
                    }
                    int staminaMax = (int)EffectManager.GetValue(PassiveEffects.StaminaMax, itemValue, quality - 1, player, null, itemValue.ItemClass.ItemTags, false, false, false, false, 1, false);
                    if (staminaMax != 0)
                    {
                        if (stats.Length > 0)
                        {
                            stats += " </br> ";
                        }
                        stats += "Max Stamina Bonus: " + staminaMax;
                    }
                    if (stats.Length > 0)
                    {
                        stats += " </br> ";
                    }
                    stats += "Price: " + price + " </br> ";
                    string sold = "Sold: 0";
                    if (PersistentContainer.Instance.ShopLog.Count > 0)
                    {
                        int sellCount = 0;
                        shopLog = PersistentContainer.Instance.ShopLog;
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
                    itemList.Add(item[6] + "§" + name + "§" + secondaryName + "§" + itemValue.ItemClass.GetIconName() + "§" + count + "§" +
                        quality + "§" + price + "§" + stats + "§" + i + "§" + item[7] + "╚", price);
                }
                foreach (var entry in itemList.OrderBy(key => key.Value))
                {
                    panelItems += entry.Key;
                }
                if (panelItems.Length > 0)
                {
                    panelItems = panelItems.Remove(panelItems.Length - 1);
                }
            }
            return panelItems;
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
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- Do not forget to remove these omission tags/arrows on your own entries -->");
                    sw.WriteLine("    <!-- Secondary name is what will show in chat instead of the item name -->");
                    sw.WriteLine("    <!-- Items with no quality should be set to 1 -->");
                    sw.WriteLine("    <!-- <Item Name=\"\" SecondaryName=\"\" Count=\"\" Quality=\"\" Price=\"\" Category=\"\" PanelMessage=\"\" /> -->");
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
                Log.Out("[SERVERTOOLS] Error in Shop.UpdateXml: {0}", e.Message);
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

        public static void PosCheck(ClientInfo _cInfo, string _categoryOrItem, int _form, int _count)
        {
            try
            {
                if (Dict.Count > 0)
                {
                    EntityPlayer player = GeneralOperations.GetEntityPlayer(_cInfo.entityId);
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
                    Phrases.Dict.TryGetValue("Shop8", out string phrase);
                    ChatHook.ChatMessage(_cInfo, Config.Chat_Response_Color + phrase + "[-]", -1, Config.Server_Response_Name, EChatType.Whisper, null);
                }
            }
            catch (Exception e)
            {
                Log.Out("[SERVERTOOLS] Error in Shop.PosCheck: {0}", e.Message);
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
                if (Panel && WebAPI.IsEnabled && WebAPI.IsRunning && !PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Overlay)
                {
                    string ip = _cInfo.ip;
                    bool duplicate = false;
                    List<ClientInfo> clientList = GeneralOperations.ClientList();
                    if (clientList != null && clientList.Count > 1)
                    {
                        for (int i = 0; i < clientList.Count; i++)
                        {
                            ClientInfo cInfoTwo = clientList[i];
                            if (cInfoTwo != null && cInfoTwo.entityId != _cInfo.entityId && ip == cInfoTwo.ip)
                            {
                                duplicate = true;
                                break;
                            }
                        }
                    }
                    uint ipLong = NetworkUtils.ToInt(_cInfo.ip);
                    if (duplicate || (ipLong >= NetworkUtils.ToInt("10.0.0.0") && ipLong <= NetworkUtils.ToInt("10.255.255.255")) ||
                        (ipLong >= NetworkUtils.ToInt("172.16.0.0") && ipLong <= NetworkUtils.ToInt("172.31.255.255")) ||
                        (ipLong >= NetworkUtils.ToInt("192.168.0.0") && ipLong <= NetworkUtils.ToInt("192.168.255.255")) ||
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
                Log.Out("[SERVERTOOLS] Error in Shop.ListCategories: {0}", e.Message);
            }
        }

        public static void ShowCategory(ClientInfo _cInfo, string _category)
        {
            try
            {
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
                Log.Out("[SERVERTOOLS] Error in Shop.ShowCategory: {0}", e.Message);
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
                        int currency = 0, bankCurrency = 0, cost = int.Parse(itemData[5]) * _count;
                        if (Wallet.IsEnabled)
                        {
                            currency = Wallet.GetCurrency(_cInfo.CrossplatformId.CombinedString);
                        }
                        if (Bank.IsEnabled && Bank.Direct_Payment)
                        {
                            bankCurrency = PersistentContainer.Instance.Players[_cInfo.CrossplatformId.CombinedString].Bank;
                        }
                        if (currency + bankCurrency >= cost)
                        {
                            if (currency > 0)
                            {
                                if (currency < cost)
                                {
                                    Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, currency);
                                    cost -= currency;
                                    Bank.SubtractCurrencyFromBank(_cInfo.CrossplatformId.CombinedString, cost);
                                }
                                else
                                {
                                    Wallet.RemoveCurrency(_cInfo.CrossplatformId.CombinedString, cost);
                                }
                            }
                            else
                            {
                                Bank.SubtractCurrencyFromBank(_cInfo.CrossplatformId.CombinedString, cost);
                            }
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
                Log.Out("[SERVERTOOLS] Error in Shop.Walletcheck: {0}", e.Message);
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
                itemValue.Quality = 0;
                itemValue.Modifications = new ItemValue[0];
                itemValue.CosmeticMods = new ItemValue[0];
                int modSlots = (int)EffectManager.GetValue(PassiveEffects.ModSlots, itemValue, itemValue.Quality - 1);
                if (modSlots > 0)
                {
                    itemValue.Modifications = new ItemValue[modSlots];
                }
                itemValue.CosmeticMods = new ItemValue[itemValue.ItemClass.HasAnyTags(ItemClassModifier.CosmeticItemTags) ? 1 : 0];
                if (itemValue.HasQuality)
                {
                    if (_quality > 0)
                    {
                        itemValue.Quality = _quality;
                    }
                    else
                    {
                        itemValue.Quality = 1;
                    }
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
                Log.Out("[SERVERTOOLS] Error in Shop.ShopPurchase: {0}", e.Message);
            }
        }

        public static string CreatePassword(int _length)
        {
            string pass = string.Empty;
            System.Random rnd = new System.Random();
            for (int i = 0; i < _length; i++)
            {
                pass += GeneralOperations.NumSet.ElementAt(rnd.Next(0, 10));
            }
            return pass;
        }

        public static void UpgradeXml(XmlNodeList nodeList)
        {
            try
            {
                FileWatcher.EnableRaisingEvents = false;
                File.Delete(FilePath);
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sw.WriteLine("<Shop>");
                    sw.WriteLine("    <!-- <Version=\"{0}\" /> -->", Config.Version);
                    sw.WriteLine("    <!-- Do not forget to remove these omission tags/arrows on your own entries -->");
                    sw.WriteLine("    <!-- Secondary name is what will show in chat instead of the item name -->");
                    sw.WriteLine("    <!-- Items with no quality should be set to 1 -->");
                    sw.WriteLine("    <!-- <Item Name=\"cannedFood\" SecondaryName=\"cannedFood\" Count=\"1\" Quality=\"1\" Price=\"20\" Category=\"food\" PanelMessage=\"Tuna a la carte\" /> -->");
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (nodeList[i].NodeType == XmlNodeType.Comment)
                        {
                            if (!nodeList[i].OuterXml.Contains("<!-- Secondary name") && !nodeList[i].OuterXml.Contains("<!-- Items with") &&
                            !nodeList[i].OuterXml.Contains("<Item Name=\"\"") && !nodeList[i].OuterXml.Contains("<!-- <Version") &&
                            !nodeList[i].OuterXml.Contains("<!-- Do not forget") && !nodeList[i].OuterXml.Contains("<!-- <Item Name=\"cannedFood"))
                            {
                                sw.WriteLine(nodeList[i].OuterXml);
                            }
                        }
                    }
                    for (int i = 0; i < nodeList.Count; i++)
                    {
                        if (nodeList[i].NodeType != XmlNodeType.Comment)
                        {
                            XmlElement line = (XmlElement)nodeList[i];
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
                Log.Out("[SERVERTOOLS] Error in Shop.UpgradeXml: {0}", e.Message);
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
                }
            }
            catch (Exception e)
            {
                Log.Out("Error in Shop.Writer: {0}", e.Message);
            }
        }
    }
}