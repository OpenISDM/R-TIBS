using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GPL_Testing
{
    public partial class FormMain : Form
    {
        GPLCompiler.GPLCompiling cp = new GPLCompiler.GPLCompiling();

        public FormMain()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private bool Compile()
        {
           
            TextBoxGPL_Error.Text = "";
            string result = "";
            
            cp.doCompile(TextBoxGPL_Code.Text);

            TextBoxGPL_Error.ForeColor = System.Drawing.Color.Blue;
            if (cp.symanticErrors.Count > 0)
            {
                result = "";
                for (int i = 0; i < cp.symanticErrors.Count; i++)
                {
                    result += "Error: " + cp.symanticErrors[i] + Environment.NewLine + Environment.NewLine;
                }
                TextBoxGPL_Error.ForeColor = System.Drawing.Color.Red;
                TextBoxGPL_Error.Text = result;
                return false;
            }
            else
            {
                GPLCompiler.Subjects s = cp.findASubject(TextBox_UserName.Text);
                if (s != null)
                {
                    string message = "Hello " + TextBox_UserName.Text + ". In this session you will play the following Role(s) and have rights to do following action(s) on object(s):" + Environment.NewLine;
                    string subjRoles = Environment.NewLine + "+ Role(s): ";
                    foreach (GPLCompiler.Roles r in s.Roles)
                        subjRoles += cp.getFullRoleName(r) + Environment.NewLine;
                    string actObj = Environment.NewLine + "+ Action-Object (s): " + Environment.NewLine;
                    foreach (GPLCompiler.Roles r in s.Roles)
                    {
                        List<GPLCompiler.Permissions> p = new List<GPLCompiler.Permissions>();
                        p = cp.getAllPermissions(cp.lstSubjectRoles, r);
                        foreach (GPLCompiler.Permissions p1 in p)
                        {
                            string ss = p1.op.name + " ==> " + cp.getFullRoleName(p1.role);
                            List<GPLCompiler.Objects> lst = cp.getAllObject(p1.role);
                            if (lst.Count > 0)
                            {
                                ss += "{";
                                foreach (GPLCompiler.Objects o in lst)
                                {
                                    ss += o.name + ",";
                                }
                                ss=ss.Substring(0, ss.Length - 1);
                                ss += "}";
                            }
                            actObj += ss + Environment.NewLine;
                        }
                    }
                    message += subjRoles + actObj;
                    TextBoxGPL_Error.Text = message;
                }
                else
                {
                    GPLCompiler.Objects o = cp.findAObject(TextBox_UserName.Text);
                    if (o != null)
                    {
                        string message = "Hello " + TextBox_UserName.Text + ". In this session you play the following Role(s) and under the action(s) from the following User(s):" + Environment.NewLine;
                        string objRoles = "+ Role(s): ";
                        foreach (GPLCompiler.Roles r in o.Roles)
                            objRoles += cp.getFullRoleName(r) + Environment.NewLine;
                        string actObj = Environment.NewLine + "+ Action-Subject (s): " + Environment.NewLine;
                        foreach (GPLCompiler.Roles r in o.Roles)
                        {
                            List<GPLCompiler.Permissions> p = new List<GPLCompiler.Permissions>();
                            p = cp.getAllPermissions(cp.lstObjectRoles, r);
                            foreach (GPLCompiler.Permissions p1 in p)
                            {
                                string ss = p1.op.name + " =by=> " + cp.getFullRoleName(p1.role);
                                List<GPLCompiler.Subjects> lst = cp.getAllSubject(p1.role);
                                if (lst.Count > 0)
                                {
                                    ss += "{";
                                    foreach (GPLCompiler.Subjects s1 in lst)
                                    {
                                        ss += s1.name + ",";
                                    }
                                    ss = ss.Substring(0, ss.Length - 1);
                                    ss += "}";
                                }
                                actObj += ss + Environment.NewLine;
                            }
                        }
                        message += objRoles + actObj;
                        TextBoxGPL_Error.Text = message;
                    }
                    else
                    {
                        TextBoxGPL_Error.ForeColor = System.Drawing.Color.Red;
                        TextBoxGPL_Error.Text = "The User/Object Name: '" + TextBox_UserName.Text + "' is not found.";
                    }

                }
               

                return true;

            }
        }

        private void button_Compile_Click(object sender, EventArgs e)
        {
            Compile();
        }

        private void fastColoredTextBox_Source_Load(object sender, EventArgs e)
        {

        }

        private void button_LoadFile_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            string fileName = openFileDialog1.FileName;

            if (fileName != "")
            {
                try
                {
                    TextBoxGPL_Code.Text = System.IO.File.ReadAllText(fileName);
                }
                catch
                {
                }
            }
        }

        private void button_PERMIS_xml_Click(object sender, EventArgs e)
        {
            //TextBoxGPL_Error.Text = "";
           
            //button_Compile_Click(sender, e);
            if (Compile())
            {
                if (cp.symanticErrors.Count == 0)
                {
                    string OID = "132.2434.541543.53";
                    string subjectDNs = "ou=GI,o=MacKay,c=tw";
                    string objectDNs = "ou=GI,o=MacKay,c=tw"; ;
                    string SOADn = "cn=Jane,ou=GI,o=MacKay,c=tw";

                    string xmlPolicy = cp.exportPERMIS(OID, subjectDNs, SOADn, objectDNs);
                    cp.exportPERMIS2(@"C:\policies\1234.xml",OID, subjectDNs, SOADn, objectDNs);
                    if (System.IO.Directory.Exists("C:\\policies")) System.IO.Directory.CreateDirectory("C:\\policies");
                    System.IO.File.WriteAllText("C:\\policies\\" + OID + ".xml", xmlPolicy);
                    TextBoxGPL_Error.ForeColor = System.Drawing.Color.Blue;
                    TextBoxGPL_Error.Text = xmlPolicy;
                }
            }
        }

        private void TextBoxGPL_Code_TextChanged(object sender, FastColoredTextBoxNS.TextChangedEventArgs e)
        {
            /*
            
            e.ChangedRange.ClearStyle();
            FastColoredTextBoxNS.Style keyWord = new FastColoredTextBoxNS.TextStyle(Brushes.Blue, null, FontStyle.Regular);
            FastColoredTextBoxNS.Style GreenStyle = new FastColoredTextBoxNS.TextStyle(Brushes.Green, null, FontStyle.Italic);
            FastColoredTextBoxNS.Style BoldStyle = new FastColoredTextBoxNS.TextStyle(Brushes.BlueViolet, null, FontStyle.Bold);
            FastColoredTextBoxNS.Style BoldStyleExtends = new FastColoredTextBoxNS.TextStyle(Brushes.BlueViolet, null, FontStyle.Italic | FontStyle.Bold );
            e.ChangedRange.SetStyle(GreenStyle, @"//.*$", System.Text.RegularExpressions.RegexOptions.Multiline);
            e.ChangedRange.SetStyle(BoldStyle, @"\b(class)\s+(?<range>[\w_]+?)\b");
            e.ChangedRange.SetStyle(BoldStyleExtends, @"\b(extends)\s+(?<range>[\w_]+?)\b");
            e.ChangedRange.SetFoldingMarkers("{", "}");
            e.ChangedRange.SetFoldingMarkers(@"#region\b", @"#endregion\b");
            e.ChangedRange.SetStyle(keyWord,"class|typedef|session|extends|initialize", System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace);
             * */
            
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

    }
}
