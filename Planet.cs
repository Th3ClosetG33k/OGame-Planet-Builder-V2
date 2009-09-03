using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace OGPB
{
    class Planet : ICloneable
    {
        public string planetName;
        public int planetID;
        private http httpClient;
        private GUI gui;

        #region Planet Information and Levels
        public int metal, crystal, deuterium, energy;
        public int hourlyMetal, hourlyCrystal, hourlyDeuterium;

        public int resourcesMetalMine, resourcesCrystalMine, resourcesDeuteriumSynthesizer, resourcesSolarPlant, resourcesFusionReactor,
            resourcesMetalStorage, resourcesCrystalStorage, resourcesDeuteriumStorage;

        public int facilitiesRoboticsFactory, facilitiesShipyard, facilitiesResearchLab, facilitiesAllianceDepot, facilitiesMissileSilo,
            facilitiesNaniteFactory, facilitiesTerraformer;

        public int shipyardLightFighter, shipyardHeavyFighter, shipyardCruiser, shipyardBattleship, shipyardBattlecruiser, shipyardBomber,
            shipyardDestroyer, shipyardDeathstar, shipyardSmallCargo, shipyardLargeCargo, shipyardColonyShip, shipyardRecycler,
            shipyardEspionageProbe, shipyardSolarSatellite;

        public int researchEnergy, researchLaser, researchIon, researchHyperspaceTech, researchPlasma, researchCombustion, researchImpulse,
            researchHyperspaceDrive, researchEspionage, researchComputer, researchAstrophysics, researchIRN, researchGraviton,
            researchArmour, researchWeapons, researchShielding;
        #endregion

        #region Currently Working On:
        public string currentBuilding, currentResearch, currentShip, currentDefense;
        public DateTime timeTillBuilding, timeTillResearch, timeTillShipyard, timeTillNext;
        #endregion

        #region Construction Information
        public double[] infoResourcesMetalMine = { 1, 60, 15, 0, 0, 1.5 }, infoResourcesCrystalMine = { 2, 48, 24, 0, 0, 1.6 },
            infoResourcesDeuteriumSynthesizer = { 3, 225, 75, 0, 0, 1.5 }, infoResourcesSolarPlant = { 4, 75, 30, 0, 0, 1.5 },
            infoResourcesFusionReactor = { 12, 200, 400, 200, 0, 2 }, infoResourcesMetalStorage = { 22, 2000, 0, 0, 0, 2 },
            infoResourcesCrystalStorage = { 23, 2000, 1000, 0, 0, 2 }, infoResourcesDeuteriumTank = { 24, 2000, 2000, 0, 0, 2 },

            infoFacilitiesRoboticsFactory = { 14, 400, 120, 200, 0 }, infoFacilitiesShipyard = { 21, 400, 200, 100, 0 },
            infoFacilitiesResearchLab = { 31, 200, 400, 200, 0 }, infoFacilitiesAllianceDepot = { 34, 20000, 40000, 0, 0 },
            infoFacilitiesMissleSilo = { 44, 20000, 20000, 1000, 0 }, infoFacilitiesNanitefactory = { 15, 1000000, 500000, 100000, 0 },
            infoFacilitiesTerraformer = { 33, 0, 50000, 100000, 1000 },

            infoResearchEspionage = { 106, 200, 1000, 200, 0, 2 }, infoResearchComputer = { 108, 0, 400, 600, 0, 2 },
            infoResearchWeapon = { 109, 800, 200, 0, 0, 2 }, infoResearchShield = { 110, 200, 600, 0, 0, 2 },
            infoResearchArmour = { 111, 1000, 0, 0, 0, 2 }, infoResearchEnergy = { 113, 0, 800, 400, 0, 2 },
            infoResearchHyperspaceTech = { 114, 0, 4000, 2000, 0, 2 }, infoResearchCombustion = { 115, 400, 0, 600, 0, 2 },
            infoResearchImpulse = { 117, 2000, 4000, 600, 0, 2 }, infoResearchHyperspaceDrive = { 118, 10000, 20000, 6000, 0, 2 },
            infoResearchLaser = { 120, 200, 100, 0, 0, 2 }, infoResearchIon = { 121, 1000, 300, 100, 0, 2 },
            infoResearchPlasma = { 112, 2000, 4000, 1000, 0, 2 }, infoResearchIRN = { 123, 240000, 400000, 160000, 0, 2 },
            infoResearchAstrophysics = { 124, 4000, 8000, 4000, 0, 1.75 }, infoResearchGraviton = { 199, 0, 0, 0, 0, 2 },
            
            infoShipyardSmallCargo = { 202, 2000, 2000, 0, 0 }, infoShipyardLargeCargo = { 203, 6000, 6000, 0, 0 },
            infoShipyardLightFighter =  {204, 3000, 1000, 0, 0 }, infoShipyardHeavyFighter = { 205, 6000, 4000, 0, 0 },
            infoShipyardCruiser = { 206, 20000, 7000, 2000, 0 }, infoShipyardBattleship = { 207, 45000, 15000, 0, 0 },
            infoShipyardColonyShip = { 208, 10000, 20000, 10000, 0 }, infoShipyardRecyler = { 209, 10000, 6000, 2000, 0 },
            infoShipyardEspionageProbe =  {210, 0, 1000, 0, 0 }, infoShipyardBomber = { 211, 50000, 25000, 15000, 0 },
            infoShipyardSolarSatellite = { 212, 0, 2000, 5000, 0 }, infoShipyardDestroyer = { 213, 60000, 50000, 15000, 0 },
            infoShipyardDeathstar = { 214, 5000000, 4000000, 1000000, 0 }, infoShipyardBattleCruiser = { 215, 30000, 40000, 15000, 0 },

            infoDefenseRocketLauncher = { 401, 2000, 0, 0 }, infoDefenseLightLaser = { 402, 1500, 500, 0 },
            infoDefenseHeavyLaser = { 403, 6000, 2000, 0 }, infoDefenseGaussCannon = { 404, 20000, 15000, 2000 },
            infoDefenseIonCannon = { 405, 2000, 6000, 0 }, infoDefensePlasmaTurret = { 406, 50000, 50000, 30000 },
            infoDefenseSmallShieldDome = { 407, 10000, 10000, 0 }, infoDefenseLargeShieldDome = { 408, 50000, 50000, 0 },
            infoDefenseAntiBallisticMissles = { 502, 8000, 0, 2000 }, infoDefenseInterplanetaryMissles = { 503, 12500, 2500, 10000 };
        #endregion

        public Object[] buildList = new Object[0];

        public Planet(string name, int id, http http, GUI guiVar)
        {
            planetName = name;
            planetID = id;
            httpClient = http;
            gui = guiVar;
        }

        public object Clone() // ICloneable implementation
        {
            Planet clone = this.MemberwiseClone() as Planet;
            return clone;
        }

        private string LoadPage(string page)
        {
            return LoadPage(page, "", true);
        }

        private string LoadPage(string page, bool resources)
        {
            return LoadPage(page, "", resources);
        }

        private string LoadPage(string page, string post)
        {
            return LoadPage(page, post, true);
        }

        private string LoadPage(string page, string post, bool resources)
        {
            string changePlanet = "";
            string content = "";
            if (httpClient.currentPlanet != planetID)
            {
                changePlanet = "&cp=" + planetID.ToString();
            }
            if (post != "")
            {
                content = httpClient.PostFile("index.php?page=" + page + "&session=" + httpClient.session + changePlanet, post);
            }
            else
            {
                content = httpClient.DownloadFile("index.php?page=" + page + "&session=" + httpClient.session + changePlanet);
            }
            if (content.IndexOf("<input type='hidden' name='token' value='") > 0)
            {
                httpClient.token = content.Substring(content.IndexOf("<input type='hidden' name='token' value='") + "<input type='hidden' name='token' value='".Length, 32);
            }
            if (resources == true)
            {
                UpdateResourceCount(content);
            }
            return content;
        }

        public void GetEvents()
        {
            string events = LoadPage("fetchEventbox&ajax=1", false);
        }

        public void UpdateResourcePage()
        {
            string content = LoadPage("resources");
            content = content.Substring(content.IndexOf("<!-- CONTENT AREA -->"));
            content = content.Substring(0, content.IndexOf("<div class=\"content-box-s\">"));
            int[] levels = new int[9];
            for (int i = 0; i < 9; i++)
            {
                content = content.Substring(content.IndexOf("<span class=\"level\">") + "<span class=\"level\">".Length).TrimStart();
                if (content.IndexOf("<span class=\"textlabel\">") == 0)
                {
                    content = content.Substring(content.IndexOf("</span>") + "</span>".Length);
                }
                levels[i] = Convert.ToInt32(content.Substring(0, content.IndexOf("</span>")).Trim());
            }
            resourcesMetalMine = levels[0];
            resourcesCrystalMine = levels[1];
            resourcesDeuteriumSynthesizer = levels[2];
            resourcesSolarPlant = levels[3];
            resourcesFusionReactor = levels[4];
            resourcesMetalStorage = levels[6];
            resourcesCrystalStorage = levels[7];
            resourcesDeuteriumStorage = levels[8];
        }

        public void UpdateFacilitiesPage()
        {
            string content = LoadPage("station");
            content = content.Substring(content.IndexOf("<!-- CONTENT AREA -->"));
            content = content.Substring(0, content.IndexOf("<div class=\"content-box-s\">"));
            int[] levels = new int[7];
            for (int i = 0; i < 7; i++)
            {
                content = content.Substring(content.IndexOf("<span class=\"level\">") + "<span class=\"level\">".Length).TrimStart();
                if (content.IndexOf("<span class=\"textlabel\">") == 0)
                {
                    content = content.Substring(content.IndexOf("</span>") + "</span>".Length);
                }
                levels[i] = Convert.ToInt32(content.Substring(0, content.IndexOf("</span>")).Trim());
            }
            facilitiesRoboticsFactory = levels[0];
            facilitiesShipyard = levels[1];
            facilitiesResearchLab = levels[2];
            facilitiesAllianceDepot = levels[3];
            facilitiesMissileSilo = levels[4];
            facilitiesNaniteFactory = levels[5];
            facilitiesTerraformer = levels[6];
        }

        public void UpdateShipyard()
        {
            string content = LoadPage("shipyard");
            content = content.Substring(content.IndexOf("<!-- CONTENT AREA -->"));
            int[] levels = new int[14];
            for (int i = 0; i < 14; i++)
            {
                content = content.Substring(content.IndexOf("<span class=\"level\">") + "<span class=\"level\">".Length).TrimStart();
                if (content.IndexOf("<span class=\"textlabel\">") == 0)
                {
                    content = content.Substring(content.IndexOf("</span>") + "</span>".Length);
                }
                levels[i] = Convert.ToInt32(content.Substring(0, content.IndexOf("</span>")).Trim());
            }
            shipyardLightFighter = levels[0];
            shipyardHeavyFighter = levels[1];
            shipyardCruiser = levels[2];
            shipyardBattleship = levels[3];
            shipyardBattlecruiser = levels[4];
            shipyardBomber = levels[5];
            shipyardDestroyer = levels[6];
            shipyardDeathstar = levels[7];
            shipyardSmallCargo = levels[8];
            shipyardLargeCargo = levels[9];
            shipyardColonyShip = levels[10];
            shipyardRecycler = levels[11];
            shipyardEspionageProbe = levels[12];
            shipyardSolarSatellite = levels[13];
        }

        public void UpdateResearch()
        {
            string content = LoadPage("research");
            content = content.Substring(content.IndexOf("<!-- CONTENT AREA -->"));
            int[] levels = new int[16];
            for (int i = 0; i < 16; i++)
            {
                content = content.Substring(content.IndexOf("<span class=\"level\">") + "<span class=\"level\">".Length).TrimStart();
                if (content.IndexOf("<span class=\"textlabel\">") == 0)
                {
                    content = content.Substring(content.IndexOf("</span>") + "</span>".Length);
                }
                levels[i] = Convert.ToInt32(content.Substring(0, content.IndexOf("</span>")).Trim());
            }
            researchEnergy = levels[0];
            researchLaser = levels[1];
            researchIon = levels[2];
            researchHyperspaceTech = levels[3];
            researchPlasma = levels[4];
            researchCombustion = levels[5];
            researchImpulse = levels[6];
            researchHyperspaceDrive = levels[7];
            researchEspionage = levels[8];
            researchComputer = levels[9];
            researchAstrophysics = levels[10];
            researchIRN = levels[11];
            researchGraviton = levels[12];
            researchArmour = levels[13];
            researchWeapons = levels[14];
            researchShielding = levels[15];
        }

        private void UpdateResourceCount(string content)
        {
            content = content.Substring(content.IndexOf("<ul id=\"resources\">"));
            content = content.Substring(0, content.IndexOf("<div id=\"officers\">")).Replace("overmark", "").Replace("middlemark", "").Replace(".", "");
            content = content.Substring(content.IndexOf("<span class='undermark'>(") + "<span class='undermark'>(".Length);
            hourlyMetal = Convert.ToInt32(content.Substring(0, content.IndexOf(")</span>")).Trim());
            content = content.Substring(content.IndexOf("<span id=\"resources_metal\" class=\"\">") + "<span id=\"resources_metal\" class=\"\">".Length).TrimStart();
            metal = Convert.ToInt32(content.Substring(0, content.IndexOf("</span>")));
            content = content.Substring(content.IndexOf("<span class='undermark'>(") + "<span class='undermark'>(".Length);
            hourlyCrystal = Convert.ToInt32(content.Substring(0, content.IndexOf(")</span>")).Trim());
            content = content.Substring(content.IndexOf("<span id=\"resources_crystal\" class=\"\">") + "<span id=\"resources_crystal\" class=\"\">".Length).TrimStart();
            crystal = Convert.ToInt32(content.Substring(0, content.IndexOf("</span>")));
            if (content.IndexOf("<span class='undermark'>(") > 0)
            {
                content = content.Substring(content.IndexOf("<span class='undermark'>(") + "<span class='undermark'>(".Length);
                hourlyDeuterium = Convert.ToInt32(content.Substring(0, content.IndexOf(")</span>")).Trim());
            }
            else
            {
                hourlyDeuterium = 0;
            }
            content = content.Substring(content.IndexOf("<span id=\"resources_deuterium\" class=\"\">") + "<span id=\"resources_deuterium\" class=\"\">".Length).TrimStart();
            deuterium = Convert.ToInt32(content.Substring(0, content.IndexOf("</span>")));
            content = content.Substring(content.IndexOf("<span id=\"resources_energy\" class=\"\">") + "<span id=\"resources_energy\" class=\"\">".Length).TrimStart();
            energy = Convert.ToInt32(content.Substring(0, content.IndexOf("</span>")));
        }

        private void UpdateCurrentlyWorkingOn(string content)
        {
            bool foundBuildings = content.Contains("new baulisteCountdown(getElementByIdWithCache(\"Countdown\")"),
                foundResearch = content.Contains("new baulisteCountdown(getElementByIdWithCache('researchCountdown')"), foundShipyard = false;
            int tempTimeTillBuilding = 0, tempTimeTillResearch = 0, tempTimeTillShipyard = 0;
            // Buildings
            if (foundBuildings == true)
            {
                content = content.Substring(content.IndexOf("new baulisteCountdown(getElementByIdWithCache(\"Countdown\")"));
                content = content.Substring(content.IndexOf(",") + 1);
                tempTimeTillBuilding = Convert.ToInt32(content.Substring(0, content.IndexOf(",")).Trim());
            }
            else
            {
                content = content.Substring(content.IndexOf("new bauCountdown("));
                if (foundResearch == true || (content.IndexOf("getElementByIdWithCache('b_supply") < 50 && content.IndexOf("getElementByIdWithCache('b_supply") > -1))
                {
                    content = content.Substring(content.IndexOf(",") + 1);
                    tempTimeTillBuilding = Convert.ToInt32(content.Substring(0, content.IndexOf(",")).Trim());
                    foundBuildings = true;
                }
            }
            // Research
            if (foundResearch == true)
            {
                content = content.Substring(content.IndexOf("new baulisteCountdown(getElementByIdWithCache('researchCountdown')"));
                content = content.Substring(content.IndexOf(",") + 1);
                tempTimeTillResearch = Convert.ToInt32(content.Substring(0, content.IndexOf(",")).Trim());
            }
            else if (content.Contains("new bauCountdown(") == true)
            {
                content = content.Substring(content.IndexOf("new bauCountdown("));
                if (content.IndexOf("getElementByIdWithCache('b_research") < 50 && content.IndexOf("getElementByIdWithCache('b_research") > -1)
                {
                    content = content.Substring(content.IndexOf(",") + 1);
                    tempTimeTillResearch = Convert.ToInt32(content.Substring(0, content.IndexOf(",")).Trim());
                    foundResearch = true;
                }
            }
            
            // Shipyard
            if (content.Contains("new shipCountdown(") == true)
            {
                content = content.Substring(content.IndexOf("new shipCountdown("));
                content = content.Substring(content.IndexOf("getElementByIdWithCache('shipSumCount'),") + "getElementByIdWithCache('shipSumCount'),".Length);
                content = content.Substring(content.IndexOf(",") + 1);
                content = content.Substring(content.IndexOf(",") + 1);
                tempTimeTillShipyard = Convert.ToInt32(content.Substring(0, content.IndexOf(",")).Trim());
                foundShipyard = true;
            }
            else if (content.Contains("new schiffbauCountdown(") == true)
            {
                content = content.Substring(content.IndexOf("getElementByIdWithCache('research"));
                string[] tempContent = content.Substring(0, content.IndexOf(");")).Split(',');
                int toMake = Convert.ToInt32(tempContent[1].Trim());
                int leftOnCurrent = Convert.ToInt32(tempContent[3].Trim());
                int timeForEach = Convert.ToInt32(tempContent[4].Trim());
                tempTimeTillResearch = (toMake - 1) * timeForEach + leftOnCurrent;
                foundShipyard = true;
            }

            if (foundBuildings == true)
            {
                timeTillBuilding = DateTime.Now.AddSeconds(tempTimeTillBuilding);
            }
            if (foundResearch == true)
            {
                timeTillResearch = DateTime.Now.AddSeconds(tempTimeTillResearch);
            }
            if (foundShipyard == true)
            {
                timeTillShipyard = DateTime.Now.AddSeconds(tempTimeTillShipyard);
            }
        }

        public void UpdateBuildList(ListBox.ObjectCollection newBuildList)
        {
            Object[] tempBuildList = new Object[newBuildList.Count];
            for (int i = 0; i < newBuildList.Count; i++)
            {
                tempBuildList[i] = newBuildList[i];
            }
            buildList = tempBuildList;
        }

        private double[] GetTechID(string techName)
        {
            Type type = Type.GetType("OGPB.Planet");
            FieldInfo field = type.GetField("info" + techName);
            return (double[])field.GetValue(this);
        }

        public void BuildResource(string buildingName)
        {
            UpdateResourcePage();
            Thread.Sleep(500);
            GetEvents();
            Thread.Sleep(5000);
            double[] techID = GetTechID(buildingName);
            string content = LoadPage("resources", "modus=1&type=" + techID[0].ToString() + "&menge=1&token=" + httpClient.token);
            UpdateCurrentlyWorkingOn(content);
            Thread.Sleep(500);
            GetEvents();
        }
    }
}
