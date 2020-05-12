using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;


namespace Winuntu
{
    public partial class MenuProfil : Form
    {
        #region Déclarations
        //-----------------------------------------------
        Bitmap OriginalCmd;
        Bitmap OriginalPS;

        Profil CurrentProfil;
        //-----------------------------------------------
        #endregion Déclarations



        //=====================================================================
        public MenuProfil(Profil pProfil = null)
        {
            //-----------------------------------------------
            InitializeComponent();
            CurrentProfil = pProfil;
            // Supperpose les images de preview du wallpaper (pour les gifs)
            PicturePreviewCmdWallpaper.Controls.Add(PicturePreviewCmd);
            PicturePreviewPSWallpaper.Controls.Add(PicturePreviewPS);
            //-----------------------------------------------
        }
        //=====================================================================



        //=====================================================================
        private void MenuProfil_Load(object sender, EventArgs e)
        {
            //-----------------------------------------------
            // Clone les images pour le changement d'opacité
            OriginalCmd = (Bitmap)PicturePreviewCmd.Image.Clone();
            OriginalPS = (Bitmap)PicturePreviewPS.Image.Clone();

            // Nouveau profil
            if (CurrentProfil == null)
            {
                this.Text = "Créer un profil";
                ButtonSupprimer.Visible = false;
                ButtonCreer.Text = "Créer";
            }
            // Modifier profil
            else
            {
                // Charge les infos du profil
                TextBoxName.Text = CurrentProfil.Name;
                TextBoxDirectory.Text = CurrentProfil.Directory;
                TextBoxDesc.Text = CurrentProfil.Description;
                TextBoxWallpaper.Text = CurrentProfil.Wallpaper;
                int opacityverif = (int)Math.Round(10.0 / 255.0 * CurrentProfil.Opacity, 0);
                // Si le profil à été modifier
                if (opacityverif > 10) { TrackOpacity.Value = 10; }
                else if (opacityverif < 0) { TrackOpacity.Value = 0; }
                else { TrackOpacity.Value = opacityverif; }
                TextBoxCommandCmd.Text = CurrentProfil.CommandCmd;
                TextBoxCommandPS.Text = CurrentProfil.CommandPS;
            }
            //-----------------------------------------------
        }
        //=====================================================================



        //=====================================================================
        private void PictureOpenWallpaper_Click(object sender, EventArgs e)
        {
            //-----------------------------------------------
            // Prepare le dialog
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Choisissez une image...";
            dialog.Filter = "Tous les fichiers (*.*)|*.*|.GIF (*.gif)|*.gif|.JPG (*.jpg)|*.jpg|.JPEG (*.jpeg)|*.jpeg|.PNG (*.png)|*.png";
            dialog.RestoreDirectory = true;
            dialog.CheckFileExists = true;
            dialog.CheckPathExists = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                TextBoxWallpaper.Text = dialog.FileName;
                TrackOpacity.Value = 7;
            }

            dialog.Dispose();
            //-----------------------------------------------
        }
        //=====================================================================



        //=====================================================================
        private void TextBoxWallpaper_TextChanged(object sender, EventArgs e)
        {
            //-----------------------------------------------
            // Change l'image
            // Si caractere non conforme
            try
            {
                PicturePreviewCmdWallpaper.Image = Image.FromFile(TextBoxWallpaper.Text);
                PicturePreviewPSWallpaper.Image = Image.FromFile(TextBoxWallpaper.Text);
            }
            catch { }
            //-----------------------------------------------
        }
        private void TrackOpacity_ValueChanged(object sender, EventArgs e)
        {
            //-----------------------------------------------
            // Change l'opacite
            ChangerOpacite(OriginalCmd, PicturePreviewCmd);
            ChangerOpacite(OriginalPS, PicturePreviewPS);
            //-----------------------------------------------
        }
        private void ChangerOpacite(Bitmap pImage, PictureBox pBox)
        {
            //-----------------------------------------------
            Bitmap pic = pImage;
            for (int w = 0; w < pic.Width; w++)
            {
                for (int h = 0; h < pic.Height; h++)
                {
                    // Change l'oppacite de chaque pixel
                    Color c = pic.GetPixel(w, h);
                    Color newC = Color.FromArgb(255 / 10 * TrackOpacity.Value, c);
                    pic.SetPixel(w, h, newC);
                }
            }
            pBox.Image = pic;
            //-----------------------------------------------
        }
        //=====================================================================



