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
                    sw.WriteLine(string.Format("        <item itemName=\"cinderBlocks03\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntBirdnest\" />"));
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
                    sw.WriteLine(string.Format("        <item itemName=\"treeMaple15m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeMaple16m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeMaple17m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeBurntPine01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeBurntPine02\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeBurntPine03\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeDeadTree01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeDeadTree02\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"brickDecayedBroke1Block\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"brickDecayedBroke2Block\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"brickDecayedBroke3Block\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeWinterEverGreen\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeWinterPine01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntTreeStump\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeWinterPine02\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeWinterPine03\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeDesertShrub\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntBackpackDropped\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteDestroyed1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteDestroyed2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteDestroyed3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteDestroyed4\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteDestroyed5\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteDestroyed6\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteDestroyed7\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteDestroyed8\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteDestroyed9\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"tanBrickDecayedBroke1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"tanBrickDecayedBroke2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"tanBrickDecayedBroke3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"paintedBrickBroke1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"paintedBrickBroke2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"paintedBrickBroke3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteTrim2Broke1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteTrim2Broke2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteTrim2Broke3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteTrim2Broke4\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"whiteSidingWoodPanelBurnt1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"whiteSidingWoodPanelBurnt2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"whiteSidingWoodPanelBurnt3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"whiteSidingWoodPanelBurnt4\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"whiteSidingWoodPanelBurnt5\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"whiteSidingWoodPanelBurnt6\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"whiteSidingWoodPanelBurnt7\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntFileCabinet\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"schoolDesk\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntLockers\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"fountain\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntGraniteSink\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rockResource\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"blueMetalRamp1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"blueMetalBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"blueDrywallBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"brickPaintedTanDrywallBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"brickPaintedTanBaseboardBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"brickPaintedBlueDrywallBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rockResource02\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"secureReinforcedDoorWooden\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"metalReinforcedDoorWooden\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rockResourceBroke1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rockResourceBroke2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rockResource02Broke1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rockResource02Broke2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rockResource02Broke3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"window02Frame\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"window02\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeCactus01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeCactus02\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeCactus03\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeCactus04\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeCactus05\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeCactus06\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treePlantedMountainPine1m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treePlantedMountainPine6m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treePlantedMountainPine8m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treePlantedMountainPine13m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treePlantedMountainPine16m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treePlantedMountainPine19m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treePlantedMaple1m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treePlantedMaple6m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treePlantedMaple13m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treePlantedMaple15m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treePlantedMaple17m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"adobeWhiteDrywallBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"adobeWhiteDrywallBaseboardBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"adobePeachDrywallBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"adobePeachDrywallBaseboardBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cremeDrywallBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cremeDrywallBaseboardBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"tileFloor5Stairs25\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"asphaltDrywallBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"GoreBlock1Prefab\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"GoreBlock1BonesPrefab\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodLogSpike2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodMetalLogSpike3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodPillar100\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rWoodMetalPillar100\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"dropCeiling02Block\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cabinetOldFillerBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cabinetFillerBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cabinetOldTopPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cabinetTopPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedCotton1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"wallPlasterBaseboard2Block\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"wallPlasterMiddle2Block\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"wallPlasterTop2Block\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedCotton3HarvestLarge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"drywallWhiteCNRInside\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"drywallWhitePlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"dropCeiling2RoofAsphaltBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"railingBlackSheet\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"tileFloor3Stairs25\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntBookStoreBookcase\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteTrim2TanDrywallBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"drywallTrim2SidesBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cremeBacksplashBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"drywallOldwoodBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cremeDrywallBaseboard2SidesBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapIronBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rScrapIronBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedCoffee1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedCoffee2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedCoffee3Harvest\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedGoldenrod1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedGoldenrod2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedGoldenrod3HarvestLarge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeBrownGrassDiagonal\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeTallGrassDiagonal\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treePlainsGrassDiagonal\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteBricksBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"panelingBurntStairs25\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cobblestoneBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"garageDoor1CTRPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rustyIronPole\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"metalRivetBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"metalRivetRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"curtain_top1Sheet3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sidingBluePlasterBaseboardBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sidingGreenPlasterBaseboardBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sidingWhitePlasterBaseboardBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sidingYellowPlasterBaseboardBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sidingTanPlasterBaseboardBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"brickPlasterBaseboardBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"awningRedPyramid\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"awningRedGable\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"shinglesAspaltBlueWedge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sidingWhiteBurnt01Ramp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rustyIronStairs25\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"shinglesAsphaltBlueWedgeTip\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"panelingBurntBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corrugatedMetalCNRRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treePlainsTree\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"bridgeAsphaltBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"bridgeMetalRivetBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"bridgeConcreteBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"bridgeWoodBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteTrimTanDrywallBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"dropCeilingRedWoodTrimBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"waterMovingBucket\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"waterStaticBucket\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"waterMoving\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"water\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"asphaltShingleCNRInside\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"brickModernGable\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"tanStuccoBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"tanStuccoPlasterBaseboardBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodStairs50\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteStairs50\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rug01Block\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodQuarter\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"Carpet1Block\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"tanStuccotileFloor1Block\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"adobeTanBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"adobeTanRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"adobeWhiteTrimRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"adobeWhiteTrimCNRFull\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"adobeWhiteTrimCNRInside\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"greenShinglesBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"adobeWhiteWedgeTip\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedCoffee1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedCoffee2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedCoffee3Harvest\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedGoldenrod1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedGoldenrod2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedGoldenrod3HarvestLarge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeBrownGrassDiagonal\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeTallGrassDiagonal\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treePlainsGrassDiagonal\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteBricksBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"panelingBurntStairs25\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cobblestoneBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"garageDoor1CTRPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rustyIronPole\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"metalRivetBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"metalRivetRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"curtain_top1Sheet3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sidingBluePlasterBaseboardBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sidingGreenPlasterBaseboardBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sidingWhitePlasterBaseboardBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sidingYellowPlasterBaseboardBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sidingTanPlasterBaseboardBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"brickPlasterBaseboardBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"awningRedPyramid\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"awningRedGable\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"shinglesAspaltBlueWedge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sidingWhiteBurnt01Ramp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rustyIronStairs25\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"shinglesAsphaltBlueWedgeTip\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"panelingBurntBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corrugatedMetalCNRRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treePlainsTree\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"bridgeAsphaltBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"bridgeMetalRivetBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"bridgeConcreteBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"bridgeWoodBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteTrimTanDrywallBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"dropCeilingRedWoodTrimBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"waterMovingBucket\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"waterStaticBucket\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"waterMoving\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"water\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"asphaltShingleCNRInside\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"brickModernGable\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"tanStuccoBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"tanStuccoPlasterBaseboardBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodStairs50\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteStairs50\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rug01Block\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodQuarter\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"Carpet1Block\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"tanStuccotileFloor1Block\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"adobeTanBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"adobeTanRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"adobeWhiteTrimRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"adobeWhiteTrimCNRFull\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"adobeWhiteTrimCNRInside\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"greenShinglesBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corrugatedMetalBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treePlainsTree2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeGreenBrownGrassDiagonal\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"tanDrywallBaseboardRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"tanDrywallRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"drywallWhiteBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeDeadPineLeaf\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sidewalkBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"lootForest\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"lootHouse\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"lootStreet\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"lootYard\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"panelingBurntSupport\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"DrywallIndustrialTileBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"lootGarage\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corrugatedMetalRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntCardboardBox\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"panelingBurntPillar100\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"tire\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedCorn1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedCornTop1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedCornTop2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedCornStalk1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedCornStalk2Harvest\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedCornTop3Harvest\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"grownUpCornTop2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"tileFloor4Block\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treePlantedGrass\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"oldStuccoWoodWedge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"oldStuccoWoodWedgeTip\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"awningRedCNRFull\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"awningTanCNRFull\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"awningTanCNRInside\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"awningGreenCNRInside\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"peachDrywallBaseboardPillar100\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"powerPoleWoodPillar100\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rustyIronPillar100\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corrugatedMetalPillar100\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"powerPoleWoodPillar50\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rustyIronPillar50\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corrugatedMetalPillar50\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"decals2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"decals3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corrugatedMetalSheetDuplicate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"whiteMetalPillar100\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"whiteMetalBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"whiteMetalRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"chaulkboardPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteTrim2Ramp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteTrim3TanDrywallBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"gravelWedge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"barnWoodWedge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"brickMetalWedge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodGreenShinglesRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodAsphaltShinglesRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntCabinetOldSink\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"flagstoneDrywallBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"brickModernTanDrywallBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"brickModernTanBaseboardBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"flagstoneDrywallBaseboardBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"brickAsphaltBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"redWoodFloorBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"brickDecayedAsphaltBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"tileIndustrialConcreteBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"brickDecayedTanBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteTrim3Block\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteTrimBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"gravelHalf\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"graveStone1Half\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteAsphaltHalf\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"metalAsphaltHalf\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"brickMetalHalf\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"dotsGreenShinglesRampDuplicate1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"barnShinglesRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"dotsAsphaltShinglesRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"brickModern01ShinglesRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"flagstoneShinglesRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"drywallCeilingRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"dotsGreenShinglesCNRRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"dotsAsphaltShinglesCNRRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"dotsWoodShinglesWoodCNRRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"greenShinglesPyramid\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"shinglesAsphaltPyramid\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"asphaltBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"dropCeilingRoofAsphaltBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteTileGreenBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteTileBrownBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"dotsGreenShinglesCNRFull\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"asphaltShinglesCNRFull\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"shinglesAsphaltCNRFull\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodPainted1Pole\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodPainted3Pole\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"dotsWoodShinglesRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"oldStuccoWoodShakeRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodAsphaltShinglesRampDuplicate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"awningTanRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"awningGreenRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"awningTanBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"awningGreenBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cabinShinglesWoodRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cabinOldWoodRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"dotsWoodShinglesRampDuplicate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"dotsAsphaltShinglesRampDuplicate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"dotsGreenShinglesRampDuplicate2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"houseWoodWhiteRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rustyIronBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corrugatedMetalBlockDuplicate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rustyIronRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"curtain_bottom1Sheet3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sidingBluePlasterMiddleBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sidingBluePlasterTopBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sidingBlueOldWoodBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sidingGreenPlasterMiddleBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sidingGreenPlasterTopBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sidingGreenOldWoodBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sidingWhitePlasterMiddleBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sidingWhitePlasterTopBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sidingWhiteOldWoodBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sidingWhiteBurntBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sidingYellowPlasterMiddleBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sidingYellowPlasterTopBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sidingYellowOldWoodBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sidingTanPlasterMiddleBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sidingTanPlasterTopBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sidingTanOldWoodBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"brickPlasterMiddleBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"brickPlasterTopBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"brickOldWoodBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"awningTanPyramid\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"awningGreenPyramid\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"awningTanGable\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"awningGreenGable\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"shinglesAsphaltWhiteWedge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"shinglesAsphaltYellowWedge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"shinglesAsphaltTanWedge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"shinglesAsphaltBurntWedge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sidingWhiteBurnt02Ramp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"asphaltRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"gravelRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"brickDecayedRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cobblestoneRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"brickDecayedTanRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"brickPaintedRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"metalRivetStairs25\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"shinglesAsphaltGreenWedge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"shinglesAsphaltWhiteWedgeDuplicate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"shinglesAsphaltYellowWedgeDuplicate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"shinglesAsphaltTanWedgeDuplicate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"shinglesAsphaltBurntWedgeDuplicate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"dotsGreenShinglesCNRInside\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"dotsAsphaltShinglesCNRInside\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"dotsWoodShinglesCNRInside\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodShinglesCNRInside\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodCNRInsideDuplicate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"flagstoneGable\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"dotsGreenShinglesGable\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"dotsAsphaltShinglesGable\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corrugatedMetalGable\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"dotsWoodShinglesGable\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"tanStuccoPlasterMiddleBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"tanStuccoPlasterTopBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"tanStuccoOldWoodBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodPainted1Stairs25\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"carpet1Stairs25\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"carpet2Stairs25\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"tileFloor4Stairs25\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntCabinetOldBottom\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntCabinetOldTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntWallOven\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"Carpet2Block\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"Carpet2WoodPainted2Block\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"tanStuccotileFloor2Block\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"tanStuccoCarpet1Block\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"tanStuccoCarpet2Block\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"OldWoodWoodPainted2Block\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"ShinglesAsphaltFlatBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteBlueDrywallBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"dotsWoodBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"dotsGreenShinglesBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodPainted1Block\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"tileFloor2WoodPainted2Block\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"tileFloor1WoodPainted2Block\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"Carpet1WoodPainted2Block\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapIronBlockDuplicate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"railingMetalSheet\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cable1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"garageDoor1Plate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"panelingBurntPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"fenceWoodSheet\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"shutters1Plate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plywoodOsbPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plywoodOsbCTRPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"drywallBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"peachDrywallBaseboardBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"dropCeilingBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rug01DrywallBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"tileFloor3DrywallBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"carpet3DrywallBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"tileBackspashDrywallBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeSnowyDeadBush\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"BaseboardIndustrialTileBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"brickModernTanIndustrialTileBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sidewalkWedge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"asphaltWedge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"gravelWedgeTip\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"barnWoodWedgeTip\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"asphaltWedgeTip\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"brickMetalWedgeTip\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"barnMetalRoofRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"barnWoodRoofRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"railingWoodWhiteSheet\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"railingWroughtIronSheet\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plantedCotton2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteTrim2TanBaseboardBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteTrim2IndustrialTileBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteTrim3TanBaseboardBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteTrim3IndustrialTileBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteTrimTanBaseboardBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteTrimIndustrialTileBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteTanDrywallBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteTanBaseboardBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteIndustrialTileBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cabinetFiller2Block\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cabinetOldFiller2Block\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"commercialDoor1_v1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"commercialDoor1_v2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"commercialDoor1_v3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"commercialDoor2_v1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"commercialDoor2_v2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"commercialDoor2_v3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"commercialDoor3_v1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"commercialDoor3_v2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"commercialDoor3_v3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"commercialDoor4_v1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"commercialDoor4_v2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"commercialDoor4_v3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"houseFrontDoor1_v1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"houseFrontDoor1_v2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"houseFrontDoor1_v3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntSupplyCrateShamway\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntLootCrateShamway\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sidingWhiteBurntCTRPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodBarricadePlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodBarricadeCTRPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plainWoodBarricade02Plate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plainWoodBarricade02CTRPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"window02Broken01CTRPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"window02Broken02CTRPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"window02Broken03CTRPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"window03Broke1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"window03Broke2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"window03Broke3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"window03Broke4\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"window03\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"window03Frame\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"glassIndustrial02Broken01CTRPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"glassIndustrial02Broken02CTRPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"miniblindTopSheet\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"miniblindBottomSheet\" />"));
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
                    sw.WriteLine(string.Format("        <item itemName=\"cntSupplyCrateShotgunMessiah\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntLootCrateShotgunMessiah\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"ironDoor1_v2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"ironDoor1_v3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodHatch1_v2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodHatch1_v3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"brickModernBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"torchWallHolder\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntPillCase\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"greenMetalRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"greenMetalBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"orangeMetalRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"orangeMetalBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"greenMetalCNRFull\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"orangeMetalTanBaseboardBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"orangeMetalTanDrywallBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"orangeMetalOrangeDrywallBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"orangeMetalTanIndustrialTileBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cobblestoneBlockHalf\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cobblestoneBlockThreeQuarters\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cobblestoneBlockQuarter\" />"));
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
                    sw.WriteLine(string.Format("        <item itemName=\"orangeDrywallBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"brickModernOrangeDrywallBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntSupplyCrateWorkingStiffs\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntLootCrateWorkingStiffs\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cobblestoneFrameRampQuarter\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cobblestoneFrameRampHalf\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cashRegisterEmpty\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntCashRegister\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntStorageAmmoInsecure\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntStorageBuildingInsecure\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntStorageExplosivesInsecure\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntStorageFoodInsecure\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntStorageWeaponsInsecure\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"shoppingCartEmpty\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntShoppingCart\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodShinglesWedge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodShinglesWedgeTip\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"furRugBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"logCabinRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"greenMetalWedge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"greenMetalWedgeTip\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"greenMetalGable\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopGrocery\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopGroceryLit\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopGroceryWall\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopGroceryWallLit\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopGroceryLarge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopGroceryLargeLit\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopGroceryLargeWall\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"signShopGroceryLargeWallLit\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"snowstorm1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sandstorm\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"smokestorm\" />"));
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
                    sw.WriteLine(string.Format("        <item itemName=\"treeHauntedTreeWasteland42\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treeAzalea\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"emberPile1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"emberPile2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"emberPile3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"emberPile4\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"window03WoodBroke1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"window03WoodBroke2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"window03WoodBroke3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"window03WoodBroke4\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"window03WoodFrame\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"dotsWoodTable\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"chainLink\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelStairs25\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"steelRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntDeskSafeInsecure\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntWallSafeInsecure\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntGunSafeInsecure\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"garageDoorMetal_v3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rScrapIronLogSpike5\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"chainLinkFenceBottomPole\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"chainLinkFenceTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"chainLinkFenceTopPole\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"chainLinkPole\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"chainLinkCornerTop\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"chainLinkCornerBottom\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"chainLinkFenceTopPole2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"spotlightNailedDown\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"vaultDoor02\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"vaultDoor03\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntCar03Blue\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntCar03BlueDamage1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntCar03Damage2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntCar03Red\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntCar03RedDamage1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntCar03Yellow\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntCar03YellowDamage1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntCar03White\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntCar03WhiteDamage1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"randomCars\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapHatch_v2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"scrapHatch_v3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntCoffin\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"vaultHatch_v2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"vaultHatch_v3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"waterSource8\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"shapeTestRamp\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"shapeTestWedge\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"shapeTestWedgeTip\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"shapeTestCap\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntDumpster\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"garbage_decor1\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"garbage_decor2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"garbage_decor3\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"concreteFormCNRInside\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"pouredConcreteCNRInside\" />"));
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
                    sw.WriteLine(string.Format("        <item itemName=\"corrugatedDecayedBrickGable\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corrugatedDecayedBrickQuarterGable\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"fusebox\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"metalPipeStraight\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"metalPipeCorner\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"metalPipeJoint\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"metalPipeCap\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"industrialLight01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"metalPipeValve\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"metalPipeFlange\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"electricalBox01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"asphaltCracked\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"conduit01\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"conduit01Curve\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"conduit01End\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sofa02\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"chair04\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treePlantedWinterPine1m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treePlantedWinterPine6m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"greenRustyMetalWall04Sheet2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treePlantedWinterPine13m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treePlantedWinterPine16m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"treePlantedWinterPine19m\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corrugatedMetalWallSheet\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corrugatedMetalWall01Sheet2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corrugatedMetalWall02Sheet2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corrugatedMetalWall03Sheet2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"corrugatedMetalWall04Sheet2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodPoleSupportCtr\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"forestCurb\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"forestCNRCurb\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodPoleSupport\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"sidewalkCurb\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plainsCurb\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodCrossCtr\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodCross\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"plainsCNRCurb\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"woodPoleSupport2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"elevatorTest\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rustyIronWall01Sheet2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rustyIronWall02Sheet2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rustyIronWall03Sheet2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rustyIronWall04Sheet2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rustyIronPoleSupportCtr\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rustyIronCrossCtr\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"rustyIronPillar100Duplicate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"blueMetalWall01Sheet2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"blueMetalWall02Sheet2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"blueMetalWall03Sheet2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"blueMetalWall04Sheet2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"blueMetalPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"greenMetalWall01Sheet2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"greenMetalWall02Sheet2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"greenMetalWall03Sheet2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"greenMetalWall04Sheet2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"greenMetalPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"orangeMetalWall01Sheet2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"orangeMetalWall02Sheet2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"orangeMetalWall03Sheet2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"orangeMetalWall04Sheet2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"orangeMetalPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"roadRailing\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"roadRailingPole\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"roadRailingEnd\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"roadRailingEnd2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"greenRustyMetalPlate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"greenRustyMetalWall01Sheet2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"greenRustyMetalWall02Sheet2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"greenRustyMetalWall03Sheet2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"greenRustyMetalWallBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"brickCNRRound\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"awningRedCNRRound\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"awningTanCNRRound\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"awningGreenCNRRound\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"greenRustyMetalCNRRound\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"blueMetalCNRRound\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"whiteMetalCNRRound\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"whiteMetalBlockDuplicate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"cntCabinetOldCNRRound\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"greenRustyMetalWallBlockDuplicate\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"redMetalBlock\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"redMetalCNRRound\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"redMetalWall01Sheet2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"redMetalWall02Sheet2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"redMetalWall03Sheet2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"redMetalWall04Sheet2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"whiteMetalWall01Sheet2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"whiteMetalWall02Sheet2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"whiteMetalWall03Sheet2\" />"));
                    sw.WriteLine(string.Format("        <item itemName=\"whiteMetalWall04Sheet2\" />"));
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
                    // last block = 1913
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