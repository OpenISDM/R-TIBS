using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Irony;
using Irony.Ast;
using Irony.Parsing;
using System.Collections.Generic;
using System.Xml;
using System.Text;

namespace GPLCompiler
{
    public class GPLCompiling
    {
        public List<Roles> lstSubjectRoles = new List<Roles>();
        public List<Roles> lstObjectRoles = new List<Roles>();
        public List<Types> typeDefs = new List<Types>();
        public List<Subjects> lstSubjects = new List<Subjects>();
        public List<Objects> lstObjects = new List<Objects>();
        public List<Policy> policies = new List<Policy>();
        public List<string> symanticErrors = new List<string>();
        public List<Operators> lstOperators = new List<Operators>();

        public GPLCompiling (){
        }

        public bool doCompile(string source)
        {
            typeDefs = new List<Types>();
            lstObjectRoles = new List<Roles>();
            lstSubjectRoles = new List<Roles>();
            lstSubjects = new List<Subjects>();
            lstObjects = new List<Objects>();
            symanticErrors = new List<string>();
            policies = new List<Policy>();
            lstOperators = new List<Operators>();

            NCTU.Grammar grammar = new NCTU.Grammar();
            Parser parser = new Parser(grammar);
            ParseTree pTree = parser.Parse(source);
            if (parser.Context.Status.ToString().ToLower() != "error")
            {
                foreach (ParseTreeNode pNode in pTree.Root.ChildNodes)
                {
                    if (pNode.Term.Name.ToLower() == "typedeclarations")
                    {
                        getTypeDeclaration(pNode);
                    }
                    if (pNode.Term.Name.ToLower() == "classdeclarations")
                    {
                        foreach (ParseTreeNode p in pNode.ChildNodes)
                        {
                            if (p.Term.Name.ToLower() == "classdeclaration")
                            {
                                if (p.FirstChild.Term.Name.ToLower() == "actiondefinitions")
                                    getActionDefintion(p.FirstChild);

                                if (p.FirstChild.Term.Name.ToLower() == "subjectroleclasses")
                                    getSubjectRoleInterfaces(p.FirstChild.FirstChild);

                                if (p.FirstChild.Term.Name.ToLower() == "objectroleclasses")
                                    getObjectRoleInterfaces(p.FirstChild.FirstChild);

                                if (p.FirstChild.Term.Name.ToLower() == "instantiatedclass")
                                    getInstantiatedRole(p.FirstChild);


                            }
                        }
                    }
                    if (pNode.Term.Name.ToLower() == "sessions")
                        getSession(pNode);
                }
                if (symanticErrors.Count == 0)
                {
                    verifyActions();
                    if (symanticErrors.Count == 0)
                        verifyPermissions();
                }
                buildRefinedGraph(lstObjectRoles);
                buildRefinedGraph(lstSubjectRoles);
                verifyOverlapDeclarationRoleSubject();
                verifyOverlapDeclarationRoleObject();
                verifyDuplicatePermission(lstObjectRoles);
                verifyDuplicatePermission(lstSubjectRoles);
                verifyConsistent(lstObjectRoles);
                verifyConsistent(lstSubjectRoles);
                //verifyDupplicateInheriatance(lstObjectRoles);
                //verifyDupplicateInheriatance(lstSubjectRoles);
                return true;
            }
            string errLine = parser.Context.CurrentToken.Location.Line.ToString();
            string errCols = parser.Context.CurrentToken.Location.Column.ToString();
            string errType = parser.Context.CurrentToken.Terminal.ToString();
            string errMessage = parser.Context.CurrentToken.ValueString;
            symanticErrors.Add("Syntax Error: " + errMessage + "\nLine: " + errLine.ToString() + "\n Column: " + errCols);
            return false;
        }

        public Roles inList(List<Roles> lst, string name)
        {
            foreach (Roles r in lst)
                if (r.name == name) return r;
            return null;
        }
        public Types findType(string name)
        {
            foreach (Types t in typeDefs)
                if (t.name == name) return t;
            return null;
        }
        public Operators findOperator(string name)
        {
            foreach (Operators op in lstOperators)
                if (op.name == name) return op;
            return null;
        }        
        public Roles findRoleDefinition(Roles r)
        {
            foreach (Roles r1 in lstObjectRoles)
            {
                if (r1.name == r.name && r1.roleType == rTypes.roleDefinition)
                    return r1;
            }
            foreach (Roles r1 in lstSubjectRoles)
            {
                if (r1.name == r.name && r1.roleType == rTypes.roleDefinition)
                    return r1;
            }
            return r;
        } 
        public Roles findRoleInstantiated(List<Roles> lst, Roles r)
        {
            foreach (Roles r1 in lst)
            {
                if (sameRoles(r1, r) && r1.roleType == rTypes.roleInstantiated)
                    return r1;
            }          
            return null;
        }
        public Roles findRoleDefintion(List<Roles> lst, string name)
        {
            foreach (Roles r in lst)
                if (r.name == name && r.roleType == rTypes.roleDefinition)
                    return r;
            return null;
        }
        public Types findTypeBound(Types t)
        {
            //assume that t is int typeDef
            if (t == null) return null;
            if (findType(t.name) != null)
            {
                while (t.parameterType != pTypes.parameterBounded)
                    t = t.superType;
                return t;
            }
            else return null;
        }
        public string getFullRoleName(Roles r)
        {
            string parameters = "";
            if (r.parameters.Count > 0)
            {
                parameters += "<"+ r.parameters[0].name;
                for (int i = 1; i < r.parameters.Count; i++)
                    parameters += "," + r.parameters[i].name; ;
                parameters += ">";
            }

            return r.name + parameters;
        }
        public Roles checkARole(Roles r, string optionalErrorMessage)
        {
            if (inList(lstObjectRoles, r.name) != null || inList(lstSubjectRoles, r.name) != null)
            {
                Roles roleDef = findRoleDefinition(r);
                if (r.parameters.Count == 0)
                {
                    foreach (Types t in roleDef.parameters)
                    {
                        Types tt = new Types();
                        tt.name = "*";
                        tt.parameterType = pTypes.parameterStar;
                        r.parameters.Add(tt);
                    }
                    return r;
                }
                else
                {
                    if (r.parameters.Count == roleDef.parameters.Count)
                    {
                        //check the bound
                        for (int i = 0; i < r.parameters.Count; i++)
                        {
                            if (r.parameters[i].parameterType == roleDef.parameters[i].parameterType || r.parameters[i].parameterType == pTypes.parameterStar)
                            {
                                if (r.parameters[i].parameterType != pTypes.parameterStar)
                                {
                                    Types bound = findTypeBound(r.parameters[i].superType);

                                    if (bound != roleDef.parameters[i].superType)
                                        symanticErrors.Add(optionalErrorMessage + " [" + getFullRoleName(r) + "] Type parameter '" + roleDef.parameters[i].name + "' is instantiated with value: '" + r.parameters[i].name + "' which is not in the type bound: '" + roleDef.parameters[i].superType.name + "'");
                                }
                            }
                            else symanticErrors.Add(optionalErrorMessage+" [" + getFullRoleName(r) + "] Type parameter '" + roleDef.parameters[i].name + "' should not be instantiated with value: '" + r.parameters[i].name + "'.");
                        }
                        return r;
                    }
                    else symanticErrors.Add(optionalErrorMessage+" [" + getFullRoleName(r) + "] lacks of/excesses type parameters' values. The parameterized role definition requires " + roleDef.parameters.Count.ToString() + " type parameters' values.");
                }
            }
            else symanticErrors.Add(optionalErrorMessage+" There is no parameterized interface defined for '" + r.name + "' role.");
            
            return null;
        }
        public Roles inRoleList(List<Roles> lst, Roles r)
        {
            foreach (Roles rr in lst)
            {
                if (rr.name == r.name && rr.parameters.Count == r.parameters.Count)
                {
                    int i = 0;
                    for (i = 0; i < rr.parameters.Count; i++)
                        if (rr.parameters[i].name != r.parameters[i].name || rr.parameters[i].parameterType != r.parameters[i].parameterType) break;
                    if (i == rr.parameters.Count) return rr;
                }
            }
            return null;
        }
        public bool isSubtypeP(Types p1, Types p2)
        {
            //is p1 <:p p2 p1==p2 :yes
            while (p2.parameterType != pTypes.parameterStar && p1 != null && p1.name != p2.name && p1.parameterType != pTypes.parameterBounded)
            {
                p1 = p1.superType;
            }
            if (p1 != null && (p1.name == p2.name || p2.parameterType == pTypes.parameterStar)) return true;
            return false;
        }
        public bool isRefinedRole(Roles r1, Roles r2)
        {
            //yes if role r1 < : r2, if one of type parameters is :> then no
            if (r1.parameters.Count == r2.parameters.Count && r1.name == r2.name)
            {
                for (int i = 0; i < r1.parameters.Count; i++)
                {
                    if (!isSubtypeP(r1.parameters[i], r2.parameters[i])) return false;
                }
                return true;
            }
            return false;
        }
        public bool sameRoles(Roles r1, Roles r2)
        {
            if (r1.name == r2.name && r1.roleType == r2.roleType && r1.roleType == rTypes.roleInstantiated)
            {
                if (r1.parameters.Count == r2.parameters.Count)
                {
                    int i = 0;
                    for (i = 0; i < r1.parameters.Count; i++)
                        if (r1.parameters[i].name != r2.parameters[i].name || r1.parameters[i].parameterType != r2.parameters[i].parameterType) break;
                    if (i == r1.parameters.Count) return true;
                }
            }
            return false;
        }

