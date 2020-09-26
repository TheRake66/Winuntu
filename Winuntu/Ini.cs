// ==[INDEX]======================================================================================================================
// Nom             : Ini
// Description     : Gère les fichiers de configuration en .ini
// Langue          : Français
// Créateur(s)     : BUSTOS Thibault (TheRake66)
// Version         : 1.0
// ===============================================================================================================================






// ==[FONCTIONS/PROCÉDURES]=======================================================================================================
// CreateSection
// RenameSection
// DeleteSection
// ReadKey
// WriteKey
// RenameKey
// DeleteKey
// ===============================================================================================================================






// ==[INTERNES]===================================================================================================================
// ===============================================================================================================================






// ==[USINGS]=====================================================================================================================
using System.IO;
using System.Windows.Forms;
// ===============================================================================================================================






using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winuntu
{
    class Ini
    {
        // ==[CONSTANTES]=================================================================================================================
        // ===============================================================================================================================






        // ==[VARIABLES]==================================================================================================================
        string IniFile = "";
        // ===============================================================================================================================





        // ==[FONCTION]===================================================================================================================
        // Fonction        : Constructeur supplémentaire
        // Description     : Defini le fichier .ini
        // Argument(s)     : string this.IniFile = Fichier .ini
        // Créateur(s)     : BUSTOS Thibault(TheRake66)
        // Version         : 1.0
        // ===============================================================================================================================
        public Ini(string pIniFile = null)
        {
            this.IniFile = pIniFile;
        }
        // ===============================================================================================================================





        // ==[FONCTION]===================================================================================================================
        // Fonction        : SetFile
        // Description     : Defini le fichier .ini
        // Argument(s)     : string this.IniFile = Fichier .ini
        // Créateur(s)     : BUSTOS Thibault(TheRake66)
        // Version         : 1.0
        // ===============================================================================================================================
        public void SetFile(string pIniFile)
        {
            this.IniFile = pIniFile;
        }
        // ===============================================================================================================================





        // ==[FONCTION]===================================================================================================================
        // Fonction        : DeleteSection
        // Description     : Supprime une section du fichier .ini
        // Argument(s)     : string pSection = Nom de la section à supprimer
        // Créateur(s)     : BUSTOS Thibault(TheRake66)
        // Version         : 1.0
        // ===============================================================================================================================
        public void DeleteSection(string pSection)
        {
            string[] IniText;
            string IniConvert = "";
            bool IniInSection = false;

            try { IniText = File.ReadAllText(this.IniFile).Split(new char[] { '\n', '\r' }); }
            catch { return; }

            foreach (string IniLine in IniText)
            {
                if (IniLine != "")
                {
                    // Si la section est la bonne
                    if (IniLine == "[" + pSection + "]")
                    {
                        IniInSection = true;
                    }
                    // Si on sort de la section
                    else if (IniLine.First() == '[' && IniLine.Last() == ']')
                    {
                        IniConvert += IniLine + "\n";
                        IniInSection = false;
                    }
                    // Si dans la section et que y'a la bonne clé
                    else if (IniInSection) { }
                    else { IniConvert += IniLine + "\n"; }
                }
            }

            try { File.WriteAllText(this.IniFile, IniConvert); }
            catch { }
        }
        // ===============================================================================================================================






        // ==[FONCTION]===================================================================================================================
        // Fonction        : RenameSection
        // Description     : Renomme une section du fichier .ini
        // Argument(s)     : string pSection = Nom de la section à renommer
        // Créateur(s)     : BUSTOS Thibault(TheRake66)
        // Version         : 1.0
        // ===============================================================================================================================
        public void RenameSection(string pSection, string pNewName)
        {
            string[] IniText;
            string IniConvert = "";

            try { IniText = File.ReadAllText(this.IniFile).Split(new char[] { '\n', '\r' }); }
            catch { return; }

            foreach (string IniLine in IniText)
            {
                if (IniLine != "")
                {
                    // Si la section est la bonne
                    if (IniLine == "[" + pSection + "]")
                    {
                        IniConvert += "[" + pNewName + "]\n";
                    }
                    else { IniConvert += IniLine + "\n"; }
                }
            }

            try { File.WriteAllText(this.IniFile, IniConvert); }
            catch { }
        }
        // =============================================================================================================================== 






        // ==[FONCTION]===================================================================================================================
        // Fonction        : CreateSection
        // Description     : Créer une section du fichier .ini
        // Argument(s)     : string pSection = Nom de la section à créer
        // Créateur(s)     : BUSTOS Thibault(TheRake66)
        // Version         : 1.0
        // ===============================================================================================================================
        public void CreateSection(string pSection)
        {
            string[] IniText;
            string IniConvert = "";

            try
            {
                if (!File.Exists(this.IniFile))
                {
                    FileStream CreateFile = File.Create(this.IniFile);
                    CreateFile.Close();
                }
            }
            catch { }

            try { IniText = File.ReadAllText(this.IniFile).Split(new char[] { '\n', '\r' }); }
            catch { return; }

            foreach (string IniLine in IniText)
            {
                if (IniLine != "")
                {
                    // Si la section est la bonne
                    if (IniLine == "[" + pSection + "]")
                    {
                        return;
                    }
                    IniConvert += IniLine + "\n";
                }
            }

            IniConvert += "[" + pSection + "]\n";

            try { File.WriteAllText(this.IniFile, IniConvert); }
            catch { }
        }
        // ===============================================================================================================================






        // ==[FONCTION]===================================================================================================================
        // Fonction        : ReadKey
        // Description     : Lit une clé d'une section du fichier .ini
        // Argument(s)     : string pSection = Nom de la section ou se trouve la clé
        //                 : string pKey = Nom de la clé à lire
        // Créateur(s)     : BUSTOS Thibault(TheRake66)
        // Version         : 1.0
        // ===============================================================================================================================
        public string ReadKey(string pSection, string pKey)
        {
            string[] IniText;
            bool IniInSection = false;

            try { IniText = File.ReadAllText(this.IniFile).Split(new char[] { '\n', '\r' }); }
            catch { return ""; }


            foreach (string IniLine in IniText)
            {
                if (IniLine != "")
                {

                    // Si la section est la bonne
                    if (IniLine == "[" + pSection + "]")
                    {
                        IniInSection = true;
                    }
                    // Si on sort de la section
                    else if (IniLine.First() == '[' && IniLine.Last() == ']')
                    {
                        IniInSection = false;
                    }
                    // Si dans la section et que y'a la bonne clé
                    else if (IniInSection && IniLine.Length > pKey.Length && IniLine.Substring(0, pKey.Length + 1) == pKey + "=")
                    {
                        return IniLine.Substring(pKey.Length + 1);
                    }
                }
            }

            return "";
        }
        // ===============================================================================================================================






        // ==[FONCTION]===================================================================================================================
        // Fonction        : WriteKey
        // Description     : Écrit une clé d'une section du fichier .ini
        // Argument(s)     : string pSection = Nom de la section ou se trouve la clé
        //                 : string pKey = Nom de la clé à écrire
        //                 : string pValue = Valeur de la clé
        // Créateur(s)     : BUSTOS Thibault(TheRake66)
        // Version         : 1.0
        // ===============================================================================================================================
        public void WriteKey(string pSection, string pKey, string pValue)
        {
            string[] IniText;
            bool IniInSection = false;
            string IniConvert = "";
            bool IniModif = false;
            bool IniFoundSection = false;

            // Créer le fichier si il existe pas
            try
            {
                if (!File.Exists(this.IniFile))
                {
                    FileStream CreateFile = File.Create(this.IniFile);
                    CreateFile.Close();
                }
            }
            catch { return; }

            try { IniText = File.ReadAllText(this.IniFile).Split(new char[] { '\n', '\r' }); }
            catch { return; }

            foreach (string IniLine in IniText)
            {
                if (IniLine != "")
                {
                    // Si la section est la bonne
                    if (IniLine == "[" + pSection + "]")
                    {
                        IniConvert += IniLine + "\n";
                        IniInSection = true;
                        IniFoundSection = true;
                    }
                    // Si on sort de la section
                    else if (IniLine.First() == '[' && IniLine.Last() == ']')
                    {
                        // Si on sort de la section sans avoir ecrit
                        if (IniInSection && !IniModif)
                        {
                            IniConvert += pKey + "=" + pValue + "\n";
                            IniModif = true;
                        }

                        IniConvert += IniLine + "\n";
                        IniInSection = false;
                    }
                    // Si dans la section et que y'a la bonne clé
                    else if (IniInSection && IniLine.Length > pKey.Length && IniLine.Substring(0, pKey.Length + 1) == pKey + "=")
                    {
                        IniConvert += pKey + "=" + pValue + "\n";
                        IniModif = true;
                    }
                    else { IniConvert += IniLine + "\n"; }
                }
            }

            // Si la section n'existe pas
            if (!IniFoundSection)
            {
                IniConvert += "[" + pSection + "]\n";
            }
            // Si la clé existe pas
            if (!IniModif)
            {
                IniConvert += pKey + "=" + pValue + "\n";
            }

            try { File.WriteAllText(this.IniFile, IniConvert); }
            catch { }
        }
        // ===============================================================================================================================






        // ==[FONCTION]===================================================================================================================
        // Fonction        : RenameKey
        // Description     : Renomme une clé d'une section du fichier .ini
        // Argument(s)     : string pSection = Nom de la section ou se trouve la clé
        //                 : string pKey = Nom de la clé à renommer
        //                 : string pNewName = Nouveau nom
        // Créateur(s)     : BUSTOS Thibault(TheRake66)
        // Version         : 1.0
        // ===============================================================================================================================
        public void RenameKey(string pSection, string pKey, string pNewName)
        {
            string[] IniText;
            string IniConvert = "";
            bool IniInSection = false;

            try { IniText = File.ReadAllText(this.IniFile).Split(new char[] { '\n', '\r' }); }
            catch { return; }

            foreach (string IniLine in IniText)
            {
                if (IniLine != "")
                {
                    // Si la section est la bonne
                    if (IniLine == "[" + pSection + "]")
                    {
                        IniConvert += IniLine + "\n";
                        IniInSection = true;
                    }
                    // Si on sort de la section
                    else if (IniLine.First() == '[' && IniLine.Last() == ']')
                    {
                        IniConvert += IniLine + "\n";
                        IniInSection = false;
                    }
                    // Si dans la section et que y'a la bonne clé
                    else if (IniInSection && IniLine.Length > pKey.Length && IniLine.Substring(0, pKey.Length + 1) == pKey + "=")
                    {
                        IniConvert += pNewName + "=" + IniLine.Substring(pKey.Length + 1) + "\n";
                    }
                    else { IniConvert += IniLine + "\n"; }
                }
            }

            try { File.WriteAllText(this.IniFile, IniConvert); }
            catch { }
        }
        // ===============================================================================================================================






        // ==[FONCTION]===================================================================================================================
        // Fonction        : DeleteKey
        // Description     : Supprime une clé d'une section du fichier .ini
        // Argument(s)     : string pSection = Nom de la section ou se trouve la clé
        //                 : string pKey = Nom de la clé à supprimer
        // Créateur(s)     : BUSTOS Thibault(TheRake66)
        // Version         : 1.0
        // ===============================================================================================================================
        public void DeleteKey(string pSection, string pKey)
        {
            string[] IniText;
            bool IniInSection = false;
            string IniConvert = "";

            try { IniText = File.ReadAllText(this.IniFile).Split(new char[] { '\n', '\r' }); }
            catch { return; }

            foreach (string IniLine in IniText)
            {
                if (IniLine != "")
                {
                    // Si la section est la bonne
                    if (IniLine == "[" + pSection + "]")
                    {
                        IniConvert += IniLine + "\n";
                        IniInSection = true;
                    }
                    // Si on sort de la section
                    else if (IniLine.First() == '[' && IniLine.Last() == ']')
                    {
                        IniConvert += IniLine + "\n";
                        IniInSection = false;
                    }
                    // Si dans la section et que y'a la bonne clé
                    else if (IniInSection && IniLine.Length > pKey.Length && IniLine.Substring(0, pKey.Length + 1) == pKey + "=") { }
                    else { IniConvert += IniLine + "\n"; }
                }
            }

            try { File.WriteAllText(this.IniFile, IniConvert); }
            catch { }
        }
        // ===============================================================================================================================
    }
}
