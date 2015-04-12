using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GPLCompiler
{
     public enum rTypes { baseRoles, roleDefinition, roleInstantiated, roleInstance };
     public enum pTypes {parameterBounded, parameterStar, parameterType, parameterValue};

    public class Roles //for both Subject-Role, Object-Role
    {
        public List<Roles> baseRoles = new List<Roles>();
        public string name = "";
        public string alias = "";
        public List<Types> parameters = new List<Types>();
        public List<Permissions> permissions = new List<Permissions>();
        public rTypes roleType = rTypes.roleInstantiated;
        public Roles searchBaseRoles(string roleName)
        {
            foreach (Roles r in baseRoles)
            {
                if (r.name == roleName) return r;
            }
            return null;
        }
    }

    public class Types //for type hierarchy
    {
        public pTypes parameterType ;
        public string name ;
        public Types superType;
        public Types() {            
            parameterType = pTypes.parameterType;
            name = "";
        }

    }
    public class Permissions //for permissions
    {
        public string name = "";
        public Operators op = new Operators();
        public Roles role = new Roles();
    }
    public class Subjects //for Subject or Object
    {
        public List<Roles> Roles = new List<Roles>();
        public string name = "";
    }
    public class Objects //for Object
    {
        public List<Roles> Roles = new List<Roles>();
        public string name = "";
    }
    public  class Operators//for Operator - action 
    {
        public string name = "";
        public Roles leftOperand = new Roles();
        public Roles rightOperand = new Roles();
    }
    
}
