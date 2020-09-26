using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Winuntu
{
    //=====================================================================
    public class Profil
    {
        //-----------------------------------------------
        public string Name;
        public string Directory;
        public string Description;
        public string Wallpaper;
        public int Opacity;
        public string CommandCmd;
        public string CommandPS;
        public string FichierIni;
        public TreeNode ParentNode;
        public ToolStripItem ToolItem;
        public List<Instance> ListeInstances = new List<Instance>();
        //-----------------------------------------------
    }
    //=====================================================================
}
