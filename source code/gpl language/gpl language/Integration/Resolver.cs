using System;
using System.Collections.Generic;
using System.Text;
using Irony.Parsing;

namespace NCTU
{
    public class Resolver : NCTU.IASTResolver
    {
        #region IASTResolver Members


        public IList<NCTU.Declaration> FindCompletions(object result, int line, int col)
        {
            // Used for intellisense.
            List<NCTU.Declaration> declarations = new List<NCTU.Declaration>();

            // Add keywords defined by grammar
            foreach (KeyTerm key in Configuration.Grammar.KeyTerms.Values)
            {
                if (key.OptionIsSet(TermOptions.IsKeyword))
                {
                    declarations.Add(new Declaration("", key.Name, 206, key.Name));
                }
            }

            declarations.Sort();
            return declarations;
        }

        public IList<NCTU.Declaration> FindMembers(object result, int line, int col)
        {
            List<NCTU.Declaration> members = new List<NCTU.Declaration>();

            return members;
        }

        public string FindQuickInfo(object result, int line, int col)
        {
            return "unknown";
        }

        public IList<NCTU.Method> FindMethods(object result, int line, int col, string name)
        {
            return new List<NCTU.Method>();
        }

        #endregion
    }
}
