============================================================================
            A-RBAC: A GENERIC POLICY SPECIFICATION PARADIGM FOR 
               ATTRIBUTE-ENRICHED ROLE BASED ACCESS CONTROL 
============================================================================

-----------------------------I. DESCRIPTION---------------------------------

  Mimicking the generic object-oriented programming paradigm, we devised a 
generic A-RBAC policy specification as a modular, expressive and reusable 
way to specify attribute/role-based access control policies in multi-domain 
environments. Beside of developing a polymorphic typing scheme to articulate 
the role hierarchies, we also implemented a policy translator and a static 
policy checker, which can convert Java-like generic policy specification 
into standard XACML policies.
  The generic A/RBAC policy specification was built upon three polymorphic 
typing principles: First, for the sake of mutual independence and symmetry, 
Object Roles were added as a first-class component alongside with Subject 
Roles and Actions. Second, type parameters were introduced to cast Subject 
and Object Roles as bounded polymorphic types. Finally, static type checking 
was employed to verify the consistency among policy specifications.

----------------------------II. IN THIS PACKAGE----------------------------------

1. INSTALLER PACKAGES

  The installer package is in "Intaller" directory which includes the deployable 
modules for A-RBAC as well as all required packages for Microsofot .NET 
Framework 3.5

2. SOURCE CODES

  The source code for A-RBAC is in "Source Code" directory which includes two
projects which have written in .NET environment using Microsoft Visual Studio 
2008. 
  The first project, "GPL Language", implements a Generic Policy Specification 
Language for specifying A-RBAC policy including typing principles, compiling
and policy checker. This is the core module for A-RBAC specification system.
  The second project, "GPL Testing", uses core functionalities provided in GPL
Language project to implement the user interface for writing, testing policy 
written in A-RBAC scheme and translating the A-RBAC policy to an XACML policy.

3. A SAMPLE A-RBAC POLICY
  This package also provides a sample policy written in A-RBAC scheme for testing.
The sample policy is in the file "GPL Example 23.gpl". The extension ".gpl" is
for A-RBAC GPL Language policy.


---------------------------III. INSTALLATION-------------------------------------

1. REQUIREMENTS

- Windows 7 or later
- .NET Framework 3.5

2. INSTALLATION

Run the "setup.exe" program in "Installer" directory to start the installation. 

3. TESTING

(a) Open the installed "GPL Testing" program from its shortcut in your desktop
(b) Choose Load Files to load a written A-RBAC policy 
(c) Navigate to the sample policy "GPL Example 23.gpl"
(d) Type "Joan" in "User/Object Name" textbox and hit "Compile and Test Policy".
(e) See the output window.

4. NOTICE

If your Operating System is Windows 7 or later with .NET Framework 3.5 installed
you may be able to run the pre-compiled binary GPL Testing program under "Debug"
directory (Source Code\GPL Testing\GPL Testing\bin\Debug\GPL Testing.exe)

-----------------IV. REQUIREMENTS for RE-COMPILING THE SOURCE CODE--------------

- Windows 7 or later
- Microsoft .NET Framework 3.5
- Micrsoft Visual Studio 2008
- Micrsoft Visual Studio 2008 SDK 1.1 (http://www.microsoft.com/en-us/download/details.aspx?id=21827)

----------------------------------V. OPEN SOURCE--------------------------------

In this project we used two open-source modules:

(1) Irony - .NET Language Implementation Kit (https://irony.codeplex.com/)
We used this kit for our GPL parser. This is licensed under The MIT License.

(2) Fast Colored TextBox (https://github.com/PavelTorgashov/FastColoredTextBox) 
We used this for our editor on GPL Testing program. This is licensed under 
The GNU Lesser General Public License (LGPLv3)


---------------------------------(C) COPYRIGHT-----------------------------------
- This is Kha Tho (9955630)'s Master Thesis under suppervision of Prof.John K. Zao, 
Department of Computer Science, National Chiao Tung University, Taiwan.
- This work was sponsored by the OpenISDM project (https://openisdm.iis.sinica.edu.tw/)
from March 2011 to July 2012.
 