        public List<Roles> getBasedRoles(List<Roles> lst, Roles r)
        {
            List<Roles> father = new List<Roles>();
            foreach (Roles rFather in lst)
                if (rFather.name == r.name && !sameRoles(rFather, r) && isRefinedRole(r, rFather))
                {
                    Roles rf = findRoleInstantiated(lst, rFather);
                    if (rf!=null) father.Add(rf);                    
                }
            return father;
                   
        }
        public List<Roles> getAllSupperRoles(List<Roles> lst, Roles r)
        {
            List<Roles> father = new List<Roles>();
            father = getBasedRoles(lst, r);
            if (r.baseRoles.Count > 1)
            {
                Roles rf = findRoleInstantiated(lst, r.baseRoles[0]);
                if (rf != null && father.IndexOf(rf) < 0) father.Add(rf);
            }
            int i = 0;
            while (i < father.Count)
            {
                if (father[i].baseRoles.Count > 0)
                {
                    List<Roles> f = new List<Roles>();
                   
                    Roles rf = findRoleInstantiated(lst, father[i].baseRoles[0]);
                    if (rf != null && father.IndexOf(rf) <0) father.Add(rf);

                    f = getBasedRoles(lst, father[i].baseRoles[0]);
                    foreach (Roles r1 in f)
                        if (father.IndexOf(r1) < 0)
                            father.Add(r1);
                    
                }
                i++;   
            }
            return father;
        }
        public List<Permissions> getAllPermissions(List<Roles> lst, Roles r)
        {
            //after build refinement graph

            List<Roles> father = getAllSupperRoles(lst, r);
            if (father.IndexOf(r) <0) father.Add(r);
            List<Permissions> pList = new List<Permissions>();
            foreach (Roles r1 in father)
                foreach (Permissions p in r1.permissions) pList.Add(p);
            return pList;
        }       
        public List<Objects> getAllObject(Roles objectRoles)
        {
            List<Objects> oLst = new List<Objects>();
            foreach (Objects o in lstObjects)
            {
                foreach (Roles r in o.Roles)
                    if (isRefinedRole(r, objectRoles))
                    {
                        oLst.Add(o);
                    }
            }
            return oLst;
        }
        public List<Subjects> getAllSubject(Roles subjectRoles)
        {
            List<Subjects> sLst = new List<Subjects>();
            foreach (Subjects s in lstSubjects)
            {
                foreach (Roles r in s.Roles)
                    if (isRefinedRole(r, subjectRoles))
                    {
                        sLst.Add(s);
                    }
            }
            return sLst;
        }
        public string replaceRoleName(string roleName)
        {
            return roleName.Replace(',', '_').Replace('<', '_').Replace(">", "");
        }
        public Objects findAObject(string name)
        {
            foreach (Objects o in lstObjects)
            {
                if (o.name == name) return o;
            }
            return null;
        }
        public Subjects findASubject(string name)
        {
            foreach (Subjects s in lstSubjects)
            {
                if (s.name == name) return s;
            }
            return null;
        }
        public void buildRefinedGraph(List<Roles> lst)
        {
            foreach (Roles r in lst)
            {
                if (r.roleType == rTypes.roleInstantiated)
                    foreach (Roles r1 in lst)
                        if (r != r1 && r1.roleType == rTypes.roleInstantiated)
                            if (r.baseRoles.Count > 1)
                            {
                                //if (!isRefinedRole(r.baseRoles[0], r.baseRoles[1]) && !isRefinedRole(r.baseRoles[1], r.baseRoles[0]))
                                  if (isRefinedRole(r1, r.baseRoles[r.baseRoles.Count - 1])) r.baseRoles[r.baseRoles.Count - 1] = r1;

                            }
                            else if (r.baseRoles.Count>0 && isRefinedRole(r, r1) && !isRefinedRole(r.baseRoles[0], r1) && !isRefinedRole(r1, r.baseRoles[0])) r.baseRoles.Add(r1);
                            else if (isRefinedRole(r, r1) && r.baseRoles.Count==0) r.baseRoles.Add(r1);
            }
        }
       

#region TypeDeclarations


