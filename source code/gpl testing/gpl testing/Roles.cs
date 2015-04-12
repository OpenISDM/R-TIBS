using System;
using System.Data;
using System.Configuration;
using System.Linq;

using System.Xml.Linq;
using System.Collections.Generic;

namespace GPL_Testing
{
    public enum rTypes { baseRoles, parameterStar, parameterType, parameterValue, roleDefinition, roleInstantiated, roleInstance };
    public class TypesAndRoles
    {
        public rTypes roleType = rTypes.baseRoles;

        private string name = "";
        private List<string> father = new List<string>();
        private List<string> children = new List<string>();
        private List<TypesAndRoles> Parameters = new List<TypesAndRoles>();
        public int getNumberFathers()
        {
            return father.Count;
        }
        public int getNumberChildren()
        {
            return children.Count;
        }
        public int getNumberPararmeters()
        {
            return Parameters.Count;
        }
        public bool addFather(string aFatherName)
        {
            if (isFather(aFatherName)) return false;
            if (father.Count > 0)
            {
                father[0] = aFatherName;
            }
            else
            {
                father.Add(aFatherName);
            }
            return true;
        }
        public bool addChild(string aChildName)
        {
            if (isChild(aChildName)) return false;

            children.Add(aChildName);
            return true;
        }
        public bool addParameters(TypesAndRoles aPara)
        {
            if (Parameters.Contains(aPara)) return false;
            Parameters.Add(aPara);
            return true;
        }
        public bool isFather(string aFatherName)
        {
            return father.Contains(aFatherName);
        }
        public bool isChild(string aChildName)
        {
            return children.Contains(aChildName);
        }
        public bool hasFathers()
        {
            return (father.Count > 0);
        }
        public bool hasChildren()
        {
            return (children.Count > 0);
        }
        public void setName(string aName)
        {
            name = aName;
        }
        public string getAFather(int index)
        {
            if ((index < father.Count) && (index >= 0))
            {
                return father[index];
            }
            return null;
        }
        public string getAChild(int index)
        {
            if ((index >= 0) && (index < children.Count))
            {
                return children[index];
            }
            return null;
        }
        public string getName()
        {
            return name;
        }
        public string getParameterName(int paraIndex)
        {
            return Parameters[paraIndex].getName();
        }
       
        public List<string> getAllFathers()
        {
            return father;
        }
        public List<string> getAllChildren()
        {
            return children;
        }
        public TypesAndRoles getAParameters(int index)
        {
            if ((index >= 0) && (index < Parameters.Count))
            {
                return Parameters[index];
            }
            return null;
        }
    }
    public class ObjectRoles : TypesAndRoles
    {
        private List<string> Permissions = new List<string>();
        /*
        private List<string> Relatives = new List<string>();
        private List<string> InRelatives = new List<string>();
         */

        public ObjectRoles()
        {
        }
        public bool addPermission(string subjectRole, string action)
        {
            if (Permissions.Contains(action + "@" + subjectRole)) return false; //error

            Permissions.Add(action + "@" + subjectRole);
            return true;
        }
        public List<string> getAllPermissions()
        {
            return Permissions;
        }
        public string getPermission(int index)
        {
            if ((index >= 0) && (index < Permissions.Count))
            {
                return Permissions[index];
            }
            return null;
        }
        /* 
        public bool InRelative(string objectRole)
        {
            if (InRelatives.Contains(objectRole)) return false; //error

            InRelatives.Add(objectRole);
            return true;
        }
        public bool isInAsRelatives()
        {
            return InRelatives.Count > 0;
        }
        public bool isAssociation(string subjectRole, string action)
         {
             return Associations.Contains(subjectRole + "[" + action + "]");

         } 
         public bool isRelative(string anObjectRole)
         {
             return Relatives.Contains(anObjectRole);
         }
         public bool Relative(string objectRole)
         {
             if (Relatives.Contains(objectRole)) return false; //error

             Relatives.Add(objectRole);
             return true;
         }
         public string getInRelative(int index)
         {
             if ((index >= 0) && (index < InRelatives.Count))
             {
                 return InRelatives[index];
             }
             return null;
         }
        
         public string getRelative(int index)
         {
             if ((index >= 0) && (index < Relatives.Count))
             {
                 return Relatives[index];
             }
             return null;
         }    
        public List<string> getAllRelatives()
        {
            return Relatives;
        }*/
    }
    public class SubjectRoles : TypesAndRoles
    {
        private List<string> Permissions = new List<string>();
        /*private List<string> Relatives = new List<string>();
        private List<string> InRelatives = new List<string>();*/
        
