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

        public static void Load()
        {
            if (IsEnabled && !IsRunning)
            {
                LoadInvalidItemsXml();
                InitFileWatcher();
            }
        }

        public static void Unload()
        {
            fileWatcher.Dispose();
            IsRunning = false;
        }

        private static void LoadInvalidItemsXml()
        {
            if (!Utils.FileExists(filePath))
            {
                UpdateInvalidItemsXml();
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
            XmlNode _ICheckXml = xmlDoc.DocumentElement;
            foreach (XmlNode childNode in _ICheckXml.ChildNodes)
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

        private static void UpdateInvalidItemsXml()
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
                    sw.WriteLine(string.Format("        <item itemName=\"radiated\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"potassiumNitrate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"ironOre\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"clay\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"leadOre\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"bedrock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sandStone\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"desertGround\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"ice\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"fertileGrass\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"fertileFarmland\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"copperOre\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"coalOre\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"terrainFiller\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"hardSod\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodDebris\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"oilDeposit\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodWeakNoUpgradeMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodNoUpgradeMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"redWoodMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeMasterGrowing\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"metalNoUpgradeMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stoneToAdobeMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteNoUpgradeMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcreteMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredConcreteMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteFormMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"brickNoUpgradeMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"brickMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"adobeMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rScrapIronMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rebarFrameMaster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"stoneMaster\" />"));
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
                    sw.WriteLine(string.Format("        <item itemName=\"pouredConcreteCNRRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredConcreteCNRFull\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcreteCNRRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelWedge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodMetalPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapIronPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcretePlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelCNRRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredConcreteEighth\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcreteEighth\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodSupport\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodMetalSupport\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapIronSupport\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rScrapIronSupport\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcreteSupport\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelEighth\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredConcreteHalf\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcreteHalf\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelSupport\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelHalf\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredConcretePole\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredConcretePyramid\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcretePyramid\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelPyramid\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapIronPillar100\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rScrapIronPillar100\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredConcreteQuarter\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcreteQuarter\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcretePillar100\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelPillar100\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelQuarter\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredConcreteStairs25\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcreteStairs25\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredConcreteWedge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteFormBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredConcreteBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcreteBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredConcreteWedgeTip\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcreteWedgeTip\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelWedgeTip\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredConcretePlate\" />"));
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
                    sw.WriteLine(string.Format("        <item itemName=\"pouredConcreteSupport\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteFormRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredConcreteRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcreteRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcreteCNRFull\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteSupport\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredConcretePillar100\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredConcreteCTRPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcreteCNRInside\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredConcretePillar50\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelCNRFull\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedBlueberry1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedBlueberry3Harvest\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedPotato1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedPotato2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedPotato3Harvest\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedBlueberry2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"lootWasteland\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"lootDesert\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"lootBurntForest\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"lootPlains\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"animalGore\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntStorageHealthInsecure\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"greenMetalRoofCNRInside\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"brickPaintedIndustrialTileBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodStairs25\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntGarageStorage\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"brickpaintedBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeSnowyGrassDiagonal\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"tileFloor3Block\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"tile3Stairs50\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rConcretePillar50\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"panelingBurntRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cabinWoodWedge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cabinWoodWedgeTip\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntMedicineCabinet\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"awningGreenCNRFull\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"awningRedCNRInside\" />"));
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
                    sw.WriteLine(string.Format("        <item itemName=\"cntOven\" />"));
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
                    sw.WriteLine(string.Format("        <item itemName=\"adobeWhitePyramid\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntTrash_can01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntBin\" />"));
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
                    sw.WriteLine(string.Format("        <item itemName=\"cntGasPump\" />"));
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
                    sw.WriteLine(string.Format("        <item itemName=\"cntCorpseLoot01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"tanDrywallPillar100\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"poleTop01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"streetLight01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"airConditioner\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntCorpseLoot02\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treasureChest\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cinderBlocks01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"solidWoodFrameBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"decals1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"solidWoodFrameRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"tileFloor5Block\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"redMetalPillar100\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"redMetalBlockDuplicate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"redMetalRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"tileIndustrialDecayedBrickBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"tileIndustrialPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteTrimRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteTrimCNRRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corrugatedMetalSheet\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"lootMP\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"shinglesAsphaltGreenWedgeRename\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sidewalkHalf\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"metalTrussingSheet\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"blueConcretePillar100\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"orangeConcretePillar100\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corrugatedMetalCTRPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sidingTileIndustrialWoodBlock\" />"));
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
                    sw.WriteLine(string.Format("        <item itemName=\"sidingTileIndustrialWhiteOldWoodBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntBackpack01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntBackpack02\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntBackpack03\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntDuffle01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntSportsBag01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntSportsBag02\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntPurse01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"bed01Frame\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"mattress\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"mattressFlat\" />"));
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
            LoadInvalidItemsXml();
        }

        public static void CheckInv(ClientInfo _cInfo, PlayerDataFile _playerDataFile)
        {
            if (_cInfo != null && !GameManager.Instance.adminTools.IsAdmin(_cInfo.playerId))
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
                            string _phrase3 = "{PlayerName} you have a invalid item stack: {ItemName} {ItemCount}. Max per stack: {MaxPerStack}.";
                            if (!Phrases.Dict.TryGetValue(3, out _phrase3))
                            {
                                Log.Out("[SERVERTOOLS] Phrase 3 not found using default.");
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
                                string _phrase4 = "Cheat Detected: Auto banned {PlayerName} for having a invalid item: {ItemName}.";
                                if (!Phrases.Dict.TryGetValue(4, out _phrase4))
                                {
                                    Log.Out("[SERVERTOOLS] Phrase 4 not found using default.");
                                }
                                _phrase4 = _phrase4.Replace("{PlayerName}", _cInfo.playerName);
                                _phrase4 = _phrase4.Replace("{ItemName}", _name);
                                GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("[FF8000]{0}[-]", _phrase4), "Server", false, "", false);
                                SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 10 years \"Invalid Item {1}\"", _cInfo.entityId, _name), _cInfo);
                            }
                            else
                            {
                                string _phrase5 = "Cheat Detected: Auto kicked {PlayerName} for having a invalid item: {ItemName}.";
                                if (!Phrases.Dict.TryGetValue(5, out _phrase5))
                                {
                                    Log.Out("[SERVERTOOLS] Phrase 5 not found using default.");
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
                            string _phrase3 = "{PlayerName} you have a invalid item stack: {ItemName} {ItemCount}. Max per stack: {MaxPerStack}.";
                            if (!Phrases.Dict.TryGetValue(3, out _phrase3))
                            {
                                Log.Out("[SERVERTOOLS] Phrase 3 not found using default.");
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
                                string _phrase4 = "Cheat Detected: Auto banned {PlayerName} for having a invalid item: {ItemName}.";
                                if (!Phrases.Dict.TryGetValue(4, out _phrase4))
                                {
                                    Log.Out("[SERVERTOOLS] Phrase 4 not found using default.");
                                }
                                _phrase4 = _phrase4.Replace("{PlayerName}", _cInfo.playerName);
                                _phrase4 = _phrase4.Replace("{ItemName}", _name);
                                GameManager.Instance.GameMessageServer(_cInfo, EnumGameMessages.Chat, string.Format("[FF8000]{0}[-]", _phrase4), "Server", false, "", false);
                                SdtdConsole.Instance.ExecuteSync(string.Format("ban add {0} 10 years \"Invalid Item {1}\"", _cInfo.entityId, _name), _cInfo);
                            }
                            else
                            {
                                string _phrase5 = "Cheat Detected: Auto kicked {PlayerName} for having a invalid item: {ItemName}.";
                                if (!Phrases.Dict.TryGetValue(5, out _phrase5))
                                {
                                    Log.Out("[SERVERTOOLS] Phrase 5 not found using default.");
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