        public void getTypeDeclaration(ParseTreeNode rootNode)
        {
            if (rootNode.ChildNodes.Count  > 1)
            {
                getExtendDeclaration(rootNode.ChildNodes[1]);
                if (rootNode.ChildNodes.Count > 2)
                {
                    getTypeDeclaration(rootNode.ChildNodes[2]);
                }
            }
            
        }
        public void getExtendDeclaration(ParseTreeNode rootNode)
        {
            if (rootNode.Term.Name.ToLower() == "extenddeclaration")
            {
                //1. Lay Identitfier (s)
                List<string> identifiers = new List<string>();
                identifiers = getMultiIdentifier(rootNode.FirstChild);                
                
                if (rootNode.ChildNodes.Count > 1) //co extends
                {
                    //2. get father
                    string father = rootNode.ChildNodes[2].Token.Text;
                    Types f = findType(father);
                    if (f == null)
                        symanticErrors.Add(" The type: '" + father + "' has not defined yet.'\n Lines: " + rootNode.ChildNodes[2].Token.Location.Line.ToString() + ", Cols: " + rootNode.ChildNodes[2].Token.Location.Column.ToString());
                   
                    foreach (string s in identifiers) //store and update father defintions
                    {
                        Types t = new Types();
                        t.name = s;
                        t.superType = f;
                        typeDefs.Add(t);
                    }
                }
                else
                {
                    foreach (string s in identifiers) //store type defintions
                    {
                        Types t = new Types();
                        t.name = s;
                        t.parameterType = pTypes.parameterBounded;
                        typeDefs.Add(t);
                    }
                }
            }
        }
        public List<string> getMultiIdentifier(ParseTreeNode rootNode)
        {
            List<string> lst = new List<string>();
            if (rootNode.Term.Name.ToLower() == "multiidentifier")
            {
                lst.Add(rootNode.FirstChild.Token.Text);
                if (rootNode.ChildNodes.Count > 1)
                {
                    ParseTreeNode cNode = rootNode.ChildNodes[1];
                    while (cNode.Term.Name.ToLower() == "repeatedidentifier" && cNode.ChildNodes.Count > 1 )
                    {
                        string identifier = getRepeatedIdentifier(cNode);
                        lst.Add(identifier);                                               
                        cNode = cNode.ChildNodes[2];                                              
                    }
                }
            }
            return lst;
        }
        public string getRepeatedIdentifier(ParseTreeNode rootNode)
        {
            if (rootNode.Term.Name.ToLower() == "repeatedidentifier" && rootNode.ChildNodes.Count>1)            
            {                
                return rootNode.ChildNodes[1].Token.Text;
            }
            return "";
        }
        
#endregion TypeDeclarations

#region ActionDefintions

        public void getActionDefintion(ParseTreeNode pNode)
        {
            if (pNode.Term.Name.ToLower() == "actiondefinitions")
            {
                //1.get action name
                string action = pNode.ChildNodes[1].Token.Text;
                Operators op = new Operators();
                op.name = action;

                //2. find left and right operations
                op.leftOperand =  getRoleIdentifier(pNode.ChildNodes[3]);
                op.rightOperand = getRoleIdentifier(pNode.ChildNodes[5]);
                lstOperators.Add(op);
            }
        }
        public Roles getRoleIdentifier(ParseTreeNode rootNode)
        {
            Roles r = new Roles();             
            if (rootNode.Term.Name.ToLower() == "roleidentifier")
            {
                //1.lay identifier
                string id = rootNode.FirstChild.Token.Text;
                r.name = id;
                if (rootNode.ChildNodes.Count > 2) //instantiated role identifier
                {
                    //get identifiervalue
                   
                    r.parameters.Add(getIdentifierValue(rootNode.ChildNodes[2].FirstChild));

                    ParseTreeNode cNode = rootNode.ChildNodes[2].ChildNodes[1];
                    while (cNode.Term.Name.ToLower() == "identifiervaluelist" && cNode.ChildNodes.Count > 1)
                    {
                        r.parameters.Add(getIdentifierValue(cNode.ChildNodes[1]));
                        cNode = cNode.ChildNodes[2];
                    }                    
                }

            }
            return r;
        }
        public Types getIdentifierValue(ParseTreeNode rootNode)
        {
            Types t = new Types();
            if (rootNode.Term.Name.ToLower() == "identifiervalue")
            {
                string name = rootNode.FirstChild.Token.Text;
                t.name = name;

                if (rootNode.FirstChild.Term.Name.ToLower() == "identifier")
                {
                    Types st = findType(name);
                    if (st == null)
                        symanticErrors.Add(" The type: '" + name + "' has not defined yet.'\n Lines: " + rootNode.FirstChild.Token.Location.Line.ToString() + ", Cols: " + rootNode.FirstChild.Token.Location.Column.ToString());
                    else
                        t.superType = st.superType;

                    t.parameterType = pTypes.parameterType;
                }
                if (rootNode.FirstChild.Term.Name.ToLower() == "string")
                    t.parameterType = pTypes.parameterValue;

                if (rootNode.FirstChild.Term.Name.ToLower() == "*")
                    t.parameterType = pTypes.parameterStar;


            }
            return t;
        }

#endregion ActionDefinitions

#region SubjectRoleClass

        public void getSubjectRoleInterfaces(ParseTreeNode pNode)
        {
            if (pNode.Term.Name.ToLower() == "subjroleclass")
            {
                //1. get role identifier
                Roles r = new Roles();
                r.name = pNode.ChildNodes[1].Token.Text;
                List<Types> lst = getParameters(pNode.ChildNodes[2].ChildNodes[1]);
                foreach (Types t in lst)  
                    r.parameters.Add(t);
               
                r.roleType = rTypes.roleDefinition;
                lstSubjectRoles.Add(r);
            }
        }
        public List<Types> getParameters(ParseTreeNode rootNode)
        {
            List<Types> lst = new List<Types>();
            if (rootNode.Term.Name.ToLower() == "parameterslists")
            {
                lst.Add(getSingleExtend(rootNode.FirstChild));
                ParseTreeNode cNode = rootNode.ChildNodes[1];

                while (cNode.Term.Name.ToLower() == "extendlist" && cNode.ChildNodes.Count > 1)
                {
                    lst.Add(getSingleExtend(cNode.ChildNodes[1]));
                    cNode = cNode.ChildNodes[2];
                }
                
            }
            return lst;
        }
        public Types getSingleExtend(ParseTreeNode rootNode)
        {
            Types t = new Types();
            if (rootNode.Term.Name.ToLower() == "singleextend")
            {
                string id = rootNode.FirstChild.Token.Text;
                string father = "";               
                t.name = id;
                t.parameterType = pTypes.parameterValue;

                if (rootNode.ChildNodes.Count > 2) //extends 
                {
                    father = rootNode.ChildNodes[2].Token.Text;                    
                    Types f = findType(father);
                    if (f == null)
                        symanticErrors.Add(" The type: '" + father + "' haven't defined yet. \n Line: " + rootNode.ChildNodes[2].Token.Location.Line.ToString() + " \n Cols: " + rootNode.ChildNodes[2].Token.Location.Column.ToString());
                    t.superType = f;
                    t.parameterType = pTypes.parameterType;                    
                }              
            }
            return t;
        }
        
#endregion SubjectRoleClass

#region ObjectRoleInterface

