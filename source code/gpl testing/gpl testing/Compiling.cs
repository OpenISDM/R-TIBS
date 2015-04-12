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

namespace GPL_Testing
{

    public class Compiling
    {
        
        public Compiling()
        {
            typeDefs = new List<TypesAndRoles>();
            lstObjectRoles = new List<ObjectRoles>();
            lstSubjectRoles = new List<SubjectRoles>();
            lstSubjects = new List<Subjects>();
            lstObjects = new List<Objects>();
        }

        public List<string> symanticErrors = new List<string>();
        public List<Policy> policies = new List<Policy>();
        protected List<TypesAndRoles> typeDefs = new List<TypesAndRoles>();
        protected List<ObjectRoles> lstObjectRoles = new List<ObjectRoles>();
        protected List<SubjectRoles> lstSubjectRoles = new List<SubjectRoles>();
        protected List<Subjects> lstSubjects = new List<Subjects>();
        protected List<Objects> lstObjects = new List<Objects>();
        protected List<ActionOperator> lstActionOperator = new List<ActionOperator>();
        
       

        public bool doCompile(string source)
        {
            GPLCompiler.GPLCompiling cp = new GPLCompiler.GPLCompiling();
            cp.doCompile(source);

            /*
        
            typeDefs = new List<TypesAndRoles>();
            lstObjectRoles = new List<ObjectRoles>();
            lstSubjectRoles = new List<SubjectRoles>();
            lstSubjects = new List<Subjects>();
            lstObjects = new List<Objects>();
            symanticErrors = new List<string>();
            policies = new List<Policy>();
            lstActionOperator = new List<ActionOperator>();

            NCTU.Grammar grammar = new NCTU.Grammar();
            Parser compiler = new Parser(grammar);
            ParseTree program = compiler.Parse(source);

            string parseXML = program.ToXml();

            if (program == null || compiler.Context.Status.ToString().ToLower() == "error")
            {
                string errLine = compiler.Context.CurrentToken.Location.Line.ToString();
                string errCols = compiler.Context.CurrentToken.Location.Column.ToString();
                string errType = compiler.Context.CurrentToken.Terminal.ToString();
                string errMessage = compiler.Context.CurrentToken.ValueString;
                symanticErrors.Add("Syntax Error: " + errMessage + "\nLine: " + errLine.ToString() + "\n Column: " + errCols);
                return false; //error

            }
            else
            {
                removeComments(program.Tokens);

                int i = 0;
                //
                //main iteration to scan through all tokens and anaylize all classes, objects
                //
                while (i < program.Tokens.Count)
                {
                    string token = program.Tokens[i].ValueString.ToLower();

                    //
                    //get types
                    //
                    int oldi = i;
                    if (token == "typedef")
                    {
                        i = getTypeHierachy(program.Tokens, i);
                    }

                    if (i < program.Tokens.Count)
                    {
                        token = program.Tokens[i].ValueString.ToLower();
                    }
                    //
                    // get interfaces
                    //
                    if (token == "interface")
                    {
                        i = getRoleInterfaces(program.Tokens, ref i);
                    }
                   
                    //
                    //get actions
                    //
                    if (token == "class")
                    {
                        i = getRoleClass(program.Tokens, i);
                      //  i = skipBaseClasses(program.Tokens, i);//dangerous                        
                      
                    }
                    if (oldi == i) i++;
                    

                    if (token == "action")
                    {
                        i = getActions(program.Tokens, i);
                    }
                    
                    if (token == "session")
                    {
                        //reading sessions
                        i += 2; //skip (, ), {
                        while ((token != "}") && (i < program.Tokens.Count))
                        {
                             oldi = i;
                             i = getObjects(program.Tokens, i); if (program.Tokens[i].ValueString == ";") i++; //skip ;                            
                             i = getSubjects(program.Tokens, i); if (program.Tokens[i].ValueString == ";") i++; //skip ;
                             i = updateAssignments(program.Tokens, i); if (program.Tokens[i].ValueString == ";") i++;
                            if (oldi == i) i++;
                            token = getTokenValue(program.Tokens, i);
                        }
                    }
                   
                   

                }
                //translateToPolicies();
                if (symanticErrors.Count == 0)
                {
                    verifyObjectRoles();
                    verifySubjectRoles();
                }

           
                
                return true;
            }
            */
            return true;

        }

#region Outside interaction APIs
        
