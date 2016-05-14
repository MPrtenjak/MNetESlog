//css_import publish_worker;

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;

class Script
{
  [STAThread]
  static public int Main(string[] args)
  {
    Worker worker = new Worker();
    worker.Execute();
		return 0;
  }

  internal class Worker : PublishWorker
  {
		private string finalOutputName = @"D:\razvoj\FinalOutput\MNetESlogOutput";
    
    public Worker()
    : base()
    {
    }

    public int Execute()
    {
      showStep("Clean");

      int errorID = 1;

      helper.ExecuteExternalProgram("clean.bat", string.Empty);

      showStep("Update version");
      string newNumber;
      if (!helper.UpdateVersion(out newNumber))
        return showError("Update version error", ++errorID);

      showStep("Build");
      if (helper.ExecuteExternalProgram("build.bat", string.Empty) != 0)
        return showError("Build error", ++errorID);

  		showStep("Create folders version");
			string newFolderName = finalOutputName + @"\" + newNumber;

      showStep("Copy files");
      if (!copyFiles(newFolderName, "Net35", "MNetESlogGui"))
        return showError("Copy files error", ++errorID);

      showStep("Done!!! New file version : " + newNumber);

      return 0;
    }

    public bool copyFiles(string destinationPath, string destinationSubPath, string sourceProject)
    {
      var script = helper.GetScriptName();
      var scriptPath = System.IO.Path.GetDirectoryName(script);

      string sourcePath = System.IO.Path.Combine(scriptPath, sourceProject);
      sourcePath = System.IO.Path.Combine(sourcePath, @"bin\Release");
      destinationPath = System.IO.Path.Combine(destinationPath, destinationSubPath);
      if (!helper.CopyFolderContents(sourcePath, destinationPath,
        new string[] { "*.exe", "*.dll", "*.config" })) return false;

      return true;
    }
  }
}