        public void getObjectRoleInterfaces(ParseTreeNode pNode)
        {
            if (pNode.Term.Name.ToLower() == "objroleclass")
            {
                //1. get role identifier
                Roles r = new Roles();
                r.name = pNode.ChildNodes[1].Token.Text;
                List<Types> lst = getParameters(pNode.ChildNodes[2].ChildNodes[1]);
               
                foreach (Types t in lst)
                    r.parameters.Add(t);
                
                r.roleType = rTypes.roleDefinition;
                lstObjectRoles.Add(r);
            }
        }


#endregion ObjectRoleInterface

#region SubjectRoleInstantiated

        public void getInstantiatedRole(ParseTreeNode pNode)
        {
            if (pNode.Term.Name.ToLower() == "instantiatedclass")
            {
                Roles r = getRoleIdentifier(pNode.ChildNodes[1]);
                Roles f;
                if (pNode.ChildNodes.Count > 5) //inheriance
                {
                    //get father
                    f = getRoleIdentifier(pNode.ChildNodes[3]);
                    if ((inList(lstSubjectRoles, f.name) != null && inList(lstSubjectRoles, r.name) != null) || (inList(lstObjectRoles, f.name) != null && inList(lstObjectRoles, r.name) != null))
                    {
                        r.baseRoles.Add(f);
                    }
                    else
                        symanticErrors.Add(" Cannot extends a subject-role from a object-role or a object-role from a subject-role.\nLine: " + pNode.ChildNodes[1].FirstChild.Token.Location.Line.ToString() + "\n Column: " + pNode.ChildNodes[1].FirstChild.Token.Location.Column.ToString());
                    ParseTreeNode cNode = pNode.ChildNodes[5];
                    if (cNode.Term.Name.ToLower() == "instantiatedclassbody")
                    {
                        foreach (ParseTreeNode cn in cNode.ChildNodes)
                        {
                            Permissions p = getPermissions(cn);
                            if (inList(lstSubjectRoles, r.name) != null && inList(lstObjectRoles, p.role.name) == null)
                                symanticErrors.Add(" The second argument: '" + getFullRoleName(p.role) + "' of the permission is not an object role." + pNode.ChildNodes[1].FirstChild.Token.Location.Line.ToString() + "\n Column: " + pNode.ChildNodes[1].FirstChild.Token.Location.Column.ToString());
                            if (inList(lstObjectRoles, r.name) != null && inList(lstSubjectRoles, p.role.name) == null)
                                symanticErrors.Add(" The second argument: '" + getFullRoleName(p.role) + "' of the permission is not a subject role." + pNode.ChildNodes[1].FirstChild.Token.Location.Line.ToString() + "\n Column: " + pNode.ChildNodes[1].FirstChild.Token.Location.Column.ToString());
                            r.permissions.Add(p);
                        }
                    }
                    
                }
                else
                {                       
                    ParseTreeNode cNode = pNode.ChildNodes[3];
                    if (cNode.Term.Name.ToLower() == "instantiatedclassbody")
                    {
                        foreach (ParseTreeNode cn in cNode.ChildNodes)
                        {
                            Permissions p = getPermissions(cn);
                            if (inList(lstSubjectRoles, r.name)!= null && inList(lstObjectRoles, p.role.name)== null)
                                symanticErrors.Add(" The second argument: '" + getFullRoleName(p.role) + "' of the permission is not an object role." + pNode.ChildNodes[1].FirstChild.Token.Location.Line.ToString() + "\n Column: " + pNode.ChildNodes[1].FirstChild.Token.Location.Column.ToString());
                            if (inList(lstObjectRoles, r.name)!= null && inList(lstSubjectRoles, p.role.name) == null)
                                symanticErrors.Add(" The second argument: '" + getFullRoleName(p.role) + "' of the permission is not a subject role." + pNode.ChildNodes[1].FirstChild.Token.Location.Line.ToString() + "\n Column: " + pNode.ChildNodes[1].FirstChild.Token.Location.Column.ToString());
                            r.permissions.Add(getPermissions(cn));
                        }
                    }
                }
                if (lstSubjectRoles.Count > 0 && inList(lstSubjectRoles, r.name) != null)
                {
                    if (lstSubjectRoles.IndexOf(r) < 0)
                    {
                       // f = findRoleDefintion(lstSubjectRoles, r.name);
                        //r.baseRoles.Add(f);
                        
                        r = checkARole( r, "");
                        if (r != null)
                        {
                            lstSubjectRoles.Add(r);
                        }
                    }
                    else
                        symanticErrors.Add(" Role '" + getFullRoleName(r) + "' already exist.\nLine: " + pNode.ChildNodes[1].FirstChild.Token.Location.Line.ToString() + "\n Column: " + pNode.ChildNodes[1].FirstChild.Token.Location.Column.ToString());

                }
                else
                    if (lstObjectRoles.Count > 0 && inList(lstObjectRoles, r.name) != null)
                    {
                        if (lstObjectRoles.IndexOf(r) < 0)
                        {
                           // f = findRoleDefintion(lstObjectRoles, r.name);
                           // r.baseRoles.Add(f);
                            r = checkARole(r, "");
                            if (r != null)
                            {
                                lstObjectRoles.Add(r);
                            }
                        }
                        else
                            symanticErrors.Add(" Role '" + getFullRoleName(r) + "' already exist.\nLine: " + pNode.ChildNodes[1].FirstChild.Token.Location.Line.ToString() + "\n Column: " + pNode.ChildNodes[1].FirstChild.Token.Location.Column.ToString());
                    }
                    else 
                        symanticErrors.Add(" There was no parameterized interface defined for '" + r.name + "' role.\nLine: " + pNode.ChildNodes[1].FirstChild.Token.Location.Line.ToString() + "\n Column: " + pNode.ChildNodes[1].FirstChild.Token.Location.Column.ToString());
                    
            }
        }
        public Permissions getPermissions(ParseTreeNode rootNode)
        {
            Permissions p = new Permissions();
            if (rootNode.Term.Name.ToLower() == "permissioninstantiated")
            {
                string operatorName = rootNode.ChildNodes[2].FirstChild.Token.Text;
                Operators op = findOperator(operatorName);
                Roles r = getRoleIdentifier(rootNode.ChildNodes[4]);                                
                if (op == null)
                    symanticErrors.Add("Action '" + operatorName + "' haven't defined yet. \nLIine: " + rootNode.ChildNodes[2].FirstChild.Token.Location.Line.ToString() + "\n Cols:"+ rootNode.ChildNodes[2].FirstChild.Token.Location.Column.ToString());
                p.op = op;
                p.role = r;
            }
            return p;
        }


#endregion SubjectRoleInstantiated

#region Verify Action & Permissions

