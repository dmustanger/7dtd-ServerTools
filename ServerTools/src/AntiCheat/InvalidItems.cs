using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ServerTools
{
    public class InventoryCheck
    {
        public static bool IsEnabled = false;
        public static bool IsRunning = false;
        public static bool AnounceInvalidStack = false;
        public static bool BanPlayer = true;
        private static string file = "InvalidItems.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private static SortedDictionary<string, string> dict = new SortedDictionary<string, string>();
        private static FileSystemWatcher fileWatcher = new FileSystemWatcher(API.ConfigPath, file);
        public static int LevelToIgnore = 0;

        public static void Load()
        {
            if (IsEnabled && !IsRunning)
            {
                LoadXml();
                InitFileWatcher();
            }
        }

        public static void Unload()
        {
            dict.Clear();
            fileWatcher.Dispose();
            IsRunning = false;
        }

        private static void LoadXml()
        {
            if (!Utils.FileExists(filePath))
            {
                UpdateXml();
            }
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(filePath);
            }
            catch (XmlException e)
            {
                Log.Error(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", file, e.Message));
                return;
            }
            XmlNode _XmlNode = xmlDoc.DocumentElement;
            foreach (XmlNode childNode in _XmlNode.ChildNodes)
            {
                if (childNode.Name == "Items")
                {
                    dict.Clear();
                    foreach (XmlNode subChild in childNode.ChildNodes)
                    {
                        if (subChild.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        if (subChild.NodeType != XmlNodeType.Element)
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Unexpected XML node found in 'Items' section: {0}", subChild.OuterXml));
                            continue;
                        }
                        XmlElement _line = (XmlElement)subChild;
                        if (!_line.HasAttribute("itemName"))
                        {
                            Log.Warning(string.Format("[SERVERTOOLS] Ignoring Item entry because of missing 'itemName' attribute: {0}", subChild.OuterXml));
                            continue;
                        }
                        if (!dict.ContainsKey(_line.GetAttribute("itemName")))
                        {
                            dict.Add(_line.GetAttribute("itemName"), null);
                        }
                    }
                }
            }
        }

        private static void UpdateXml()
        {
            fileWatcher.EnableRaisingEvents = false;
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<InvalidItems>");
                sw.WriteLine("    <Items>");
                if (dict.Count > 0)
                {
                    foreach (KeyValuePair<string, string> kvp in dict)
                    {
                        sw.WriteLine(string.Format("        <item itemName=\"{0}\" />", kvp.Key));
                    }
                }
                else
                {
                    sw.WriteLine(string.Format("        <item itemName=\"air\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"dirt\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"radiated\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"potassiumNitrate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"ironOre\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"clay\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"leadOre\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"bedrock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sandStone\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"desertGround\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"ice\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"fertileDirt\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"fertileGrass\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"fertileFarmland\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"silverOre\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"coalOre\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"terrainFiller\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"goldOre\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"hardSod\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodDebris\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"oilDeposit\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"diamondOre\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"fertilizedFarmland\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"grassFromDirt\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plainsGroundFromDirt\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plainsGroundWGrass1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plainsGroundWGrass2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"burntForestGroundFromDirt\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"burntForestGroundWGrass1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"burntForestGroundWGrass2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"forestGroundFromDirt\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"forestGroundWGrass1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"forestGroundWGrass2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"clayInSandstone\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"lootStoneHelper\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteTerrain\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"imposterCTREighth\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"imposterPyramid\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"imposterQuarter\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"imposterPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"imposterHalf\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"imposterRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"imposterCTRPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"imposterSheet\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"streetLightImposter\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"imposterBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"imposterDontBlockBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"imposterDontBlockHalf\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"imposterDontBlockQuarter\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"imposterDontBlockSheet\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"imposterDontBlockCTRPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"imposterCNRFull\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"imposterDontBlockCTRQuarter\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"gravelPlusIron\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"gravelPlusLead\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"gravelPlusPotassium\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"gravelPlusCoal\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corrugatedMetalMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"awningMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"glassBulletproofMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeGrassMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cropsGrowingMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cropsHarvestableMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stainlessSteelMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapIronNoUpgradeMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cobblestoneMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodWeakNoUpgradeMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodNoUpgradeMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeMasterGrowing\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corrugatedMetalNoUpgradeMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"flagstoneMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteNoUpgradeMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcreteMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredRConcreteMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredConcreteMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"brickNoUpgradeMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"brickMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rScrapIronMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rebarFrameMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stoneNoUpgradeMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapIronMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapIronFrameMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodMetalMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodFrameMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"solidWoodFrameWedge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodWedge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodWedge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodMetalWedge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"solidScrapIronFrameWedge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapIronWedge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rScrapIronWedge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcreteWedge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredRConcreteCNRRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredRConcreteCNRFull\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcreteCNRRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelWedge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodMetalPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapIronPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rScrapIronPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcretePlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelCNRRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredRConcreteEighth\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcreteEighth\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodSupport\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodMetalSupport\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapIronSupport\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rScrapIronSupport\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcreteSupport\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelEighth\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredRConcreteHalf\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcreteHalf\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelSupport\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelHalf\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredRConcretePole\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredRConcretePyramid\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcretePyramid\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelPyramid\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapIronPillar100\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rScrapIronPillar100\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredRConcreteQuarter\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcreteQuarter\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcretePillar100\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelPillar100\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelQuarter\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredRConcreteStairs25\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcreteStairs25\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredRConcreteWedge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredRConcreteBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcreteBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredRConcreteWedgeTip\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcreteWedgeTip\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelWedgeTip\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredRConcretePlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"solidScrapIronFrameBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapIronCNRInside\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"solidScrapIronFrameCNRInside\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rScrapIronCNRInside\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapIronCNRFull\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodMetalRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"solidWoodFrameCNRInside\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodCNRInside\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodCNRInside\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodMetalCNRInside\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodCNRFull\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"solidWoodFrameCNRFull\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodCNRFull\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodMetalCNRFull\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"solidScrapIronFrameCNRFull\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rScrapIronCNRFull\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rScrapIronStairs25\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"solidScrapIronFrameRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapIronRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rScrapIronRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredRConcreteSupport\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredRConcreteRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcreteRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcreteCNRFull\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteSupport\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredRConcretePillar100\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredRConcreteCTRPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcreteCNRInside\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredRConcretePillar50\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelCNRFull\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedBlueberry3Harvest\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedPotato2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedPotato3Harvest\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedBlueberry2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"lootWastelandHelper\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"lootDesertHelper\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"lootBurntForestHelper\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"lootPlainsHelper\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"animalGore\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"lootConstructionSuppliesHelper\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteCNRInside\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodStairs25\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntGarageStorage\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeSnowyGrassDiagonal\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntLootCrateBookstore\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntLootCrateCarParts\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcretePillar50\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodDestroyed05\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodDestroyed06\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodDestroyed07\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntMedicineCabinet\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"securityGateCTRPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"securityGatePlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodPyramid\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodMetalBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelPillar50\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rock01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rock02\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rock03\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rock04\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rock05\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rock06\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rock07\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rock08\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rock09\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rock10\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rock11\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rock12\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"porchLight01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"porchLight04\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"porchLight04Brass\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"ceilingLight01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"ceilingLight02\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"ceilingLight02Brass\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"ceilingLight04\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"ceilingLight05\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"ceilingLight05Brass\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"wallLight01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"ceilingLight07\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntMailbox\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pew_segment01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pew_segment02\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pew_segment03\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntChest01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntChest02\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"metalRivetSheet\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntSuitcase\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntStorageChest\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntTrashPile01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntTrashPile02\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntTrashPile03\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntTrashPile04\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntTrashPile05\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntTrashPile06\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntTrashPile07\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntTrashPile08\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntTrashPile09\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"wallTorch\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"graveStone01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"graveStone02\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"graveStone03\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"graveStone04\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"graveStone05\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"graveStone06\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"graveStone07\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"graveStone08\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"boardedWindowsSheet4\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"arrowHelper\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"crossHelper\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"poleTop01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"streetLight01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treasureChest\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cinderBlocks01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"solidWoodFrameBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concretePillar100\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concretePillar50\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"decals1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"solidWoodFrameRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corrugatedMetalSheet\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"lootMPHelper\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"metalTrussingSheet\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corrugatedMetalCTRPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"trapSpikesDamage1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"trapSpikesDamage2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"trapSpikesDamage3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"trapSpikesDamage4\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"trapSpikesDamage5\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corrugatedMetalCNRFull\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cinderBlocks02\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapMetalPile\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corrugatedMetalCNRInside\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corrugatedMetalWedgeTip\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntBackpack01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntBackpack02\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntBackpack03\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntDuffle01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntSportsBag01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntSportsBag02\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntPurse01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"bed01Frame\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"shapedChargeBlockTest\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"driftwood\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"driftwood2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signRoadArrowheadApache\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signRoadAZ260eastSpeed65\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signRoadAZ260west\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signRoadAZ260westSpeed65\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signRoadAZ73north\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signRoadAZ73northSpeed65\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signRoadAZ73south\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signRoadAZ73southSpeed65\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signCamping\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signCampFish\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signRoadDestinationsEast\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signRoadDestinationsWest\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signInfoCenter\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signNoHazardousWaste\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signRoadWork\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signRoadRoughSurface\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signSchoolZone\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signRoadSlow\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signRoadSpeed25\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signRoadSpeed25noTrucks\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signRoadSpeed35\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signRoadSpeed45\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signRoadSpeed55\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signRoadSpeed65\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signRoadStop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signRoadStop4way\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signRoadApacheAZ260\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signRoadBellLake\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signRoadCoronado\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signRoadCoronadoCourtland\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signRoadCourtlandApache\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signRoadCourtlandAZ260\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signRoadCourtlandBell\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signRoadCourtlandHuenink\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signRoadCourtlandMaple\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signRoadCourtlandTran\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signRoadDavis\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signRoadEssig\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signRoadLangTran\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signRoadPrivate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signRoadTran\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signRoadCourtlandAZ260Duplicate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signNationalPark\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signSpillwayLake\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signAnselAdamsRiver\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signRoadMaple\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rebarDestroyed\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"storeShelving01Double\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"storeShelving01Top\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"storeShelving01TopDouble\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedHop2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedHop3Harvest\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedMushroom2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"mushroom3Harvest\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedYucca2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corrugatedMetalWedgeIncline\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteCNRRound\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cinderBlocks03\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntBirdnest\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteCNRRoundTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredRConcreteCNRRound\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredRConcreteCNRRoundTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcreteCNRRound\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"tv_fallen\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeJuniper6m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeMountainPine8m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeMountainPine13m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeMountainPine16m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeMountainPine19m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeMountainPine20m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeMountainPineDry\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeBirch10m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeBirch12m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeBirch15m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeDeadShrub\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeMaple6m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeMaple13m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcreteCNRRoundTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeMaple15m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeMaple16m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeMaple17m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelCNRRound\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeBurntPine01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeBurntPine02\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeBurntPine03\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelCNRRoundTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeDeadTree01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeDeadTree02\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeWinterEverGreen\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeWinterPine01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntTreeStump\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeWinterPine02\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeWinterPine03\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeDesertShrub\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntBackpackDropped\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodWedgeCNRInsideBottom\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodWedgeCNRInsideTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corrugatedMetalPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"brickHalf\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"burntWoodBlock1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"burntWoodBlock2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"burntWoodBlock3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"burntWoodBlock4\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"burntWoodBlock5\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"burntWoodBlock6\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"burntWoodRamp7\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"schoolDesk\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rockResource\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"solidWoodFrameWedgeIncline\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodWedgeIncline\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodMetalWedgeIncline\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"solidScrapIronFrameWedgeIncline\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapIronWedgeIncline\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stainlessSteelRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rockResource02\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stainlessSteelWedge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"secureReinforcedDoorWooden\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"metalReinforcedDoorWooden\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rockResourceBroke1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rockResourceBroke2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rockResource02Broke1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rockResource02Broke2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rockResource02Broke3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stainlessSteelWedgeTip\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stainlessSteelHalf\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stainlessSteelQuarter\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stainlessSteelEighth\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stainlessSteelPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"window02Frame\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntVendingMachine2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rScrapIronWedgeIncline\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeCactus01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeCactus02\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeCactus03\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeCactus04\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeCactus05\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeCactus06\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treePlantedMountainPine6m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treePlantedMountainPine8m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treePlantedMountainPine13m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treePlantedMountainPine16m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treePlantedMountainPine19m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treePlantedMaple6m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treePlantedMaple13m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treePlantedMaple15m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treePlantedMaple17m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteWedgeIncline\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredRConcreteWedgeIncline\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcreteWedgeIncline\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelWedgeIncline\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"solidWoodFrameWedgeStairs\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodWedgeStairs\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"lootToolsAndForgeHelper\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"lootStoreCratesHelper\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"lootWorkstationsHelper\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntCollapsedForge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntCollapsedWorkbench\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntCollapsedCementMixer\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntCollapsedChemistryStation\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"lootWorkstationsBustedHelper\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"GoreBlock1Prefab\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodLogSpike2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodMetalLogSpike3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodPillar100\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodMetalPillar100\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stainlessSteelWedgeIncline\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodWedgeStairs\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"solidWoodFrameWedgeTipCNRFullTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodWedgeTipCNRFullTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedCotton3Harvest\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"lootForgeBustedHelper\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"lootWorkbenchBustedHelper\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"lootCementMixerBustedHelper\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntShippingCrateHero\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntLootCrateHero\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteCTRPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodMetalWedgeTipCNRFullTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"railingBlackSheet\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"solidScrapIronFrameWedgeTipCNRFullTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntBookStoreBookcase\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapIronWedgeTipCNRFullTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rScrapIronWedgeTipCNRFullTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteWedgeTipCNRFullTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredRConcreteWedgeTipCNRFullTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcreteWedgeTipCNRFullTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treePlantedMaple17mPlus\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rScrapIronBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelWedgeTipCNRFullTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stainlessSteelWedgeTipCNRFullTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodMetalWedgeStairs\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cobblestoneWedgeTipCNRFullTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"solidWoodFrameWedgeTipStairs\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodWedgeTipStairs\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodWedgeTipStairs\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodMetalWedgeTipStairs\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cobblestoneWedge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cobblestoneWedgeTip\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cobblestonePlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cobblestoneCNRFull\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cobblestoneCNRInside\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cobblestonePillar100\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cobblestonePillar50\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cobblestonePole\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedCoffee2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedCoffee3Harvest\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedGoldenrod2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedGoldenrod3Harvest\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stainlessSteelCTRPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeBrownGrassDiagonal\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeTallGrassDiagonal\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stainlessSteelCNRFull\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stainlessSteelCNRRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treePlainsGrassDiagonal\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteEighth\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteQuarter\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteHalf\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stainlessSteelCNRInside\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteCNRRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concretePyramid\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"solidWoodFrameWedgeTipCNRFullBottom\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stainlessSteelSupport\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteCNRFull\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concretePole\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concretePlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodWedgeTipCNRFullBottom\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cobblestoneBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodMetalWedgeTipCNRFullBottom\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stainlessSteelPyramid\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stainlessSteelPillar100\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stainlessSteelPillar50\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"iBeamSmall\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"iBeam\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"iBeamCorner\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"iBeamBent\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"iBeamSupport\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"iBeamFoundation\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"iBeamCentered\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"iBeamCornerCentered\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"iBeamTCentered\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"iBeamCenteredFoundation\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"iBeam5wayCentered\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"iBeam6wayCentered\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stainlessSteelStairs25\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"solidScrapIronFrameWedgeTipCNRFullBottom\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapIronWedgeTipCNRFullBottom\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rScrapIronWedgeTipCNRFullBottom\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stainlessSteelPole\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteWedgeTipCNRFullBottom\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredRConcreteWedgeTipCNRFullBottom\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcreteWedgeTipCNRFullBottom\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corrugatedMetalCNRRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treePlainsTree\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"bridgeAsphaltBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"bridgeMetalRivetBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"bridgeConcreteBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"bridgeWoodBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelWedgeTipCNRFullBottom\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stainlessSteelWedgeTipCNRFullBottom\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"waterMovingBucket\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"waterStaticBucket\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"waterMoving\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"water\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cobblestoneWedgeTipCNRFullBottom\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodStairs50\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteStairs50\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cobblestoneStairs25\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntCabinetFiller\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodQuarter\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeForestGrassDiagonal\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"solidWoodFrameWedgeCNRInsideBottom\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodWedgeCNRInsideBottom\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodMetalWedgeCNRInsideBottom\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"solidScrapIronFrameWedgeStairs\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"solidScrapIronFrameWedgeCNRInsideBottom\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapIronWedgeCNRInsideBottom\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rScrapIronWedgeCNRInsideBottom\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"forestFlower\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedYucca3Harvest\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedAloe3Harvest\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedAloe2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"snowberry3Harvest\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteWedgeCNRInsideBottom\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredRConcreteWedgeCNRInsideBottom\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corrugatedMetalBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodArrowSlitHalf\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treePlainsTree2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeGreenBrownGrassDiagonal\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedChrysanthemum2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeDeadPineLeaf\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcreteWedgeCNRInsideBottom\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"lootForestHelper\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"lootHouseHelper\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"lootStreetHelper\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"lootYardHelper\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"lootChemistryStationBustedHelper\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"morticianDrawer\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"furnaceDrawer\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"greenDrawer\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelWedgeCNRInsideBottom\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodMetalArrowSlitHalf\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"lootGarageHelper\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedChrysanthemum3Harvest\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corrugatedMetalRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopTraderJoel2x5Wall\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopTraderJoel1x3Wall\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntCardboardBox\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stainlessSteelWedgeCNRInsideBottom\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"tire\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedCorn2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedCorn3Harvest\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedCornDead\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapIronWedgeStairs\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treePlantedGrass\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rScrapIronArrowSlitHalf\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cobblestoneWedgeCNRInsideBottom\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"solidWoodFrameWedgeCNRInsideTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodWedgeCNRInsideTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodMetalWedgeCNRInsideTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corrugatedMetalPillar100\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"solidScrapIronFrameWedgeCNRInsideTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corrugatedMetalPillar50\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"decals2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"decals3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"solidWoodFrameCNRRampFiller\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodCNRRampFiller\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodMetalCNRRampFiller\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapIronWedgeCNRInsideTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rScrapIronWedgeCNRInsideTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"gravelWedge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteWedgeCNRInsideTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteArrowSlitHalf\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredRConcreteArrowSlitHalf\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredRConcreteWedgeCNRInsideTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcreteWedgeCNRInsideTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcreteArrowSlitHalf\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelWedgeCNRInsideTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stainlessSteelWedgeCNRInsideTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntWashingMachine\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntTrashCompactor\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rScrapIronWedgeStairs\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelArrowSlitHalf\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stainlessSteelArrowSlitHalf\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cobblestoneWedgeCNRInsideTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"solidWoodFrameStairs25CornerCNR\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodStairs25CornerCNR\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodStairs25CornerCNR\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodMetalStairs25CornerCNR\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"solidScrapIronFrameStairs25CornerCNR\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"solidScrapIronFrameCNRRampFiller\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapIronCNRRampFiller\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rScrapIronCNRRampFiller\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredRConcreteCNRRampFiller\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcreteCNRRampFiller\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelCNRRampFiller\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapIronStairs25CornerCNR\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rScrapIronStairs25CornerCNR\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stainlessSteelCNRRampFiller\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cobblestoneCNRRampFiller\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"flagstoneGable\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cobblestoneRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"commercialBlindsTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"commercialBlindsBottom\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cobblestoneArrowSlitHalf\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntGreenDrawerInsecure\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntShippingCrateLabEquipment\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntLootCrateLabEquipment\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corrugatedMetalGable\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteStairs25CornerCNR\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredRConcreteStairs25CornerCNR\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteWedgeStairs\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"solidWoodFrameArch\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodArch\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodArch\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodWedgeTip\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodMetalArch\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"solidScrapIronFrameArch\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapIronArch\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteStairs25\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelStairs25CornerCNR\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntCabinetOldBottom\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntCabinetOldTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntWallOven\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapIronBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stainlessSteelStairs25CornerCNR\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"railingMetalSheet\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cable1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredRConcreteWedgeStairs\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodMetalWedgeTip\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"solidScrapIronFrameWedgeTip\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntMorticianDrawer\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cobblestoneStairs25CornerCNR\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plywoodOsbCTRPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rScrapIronArch\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteArch\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredRConcreteArch\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapIronWedgeTip\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rScrapIronWedgeTip\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"flagstoneArch3m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"flagstoneArch3m_tip\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"flagstoneArch3m_center\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcreteWedgeStairs\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeSnowyDeadBush\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcreteArch\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelArch\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"hubcapNoMine\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteWedge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stainlessSteelArch\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelWedgeStairs\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodWedgeIncline\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcreteCTREighth\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stainlessSteelCTREighth\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteCNRRampFiller\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteWedgeTip\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cobblestoneWedgeIncline\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cobblestoneCNRRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cobblestonePyramid\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stainlessSteelWedgeStairs\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cobblestoneWedgeStairs\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"railingWoodWhiteSheet\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"railingWroughtIronSheet\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedCotton2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"burntWoodWedge7\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"solidScrapIronFrameWedgeTipStairs\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"commercialDoor1_v1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"commercialDoor1_v2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"commercialDoor1_v3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"commercialDoor2_v1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"commercialDoor2_v2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"commercialDoor2_v3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"commercialDoor3_v1Legacy\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"commercialDoor3_v2Legacy\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"commercialDoor3_v3Legacy\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"commercialDoor4_v1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"commercialDoor4_v2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"commercialDoor4_v3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"houseFrontDoor1_v1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapIronWedgeTipStairs\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rScrapIronWedgeTipStairs\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"houseFrontDoor1_v2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"houseFrontDoor1_v3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntShippingCrateShamway\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntLootCrateShamway\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapIronPoleSupport\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapIronPoleSupport2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapIronCross\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"newTextureBlock01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"newTextureBlock02\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"newTextureBlock03\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"newTextureBlock04\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sidingWhiteBurntCTRPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntShippingCrateBookstore\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodBarricadeCTRPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntShippingCrateCarParts\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plainWoodBarricade02CTRPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"window02Broken01CTRPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"window02Broken02CTRPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodCNRRampFiller\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"window02Broken03CTRPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"window03Broke1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"window03Broke2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"window03Broke3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"window03Broke4\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"window03\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"window03Frame\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"glassIndustrial02Broken01CTRPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"glassIndustrial02Broken02CTRPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"window04Frame\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"window04\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"doorFrameWhite1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"doorFrameWhite2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"doorFrameMetal1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"doorFrameMetal2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"doorFrameWhiteBoarded1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"doorFrameWhiteBoarded2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"doorFrameMetalBoarded1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"doorFrameMetalBoarded2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntShippingCrateConstructionSupplies\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntLootCrateConstructionSupplies\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"houseFrontDoor2_v1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"houseFrontDoor2_v2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"houseFrontDoor2_v3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopGas\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopGasLit\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopGasWall\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopGasWallLit\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopGasLarge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopGasLargeLit\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopGasLargeWall\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopGasLargeWallLit\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopGunStore\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopGunStoreLit\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopGunStoreWall\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopGunStoreWallLit\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopGunStoreLarge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopGunStoreLargeLit\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopGunStoreLargeWall\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopGunStoreLargeWallLit\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntShippingCrateShotgunMessiah\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntLootCrateShotgunMessiah\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"ironDoor1_v2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"ironDoor1_v3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodHatch1_v2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodHatch1_v3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteWedgeTipStairs\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"torchWallHolder\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntStorageGenericInsecure\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cobblestoneHalf\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredRConcreteWedgeTipStairs\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntPillCase\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"newTextureBlock05\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"newTextureBlock06\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"newTextureBlock07\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"newTextureBlock08\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"newTextureBlock09\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"newTextureBlock10\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"newTextureBlock11\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"newTextureBlock12\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"newTextureBlock13\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodCatwalkWedge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"metalCatwalk\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"metalCatwalkRailing\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"metalRailing\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"metalCatwalkRailingCNR\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"metalStairsBoard\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"metalStairsBoardRailing\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"metalCatwalkWedgeRailing\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"metalCatwalkWedge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corrugatedMetalSheetBent1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corrugatedMetalSheetBent2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corrugatedMetalSheetBent3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corrugatedMetalSheetBent4\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corrugatedMetalWedge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapIronPillar50\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rScrapIronPillar50\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapIronCNRRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rScrapIronCNRRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapIronCTRPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rScrapIronCTRPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapIronCNRRound\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rScrapIronCNRRound\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapIronCNRRoundTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rScrapIronCNRRoundTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"candleWall\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopBookStore\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopBookStoreLit\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopBookStoreWall\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopBookStoreWallLit\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopBookStoreLarge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopBookStoreLargeLit\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopBookStoreLargeWall\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopBookStoreLargeWallLit\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopPharmacy\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopPharmacyLit\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopPharmacyWall\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopPharmacyWallLit\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopPharmacyLarge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopPharmacyLargeLit\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopPharmacyLargeWall\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopPharmacyLargeWallLit\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopToolStore\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopToolStoreLit\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopToolStoreWall\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopToolStoreWallLit\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopToolStoreLarge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopToolStoreLargeLit\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopToolStoreLargeWall\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopToolStoreLargeWallLit\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"candleTable\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cobblestoneArch\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntShippingCrateWorkingStiffs\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntLootCrateWorkingStiffs\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodCNRRound\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodCNRRoundTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcreteWedgeTipStairs\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"brickGable\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corrugatedMetalStairs25\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corrugatedMetalPole\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodCNRRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cashRegisterEmpty\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntShoppingCart\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"shoppingBasketEmpty\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntShoppingBasket\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredConcreteStairs50\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"wallTorchPlayer\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"candleWallPlayer\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"flagstoneStairs50\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelWedgeTipStairs\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stainlessSteelWedgeTipStairs\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodGable\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopGrocery\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopGroceryLit\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopGroceryWall\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopGroceryWallLit\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopGroceryLarge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopGroceryLargeLit\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopGroceryLargeWall\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopGroceryLargeWallLit\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cobblestoneWedgeTipStairs\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodCTRPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntMountainManStorageChest\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntApacheArtifactChest\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plainWoodWindowPlug\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"WindowPlug01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"WindowPlug02\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"WindowPlug03\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"WindowPlug04\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"mushroom01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"mushroom02\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stalactite01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stalactite02\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stalactite03\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stalactite04\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cobweb\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"hangingMoss\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stalagmite01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stalagmite02\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stalagmite03\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stalagmite04\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"artcube\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodPillar50\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"newTextureBlock14\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"newTextureBlock15\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"newTextureBlock16\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeHauntedTreeWasteland42\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"emberPile1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"emberPile2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"emberPile3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"emberPile4\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"window03WoodBroke1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"window03WoodBroke2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"window03WoodBroke3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"window03WoodBroke4\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"window03WoodFrame\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"chainLink\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelStairs25\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntDeskSafeInsecure\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntWallSafeInsecure\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntGunSafeInsecure\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"garageDoorMetal_v2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"garageDoorMetal_v3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rScrapIronLogSpike5\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"chainLinkFenceBottomPole\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"chainLinkFenceTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"chainLinkFenceTopPole\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"chainLinkPole\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"chainLinkCornerTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"chainLinkCornerBottom\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"chainLinkFenceTopPole2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelLogSpike6\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"spotlightNailedDownPOI\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"vaultDoor02\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"vaultDoor03\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"randomCarsHelper\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntCar03SedanDamage0\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntCar03SedanDamage1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntCar03SedanDamage2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapHatch_v2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapHatch_v3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntCoffin\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"vaultHatch_v2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"vaultHatch_v3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"waterSource8\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"brickDestroyedBlock01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"brickDestroyedBlock02\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"brickDestroyedBlock03\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"shapeTestRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"shapeTestCap\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntDumpster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"garbage_decor1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"garbage_decor2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"garbage_decor3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredRConcreteCNRInside\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelCNRInside\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"controlPanelBase01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"controlPanelBase02\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"controlPanelBase03\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"controlPanelBase04\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"controlPanelBase05\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"controlPanelBase06\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"controlPanelTop01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"controlPanelTop02\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"controlPanelTop03\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"controlPanelTop04\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"controlPanelTop05\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"controlPanelTop06\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"controlPanelTop07\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"controlPanelCorner\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"ductSoft\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"ductSoftCorner\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"controlPanelBase07\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"controlPanelBase08\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"ductControlPanel\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"ductControlPanelCorner\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"ductSoftCorner2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapIronWindowPlug05CTRPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rScrapIronWindowPlug06CTRPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcreteCTRPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelCTRPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodPole\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodMetalPole\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapIronPole\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rScrapIronPole\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcretePole\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelPole\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"industrialLight01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"electricalBox01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"conduit01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"conduit01Curve\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"conduit01End\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"leatherCouchSofa\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"leatherChairCouchSofa\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treePlantedWinterPine6m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treePlantedWinterPine13m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treePlantedWinterPine16m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treePlantedWinterPine19m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodPoleSupportCtr\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"forestCurb\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"forestCNRCurb\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodPoleSupport\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sidewalkCurb\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plainsCurb\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodCrossCtr\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodCross\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plainsCNRCurb\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"wastelandCurb\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"wastelandCNRCurb\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodPoleSupport2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"elevatorTest\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rustyIronPoleSupportCtr\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rustyIronCrossCtr\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"roadRailing\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"roadRailingPole\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"roadRailingEnd\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"roadRailingEnd2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"brickCNRRound\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntCabinetOldCNRRoundFiller\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"brickCNRRoundTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corrugatedMetalCNRRoundTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corrugatedMetalCNRRound\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treePlantedWinterPine19mPlus\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodCTRpole\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodMetalCTRpole\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rScrapIronCTRpole\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cobblestoneCTRpole\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteCTRpole\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredRConcreteCTRpole\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcreteCTRpole\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelCTRpole\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stainlessSteelCTRpole\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"burningBarrel\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stainlessSteelBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stainlessSteelCNRRoundTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stainlessSteelCNRRound\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"conduit01Corner\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sandbag_1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sandbag_2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sandbag_3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sandbag_4\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sandbagCorner\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sandbagEnd\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sandbagEnd2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sandbagCornerDMG1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sandbagCorner2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sandbagCorner2DMG1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sandbag_5\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sandbag_5DMG1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"spawnTrader\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"flagPoleWhiteRiver\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"arrowSlit\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntVendingMachineTrader\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopOpen\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"loudspeaker\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcreteStairs25CornerCNR\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodWedgeTipCNRFullBottom\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"archTest\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodWedgeTipCNRFullTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rampGutterTest\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rampGutterOutsideCornerTest\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"trim01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"opaqueBusinessGlass\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cube3mTest\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cube3x3x1Test\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"arch_3m_windowTEST\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"arch_3m_center_windowTEST\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"arch_3m_window_tipTEST\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"arch_3m_full_TEST\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteDestroyed01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteDestroyed02\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteDestroyed03\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteDestroyed04\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sleeperSit\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sleeperBack\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sleeperSideLeft\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sleeperSideRight\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sleeperStomach\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteDestroyed05\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteDestroyed06\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteDestroyed07\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sleeperIdle\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntCollapsedBatterybank\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntCollapsedGeneratorbank\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rampCornerCutoutTest\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"curve_3x3x1TEST\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"turret_3x3TEST\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"turret_4x4x6TEST\" />"));
                }
                sw.WriteLine("    </Items>");
                sw.WriteLine("</InvalidItems>");
                sw.Flush();
                sw.Close();
            }
            fileWatcher.EnableRaisingEvents = true;
        }

        private static void InitFileWatcher()
        {
            fileWatcher.Changed += new FileSystemEventHandler(OnFileChanged);
            fileWatcher.Created += new FileSystemEventHandler(OnFileChanged);
            fileWatcher.Deleted += new FileSystemEventHandler(OnFileChanged);
            fileWatcher.EnableRaisingEvents = true;
            IsRunning = true;
        }

        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            LoadXml();
        }

        public static void CheckInv(ClientInfo _cInfo, PlayerDataFile _playerDataFile)
        {
            if (_cInfo != null && !GameManager.Instance.adminTools.IsAdmin(_cInfo.playerId))
            {
                AdminToolsClientInfo Admin = GameManager.Instance.adminTools.GetAdminToolsClientInfo(_cInfo.playerId);
                if (Admin.PermissionLevel <= LevelToIgnore)
                {
                    for (int i = 0; i < _playerDataFile.inventory.Length; i++)
                    {
                        ItemStack _intemStack = new ItemStack();
                        ItemValue _itemValue = new ItemValue();
                        _intemStack = _playerDataFile.inventory[i];
                        _itemValue = _intemStack.itemValue;
                        int _count = _playerDataFile.inventory[i].count;
                        if (_count > 0 && _itemValue != null && !_itemValue.Equals(ItemValue.None) && _cInfo != null)
                        {
                            int _maxAllowed = ItemClass.list[_itemValue.type].Stacknumber.Value;
                            string _name = ItemClass.list[_itemValue.type].GetItemName();
                            if (AnounceInvalidStack && _count > _maxAllowed)
                            {
                                string _phrase3;
                                if (!Phrases.Dict.TryGetValue(3, out _phrase3))
                                {
                                    _phrase3 = "{PlayerName} you have a invalid item stack: {ItemName} {ItemCount}. Max per stack: {MaxPerStack}.";
                                }
                                _phrase3 = _phrase3.Replace("{PlayerName}", _cInfo.playerName);
                                _phrase3 = _phrase3.Replace("{ItemName}", _name);
                                _phrase3 = _phrase3.Replace("{ItemCount}", _count.ToString());
                                _phrase3 = _phrase3.Replace("{MaxPerStack}", _maxAllowed.ToString());
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("[FF8000]{0}[-]", _phrase3), "Server", false, "", false));
                                ChatLog.Log(_phrase3, "Server");
                            }
                            if (IsEnabled && dict.ContainsKey(_name))
                            {
                                if (BanPlayer)
                                {
                                    string _phrase4;
                                    if (!Phrases.Dict.TryGetValue(4, out _phrase4))
                                    {
                                        _phrase4 = "Cheat Detected: Auto banned {PlayerName} for having a invalid item: {ItemName}.";
                                    }
                                    _phrase4 = _phrase4.Replace("{PlayerName}", _cInfo.playerName);
                                    _phrase4 = _phrase4.Replace("{ItemName}", _name);
                                    GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("[FF8000]{0}[-]", _phrase4), "Server", false, "", false);
                                    SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 10 years \"Invalid Item {1}\"", _cInfo.entityId, _name), _cInfo);
                                }
                                else
                                {
                                    string _phrase5;
                                    if (!Phrases.Dict.TryGetValue(5, out _phrase5))
                                    {
                                        _phrase5 = "Cheat Detected: Auto kicked {PlayerName} for having a invalid item: {ItemName}.";
                                    }
                                    _phrase5 = _phrase5.Replace("{PlayerName}", _cInfo.playerName);
                                    _phrase5 = _phrase5.Replace("{ItemName}", _name);
                                    GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("[FF8000]{0}[-]", _phrase5), "Server", false, "", false);
                                    SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Invalid Item: {1}\"", _cInfo.entityId, _name), _cInfo);
                                }
                                break;
                            }
                        }
                    }
                    for (int i = 0; i < _playerDataFile.bag.Length; i++)
                    {
                        ItemStack _intemStack = new ItemStack();
                        ItemValue _itemValue = new ItemValue();
                        _intemStack = _playerDataFile.bag[i];
                        _itemValue = _intemStack.itemValue;
                        int _count = _playerDataFile.bag[i].count;
                        if (_count > 0 && _itemValue != null && !_itemValue.Equals(ItemValue.None) && _cInfo != null)
                        {
                            int _maxAllowed = ItemClass.list[_itemValue.type].Stacknumber.Value;
                            string _name = ItemClass.list[_itemValue.type].GetItemName();
                            if (AnounceInvalidStack && _count > _maxAllowed)
                            {
                                string _phrase3;
                                if (!Phrases.Dict.TryGetValue(3, out _phrase3))
                                {
                                    _phrase3 = "{PlayerName} you have a invalid item stack: {ItemName} {ItemCount}. Max per stack: {MaxPerStack}.";
                                }
                                _phrase3 = _phrase3.Replace("{PlayerName}", _cInfo.playerName);
                                _phrase3 = _phrase3.Replace("{ItemName}", _name);
                                _phrase3 = _phrase3.Replace("{ItemCount}", _count.ToString());
                                _phrase3 = _phrase3.Replace("{MaxPerStack}", _maxAllowed.ToString());
                                _cInfo.SendPackage(new NetPackageGameMessage(EnumGameMessages.Chat, string.Format("[FF8000]{0}[-]", _phrase3), "Server", false, "", false));
                                ChatLog.Log(_phrase3, "Server");
                            }
                            if (IsEnabled && dict.ContainsKey(_name))
                            {
                                if (BanPlayer)
                                {
                                    string _phrase4;
                                    if (!Phrases.Dict.TryGetValue(4, out _phrase4))
                                    {
                                        _phrase4 = "Cheat Detected: Auto banned {PlayerName} for having a invalid item: {ItemName}.";
                                    }
                                    _phrase4 = _phrase4.Replace("{PlayerName}", _cInfo.playerName);
                                    _phrase4 = _phrase4.Replace("{ItemName}", _name);
                                    GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("[FF8000]{0}[-]", _phrase4), "Server", false, "", false);
                                    SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 10 years \"Invalid Item {1}\"", _cInfo.entityId, _name), _cInfo);
                                }
                                else
                                {
                                    string _phrase5;
                                    if (!Phrases.Dict.TryGetValue(5, out _phrase5))
                                    {
                                        _phrase5 = "Cheat Detected: Auto kicked {PlayerName} for having a invalid item: {ItemName}.";
                                    }
                                    _phrase5 = _phrase5.Replace("{PlayerName}", _cInfo.playerName);
                                    _phrase5 = _phrase5.Replace("{ItemName}", _name);
                                    GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("[FF8000]{0}[-]", _phrase5), "Server", false, "", false);
                                    SdtdConsole.Instance.ExecuteSync(string.Format("kick {0} \"Invalid Item: {1}\"", _cInfo.entityId, _name), _cInfo);
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}