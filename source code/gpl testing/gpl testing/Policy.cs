using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GPLCompiler
{
    public class Policy
    {
        private string Action = "";
        private string Subject = "";
        private string Object = "";
        public Policy(string Subject, string Action, string Object)
        {

        }
        public string getAction()
        {
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
}
