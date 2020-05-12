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
            OriginalCmd = (Bitmap)PicturePreviewCmd.Image.Clone();
            OriginalPS = (Bitmap)PicturePreviewPS.Image.Clone();

            CurrentProfil = pProfil;

            if (pProfil == null)
            {
                this.Text = "Créer un profil";
                ButtonSupprimer.Visible = false;
                ButtonCreer.Text = "Créer";
            }
            else
            {
                TextBoxName.Text = pProfil.Name;
                TextBoxDirectory.Text = pProfil.Directory;
                TextBoxDesc.Text = pProfil.Description;
                TextBoxWallpaper.Text = pProfil.Wallpaper;
                int opacityverif = (int)Math.Round(10.0 / 255.0 * pProfil.Opacity, 0);
                if (opacityverif > 10)
                {
                    TrackOpacity.Value = 10;
                }
                else if (opacityverif < 0)
                {
                    TrackOpacity.Value = 0;
                }
                else
                {
                    TrackOpacity.Value = opacityverif;
                }
                TextBoxCommandCmd.Text = pProfil.CommandCmd;
                TextBoxCommandPS.Text = pProfil.CommandPS;
            }
            //-----------------------------------------------
        }
        //=====================================================================



        //=====================================================================
        private void PictureOpenWallpaper_Click(object sender, EventArgs e)
        {
            //-----------------------------------------------
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
            if (File.Exists(TextBoxWallpaper.Text))
            {
                PanelPreviewCmd.BackgroundImage = Image.FromFile(TextBoxWallpaper.Text);
                PanelPreviewPS.BackgroundImage = Image.FromFile(TextBoxWallpaper.Text);
            }
            //-----------------------------------------------
        }
        private void TrackOpacity_ValueChanged(object sender, EventArgs e)
        {
            //-----------------------------------------------
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
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "Choisissez le répertoire de travail...";
            dialog.ShowNewFolderButton = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                TextBoxDirectory.Text = dialog.SelectedPath;
            }

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
                ((Menu)Owner).Message_Error("Aucun nom de profil n'a été sélectionner.");
                return;
            }

            if (CurrentProfil == null)
            {
                CreerProfil();
            }
            else
            {
                ModifierProfil();
            }

            this.Close();
            //-----------------------------------------------
        }
        //=====================================================================



        //=====================================================================
        private void ModifierProfil()
        {
            //-----------------------------------------------
            Ini ini = new Ini(CurrentProfil.FichierIni);
            ini.WriteKey("Settings", "Name", TextBoxName.Text);
            ini.WriteKey("Settings", "Wallpaper", TextBoxWallpaper.Text);
            ini.WriteKey("Settings", "Description", TextBoxDesc.Text);
            ini.WriteKey("Settings", "Opacity", (255 / 10 * ((TrackBar)TrackOpacity).Value).ToString());
            ini.WriteKey("Settings", "CommandCmd", TextBoxCommandCmd.Text);
            ini.WriteKey("Settings", "CommandPS", TextBoxCommandPS.Text);
            ini.WriteKey("Settings", "Directory", TextBoxDirectory.Text);
            ini = null;

            CurrentProfil.Name = TextBoxName.Text;
            CurrentProfil.Wallpaper = TextBoxWallpaper.Text;
            CurrentProfil.Description = TextBoxDesc.Text;
            CurrentProfil.Opacity = (int)Math.Round(255.0 / 10.0 * ((TrackBar)TrackOpacity).Value, 0);
            CurrentProfil.CommandCmd = TextBoxCommandCmd.Text;
            CurrentProfil.CommandPS = TextBoxCommandPS.Text;
            CurrentProfil.Directory = TextBoxDirectory.Text;

            ((Menu)Owner).AfficherInstance();
            //-----------------------------------------------
        }
        private void CreerProfil()
        {
            //-----------------------------------------------
            int count = 0;
            while (File.Exists(((Menu)Owner).FOLDER_PROFILS + @"\" + count + ".ini"))
            {
                count++;
            }
            string file = ((Menu)Owner).FOLDER_PROFILS + @"\" + count + ".ini";

            Ini ini = new Ini(file);
            ini.WriteKey("Settings", "Name", TextBoxName.Text);
            ini.WriteKey("Settings", "Wallpaper", TextBoxWallpaper.Text);
            ini.WriteKey("Settings", "Description", TextBoxDesc.Text);
            ini.WriteKey("Settings", "Opacity", (255 / 10 * ((TrackBar)TrackOpacity).Value).ToString());
            ini.WriteKey("Settings", "CommandCmd", TextBoxCommandCmd.Text);
            ini.WriteKey("Settings", "CommandPS", TextBoxCommandPS.Text);
            ini.WriteKey("Settings", "Directory", TextBoxDirectory.Text);
            ini = null;

            ((Menu)Owner).AjouterProfil(
                TextBoxName.Text,
                TextBoxWallpaper.Text,
                TextBoxDesc.Text,
                (255 / 10 * ((TrackBar)TrackOpacity).Value),
                TextBoxCommandCmd.Text,
                TextBoxCommandPS.Text,
                TextBoxDirectory.Text,
                file
                );

            ((Menu)Owner).AjouterInstanceCmd();
            ((Menu)Owner).AjouterInstancePS();
            //-----------------------------------------------
        }
        private void ButtonSupprimer_Click(object sender, EventArgs e)
        {
            //-----------------------------------------------
            ((Menu)Owner).SupprimerProfil(CurrentProfil);
            this.Close();
            //-----------------------------------------------
        }

        private void MenuProfil_Load(object sender, EventArgs e)
        {

        }
        //=====================================================================
    }
}
