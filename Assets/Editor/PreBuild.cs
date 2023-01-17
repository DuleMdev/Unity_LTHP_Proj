using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

class PreBuild : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }
    public void OnPreprocessBuild(BuildReport report)
    {
        try
        {
            // buildTime változó frissítése
            ConfigurationController cf = GameObject.Find("ConfigurationController").GetComponent<ConfigurationController>();
            cf.buildTime = Common.TimeStamp();

            // AndroidManifest fájlban az android:host értékében az app nevének kicserélése
            // pl. smartemaths.classyedu.eu => tanlet.classyedu.eu
            string manifestFileName = Path.Combine(Application.dataPath, "Plugins/Android/AndroidManifest.xml");
            string manifest = File.ReadAllText(manifestFileName);

            //Match m = Regex.Match(manifest, "android:host=\"([^\\.]*)([^\"]*)\"");
            //Match m = Regex.Match(manifest, "android:scheme=\"([^\\.]*)([^\"]*)\"");

            //manifest = manifest.Replace(m.Groups[1].Value + m.Groups[2].Value, cf.appID.ToString().ToLower() + m.Groups[2].Value);

            Match m = Regex.Match(manifest, "android:scheme=\"([^\"]*)\"");
            manifest = manifest.Replace(m.Groups[1].Value, cf.appID.ToString().ToLower());

            File.WriteAllText(manifestFileName, manifest);
        }
        catch (Exception e)
        {
            Debug.Log("PreBuild Error - " + e.Message);
        }
        //Common.configurationController.buildTime = DateTime.Now.ToString();
    }
}