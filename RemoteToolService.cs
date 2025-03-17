using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unity.RemoteConfig.Editor;
using UnityEngine;
public class RemoteToolService
{
    const string cloudProjectID = "cloud_project_id";
    public Dictionary<EnviromentType, string> enviromentDict = new()
    {
        {EnviromentType.production,"environment_production_id"},
        {EnviromentType.development,"environment_development_id"},
        {EnviromentType.dev_test,"environment_dev_test_id"},
    };

    bool configLoaded = false;
    bool configTargetLoaded = false;
    bool schemaLoaded = false;

    JObject configData;
    JObject configDataTarget;
    
    // get after load configData
    public JObject ConfigData => configData;
    public JObject ConfigDataTarget => configDataTarget;

    const string backupPath = "Assets/_SOE/Features/Remote/RemoteTool/Editor/BackupFiles/";

    public async Task LoadConfigData(EnviromentType sourceEnviromentType, EnviromentType targetEnviromentType)
    {
        var sourceEnviromentID = enviromentDict[sourceEnviromentType];
        var targetEnviromentID = enviromentDict[targetEnviromentType];

        await LoadSelectedConfigs(sourceEnviromentType, sourceEnviromentID, targetEnviromentType, targetEnviromentID);
    }
    public async Task CreateBackupFiles(EnviromentType sourceEnviromentType, EnviromentType targetEnviromentType, string customName)
    {
        var sourceEnviromentID = enviromentDict[sourceEnviromentType];
        var targetEnviromentID = enviromentDict[targetEnviromentType];

        if(!configLoaded && !configTargetLoaded)
        {
            await LoadSelectedConfigs(sourceEnviromentType, sourceEnviromentID, targetEnviromentType, targetEnviromentID);
        }

        string data = System.DateTime.Now.ToShortDateString();

        string sourceFileName = "";
        string targetFileName = "";

        if (customName == "")
        {
           sourceFileName = sourceEnviromentType.ToString() + "_" + data;
           targetFileName = targetEnviromentType.ToString() + "_" + data;
        }
        else
        {
           sourceFileName = sourceEnviromentType.ToString() + "_" + customName + "_" + data;
           targetFileName = targetEnviromentType.ToString() + "_" + customName + "_" + data;
        }

        string sourcePath = backupPath + sourceFileName + ".txt";
        string targetPath = backupPath + targetFileName + ".txt";

        if (!File.Exists(sourcePath))
        {
            using (StreamWriter sw = File.CreateText(sourcePath))
            {
                sw.Write(GetLocationType(LocationType.source).ToString());
            }
        }

        if (!File.Exists(targetPath))
        {
            using (StreamWriter sw = File.CreateText(targetPath))
            {
                sw.Write(GetLocationType(LocationType.target).ToString());
            }
        }
        Debug.Log($"Create backup data file in: {backupPath}");
    }
    //create backup json file with source i target config
    public async Task CreateBackupFiles(EnviromentType sourceEnviromentType, EnviromentType targetEnviromentType)
    {
        await CreateBackupFiles(sourceEnviromentType, targetEnviromentType, "");
    }
    // get config from target and update schema list 
    public async Task UpdateTargetSchemaList(EnviromentType sourceEnviromentType, EnviromentType targetEnviromentType)
    {
        var sourceEnviromentID = enviromentDict[sourceEnviromentType];
        var targetEnviromentID = enviromentDict[targetEnviromentType];

        await LoadSelectedConfigs(sourceEnviromentType, sourceEnviromentID, targetEnviromentType, targetEnviromentID);

        UpdateSchemaList(LocationType.target,LocationType.target);

        Debug.Log(configDataTarget.ToString());

        var configID = GetEnvironmentConfigID();

        Debug.Log(configID);

        PutConfigToSelectedEnviroment(targetEnviromentID, configID, configDataTarget.Value<JArray>("value"));
    }
    // remove schemasid in all keys in target enviroment
    public async Task ClearSchemasInTargetKeys(EnviromentType sourceEnviromentType, EnviromentType targetEnviromentType)
    {
        var sourceEnviromentID = enviromentDict[sourceEnviromentType];
        var targetEnviromentID = enviromentDict[targetEnviromentType];

        await LoadSelectedConfigs(sourceEnviromentType, sourceEnviromentID, targetEnviromentType, targetEnviromentID);

        RemoveSchemasFromConfig(LocationType.target);

        var configID = GetEnvironmentConfigID();

        Debug.Log(configID);

        PutConfigToSelectedEnviroment(targetEnviromentID, configID, configDataTarget.Value<JArray>("value"));
    }
    // put schemas id in all keys from source schema list to target config files.
    // check all files in schema list, if not exist, then continue.
    public async Task PutSchemasToTargetConfig(EnviromentType sourceEnviromentType, EnviromentType targetEnviromentType)
    {
        var sourceEnviromentID = enviromentDict[sourceEnviromentType];
        var targetEnviromentID = enviromentDict[targetEnviromentType];

        await LoadSelectedConfigs(sourceEnviromentType, sourceEnviromentID, targetEnviromentType, targetEnviromentID);

        JToken schemaList = GetSchemaList();

        if (schemaList == null)
        {
            Debug.Log("schema list is null");
            return;
        }

        var configID = GetEnvironmentConfigID();

        PutSchemaToConfig(schemaList);

        PutConfigToSelectedEnviroment(targetEnviromentID, configID, configDataTarget.Value<JArray>("value"));
    }
    // move keys from one enviroment to another.
    // remove existing data in config
    // all keys are without schemaid in target enviroment
    // schema list in target enviroment is updated and ready to ovveride schema id in keys
    public async Task MoveConfigToTargetEnviroment(EnviromentType sourceEnviromentType, EnviromentType targetEnviromentType)
    {
        var sourceEnviromentID = enviromentDict[sourceEnviromentType];
        var targetEnviromentID = enviromentDict[targetEnviromentType];

        await LoadSelectedConfigs(sourceEnviromentType, sourceEnviromentID, targetEnviromentType, targetEnviromentID);

        UpdateSchemaList(LocationType.source, LocationType.target);

        RemoveSchemasFromConfig(LocationType.source);

        var configID = GetEnvironmentConfigID();

        Debug.Log(configID);

        PutConfigToSelectedEnviroment(targetEnviromentID, configID, configData.Value<JArray>("value"));
    }
    // remove existing keys in config
    public async Task RemoveKeysInTargetConfig(EnviromentType sourceEnviromentType, EnviromentType targetEnviromentType)
    {
        var sourceEnviromentID = enviromentDict[sourceEnviromentType];
        var targetEnviromentID = enviromentDict[targetEnviromentType];

        await LoadSelectedConfigs(sourceEnviromentType, sourceEnviromentID, targetEnviromentType, targetEnviromentID);

        // clear config value
        var keyConfig = GetLocationType(LocationType.target);
        ((JArray)keyConfig).Clear();

        Debug.Log("Target config keys are removed");

        var configID = GetEnvironmentConfigID();

        Debug.Log(configID);

        PutConfigToSelectedEnviroment(targetEnviromentID, configID, configDataTarget.Value<JArray>("value"));
    }