        public SubjectRoles()
        {
        }
        public bool addPermission(string action, string objectRole)
        {
            if (Permissions.Contains(action + "@" + objectRole)) return false; //error

            Permissions.Add( action + "@" + objectRole);
            return true;
        }
        public bool isAssociation(string action, string objectRole)
        {
            return Permissions.Contains("[" + action + "]" + objectRole);

        }
        public List<string> getAllPermissions()
        {
            return Permissions;
        }
        /*
        public bool InRelative(string subjectRole)
        {
            if (InRelatives.Contains(subjectRole)) return false; //error

            InRelatives.Add(subjectRole);
            return true;
        }
        public bool isInAsRelatives()
        {
            return InRelatives.Count > 0;
        }
        
        public bool isRelative(string anSubjectRole)
        {
            return Relatives.Contains(anSubjectRole);
        }
        public bool Relative(string subjectRole)
        {
            if (Relatives.Contains(subjectRole)) return false; //error

            Relatives.Add(subjectRole);
            return true;
        }
        public string getAssociation(int index)
        {
            if ((index >= 0) && (index < Permissions.Count))
            {
                return Permissions[index];
            }
            return null;
        }
        public string getRelative(int index)
        {
            if ((index >= 0) && (index < Relatives.Count))
            {
                return Relatives[index];
            }
            return null;
        }
        public string getInRelative(int index)
        {
            if ((index >= 0) && (index < InRelatives.Count))
            {
                return InRelatives[index];
            }
            return null;
        }
        
        public List<string> getAllRelatives()
        {
            return Relatives;
        }*/
       
    }
    public class Objects
    {
        //
        //attributes
        //
        private List<ObjectRoles> oRoles = new List<ObjectRoles>();
        private string name = "";
       
        public Objects() { }
        public int findObjectRole(string aORoleName)
        {
            int i = 0;
            if (oRoles.Count > 0)
            {
                foreach (ObjectRoles oR in oRoles)
                {
                    i++;
                    if (oR.getName() == aORoleName) return i;
                }
            }
            return -1;
        }
        public bool addObjectRole(ObjectRoles oR)
        {
            if (oRoles.Contains(oR)) return false;
            oRoles.Add(oR);
            return true;
        }
        public void setName(string objectName)
        {
            name = objectName;
        }
        public string getName()
        {
            return name;
        }
        public List<ObjectRoles> getAllObjectRoles()
        {
            return oRoles;
        }

    }
    public class Subjects
    {
        private List<SubjectRoles> sRoles = new List<SubjectRoles>();
        private string name = "";
      
        public Subjects() { }
        public int findSubjectRole(string aSRoleName)
        {
            int i = 0;
            if (sRoles.Count > 0)
            {
                foreach (SubjectRoles sR in sRoles)
                {
                    i++;
                    if (sR.getName() == aSRoleName) return i;
                }
            }
            return -1;
        }
        public bool addSubjectRole(SubjectRoles sR)
        {
            if (sRoles.Contains(sR)) return false;
            sRoles.Add(sR);
            return true;
        }
        public void setName(string objectName)
        {
            name = objectName;
        }
        public string getName()
        {
            return name;
        }
        public List<SubjectRoles> getAllSubjectRoles()
        {
            return sRoles;
        }
    }
    public class Policy
    {
        private string Action="";
        private string Subject="";
        private string Object="";
        public Policy(string Subject, string Action, string Object)
        {

        }
        public string getAction() {
            return Action;
        }
        public string getSubject()
        {
            return Subject;
        }
        public string getObject()
        {
            return Object;
        }
    }
    public class ActionOperator 
    {
        public string name="";
        public string leftOperand="";
        public string rightOperand = "";
       
    }
}
