using System;
using System.Collections.Generic;
using System.Linq;
using Irony.Parsing;

namespace NCTU
{
    [Language("Generic Policy Language", "1.4", "The language for RBAC policies specification [Kha-Tho N., John K. Zao]")]
    public class Grammar : Irony.Parsing.Grammar
    {
        public Grammar()
        {
            #region Declare Terminals Here

            CommentTerminal blockComment = new CommentTerminal("block-comment", "/*", "*/");
            CommentTerminal lineComment = new CommentTerminal("line-comment", "//",
                "\r", "\n", "\u2085", "\u2028", "\u2029");
            NonGrammarTerminals.Add(blockComment);
            NonGrammarTerminals.Add(lineComment);

            NumberLiteral number = new NumberLiteral("number");
            IdentifierTerminal identifier = new IdentifierTerminal("identifier");
            StringLiteral session_keyword = new StringLiteral("session");
            StringLiteral STRING = new StringLiteral("STRING", "\"", StringFlags.AllowsAllEscapes);


            #endregion

            #region Declare NonTerminals Here
            NonTerminal program = new NonTerminal("program");
            NonTerminal TypeDeclarations = new NonTerminal("TypeDeclarations");
            NonTerminal TypeDeclaration = new NonTerminal("TypeDeclaration");
            NonTerminal ClassInterDeclarations = new NonTerminal("ClassInterDeclarations");
            NonTerminal ExtendDeclaration = new NonTerminal("ExtendDeclaration");
            NonTerminal InterfaceDeclarations = new NonTerminal("InterfaceDelcarations");           
            NonTerminal ClassDeclarations = new NonTerminal("ClassDeclarations");
            NonTerminal ClassDeclaration = new NonTerminal("ClassDeclaration");
          //  NonTerminal Initialziation = new NonTerminal("Initialization");
            NonTerminal Sessions = new NonTerminal("Sessions");
            NonTerminal Parameters = new NonTerminal("Parameters");
            NonTerminal SubjectRoleClasses = new NonTerminal("SubjectRoleClasses");
            NonTerminal ObjectRoleClasses = new NonTerminal("ObjectRoleClasses");
            NonTerminal SubjRoleClass = new NonTerminal("SubjRoleClass");
            NonTerminal ObjRoleClass = new NonTerminal("ObjRoleClass");
            NonTerminal ObjectClass = new NonTerminal("ObjectClass");
            NonTerminal SubjectClass = new NonTerminal("SubjectClass");
            NonTerminal ObjectRole = new NonTerminal("ObjectRole");
            NonTerminal SubjectRole = new NonTerminal("SubjectRole");
            NonTerminal ClassBody = new NonTerminal("ClassBody");
            NonTerminal InterfaceOBody = new NonTerminal("InterfaceOBody");
            NonTerminal InterfaceSBody = new NonTerminal("InterfaceSBody");
            NonTerminal MultiExtendTypes = new NonTerminal("MultiExtendTypes");
            NonTerminal MultiIdentifier = new NonTerminal("MultiIdentifier");
            NonTerminal Function = new NonTerminal("Function");
            NonTerminal MultiFunctions = new NonTerminal("MultiFunctions");
            NonTerminal FunctionBody = new NonTerminal("FunctionBody");
         //   NonTerminal MultiAssociations = new NonTerminal("MultiAssociations");
         //   NonTerminal Relative = new NonTerminal("Relative");
            NonTerminal Permission = new NonTerminal("Permission");
            NonTerminal PermissionBody = new NonTerminal("PermissionBody");
         //   NonTerminal RelativeBody = new NonTerminal("RelativeBody");
            NonTerminal Statement = new NonTerminal("Statement");
            NonTerminal Statements = new NonTerminal("Statements");
            NonTerminal DeclarationStatement = new NonTerminal("DeclarationStatement");
          //  NonTerminal InvokeAssociation = new NonTerminal("InvokeAssociation");
            NonTerminal AssignmentStatement = new NonTerminal("AssignmentStatement");
            NonTerminal ClassIndentifier = new NonTerminal("ClassIdentifier");
            NonTerminal RoleIdentifier = new NonTerminal("RoleIdentifier");
            NonTerminal IdentifierValues = new NonTerminal("IdentifierValues");
            NonTerminal IdentifierValue = new NonTerminal("IdentifierValue");
            NonTerminal Action = new NonTerminal("Action");
            NonTerminal SingleExtend = new NonTerminal("SingleExtend");
            NonTerminal ExtendList = new NonTerminal("ExtendList");
            NonTerminal RepeatedIdentifier = new NonTerminal("RepeatedIdentifier");
            NonTerminal IdentifierValueList = new NonTerminal("IdentifierValueList");
            NonTerminal SameAssignments = new NonTerminal("SameAssignments");
            NonTerminal SameAssignment = new NonTerminal("SameAssignment");
            //NonTerminal ExtendsOrIdentifiers = new NonTerminal("ExtendsOrIdentifiers");
            NonTerminal ParametersLists = new NonTerminal("ParametersLists");
            NonTerminal SessionName = new NonTerminal("SessionName");
            NonTerminal SessionNames = new NonTerminal("SessionNames");
            NonTerminal SessionStatements = new NonTerminal("SessionStatements");
            NonTerminal SessionStmt = new NonTerminal("SessionStmt");
            NonTerminal SubjObjDelcaration = new NonTerminal("SubjObjDeclaration");
            NonTerminal SbjObjVariable = new NonTerminal("SbjObjVariable");
            NonTerminal InstantiatedClass = new NonTerminal("InstantiatedClass");
            NonTerminal InstantiatedClassBody = new NonTerminal("InstantiatedClassBody");
            NonTerminal PermissionInstantiated = new NonTerminal("PermissionInstantiated");
            NonTerminal PermissionOperator = new NonTerminal("PermissionOperator");
            

            NonTerminal ActionDefinitions = new NonTerminal("ActionDefinitions");
            NonTerminal MultiRoleIdentifier = new NonTerminal("MultiRoleIdentifier");
            NonTerminal RepeatedRoleIdentifier = new NonTerminal("RepeatedRoleIdentifier");
            

            #endregion

            #region Place Rules Here

            this.Root = program;

            //program.Rule = TypeDeclarations + InterfaceDeclarations+ ClassDeclarations + Initialziation + Sessions;
            program.Rule = TypeDeclarations + InterfaceDeclarations + ClassDeclarations +  Sessions;
          
            ////#typeDeclarations
            /*--------------for enum
            TypeDeclarations.Rule = "typedef" + ExtendDeclaration + TypeDeclaration;
            TypeDeclaration.Rule =   "typedef" + ExtendDeclaration + TypeDeclaration| Empty;
            ExtendDeclaration.Rule = identifier+ "=" + "{" + MultiIdentifier + "}";
            MultiIdentifier.Rule = identifier + RepeatedIdentifier ;
            RepeatedIdentifier.Rule =  "," + identifier + RepeatedIdentifier | Empty ; */

            TypeDeclarations.Rule = "typedef" + ExtendDeclaration + TypeDeclaration;
            TypeDeclaration.Rule = "typedef" + ExtendDeclaration +  TypeDeclaration | Empty;
            ExtendDeclaration.Rule = MultiIdentifier + "extends" + identifier | MultiIdentifier;
            MultiIdentifier.Rule = identifier + RepeatedIdentifier;
            RepeatedIdentifier.Rule = "," + identifier + RepeatedIdentifier | Empty;
               
            //#classDeclaration
            InterfaceDeclarations.Rule = SubjectRole + ObjectRole | ObjectRole + SubjectRole;
            ClassDeclarations.Rule = MakeStarRule(ClassDeclarations, ClassDeclaration);
            ClassDeclaration.Rule = ActionDefinitions | SubjectClass | ObjectClass | SubjectRoleClasses | ObjectRoleClasses | InstantiatedClass;

            ActionDefinitions.Rule = "action" + identifier+"(" + RoleIdentifier + "," + RoleIdentifier + ")";
            
            //#subject Class
            SubjectClass.Rule = ToTerm("class") + "Subject" + "{" + ClassBody + "}";
            ClassBody.Rule = Empty;
            //ClassBody.Rule = MakeStarRule(ClassBody, Function); 
            //Function.Rule = identifier + "("+ ")" + "{" + FunctionBody +"}" | identifier + "(" + MultiIdentifier + ")" + "{" + FunctionBody + "}" | Permission;            
            //FunctionBody.Rule = Empty; //!!!!!!!!!!be careful here            
            
            //#objectClass
            ObjectClass.Rule = ToTerm("class") + "Object" + "{" + ClassBody + "}";
            
            //#objectRole
            ObjectRole.Rule = ToTerm("interface") + "ObjectRole" + "{" + InterfaceOBody + "}";
            SubjectRole.Rule = ToTerm("interface") + "SubjectRole" + "{" + InterfaceSBody + "}";
            InterfaceOBody.Rule = ToTerm("permission") + "(" + ToTerm("Action") + "," + ToTerm("SubjectRole") + ")";
            InterfaceSBody.Rule = ToTerm("permission") + "(" + ToTerm("Action") + "," + ToTerm("ObjectRole") + ")";


            InstantiatedClass.Rule = "class" + RoleIdentifier + "{" + InstantiatedClassBody + "}" |                                     
                                      "class" +  RoleIdentifier + "extends" + RoleIdentifier + "{" + InstantiatedClassBody + "}";
            InstantiatedClassBody.Rule = MakeStarRule(InstantiatedClassBody, PermissionInstantiated);
            PermissionInstantiated.Rule = ToTerm("permission") + "(" + PermissionOperator +"," + RoleIdentifier+ ")";
            PermissionOperator.Rule = ToTerm("*") |  identifier ;            

            //#subjectRoleClasses          
            SubjectRoleClasses.Rule = SubjRoleClass;
            SubjRoleClass.Rule = ToTerm("interface") + identifier + Parameters + "extends" + "SubjectRole" + "{" + ClassBody + "}";          
            Parameters.Rule =  "<" + ParametersLists +">" | Empty;
            SingleExtend.Rule = identifier + "extends" + identifier | identifier | ToTerm("*") | STRING ;
            ParametersLists.Rule = SingleExtend + ExtendList ;
            ExtendList.Rule = "," + SingleExtend + ExtendList | Empty;

            //#objectRoleClasses  
            ObjectRoleClasses.Rule = ObjRoleClass;
            ObjRoleClass.Rule = ToTerm("interface") + identifier + Parameters + "extends" + "ObjectRole" + "{" + ClassBody+"}" ;

            //#initialization
           // Initialziation.Rule = ToTerm("initialize") + "(" + ")" + "{" + Statements + "}";

          
            

            //sessions definition
            Sessions.Rule = "session" + SessionNames + "{" + SessionStatements+  "}";
            SessionNames.Rule = MakeStarRule(SessionNames, SessionName);
            SessionName.Rule = identifier | number;

            //#statements
            SessionStatements.Rule = MakeStarRule(SessionStatements, SessionStmt);
            SessionStmt.Rule = SubjObjDelcaration | AssignmentStatement;
            SubjObjDelcaration.Rule = SbjObjVariable + MultiIdentifier + ";";
            SbjObjVariable.Rule = ToTerm("Subject") | ToTerm("Object");
            
            AssignmentStatement.Rule = SameAssignments + ":=" + MultiRoleIdentifier + ";";
            SameAssignments.Rule = identifier + SameAssignment;
            SameAssignment.Rule = "=" + identifier + SameAssignment | Empty;
            MultiRoleIdentifier.Rule = ToTerm( "new") + RoleIdentifier + RepeatedRoleIdentifier;
            RepeatedRoleIdentifier.Rule = "," + ToTerm("new") + RoleIdentifier + RepeatedIdentifier | Empty;
            
            RoleIdentifier.Rule = identifier + "<" + IdentifierValues + ">" | identifier ;
            IdentifierValues.Rule = IdentifierValue + IdentifierValueList;
            IdentifierValueList.Rule = "," + IdentifierValue + IdentifierValueList | Empty;
            IdentifierValue.Rule = ToTerm("*") | identifier | STRING;

            
            #endregion

            #region Define Keywords

            this.MarkReservedWords("typedef", "extends", "class", "if", ":=");

            #endregion
        }
    }
}