        public List<string> objectGetAllSubjectAction(string objectName)
        {
            List<ObjectRoles> allORoles = objectGetAllObjectRoles(objectName);
            List<string> Actions = new List<string>();
            if (allORoles != null)
            {
                //
                //getAll Permissions
                //
                List<string> allPermissions = new List<string>();
                foreach (ObjectRoles ooR in allORoles)
                {
                    List<string> aAsso = ooR.getAllPermissions();
                    foreach (string s in aAsso)
                    {
                        allPermissions.Add(s);
                    }
                    List<ObjectRoles> allFathers = getAllRoleAncestor(ooR, lstObjectRoles);
                    if (allFathers != null)
                    {
                        foreach (ObjectRoles f in allFathers)
                        {
                            List<string> aAsso1 = f.getAllPermissions();
                            foreach (string s in aAsso1)
                            {
                                allPermissions.Add(s);
                            }
                        }
                    }
                   
                    
                }
                foreach (string oA in allPermissions)
                {
                    string action = "";
                    string sRole = "";
                    action = seperatePermission(oA, ref sRole);
                    SubjectRoles s = getInstance(sRole, lstSubjectRoles);
                    if (s == null) s = createDummySubjectRole(sRole);

                    string acts = "'" + action + "' ==> ";
                    foreach (SubjectRoles sR in lstSubjectRoles)
                    {
                        if (sR.roleType == rTypes.roleInstantiated)
                        {
                            if ((sR.getName() == sRole) || (isSuRoles(sR, s, lstSubjectRoles)))
                            {
                                foreach (Subjects sj in lstSubjects)
                                {
                                    if ((sj.findSubjectRole(sR.getName()) >= 0))
                                    {
                                        acts += sj.getName() + " & ";

                                    }
                                }
                            }
                        }
                    }
                    Actions.Add(acts.Substring(0, acts.Length - 2));

                   
                }
                return Actions;
            }
            return null;

        }
        public List<string> subjRole_GetAllActionObject(SubjectRoles sr)
        {
            List<string> Actions = new List<string>();
            List<string> allPermissions = sr.getAllPermissions();

            foreach (string sA in allPermissions)
            {
                string action = "";
                string oRole = "";
                action = seperatePermission(sA, ref oRole);
                string acts = action + ",";
                foreach (ObjectRoles oR in lstObjectRoles)
                {
                    if (oR.getName() == oRole)
                    {
                        foreach (Objects oj in lstObjects)
                        {
                            if (oj.findObjectRole(oRole) >= 0)
                            {
                                acts += oj.getName() + ",";

                            }
                        }
                    }
                }
                Actions.Add(acts.Substring(0, acts.Length - 1));
            }
            return Actions;

        }
        public List<string> userGetAllActionObject(string userName)
        {
            List<SubjectRoles> allSRoles = userGetAllSubjectRoles(userName);
            List<string> Actions = new List<string>();
            if (allSRoles != null)
            {
                //
                //getAll Permissions
                //
                List<string> allPermissions = new List<string>();
                foreach (SubjectRoles ssR in allSRoles)
                {
                    List<string> aAsso = ssR.getAllPermissions();
                    foreach (string s in aAsso)
                    {
                        allPermissions.Add(s);
                    }
                    List<SubjectRoles> allFathers = getAllRoleAncestor(ssR, lstSubjectRoles);
                    if (allFathers != null)
                    {
                        foreach (SubjectRoles f in allFathers)
                        {
                            List<string> aAsso1 = f.getAllPermissions();
                            foreach (string s in aAsso1)
                            {
                                allPermissions.Add(s);
                            }
                        }
                    }
                    
                   
                }
                foreach (string sA in allPermissions)
                {
                    string action = "";
                    string oRole = "";
                    action = seperatePermission(sA, ref oRole);
                    ObjectRoles o = getInstance(oRole, lstObjectRoles);
                    if (o == null) o = createDummyObjectRole(oRole);

                    string acts = "'" + action + "' ==> ";
                    foreach (ObjectRoles oR in lstObjectRoles)
                    {
                        if (oR.roleType == rTypes.roleInstantiated)
                        {
                            if ((oR.getName() == oRole) || (isSuRoles(oR, o, lstObjectRoles)))
                            {
                                foreach (Objects oj in lstObjects)
                                {
                                    if ((oj.findObjectRole(oR.getName()) >= 0))
                                    {
                                        acts += oj.getName() + " & ";

                                    }
                                }
                            }
                        }
                    }
                    Actions.Add(acts.Substring(0, acts.Length - 2));
                }
                return Actions;
            }
            return null;
        }
        public List<ObjectRoles> objectGetAllObjectRoles(string objectName)
        {
            foreach (Objects o in lstObjects)
            {
                if (o.getName() == objectName)
                {
                    return o.getAllObjectRoles();

                }
            }
            return null;
        }
        public List<SubjectRoles> userGetAllSubjectRoles(string userName)
        {
            foreach (Subjects s in lstSubjects)
            {
                if (s.getName() == userName)
                {
                    return s.getAllSubjectRoles();

                }
            }
            return null;
        }

#endregion Outside interaction APIs

#region Protected functions
        protected T findSRorORole<T>(string aName, List<T> roleSet) where T : TypesAndRoles
        {
            foreach (T r in roleSet)
            {
                if (r.getName() == aName) return r;
            }
            return null;

        }
        protected T getInstance<T>(string aName, List<T> aSet) where T : TypesAndRoles
        {
            int i = -1;
            foreach (T r in aSet)
            {
                i++;
                if (r.getName() == aName)
                {

                    return r;
                }
            }
            return null;
        }
        protected int checkTypeArgumentsSubjectRole(SubjectRoles r)
        {
            //
            //return 0: OK, 1: unknown type value, 2 : missing argument, 3: wrong type values
            //
            //get farther
            SubjectRoles f = getInstance(r.getAFather(0), lstSubjectRoles);
            if (f.getNumberPararmeters() == r.getNumberPararmeters())
            {
                for (int i = 0; i < f.getNumberPararmeters(); i++)
                {
                    TypesAndRoles p = f.getAParameters(i);
                    if ((p.hasFathers())|| (r.getAParameters(i).roleType == rTypes.parameterStar)) //bounded type parameter
                    {
                        TypesAndRoles pFather = getInstance(p.getAFather(0), typeDefs);
                        if ((r.getAParameters(i).roleType == rTypes.parameterStar) || (pFather.roleType == r.getAParameters(i).roleType))
                        {
                            if ( (pFather != null) && (pFather.roleType == r.getAParameters(i).roleType) )
                            {
                                string rParaName = r.getAParameters(i).getName();
                                string pFatherName = p.getAFather(0);
                                if (!isSubtype(rParaName, pFatherName))
                                {
                                    return 1; //1 = unknown type arguments
                                }
                            }
                        }
                        else
                        {
                            return 3; //wrong type values, should be a type
                        }
                    }
                    else //type parameter, only accept string value
                    {

                        if (r.getAParameters(i).roleType == rTypes.parameterType)
                        {
                            return 4; //wrong value, should be string value
                        }
                    }
                }
                return 0;
            }
            return 2; // missing type argument
        }
        protected int checkTypeArgumentsObjectRole(ObjectRoles r)
        {
            //
            //return 0: OK, 1: unknown type value, 2 : missing argument, 3: wrong type values
            //
            //get farther
            ObjectRoles f = getInstance(r.getAFather(0), lstObjectRoles);
            if (f.getNumberPararmeters() == r.getNumberPararmeters())
            {
                for (int i = 0; i < f.getNumberPararmeters(); i++)
                {
                    TypesAndRoles p = f.getAParameters(i);
                    if ((p.hasFathers()) || (r.getAParameters(i).roleType == rTypes.parameterStar)) //bounded type parameter
                    {
                        TypesAndRoles pFather = getInstance(p.getAFather(0), typeDefs);
                        if ((r.getAParameters(i).roleType == rTypes.parameterStar) || (pFather.roleType == r.getAParameters(i).roleType))
                        {
                            if ( (pFather != null) && (pFather.roleType == r.getAParameters(i).roleType))
                            {
                                string rParaName = r.getAParameters(i).getName();
                                string pFatherName = p.getAFather(0);
                                if (!isSubtype(rParaName, pFatherName))
                                {
                                    return 1; //1 = unknown type arguments
                                }
                            }
                        }
                        else
                        {
                            return 3; //wrong type values
                        }
                    }
                    else //type parameter, only accept string value
                    {
                        if (r.getAParameters(i).roleType == rTypes.parameterType)
                        {
                            return 4; //wrong value, should be string value
                        }
                    }

                }
                return 0;
            }
            return 2; // missing type argument
        }
        protected int getTypeHierachy(Irony.Parsing.TokenList tokenList, int fromIndex)
        {
            int i = fromIndex;
            string token = tokenList[i].ValueString.ToLower();
            while ((i < tokenList.Count) && (token == "typedef"))
            {

                if (tokenList[i].ValueString.ToLower() == "typedef")
                {
                    i++;
                    //get TypeDef
                    int counts = 0;
                    while (tokenList[i].Terminal.ToString().ToLower() == "identifier")
                    {
                        TypesAndRoles tdef = new TypesAndRoles();
                        tdef.setName(tokenList[i].ValueString);
                        tdef.roleType = rTypes.parameterType;

                        if (isInSet(tdef.getName(), typeDefs) < 0)
                        {
                            typeDefs.Add(tdef);
                        }
                        else
                        {
                            symanticErrors.Add("Adding a duplicate type definition: '" + tdef.getName() + "' (Line:" + tokenList[i].Location.Line.ToString() + ", Cols: " + tokenList[i].Location.Column.ToString() + ")");
                        }
                        counts++;
                        i++;
                        if (tokenList[i].ValueString == ",") i++;
                    }

                    if (tokenList[i].ValueString.ToLower() == "extends")
                    {
                        //
                        //looking for father
                        //
                        i++;//skip extends, move to father
                        int j = 0;
                        if (isInSet(tokenList[i].ValueString, typeDefs) >= 0)
                        {
                            for (j = 0; j < counts; j++)
                            {
                                if (!typeDefs[typeDefs.Count - 1 - j].addFather(tokenList[i].ValueString))
                                {
                                    //error: duplicate father;
                                    symanticErrors.Add("Adding a duplicate super-type: '" + tokenList[i].ValueString + "'(Line:" + tokenList[i].Location.Line.ToString() + ", Cols: " + tokenList[i].Location.Column.ToString() + ")");
                                }
                                else
                                {
                                    //update child
                                    typeDefs[typeDefs.Count - 1 - j].roleType = rTypes.parameterType;
                                    for (int m = 0; m < typeDefs.Count; m++)
                                    {
                                        if (typeDefs[m].getName() == tokenList[i].ValueString)
                                        {
                                            typeDefs[m].addChild(typeDefs[typeDefs.Count - 1 - j].getName());
                                            typeDefs[m].roleType = rTypes.parameterType;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            symanticErrors.Add("the type definition: '" + tokenList[i].ValueString + "' is not exist. (Line:" + tokenList[i].Location.Line.ToString() + ", Cols: " + tokenList[i].Location.Column.ToString() + ")");
                        }
                        i++;
                    }
                }
               // i++;
                if (i < tokenList.Count) token = tokenList[i].ValueString.ToLower();
            }
            return i;
        }
        protected int getTypeHierachy1(Irony.Parsing.TokenList tokenList, int fromIndex)
        {
            //
            //update July 14,2012: change structure of type definitions
            //
            int i = fromIndex;
            string token = tokenList[i].ValueString.ToLower();
            while ((i < tokenList.Count) && (tokenList[i].ValueString.ToLower() == "typedef"))
            {

                //get TypeDef
                int counts = 0;
                string fType = "";
                i++;
                while (tokenList[i].Terminal.ToString().ToLower() == "identifier")
                {
                    TypesAndRoles tdef = new TypesAndRoles();
                    tdef.setName(tokenList[i].ValueString);
                    fType = tokenList[i].ValueString;
                    tdef.roleType = rTypes.parameterType;

                    if (isInSet(tdef.getName(), typeDefs) < 0)
                    {
                        typeDefs.Add(tdef);
                    }
                    else
                    {
                        symanticErrors.Add("Adding a duplicate type definition: '" + tdef.getName() + "' (Line:" + tokenList[i].Location.Line.ToString() + ", Cols: " + tokenList[i].Location.Column.ToString() + ")");
                    }
                    counts++;
                    i++;
                    if (tokenList[i].ValueString == ",") i++;
                }

                if (tokenList[i].ValueString.ToLower() == "=")
                {

                    i += 2;//skip =, {

                    while ((i < tokenList.Count) && (tokenList[i].ValueString.ToLower() != "}"))
                    {
                        //get all types and update
                        if (isInSet(tokenList[i].ValueString, typeDefs) < 0)
                        {
                            if (tokenList[i].Terminal.ToString().ToLower() == "identifier")
                            {
                                TypesAndRoles tdef_child = new TypesAndRoles();
                                tdef_child.setName(tokenList[i].ValueString);
                                tdef_child.roleType = rTypes.parameterType;
                                tdef_child.addFather(fType);
                                typeDefs.Add(tdef_child);
                            }
                        }
                        else
                        {
                            symanticErrors.Add("the type definition: '" + tokenList[i].ValueString + "' is already exist. (Line:" + tokenList[i].Location.Line.ToString() + ", Cols: " + tokenList[i].Location.Column.ToString() + ")");
                        }
                        i++;
                    }

                }
                i++;

            }
            return i;
        }
        protected int getRoleClass(Irony.Parsing.TokenList tokenList, int fromIndex)
        {
            //
            //return 1 if get an ObjectRole class
            //return 2 if get a SubjectRole class
            //return 0 if get an other class
            //
            int i = fromIndex;
            string token = getTokenValue(tokenList, i);
            string sClass = getTokenValue(tokenList, i+1);
           
            // parameters
            List<TypesAndRoles> paras = new List<TypesAndRoles>();
            if (token.ToLower() == "class")
            {
                string className = getInstantiatedRoleObjects(tokenList, ref i);
                if (className != "")
                {
                    token = getTokenValue(tokenList, i);
                    if (token.ToLower() == "extends")
                    {
                        string extendedType = checkAndGetInstantiatedRole(tokenList, ref i);
                        if (extendedType == null)
                        {
                            symanticErrors.Add("The extend type is not exists.(Line:" + tokenList[i].Location.Line.ToString() + ", Cols: " + tokenList[i].Location.Column.ToString() + ")");

                        }
                        else
                        {
                            if (isInSet(className, lstSubjectRoles) >= 0)
                            {
                                SubjectRoles sr = getInstance(className, lstSubjectRoles);
                                sr.addFather(extendedType);
                            }
                            else if (isInSet(className, lstObjectRoles) >= 0)
                            {
                                ObjectRoles or = getInstance(className, lstObjectRoles);
                                or.addFather(extendedType);
                            }
                        }
                    }
                    //
                    //get all permission
                    //
                    i++;

                    i = updatePermissions(tokenList, i, className);
                }
                else
                {
                    //skip this class
                    token = getTokenValue(tokenList, i);
                    //symanticErrors.Add("The instatiation of class '"+sClass+"' is incorrect.(Line:" + tokenList[i].Location.Line.ToString() + ", Cols: " + tokenList[i].Location.Column.ToString() + ")");
                    while ((i < tokenList.Count) && (token != "}"))
                    {
                        i++;
                        token = getTokenValue(tokenList, i);
                    }

                }

                return i;

            }


            return -1;
        }
        protected int getSubjects(Irony.Parsing.TokenList tokenList, int fromIndex)
        {
            int i = fromIndex;
            string token = getTokenValue(tokenList, i);
            if (token.ToLower() == "subject")
            {
                i++;
                token = getTokenValue(tokenList, i);
                while ((token != ";") && i < tokenList.Count)
                {
                    string term = getTokenTerminal(tokenList, i);
                    if (term.ToLower() == "identifier")
                    {
                        bool isIn = false;
                        foreach (Subjects s in lstSubjects)
                        {
                            if (s.getName() == token)
                            {
                                isIn = true;
                                break;
                            }
                        }
                        if (!isIn)
                        {
                            Subjects sj = new Subjects();
                            sj.setName(token);
                            lstSubjects.Add(sj);
                        }
                    }
                    i++;
                    token = getTokenValue(tokenList, i);
                }
            }
            return i;
        }
        protected int getObjects(Irony.Parsing.TokenList tokenList, int fromIndex)
        {
            int i = fromIndex;
            string token = getTokenValue(tokenList, i);
            if (token.ToLower() == "object")
            {
                i++;
                token = getTokenValue(tokenList, i);
                while ((token != ";") && i < tokenList.Count)
                {
                    string term = getTokenTerminal(tokenList, i);
                    if (term.ToLower() == "identifier")
                    {
                        bool isIn = false;
                        foreach (Objects o in lstObjects)
                        {
                            if (o.getName() == token)
                            {
                                isIn = true;
                                break;
                            }
                        }
                        if (!isIn)
                        {
                            Objects oj = new Objects();
                            oj.setName(token);
                            lstObjects.Add(oj);
                        }

                    }
                    i++;
                    token = getTokenValue(tokenList, i);
                }
            }
            return i;
        }
        protected int getRoleInterfaces(Irony.Parsing.TokenList tokenList, ref int fromIndex)
        {
            //
            //return 1 if get an ObjectRole class
            //return 2 if get a SubjectRole class
            //return 0 if get an other class
            //
            int i = fromIndex;
            //int isRoleInterface = 0;
            string token = getTokenValue(tokenList, i);
            // parameters
            List<TypesAndRoles> paras = new List<TypesAndRoles>();
            if (token.ToLower() == "interface")
            {
                i++;
                //it is objectRole class or subjectrole class
                string roleClassName = tokenList[i].ValueString;
                i++;
                token = getTokenValue(tokenList, i);
                if (token == "<")
                {


                    i++;
                    token = getTokenValue(tokenList, i);
                    string term = getTokenTerminal(tokenList, i);


                    while ((term == "identifier") && (token != ">") && (i < tokenList.Count))
                    {
                        TypesAndRoles para = new TypesAndRoles();
                        para.setName(token);
                        para.roleType = rTypes.parameterValue;
                        i++;
                        token = getTokenValue(tokenList, i);
                        if (token.ToLower() == "in")
                        {
                            i++; //move to father of para
                            token = getTokenValue(tokenList, i);
                            if (isInSet(token, typeDefs) >= 0)
                            {
                                para.addFather(token);
                                para.roleType = rTypes.parameterType;
                            }
                            else
                            {
                                symanticErrors.Add("The type: '" + token + "' is not exists.(Line:" + tokenList[i].Location.Line.ToString() + ", Cols: " + tokenList[i].Location.Column.ToString() + ")");
                            }
                            i++; //move to , or >
                            token = getTokenValue(tokenList, i);
                        }
                        if (token == ",") i++;
                        paras.Add(para);
                        //roleClass.addParameters(para);                       
                        token = getTokenValue(tokenList, i);
                        term = getTokenTerminal(tokenList, i);

                    }
                    //
                    //checking is extend ObjectRole / SubjectRole
                    //
                    if (token == ">")
                    {
                        i++;

                    }
                }
                token = getTokenValue(tokenList, i);
                if (token.ToLower() == "extends")
                {
                    i++;
                    token = getTokenValue(tokenList, i);
                    if (token.ToLower() == "objectrole")
                    {
                        ObjectRoles oR = new ObjectRoles();
                        oR.roleType = rTypes.roleDefinition;
                        oR.setName(roleClassName);
                        oR.addFather("ObjectRole");
                        foreach (TypesAndRoles p in paras)
                        {
                            oR.addParameters(p);
                        }
                        lstObjectRoles.Add(oR);
                        //isRoleInterface = 1;
                    }
                    else if (token.ToLower() == "subjectrole")
                    {
                        SubjectRoles sR = new SubjectRoles();
                        sR.roleType = rTypes.roleDefinition;
                        sR.setName(roleClassName);
                        sR.addFather("SubjectRole");
                        foreach (TypesAndRoles p in paras)
                        {
                            sR.addParameters(p);
                        }
                        lstSubjectRoles.Add(sR);
                        //isRoleInterface = 2;
                    }
                }
                else
                {
                    //other class;
                   // isRoleInterface = 0;
                }
                //
                //skip class body
                //
                i++;
                token = getTokenValue(tokenList, i).ToLower(); ;
                while ((token != "interface") && (token != "class") && (token != "typedef") && (i < tokenList.Count))
                {
                    i++;
                    token = getTokenValue(tokenList, i).ToLower();
                }
                fromIndex = i;
                //return isRoleInterface;
                return i;

            }


            return 0;
        }
        protected int getActions(Irony.Parsing.TokenList tokenList, int fromIndex)
        {

            int i = fromIndex;
            string token = getTokenValue(tokenList, i-1);
            ActionOperator op = new ActionOperator();
            op.name = getTokenValue(tokenList, i);
            
            if (token.ToLower() == "action")
            {
                //skip ( 
                i++;

                op.leftOperand = checkAndGetInstantiatedRole(tokenList, ref i);               
                op.rightOperand = checkAndGetInstantiatedRole(tokenList, ref i);
                if ((op.leftOperand != null)&&(isInSet(getTypeAncestor(op.leftOperand), lstSubjectRoles) < 0))
                {
                    symanticErrors.Add("[action "+op.name+"] Left operand: '" + op.leftOperand + "' is not a subject role. (Line:" + tokenList[i].Location.Line.ToString() + ", Cols: " + tokenList[i].Location.Column.ToString() + ")");
                }
                if ((op.rightOperand!=null)&&(isInSet(getTypeAncestor(op.rightOperand), lstObjectRoles) < 0))
                {
                    symanticErrors.Add("[action " + op.name + "] Right operand: '" + op.rightOperand + "' is not a subject role. (Line:" + tokenList[i].Location.Line.ToString() + ", Cols: " + tokenList[i].Location.Column.ToString() + ")");
                }

                //skip )
                i++;


            }
            lstActionOperator.Add(op);


            return i;
        } 
        protected int isInObjectSet(string objectName)
        {
            int i = 0;
            for (i = 0; i < lstObjects.Count; i++)
            {
                if (lstObjects[i].getName() == objectName)
                {
                    return i;
                }
            }
            return -1;
        }
        protected int isInSet<T>(string aName, List<T> aSet) where T : TypesAndRoles
        {
            int i = -1;
            foreach (T r in aSet)
            {
                i++;
                if (r.getName() == aName)
                {

                    return i;
                }
            }
            return -1;
        }
        protected int isInActionSet(string aName)
        {
            int i = -1;
            foreach (ActionOperator o in lstActionOperator)
            {
                i++;
                if (o.name == aName) return i;
            }
            return -1;
        }
        protected int isInSubjectSet(string subjectName)
        {
            int i = 0;
            for (i = 0; i < lstSubjects.Count; i++)
            {
                if (lstSubjects[i].getName() == subjectName)
                {
                    return i;
                }
            }
            return -1;
        }       
        protected int roleInstantiationAndObject<T>(Irony.Parsing.TokenList tokenList, ref T instantiatedRole, ref T roleObject, int fromIndex) where T : TypesAndRoles
        {
            int i = fromIndex;
            string token = getTokenValue(tokenList, i);
            string term = getTokenTerminal(tokenList, i);

            string aRoleName = token;

            i++;
            token = getTokenValue(tokenList, i);
            string sParas = "";
            if (token == "<")
            {
                //
                //get parameters' values
                //
                sParas += "<";
                i++;
                token = getTokenValue(tokenList, i);
                term = getTokenTerminal(tokenList, i);
                while (((term == "identifier") || (term.ToLower() == "string") || (term.ToLower() == "*")) && (token != ">") && (i < tokenList.Count))
                {
                    TypesAndRoles para = new TypesAndRoles();
                    //
                    //chec type variables
                    //
                    para.setName(token);
                    if (term == "identifier") para.roleType = rTypes.parameterType;
                    if (term.ToLower() == "string")
                    {
                        if (token == "*")
                        {
                            //para.roleType = rTypes.parameterStar;
                            //raise erro
                            symanticErrors.Add("String \"*\" is not allowed. Please use * instead of string \"*\". (Line:" + tokenList[i].Location.Line.ToString() + ", Cols: " + tokenList[i].Location.Column.ToString() + ")");
                        }
                        else
                        {
                            para.roleType = rTypes.parameterValue;
                        }
                    }
                    if (term.ToLower() == "*")
                    {
                        para.roleType = rTypes.parameterStar;
                    }

                    sParas += token;
                    i++;
                    token = getTokenValue(tokenList, i);

                    if (token == ",")
                    {
                        i++;
                        sParas += ",";
                    }
                    instantiatedRole.addParameters(para);
                    token = getTokenValue(tokenList, i);
                    term = getTokenTerminal(tokenList, i);
                }
                sParas += ">";
                i++; //skip >
                instantiatedRole.roleType = rTypes.roleInstantiated;
                instantiatedRole.addFather(aRoleName);

                aRoleName += sParas;
                instantiatedRole.setName(aRoleName);

            }
            else instantiatedRole = null;

            token = getTokenValue(tokenList, i);
            term = getTokenTerminal(tokenList, i);
            if ((term.ToLower() == "identifier") || (term.ToLower() == "string"))
            {
                roleObject.roleType = rTypes.roleInstance;
                roleObject.setName(token);
                roleObject.addFather(aRoleName);
                i++;
            }
            else roleObject = null;
            return i;
        }
        protected int skipBaseClasses(Irony.Parsing.TokenList tokenList, int fromIndex)
        {
            int i = fromIndex;
            string token = tokenList[i].ValueString.ToLower();
            while ((token == "class") && (i < tokenList.Count) && ((tokenList[i + 1].ValueString.ToLower() == "object") || (tokenList[i + 1].ValueString.ToLower() == "subject")))
            {
                //skiping all inside of this class
                i++;
                token = tokenList[i].ValueString.ToLower();
                while ((token != "class") && (i < tokenList.Count))
                {
                    i++;
                    token = tokenList[i].ValueString.ToLower();
                }
            }
            return i;
        }       
        protected int updatePermissions(Irony.Parsing.TokenList tokenList, int fromIndex, string forClass)
        {

           
            int i = fromIndex;
            string token = getTokenValue(tokenList, i);
            string term = getTokenTerminal(tokenList, i);
            //string nextTwoTokens = getTokenValue(tokenList, i + 2);
            while ((term.ToLower() == "permission") && (token.ToLower() == "permission"))
            {
                
                SubjectRoles sr = null;
                ObjectRoles or = null;

                if (isInSet(forClass, lstSubjectRoles) >= 0)
                {
                    sr = getInstance(forClass, lstSubjectRoles);
                    i+=2; //skip (
                    //get action
                    token = getTokenValue(tokenList, i);
                    i += 1; //skip ,
                    string objRole = checkAndGetInstantiatedRole(tokenList, ref i);
                    sr.addPermission(token, objRole);


                }
                else if (isInSet(forClass, lstObjectRoles) >= 0)
                {
                    or = getInstance(forClass, lstObjectRoles);
                    i += 1;
                    string sbjRole = checkAndGetInstantiatedRole(tokenList, ref i);
                    i += 1; //skip ,
                    token = getTokenValue(tokenList, i);
                    or.addPermission(sbjRole, token);
                }
                
                while ((token != "permission") && (token != "}") && (i < tokenList.Count))
                {
                    i++;
                    token = getTokenValue(tokenList, i);
                }

                token = getTokenValue(tokenList, i);
                term = getTokenTerminal(tokenList, i);


            }
            return i;
        }
        protected int updateAssignments(Irony.Parsing.TokenList tokenList, int fromIndex)
        {
            int i = fromIndex;
            string token = getTokenValue(tokenList, i);
            string term = getTokenTerminal(tokenList, i);
            string nextToken = getTokenValue(tokenList, i + 1);
            if ((term == "identifier") && ((nextToken == "=") || (nextToken == ":=")))
            {
                //
                //rememeber to do type-check
                //


                int iSRole = isInSubjectSet(token);
                if (iSRole >= 0)
                {
                    //the left side is an subject(s)
                    List<string> sbj = new List<string>();
                    sbj.Add(token);
                    i++;
                    token = getTokenValue(tokenList, i);
                    while ((token == "=") && (i < tokenList.Count))
                    {
                        i++;
                        token = getTokenValue(tokenList, i);
                        if (isInSubjectSet(token) >= 0)
                        {
                            sbj.Add(token);
                            i++;
                            token = getTokenValue(tokenList, i);
                        }
                        else
                        {
                            //error: assign two (or more) different types (subject, object)
                            symanticErrors.Add("Assign two (or more) different types (subject, object)(Line:" + tokenList[i].Location.Line.ToString() + ", Cols: " + tokenList[i].Location.Column.ToString() + ")");
                        }
                    }
                    if (token == ":=")
                    {
                        
                        while (token != ";" && i < tokenList.Count)                        {

                            string sRole = checkAndGetInstantiatedRole(tokenList, ref i);
                            if ((sRole != null) && (isInSet(sRole, lstSubjectRoles) >= 0))
                            {
                                for (int j = 0; j < sbj.Count; j++)
                                {
                                    int k = isInSubjectSet(sbj[j]);
                                    lstSubjects[k].addSubjectRole(getInstance(sRole, lstSubjectRoles));
                                }
                            }
                            else
                            {
                                if (isInSet(sRole, lstObjectRoles) >= 0)
                                {
                                    symanticErrors.Add("Cannot assign an object-role: '" + sRole + "' to a subject. (Line:" + tokenList[i].Location.Line.ToString() + ", Cols: " + tokenList[i].Location.Column.ToString() + ")");
                                }
                                else
                                {
                                    //error: assign another roles (not a subjectRole) to a subject
                                    symanticErrors.Add("Assign: '" + sRole + "' which is not a SubjectRole to a subject. (Line:" + tokenList[i].Location.Line.ToString() + ", Cols: " + tokenList[i].Location.Column.ToString() + ")");
                                }
                            }
                            token = getTokenValue(tokenList, i );
                        }

                        //    i++;
                        //    token = getTokenValue(tokenList, i);
                        //    int isSRole = isInSet<SubjectRoles>(token, lstSubjectRoles);
                        //    if (isSRole >= 0)
                        //    {
                        //        for (int j = 0; j < sbj.Count; j++)
                        //        {
                        //            int k = isInSubjectSet(sbj[j]);
                        //            lstSubjects[k].addSubjectRole(lstSubjectRoles[isSRole]);
                        //        }
                        //    }
                        //    else
                        //    {
                        //        //error: assign another roles (not a subjectRole) to a subject
                        //        symanticErrors.Add("Assign: '" + token + "' which is not a SubjectRole to a subject. (Line:" + tokenList[i].Location.Line.ToString() + ", Cols: " + tokenList[i].Location.Column.ToString() + ")");
                        //    }
                    }
                    i++;
                }
                else
                {
                    int iORole = isInObjectSet(token);

                    if (iORole >= 0)
                    {
                        //the left side is an subject(s)
                        List<string> obj = new List<string>();
                        obj.Add(token);
                        i++;
                        token = getTokenValue(tokenList, i);
                        while ((token == "=") && (i < tokenList.Count))
                        {
                            i++;
                            token = getTokenValue(tokenList, i);
                            if (isInObjectSet(token) >= 0)
                            {
                                obj.Add(token);
                                i++;
                                token = getTokenValue(tokenList, i);
                            }
                            else
                            {
                                //error: assign two (or more) different types (subject, object)
                                symanticErrors.Add("Assign two (or more) different types (subject, object) (Line:" + tokenList[i].Location.Line.ToString() + ", Cols: " + tokenList[i].Location.Column.ToString() + ")");
                            }
                        }
                        if (token == ":=")
                        {
                            string oRole = checkAndGetInstantiatedRole(tokenList, ref i);
                            if ((oRole != null) && isInSet(oRole, lstObjectRoles)>=0)
                            {
                                for (int j = 0; j < obj.Count; j++)
                                {
                                    int k = isInObjectSet(obj[j]);
                                    lstObjects[k].addObjectRole(getInstance(oRole, lstObjectRoles));
                                }
                            }
                            else
                            {
                                //error: assign another roles (not ObjectRole) to an object
                                if (isInSet(oRole, lstSubjectRoles) >= 0)
                                {
                                    symanticErrors.Add("Cannot assign a subject-role: '" + oRole + "' to an object. (Line:" + tokenList[i].Location.Line.ToString() + ", Cols: " + tokenList[i].Location.Column.ToString() + ")");
                                }
                                else
                                {
                                    symanticErrors.Add("Assign: '" + oRole + "' which is not ObjectRole to an object. (Line:" + tokenList[i].Location.Line.ToString() + ", Cols: " + tokenList[i].Location.Column.ToString() + ")");
                                }
                            }
                        
                        }
                        i++;
                    }
                    else
                    {
                        //error: assign another roles (not ObjectRole) to an object
                        symanticErrors.Add(token + " is unknown . (Line:" + tokenList[i].Location.Line.ToString() + ", Cols: " + tokenList[i].Location.Column.ToString() + ")");
                    }

                }
            }

            return i;

        }
        
        protected bool isSuRoles<T>(T r1, T r2, List<T> roleList) where T : TypesAndRoles
        {
            //
            //r1 is subtype of r2 ????
            //

            //
            //check direct from inheritance graph
            //
            List<T> allFathers = getAllRoleAncestor(r1, roleList);
            foreach (T r in allFathers)
            {
                if (r.getName() == r2.getName()) return true;
            }


            //
            //and then check star
            //
            if (getTypeAncestor(r1.getName()) == getTypeAncestor(r2.getName()))
            {
                if ((r2.getNumberPararmeters() == 0) && (r1.getNumberPararmeters() > 0)) return true;
                if (r1.getNumberPararmeters() == r2.getNumberPararmeters())
                {
                    int count = 0;
                    for (int i = 0; i < r1.getNumberPararmeters(); i++)
                    {
                        TypesAndRoles pr1 = r1.getAParameters(i);
                        TypesAndRoles pr2 = r2.getAParameters(i);

                        //if ((pr1.roleType == rTypes.parameterType) || (pr2.roleType == rTypes.parameterStar))
                        //{
                        if (pr2.roleType == pr1.roleType)
                        {
                            if (!isSubtype(pr1.getName(), pr2.getName()))
                            {
                                TypesAndRoles fpr1 = findType(pr1.getName());
                                TypesAndRoles fpr2 = findType(pr2.getName());
                                if ((pr1.roleType == rTypes.parameterType) && (fpr2.getAFather(0) != fpr1.getAFather(0)))
                                    return false;
                                else
                                    if (pr2.roleType != rTypes.parameterStar) count--;
                            }

                        }
                        else
                        {
                            if (pr2.getName() != "*") return false;
                        }
                        //}
                        count++;
                    }
                    return count >= 0;
                }
            }


            return false; ;
        }
        protected bool isSubtype(string subT, string supperT)
        {
            //find the supper type

            if (supperT == "*")
            {
                if (subT != "*") return true;
                else return false;
            }


            TypesAndRoles supperType = findType(supperT);
            TypesAndRoles subType = findType(subT);
            try
            {
                subType = findType(subType.getAFather(0));
                while (subType != null)
                {
                    if (supperType == subType) return true;
                    subType = findType(subType.getAFather(0));
                }

            }
            catch
            {
                return false;
            }
            return false;
        }
        protected bool verifySubjectRoles()
        {
            //
            //read all roles' permissions, check the action --> is it correct, check the instatiation of roles
            //
            foreach (SubjectRoles r in lstSubjectRoles)
            {
                if (r.roleType == rTypes.roleInstantiated)
                {
                    //read all permissions
                    List<string> ps = r.getAllPermissions();

                    foreach (string s in ps)
                    {
                        string action = "";
                        string oRoleName = role_inPermission(s, ref action);
                        ObjectRoles or = createDummyObjectRole(oRoleName);

                        if (isInSet(getTypeAncestor(or.getName()), lstObjectRoles) < 0)
                        { //kiem tra xem oRole co phai la object-role hay khong
                            symanticErrors.Add("[class " + r.getName() + "] Role: '" + or.getName() + "' is not an object-role. " + r.getName() + " (subject-role)'s permission should be assigned with an object-role. ");
                        }
                        //
                        //kiem tra action
                        //
                        int iAction = isInActionSet(action);
                        if (iAction >= 0)
                        {
                            ActionOperator ac = lstActionOperator[iAction];
                            //SubjectRoles left = getInstance(ac.leftOperand, lstSubjectRoles) ;
                            SubjectRoles left = createDummySubjectRole(ac.leftOperand);
                            ObjectRoles right = createDummyObjectRole(ac.rightOperand);

                            if (!isSuRoles(r, left, lstSubjectRoles))
                            {
                                //error: the action is not compatable with the subject role
                                symanticErrors.Add("[class " + r.getName() + "] Action: " + action + " is not compatable with the subject role: '" + r.getName() + "'. ");
                            }
                            if (!isSuRoles(or, right, lstObjectRoles))
                            {
                                //error: the action is not comparable with the object role
                                symanticErrors.Add("[class " + r.getName() + "] Action: " + action + " is not compatable with the object role: '" + or.getName() + "'.");
                            }



                        }
                        else
                        {
                            symanticErrors.Add("[class " + r.getName() + "] Action: '" + action + "' is not exits. ");
                        }


                    }
                }
            }
            return false;
        }
        protected bool verifyObjectRoles()
        {
            //
            //read all roles' permissions, check the action --> is it correct, check the instatiation of roles
            //
            foreach (ObjectRoles r in lstObjectRoles)
            {
                //read all permissions
                if (r.roleType == rTypes.roleInstantiated)
                {
                    List<string> ps = r.getAllPermissions();

                    foreach (string s in ps)
                    {
                        string action = "";
                        string sRoleName = role_inPermission(s, ref action);
                        SubjectRoles sr = createDummySubjectRole(sRoleName);

                        if (isInSet(getTypeAncestor(sr.getName()), lstSubjectRoles) < 0)
                        { //kiem tra xem oRole co phai la object-role hay khong
                            symanticErrors.Add("[class " + r.getName() + "] Role: '" + sr.getName() + "' is not an object-role. " + r.getName() + " (subject-role)'s permission should be assigned with an object-role. ");
                        }
                        //
                        //kiem tra action
                        //
                        int iAction = isInActionSet(action);
                        if (iAction >= 0)
                        {
                            ActionOperator ac = lstActionOperator[iAction];
                            //SubjectRoles left = getInstance(ac.leftOperand, lstSubjectRoles) ;
                            SubjectRoles left = createDummySubjectRole(ac.leftOperand);
                            ObjectRoles right = createDummyObjectRole(ac.rightOperand);

                            if (!isSuRoles(r, right, lstObjectRoles))
                            {
                                //error: the action is not compatable with the subject role
                                symanticErrors.Add("[clas " + r.getName() + "] Action: " + action + " is not compatable with the object role: '" + r.getName() + "'. ");
                            }
                            if (!isSuRoles(sr, left, lstSubjectRoles))
                            {
                                //error: the action is not comparable with the object role
                                symanticErrors.Add("[class " + r.getName() + "] Action: " + action + " is not compatable with the subject role: '" + sr.getName() + "'.");
                            }



                        }
                        else
                        {
                            symanticErrors.Add("[clas " + r.getName() + "] Action: '" + action + "' is not exits. ");
                        }


                    }
                }
            }
            return false;
        }
        protected bool verifyAction()
        {
            //
            //must be excuted before verify roles
            //
            foreach (ActionOperator ac in lstActionOperator)
            {

            }
            return false;
        }

        protected void removeComments(Irony.Parsing.TokenList tokenList)
        {
            int i = 0;
            string token = tokenList[i].Terminal.ToString().ToLower();
            while (i < tokenList.Count)
            {
                token = tokenList[i].Terminal.ToString().ToLower();
                if ((token == "line-comment") || (token == "block-comment"))
                {
                    tokenList.RemoveAt(i);
                    i--;
                }
                i++;

            }
        }
        
        protected string getTokenValue(Irony.Parsing.TokenList tokenList, int i)
        {
            if (i < tokenList.Count)
            {
                return tokenList[i].ValueString;
            }
            return null;
        }
        protected string getTokenTerminal(Irony.Parsing.TokenList tokenList, int i)
        {
            if (i < tokenList.Count)
            {
                return tokenList[i].Terminal.ToString().ToLower();
            }
            else return null;
        }
        protected string getInstantiatedRoleObjects(Irony.Parsing.TokenList tokenList, ref int i)
        {
            i++;
            string instantiatedClassName = "";
            string token = getTokenValue(tokenList, i);
            string term = getTokenTerminal(tokenList, i);
            if (term == "identifier")
            {
                string nextToken = getTokenValue(tokenList, i + 1);
                string nextTerm = getTokenTerminal(tokenList, i + 1);
                if ((nextToken == "<") || (nextTerm == "identifier"))
                {
                    //exactly is subjectRole or objectRole declaration
                    if (isInSet<SubjectRoles>(token, lstSubjectRoles) >= 0)
                    {
                        SubjectRoles anInstantiatedRole = new SubjectRoles();
                        SubjectRoles anRoleObject = new SubjectRoles();

                        i = roleInstantiationAndObject<SubjectRoles>(tokenList, ref anInstantiatedRole, ref anRoleObject, i);


                        if ((anInstantiatedRole != null) || (anRoleObject != null))
                        {
                            if (anInstantiatedRole == null)
                            {
                                if (anRoleObject != null)
                                {

                                    lstSubjectRoles.Add(anRoleObject);
                                    instantiatedClassName = anRoleObject.getName();
                                }
                            }
                            else
                            {
                                SubjectRoles f = getInstance(anInstantiatedRole.getAFather(0), lstSubjectRoles);
                                int check = checkTypeArgumentsSubjectRole(anInstantiatedRole);
                                if (check == 0)
                                {
                                    //
                                    //check type variables
                                    //

                                    lstSubjectRoles.Add(anInstantiatedRole);
                                    instantiatedClassName = anInstantiatedRole.getName();
                                    //lstSubjectRoles.Add(anRoleObject);
                                }
                                else
                                {
                                    if (check == 1)
                                    {
                                        symanticErrors.Add("The instantiation of role " + f.getName() + " is incorrect. There is an incorrect parameter value.  (Line:" + tokenList[i].Location.Line.ToString() + ", Cols: " + tokenList[i].Location.Column.ToString() + ")");
                                    }
                                    if (check == 2)
                                    {
                                        //missing parameters values
                                        symanticErrors.Add("Lack of/excess parameter values. The subject role instantiation requires: " + f.getNumberPararmeters() + " parameter values while " + anInstantiatedRole.getName() + " provides: " + anInstantiatedRole.getNumberPararmeters() + " values. (Line:" + tokenList[i].Location.Line.ToString() + ", Cols: " + tokenList[i].Location.Column.ToString() + ")");
                                    }
                                    if (check == 3)
                                    {
                                        symanticErrors.Add("The instantiation of role " + f.getName() + " is incorrect. The type argument should be a type, not a string/value.  (Line:" + tokenList[i].Location.Line.ToString() + ", Cols: " + tokenList[i].Location.Column.ToString() + ")");
                                    }
                                    if (check == 4)
                                    {
                                        symanticErrors.Add("The instantiation of role " + f.getName() + " is incorrect. The type argument should be a string/value, not a type.  (Line:" + tokenList[i].Location.Line.ToString() + ", Cols: " + tokenList[i].Location.Column.ToString() + ")");
                                    }
                                }
                            }
                        }
                        else
                        {
                            //error duplicate subjcet role instantiation
                            symanticErrors.Add("The subject role: '" + anInstantiatedRole.getName() + "' already exist. (Line:" + tokenList[i].Location.Line.ToString() + ", Cols: " + tokenList[i].Location.Column.ToString() + ")");

                        }

                    }
                    if (isInSet<ObjectRoles>(token, lstObjectRoles) >= 0)
                    {
                        ObjectRoles anInstantiatedRole = new ObjectRoles();
                        ObjectRoles anRoleObject = new ObjectRoles();
                        i = roleInstantiationAndObject<ObjectRoles>(tokenList, ref anInstantiatedRole, ref anRoleObject, i);



                        if ((anInstantiatedRole != null) || (anRoleObject != null))
                        {
                            if (anInstantiatedRole == null)
                            {
                                if (anRoleObject != null)
                                {
                                    lstObjectRoles.Add(anRoleObject);
                                    instantiatedClassName = anRoleObject.getName();
                                }
                            }
                            else
                            {
                                ObjectRoles o = getInstance(anInstantiatedRole.getAFather(0), lstObjectRoles);
                                int check = checkTypeArgumentsObjectRole(anInstantiatedRole);
                                if (check == 0)
                                {
                                    lstObjectRoles.Add(anInstantiatedRole);
                                    instantiatedClassName = anInstantiatedRole.getName();
                                    // lstObjectRoles.Add(anRoleObject);
                                }
                                else
                                {
                                    if (check == 1)
                                    {
                                        symanticErrors.Add("The instantiation of role " + o.getName() + " is incorrect. There is an incorrect parameter value.  (Line:" + tokenList[i].Location.Line.ToString() + ", Cols: " + tokenList[i].Location.Column.ToString() + ")");
                                    }
                                    if (check == 2)
                                    {
                                        //missing parameters values
                                        symanticErrors.Add("Lack of/excess parameter values. The object role instantiation requires: " + o.getNumberPararmeters() + " parameter values while " + anInstantiatedRole.getName() + " provides: " + anInstantiatedRole.getNumberPararmeters() + " values. (Line:" + tokenList[i].Location.Line.ToString() + ", Cols: " + tokenList[i].Location.Column.ToString() + ")");
                                    }
                                    if (check == 3)
                                    {
                                        symanticErrors.Add("The instantiation of role " + o.getName() + " is incorrect. The type argument should be a type, not a string/value.  (Line:" + tokenList[i].Location.Line.ToString() + ", Cols: " + tokenList[i].Location.Column.ToString() + ")");
                                    }
                                    if (check == 4)
                                    {
                                        symanticErrors.Add("The instantiation of role " + o.getName() + " is incorrect. The type argument should be a string/value, not a type.  (Line:" + tokenList[i].Location.Line.ToString() + ", Cols: " + tokenList[i].Location.Column.ToString() + ")");
                                    }
                                }
                            }
                        }
                        else
                        {
                            //error duplicate object role instantiation
                            symanticErrors.Add("The object role: '" + anInstantiatedRole.getName() + "' already exist. (Line:" + tokenList[i].Location.Line.ToString() + ", Cols: " + tokenList[i].Location.Column.ToString() + ")");
                        }

                    }

                }

            }
            return instantiatedClassName;

        }
        protected string replaceRoleName(string aRoleName)
        {
            aRoleName = aRoleName.Replace('<', '_');
            aRoleName = aRoleName.Replace(">", "");
            aRoleName = aRoleName.Replace(',', '_');
            return aRoleName;
        }
        protected string role_inPermission(string s, ref string action)
        {
            string oRole = "";
            action = seperatePermission(s, ref oRole);
            return oRole;
        }
        protected string seperatePermission(string permission, ref string part2)
        {
            string[] a = permission.Split('@');
            part2 = a[1];
            return a[0];

        }
        protected string[] sepeartedActionObject(string actionObject)
        {
            return actionObject.Split(',');

        }
        protected string checkAndGetInstantiatedRole(Irony.Parsing.TokenList tokenList, ref int i)
        {
            i++;
            string token = getTokenValue(tokenList, i);
            string term = getTokenTerminal(tokenList, i);
            if (term == "identifier")
            {
                string nextToken = getTokenValue(tokenList, i + 1);
                string nextTerm = getTokenTerminal(tokenList, i + 1);
                if ((nextToken == "<") || (nextTerm == "identifier"))
                {
                    //exactly is subjectRole or objectRole declaration
                    if (isInSet<SubjectRoles>(token, lstSubjectRoles) >= 0)
                    {
                        SubjectRoles anInstantiatedRole = new SubjectRoles();
                        SubjectRoles anRoleObject = new SubjectRoles();

                        i = roleInstantiationAndObject<SubjectRoles>(tokenList, ref anInstantiatedRole, ref anRoleObject, i);


                        if ((anInstantiatedRole != null) || (anRoleObject!= null))
                        {
                            if (anInstantiatedRole == null)
                            {
                                //lstSubjectRoles.Add(anRoleObject);
                                if (anRoleObject != null)
                                {
                                    return anRoleObject.getName();
                                }
                            }
                            else
                            {
                                SubjectRoles f = getInstance(anInstantiatedRole.getAFather(0), lstSubjectRoles);
                                int check = checkTypeArgumentsSubjectRole(anInstantiatedRole);
                                if (check == 0)
                                {
                                    //
                                    //check type variables
                                    //
                                    return anInstantiatedRole.getName();

                                }
                                else
                                {
                                    if (check == 1)
                                    {
                                        symanticErrors.Add("The instantiation of role " + f.getName() + " is incorrect. There is an incorrect parameter value.  (Line:" + tokenList[i].Location.Line.ToString() + ", Cols: " + tokenList[i].Location.Column.ToString() + ")");
                                    }
                                    if (check == 2)
                                    {
                                        //missing parameters values
                                        symanticErrors.Add("Lack of/excess parameter values. The subject role instantiation requires: " + f.getNumberPararmeters() + " parameter values while " + anInstantiatedRole.getName() + " provides: " + anInstantiatedRole.getNumberPararmeters() + " values. (Line:" + tokenList[i].Location.Line.ToString() + ", Cols: " + tokenList[i].Location.Column.ToString() + ")");
                                    }
                                    if (check == 3)
                                    {
                                        symanticErrors.Add("The instantiation of role " + f.getName() + " is incorrect. The type argument should be a type, not a string/value.  (Line:" + tokenList[i].Location.Line.ToString() + ", Cols: " + tokenList[i].Location.Column.ToString() + ")");
                                    }
                                    if (check == 4)
                                    {
                                        symanticErrors.Add("The instantiation of role " + f.getName() + " is incorrect. The type argument should be a string/value, not a type.  (Line:" + tokenList[i].Location.Line.ToString() + ", Cols: " + tokenList[i].Location.Column.ToString() + ")");
                                    }
                                    return null;
                                }
                            }
                        }

                    }
                    else  if (isInSet<ObjectRoles>(token, lstObjectRoles) >= 0)
                    {
                        ObjectRoles anInstantiatedRole = new ObjectRoles();
                        ObjectRoles anRoleObject = new ObjectRoles();
                        i = roleInstantiationAndObject<ObjectRoles>(tokenList, ref anInstantiatedRole, ref anRoleObject, i);



                        if ((anInstantiatedRole != null) || (anRoleObject!= null))
                        {
                            if (anInstantiatedRole == null)
                            {
                                if (anRoleObject != null)
                                {
                                    return anRoleObject.getName();
                                }
                            }
                            else
                            {
                                ObjectRoles o = getInstance(anInstantiatedRole.getAFather(0), lstObjectRoles);
                                int check = checkTypeArgumentsObjectRole(anInstantiatedRole);
                                if (check == 0)
                                {

                                    return anInstantiatedRole.getName();
                                }
                                else
                                {
                                    if (check == 1)
                                    {
                                        symanticErrors.Add("The instantiation of role " + o.getName() + " is incorrect. There is an incorrect parameter value.  (Line:" + tokenList[i].Location.Line.ToString() + ", Cols: " + tokenList[i].Location.Column.ToString() + ")");
                                    }
                                    if (check == 2)
                                    {
                                        //missing parameters values
                                        symanticErrors.Add("Lack of/excess parameter values. The object role instantiation requires: " + o.getNumberPararmeters() + " parameter values while " + anInstantiatedRole.getName() + " provides: " + anInstantiatedRole.getNumberPararmeters() + " values. (Line:" + tokenList[i].Location.Line.ToString() + ", Cols: " + tokenList[i].Location.Column.ToString() + ")");
                                    }
                                    if (check == 3)
                                    {
                                        symanticErrors.Add("The instantiation of role " + o.getName() + " is incorrect. The type argument should be a type, not a string/value.  (Line:" + tokenList[i].Location.Line.ToString() + ", Cols: " + tokenList[i].Location.Column.ToString() + ")");
                                    }
                                    if (check == 4)
                                    {
                                        symanticErrors.Add("The instantiation of role " + o.getName() + " is incorrect. The type argument should be a string/value, not a type.  (Line:" + tokenList[i].Location.Line.ToString() + ", Cols: " + tokenList[i].Location.Column.ToString() + ")");
                                    }
                                    return null;
                                }
                            }
                        }


                    }

                }
                else
                {
                    //
                    //star star star
                    //
                   // token += "<*,*,*>";
                    if (isInSet<SubjectRoles>(token, lstSubjectRoles) >= 0)
                    {
                        SubjectRoles sr = getInstance(token, lstSubjectRoles);
                        token += "<";
                        for (int k = 0; k < sr.getNumberPararmeters()-1; k++)
                        {
                            token += "*,";
                        }
                        token += "*>";
                        
                    }
                    else if (isInSet<ObjectRoles>(token, lstObjectRoles) >= 0)
                    {
                        ObjectRoles or = getInstance(token, lstObjectRoles);
                        //SubjectRoles fSR = getInstance(sr.getAFather(0), lstSubjectRoles);
                        token += "<";
                        for (int k = 0; k < or.getNumberPararmeters() - 1; k++)
                        {
                            token += "*,";
                        }
                        token += "*>";
                    }
                    i++;
                    return token;
                }
            }
            return null;
        }        
        protected string getTypeAncestor(string name)
        {
           
            if (name.IndexOf('<') < 0) return name;
            else return name.Substring(0, name.IndexOf('<'));

        }
        
        protected List<T> getAllRoleAncestor<T>(T r, List<T> lst) where T : TypesAndRoles
        {
            List<T> allFathers = new List<T>();
            T father = getInstance(r.getAFather(r.getNumberFathers() - 1), lst);
            
            while ((father != null) && (father.roleType == rTypes.roleInstantiated))
            {
                allFathers.Add(father);
                father = getInstance(father.getAFather(father.getNumberFathers() - 1), lst);
            }
            return allFathers;
        }
        protected ObjectRoles createDummyObjectRole(string roleName)
        {
            ObjectRoles r = new ObjectRoles();
            r.setName(roleName);
            int less = roleName.IndexOf('<');
            if (less >= 0)
            {
                int comma1 = less;
                int comma2 = roleName.IndexOf(',', comma1);
                if (comma2 < 0) comma2 = roleName.IndexOf('>');
                while (true)
                {

                    string para = roleName.Substring(comma1 + 1, comma2 - comma1 - 1);
                    TypesAndRoles p = new TypesAndRoles();
                    p.setName(para);
                    if (isInSet(para, typeDefs) >= 0)
                    {
                        p.roleType = rTypes.parameterType;
                    }
                    else if (para == "*")
                    {
                        p.roleType = rTypes.parameterStar;
                    }
                    else
                    {
                        p.roleType = rTypes.parameterValue;
                    }


                    r.addParameters(p);

                    comma1 = roleName.IndexOf(',', comma2);
                    comma2 = roleName.IndexOf(',', comma1 + 1);
                    if (comma2 < 0) comma2 = roleName.IndexOf('>');
                    if (comma1 < 0) break;


                }

            }            
            r.roleType = rTypes.roleInstantiated;
            return r;

        }
        protected SubjectRoles createDummySubjectRole(string roleName)
        {
            SubjectRoles r = new SubjectRoles();
            r.setName(roleName);
            int less = roleName.IndexOf('<');
            if (less >= 0)
            {
                int comma1 = less;
                int comma2 = roleName.IndexOf(',', comma1);
                if (comma2 < 0) comma2 = roleName.IndexOf('>');
                while (true)
                {

                    string para = roleName.Substring(comma1 + 1, comma2 - comma1 - 1);
                    TypesAndRoles p = new TypesAndRoles();
                    p.setName(para);
                    if (isInSet(para, typeDefs) >= 0)
                    {
                        p.roleType = rTypes.parameterType;
                    }
                    else if (para == "*")
                    {
                        p.roleType = rTypes.parameterStar;
                    }
                    else
                    {
                        p.roleType = rTypes.parameterValue;
                    }

                    r.addParameters(p);

                    comma1 = roleName.IndexOf(',', comma2);
                    comma2 = roleName.IndexOf(',', comma1 + 1);
                    if (comma2 < 0) comma2 = roleName.IndexOf('>');
                    if (comma1 < 0) break;


                }

            }
            r.roleType = rTypes.roleInstantiated;
            return r;

        }
        protected TypesAndRoles findType(string aName)
        {
            int i = 0;
            for (i = 0; i < typeDefs.Count; i++)
            {
                if (typeDefs[i].getName() == aName) break;
            }
            if (i >= typeDefs.Count) return null;
            return typeDefs[i];
        }


#endregion Protected functions
        /////////////////////////////////////////////////////////////////////////////////////////////////
        ///////Exporting...........
        /////////////////////////////////////////////////////////////////////////////////////////////////

#region Export policies

        private void translateToPolicies()
        {
            if (symanticErrors.Count > 0)
            {
                policies = null;

            }
            else
            {
                foreach (Subjects s in lstSubjects)
                {
                    //get all subject role for this subject
                    List<SubjectRoles> sRs = s.getAllSubjectRoles();

                    foreach (SubjectRoles sr in sRs)
                    {
                        //get all Permissions
                        List<string> asso = sr.getAllPermissions();

                        foreach (string a in asso)
                        {
                            string oRole = "";
                            string action = seperatePermission(a, ref oRole);
                            foreach (ObjectRoles oR in lstObjectRoles)
                            {
                                if (oR.getName() == oRole)
                                {
                                    foreach (Objects oj in lstObjects)
                                    {
                                        if (oj.findObjectRole(oRole) >= 0)
                                        {
                                            //acts += oj.getName() + " & ";
                                            Policy p = new Policy(sr.getName(), action, oj.getName());
                                            policies.Add(p);

                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        public string exportPERMIS(string OID, string subjectDNs, string SOA_Dn, string objectDNs)
        {

            string permisXML = "";
            permisXML += "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + Environment.NewLine;
            permisXML += "<X.509_PMI_RBAC_Policy OID=\"" + OID + "\">" + Environment.NewLine;

            //1. Subject Domain (all rubjects)
            permisXML += "  <SubjectPolicy>" + Environment.NewLine;
            foreach (Subjects s in lstSubjects)
            {
                permisXML += "        <SubjectDomainSpec ID=\"" + s.getName() + "\">" + Environment.NewLine;
                permisXML += "            <Include LDAPDN=\"cn=" + s.getName() + "," + subjectDNs + "\"/>" + Environment.NewLine;
                permisXML += "        </SubjectDomainSpec>" + Environment.NewLine;
            }
            permisXML += "    </SubjectPolicy>" + Environment.NewLine;

            //2. Role Hierachy
            permisXML += "   <RoleHierarchyPolicy>" + Environment.NewLine;
            permisXML += "       <RoleSpec OID=\"1.2.826.0.1.3344810.1.1.14\" Type=\"permisRole\">" + Environment.NewLine;
            foreach (SubjectRoles sr in lstSubjectRoles)
            {
                if (sr.roleType == rTypes.roleInstantiated)
                {
                    //if (sr.getRelative(0) != null)
                    //{
                    //    permisXML += "           <SupRole Value=\"" + replaceRoleName(sr.getName()) + "\">" + Environment.NewLine;
                    //    permisXML += "               <SubRole Value=\"" + replaceRoleName(sr.getRelative(0)) + "\"/>" + Environment.NewLine;
                    //    permisXML += "           </SupRole>" + Environment.NewLine;
                    //}
                    //else //if (!sr.isInAsRelatives())
                    //{
                    //   permisXML += "           <SupRole Value=\"" + replaceRoleName(sr.getName()) + "\"/>" + Environment.NewLine;
                    //}

                    if (sr.getAllFathers().Count > 0)
                    {
                        //permisXML += "           <SupRole Value=\"" + replaceRoleName(sr.getName()) + "\"/>" + Environment.NewLine;
                        SubjectRoles fSR = getInstance(sr.getAFather(sr.getNumberFathers() - 1), lstSubjectRoles);
                        if (fSR.roleType == rTypes.roleInstantiated)
                        {
                            permisXML += "           <SubRole Value=\"" + replaceRoleName(sr.getAFather(sr.getNumberFathers() - 1)) + "\"/>" + Environment.NewLine;
                            permisXML += "           <SupRole Value=\"" + replaceRoleName(sr.getName()) + "\">" + Environment.NewLine;
                            permisXML += "               <SubRole Value=\"" + replaceRoleName(sr.getAFather(sr.getNumberFathers() - 1)) + "\"/>" + Environment.NewLine;
                            permisXML += "           </SupRole>" + Environment.NewLine;
                        }
                    }
                    else
                    {
                        permisXML += "           <SupRole Value=\"" + replaceRoleName(sr.getName()) + "\"/>" + Environment.NewLine;
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
                List<SubjectRoles> sSR = s.getAllSubjectRoles();
                permisXML += "      <RoleAssignment ID=\"Assign_Roles_" + s.getName() + "\">" + Environment.NewLine;
                permisXML += "          <SubjectDomain ID=\"" + s.getName() + "\"/>" + Environment.NewLine;
                permisXML += "          <RoleList>" + Environment.NewLine;
                foreach (SubjectRoles sr in sSR)
                {
                    //find father (because sr is an InstanceRole)
                    //SubjectRoles fSR = getInstance(sr.getAFather(0), lstSubjectRoles);
                    //string fName = replaceRoleName(fSR.getName());
                    permisXML += "              <Role Type=\"permisRole\" Value=\"" + replaceRoleName(sr.getName()) + "\"/>" + Environment.NewLine;
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
                permisXML += "        <TargetDomainSpec ID=\"" + o.getName() + "\">" + Environment.NewLine;
                permisXML += "            <Include LDAPDN=\"cn=" + o.getName() + "," + objectDNs + "\"/>" + Environment.NewLine;
                permisXML += "        </TargetDomainSpec>" + Environment.NewLine;
            }
            permisXML += "  </TargetPolicy>" + Environment.NewLine;

            //6. Action Policy
            permisXML += "  <ActionPolicy>" + Environment.NewLine;
            foreach (SubjectRoles sr in lstSubjectRoles)
            {
                if (sr.roleType == rTypes.roleInstantiated)
                {

                    List<string> actionOBjects = subjRole_GetAllActionObject(sr);
                    foreach (string ao in actionOBjects)
                    {
                        string[] sAO = sepeartedActionObject(ao);
                        permisXML += "      <Action ID=\"" + sAO[0] + "\" Name =\"" + sAO[0] + "\">" + Environment.NewLine;
                        for (int i = 1; i < sAO.Length; i++)
                        {
                            permisXML += "          <TargetDomain ID=\"" + sAO[i] + "\"/>" + Environment.NewLine;
                        }
                        permisXML += "      </Action>" + Environment.NewLine;

                    }
                }
            }
            permisXML += "  </ActionPolicy>" + Environment.NewLine;

            //7. Target Access Policy
            permisXML += "  <TargetAccessPolicy>" + Environment.NewLine;
            foreach (SubjectRoles sr in lstSubjectRoles)
            {
                if (sr.roleType == rTypes.roleInstantiated)
                {

                    List<string> actionOBjects = subjRole_GetAllActionObject(sr);
                    foreach (string ao in actionOBjects)
                    {
                        string[] sAO = sepeartedActionObject(ao);

                        for (int i = 1; i < sAO.Length; i++)
                        {
                            permisXML += "      <TargetAccess ID=\"TargetAccessPolicy_" + replaceRoleName(sr.getName()) + sAO[i] + i.ToString() + "\">" + Environment.NewLine;
                            permisXML += "          <RoleList>" + Environment.NewLine;
                            permisXML += "              <Role Type=\"permisRole\" Value=\"" + replaceRoleName(sr.getName()) + "\"/>" + Environment.NewLine;
                            permisXML += "          </RoleList>" + Environment.NewLine;
                            permisXML += "          <TargetList>" + Environment.NewLine;
                            permisXML += "              <Target>" + Environment.NewLine;
                            permisXML += "                  <TargetDomain ID=\"" + sAO[i] + "\"/>" + Environment.NewLine;
                            permisXML += "                  <AllowedAction ID=\"" + sAO[0] + "\"/>" + Environment.NewLine;
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

#endregion Export policy

    }
}

#region Unused -- for backing up

/*protected int updateRelatives(Irony.Parsing.TokenList tokenList, int fromIndex)
        {
            int i = fromIndex;
            string token = getTokenValue(tokenList, i);
            string term = getTokenTerminal(tokenList, i);
            string nextTwoTokens = getTokenValue(tokenList, i + 2);
            if ((term.ToLower() == "identifier") && (nextTwoTokens.ToLower() == "relative"))
            {
                int iSRole = isInSet(token, lstSubjectRoles);
                if (iSRole >= 0)
                {
                    //pracNurse.Relative(regNurse);
                    //token=pracNurse
                    i += 4; //skip (
                    string sjRole = getTokenValue(tokenList, i);
                    int iSubjectR = isInSet(sjRole, lstSubjectRoles);
                    if (iSubjectR >= 0)
                    {
                        if (checkSRoleRelative(lstSubjectRoles[iSRole], lstSubjectRoles[iSubjectR]))
                        {
                            lstSubjectRoles[iSRole].Relative(sjRole);
                            //
                            //update relative for father role
                            //
                            SubjectRoles fSR = getInstance(lstSubjectRoles[iSRole].getAFather(0), lstSubjectRoles);
                            SubjectRoles fRelativeSR = getInstance(getInstance(sjRole, lstSubjectRoles).getAFather(0), lstSubjectRoles);
                            fSR.Relative(fRelativeSR.getName());
                            fRelativeSR.InRelative(fSR.getName());

                            i += 2;//skip )
                        }
                        else
                        {
                            //error: Not subtype
                            symanticErrors.Add("TypeOfRole('" + token + "') is not subtype of TypeOfRole('" + sjRole + "')(Line:" + tokenList[i].Location.Line.ToString() + ", Cols: " + tokenList[i].Location.Column.ToString() + ")");
                        }
                    }
                    else
                    {
                        //error: set Relative to a not compatative subjectRole
                        symanticErrors.Add("Set SubjectRole: '" + token + ".Relative' to a not compatative subjectRole (" + sjRole + ")(Line:" + tokenList[i].Location.Line.ToString() + ", Cols: " + tokenList[i].Location.Column.ToString() + ")");

                    }
                }
                else
                {
                    int iORole = isInSet(token, lstObjectRoles);
                    if (iORole >= 0)
                    {
                        i += 4; //skip (
                        string ojRole = getTokenValue(tokenList, i);
                        int iObjectR = isInSet(ojRole, lstObjectRoles);
                        if (isInSet(ojRole, lstObjectRoles) >= 0)
                        {
                            if (checkORoleRelative(lstObjectRoles[iORole], lstObjectRoles[iObjectR]))
                            {
                                lstObjectRoles[iORole].Relative(ojRole);
                                //
                                //update relative for father role
                                //
                                ObjectRoles fOR = getInstance(lstObjectRoles[iORole].getAFather(0), lstObjectRoles);
                                ObjectRoles fRelativeOR = getInstance(getInstance(ojRole, lstObjectRoles).getAFather(0), lstObjectRoles);
                                fOR.Relative(fRelativeOR.getName());
                                fRelativeOR.InRelative(fOR.getName());

                                i += 2; //skip )
                            }
                            else
                            {
                                //error: Not subtype
                                symanticErrors.Add("TypeOfRole('" + token + "') is not subtype of TypeOfRole('" + ojRole + "')(Line:" + tokenList[i].Location.Line.ToString() + ", Cols: " + tokenList[i].Location.Column.ToString() + ")");
                            }
                        }
                        else
                        {
                            //error: set Relative to a not compatative subjectRole
                            symanticErrors.Add("Set ObjectRole: '" + token + ".Relative' to a not compatative ObjectRole (" + ojRole + ")(Line:" + tokenList[i].Location.Line.ToString() + ", Cols: " + tokenList[i].Location.Column.ToString() + ")");
                        }

                    }
                    else
                    {
                        //error: set unkown subjectRole/objectRole Relative
                        symanticErrors.Add(token + " is an unkonwn SubjectRole/ObjectRole)(Line:" + tokenList[fromIndex].Location.Line.ToString() + ", Cols: " + tokenList[fromIndex].Location.Column.ToString() + ")");
                    }
                }
            }
            return i;
        }*/
/*protected bool checkSRoleRelative(SubjectRoles left, SubjectRoles right)
{
    //
    //Duyet het parameter value, check 
    //
    string rFName = right.getAFather(0);
    string lFName = left.getAFather(0);

    SubjectRoles rightFather = findSRorORole<SubjectRoles>(rFName, lstSubjectRoles);
    SubjectRoles leftFather = findSRorORole<SubjectRoles>(lFName, lstSubjectRoles);
    for (int i = 0; i < rightFather.getNumberPararmeters(); i++)
    {
        if (isSubtype(leftFather.getParameterName(i), rightFather.getParameterName(i)))
        {
            return true;
        }
    }

    return false;
}*/
/*protected bool checkORoleRelative(ObjectRoles left, ObjectRoles right)
{
    string rFName = right.getAFather(0);
    string lFName = left.getAFather(0);

    ObjectRoles rightFather = findSRorORole<ObjectRoles>(rFName, lstObjectRoles);
    ObjectRoles leftFather = findSRorORole<ObjectRoles>(lFName, lstObjectRoles);
    for (int i = 0; i < rightFather.getNumberPararmeters(); i++)
    {
        if (isSubtype(leftFather.getParameterName(i), rightFather.getParameterName(i)))
        {
            return true;
        }
    }

    return false;
}*/

#endregion Unused -- for backingup