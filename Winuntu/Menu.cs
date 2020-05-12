using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Resources;
using System.Security.Principal;
using System.Management;
using System.Windows.Input;

namespace Winuntu

{
    //=====================================================================
    public partial class Menu : Form
    {
        #region Déclarations
        //-----------------------------------------------
        // Infos
        public string VERSION = "6.1.0.0";
        public string COPYRIGHT = "Copyright © Bustos Thibault (TheRake66) - 2020";
        public string ARCH = "64-bit";

        // Dossiers
        public string FOLDER_INSTALL = Application.UserAppDataPath;
        public string FOLDER_PROFILS = Application.UserAppDataPath + @"\Profils";
        // Fichiers
        string FILE_CONFIG = Application.UserAppDataPath + @"\config.ini";

        // Variables
        Profil CurrentProfil;
        Instance CurrentInstance;
        List<Profil> ListeProfils = new List<Profil>();
        Ini IniConfig = new Ini();

        // Imports
        [DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        private static extern IntPtr SetLayeredWindowAttributes(IntPtr hWndChild, int crKey, int bAlpha, int dwFlags);

        [DllImport("user32.dll")]
        private static extern long GetWindowLongA(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern long SetWindowLongA(IntPtr hWnd, int nIndex, long dwNewLong);

        [DllImport("User32.Dll")]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern void RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, uint flags);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int GetWindowTextA(IntPtr hWnd, IntPtr lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        //-----------------------------------------------
        #endregion Déclarations



        //=====================================================================
        public Menu()
        {
            //-----------------------------------------------
            InitializeComponent();
            //-----------------------------------------------
        }
        //=====================================================================



        //=====================================================================
        // Initalisation
        private void Menu_Load(object sender, EventArgs e)
        {
            //-----------------------------------------------
            // Verifi que ça soit windows 10
            try
            {
                var reg = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                string productName = (string)reg.GetValue("ProductName");
                if (!productName.Contains("Windows 10"))
                {
                    Message_FataleError("Compatible uniquement avec Windows 10 et plus.");
                }
            }
            catch
            {
                Message_FataleError("Impossible de vérifier la version de Windows.");
            }
            //-----------------------------------------------


            //-----------------------------------------------
            // Force la nouvelle console
            try
            {
                var cle = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Console", true);
                cle.SetValue("ForceV2", 1);
                cle.Close();
            }
            catch
            {
                Message_FataleError("Impossible d'accéder au registre pour activer la nouvelle console.");
            }
            //-----------------------------------------------


            //-----------------------------------------------
            // Verifi si admin
            if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
            {
                this.Text += " (Administrateur)";
            }
            //-----------------------------------------------


            //-----------------------------------------------
            if (!Directory.Exists(FOLDER_INSTALL))
            {
                Directory.CreateDirectory(FOLDER_INSTALL);
            }
            if (!Directory.Exists(FOLDER_PROFILS))
            {
                Directory.CreateDirectory(FOLDER_PROFILS);
            }
            //-----------------------------------------------


            //-----------------------------------------------
            IniConfig.SetFile(FILE_CONFIG);

            ChargerProfils();
            ChargerRaccourci();

            //Les shells ne peuvent pas depasser la taille de lecran principal
            this.MaximumSize = new Size(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height);

            // Demarre deux fenetre par defaut
            AjouterInstancePS();
            AjouterInstanceCmd();
            TreeListeOnglets.ExpandAll();
            //-----------------------------------------------
        }
        private void Menu_FormClosing(object sender, FormClosingEventArgs e)
        {
            //-----------------------------------------------
            if (e.CloseReason == CloseReason.UserClosing)
            {
                if (!Message_Continue("Êtes-vous sûr de vouloir quitter ?"))
                {
                    e.Cancel = true;
                    return;
                }
            }

            FermerWinuntu();
            //-----------------------------------------------
        }
        //=====================================================================



        //=====================================================================
        private void ChargerProfils()
        {
            //-----------------------------------------------
            // ajouter le profil par defaut
            AjouterProfil(
                "Défaut",
                "",
                "",
                255,
                "",
                "",
                "",
                ""
                );


            // Liste les profil
            string[] listeprofil = Directory.GetFiles(FOLDER_PROFILS, "*.ini");
            foreach (string fileprofil in listeprofil)
            {
                Ini fileprofilini = new Ini();
                fileprofilini.SetFile(fileprofil);

                string opacity = fileprofilini.ReadKey("Settings", "Opacity");
                int opacityint = 255;
                try
                {
                    opacityint = Convert.ToInt32(opacity);
                }
                catch { }
                AjouterProfil(
                    fileprofilini.ReadKey("Settings", "Name"),
                    fileprofilini.ReadKey("Settings", "Wallpaper"),
                    fileprofilini.ReadKey("Settings", "Description"),
                    opacityint,
                    fileprofilini.ReadKey("Settings", "CommandCmd"),
                    fileprofilini.ReadKey("Settings", "CommandPS"),
                    fileprofilini.ReadKey("Settings", "Directory"),
                    fileprofil
                    );
                fileprofilini = null;
            }

            // Defini l'ancien profil selectionner
            string lastprofil = IniConfig.ReadKey("Options", "LastProfilSelected");
            foreach (Profil profil in ListeProfils)
            {
                if (profil.FichierIni == lastprofil)
                {
                    DefinirProfil(profil);
                }
            }

            if (CurrentProfil == null)
            {
                // Defini le profil par defaut
                DefinirProfil(ListeProfils[0]);
            }
            //-----------------------------------------------
        }
        public void AjouterProfil(
            string pName,
            string pWallpaper,
            string pDescription,
            int pOpacity,
            string pCommandCmd,
            string pCommandPS,
            string pDirectory,
            string pFichierIni)
        {
            //-----------------------------------------------
            Profil profil = new Profil();
            TreeNode node = new TreeNode();
            ToolStripItem toolitem = new ToolStripMenuItem();

            toolitem.Text = pName + " (0)";
            toolitem.Click += new EventHandler((s, e) =>
            {
                DefinirProfil(profil);
            });
            StripProfils.DropDownItems.Add(toolitem);

            node.Text = pName + " (0)";
            TreeListeOnglets.Nodes.Add(node);

            profil.Name = pName;
            profil.Wallpaper = pWallpaper;
            profil.Opacity = pOpacity;
            profil.CommandCmd = pCommandCmd;
            profil.CommandPS = pCommandPS;
            profil.Directory = pDirectory;
            profil.Description = pDescription;
            profil.FichierIni = pFichierIni;
            profil.ParentNode = node;
            profil.ToolItem = toolitem;
            ListeProfils.Add(profil);

            DefinirProfil(profil);
            //-----------------------------------------------
        }
        public void DefinirProfil(Profil pProfil)
        {
            //-----------------------------------------------
            IniConfig.WriteKey("Options", "LastProfilSelected", pProfil.FichierIni);
            CurrentProfil = pProfil;
            //-----------------------------------------------
        }
        public void SupprimerProfil(Profil pProfil)
        {
            //-----------------------------------------------
            if (!Message_Continue("Êtes-vous sûr de vouloir supprimer le profil ?"))
            {
                return;
            }

            FermerToutesLesInstancesDUnProfil(pProfil);

            ListeProfils.Remove(pProfil);
            if (CurrentProfil == pProfil)
            {
                // Chercher un nouveau profil
                CurrentProfil = null;
                foreach (Profil profil in ListeProfils)
                {
                    CurrentProfil = profil;
                }
                DefinirProfil(CurrentProfil);
            }

            pProfil.ParentNode.Remove();
            try
            {
                File.Delete(pProfil.FichierIni);
            }
            catch { }
            pProfil = null;
            //-----------------------------------------------
        }
        //=====================================================================



        //=====================================================================
        public void Message_Error(string pText)
        {
            //-----------------------------------------------
            MessageBox.Show(pText, "Erreur !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //-----------------------------------------------
        }
        public void Message_FataleError(string pText)
        {
            //-----------------------------------------------
            MessageBox.Show(pText, "Erreur !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            FermerWinuntu();
            //-----------------------------------------------
        }
        public bool Message_Continue(string pText)
        {
            //-----------------------------------------------
            if (MessageBox.Show(pText, "Continuer ?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                return true;
            }
            else
            {
                return false;
            }
            //-----------------------------------------------
        }
        //=====================================================================



        //=====================================================================
        // Gestion des bouttons
        private void ToolStripCmd_Click(object sender, EventArgs e)
        {
            //-----------------------------------------------
            AjouterInstanceCmd();
            //-----------------------------------------------
        }
        private void ToolStripPS_Click(object sender, EventArgs e)
        {
            //-----------------------------------------------
            AjouterInstancePS();
            //-----------------------------------------------
        }
        private void ToolStripFolder_Click(object sender, EventArgs e)
        {
            //-----------------------------------------------
            AjouterDossierInstance();
            //-----------------------------------------------
        }
        private void ToolStripFile_Click(object sender, EventArgs e)
        {
            //-----------------------------------------------
            AjouterFichierInstance();
            //-----------------------------------------------
        }
        private void ToolStripClose_Click(object sender, EventArgs e)
        {
            //-----------------------------------------------
            FermerInstance(CurrentInstance);
            //-----------------------------------------------
        }
        private void ToolStripExpand_Click(object sender, EventArgs e)
        {
            //-----------------------------------------------
            TreeListeOnglets.ExpandAll();
            //-----------------------------------------------
        }
        private void ToolStripCollapse_Click(object sender, EventArgs e)
        {
            //-----------------------------------------------
            TreeListeOnglets.CollapseAll();
            //-----------------------------------------------
        }
        private void ToolStripAddProfil_Click(object sender, EventArgs e)
        {
            //-----------------------------------------------
            AjouterProfilDialog();
            //-----------------------------------------------
        }
        private void ToolStripModProfil_Click(object sender, EventArgs e)
        {
            //-----------------------------------------------
            ModifierProfilDialog();
            //-----------------------------------------------
        }
        private void ToolStripRunAs_Click(object sender, EventArgs e)
        {
            //-----------------------------------------------
            RunAsAdmin();
            //-----------------------------------------------
        }
        private void ToolStripNormal_Click(object sender, EventArgs e)
        {
            //-----------------------------------------------
            RunAsNormal();
            //-----------------------------------------------
        }
        private void ToolStripTaskmgr_Click(object sender, EventArgs e)
        {
            //-----------------------------------------------
            OpenTaskMgr();
            //-----------------------------------------------
        }
        private void ToolStripRegedit_Click(object sender, EventArgs e)
        {
            //-----------------------------------------------
            OpenRegedit();
            //-----------------------------------------------
        }
        private void ToolStripInfos_Click(object sender, EventArgs e)
        {
            //-----------------------------------------------
            AfficherInfos();
            //-----------------------------------------------
        }
        //=====================================================================



        //=====================================================================
        // Gestion des Instances
        public void AjouterInstanceCmd()
        {
            //-----------------------------------------------
            AjouterInstance("cmd.exe", @" /k prompt [$D$S$T]$S%username%:~$$ && echo; _       ___                   __         && echo;^| ^|     / (_)___  __  ______  / /___  __  && echo;^| ^| /^| / / / __ \/ / / / __ \/ __/ / / / version && echo;^| ^|/ ^|/ / / / / / /_/ / / / / /_/ /_/ /  " + VERSION + @"&& echo;^|__/^|__/_/_/ /_/\__,_/_/ /_/\__/\__,_/  && echo;" + COPYRIGHT + @" && title Invite de commandes",
                CurrentProfil.CommandCmd, "&&");
            //-----------------------------------------------
        }
        public void AjouterInstancePS()
        {
            //-----------------------------------------------
            AjouterInstance("powershell.exe", @" -NoExit -Command function prompt {\""[$(get-date)] $env:UserName~:$ \""} ""echo ' _       ___                   __'"" ""'| |     / (_)___  __  ______  / /___  __'"" ""'| | /| / / / __ \/ / / / __ \/ __/ / / / version'"" ""'| |/ |/ / / / / / /_/ / / / / /_/ /_/ /  " + VERSION + @"'"" ""'|__/|__/_/_/ /_/\__,_/_/ /_/\__/\__,_/'"" ""'" + COPYRIGHT + @"'"" ""''""",
                CurrentProfil.CommandPS, ";");
            //-----------------------------------------------
        }
        private void AjouterInstance(string pProcess, string pArgs, string pCommand, string pCommandSep)
        {
            //-----------------------------------------------
            TreeNode node = new TreeNode();
            Process process = new Process();
            Instance instance = new Instance();
            process.StartInfo.FileName = pProcess;
            if (pCommand == "")
            {
                process.StartInfo.Arguments = pArgs;
            }
            else
            {
                process.StartInfo.Arguments = pArgs + " " + pCommandSep + " " + pCommand;
            }
            process.StartInfo.WorkingDirectory = CurrentProfil.Directory;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            process.EnableRaisingEvents = true;
            process.Exited += new EventHandler((s, e) =>
            {
                FermerInstance(instance);
            });
            process.Start();
            while (process.MainWindowHandle == (IntPtr)0)
            {
            }
            SetParent(process.MainWindowHandle, PanelViewOnglets.Handle);

            instance.Pro = CurrentProfil;
            instance.Proc = process;
            instance.ChildNode = node;

            CurrentProfil.ParentNode.Nodes.Add(node);
            CurrentProfil.ListeInstances.Add(instance);

            DefinirInstance(instance);
            AcualiserNodes(new object(), new EventArgs());
            //-----------------------------------------------
        }
        private void DefinirInstance(Instance pInstance)
        {
            //-----------------------------------------------
            // Cacher l'instance si elle existe (premier démarrage)
            if (CurrentInstance != null)
            {
                ShowWindow(CurrentInstance.Proc.MainWindowHandle, 0);
            }
            CurrentInstance = pInstance;
            SetForegroundWindow(pInstance.Proc.MainWindowHandle);
            AfficherInstance();
            //-----------------------------------------------
        }
        delegate void dFermerInstance(Instance pInstance);
        private void FermerInstance(Instance pInstance)
        {
            //-----------------------------------------------
            if (this.TreeListeOnglets.InvokeRequired)
            {
                this.TreeListeOnglets.Invoke(new dFermerInstance(FermerInstance), pInstance);
            }
            else
            {
                pInstance.ChildNode.Remove();

                while (!pInstance.Proc.HasExited)
                {
                    pInstance.Proc.Refresh();
                    pInstance.Proc.Kill();
                }

                pInstance.Pro.ListeInstances.Remove(pInstance);
                if (CurrentInstance == pInstance)
                {
                    // Chercher une nouvelle instance
                    CurrentInstance = null;
                    foreach (Profil profil in ListeProfils)
                    {
                        foreach (Instance instance in profil.ListeInstances)
                        {
                            CurrentInstance = instance;
                        }
                    }
                    // Si il n'y a plus aucune instance
                    if (CurrentInstance == null)
                    {
                        FermerWinuntu();
                        return; // Important pour quitter
                    }
                    DefinirInstance(CurrentInstance);
                }
                pInstance = null;
            }
            //-----------------------------------------------
        }
        private void FermerToutesLesInstancesDUnProfil(Profil pProfil)
        {
            //-----------------------------------------------
            // Un foreach brise le IN
            while (pProfil.ListeInstances.Count != 0)
            {
                FermerInstance(pProfil.ListeInstances[0]);
            }
            //-----------------------------------------------
        }
        private void FermerToutesLesInstances()
        {
            //-----------------------------------------------
            foreach (Profil profil in ListeProfils)
            {
                FermerToutesLesInstancesDUnProfil(profil);
            }
            //-----------------------------------------------
        }
        //=====================================================================



        //=====================================================================
        // Actualiser ram
        private void ActualiserRAM(object sender, EventArgs e)
        {
            //-----------------------------------------------
            ObjectQuery wql = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
            ManagementObjectSearcher os_searcher = new ManagementObjectSearcher(wql);

            foreach (ManagementObject mobj in os_searcher.Get())
            {
                double used;
                double total;
                double percent;

                used = (Convert.ToDouble(mobj["TotalVisibleMemorySize"]) - Convert.ToDouble(mobj["FreePhysicalMemory"])) / 1024;
                total = Convert.ToDouble(mobj["TotalVisibleMemorySize"]) / 1024;
                percent = 100 /
                       Convert.ToDouble(mobj["TotalVisibleMemorySize"]) *
                       (Convert.ToDouble(mobj["TotalVisibleMemorySize"]) - Convert.ToDouble(mobj["FreePhysicalMemory"]));

                LabelRAM.Text = Math.Round(used, 0) + "Mo / " + Math.Round(total, 0) + "Mo (" + Math.Round(percent, 0) + "%)";

                if (percent < 60)
                {
                    ButtonCouleurRAM.BackColor = Color.FromArgb(0, 255, 0);
                }
                else if (percent < 80)
                {
                    ButtonCouleurRAM.BackColor = Color.FromArgb(255, 255, 0);
                }
                else
                {
                    ButtonCouleurRAM.BackColor = Color.FromArgb(255, 0, 0);
                }
            }

            os_searcher.Dispose();
            //-----------------------------------------------
        }
        //=====================================================================



        //=====================================================================
        // Actualiser instances
        private void PanelViewOnglets_Resize(object sender, EventArgs e)
        {
            AfficherInstance(false);
        }
        private void ActualiserInstance(object sender, EventArgs e)
        {
            //-----------------------------------------------
            AfficherInstance(false);
            //-----------------------------------------------
        }
        public void AfficherInstance(bool pSetUp = true)
        {
            //-----------------------------------------------
            RedrawWindow(CurrentInstance.Proc.MainWindowHandle, (IntPtr)0, (IntPtr)0, 0x0400/*RDW_FRAME*/ | 0x0100/*RDW_UPDATENOW*/ | 0x0001/*RDW_INVALIDATE*/);
            MoveWindow(CurrentInstance.Proc.MainWindowHandle, 0, 0, PanelViewOnglets.Width - 4, PanelViewOnglets.Height - 4, true);
            SetWindowPos(CurrentInstance.Proc.MainWindowHandle, (IntPtr)(-1), 0, 0, 0, 0, 0);

            if (pSetUp)
            {
                if (File.Exists(CurrentInstance.Pro.Wallpaper))
                {
                    PictureWallpaper.Image = (Image)(new Bitmap(CurrentInstance.Pro.Wallpaper));
                    try
                    {
                        SetLayeredWindowAttributes(CurrentInstance.Proc.MainWindowHandle, 0, Convert.ToInt32(CurrentInstance.Pro.Opacity), 2);
                    }
                    catch { }
                }
                else
                {
                    PictureWallpaper.Image = null;
                }
                SetWindowLongA(CurrentInstance.Proc.MainWindowHandle, -16, 0x80000000L);
                ShowWindow(CurrentInstance.Proc.MainWindowHandle, 5);
                EnableWindow(CurrentInstance.Proc.MainWindowHandle, true);
            }
            //-----------------------------------------------
        }
        //=====================================================================



        //=====================================================================
        //Gestions des nodes
        private void AcualiserNodes(object sender, EventArgs e)
        {
            //-----------------------------------------------
            int counttotal = 0;
            foreach (Profil profil in ListeProfils)
            {
                counttotal += profil.ListeInstances.Count;

                profil.ParentNode.Text = profil.Name + " (" + profil.ListeInstances.Count + ")";
                profil.ToolItem.Text = profil.Name + " (" + profil.ListeInstances.Count + ")";

                if (profil == CurrentProfil)
                {
                    profil.ParentNode.Text = char.ConvertFromUtf32(0x1f846) + " " + profil.ParentNode.Text;
                    ((ToolStripMenuItem)(profil.ToolItem)).Checked = true;
                    LabelProfilActuel.Text = profil.Name;
                }
                else
                {
                    ((ToolStripMenuItem)(profil.ToolItem)).Checked = false;
                }

                foreach (Instance instance in profil.ListeInstances)
                {
                    var length = GetWindowTextLength(instance.Proc.MainWindowHandle) + 1;
                    var title = new StringBuilder(length);
                    GetWindowText(instance.Proc.MainWindowHandle, title, length);
                    instance.ChildNode.Text = title.ToString();


                    if (instance == CurrentInstance)
                    {
                        instance.ChildNode.Text = char.ConvertFromUtf32(0x1f846) + " " + instance.ChildNode.Text;
                    }
                }
            }
            LabelNbTotal.Text = counttotal.ToString();
            //-----------------------------------------------
        }
        private void TreeListeOnglets_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            //-----------------------------------------------
            foreach (Profil profil in ListeProfils)
            {
                foreach (Instance instance in profil.ListeInstances)
                {
                    if (instance.ChildNode == e.Node)
                    {
                        DefinirInstance(instance);
                        return;
                    }
                }
            }
            //-----------------------------------------------
        }
        //=====================================================================



        //=====================================================================
        private ToolStripItem AjouterSubRaccourciOuvrir(Raccourci pRaccourci)
        {
            //-----------------------------------------------
            ToolStripItem toolitem = new ToolStripMenuItem();
            toolitem.Text = "Ouvrir le raccourci";
            toolitem.Click += new EventHandler((s, e_) =>
            {
                OuvrirRaccourci(pRaccourci);
            });
            return toolitem;
            //-----------------------------------------------
        }
        private ToolStripItem AjouterSubRaccourciupprimer(Raccourci pRaccourci)
        {
            //-----------------------------------------------
            ToolStripItem toolitem = new ToolStripMenuItem();
            toolitem.Text = "Supprimer le raccourci";
            toolitem.Click += new EventHandler((s, e_) =>
            {
                SupprimerRaccourci(pRaccourci);
            });

            return toolitem;
            //-----------------------------------------------
        }
        private ToolStripItem AjouterSubRaccourciCreer(Raccourci pRaccourci)
        {
            //-----------------------------------------------
            ToolStripItem toolitem = new ToolStripMenuItem();
            toolitem.Text = "Créer ce raccourci";
            toolitem.Click += new EventHandler((s, e_) =>
            {
                CreerRaccourci(pRaccourci);
            });
            return toolitem;
            //-----------------------------------------------
        }
        private void ChargerRaccourci()
        {
            //-----------------------------------------------
            // Liste les Raccourci
            for (int i = 1; i < 10; i++)
            {
                Raccourci raccourci = new Raccourci();
                ToolStripMenuItem toolitem = new ToolStripMenuItem();

                string path = IniConfig.ReadKey("Shortcut_" + i, "Path");

                if (path == "")
                {
                    toolitem.Text = i + ": ...";
                    toolitem.DropDownItems.Add(AjouterSubRaccourciCreer(raccourci));
                }
                else
                {
                    toolitem.Text = i + ": " + path;
                    toolitem.DropDownItems.Add(AjouterSubRaccourciOuvrir(raccourci));
                    toolitem.DropDownItems.Add(new ToolStripSeparator());
                    toolitem.DropDownItems.Add(AjouterSubRaccourciupprimer(raccourci));
                }

                StripRaccourcis.DropDownItems.Add(toolitem);

                raccourci.Keynum = i;
                raccourci.ToolItem = toolitem;
                raccourci.Path = path;
            }
            //-----------------------------------------------
        }
        private void CreerRaccourci(Raccourci pRaccourci)
        {
            //-----------------------------------------------
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Choisissez un fichier...";
            dialog.Filter = "Tous les fichiers (*.*)|*.*";
            dialog.RestoreDirectory = true;
            dialog.CheckFileExists = true;
            dialog.CheckPathExists = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                IniConfig.WriteKey("Shortcut_" + pRaccourci.Keynum, "Path", dialog.FileName);
                pRaccourci.Path = dialog.FileName;

                pRaccourci.ToolItem.DropDownItems.Clear();
                pRaccourci.ToolItem.Text = pRaccourci.Keynum + ": " + pRaccourci.Path;
                pRaccourci.ToolItem.DropDownItems.Add(AjouterSubRaccourciOuvrir(pRaccourci));
                pRaccourci.ToolItem.DropDownItems.Add(new ToolStripSeparator());
                pRaccourci.ToolItem.DropDownItems.Add(AjouterSubRaccourciupprimer(pRaccourci));
            }

            dialog.Dispose();
            //-----------------------------------------------
        }
        private void OuvrirRaccourci(Raccourci pRaccourci)
        {
            //-----------------------------------------------
            try
            {
                Process.Start("explorer.exe", pRaccourci.Path);
            }
            catch { }
            //-----------------------------------------------
        }
        private void SupprimerRaccourci(Raccourci pRaccourci)
        {
            //-----------------------------------------------
            IniConfig.DeleteKey("Shortcut_" + pRaccourci.Keynum, "Path");
            pRaccourci.Path = "";

            pRaccourci.ToolItem.DropDownItems.Clear();
            pRaccourci.ToolItem.Text = pRaccourci.Keynum + ": ...";
            pRaccourci.ToolItem.DropDownItems.Add(AjouterSubRaccourciCreer(pRaccourci));
            //-----------------------------------------------
        }
        //=====================================================================



        //=====================================================================
        private void AfficherInfos()
        {
            //-----------------------------------------------
            MenuInfos form = new MenuInfos();
            form.Owner = this;
            form.ShowDialog();
            form.Dispose();
            //-----------------------------------------------
        }
        private void OpenRegedit()
        {
            //-----------------------------------------------
            try
            {
                Process.Start("regedit.exe");
            }
            catch { }
            //-----------------------------------------------
        }
        private void OpenTaskMgr()
        {
            //-----------------------------------------------
            try
            {
                Process.Start("taskmgr.exe");
            }
            catch { }
            //-----------------------------------------------
        }
        private void RunAsNormal()
        {
            //-----------------------------------------------
            if (Message_Continue("Ceci va nécessiter un redémarrage. Continuer ?"))
            {
                try
                {
                    Process.Start("explorer.exe", Application.ExecutablePath);
                    FermerWinuntu();
                }
                catch { }
            }
            //-----------------------------------------------
        }
        private void RunAsAdmin()
        {
            //-----------------------------------------------
            if (Message_Continue("Ceci va nécessiter un redémarrage. Continuer ?"))
            {
                try
                {
                    ProcessStartInfo process = new ProcessStartInfo();
                    process.FileName = Application.ExecutablePath;
                    process.Verb = "runas";
                    Process.Start(process);
                    FermerWinuntu();
                }
                catch { }
            }
            //-----------------------------------------------
        }
        private void FermerWinuntu()
        {
            //-----------------------------------------------
            // Empeche une erreur pendant la disposition des ressources
            TimerActualiserRAM.Stop();
            TimerActualiseNodes.Stop();
            TimerKeys.Stop();

            FermerToutesLesInstances();

            Application.Exit();
            //-----------------------------------------------
        }
        private void AjouterProfilDialog()
        {
            //-----------------------------------------------
            MenuProfil form = new MenuProfil();
            form.Owner = this;
            form.ShowDialog();
            form.Dispose();
            //-----------------------------------------------
        }
        private void ModifierProfilDialog()
        {
            //-----------------------------------------------
            if (CurrentProfil == ListeProfils[0])
            {
                Message_Error("Impossible de modifier le profil par défaut.");
                return;
            }

            MenuProfil form = new MenuProfil(CurrentProfil);
            form.Owner = this;
            form.ShowDialog();
            form.Dispose();
            //-----------------------------------------------
        }
        private void AjouterFichierInstance()
        {
            //-----------------------------------------------
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Choisissez un fichier à ajouter...";
            dialog.Filter = "Tous les fichiers (*.*)|*.*";
            dialog.RestoreDirectory = true;
            dialog.CheckFileExists = true;
            dialog.CheckPathExists = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                SetForegroundWindow(CurrentInstance.Proc.MainWindowHandle);
                SendKeys.Send(dialog.FileName);
            }

            dialog.Dispose();
            //-----------------------------------------------
        }
        private void AjouterDossierInstance()
        {
            //-----------------------------------------------
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "Choisissez un dossier à ajouter...";
            dialog.ShowNewFolderButton = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                SetForegroundWindow(CurrentInstance.Proc.MainWindowHandle);
                SendKeys.Send(dialog.SelectedPath);
            }

            dialog.Dispose();
            //-----------------------------------------------
        }
        //=====================================================================



        //=====================================================================
        private void StripAjouterCmd_Click(object sender, EventArgs e)
        {
            //-----------------------------------------------
            AjouterInstanceCmd();
            //-----------------------------------------------
        }
        private void StripAjouterPS_Click(object sender, EventArgs e)
        {
            //-----------------------------------------------
            AjouterInstancePS();
            //-----------------------------------------------
        }
        private void StripAjouterDossier_Click(object sender, EventArgs e)
        {
            //-----------------------------------------------
            AjouterDossierInstance();
            //-----------------------------------------------
        }
        private void StripAjouterFichier_Click(object sender, EventArgs e)
        {
            //-----------------------------------------------
            AjouterFichierInstance();
            //-----------------------------------------------
        }
        private void StripFermer_Click(object sender, EventArgs e)
        {
            //-----------------------------------------------
            FermerInstance(CurrentInstance);
            //-----------------------------------------------
        }
        private void StripDeplier_Click(object sender, EventArgs e)
        {
            //-----------------------------------------------
            TreeListeOnglets.ExpandAll();
            //-----------------------------------------------
        }
        private void StripReplier_Click(object sender, EventArgs e)
        {
            //-----------------------------------------------
            TreeListeOnglets.CollapseAll();
            //-----------------------------------------------
        }
        private void StripCreer_Click(object sender, EventArgs e)
        {
            //-----------------------------------------------
            AjouterProfilDialog();
            //-----------------------------------------------
        }
        private void StripModifier_Click(object sender, EventArgs e)
        {
            //-----------------------------------------------
            ModifierProfilDialog();
            //-----------------------------------------------
        }
        private void StripRunAsAdmin_Click(object sender, EventArgs e)
        {
            //-----------------------------------------------
            RunAsAdmin();
            //-----------------------------------------------
        }
        private void StripRunAsNormal_Click(object sender, EventArgs e)
        {
            //-----------------------------------------------
            RunAsNormal();
            //-----------------------------------------------
        }
        private void StripTaskMgr_Click(object sender, EventArgs e)
        {
            //-----------------------------------------------
            OpenTaskMgr();
            //-----------------------------------------------
        }
        private void StripRegedit_Click(object sender, EventArgs e)
        {
            //-----------------------------------------------
            OpenRegedit();
            //-----------------------------------------------
        }
        private void StripInfos_Click(object sender, EventArgs e)
        {
            //-----------------------------------------------
            AfficherInfos();
            //-----------------------------------------------
        }
        //=====================================================================



        //=====================================================================
        private void ActualiserKeysPressed(object sender, EventArgs e)
        {
            //-----------------------------------------------
            if (GetActiveWindow() != this.Handle) { return; }

            if (GetAsyncKeyState(0x70) != 0) // F1
            {
                AjouterInstanceCmd();
            }
            if (GetAsyncKeyState(0x71) != 0) // F2
            {
                AjouterInstancePS();
            }
            if (GetAsyncKeyState(0x72) != 0) // F3
            {
                FermerInstance(CurrentInstance);
            }
            if (GetAsyncKeyState(0x11) != 0 && GetAsyncKeyState(0x41) != 0) // Ctrl + A
            {
                TreeListeOnglets.ExpandAll();
            }
            if (GetAsyncKeyState(0x11) != 0 && GetAsyncKeyState(0x5A) != 0) // Ctrl + Z
            {
                TreeListeOnglets.CollapseAll();
            }
            if (GetAsyncKeyState(0x7B) != 0) // F12
            {
                AfficherInfos();
            }
            //-----------------------------------------------
        }
        //=====================================================================

    }
    //=====================================================================
}