    // load file from backup to target config
    public async Task LoadBackupFileToConfig(EnviromentType sourceEnviromentType, EnviromentType targetEnviromentType,string filename)
    {
        var sourceEnviromentID = enviromentDict[sourceEnviromentType];
        var targetEnviromentID = enviromentDict[targetEnviromentType];

        await LoadSelectedConfigs(sourceEnviromentType, sourceEnviromentID, targetEnviromentType, targetEnviromentID);

        string path = backupPath + filename + ".txt";

        if (File.Exists(path))
        {
            string value = File.ReadAllText(path);

            var jtoken = JToken.Parse(value);
            
            configDataTarget["value"] = jtoken;
        }
        else
        {
            Debug.Log("file not exists");

            return;
        }

        Debug.Log("Load config from backup");

        var configID = GetEnvironmentConfigID();

        Debug.Log(configID);

        PutConfigToSelectedEnviroment(targetEnviromentID, configID, configDataTarget.Value<JArray>("value"));
    }

    public JToken GetLocationType(LocationType locationType)
    {
        if (locationType == LocationType.source)
        {
            return configData.SelectToken("value");
        }
        else if (locationType == LocationType.target)
        {
            return configDataTarget.SelectToken("value");
        }
        return null;
    }
    private void RemoveSchemasFromConfig(LocationType locationType)
    {
        var keyConfig = GetLocationType(locationType);

        foreach (var value in keyConfig)
        {
            value["schemaId"] = null;
        }

        Debug.Log("new config: \n" + configData);
    }
    private void UpdateSchemaList(LocationType from, LocationType to)
    {
        JToken fromKeyConfig = GetLocationType(from);
        JToken toKeyConfig = GetLocationType(to);

        JObject schemaList = new();

        foreach (var value in fromKeyConfig)
        {
            JProperty jProperty = new(value["key"].ToString(), value["schemaId"]);

            schemaList.Add(jProperty);
        }

        bool schemaListExist = false;

        foreach (var value in toKeyConfig)
        {
            if ((string)value["key"] == "schemaList")
            {
                schemaListExist = true;

                value["value"] = schemaList;

                Debug.Log("Update schema list \n" + value);
                break;
            }
        }

        if(!schemaListExist)
        {
            CreateSchemaList(toKeyConfig, schemaList);
        }
    }
    private string GetEnvironmentConfigID()
    {
        return (string)configDataTarget["id"];
    }
    private void CreateSchemaList(JToken keyConfig, JObject schemaList)
    {
        JProperty jProperty = new ("schemaList", schemaList);
        JObject jObject = new JObject(jProperty);
        (keyConfig as JArray).Add(jObject);

        Debug.Log(keyConfig.ToString());
    }
    private void CreateSchemaList(JObject schemaList)
    {
        var keyConfig = configDataTarget.SelectToken("value");
        keyConfig["schemaList"] = schemaList;

        Debug.Log(keyConfig.ToString());
    }
    public void PutConfigToSelectedEnviroment(JArray newConfig, EnviromentType targetEnviromentType)
    {
        var targetEnviromentID = enviromentDict[targetEnviromentType];

        PutConfigToSelectedEnviroment(targetEnviromentID, GetEnvironmentConfigID(), newConfig);
    }
    private void PutConfigToSelectedEnviroment(string enviromentID,string configID , JArray newConfig)
    {
        RemoteConfigWebApiClient.PutConfig(cloudProjectID, enviromentID, configID, newConfig);
        RemoteConfigWebApiClient.settingsRequestFinished += PutConfig;
    }