        public bool verifyActions()
        {
            foreach (Operators op in lstOperators)
            {
                string optErrorMessage = "[action "+ op.name + "(" +getFullRoleName(op.leftOperand) +", " + getFullRoleName(op.rightOperand) + ")]" ;
                Roles r = checkARole(op.leftOperand, optErrorMessage);
                if (r == null) return false;                
                r = checkARole(op.rightOperand, optErrorMessage );
                if (r == null) return false;
            }
            return true;
        }
        public bool verifyPermissions()
        {
            //for subjectRoles
            foreach (Roles r in lstSubjectRoles)
            {
                if (r.roleType == rTypes.roleInstantiated)
                {
                    Roles r1 = checkARole(r, "");
                    if (r.baseRoles.Count > 0)
                    {
                        r1 = checkARole(r.baseRoles[0], "Extends");
                        Roles ri = findRoleInstantiated(lstSubjectRoles, r.baseRoles[0]);
                        if (ri == null)
                            symanticErrors.Add("Role: '" + getFullRoleName(r) +"' is inherited from a unknown role: '" + getFullRoleName(r.baseRoles[0]) + "'");
                    }
                    foreach (Permissions p in r.permissions)
                    {
                        string optErrorMesseage = getFullRoleName(r) + ".permission(" + p.op.name + "," + getFullRoleName(p.role) + ")";
                        Operators op = findOperator(p.op.name);
                        if (op == null)
                            symanticErrors.Add(" [" + getFullRoleName(r) + ".permission(" + p.op.name + "," + getFullRoleName(p.role) + ")] Action '" + p.op.name + "' haven't defined yet.");
                        Roles r2 = checkARole(p.role, optErrorMesseage);
                        if (r2 != null && op != null)
                        {
                            //verify the compatability
                            if (isRefinedRole(r, op.leftOperand))
                            {
                                if (!isRefinedRole(p.role, op.rightOperand))
                                    symanticErrors.Add("[permission(" + p.op.name + "," + getFullRoleName(p.role) + ")] Action: '" + op.name + "' cannot be performed by subject-role: " + getFullRoleName(r) + " on object-role: " + getFullRoleName(p.role));
                            }
                            else
                                symanticErrors.Add("[permission(" + p.op.name + "," + getFullRoleName(p.role) + ")] Action: '" + op.name + "' cannot be performed by subject-role: " + getFullRoleName(r) + " on object-role: " + getFullRoleName(p.role));
                        }
                    }
                }
            }
            foreach (Roles r in lstObjectRoles)
            {
                if (r.roleType == rTypes.roleInstantiated)
                {
                    Roles r1 = checkARole(r,"");
                    if (r.baseRoles.Count > 0)
                    {
                        r1 = checkARole(r.baseRoles[0], "Extends");
                        Roles ri = findRoleInstantiated(lstObjectRoles, r.baseRoles[0]);
                        if (ri == null)
                            symanticErrors.Add("Role: '" + getFullRoleName(r) + "' is inherited from a unknown role: '" + getFullRoleName(r.baseRoles[0]) + "'");
                    }
                
                    foreach (Permissions p in r.permissions)
                    {
                        string optErrorMesseage = getFullRoleName(r) + ".permission(" + p.op.name + "," + getFullRoleName(p.role) + ")";
                        Operators op = findOperator(p.op.name);
                        if (op == null)
                            symanticErrors.Add(" [" + getFullRoleName(r) + ".permission(" + p.op.name + "," + getFullRoleName(p.role) + ")] Action '" + p.op.name + "' haven't defined yet.");
                        Roles r2 = checkARole(p.role,optErrorMesseage);
                        if (r2 != null && op != null)
                        {
                            //verify the compatability
                            if (isRefinedRole(r, op.rightOperand))
                            {
                                if (!isRefinedRole(p.role, op.leftOperand))
                                    symanticErrors.Add("[permission(" + p.op.name + "," + getFullRoleName(p.role) + ")] Action: '" + op.name + "' cannot be performed by subject-role: " + getFullRoleName(p.role) + " on object-role: " + getFullRoleName(r));
                            }
                            else
                                symanticErrors.Add("[permission(" + p.op.name + "," + getFullRoleName(p.role) + ")] Action: '" + op.name + "' cannot be  performed by subject-role: " + getFullRoleName(p.role) + " on object-role: " + getFullRoleName(r));
                        }
                    }
                }
            }
            return true;
        }

#endregion Verify Actions & Permissions

#region Other Checking

        public void verifyDuplicatePermission(List<Roles> lst)
        {
            //must be built inheritance first
            foreach (Roles r in lst)
                if (r.roleType == rTypes.roleInstantiated)
                {
                    List<Permissions> pList = getAllPermissions(lst, r);
                    for (int i = 0; i < pList.Count; i++)
                        for (int j = i+1; j < pList.Count; j++)
                            if (pList[i].op == pList[j].op && getFullRoleName(pList[i].role) == getFullRoleName(pList[i].role))
                                symanticErrors.Add("The permission: permission(" + pList[i].op.name + ", " + getFullRoleName(pList[i].role) + ") is already defined in a supper role of role: " + getFullRoleName(r));

                }
        }
        public void verifyOverlapDeclarationRoleSubject()
        {
            //must be called after building refinement graph
            foreach (Subjects s in lstSubjects)
                foreach(Roles r in s.Roles)
                    foreach(Roles r1 in s.Roles)
                        if (r != r1 && isRefinedRole(r, r1))
                            symanticErrors.Add("No need to assign role: '" + getFullRoleName(r1) + "' to the subject: '" + s.name + "' because this role is supper type of role: '" + getFullRoleName(r) + "' which already assigned to the subjet." );
        }
        public void verifyOverlapDeclarationRoleObject()
        {
            //must be called after building refinement graph
            foreach (Objects o in lstObjects)
                foreach (Roles r in o.Roles)
                    foreach (Roles r1 in o.Roles)
                        if (r != r1 && isRefinedRole(r, r1))
                            symanticErrors.Add("No need to assign role: '" + getFullRoleName(r1) + "' to the object: '" + o.name + "' because this role is supper type of role: '" + getFullRoleName(r) + "' which already assigned to the subjet.");
        }
        public void verifyConsistent(List<Roles> lst)
        {
            //must be called after building refinement graph
            foreach (Roles r in lst)
            {
                List<Roles> sRole = getAllSupperRoles(lst, r);
                foreach (Roles fR in sRole)
                {
                    if (isRefinedRole(fR, r))
                    {
                        symanticErrors.Add("Type '" + getFullRoleName(fR) + "' is inconsitent.");
                        break;
                    }
                    else
                    {
                        List<Roles> fSRole = getAllSupperRoles(lst, fR);
                        foreach (Roles ffR in fSRole)
                            if (sameRoles(r, ffR))
                            {
                                symanticErrors.Add("Type '" + getFullRoleName(fR) + "' is inconsitent.");
                                break;
                            }
                    }
                }
                
            }
                
        }
        public void verifyDupplicateInheriatance(List<Roles> lst)
        {          
            foreach (Roles r in lst)
            {
                if (r.baseRoles.Count > 1)
                {
                    List<Roles> bRole = getBasedRoles(lst, r);
                    foreach (Roles fR in bRole)
                        if (sameRoles(r.baseRoles[0], fR))
                            symanticErrors.Add("No need to extends from role: '" + getFullRoleName(fR) + "' for the role: '" +getFullRoleName(r)+"'");

                }
            }
        }


#endregion Other Checking

#region Sessions

