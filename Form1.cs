using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace OGPB
{
    public partial class Form1 : Form
    {
        private OGPB.http httpClient = new OGPB.http();
        private Thread processing;
        private object[] planets = new object[0];
        private Planet currentPlanet;
        private GUI gui = new GUI();
        private Translate translate;
        private string currentObject;

        public Form1()
        {
            InitializeComponent();

            translate = new Translate("en");

            gui.translate = translate;
            gui.formMain = this;
            gui.txtUsername = txtUsername;
            gui.txtPassword = txtPassword;
            gui.btnLogin = btnLogin;
            gui.btnBotStatus = btnBotStatus;
            gui.txtLog = txtLog;
            gui.lblStatus = stsPrimaryLabel;
            gui.tabMaster = tabControl1;
            gui.lstToBuild = lstToBuild;
            gui.lstToBuildTrue = lstToBuildTrue;

            httpClient.gui = gui;

            tabControl1.TabPages.Remove(tabInvisiblePlanet);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (httpClient.development == false && httpClient.NeedUpdate() == true)
            {
                Application.Exit();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            gui.EnableDisableBotStatus(false);
            if (processing != null && processing.IsAlive == true)
            {
                processing.Abort();
                while (processing.IsAlive == true)
                {
                    Thread.Sleep(250);
                }
            }
            gui.EnableDisableLogin(false);
            gui.ChangeStatus("Logging in...");
            processing = new Thread(new ThreadStart(this.Login));
            processing.Start();
        }

        public void Login()
        {
            httpClient.Login(txtUsername.Text, txtPassword.Text);
            string content = httpClient.DownloadFile("index.php?page=overview&session=" + httpClient.session + "&lgn=1");
            if (content.IndexOf("<script>document.location.href='http://ogame") == 0 || content == "")
            {
                gui.ChangeStatus("Login failed. Check your username/password!");
                gui.EnableDisableLogin(true);
                gui.FocusUsername();
                return;
            }
            string planetCode = content.Substring(content.IndexOf("<div id=\"countColonies\">"));
            planetCode = planetCode.Substring(0, planetCode.IndexOf("<!-- END RIGHTMENU -->"));
            MatchCollection matches = Regex.Matches(planetCode, "<span class=\"planet-name\">([^>]*)</span>");
            MatchCollection matchedIDs = Regex.Matches(planetCode, "[0-9]{8}");
            planets = (object[])ResizeArray(planets, matches.Count);
            Planet currentPlanet;
            string planetName;
            int planetID;
            for (int i = 0; i < matches.Count; i++)
            {
                planetName = matches[i].Value.Replace("<span class=\"planet-name\">", "").Replace("</span>", "");
                if (i > 0)
                {
                    planetID = Convert.ToInt32(matchedIDs[i - 1].Value);
                }
                else
                {
                    planetID = 0;
                }
                currentPlanet = new Planet(planetName, planetID, httpClient, gui);
                Object[] test = new Object[5];
                planets[i] = currentPlanet;
            }
            gui.ChangeStatus("Logged in");
            ProcessPlanets();
        }

        public void ProcessPlanets()
        {
            foreach (Planet loopPlanet in planets)
            {
                gui.ChangeStatus("Retrieving Resource Buildings for \"" + loopPlanet.planetName + "\"");
                loopPlanet.UpdateResourcePage();
                Thread.Sleep(500);
                loopPlanet.GetEvents();
                Thread.Sleep(5000);
                gui.ChangeStatus("Retrieving Facilities for \"" + loopPlanet.planetName + "\"");
                loopPlanet.UpdateFacilitiesPage();
                Thread.Sleep(500);
                loopPlanet.GetEvents();
                Thread.Sleep(5000);
                gui.ChangeStatus("Retrieving Shipyard for \"" + loopPlanet.planetName + "\"");
                loopPlanet.UpdateShipyard();
                Thread.Sleep(500);
                loopPlanet.GetEvents();
                if (loopPlanet.planetID == 0)
                {
                    gui.ChangeStatus("Retrieving Research for all planets");
                    Thread.Sleep(5000);
                    loopPlanet.UpdateResearch();
                    Thread.Sleep(500);
                    loopPlanet.GetEvents();
                }
                gui.ChangeStatus("\"" + loopPlanet.planetName + "\" has now been updated and loaded!");
                if (tabControl1.TabPages.IndexOfKey("tab" + loopPlanet.planetID) == -1)
                {
                    gui.AddPlanetTab(loopPlanet.planetID, loopPlanet.planetName);
                    if (loopPlanet.planetID == 0)
                    {
                        gui.GoToHomePlanet();
                    }
                }
            }
            gui.ChangeStatus("All planets have been loaded!");
            gui.EnableDisableBotStatus(true);
        }

        public void RunBot()
        {
            string toWorkOn;
            while (true)
            {
                foreach (Planet loopPlanet in planets)
                {
                    gui.ChangeStatus("Running bot on planet " + currentPlanet.planetName);
                    while (loopPlanet.buildList.Length > 0)
                    {
                        // Calculate the resources!


                        toWorkOn = currentPlanet.buildList[0].ToString();
                        if (toWorkOn.StartsWith("Resources") == true)
                        {
                            if (DateTime.Now.CompareTo(loopPlanet.timeTillBuilding) >= 0)
                            {
                                lstToBuildTrue.Items.RemoveAt(0);
                                loopPlanet.UpdateBuildList(lstToBuildTrue.Items);
                                if (currentPlanet.planetID == loopPlanet.planetID)
                                {
                                    currentPlanet = loopPlanet;
                                    gui.UpdateBuildList(currentPlanet);
                                }
                                gui.ChangeStatus("Building " + translate.GetString(toWorkOn));
                                loopPlanet.BuildResource(toWorkOn);
                            }
                        }
                        Thread.Sleep(15000);
                    }
                }
            }
        }

        public static System.Array ResizeArray(System.Array oldArray, int newSize)
        {
            int oldSize = oldArray.Length;
            System.Type elementType = oldArray.GetType().GetElementType();
            System.Array newArray = System.Array.CreateInstance(elementType, newSize);
            int preserveLength = System.Math.Min(oldSize, newSize);
            if (preserveLength > 0)
            {
                System.Array.Copy(oldArray, newArray, preserveLength);
            }
            return newArray;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (processing != null && processing.IsAlive == true)
            {
                processing.Abort();
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex > 1 && tabControl1.SelectedTab.Name != "tabInvisiblePlanet")
            {
                string toLoad = tabControl1.SelectedTab.Text;
                Planet researchPlanet = (Planet)planets[0];
                foreach (Planet checkPlanet in planets)
                {
                    if (checkPlanet.planetName == toLoad)
                    {
                        currentPlanet = checkPlanet;
                        break;
                    }
                }

                lblPlanetMetal.Text = currentPlanet.metal.ToString() + " " + translate.GetString("ResourcesMetal");
                lblPlanetCrystal.Text = currentPlanet.crystal.ToString() + " " + translate.GetString("ResourcesCrystal");
                lblPlanetDeuterium.Text = currentPlanet.deuterium.ToString() + " " + translate.GetString("ResourcesDeuterium");
                lblPlanetEnergy.Text = currentPlanet.energy.ToString() + " " + translate.GetString("ResourcesEnergy");

                lblResourcesMetalMine.Text = translate.GetString("ResourcesMetalMine") + " " + currentPlanet.resourcesMetalMine.ToString();
                lblResourcesCrystalMine.Text = translate.GetString("ResourcesCrystalMine") + " " + currentPlanet.resourcesCrystalMine.ToString();
                lblResourcesDeuteriumSynthesizer.Text = translate.GetString("ResourcesDeuteriumSynthesizer") + " " + currentPlanet.resourcesDeuteriumSynthesizer.ToString();
                lblResourcesSolarPlant.Text = translate.GetString("ResourcesSolarPlant") + " " + currentPlanet.resourcesSolarPlant.ToString();
                lblResourcesFusionReactor.Text = translate.GetString("ResourcesFusionReactor") + " " + currentPlanet.resourcesFusionReactor.ToString();
                lblResourcesMetalStorage.Text = translate.GetString("ResourcesMetalStorage") + " " + currentPlanet.resourcesMetalStorage.ToString();
                lblResourcesCrystalStorage.Text = translate.GetString("ResourcesCrystalStorage") + " " + currentPlanet.resourcesCrystalStorage.ToString();
                lblResourcesDeuteriumTank.Text = translate.GetString("ResourcesDeuteriumTank") + " " + currentPlanet.resourcesDeuteriumStorage.ToString();

                lblFacilitiesRoboticsFactory.Text = translate.GetString("FacilitiesRoboticsFactory") + " " + currentPlanet.facilitiesRoboticsFactory.ToString();
                lblFacilitiesShipyard.Text = translate.GetString("FacilitiesShipyard") + " " + currentPlanet.facilitiesShipyard.ToString();
                lblFacilitiesResearchLab.Text = translate.GetString("FacilitiesResearchLab") + " " + currentPlanet.facilitiesResearchLab.ToString();
                lblFacilitiesAllianceDepot.Text = translate.GetString("FacilitiesAllianceDepot") + " " + currentPlanet.facilitiesAllianceDepot.ToString();
                lblFacilitiesMissileSilo.Text = translate.GetString("FacilitiesMissileSilo") + " " + currentPlanet.facilitiesMissileSilo.ToString();
                lblFacilitiesNaniteFactory.Text = translate.GetString("FacilitiesNaniteFactory") + " " + currentPlanet.facilitiesNaniteFactory.ToString();
                lblFacilitiesTerraformer.Text = translate.GetString("FacilitiesTerraformer") + " " + currentPlanet.facilitiesTerraformer.ToString();

                lblShipyardLightFighter.Text = translate.GetString("ShipyardLightFighter") + " " + currentPlanet.shipyardLightFighter.ToString();
                lblShipyardHeavyFighter.Text = translate.GetString("ShipyardHeavyFighter") + " " + currentPlanet.shipyardHeavyFighter.ToString();
                lblShipyardCruiser.Text = translate.GetString("ShipyardCruiser") + " " + currentPlanet.shipyardCruiser.ToString();
                lblShipyardBattleship.Text = translate.GetString("ShipyardBattleship") + " " + currentPlanet.shipyardBattleship.ToString();
                lblShipyardBattlecruiser.Text = translate.GetString("ShipyardBattlecruiser") + " " + currentPlanet.shipyardBattlecruiser.ToString();
                lblShipyardBomber.Text = translate.GetString("ShipyardBomber") + " " + currentPlanet.shipyardBomber.ToString();
                lblShipyardDestroyer.Text = translate.GetString("ShipyardDestroyer") + " " + currentPlanet.shipyardDestroyer.ToString();
                lblShipyardDeathstar.Text = translate.GetString("ShipyardDeathstar") + " " + currentPlanet.shipyardDeathstar.ToString();
                lblShipyardSmallCargo.Text = translate.GetString("ShipyardSmallCargo") + " " + currentPlanet.shipyardSmallCargo.ToString();
                lblShipyardLargeCargo.Text = translate.GetString("ShipyardLargeCargo") + " " + currentPlanet.shipyardLargeCargo.ToString();
                lblShipyardColonyShip.Text = translate.GetString("ShipyardColonyShip") + " " + currentPlanet.shipyardColonyShip.ToString();
                lblShipyardRecycler.Text = translate.GetString("ShipyardRecycler") + " " + currentPlanet.shipyardRecycler.ToString();
                lblShipyardEspionageProbe.Text = translate.GetString("ShipyardEspionageProbe") + " " + currentPlanet.shipyardEspionageProbe.ToString();
                lblShipyardSolarSatellite.Text = translate.GetString("ShipyardSolarSatellite") + " " + currentPlanet.shipyardSolarSatellite.ToString();

                lblResearchEnergy.Text = translate.GetString("ResearchEnergy") + " " + researchPlanet.researchEnergy.ToString();
                lblResearchLaser.Text = translate.GetString("ResearchLaser") + " " + researchPlanet.researchLaser.ToString();
                lblResearchIon.Text = translate.GetString("ResearchIon") + " " + researchPlanet.researchIon.ToString();
                lblResearchHyperspaceTech.Text = translate.GetString("ResearchHyperspaceTech") + " " + researchPlanet.researchHyperspaceTech.ToString();
                lblResearchPlasma.Text = translate.GetString("ResearchPlasma") + " " + researchPlanet.researchPlasma.ToString();
                lblResearchCombustion.Text = translate.GetString("ResearchCombustion") + " " + researchPlanet.researchCombustion.ToString();
                lblResearchImpulse.Text = translate.GetString("ResearchImpulse") + " " + researchPlanet.researchImpulse.ToString();
                lblResearchHyperspaceDrive.Text = translate.GetString("ResearchHyperspaceDrive") + " " + researchPlanet.researchHyperspaceDrive.ToString();
                lblResearchEspionage.Text = translate.GetString("ResearchEspionage") + " " + researchPlanet.researchEspionage.ToString();
                lblResearchComputer.Text = translate.GetString("ResearchComputer") + " " + researchPlanet.researchComputer.ToString();
                lblResearchAstrophysics.Text = translate.GetString("ResearchAstrophysics") + " " + researchPlanet.researchAstrophysics.ToString();
                lblResearchIRN.Text = translate.GetString("ResearchIRN") + " " + researchPlanet.researchIRN.ToString();
                lblResearchGraviton.Text = translate.GetString("ResearchGraviton") + " " + researchPlanet.researchGraviton.ToString();
                lblResearchArmour.Text = translate.GetString("ResearchArmour") + " " + researchPlanet.researchArmour.ToString();
                lblResearchWeapons.Text = translate.GetString("ResearchWeapons") + " " + researchPlanet.researchWeapons.ToString();
                lblResearchShielding.Text = translate.GetString("ResearchShielding") + " " + researchPlanet.researchShielding.ToString();

                lstToBuildTrue.Items.Clear();
                lstToBuildTrue.Items.AddRange(currentPlanet.buildList);
                gui.UpdateBuildList(currentPlanet);

                pnlPlanetInfo.Parent = tabControl1.SelectedTab;
            }
        }

        private void txtUsername_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                gui.FocusPassword();
            }
        }

        private void txtPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                button1_Click(sender, e);
            }
        }

        private void lblPlanetResourceItem_MouseHover(object sender, EventArgs e)
        {
            if (((Label)sender).Name == "lblPlanetMetal")
            {
                ((Label)sender).Text = currentPlanet.hourlyMetal.ToString() + " " + translate.GetString("ResourcesPerHour");
            }
            else if (((Label)sender).Name == "lblPlanetCrystal")
            {
                ((Label)sender).Text = currentPlanet.hourlyCrystal.ToString() + " " + translate.GetString("ResourcesPerHour");
            }
            else if (((Label)sender).Name == "lblPlanetDeuterium")
            {
                ((Label)sender).Text = currentPlanet.hourlyDeuterium.ToString() + " " + translate.GetString("ResourcesPerHour");
            }
        }

        private void lblPlanetResourceItem_MouseLeave(object sender, EventArgs e)
        {
            if (((Label)sender).Name == "lblPlanetMetal")
            {
                ((Label)sender).Text = currentPlanet.metal.ToString() + " " + translate.GetString("ResourcesMetal");
            }
            else if (((Label)sender).Name == "lblPlanetCrystal")
            {
                ((Label)sender).Text = currentPlanet.crystal.ToString() + " " + translate.GetString("ResourcesCrystal");
            }
            else if (((Label)sender).Name == "lblPlanetDeuterium")
            {
                ((Label)sender).Text = currentPlanet.deuterium.ToString() + " " + translate.GetString("ResourcesDeuterium");
            }
        }

        private void lblPlanetConstruction_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            currentObject = ((LinkLabel)sender).Name.Replace("lbl", "");

            grpConstructionInfo.Text = translate.GetString(currentObject);
            lblConstructionInfo.Text = translate.GetString(currentObject + "Description");
            if (tabControl2.SelectedTab.Name == "tabShipyard" || tabControl2.SelectedTab.Name == "tabDefense")
            {
                lblConstructionNumber.Visible = true;
                nudConstructionNumber.Visible = true;
                nudConstructionNumber.Value = 0;
                lblConstructionLevel.Text = "Currently have " + ((LinkLabel)sender).Text.Replace(translate.GetString(currentObject), "");
            }
            else
            {
                lblConstructionNumber.Visible = false;
                nudConstructionNumber.Visible = false;
                lblConstructionLevel.Text = "Currently Level " + ((LinkLabel)sender).Text.Replace(translate.GetString(currentObject), "");
            }
            btnAddToQueue.Visible = true;
            button2.Visible = true;
        }

        private void btnAddToQueue_Click(object sender, EventArgs e)
        {
            if (tabControl2.SelectedTab.Name != "tabResources")
            {
                MessageBox.Show("The bot only works with the resources tab right now!");
            }
            else
            {
                if (tabControl2.SelectedTab.Name == "tabShipyard" || tabControl2.SelectedTab.Name == "tabDefense")
                {
                    if (nudConstructionNumber.Value > 0)
                    {
                        lstToBuildTrue.Items.Add(currentObject + "#" + nudConstructionNumber.Value.ToString());
                    }
                }
                else
                {
                    lstToBuildTrue.Items.Add(currentObject);
                }
                currentPlanet.UpdateBuildList(lstToBuildTrue.Items);
                gui.UpdateBuildList(currentPlanet);
            }
        }

        private void tabControl2_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblConstructionNumber.Visible = false;
            nudConstructionNumber.Visible = false;
            btnAddToQueue.Visible = false;
            button2.Visible = false;
            grpConstructionInfo.Text = "THIS INFO MUST CHANGE!";
            lblConstructionInfo.Text = "THIS INFO MUST CHANGE!";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int selectedIndex = lstToBuild.SelectedIndex;
            if (selectedIndex >= 0)
            {
                lstToBuildTrue.Items.RemoveAt(selectedIndex);
                currentPlanet.UpdateBuildList(lstToBuildTrue.Items);
                gui.UpdateBuildList(currentPlanet);
                if (selectedIndex > (lstToBuild.Items.Count - 1))
                {
                    lstToBuild.SelectedIndex = (lstToBuildTrue.Items.Count - 1);
                }
                else
                {
                    lstToBuild.SelectedIndex = selectedIndex;
                }
            }
        }

        private void btnBotStatus_Click(object sender, EventArgs e)
        {
            if (processing != null && processing.IsAlive == true)
            {
                gui.ChangeStatus("Stopping Bot");
                processing.Abort();
                while (processing.IsAlive == true)
                {
                    Thread.Sleep(250);
                }
                gui.EnableDisableLogin(true);
                gui.ChangeStatus("Bot Stopped");
                btnBotStatus.Text = "Start Bot";
            }
            else
            {
                gui.EnableDisableLogin(false);
                gui.ChangeStatus("Running Bot");
                processing = new Thread(new ThreadStart(this.RunBot));
                processing.Start();
                btnBotStatus.Text = "Stop Bot";
            }
        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            if (comboBox1.Text.IndexOf("ogame") > -1)
            {
                httpClient.server = "http://" + comboBox1.Text + "/game/";
            }
        }
    }
}