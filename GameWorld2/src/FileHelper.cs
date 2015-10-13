using System;

namespace GameWorld2
{
	public class FileHelper
	{
		public static string GetNameFromFilepath(string pFilepath)
		{
			if(string.IsNullOrEmpty(pFilepath)) {
				throw new Exception("Filepath is empty!");
			}
            int index = pFilepath.LastIndexOf("/");
            int index2 = pFilepath.LastIndexOf(@"\");
            if (index2 > index)
                index = index2;
            string filenameWithEnding = pFilepath.Substring(index + 1);
			string sourceCodeName = filenameWithEnding;
			int i = filenameWithEnding.LastIndexOf(".");
			if(i > -1) {
				sourceCodeName = filenameWithEnding.Substring(0, i);
			}
            //Console.WriteLine("source code name " + sourceCodeName);
			return sourceCodeName;
		}
	}
}