        public void getSession(ParseTreeNode pNode)
        {
            if (pNode.Term.Name.ToLower() == "sessions")
            {
                //1. get subject/object declarations
                //2.get assignments
                foreach (ParseTreeNode p in pNode.ChildNodes[3].ChildNodes)
                {
                    if (p.FirstChild.Term.Name.ToLower() == "subjobjdeclaration")
                        getObjectSubjectDeclarations(p.FirstChild);
                    if (p.FirstChild.Term.Name.ToLower() == "assignmentstatement")
                        getAssignments(p.FirstChild);
                }
            }
        }
        public void getObjectSubjectDeclarations(ParseTreeNode rootNode)
        {
            if (rootNode.Term.Name.ToLower() == "subjobjdeclaration")
            {
                string type = rootNode.FirstChild.FirstChild.Token.Text;
                if (type.ToLower() == "subject")
                {
                    List<string> lst = getMultiIdentifier(rootNode.ChildNodes[1]);
                    foreach (string s in lst)
                    {
                        Subjects ss = new Subjects();
                        ss.name = s;
                        lstSubjects.Add(ss);
                    }
                }
                else if (type.ToLower() == "object")
                {
                    List<string> lst = getMultiIdentifier(rootNode.ChildNodes[1]);
                    foreach (string s in lst)
                    {
                        Objects oj = new Objects();
                        oj.name = s;
                        lstObjects.Add(oj);
                    }
                }
            }
            
        }
        public void getAssignments(ParseTreeNode rootNode)
        {
            if (rootNode.Term.Name.ToLower() == "assignmentstatement")
            {
                List<string> ids = new List<string>();
                ids.Add(rootNode.FirstChild.FirstChild.Token.Text);
                
                ParseTreeNode cNode = rootNode.FirstChild.ChildNodes[1];
                while (cNode.Term.Name.ToLower() == "sameassignment" && cNode.ChildNodes.Count> 1)
                {
                    ids.Add(cNode.ChildNodes[1].Token.Text);
                    cNode = cNode.ChildNodes[2];
                }

                List<Roles> r = new List<Roles>();
                cNode = rootNode.ChildNodes[2];
                Roles rg = getRoleIdentifier(cNode.ChildNodes[1]);
                Roles rr = checkARole(rg, "Assignment:");

                Roles sr = inRoleList(lstSubjectRoles, rg);
                Roles or;
                if (sr != null)
                    r.Add(sr);
                else
                {
                    or = inRoleList(lstObjectRoles, rg);
                    if (or != null)
                        r.Add(or);
                    else
                        symanticErrors.Add(" The role '" + getFullRoleName(rg) + "' is not exist.'\n Lines: " + rootNode.FirstChild.FirstChild.Token.Location.Line.ToString() + ", Cols: " + rootNode.FirstChild.FirstChild.Token.Location.Column.ToString());
                }
                
                

                if (cNode.ChildNodes.Count > 2)
                {
                    cNode = cNode.ChildNodes[2];
                    while (cNode.Term.Name.ToLower() == "repeatedroleidentifier" && cNode.ChildNodes.Count > 1)
                    {
                        rg = getRoleIdentifier(cNode.ChildNodes[2]);
                        rr = checkARole(rg, "Assignment:");
                         sr = inRoleList(lstSubjectRoles, rg);
                        if (sr != null)
                            r.Add(sr);
                        else
                        {
                            or = inRoleList(lstObjectRoles, rg);
                            if (or != null)
                                r.Add(or);
                            else
                                symanticErrors.Add(" The role '" + getFullRoleName(rg) + "' is not exist.'\n Lines: " + rootNode.FirstChild.FirstChild.Token.Location.Line.ToString() + ", Cols: " + rootNode.FirstChild.FirstChild.Token.Location.Column.ToString());
                        }
                        cNode = cNode.ChildNodes[3];
                    }
                }

                foreach(string si in ids) {
                    Subjects s = findASubject(si);
                    Objects o = findAObject(si);
                    if (s!= null) {
                        foreach (Roles r1 in r)
                        {
                            Roles r2 = inList(lstSubjectRoles, r1.name);
                            if (r2 != null) s.Roles.Add(r1);
                            else
                                 symanticErrors.Add(" The role '" + getFullRoleName(r1) + "' is not a subject-role.'\n Lines: " + rootNode.FirstChild.FirstChild.Token.Location.Line.ToString() + ", Cols: " + rootNode.FirstChild.FirstChild.Token.Location.Column.ToString());
                        }
                    }else if (o != null) {
                        foreach (Roles r1 in r)
                        {
                            Roles r2 = inList(lstObjectRoles, r1.name);
                            if (r2 != null) o.Roles.Add(r1);
                            else
                                symanticErrors.Add(" The role '" + getFullRoleName(r1) + "' is not an object-role.'\n Lines: " + rootNode.FirstChild.FirstChild.Token.Location.Line.ToString() + ", Cols: " + rootNode.FirstChild.FirstChild.Token.Location.Column.ToString());
                        }
                    }
                    else {
                        symanticErrors.Add(" The object/subject '" + si + "' has not defined yet.'\n Lines: " + rootNode.FirstChild.FirstChild.Token.Location.Line.ToString() + ", Cols: " + rootNode.FirstChild.FirstChild.Token.Location.Column.ToString());
                    }
                }

            }
        }
       
#endregion Sessions

#region Export to PERMIS
  
