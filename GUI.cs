using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace OGPB
{
    public class GUI
    {
        private delegate void callback();
        private delegate void callbackSingle(Object param1);
        private delegate void callbackDouble(Object param1, Object param2);

        public Form formMain;
        public TextBox txtUsername;
        public TextBox txtPassword;
        public Button btnLogin;
        public Button btnBotStatus;
        public TextBox txtLog;
        public ToolStripStatusLabel lblStatus;
        public TabControl tabMaster;
        public ListBox lstToBuild;
        public ListBox lstToBuildTrue;

        public Translate translate;

        public void FocusUsername()
        {
            if (formMain.InvokeRequired)
            {
                callback inv = new callback(this.FocusUsername);
                formMain.Invoke(inv, new object[] { });
            }
            else
            {
                txtUsername.Focus();
            }
        }

        public void FocusPassword()
        {
            if (formMain.InvokeRequired)
            {
                callback inv = new callback(this.FocusPassword);
                formMain.Invoke(inv, new object[] { });
            }
            else
            {
                txtPassword.Focus();
            }
        }

        public void EnableDisableLogin(Object enabled)
        {
            if (formMain.InvokeRequired)
            {
                callbackSingle inv = new callbackSingle(this.EnableDisableLogin);
                formMain.Invoke(inv, new object[] { enabled });
            }
            else
            {
                txtUsername.Enabled = (bool)enabled;
                txtPassword.Enabled = (bool)enabled;
                btnLogin.Enabled = (bool)enabled;
            }
        }

        public void EnableDisableBotStatus(Object enabled)
        {
            if (formMain.InvokeRequired)
            {
                callbackSingle inv = new callbackSingle(this.EnableDisableBotStatus);
                formMain.Invoke(inv, new object[] { enabled });
            }
            else
            {
                btnBotStatus.Enabled = (bool)enabled;
            }
        }

        public void ChangeStatus(Object newStatus)
        {
            if (formMain.InvokeRequired)
            {
                callbackSingle inv = new callbackSingle(this.ChangeStatus);
                formMain.Invoke(inv, new object[] { newStatus });
            }
            else
            {
                lblStatus.Text = (string)newStatus;
                AddToLog("Status Changed: " + newStatus);
            }
        }

        public void AddToLog(Object toAdd)
        {
            if (formMain.InvokeRequired)
            {
                callbackSingle inv = new callbackSingle(this.AddToLog);
                formMain.Invoke(inv, new object[] { toAdd });
            }
            else
            {
                txtLog.Text += (string)toAdd + "\r\n";
            }
        }

        public void AddPlanetTab(object planetID, object planetName)
        {
            if (formMain.InvokeRequired)
            {
                callbackDouble inv = new callbackDouble(this.AddPlanetTab);
                formMain.Invoke(inv, new object[] { planetID, planetName });
            }
            else
            {
                tabMaster.TabPages.Add("tab" + planetID.ToString(), (string)planetName);
            }
        }

        public void GoToHomePlanet()
        {
            if (formMain.InvokeRequired)
            {
                callback inv = new callback(this.GoToHomePlanet);
                formMain.Invoke(inv, new object[] {  });
            }
            else
            {
                tabMaster.SelectedIndex = 2;
            }
        }

        public void UpdateBuildList(object basePlanetInfo)
        {
            Planet currentPlanet = (Planet)((Planet)basePlanetInfo).Clone();
            if (formMain.InvokeRequired)
            {
                callbackSingle inv = new callbackSingle(this.UpdateBuildList);
                formMain.Invoke(inv, new object[] { basePlanetInfo });
            }
            else
            {
                string toTranslate;
                Type type = Type.GetType("OGPB.Planet");
                FieldInfo field;
                lstToBuild.Items.Clear();
                for (int i = 0; i < currentPlanet.buildList.Length; i++)
                {
                    toTranslate = currentPlanet.buildList[i].ToString();
                    if (toTranslate.Contains("#") == true)
                    {
                        lstToBuild.Items.Insert(i, toTranslate.Substring(toTranslate.IndexOf("#") + 1) + " " + translate.GetString(toTranslate.Substring(0, toTranslate.IndexOf("#"))));
                    }
                    else
                    {
                        field = type.GetField(toTranslate.Substring(0, 1).ToLower() + toTranslate.Substring(1));
                        field.SetValue(currentPlanet, (Convert.ToInt32(field.GetValue(currentPlanet)) + 1));
                        lstToBuild.Items.Insert(i, translate.GetString(toTranslate) + " " + field.GetValue(currentPlanet));
                    }
                }
            }
        }
    }
}
