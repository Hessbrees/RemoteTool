using UnityEditor;
using UnityEngine;


    public enum LocationType
    {
        none,
        source,
        target
    }
    public enum EnviromentType
    {
        none,
        dev_test,
        production,
        development,
    }
    public class RemoteTool : EditorWindow
    {
        RemoteToolService remoteToolService = new();

        private bool createBackupFiles;
        private bool updateTargetSchemaList;
        private bool clearSchemasInTargetKeys;
        private bool putSchemasToTargetConfig;

        private bool moveConfigToTargetEnviroment;
        private bool removeKeysInTargetConfig;
        private bool loadBackupFileToConfig;

        private EnviromentType sourceEnviroment;
        private EnviromentType targetEnviroment;
        private string filename;
        private bool isActive;

        [MenuItem("Tools/RemoteTool")]
        public static void ShowWindow()
        {
            GetWindow(typeof(RemoteTool));
        }

        private void OnEnable()
        {
            EditorApplication.update += Update;
        }

        private void OnDisable()
        {
            EditorApplication.update -= Update;
        }

        private async void Update()
        {
            if (isActive) return;
            isActive = true;

            if (createBackupFiles)
            {
                createBackupFiles = false;
                await remoteToolService.CreateBackupFiles(sourceEnviroment, targetEnviroment);
            }

            if (updateTargetSchemaList)
            {
                updateTargetSchemaList = false;
                await remoteToolService.UpdateTargetSchemaList(sourceEnviroment, targetEnviroment);
            }

            if (clearSchemasInTargetKeys)
            {
                clearSchemasInTargetKeys = false;
                await remoteToolService.ClearSchemasInTargetKeys(sourceEnviroment, targetEnviroment);
            }

            if (putSchemasToTargetConfig)
            {
                putSchemasToTargetConfig = false;
                await remoteToolService.PutSchemasToTargetConfig(sourceEnviroment, targetEnviroment);
            }

            if (moveConfigToTargetEnviroment)
            {
                moveConfigToTargetEnviroment = false;
                await remoteToolService.MoveConfigToTargetEnviroment(sourceEnviroment, targetEnviroment);
            } 

            if (removeKeysInTargetConfig)
            {
                removeKeysInTargetConfig = false;
                await remoteToolService.RemoveKeysInTargetConfig(sourceEnviroment, targetEnviroment);
            } 
            
            if (loadBackupFileToConfig)
            {
                loadBackupFileToConfig = false;
                await remoteToolService.LoadBackupFileToConfig(sourceEnviroment, targetEnviroment ,filename);
            }
            isActive = false;
        }

        private void OnGUI()
        {
            sourceEnviroment = (EnviromentType)EditorGUILayout.EnumPopup("Source enviroment", sourceEnviroment);
            targetEnviroment = (EnviromentType)EditorGUILayout.EnumPopup("Target enviroment", targetEnviroment);

            // block default enum
            if (sourceEnviroment == EnviromentType.none || targetEnviroment == EnviromentType.none) return;

            if (GUILayout.Button("Create Backup Files"))
            {
                createBackupFiles = true;
            }

            if (GUILayout.Button("Update Target Schema List"))
            {
                updateTargetSchemaList = true;
            }

            if (GUILayout.Button("Clear Schemas In Target Keys"))
            {
                clearSchemasInTargetKeys = true;
            }

            if (GUILayout.Button("Put Config To Target Enviroment"))
            {
                moveConfigToTargetEnviroment = true;
            }

            if (GUILayout.Button("Put Schema To Target Config"))
            {
                putSchemasToTargetConfig = true;
            } 

            if (GUILayout.Button("Remove Keys In Target Config"))
            {
                removeKeysInTargetConfig = true;
            }

            if (GUILayout.Button("Load Backup File To Config"))
            {
                loadBackupFileToConfig = true;
            }

            if (GUILayout.Button("Merge To Target Enviroment"))
            {
                if (sourceEnviroment == targetEnviroment) return;

                RemoteMergeWindow window = GetWindow<RemoteMergeWindow>();
                window.Init(sourceEnviroment, targetEnviroment);
                
            }

            filename = GUILayout.TextField(filename);
        }
    }