    private void GetConfigs(JObject obj)
    {
        configData = obj;

        configLoaded = true;

        RemoteConfigWebApiClient.fetchConfigsFinished -= GetConfigs;
    }
    private void GetConfigsTarget(JObject obj)
    {
        configDataTarget = obj;

        configTargetLoaded = true;

        RemoteConfigWebApiClient.fetchConfigsFinished -= GetConfigsTarget;
    }
    private void PutConfig()
    {
        Debug.Log("put config finished");
        RemoteConfigWebApiClient.settingsRequestFinished -= PutConfig;
    }
    private void PostConfig(string data)
    {
        Debug.Log("post config: " + data);
    }

    private void PutSchemaToConfig(JToken schemaList)
    {
        var keyConfig = configDataTarget.SelectToken("value");

        foreach (var key in schemaList)
        {
            JProperty property = (JProperty)key;

            foreach (var value in keyConfig)
            {
                if((string)value["key"] == property.Name)
                {
                    value["schemaId"] = property.First;
                }
            }        
        }

        Debug.Log("edited config (schema name): \n" + configDataTarget);
    }
    private JToken GetSchemaList()
    {
        var keyConfig = configData.SelectToken("value");

        JToken schemaList = null;

        // get schema list from source config
        foreach (var value in keyConfig)
        {
            if ((string)value["key"] == "schemaList")
            {
                schemaList = value["value"];
                break;
            }
        }
        return schemaList;
    }
    // fetch configs from remote 
    private async Task LoadSelectedConfigs(EnviromentType sourceEnviromentType, string sourceEnviromentID, EnviromentType targetEnviromentType, string targetEnviromentID)
    {
        configLoaded = false;
        configTargetLoaded = false;

        RemoteConfigWebApiClient.FetchConfigs(cloudProjectID, sourceEnviromentID);
        RemoteConfigWebApiClient.fetchConfigsFinished += GetConfigs;

        await TaskUtils.WaitUntil(() => configLoaded);

        RemoteConfigWebApiClient.FetchConfigs(cloudProjectID, targetEnviromentID);
        RemoteConfigWebApiClient.fetchConfigsFinished += GetConfigsTarget;

        await TaskUtils.WaitUntil(() => configTargetLoaded);

        Debug.Log($"Load config from selected enviroment {sourceEnviromentType}: \n" + configData);
    }
}