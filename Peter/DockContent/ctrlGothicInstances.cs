/**************************************************************************************
    Stampfer - Gothic Script Editor
    Copyright (C) 2009 Alexander "Sumpfkrautjunkie" Ruppert

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
**************************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using PeterInterface;
using WeifenLuo.WinFormsUI.Docking;

namespace Peter
{
    public partial class ctrlGothicInstances : DockContent, IPeterPluginTab
    {
        private readonly MainForm mainForm;
        private readonly Regex NoSpaces = new Regex(@"  ");
        private readonly bool m_CanScroll;

        private readonly string ScriptsPath;

        private readonly TreeNode DialogTree = new TreeNode("Dialoge");
        private readonly TreeNode NPCTree = new TreeNode("NPCs");
        private readonly TreeNode ItemTree = new TreeNode("Items");

        private readonly TreeNode FuncTree = new TreeNode("Funktionen");
        private readonly TreeNode VarTree = new TreeNode("Variablen");
        public TreeNode ConstTree = new TreeNode("Konstanten");

        public Dictionary<string, Instance> FuncList = new Dictionary<string, Instance>();
        public Dictionary<string, Instance> VarList = new Dictionary<string, Instance>();
        public Dictionary<string, Instance> ConstList = new Dictionary<string, Instance>();
        public Dictionary<string, Instance> ItemList = new Dictionary<string, Instance>();
        public Dictionary<string, Instance> NPCList = new Dictionary<string, Instance>();
        public Dictionary<string, Instance> DialogList = new Dictionary<string, Instance>();
        private const int DIALOGIMG = 0;
        private const int NPCIMG = 1;
        private const int ITEMIMG = 2;
        private const int FUNCIMG = 3;
        private const int VARIMG = 4;
        private const int CONSTIMG = 5;

        public ctrlGothicInstances(MainForm m)
        {

            this.mainForm = m;
            InitializeComponent();
            this.ScriptsPath = m.m_ScriptsPath;
            this.m_CanScroll = true;
            this.treeMain.ImageList = m.ImgList;
            this.TabText = "Gothic Bezeichner";
            this.treeMain.NodeMouseDoubleClick += new TreeNodeMouseClickEventHandler(treeMain_NodeMouseDoubleClick);
            if (this.ScriptsPath != "")
            {
                this.treeMain.BeginUpdate();
                ReadInstancesFromFile();
                this.treeMain.EndUpdate();
            }

            DialogTree.ImageIndex = DIALOGIMG;
            DialogTree.SelectedImageIndex = DialogTree.ImageIndex;

            NPCTree.ImageIndex = NPCIMG;
            NPCTree.SelectedImageIndex = NPCTree.ImageIndex;

            ItemTree.ImageIndex = ITEMIMG;
            ItemTree.SelectedImageIndex = ItemTree.ImageIndex;

            FuncTree.ImageIndex = FUNCIMG;
            FuncTree.SelectedImageIndex = FuncTree.ImageIndex;

            VarTree.ImageIndex = VARIMG;
            VarTree.SelectedImageIndex = VarTree.ImageIndex;

            ConstTree.ImageIndex = CONSTIMG;
            ConstTree.SelectedImageIndex = ConstTree.ImageIndex;

            DialogTree.Nodes.Add("fakenode");
            NPCTree.Nodes.Add("fakenode");
            ItemTree.Nodes.Add("fakenode");
            FuncTree.Nodes.Add("fakenode");
            VarTree.Nodes.Add("fakenode");
            ConstTree.Nodes.Add("fakenode");
            treeMain.Nodes.Add(DialogTree);
            treeMain.Nodes.Add(NPCTree);
            treeMain.Nodes.Add(ItemTree);
            treeMain.Nodes.Add(FuncTree);
            treeMain.Nodes.Add(VarTree);
            treeMain.Nodes.Add(ConstTree);
            treeMain.BeforeExpand += new TreeViewCancelEventHandler(treeMain_BeforeExpand);
            treeMain.BeforeCollapse += new TreeViewCancelEventHandler(treeMain_BeforeCollapse);
        }

        private void treeMain_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            treeMain.BeginUpdate();
            if (e.Node.ImageIndex == DIALOGIMG)//Dialogtree
            {
                DialogTree.Nodes.Clear();
                DialogTree.Nodes.Add("fakenode");
            }
            else if (e.Node.ImageIndex == NPCIMG)//Dialogtree
            {
                NPCTree.Nodes.Clear();
                NPCTree.Nodes.Add("fakenode");
            }
            else if (e.Node.ImageIndex == ITEMIMG)//Dialogtree
            {
                ItemTree.Nodes.Clear();
                ItemTree.Nodes.Add("fakenode");
            }
            else if (e.Node.ImageIndex == FUNCIMG)//Dialogtree
            {
                FuncTree.Nodes.Clear();
                FuncTree.Nodes.Add("fakenode");
            }
            else if (e.Node.ImageIndex == VARIMG)//Dialogtree
            {
                VarTree.Nodes.Clear();
                VarTree.Nodes.Add("fakenode");
            }
            else if (e.Node.ImageIndex == CONSTIMG)//Dialogtree
            {
                ConstTree.Nodes.Clear();
                ConstTree.Nodes.Add("fakenode");
            }

            treeMain.EndUpdate();
        }

        private void treeMain_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            treeMain.BeginUpdate();
            if (e.Node.ImageIndex == DIALOGIMG)//Dialogtree
            {
                if (DialogList.Count == 0) { treeMain.EndUpdate(); return; }
                UpdateDialogTree();
            }
            else if (e.Node.ImageIndex == NPCIMG)//Dialogtree
            {
                if (NPCList.Count == 0) { treeMain.EndUpdate(); return; }
                UpdateNPCTree();
            }
            else if (e.Node.ImageIndex == ITEMIMG)//Dialogtree
            {
                if (ItemList.Count == 0) { treeMain.EndUpdate(); return; }
                UpdateItemTree();
            }
            else if (e.Node.ImageIndex == FUNCIMG)//Dialogtree
            {
                if (FuncList.Count == 0) { treeMain.EndUpdate(); return; }
                UpdateFuncTree();
            }
            else if (e.Node.ImageIndex == VARIMG)//Dialogtree
            {
                if (VarList.Count == 0) { treeMain.EndUpdate(); return; }
                UpdateVarTree();
            }
            else if (e.Node.ImageIndex == CONSTIMG)//Dialogtree
            {
                if (ConstList.Count == 0) { treeMain.EndUpdate(); return; }
                UpdateConstTree();
            }
            treeMain.EndUpdate();
        }

        #region -= Helpers =-

        /// <summary>
        /// After an item is selected, scroll to it...
        /// </summary>
        /// <param name="sender">TreeView</param>
        /// <param name="e">TreeViewEventArgs</param>

        private void treeMain_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (this.m_CanScroll)
            {
                if (e.Node.Parent != null)
                {
                    if (e.Node.Tag != null)
                    {
                        var file = e.Node.Tag.ToString();
                        OpenFile(file, e.Node.Text);
                    }
                }
            }

        }
        public void OpenFile(string file, string txt)
        {
            if (!File.Exists(file))
            {
                return;
            }

            var searchstring = "";

            this.mainForm.CreateEditor(file, Path.GetFileName(file));
            foreach (var c in txt)
            {
                if (c == '=' || c == '(')
                {
                    break;
                }
                searchstring += c.ToString();
            }
            if (txt.StartsWith("void", StringComparison.OrdinalIgnoreCase))
            {
                this.mainForm.FindText(new Regex(@"void(\s)*" + RemoveType(ref searchstring, ref file), RegexOptions.IgnoreCase), true);
            }
            else if (txt.StartsWith("int", StringComparison.OrdinalIgnoreCase))
            {
                this.mainForm.FindText(new Regex(@"int(\s)*" + RemoveType(ref searchstring, ref file), RegexOptions.IgnoreCase), true);
            }
            else if (txt.StartsWith("string", StringComparison.OrdinalIgnoreCase))
            {
                this.mainForm.FindText(new Regex(@"string(\s)*" + RemoveType(ref searchstring, ref file), RegexOptions.IgnoreCase), true);
            }
            else if (txt.StartsWith("c_npc", StringComparison.OrdinalIgnoreCase))
            {
                this.mainForm.FindText(new Regex(@"c_npc(\s)*" + RemoveType(ref searchstring, ref file), RegexOptions.IgnoreCase), true);
            }
            else if (txt.StartsWith("c_item", StringComparison.OrdinalIgnoreCase))
            {
                this.mainForm.FindText(new Regex(@"c_item(\s)*" + RemoveType(ref searchstring, ref file), RegexOptions.IgnoreCase), true);
            }
            else if (txt.StartsWith("float", StringComparison.OrdinalIgnoreCase))
            {
                this.mainForm.FindText(new Regex(@"float(\s)*" + RemoveType(ref searchstring, ref file), RegexOptions.IgnoreCase), true);
            }
            else
            {
                this.mainForm.FindText(new Regex(@"\s" + RemoveType(ref searchstring, ref file), RegexOptions.IgnoreCase), true);
            }
        }

        #endregion

        #region IPeterPluginTab Members

        public void Save()
        {
        }

        public void SaveAs(string filePath)
        {
        }

        public void Cut()
        {
        }

        public void Copy()
        {
        }

        public void Paste()
        {
        }

        public void Undo()
        {
        }

        public void Redo()
        {
        }

        public void Delete()
        {
        }

        public void Duplicate()
        {
        }

        public void Print()
        {
        }

        public void SelectAll()
        {
        }

        public bool CloseTab()
        {
            this.Close();
            return true;
        }

        public void MarkAll(System.Text.RegularExpressions.Regex reg)
        {
        }

        public bool FindNext(System.Text.RegularExpressions.Regex reg, bool searchUp)
        {
            return false;
        }

        public void ReplaceNext(System.Text.RegularExpressions.Regex reg, string replaceWith, bool searchUp)
        {
        }

        public void ReplaceAll(System.Text.RegularExpressions.Regex reg, string replaceWith)
        {
        }

        public void SelectWord(int line, int offset, int wordLeng)
        {
        }

        public IPeterPluginHost Host { get; set; }

        public string FileName
        {
            get { return ""; }
        }

        public string Selection
        {
            get { return ""; }
        }

        public bool AbleToUndo
        {
            get { return false; }
        }

        public bool AbleToRedo
        {
            get { return false; }
        }

        public bool NeedsSaving
        {
            get { return false; }
        }

        public bool AbleToPaste
        {
            get { return false; }
        }

        public bool AbleToCut
        {
            get { return false; }
        }

        public bool AbleToCopy
        {
            get { return false; }
        }

        public bool AbleToSelectAll
        {
            get { return false; }
        }

        public bool AbleToSave
        {
            get { return false; }
        }

        public bool AbleToDelete
        {
            get { return false; }
        }

        #endregion

        #region -= Tool Bar =-

        private void tsbExpandAll_Click(object sender, EventArgs e)
        {
            this.treeMain.BeginUpdate();
            for (var a = 0; a < this.treeMain.Nodes.Count; a++)
            {
                this.treeMain.Nodes[a].ExpandAll();
            }
            this.treeMain.EndUpdate();
        }

        private void tsbCollapseAll_Click(object sender, EventArgs e)
        {
            this.treeMain.BeginUpdate();
            for (var a = 0; a < this.treeMain.Nodes.Count; a++)
            {
                this.treeMain.Nodes[a].Collapse();
            }
            this.treeMain.EndUpdate();
        }

        #endregion

        private void ClearArrays()
        {
            this.DialogTree.Nodes.Clear();
            this.NPCTree.Nodes.Clear();
            this.ItemTree.Nodes.Clear();
            this.FuncTree.Nodes.Clear();
            this.VarTree.Nodes.Clear();
            this.ConstTree.Nodes.Clear();
            ItemList.Clear();
            DialogList.Clear();
            NPCList.Clear();
            FuncList.Clear();
            VarList.Clear();
            ConstList.Clear();
        }

        private static readonly Regex rxInstance = new Regex(@"((^)|(\s))instance ", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex rxFunc = new Regex(@"((^)|(\s))func ", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex rxVar = new Regex(@"((^)|(\s))var ", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex rxConst = new Regex(@"((^)|(\s))const ", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private const int SB_LENGTH = 256;

        private int GetAndAddInstances(string path, Func<string, string, int> onMatch)
        {
            var s = File.ReadAllText(path, Encoding.Default);
            var m = rxInstance.Matches(s);
            var sb1 = new StringBuilder(SB_LENGTH);
            var i = 0;
            var k = 0;
            foreach (Match match in m)
            {
                i = match.Index + match.Length;
                while ((i < s.Length) && (s[i] != '(') && (s[i] != ' ') && (s[i] != '\t'))
                {
                    sb1.Append(s[i]);
                    i++;
                }
                k |= onMatch(sb1.ToString(), path);
                sb1.Clear();
            }
            return k;
        }
        public int GetItems(string path) => GetAndAddInstances(path, AddItem);
        public int AddItem(string value, string path)
        {
            if (value.Length > 0 && !ItemList.ContainsKey(value))
            {
                ItemList.Add(value, new Instance(value, path));
                return 32;
            }
            return 0;
        }
        public int GetNPCs(string path) => GetAndAddInstances(path, AddNPC);
        public int AddNPC(string value, string path)
        {
            if (value.Length > 0 && !NPCList.ContainsKey(value))
            {
                NPCList.Add(value, new Instance(value, path));
                return 16;
            }
            return 0;
        }
        public int GetDias(string path) => GetAndAddInstances(path, AddDia);
        public int AddDia(string value, string path)
        {
            if (value.Length > 0 && !DialogList.ContainsKey(value))
            {
                DialogList.Add(value, new Instance(value, path));
                return 8;
            }
            return 0;
        }
        public int GetFuncs(string path)
        {
            var externals = Path.GetFileName(path).ToLower() == "externals.d";
            var s = File.ReadAllText(path, Encoding.Default);
            var k = 0;
            int i;

            var sb1 = new StringBuilder(SB_LENGTH * 2);
            foreach (Match match in rxFunc.Matches(s))
            {
                i = match.Index + match.Length;
                while (s[i] != '{' && s[i] != '\n' && s[i] != '/')
                {
                    if (s[i] == '(')
                    {
                        sb1.Append(" " + s[i]);
                        i++;
                        continue;
                    }
                    if (s[i] == ',')
                    {
                        sb1.Append(s[i] + " ");
                        i++;
                        continue;
                    }

                    sb1.Append(s[i]);
                    if (s[i] == ')')
                        break;
                    i++;
                }
                k |= AddFunc(sb1.ToString(), path);
                sb1.Clear();
            }

            if (externals) return k;

            foreach (Match match in rxVar.Matches(s))
            {
                i = match.Index + match.Length;
                while (s[i] != '\n' && s[i] != ')')
                {
                    if (s[i] == ';')
                        break;
                    sb1.Append(s[i]);
                    i++;
                }
                k |= AddVar(sb1.ToString(), path);
                sb1.Clear();
            }

            foreach (Match match in rxConst.Matches(s))
            {
                i = match.Index + match.Length;
                while (s[i] != ';' && s[i] != '\n')
                {
                    if (s[i] == '=')
                    {
                        sb1.Append(" " + s[i] + " ");
                        i++;
                        continue;
                    }
                    sb1.Append(s[i]);
                    i++;
                }
                k |= AddConst(sb1.ToString(), path);
                sb1.Clear();
            }
            return k;
        }

        public int AddFunc(string sb1, string path)
        {
            if (sb1.Length > 0)
            {
                var tempstring = sb1.Trim().Replace('\t', ' ');
                string tempstring2;
                if (tempstring.Length == 0) return 0;
                tempstring = RemoveDoubleSpaces(tempstring);
                var y = tempstring.IndexOf(" ");
                if (y > 0)
                {
                    var temp = tempstring.Substring(0, y);
                    tempstring = temp.ToLower() + tempstring.Substring(y);
                }
                tempstring2 = tempstring.ToLower();
                if (tempstring2.StartsWith("void") || tempstring2.StartsWith("int") || tempstring2.StartsWith("c_npc") || tempstring2.StartsWith("c_item") || tempstring2.StartsWith("string"))
                {
                    if (!FuncList.ContainsKey(tempstring))
                    {
                        FuncList.Add(tempstring, new Instance(tempstring, path));
                        return 4;
                    }
                }
            }
            return 0;
        }
        public int AddVar(string sb1, string path)
        {
            if (sb1.Length > 0)
            {
                var tempstring = sb1.Trim().Replace('\t', ' ');
                string temp;
                if (tempstring.Length == 0) return 0;
                tempstring = RemoveDoubleSpaces(tempstring);
                var y = tempstring.IndexOf(" ");
                if (y > 0)
                {
                    temp = tempstring.Substring(0, y);
                    tempstring = temp.ToLower() + tempstring.Substring(y);
                }
                if (!VarList.ContainsKey(tempstring))
                {
                    VarList.Add(tempstring, new Instance(tempstring, path));
                    return 2;
                }
            }
            return 0;
        }
        public int AddConst(string sb1, string path)
        {
            if (sb1.Length > 0)
            {
                var tempstring = sb1.Trim().Replace('\t', ' ');
                string temp;
                if (tempstring.Length == 0) return 0;
                tempstring = RemoveDoubleSpaces(tempstring);
                var y = tempstring.IndexOf(" ");
                if (y > 0)
                {
                    temp = tempstring.Substring(0, y);
                    tempstring = temp.ToLower() + tempstring.Substring(y);
                }
                if (!ConstList.ContainsKey(tempstring))
                {
                    ConstList.Add(tempstring, new Instance(tempstring, path));
                    return 1;
                }
            }
            return 0;
        }
        private void GetInstancesToFile(bool dialoge, bool npcs, bool items, bool functions)
        {
            ClearTree(dialoge, npcs, items, functions);

            this.mainForm.Trace("Gothic-Bezeichner werden aktualisert (kann einige Sekunden dauern).");
            var handlers = new Dictionary<string, Func<string, int>>();

            if (items) handlers[FilePaths.ContentItems] = GetItems;
            if (npcs) handlers[FilePaths.ContentNPC] = GetNPCs;
            if (dialoge) handlers[FilePaths.ContentDialoge] = GetDias;
            if (functions) handlers[FilePaths.ContentDir] = GetFuncs;

            Task.Run(() =>
            {
                foreach (var kvp in handlers)
                {
                    var dir = Path.Combine(this.ScriptsPath, kvp.Key);
                    foreach (var file in Directory.GetFiles(dir, "*.d", SearchOption.AllDirectories))
                    {
                        // execute handler
                        kvp.Value(file);
                    }
                }
                this.mainForm.Trace("Gothic-Bezeichner wurden erfolgreich aktualisert.");
            }).ContinueWith(t =>
            {
                SetAutoCompleteContent();
            });
        }

        private void ClearTree(bool dialoge, bool npcs, bool items, bool functions)
        {
            if (dialoge)
            {
                DialogTree.Collapse();
                DialogList.Clear();
            }

            if (npcs)
            {
                NPCTree.Collapse();
                NPCList.Clear();
            }

            if (items)
            {
                ItemTree.Collapse();
                ItemList.Clear();
            }

            if (functions)
            {
                FuncTree.Collapse();
                VarTree.Collapse();
                ConstTree.Collapse();
                FuncList.Clear();
                VarList.Clear();
                ConstList.Clear();
            }
        }

        private void UpdateConstTree()
        {
            var t = new List<Instance>();
            t.AddRange(ConstList.Values);
            t.Sort();
            this.ConstTree.Nodes.Clear();
            foreach (var sl in t)
            {
                var node = new TreeNode(sl.ToString())
                {
                    ImageIndex = CONSTIMG
                };
                node.SelectedImageIndex = node.ImageIndex;
                node.Tag = sl.File;
                this.ConstTree.Nodes.Add(node);
            }
            t.Clear();
        }
        private void UpdateVarTree()
        {
            var t = new List<Instance>();
            t.AddRange(VarList.Values);
            t.Sort();
            this.VarTree.Nodes.Clear();
            foreach (var sl in t)
            {
                var node = new TreeNode(sl.ToString())
                {
                    ImageIndex = VARIMG
                };
                node.SelectedImageIndex = node.ImageIndex;
                node.Tag = sl.File;
                this.VarTree.Nodes.Add(node);
            }
            t.Clear();
        }
        private void UpdateFuncTree()
        {
            var t = new List<Instance>();
            t.AddRange(FuncList.Values);
            t.Sort();
            this.FuncTree.Nodes.Clear();
            foreach (var sl in t)
            {
                var node = new TreeNode(sl.ToString())
                {
                    ImageIndex = FUNCIMG
                };
                node.SelectedImageIndex = node.ImageIndex;
                node.Tag = sl.File;
                this.FuncTree.Nodes.Add(node);
            }
            t.Clear();
        }
        private void UpdateItemTree()
        {
            var t = new List<Instance>();
            t.AddRange(ItemList.Values);
            t.Sort();
            this.ItemTree.Nodes.Clear();
            foreach (var sl in t)
            {
                var node = new TreeNode(sl.ToString())
                {
                    ImageIndex = ITEMIMG
                };
                node.SelectedImageIndex = node.ImageIndex;
                node.Tag = sl.File;
                this.ItemTree.Nodes.Add(node);
            }
            t.Clear();
        }
        private void UpdateNPCTree()
        {
            var t = new List<Instance>();
            t.AddRange(NPCList.Values);
            t.Sort();
            this.NPCTree.Nodes.Clear();
            foreach (var sl in t)
            {
                var node = new TreeNode(sl.ToString())
                {
                    ImageIndex = NPCIMG
                };
                node.SelectedImageIndex = node.ImageIndex;
                node.Tag = sl.File;
                this.NPCTree.Nodes.Add(node);
            }
            t.Clear();
        }
        private void UpdateDialogTree()
        {
            var t = new List<Instance>();
            t.AddRange(DialogList.Values);
            t.Sort();
            this.DialogTree.Nodes.Clear();
            foreach (var sl in t)
            {
                var node = new TreeNode(sl.ToString())
                {
                    ImageIndex = DIALOGIMG
                };
                node.SelectedImageIndex = node.ImageIndex;
                node.Tag = sl.File;
                this.DialogTree.Nodes.Add(node);
            }
            t.Clear();
        }

        private void ReadInstancesFromFile()
        {
            ClearArrays();
            var handlers = new Dictionary<string, Action<string, Instance>>
            {
                { FilePaths.DIALOGE, DialogList.Add },
                { FilePaths.NPCS, NPCList.Add },
                { FilePaths.ITEMS, ItemList.Add },
                { FilePaths.FUNC, FuncList.Add },
                { FilePaths.VARS, VarList.Add },
                { FilePaths.CONSTS, ConstList.Add },
            };
            if (handlers.Keys.All(path => File.Exists(Path.Combine(this.ScriptsPath, path))))
            {
                this.mainForm.Trace("Gothic-Bezeichner werden ausgelesen.");
                foreach (var kvp in handlers)
                {
                    var filePath = Path.Combine(this.ScriptsPath, kvp.Key);
                    using (var sr = new StreamReader(filePath, Encoding.Default))
                    {
                        while (sr.ReadLine() is string line)
                        {
                            var line2 = sr.ReadLine();
                            kvp.Value(line, new Instance(line, line2));
                        }
                    }
                }
                this.mainForm.Trace("Gothic-Bezeichner erfolgreich ausgelesen.");
            }
            else
            {
                GetInstancesToFile(true, true, true, true);
            }
            SetAutoCompleteContent();
        }
        public string RemoveDoubleSpaces(string s)
        {
            var sb = new StringBuilder(s.Length);
            var spaceCount = 0;
            for (var i = 0; i < s.Length; i++)
            {
                if (s[i] == ' ')
                {
                    spaceCount++;
                    if (spaceCount > 1) continue;
                }
                else
                {
                    spaceCount = 0;
                }
                sb.Append(s[i]);
            }
            return sb.ToString();
        }

        public void SetAutoCompleteContent()
        {
            if (this.mainForm.m_AutoComplete != null)
            {
                if (this.mainForm.m_AutoComplete.Extension == ".d")
                {
                    this.mainForm.m_AutoComplete.KeyWordsList.Clear();
                    string s1, s2 = "";

                    foreach (var i in VarList.Values)
                    {
                        s1 = i.Name;
                        ConvertVarForAutoComplete(ref s1, ref s2);
                        var k = new Classes.KeyWord(s1, 4, s2, "Variable");

                        if (this.mainForm.m_AutoComplete.KeyWordsList.BinarySearch(k) >= 0)
                        {
                            continue;
                        }
                        this.mainForm.m_AutoComplete.KeyWordsList.Add(k);
                    }

                    foreach (var i in FuncList.Values)
                    {
                        s1 = i.Name;
                        ConvertFuncForAutoComplete(ref s1, ref s2);
                        var k = new Classes.KeyWord(s1, 3, s2, " ");
                        this.mainForm.m_AutoComplete.KeyWordsList.Add(k);
                    }

                    foreach (var i in ConstList.Values)
                    {
                        s1 = i.Name;
                        ConvertConstForAutoComplete(ref s1, ref s2);

                        var k = new Classes.KeyWord(s1, 5, s2, "Konstante");
                        if (this.mainForm.m_AutoComplete.KeyWordsList.BinarySearch(k) >= 0)
                        {
                            continue;
                        }
                        this.mainForm.m_AutoComplete.KeyWordsList.Add(k);
                    }
                    foreach (var i in DialogList.Values)
                    {
                        var k = new Classes.KeyWord(i.Name, 0, "Dialog", " ");
                        this.mainForm.m_AutoComplete.KeyWordsList.Add(k);
                    }

                    foreach (var i in ItemList.Values)
                    {
                        var k = new Classes.KeyWord(i.Name, 2, "Item", " ");
                        this.mainForm.m_AutoComplete.KeyWordsList.Add(k);
                    }

                    foreach (var i in NPCList.Values)
                    {
                        var k = new Classes.KeyWord(i.Name, 1, "NPC", " ");
                        this.mainForm.m_AutoComplete.KeyWordsList.Add(k);
                    }

                    this.mainForm.m_AutoComplete.KeyWordsList.AddRange(this.mainForm.m_AutoComplete.Properties);
                    this.mainForm.m_AutoComplete.KeyWordsList.AddRange(this.mainForm.m_AutoComplete.ShortFuncs);
                    this.mainForm.m_AutoComplete.KeyWordsList.Sort();
                }
            }
        }
        private string RemoveType(ref string ts, ref string t)
        {
            var y = ts.IndexOf(" ");
            if (y > 0)
            {
                t = ts.Substring(0, y);
                ts = ts.Substring(y + 1);
            }
            return ts;
        }
        public void ConvertFuncForAutoComplete(ref string s1, ref string s2)
        {
            if (!s1.Contains("("))
            {
                s1 = RemoveType(ref s1, ref s2);
            }
            else
            {
                var sa = s1.Split('(');
                if (sa.Length > 1)
                {
                    sa[1] = "(" + sa[1];
                    s1 = RemoveType(ref sa[0], ref s2);
                    s2 = s2 + " " + sa[1];
                }
            }
        }
        public void ConvertVarForAutoComplete(ref string s1, ref string s2)
        {
            RemoveType(ref s1, ref s2);
        }
        public void ConvertConstForAutoComplete(ref string s1, ref string s2)
        {
            if (!s1.Contains('='))
            {
                s1 = RemoveType(ref s1, ref s2);
            }
            else
            {
                var sa = s1.Split('=');
                if (sa.Length > 1)
                {
                    sa[1] = "=" + sa[1];
                    s1 = RemoveType(ref sa[0], ref s2).Trim();
                    s2 = s2 + " " + sa[1];
                }
            }
        }

        private readonly ArrayList TreeMatches = new ArrayList();
        private int currentMatch = 0;
        private void TxtSuchString_TextChanged(object sender, EventArgs e)
        {
            FindInTree(false);
        }

        private Regex rg;
        private void FindInTreeSub(TreeNode i, string Temp, bool mode)
        {
            if (mode == false)
            {
                if (i.Text.ToLower().Contains(Temp))
                {
                    var lokIndex = GetNodeIndex(i);
                    i.StateImageKey = lokIndex.ToString();
                    TreeMatches.Add(i);
                    i.BackColor = Color.Yellow;
                }
            }
            else if (rg != null)
            {
                if (rg.IsMatch(i.Text.ToLower()))
                {
                    var lokIndex = GetNodeIndex(i);
                    i.StateImageKey = lokIndex.ToString();
                    TreeMatches.Add(i);
                    i.BackColor = Color.Yellow;
                }
            }
        }
        private int GetNodeIndex(TreeNode node)
        {
            var tempnode = node.Parent;
            var lokIndex = node.Index;
            while (tempnode.PrevNode != null)
            {
                lokIndex += tempnode.PrevNode.Nodes.Count;
                tempnode = tempnode.PrevNode;
            }
            return lokIndex;
        }
        private void FindInTree(bool mode)
        {
            foreach (TreeNode k in TreeMatches)
            {
                k.BackColor = treeMain.BackColor;
            }
            TreeMatches.Clear();
            currentMatch = 0;
            if (mode == false && TxtSuchString.Text.Length < 3)
            {
                LbFound.Text = "0";
                return;
            }
            var Temp = TxtSuchString.Text.ToLower();
            rg = null;
            try
            {
                rg = new Regex(Temp);
            }
            catch
            {
            }
            var trees = new[] { ItemTree, DialogTree, NPCTree, FuncTree, VarTree, ConstTree };
            for (var i = 0; i < trees.Length; i++)
            {
                if (trees[i].IsExpanded)
                {
                    foreach (TreeNode n in trees[i].Nodes)
                    {
                        FindInTreeSub(n, Temp, mode);
                    }
                }
            }
            if (TreeMatches.Count > 0)
            {
                treeMain.SelectedNode = (TreeNode)TreeMatches[0];
                TxtFoundIndex.Text = "1";
            }
            else
            {
                TxtFoundIndex.Text = "";
            }
            LbFound.Text = TreeMatches.Count.ToString();
        }

        private void treeMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && treeMain.SelectedNode != null)
            {
                treeMain_NodeMouseDoubleClick(null, new TreeNodeMouseClickEventArgs(treeMain.SelectedNode, MouseButtons.Left, 0, 0, 0));
            }
        }

        private void treeMain_AfterSelect(object sender, TreeViewEventArgs e)
        {
            e.Node.EnsureVisible();
        }

        private void BtLeft_Click(object sender, EventArgs e)
        {
            if (TreeMatches.Count > 0)
            {
                var selectedNode = treeMain.SelectedNode;
                if (selectedNode != null)
                {
                    var globIndex = GetNodeIndex(selectedNode);
                    int z;
                    if (globIndex > Convert.ToInt32(((TreeNode)TreeMatches[currentMatch]).StateImageKey))
                    {
                        z = TreeMatches.Count - 1;
                    }
                    else
                    {
                        z = currentMatch;
                    }
                    for (var i = z; i > -1; i--)
                    {
                        if (globIndex > Convert.ToInt32(((TreeNode)TreeMatches[i]).StateImageKey))
                        {
                            currentMatch = i;
                            break;
                        }
                        else
                        {
                            currentMatch = TreeMatches.Count - 1;
                        }
                    }
                }
                TxtFoundIndex.Text = (currentMatch + 1).ToString();
                treeMain.SelectedNode = (TreeNode)TreeMatches[currentMatch];
                treeMain.Focus();
            }
        }

        private void BtRight_Click(object sender, EventArgs e)
        {
            if (TreeMatches.Count > 0)
            {
                var selectedNode = treeMain.SelectedNode;
                if (selectedNode != null)
                {
                    var globIndex = GetNodeIndex(selectedNode);

                    int z;
                    if (globIndex < Convert.ToInt32(((TreeNode)TreeMatches[currentMatch]).StateImageKey))
                    {
                        z = 0;
                    }
                    else
                    {
                        z = currentMatch;
                    }
                    for (var i = z; i < TreeMatches.Count; i++)
                    {

                        if (globIndex < Convert.ToInt32(((TreeNode)TreeMatches[i]).StateImageKey))
                        {
                            currentMatch = i;
                            break;
                        }
                        else
                        {
                            currentMatch = 0;
                        }
                    }
                }
                TxtFoundIndex.Text = (currentMatch + 1).ToString();
                treeMain.SelectedNode = (TreeNode)TreeMatches[currentMatch];
                treeMain.Focus();
            }
        }

        private void TxtFoundIndex_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (TreeMatches.Count > 0)
                {
                    if (!int.TryParse(TxtFoundIndex.Text, out var foundIndex))
                    {
                        return;
                    }
                    if (foundIndex < 1)
                    {
                        foundIndex = 1;
                    }
                    else if (foundIndex > TreeMatches.Count)
                    {
                        foundIndex = TreeMatches.Count;
                    }
                    TxtFoundIndex.Text = foundIndex.ToString();
                    currentMatch = foundIndex - 1;
                    treeMain.SelectedNode = (TreeNode)TreeMatches[currentMatch];
                    treeMain.Focus();
                }
            }
        }

        private void BtCopyTreeItem_Click(object sender, EventArgs e)
        {
            if (treeMain.SelectedNode != null)
            {
                Clipboard.SetText(treeMain.SelectedNode.Text);
            }
        }

        private void treeMain_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (e.Node != null)
                {
                    var s2 = "";
                    var Temp = e.Node.Text;
                    if (e.Node.Parent == FuncTree)
                    {
                        ConvertFuncForAutoComplete(ref Temp, ref s2);
                    }
                    else if (e.Node.Parent == VarTree)
                    {
                        ConvertVarForAutoComplete(ref Temp, ref s2);
                    }
                    else if (e.Node.Parent == ConstTree)
                    {
                        ConvertConstForAutoComplete(ref Temp, ref s2);
                    }
                    Clipboard.SetText(Temp);
                }
            }
        }

        private void TxtSuchString_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                TxtSuchString.Text = Clipboard.GetText();
            }
            if (e.Control && e.Alt && e.KeyCode == Keys.D9)
            {
                TxtSuchString.Text += "]";
            }
            if (e.Control && e.Alt && e.KeyCode == Keys.OemBackslash)
            {
                TxtSuchString.Text += "\\";
            }
        }

        private void mRrefresh_all_Click(object sender, EventArgs e)
        {
            GetInstancesToFile(true, true, true, true);
        }

        private void mRrefresh_dia_Click(object sender, EventArgs e)
        {
            GetInstancesToFile(true, false, false, false);
        }

        private void mRrefresh_npc_Click(object sender, EventArgs e)
        {
            GetInstancesToFile(false, true, false, false);
        }

        private void mRrefresh_items_Click(object sender, EventArgs e)
        {
            GetInstancesToFile(false, false, true, false);
        }

        private void mRrefresh_Func_Click(object sender, EventArgs e)
        {
            GetInstancesToFile(false, true, false, true);
        }

        private void BtRegex_Click(object sender, EventArgs e)
        {
            FindInTree(true);
        }
    }
}