        public string exportPERMIS( string OID, string subjectDNs, string SOA_Dn, string objectDNs)
        {

            string permisXML = "";
            permisXML += "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + Environment.NewLine;
            permisXML += "<X.509_PMI_RBAC_Policy OID=\"" + OID + "\">" + Environment.NewLine;

            //1. Subject Domain (all rubjects)
            permisXML += "  <SubjectPolicy>" + Environment.NewLine;
            foreach (Subjects s in lstSubjects)
            {
                permisXML += "        <SubjectDomainSpec ID=\"" + s.name + "\">" + Environment.NewLine;
                permisXML += "            <Include LDAPDN=\"cn=" + s.name + "," + subjectDNs + "\"/>" + Environment.NewLine;
                permisXML += "        </SubjectDomainSpec>" + Environment.NewLine;
            }
            permisXML += "    </SubjectPolicy>" + Environment.NewLine;

            //2. Role Hierachy
            permisXML += "   <RoleHierarchyPolicy>" + Environment.NewLine;
            permisXML += "       <RoleSpec OID=\"1.2.826.0.1.3344810.1.1.14\" Type=\"permisRole\">" + Environment.NewLine;
            foreach (Roles sr in lstSubjectRoles)
            {
                if (sr.roleType == rTypes.roleInstantiated)
                {

                    if (sr.baseRoles.Count > 0)
                    {
                        permisXML += "           <SupRole Value=\"" + replaceRoleName(getFullRoleName(sr)) + "\">" + Environment.NewLine;
                        for (int i = 0; i < sr.baseRoles.Count; i++)
                        {
                            Roles fSR = sr.baseRoles[i];
                            if (fSR != null && fSR.roleType == rTypes.roleInstantiated)
                            {
                                //permisXML += "           <SupRole Value=\"" + replaceRoleName(getFullRoleName(fSR)) + "\"/>" + Environment.NewLine;

                                permisXML += "               <SubRole Value=\"" + replaceRoleName(getFullRoleName(fSR)) + "\"/>" + Environment.NewLine;

                            }
                        }
                        permisXML += "           </SupRole>" + Environment.NewLine;
                    }
                    else
                    {
                        permisXML += "           <SupRole Value=\"" +replaceRoleName(getFullRoleName(sr)) + "\"/>" + Environment.NewLine;
                    }


                }
            }
            permisXML += "        </RoleSpec>" + Environment.NewLine;
            permisXML += "    </RoleHierarchyPolicy>" + Environment.NewLine;

            //3. SOA Policy
            permisXML += "    <SOAPolicy>" + Environment.NewLine;
            permisXML += "      <SOASpec ID=\"TheSOA\" LDAPDN=\"" + SOA_Dn + "\"/>" + Environment.NewLine;
            permisXML += "    </SOAPolicy>" + Environment.NewLine;

            //4. Role Assignment Policy
            permisXML += "  <RoleAssignmentPolicy>" + Environment.NewLine;
            foreach (Subjects s in lstSubjects)
            {
                List<Roles> sSR = s.Roles;
                permisXML += "      <RoleAssignment ID=\"Assign_Roles_" + s.name + "\">" + Environment.NewLine;
                permisXML += "          <SubjectDomain ID=\"" + s.name + "\"/>" + Environment.NewLine;
                permisXML += "          <RoleList>" + Environment.NewLine;
                foreach (Roles sr in sSR)
                {
                    permisXML += "              <Role Type=\"permisRole\" Value=\"" + replaceRoleName(getFullRoleName(sr)) + "\"/>" + Environment.NewLine;
                }
                permisXML += "          </RoleList>" + Environment.NewLine;
                permisXML += "          <Delegate/>" + Environment.NewLine;
                permisXML += "          <SOA ID=\"TheSOA\"/>" + Environment.NewLine;
                permisXML += "          <Validity />" + Environment.NewLine;
                permisXML += "      </RoleAssignment>" + Environment.NewLine;
            }
            permisXML += "  </RoleAssignmentPolicy>" + Environment.NewLine;

            //5. Target Policy
            permisXML += "  <TargetPolicy>" + Environment.NewLine;
            foreach (Objects o in lstObjects)
            {
                permisXML += "        <TargetDomainSpec ID=\"" + o.name + "\">" + Environment.NewLine;
                permisXML += "            <Include LDAPDN=\"cn=" + o.name + "," + objectDNs + "\"/>" + Environment.NewLine;
                permisXML += "        </TargetDomainSpec>" + Environment.NewLine;
            }
            permisXML += "  </TargetPolicy>" + Environment.NewLine;

            //6. Action Policy
            permisXML += "  <ActionPolicy>" + Environment.NewLine;
            foreach (Roles sr in lstSubjectRoles)
            {
                if (sr.roleType == rTypes.roleInstantiated)
                {
                                       
                    foreach (Permissions p in sr.permissions)
                    {
                        permisXML += "      <Action ID=\"" + p.op.name + "\" Name =\"" +p.op.name  + "\">" + Environment.NewLine;
                        List<Objects> lst = getAllObject(p.role);
                        foreach (Objects o in lst)
                        {
                            permisXML += "          <TargetDomain ID=\"" + o.name + "\"/>" + Environment.NewLine;
                        }
                        permisXML += "      </Action>" + Environment.NewLine;

                    }
                }
            }
            permisXML += "  </ActionPolicy>" + Environment.NewLine;

            //7. Target Access Policy
            permisXML += "  <TargetAccessPolicy>" + Environment.NewLine;
            foreach (Roles sr in lstSubjectRoles)
            {
                if (sr.roleType == rTypes.roleInstantiated)
                {
                    //List<Permissions> pList = getAllPermissions(lstSubjectRoles, sr);
                    foreach (Permissions p in sr.permissions)
                    {
                        List<Objects> lst = getAllObject(p.role);
                         foreach (Objects o in lst)
                            {
                            permisXML += "      <TargetAccess ID=\"TargetAccessPolicy_" + replaceRoleName( getFullRoleName(sr)) + p.op.name + "\">" + Environment.NewLine;
                            permisXML += "          <RoleList>" + Environment.NewLine;
                            permisXML += "              <Role Type=\"permisRole\" Value=\"" + replaceRoleName(getFullRoleName(sr)) + "\"/>" + Environment.NewLine;
                            permisXML += "          </RoleList>" + Environment.NewLine;
                            permisXML += "          <TargetList>" + Environment.NewLine;
                           
                                permisXML += "              <Target>" + Environment.NewLine;
                                permisXML += "                  <TargetDomain ID=\"" + o.name + "\"/>" + Environment.NewLine;
                                permisXML += "                  <AllowedAction ID=\"" + p.op.name + "\"/>" + Environment.NewLine;
                                permisXML += "              </Target>" + Environment.NewLine;
                            
                            permisXML += "          </TargetList>" + Environment.NewLine;
                            permisXML += "      </TargetAccess>" + Environment.NewLine;
                        }

                    }

                }
            }
            permisXML += "  </TargetAccessPolicy>" + Environment.NewLine;

            permisXML += "</X.509_PMI_RBAC_Policy>";

           

            return permisXML;

        }


