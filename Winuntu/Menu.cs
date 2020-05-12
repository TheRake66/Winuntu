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
        public const string VERSION = "6.2.0.0";
        public const string COPYRIGHT = "Copyright © Bustos Thibault (TheRake66) - 2020";
        public const string ARCH = "64-bit";

        // Dossiers
        public static string FOLDER_INSTALL = Application.UserAppDataPath;
        public static string FOLDER_PROFILS = FOLDER_INSTALL + @"\Profils";
        // Fichiers
        public static string FILE_CONFIG = FOLDER_INSTALL + @"\config.ini";

        // Variables
        Profil CurrentProfil;
        Instance CurrentInstance;
        List<Profil> ListeProfils = new List<Profil>();
        Ini IniConfig = new Ini();

        // Imports des dlls
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
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern int GetWindowTextA(IntPtr hWnd, IntPtr lpString, int nMaxCount);
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
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
            // Seule la nouvelle console de windows 10 peut être modifiée comme tel
            try
            {
                var reg = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                string productName = (string)reg.GetValue("ProductName");
                if (!productName.Contains("Windows 10")) { Message_FataleError("Compatible uniquement avec Windows 10 et plus."); }
            }
            catch { Message_FataleError("Impossible de vérifier la version de Windows."); }
            //-----------------------------------------------


            //-----------------------------------------------
            // Force la nouvelle console
            try
            {
                var cle = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Console", true);
                cle.SetValue("ForceV2", 1);
                cle.Close();
            }
            catch { Message_FataleError("Impossible d'accéder au registre pour activer la nouvelle console."); }
            //-----------------------------------------------


            //-----------------------------------------------
            // Verifi si admin
            if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator)) { this.Text += " (Administrateur)"; }
            //-----------------------------------------------


            //-----------------------------------------------
            // Créer les dossier de l'application
            if (!Directory.Exists(FOLDER_INSTALL)) { Directory.CreateDirectory(FOLDER_INSTALL); }
            if (!Directory.Exists(FOLDER_PROFILS)) { Directory.CreateDirectory(FOLDER_PROFILS); }
            //-----------------------------------------------


            //-----------------------------------------------
            // Charge les parametres preenregistrés
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

            // Ferme toutes les instances
            FermerWinuntu();
            //-----------------------------------------------
        }
        private void Menu_ResizeEnd(object sender, EventArgs e)
        {
            //-----------------------------------------------
            // Donne le focus à la fenetre console lors d'un déplacement ou redimensionnement
            if (CurrentInstance != null) { SetForegroundWindow(CurrentInstance.Proc.MainWindowHandle); }
            //-----------------------------------------------
        }
        //=====================================================================



        //=====================================================================
        private void ChargerProfils()
        {
            //-----------------------------------------------
            // ajouter le profil par defaut
            AjouterProfil(
                "Défaut", // Nom par défaut
                "",
                "",
                255, // Opacité par défaut
                "",
                "",
                "",
                ""
                );

            // Liste les profils
            string[] listeprofil = Directory.GetFiles(FOLDER_PROFILS, "*.ini");
            foreach (string fileprofil in listeprofil)
            {
                // Instance un ini avec l'ini traité
                Ini fileprofilini = new Ini();
                fileprofilini.SetFile(fileprofil);

                // Récupère l'oppacity (255 en cas de problème)
                string opacity = fileprofilini.ReadKey("Settings", "Opacity");
                int opacityint = 255;
                try { opacityint = Convert.ToInt32(opacity); }
                catch { }

                // Récupère tout les autres parametres
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
            }

            // Defini l'ancien profil selectionner
            string lastprofil = IniConfig.ReadKey("Options", "LastProfilSelected");
            foreach (Profil profil in ListeProfils)
            {
                // Cherche le profil par rapport à son fichier
                if (profil.FichierIni == lastprofil) { DefinirProfil(profil); }
            }

            // Defini le profil par defaut
            if (CurrentProfil == null) { DefinirProfil(ListeProfils[0]); }
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
            string pFichierIni,
            bool pCreated = false)
        {
            //-----------------------------------------------
            Profil profil = new Profil(); // Objet profil
            TreeNode node = new TreeNode(); // Handle du treenode
            ToolStripItem toolitem = new ToolStripMenuItem(); // Handle du toolstrip

            // toolstrip
            toolitem.Text = pName + " (0)";
            toolitem.Click += new EventHandler((s, e) => { DefinirProfil(profil); });
            StripProfils.DropDownItems.Add(toolitem);

            // treenode
            node.Text = pName + " (0)";
            TreeListeOnglets.Nodes.Add(node);

            // profil
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

            // Ajoute le profil à la liste
            ListeProfils.Add(profil);

            if (pCreated) { DefinirProfil(profil); }
            //-----------------------------------------------
        }
        private void DefinirProfil(Profil pProfil)
        {
            //-----------------------------------------------
            // Enregistre que c'est le dernier profil selectionné
            IniConfig.WriteKey("Options", "LastProfilSelected", pProfil.FichierIni);
            CurrentProfil = pProfil;
            //-----------------------------------------------
        }
        public void SupprimerProfil(Profil pProfil)
        {
            //-----------------------------------------------
            // Ferme ses instances actives
            FermerToutesLesInstancesDUnProfil(pProfil);

            ListeProfils.Remove(pProfil);
            pProfil.ParentNode.Remove();

            // Chercher un nouveau profil si c'était le profil séléctionné
            if (CurrentProfil == pProfil)
            {
                foreach (Profil profil in ListeProfils)
                {
                    CurrentProfil = profil;
                }
                DefinirProfil(CurrentProfil);
            }

            // Supprime le fichier du profil
            try { File.Delete(pProfil.FichierIni); }
            catch { }
            //-----------------------------------------------
        }
        //=====================================================================



        //=====================================================================
        public static void Message_Error(string pText)
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
        public static bool Message_Continue(string pText)
        {
            //-----------------------------------------------
            if (MessageBox.Show(pText, "Continuer ?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) { return true; }
            else { return false; }
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
            TreeNode node = new TreeNode(); // treenode de l'instance
            Process process = new Process(); // processus de l'instance
            Instance instance = new Instance(); // instance

            // Défini les parametres du processus
            process.StartInfo.FileName = pProcess;
            if (pCommand == "") { process.StartInfo.Arguments = pArgs; }
            else { process.StartInfo.Arguments = pArgs + " " + pCommandSep + " " + pCommand; }
            process.StartInfo.WorkingDirectory = CurrentProfil.Directory;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            process.EnableRaisingEvents = true;
            process.Exited += new EventHandler((s, e) => { FermerInstance(instance); });
            process.Start();

            // Attends que la fenetre s'affiche
            while (process.MainWindowHandle == (IntPtr)0) { }
            SetParent(process.MainWindowHandle, PanelViewOnglets.Handle);

            // Hydrate l'objet instance
            instance.Pro = CurrentProfil;
            instance.Proc = process;
            instance.ChildNode = node;

            // Ajoute l'instance au profil actif
            CurrentProfil.ParentNode.Nodes.Add(node);
            CurrentProfil.ListeInstances.Add(instance);

            // Affiche l'instance
            DefinirInstance(instance);
            AcualiserNodes(new object(), new EventArgs());
            //-----------------------------------------------
        }
        private void DefinirInstance(Instance pInstance)
        {
            //-----------------------------------------------
            // Cacher l'instance si elle existe (premier démarrage pas d'instance à cacher)
            if (CurrentInstance != null) { ShowWindow(CurrentInstance.Proc.MainWindowHandle, 0); }
            CurrentInstance = pInstance;
            AfficherInstance();
            //-----------------------------------------------
        }
        delegate void dFermerInstance(Instance pInstance); // délégué pour l'invoke si l'instance se ferme toute seule
        private void FermerInstance(Instance pInstance)
        {
            //-----------------------------------------------
            if (this.TreeListeOnglets.InvokeRequired) { this.TreeListeOnglets.Invoke(new dFermerInstance(FermerInstance), pInstance); }
            else
            {
                // Ordonne de fermer jusqu'à fermeture
                pInstance.Proc.Refresh();
                while (!pInstance.Proc.HasExited)
                {
                    pInstance.Proc.Kill();
                    pInstance.Proc.Refresh();
                }

                // Supprime les object en lien
                pInstance.Pro.ListeInstances.Remove(pInstance);
                pInstance.ChildNode.Remove();

                // Chercher une nouvelle instance si c'était l'instance active
                if (CurrentInstance == pInstance)
                {
                    CurrentInstance = null;
                    // Cherche d'abord dans le profil actif
                    foreach (Instance instance in CurrentProfil.ListeInstances)
                    {
                        CurrentInstance = instance;
                    }
                    // Sinon cherche dans tous les profils
                    if (CurrentInstance == null)
                    {
                        foreach (Profil profil in ListeProfils)
                        {
                            foreach (Instance instance in profil.ListeInstances)
                            {
                                CurrentInstance = instance;
                            }
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
                else
                {
                    // Supprime les object en lien
                    pInstance = null;
                }
            }
            //-----------------------------------------------
        }
        private void FermerToutesLesInstancesDUnProfil(Profil pProfil)
        {
            //-----------------------------------------------
            // Un foreach brise le IN
            while (pProfil.ListeInstances.Count != 0) { FermerInstance(pProfil.ListeInstances[0]); }
            //-----------------------------------------------
        }
        private void FermerToutesLesInstances()
        {
            //-----------------------------------------------
            foreach (Profil profil in ListeProfils) { FermerToutesLesInstancesDUnProfil(profil); }
            //-----------------------------------------------
        }
        //=====================================================================



        //=====================================================================
        // Actualiser ram
        private void ActualiserRAM(object sender, EventArgs e)
        {
            //-----------------------------------------------
            // Recupere la bb system
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

                // Change la couleur
                if (percent < 60) { ButtonCouleurRAM.BackColor = Color.FromArgb(0, 255, 0); }
                else if (percent < 80) { ButtonCouleurRAM.BackColor = Color.FromArgb(255, 255, 0); }
                else { ButtonCouleurRAM.BackColor = Color.FromArgb(255, 0, 0); }
            }

            os_searcher.Dispose();
            //-----------------------------------------------
        }
        //=====================================================================



        //=====================================================================
        // Actualiser instances
        private void PanelViewOnglets_Resize(object sender, EventArgs e)
        {
            //-----------------------------------------------
            AfficherInstance(false);
            //-----------------------------------------------
        }
        private void ActualiserInstance(object sender, EventArgs e)
        {
            //-----------------------------------------------
            AfficherInstance(false);
            //-----------------------------------------------
        }
        public void AfficherInstance(bool pSetUp = true /* Met au premier plan et charge l'image */)
        {
            //-----------------------------------------------
            if (CurrentInstance == null) { return; }

            // Déplace la fenetre, il faut associer les trois fonctions pour pouvoir avoir un déplacement graphique et mémoire (voir bug version 5)
            RedrawWindow(CurrentInstance.Proc.MainWindowHandle, (IntPtr)0, (IntPtr)0, 0x0400/*RDW_FRAME*/ | 0x0100/*RDW_UPDATENOW*/ | 0x0001/*RDW_INVALIDATE*/);
            MoveWindow(CurrentInstance.Proc.MainWindowHandle, 0, 0, PanelViewOnglets.Width - 4, PanelViewOnglets.Height - 4, true);
            SetWindowPos(CurrentInstance.Proc.MainWindowHandle, (IntPtr)(-1), 0, 0, 0, 0, 0);

            if (pSetUp)
            {
                // Charge l'image si elle existe et change l'oppacité
                if (File.Exists(CurrentInstance.Pro.Wallpaper))
                {
                    PictureWallpaper.Image = (Image)(new Bitmap(CurrentInstance.Pro.Wallpaper));
                    try { SetLayeredWindowAttributes(CurrentInstance.Proc.MainWindowHandle, 0, Convert.ToInt32(CurrentInstance.Pro.Opacity), 2); }
                    catch { }
                }
                else { PictureWallpaper.Image = null; }

                // Met au premier plan et enlève les bords de la console
                SetWindowLongA(CurrentInstance.Proc.MainWindowHandle, -16, 0x80000000L);
                ShowWindow(CurrentInstance.Proc.MainWindowHandle, 5);
                SetForegroundWindow(CurrentInstance.Proc.MainWindowHandle);
            }
            //-----------------------------------------------
        }
        //=====================================================================



        //=====================================================================
        //Gestions des nodes
        private void AcualiserNodes(object sender, EventArgs e)
        {
            //-----------------------------------------------
            int counttotal = 0; // Compte le nombre d'instance

            // Parcours chaque profil
            foreach (Profil profil in ListeProfils)
            {
                counttotal += profil.ListeInstances.Count;

                profil.ParentNode.Text = profil.Name + " (" + profil.ListeInstances.Count + ")";
                profil.ToolItem.Text = profil.Name + " (" + profil.ListeInstances.Count + ")";

                // Coche le bon profil dans le toolstrip et le treeview (hex 0x1f846)
                if (profil == CurrentProfil)
                {
                    profil.ParentNode.Text = char.ConvertFromUtf32(0x1f846) + " " + profil.ParentNode.Text;
                    ((ToolStripMenuItem)(profil.ToolItem)).Checked = true;
                    LabelProfilActuel.Text = profil.Name;
                }
                else { ((ToolStripMenuItem)(profil.ToolItem)).Checked = false; }


                // Parcours chaque instance de ce profil
                foreach (Instance instance in profil.ListeInstances)
                {
                    // Récupère le titre de la console et le met dans le treenode
                    var length = GetWindowTextLength(instance.Proc.MainWindowHandle) + 1;
                    var title = new StringBuilder(length);
                    GetWindowText(instance.Proc.MainWindowHandle, title, length);
                    instance.ChildNode.Text = title.ToString();

                    // Coche la bonne instance dans le treeview (hex 0x1f846)
                    if (instance == CurrentInstance) { instance.ChildNode.Text = char.ConvertFromUtf32(0x1f846) + " " + instance.ChildNode.Text; }
                }
            }

            LabelNbTotal.Text = counttotal.ToString(); // Affiche le nombre d'instance
            //-----------------------------------------------
        }
        private void TreeListeOnglets_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            //-----------------------------------------------
            // Quand on veut changer d'instance
            foreach (Profil profil in ListeProfils)
            {
                foreach (Instance instance in profil.ListeInstances)
                {
                    // Compare avec le handle cliqué
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
            toolitem.Click += new EventHandler((s, e_) => { OuvrirRaccourci(pRaccourci); });
            return toolitem;
            //-----------------------------------------------
        }
        private ToolStripItem AjouterSubRaccourciupprimer(Raccourci pRaccourci)
        {
            //-----------------------------------------------
            ToolStripItem toolitem = new ToolStripMenuItem();
            toolitem.Text = "Supprimer le raccourci";
            toolitem.Click += new EventHandler((s, e_) => { SupprimerRaccourci(pRaccourci); });
            return toolitem;
            //-----------------------------------------------
        }
        private ToolStripItem AjouterSubRaccourciCreer(Raccourci pRaccourci)
        {
            //-----------------------------------------------
            ToolStripItem toolitem = new ToolStripMenuItem();
            toolitem.Text = "Créer ce raccourci";
            toolitem.Click += new EventHandler((s, e_) => { CreerRaccourci(pRaccourci); });
            return toolitem;
            //-----------------------------------------------
        }
        private void ChargerRaccourci()
        {
            //-----------------------------------------------
            // Liste les Raccourci enregistrés
            for (int i = 1; i < 10; i++)
            {
                // Prépare les objects
                Raccourci raccourci = new Raccourci();
                ToolStripMenuItem toolitem = new ToolStripMenuItem();

                // Récupere le raccourci
                string path = IniConfig.ReadKey("Shortcut_" + i, "Path");

                // Si aucun n'a été enregistrer
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

                // Hydrate l'object
                raccourci.Keynum = i;
                raccourci.ToolItem = toolitem;
                raccourci.Path = path;

                // Ajoute l'objet final
                StripRaccourcis.DropDownItems.Add(toolitem);
            }
            //-----------------------------------------------
        }
        private void CreerRaccourci(Raccourci pRaccourci)
        {
            //-----------------------------------------------
            // Prepare le dialog
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Choisissez un fichier...";
            dialog.Filter = "Tous les fichiers (*.*)|*.*";
            dialog.RestoreDirectory = true;
            dialog.CheckFileExists = true;
            dialog.CheckPathExists = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                // Change les fichiers de config
                IniConfig.WriteKey("Shortcut_" + pRaccourci.Keynum, "Path", dialog.FileName);
                pRaccourci.Path = dialog.FileName;

                // Efface et creer les bons toolitem
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
            // Si probleme de privilege
            try { Process.Start("explorer.exe", pRaccourci.Path); }
            catch { }
            //-----------------------------------------------
        }
        private void SupprimerRaccourci(Raccourci pRaccourci)
        {
            //-----------------------------------------------
            // Change les fichiers de config
            IniConfig.DeleteKey("Shortcut_" + pRaccourci.Keynum, "Path");
            pRaccourci.Path = "";

            // Efface et creer les bons toolitem
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
            OuvrirFormulaire(new MenuInfos());
            //-----------------------------------------------
        }
        private void OpenRegedit()
        {
            //-----------------------------------------------
            // Si probleme de privilege
            try { Process.Start("regedit.exe"); }
            catch { }
            //-----------------------------------------------
        }
        private void OpenTaskMgr()
        {
            //-----------------------------------------------
            // Si probleme de privilege
            try { Process.Start("taskmgr.exe"); }
            catch { }
            //-----------------------------------------------
        }
        private void RunAsNormal()
        {
            //-----------------------------------------------
            if (Message_Continue("Ceci va nécessiter un redémarrage. Continuer ?"))
            {
                // Si probleme de privilege
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
                    // Lance en admin
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
            OuvrirFormulaire(new MenuProfil());
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

            OuvrirFormulaire(new MenuProfil(CurrentProfil));
            //-----------------------------------------------
        }
        private void AjouterFichierInstance()
        {
            //-----------------------------------------------
            // Prepare le dialog
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Choisissez un fichier à ajouter...";
            dialog.Filter = "Tous les fichiers (*.*)|*.*";
            dialog.RestoreDirectory = true;
            dialog.CheckFileExists = true;
            dialog.CheckPathExists = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                // Envoi les touche a la console
                SetForegroundWindow(CurrentInstance.Proc.MainWindowHandle);
                SendKeys.Send(dialog.FileName);
            }

            dialog.Dispose();
            //-----------------------------------------------
        }
        private void AjouterDossierInstance()
        {
            //-----------------------------------------------
            // Prepare le dialog
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "Choisissez un dossier à ajouter...";
            dialog.ShowNewFolderButton = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                // Envoi les touche a la console
                SetForegroundWindow(CurrentInstance.Proc.MainWindowHandle);
                SendKeys.Send(dialog.SelectedPath);
            }

            dialog.Dispose();
            //-----------------------------------------------
        }
        private void OuvrirFormulaire(Form pForm)
        {
            //-----------------------------------------------
            pForm.Owner = this;
            pForm.ShowDialog();
            pForm.Dispose();
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
            // Gere les raccourcis clavier
            if (GetForegroundWindow() != this.Handle 
                && GetForegroundWindow() != CurrentInstance.Proc.MainWindowHandle) { return; }

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
