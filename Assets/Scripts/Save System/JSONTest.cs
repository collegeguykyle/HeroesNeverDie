using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;



public class JSONTest : MonoBehaviour
{
    public void Start()
    {
        Main();
    }

    void Main()
    {
        testClass1 test = new testClass1();

        JsonSerializerSettings settings = new JsonSerializerSettings();
        settings.TypeNameHandling = TypeNameHandling.All;
        NamingStrategy namingStrategy = new CamelCaseNamingStrategy(true, false);
        settings.Converters.Add(new StringEnumConverter (namingStrategy));

        var serializedObj = JsonConvert.SerializeObject(test, Newtonsoft.Json.Formatting.Indented, settings);
        

        string path = "D:\\Unity Testing\\";
        string filePath = Path.Combine(path, "saveFile.txt");

        using (StreamWriter sw = new StreamWriter(filePath))
        {
            sw.Write(serializedObj);
        }
        print("Done");
    }
}



public class testClass1
{
    
    public int num1 = 1;
    public int num2 = 2;
    public TestClass2 test2;
    public testClass1()
    {
        test2 = new TestClass2(this, "dan");
    }
}


public class TestClass2
{
    private testClass1 class1;
    public string name = "name name";


    public TestClass2(testClass1 class1, string name)
    {
        this.class1 = class1;
        this.name = name;   
    }


}