        //=====================================================================
        private void PictureOpenDirectory_Click(object sender, EventArgs e)
        {
            //-----------------------------------------------
            // Prepare le dialog
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "Choisissez le répertoire de travail...";
            dialog.ShowNewFolderButton = true;

            if (dialog.ShowDialog() == DialogResult.OK) { TextBoxDirectory.Text = dialog.SelectedPath; }

            dialog.Dispose();
            //-----------------------------------------------
        }
        //=====================================================================



        //=====================================================================
        private void ButtonAnnuler_Click(object sender, EventArgs e)
        {
            //-----------------------------------------------
            this.Close();
            //-----------------------------------------------
        }
        private void ButtonCreer_Click(object sender, EventArgs e)
        {
            //-----------------------------------------------
            if (TextBoxName.Text.Replace(" ", "") == "")
            {
                Winuntu.Menu.Message_Error("Aucun nom de profil n'a été sélectionner.");
                return;
            }

            if (CurrentProfil == null) { CreerProfil(); }
            else { ModifierProfil(); }
            this.Close();
            //-----------------------------------------------
        }
        //=====================================================================



        //=====================================================================
        private void ModifierProfil()
        {
            //-----------------------------------------------
            // Met à jour les informations
            Ini ini = new Ini(CurrentProfil.FichierIni);
            ini.WriteKey("Settings", "Name", TextBoxName.Text);
            ini.WriteKey("Settings", "Wallpaper", TextBoxWallpaper.Text);
            ini.WriteKey("Settings", "Description", TextBoxDesc.Text);
            ini.WriteKey("Settings", "Opacity", (255 / 10 * ((TrackBar)TrackOpacity).Value).ToString());
            ini.WriteKey("Settings", "CommandCmd", TextBoxCommandCmd.Text);
            ini.WriteKey("Settings", "CommandPS", TextBoxCommandPS.Text);
            ini.WriteKey("Settings", "Directory", TextBoxDirectory.Text);

            CurrentProfil.Name = TextBoxName.Text;
            CurrentProfil.Wallpaper = TextBoxWallpaper.Text;
            CurrentProfil.Description = TextBoxDesc.Text;
            CurrentProfil.Opacity = (int)Math.Round(255.0 / 10.0 * ((TrackBar)TrackOpacity).Value, 0);
            CurrentProfil.CommandCmd = TextBoxCommandCmd.Text;
            CurrentProfil.CommandPS = TextBoxCommandPS.Text;
            CurrentProfil.Directory = TextBoxDirectory.Text;

            // Actualise l'affichage (fond d'écran)
            ((Menu)Owner).AfficherInstance();
            //-----------------------------------------------
        }
        private void CreerProfil()
        {
            //-----------------------------------------------
            // Cherche un nom de fichier inexistant
            int count = 0;
            while (File.Exists(Winuntu.Menu.FOLDER_PROFILS + @"\" + count + ".ini")) { count++; }
            string file = (Winuntu.Menu.FOLDER_PROFILS + @"\" + count + ".ini");

            // Enregistre le profil
            Ini ini = new Ini(file);
            ini.WriteKey("Settings", "Name", TextBoxName.Text);
            ini.WriteKey("Settings", "Wallpaper", TextBoxWallpaper.Text);
            ini.WriteKey("Settings", "Description", TextBoxDesc.Text);
            ini.WriteKey("Settings", "Opacity", (255 / 10 * ((TrackBar)TrackOpacity).Value).ToString());
            ini.WriteKey("Settings", "CommandCmd", TextBoxCommandCmd.Text);
            ini.WriteKey("Settings", "CommandPS", TextBoxCommandPS.Text);
            ini.WriteKey("Settings", "Directory", TextBoxDirectory.Text);

            ((Menu)Owner).AjouterProfil(
                TextBoxName.Text,
                TextBoxWallpaper.Text,
                TextBoxDesc.Text,
                (255 / 10 * ((TrackBar)TrackOpacity).Value),
                TextBoxCommandCmd.Text,
                TextBoxCommandPS.Text,
                TextBoxDirectory.Text,
                file,
                true);

            ((Menu)Owner).AjouterInstanceCmd();
            ((Menu)Owner).AjouterInstancePS();
            //-----------------------------------------------
        }
        private void ButtonSupprimer_Click(object sender, EventArgs e)
        {
            //-----------------------------------------------
            if (!Winuntu.Menu.Message_Continue("Êtes-vous sûr de vouloir supprimer le profil ?")) { return; }

            ((Menu)Owner).SupprimerProfil(CurrentProfil);
            ((Menu)Owner).AfficherInstance();
            this.Close();
            //-----------------------------------------------
        }
        //=====================================================================
    }
}
