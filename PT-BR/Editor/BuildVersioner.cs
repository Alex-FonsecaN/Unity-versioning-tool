using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Reporting;

public class BuildVersioner : EditorWindow
{
    string versionNumber, lastVersion, buildPath;
    string stageField="";

    string[] stageOptions = new string[] {"Alpha","Beta","Release Candidate","Final" };

    int major=0, minor=0, patch=0, dev=0,stageIndex=0;
    int MaCount = 0, MiCount = 0, PaCount = 0, DevCount = 0;

    bool devMode;


    //Creates the window
    [MenuItem("QA/Versionamento de Build")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(BuildVersioner));
    }
    //Happens when the window is opened
    private void OnEnable()
    {

        if (EditorPrefs.HasKey("VersionNumber"))
        {
            versionNumber = EditorPrefs.GetString("VersionNumber");
            lastVersion = versionNumber;
            CheckDev(versionNumber);
            CurrentPatchToInt(versionNumber);
        }
        if(EditorPrefs.HasKey("BuildPath"))
        {
            buildPath = EditorPrefs.GetString("BuildPath");
        }

    }
    private void OnGUI()
    {

        //Defines width for labels (Texts in front of input fields or buttons)
        EditorGUIUtility.labelWidth = 120;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(" Versão atual: " + versionNumber, GUILayout.MaxWidth(150));
        if (GUILayout.Button("Aplicar versão e atribuir ao Player Settings"))
        {
            UpdatePatch();
            PlayerSettings.bundleVersion = versionNumber;
            lastVersion = versionNumber;

        }
        EditorGUILayout.EndHorizontal();

        //Creates the Version menu with the buttons
        EditorGUILayout.BeginHorizontal();
        Labels();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        PlusButtons();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        NumberFields();

        //If dev mode is active, user will type the production stage (Ex: Alpha)
        if (devMode)
        {
            stageIndex = EditorGUILayout.Popup("Estágio de produção", stageIndex, stageOptions);

            SelectProductionStage();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        MinusButtons();
        EditorGUILayout.EndHorizontal();


        //Defines if dev mode is enabled
        devMode = EditorGUILayout.Toggle("Build Dev", devMode);

        //Checks if more than one changes were made and warns the user
        if (MaCount + MiCount + PaCount + DevCount > 1)
            EditorGUILayout.LabelField("A versão foi alterada em mais de um número, tem certeza?", GUILayout.MaxWidth(350));

        //Applies numbers to versionNumber and renames Build
        if (GUILayout.Button("Aplicar versão e renomear build"))
        {
            UpdatePatch();

            lastVersion = versionNumber;

            BuildNaming build = new BuildNaming(buildPath);
            build.NameBuild(versionNumber);

            ResetCounters();
            EditorPrefs.SetString("VersionNumber", versionNumber);

        }

        //Gets the Build Path's value
        buildPath = EditorGUILayout.TextField("Caminho da Build", buildPath);

        //If Build Path's value is not null, saves it.
        if (buildPath != null)
            EditorPrefs.SetString("BuildPath", buildPath);


        EditorGUILayout.LabelField("Ou", GUILayout.MaxWidth(40));

        //Reset interactive menu numbers back to lastVersion Variable.
        if (GUILayout.Button("Resetar números à Versão atual"))
        {
            CheckDev(lastVersion);
            CurrentPatchToInt(lastVersion);

            ResetCounters();

        }

    }
    //Reset all the Counters values to 0
    void ResetCounters()
    {
        MaCount = 0;
        MiCount = 0;
        PaCount = 0;
        DevCount = 0;
    }
    //Gets the version as a string and transforms into integer numbers, apllying to the patch int variables
    void CurrentPatchToInt(string aux)
    {
            major = int.Parse(aux.Substring(0, aux.IndexOf(".")));
            aux = aux.Substring(aux.IndexOf(".") + 1);

            minor = int.Parse(aux.Substring(0, aux.IndexOf(".")));
            aux = aux.Substring(aux.IndexOf(".") + 1);

            if (devMode)
            {
                patch = int.Parse(aux.Substring(0, aux.IndexOf(stageField)));
                aux = aux.Substring(aux.IndexOf(stageField) + stageField.Length);

                dev = int.Parse(aux.Substring(0));
            }
            else
            {
                patch = int.Parse(aux.Substring(0));
            }

    }
    //Update the variable versionNumber to the new version
    void UpdatePatch()
    {

        versionNumber = major + "." + minor + "." + patch;

        if (devMode)
            versionNumber += stageField + dev;
    }

    //Checks if the last version applied before opening the winwow, was a developer version
    void CheckDev(string str)
    {
        stageField = "";
        for (int i = 0; i < str.Length; i++)
        {
            if ((str[i] < '0' || str[i] > '9') && str[i] != '.')
            {
                devMode = true;
                stageField += str[i];
            }
        }

    }
    //Major, Minor, Patch, Dev Labels
    void Labels()
    {
        EditorGUILayout.LabelField(" Major", GUILayout.MaxWidth(40));
        EditorGUILayout.LabelField(" Minor", GUILayout.MaxWidth(40));
        EditorGUILayout.LabelField(" Patch", GUILayout.MaxWidth(40));

        if (devMode)
            EditorGUILayout.LabelField(" Dev", GUILayout.MaxWidth(40));

    }
    //All the "+" buttons
    void PlusButtons()
    {

        major = Increase(major, ref MaCount, 0);
        minor = Increase(minor, ref MiCount, 1);
        patch = Increase(patch, ref PaCount, 2);

        if (devMode)
            dev = Increase(dev, ref DevCount, 3);
    }
    // Major, Minor, Patch, Dev Number Fields
    void NumberFields()
    {
        major = EditorGUILayout.IntField(major, GUILayout.MaxWidth(40));
        minor = EditorGUILayout.IntField(minor, GUILayout.MaxWidth(40));
        patch = EditorGUILayout.IntField(patch, GUILayout.MaxWidth(40));

        if (devMode)
            dev = EditorGUILayout.IntField(dev, GUILayout.MaxWidth(40));
    }
    //All the "-" buttons
    void MinusButtons()
    {
        major = Decrease(major, ref MaCount);
        minor = Decrease(minor, ref MiCount);
        patch = Decrease(patch, ref PaCount);

        if (devMode)
            dev = Decrease(dev, ref DevCount);

    }

    //Increases the parameter num, increases it's counter and get the number index to use as a parameter for another method
    int Increase(int num, ref int cont, int index)
    {
        if (GUILayout.Button("+", GUILayout.MaxWidth(40)))
        {
            num++;
            cont++;

            ResetPatchNumbers(index);
        }
        return num;
    }

    //Decreases the parameter num and decreases it's counter
    int Decrease(int num, ref int cont)
    {
        if (GUILayout.Button("-", GUILayout.MaxWidth(40)))
        {
            if (num > 0)
            {
                num--;

                if (cont > 0)
                    cont--;
            }
        }

        return num;
    }

    //When the user press a "+" button, all the numbers on the right of that one, will be set to 0.
    void ResetPatchNumbers(int index)
    {
        for (int i = index + 1; i < 4; i++)
        {
            switch (i)
            {
                case 1:
                    minor = 0;
                    MiCount = 0;
                    break;
                case 2:
                    patch = 0;
                    PaCount = 0;
                    break;
                case 3:
                    dev = 0;
                    DevCount = 0;
                    break;

            }
        }

    }

    //Defines the production stage for dev builds..
    void SelectProductionStage()
    {
        switch (stageIndex)
        {
            case 0:
                stageField = "a";
                break;
            case 1:
                stageField = "b";
                break;
            case 2:
                stageField = "rc";
                break;
            case 3:
                stageField = "f";
                break;
        }
    }
}
public class BuildNaming
{
    string buildPath;
    string productName = PlayerSettings.productName;