        public void exportPERMIS2(string filePath, string OID, string subjectDNs, string SOA_Dn, string objectDNs)
        {

            XmlWriterSettings wsetting = new XmlWriterSettings();
            wsetting.Indent = true;
            XmlWriter writer = XmlWriter.Create(filePath, wsetting);
            writer.WriteStartDocument();

            writer.WriteStartElement("X.509_PMI_RBAC_Policy");
            writer.WriteAttributeString("OID", OID);

            //1. Subject Domain (all rubjects)
            writer.WriteStartElement("SubjectPolicy");
            foreach (Subjects s in lstSubjects)
            {
                writer.WriteStartElement("SubjectDomainSpec");
                writer.WriteAttributeString("ID", s.name);

                writer.WriteStartElement("Include");
                writer.WriteAttributeString("LDAPDN", "cn=" + s.name + "," + subjectDNs);
                writer.WriteEndElement();//Include

                writer.WriteEndElement();//Subject Domain Spec
            }
            writer.WriteEndElement();//SubjectPolicy

            //2. Role Hierachy
            writer.WriteStartElement("RoleHierarchyPolicy");
            writer.WriteStartElement("RoleSpec");
            writer.WriteAttributeString("OID", "1.2.826.0.1.3344810.1.1.14");
            writer.WriteAttributeString("Type", "permisRole");
            foreach (Roles sr in lstSubjectRoles)
            {
                if (sr.roleType == rTypes.roleInstantiated)
                {

                    if (sr.baseRoles.Count > 0)
                    {
                        writer.WriteStartElement("SupRole");
                        writer.WriteAttributeString("Value", replaceRoleName(getFullRoleName(sr)));

                        for (int i = 0; i < sr.baseRoles.Count; i++)
                        {
                            Roles fSR = sr.baseRoles[i];
                            if (fSR != null && fSR.roleType == rTypes.roleInstantiated)
                            {

                                writer.WriteStartElement("SubRole");
                                writer.WriteAttributeString("Value", replaceRoleName(getFullRoleName(fSR)));
                                writer.WriteEndElement();//sub role
                            }
                        }
                        writer.WriteEndElement();//sup role
                    }
                    else
                    {
                        writer.WriteStartElement("SupRole");
                        writer.WriteAttributeString("Value",replaceRoleName(getFullRoleName(sr)));
                        writer.WriteEndElement();//sup role
                    }

                }
            }
            writer.WriteEndElement();//RoleSpec
            writer.WriteEndElement();//RoleHierarchyPolicy

            //3. SOA Policy
            writer.WriteStartElement("SOAPolicy");

            writer.WriteStartElement("SOASpec");
            writer.WriteAttributeString("ID","TheSOA");
            writer.WriteAttributeString("LDAPDN", SOA_Dn);
            writer.WriteEndElement();//SOASpec

            writer.WriteEndElement();//SOAPolicy

            //4. Role Assignment Policy
            writer.WriteStartElement("RoleAssignmentPolicy");
            foreach (Subjects s in lstSubjects)
            {
                List<Roles> sSR = s.Roles;
                writer.WriteStartElement("RoleAssignment");
                writer.WriteAttributeString("ID", "Assign_Roles_" + s.name);
                
                writer.WriteStartElement("SubjectDomain");                
                writer.WriteAttributeString("ID", s.name);                
                writer.WriteEndElement();//SubjectDomain
                
                writer.WriteStartElement("RoleList");
                foreach (Roles sr in sSR)
                {
                    writer.WriteStartElement("Role");
                    writer.WriteAttributeString("Type", "permisRole");
                    writer.WriteAttributeString("Value", replaceRoleName(getFullRoleName(sr)));
                    writer.WriteEndElement();//Role
                }
                writer.WriteEndElement();//role list
                
                writer.WriteStartElement("Delegate");                
                writer.WriteEndElement();//Delegate

                writer.WriteStartElement("SOA");
                writer.WriteAttributeString("ID", "TheSOA");
                writer.WriteEndElement();//SOA
                
                writer.WriteStartElement("Validity");                
                writer.WriteEndElement(); //Validity
                
                writer.WriteEndElement();//Role Assigment

            }
            writer.WriteEndElement(); //RoleAssignemtn Policy

            //5. Target Policy
            writer.WriteStartElement("TargetPolicy");
            foreach (Objects o in lstObjects)
            {
                writer.WriteStartElement("TargetDomainSpec");
                writer.WriteAttributeString("ID", o.name);

                writer.WriteStartElement("Include");
                writer.WriteAttributeString("LDAPDN", "cn=" + o.name + "," + objectDNs);
                writer.WriteEndElement();//Include

                writer.WriteEndElement();//Target Domain Spec
            }
            writer.WriteEndElement();//target policy

            //6. Action Policy
            writer.WriteStartElement("ActionPolicy");
            foreach (Roles sr in lstSubjectRoles)
            {
                if (sr.roleType == rTypes.roleInstantiated)
                {

                    foreach (Permissions p in sr.permissions)
                    {
                        writer.WriteStartElement("Action");
                        writer.WriteAttributeString("ID", p.op.name);
                        writer.WriteAttributeString("Name", p.op.name);
                        List<Objects> lst = getAllObject(p.role);
                        foreach (Objects o in lst)
                        {
                            writer.WriteStartElement("TargetDomain");
                            writer.WriteAttributeString("ID", o.name);
                            writer.WriteEndElement();//target domain
                        }
                        writer.WriteEndElement();//Action

                    }
                }
            }
            writer.WriteEndElement();//Action Policy

            //7. Target Access Policy
            writer.WriteStartElement("TargetAccessPolicy");
            foreach (Roles sr in lstSubjectRoles)
            {
                if (sr.roleType == rTypes.roleInstantiated)
                {
                    //List<Permissions> pList = getAllPermissions(lstSubjectRoles, sr);
                    foreach (Permissions p in sr.permissions)
                    {
                        List<Objects> lst = getAllObject(p.role);
                        foreach (Objects o in lst)
                        {
                            writer.WriteStartElement("TargetAccess");
                            writer.WriteAttributeString("ID", "TargetAccessPolicy_" + replaceRoleName(getFullRoleName(sr)) + p.op.name);
                            
                            writer.WriteStartElement("RoleList");

                            writer.WriteStartElement("Role");
                            writer.WriteAttributeString("Type", "permisRole");
                            writer.WriteAttributeString("Value", replaceRoleName(getFullRoleName(sr)));
                            writer.WriteEndElement();//Role

                            writer.WriteEndElement();//RoleList

                            writer.WriteStartElement("TargetList");
                            writer.WriteStartElement("Target");

                            writer.WriteStartElement("TargetDomain");
                            writer.WriteAttributeString("ID", o.name);
                            writer.WriteEndElement();//Target Domain

                            writer.WriteStartElement("AllowedAction");
                            writer.WriteAttributeString("ID", p.op.name);
                            writer.WriteEndElement();//AllowedAction
                            
                            writer.WriteEndElement();//Targer
                            writer.WriteEndElement();//TargetList
                            
                            writer.WriteEndElement();//target access
                        }

                    }

                }
            }
            writer.WriteEndElement();//TargetAccessPolicy
            writer.WriteEndElement();//X.509.PMI
            writer.WriteEndDocument();
            writer.Flush();
            writer.Close();
            //return permisXML;
        }

      
#endregion Export to PERMIS

    }
}