    //Initial configurations for build naming
    public BuildNaming(string path)
    {
        buildPath = path;
        buildPath += @"\";

        productName = RemoveBlankSpaces(productName);
    }

    //Renames the build to the versioning pattern.
    public void NameBuild(string version)
    {
        string aux = "";
        string extension;
        FileInfo file;

        extension = GetExtension();
        string[] files = System.IO.Directory.GetFiles(buildPath, "*" + extension);


        if (files.Length == 0)
        {
            Debug.LogError("Não há builds para renomear no caminho: " + buildPath);
        }
        else
        {

            aux = files[0];
            file = new FileInfo(files[0]);

            aux = aux.Substring(aux.LastIndexOf(@"\") + 1);
            aux = aux.Substring(0, aux.IndexOf(extension));

            aux = productName + "_" + version + extension;

            file.MoveTo(buildPath + aux);

            Debug.Log("Build renomeada com sucesso para: " + file.Name);

        }

    }

    //Gets the file extension/suffix based on the current build platform
    string GetExtension()
    {
        string extension = "";
        switch (EditorUserBuildSettings.activeBuildTarget)
        {
            case BuildTarget.Android:
                extension = ".apk";
                break;
            case BuildTarget.iOS:
                extension = ".ipa";
                break;
            case BuildTarget.StandaloneWindows:
                extension = ".exe";
                break;
        }
        return extension;
    }

    //Remove blank spaces from Edit/Project Settings/Player/Product Name
    string RemoveBlankSpaces(string str)
    {
        string aux="";

        while(str.IndexOf(" ")!=-1)
        {
            aux += str.Substring(0, str.IndexOf(" "));
            str = str.Substring(str.IndexOf(" ") + 1);      
        }

        aux += str;

        return aux;
    }


}
