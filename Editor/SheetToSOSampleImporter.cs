using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SheetToSO
{
    // 目前只建預設的 CSV Parser，未來想改成其他 parser，改這裡
    public static class SheetToSOConfig
    {
        public static ICsvParser CreateCsvParser()
        {
            return new VisualBasicCsvParser();
        }
    }

    public static class SheetToSOSampleImporter
    {
        //replace path w/ your customize setting
        const string SettingsAssetPath = "Assets/SheetToSO/SO/SampleImportSettings.asset";

        [MenuItem("Tools/SheetToSO/Import SampleData from CSV")]
        public static void ImportSampleDataFromCsv()
        {
            var settings = AssetDatabase.LoadAssetAtPath<SampleImportSettings>(SettingsAssetPath);

            if (settings == null)
            {
                Debug.LogError($"找不到設定檔：{SettingsAssetPath}");
                return;
            }

            string initialDir = string.IsNullOrEmpty(settings.csvFolderPath)
                ? Application.dataPath : settings.csvFolderPath;

            // expect only one file
            string path = EditorUtility.OpenFilePanel("Select CSV file", initialDir, "csv");
            if (string.IsNullOrEmpty(path)) return;
                
            Debug.Log($"Accessing {path}");
            ICsvParser parser = SheetToSOConfig.CreateCsvParser();

            Dictionary<string, int> headerIndex = BuildHeaderIndex(path, settings.headerToSkip);

            ImportSampleItemCsv(path, parser, settings, headerIndex);
        }

        /// <summary>
        /// 讀第一行 header，建立 欄位名 -> index 字典
        /// </summary>
        private static Dictionary<string, int> BuildHeaderIndex(string path, int headerToSkip)
        {
            var parser = SheetToSOConfig.CreateCsvParser();
            var dict = new Dictionary<string, int>();
            
            bool first = true;
            foreach (var fields in parser.Parse(path, headerToSkip))
            {
                if (!first) break;
                first = false;

                for (int i = 0; i < fields.Length; i++)
                {
                    string name = fields[i].Trim();
                    if (!string.IsNullOrEmpty(name) && !dict.ContainsKey(name))
                    {
                        dict[name] = i;
                        Debug.Log($"Header found: {name} at index {i}");
                    } else Debug.LogWarning($"Duplicate or empty header name: {name}");
                        
                }
            }

            return dict;
        }

        private static void ImportSampleItemCsv(
            string path,
            ICsvParser parser,
            SampleImportSettings settings,
            Dictionary<string, int> headerIndex)
        {
            EnsureFolder(settings.soFolderPath);

            // Replace here with your column indices
            int idIndex   = headerIndex[settings.idColumn];
            int imgIndex  = headerIndex[settings.imgColumn];
            int nameIndex = headerIndex[settings.nameColumn];
            int descIndex = headerIndex[settings.descriptionColumn];


            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var fields in parser.Parse(path, settings.headerToSkip + 1)) //skip tags
                {
                    if (fields.Length <= descIndex)
                    {
                        Debug.LogWarning("Skipping row: 欄位數不足");
                        continue;
                    }

                    //Replace here with your column data extraction
                    string id          = fields[idIndex].Trim();
                    string imgName     = fields[imgIndex].Trim();
                    string displayName = fields[nameIndex].Trim();
                    string description = fields[descIndex].Trim();

                    if (string.IsNullOrEmpty(id))
                    {
                        Debug.LogWarning("Skipping row: ID 為空");
                        continue;
                    }

                    string assetPath = Path.Combine(settings.soFolderPath, id + ".asset")
                                            .Replace("\\", "/");

                    SampleItemData item = AssetDatabase.LoadAssetAtPath<SampleItemData>(assetPath);
                    if (item == null)
                    {
                        item = ScriptableObject.CreateInstance<SampleItemData>();
                        AssetDatabase.CreateAsset(item, assetPath);
                        Debug.Log($"Create SampleItemData: {id}");
                    }
                    else
                    {
                        Debug.Log($"Update SampleItemData: {id}");
                    }

                    //Replace here with your data assignment
                    item.id          = id;
                    item.displayName = displayName;
                    item.description = description;

                    Sprite sprite = LoadSpriteFromFolder(settings.imgFolderPaths[0], imgName);
                    if (sprite == null)
                    {
                        Debug.LogWarning($"找不到圖片：{imgName} (搜尋資料夾：{settings.imgFolderPaths[0]})");
                    }
                    else item.image = sprite;

                    EditorUtility.SetDirty(item);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            Debug.Log("SampleItemData Import completed.");
        }

        private static void EnsureFolder(string fullPath)
        {
            if (AssetDatabase.IsValidFolder(fullPath)) return;
                
            string parent = Path.GetDirectoryName(fullPath).Replace("\\", "/");
            string folder = Path.GetFileName(fullPath);

            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);

            AssetDatabase.CreateFolder(parent, folder);
        }

        private static Sprite LoadSpriteFromFolder(string folderPath, string fileName)
        {
            if (string.IsNullOrEmpty(folderPath) || string.IsNullOrEmpty(fileName))
            {
                Debug.LogWarning("LoadSpriteFromFolder: folderPath or fileName is null or empty");
                return null;
            }
                
            string nameNoExt = Path.GetFileNameWithoutExtension(fileName);
            string ext = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(ext))
                ext = ".png";

            string fullPath = Path.Combine(folderPath, nameNoExt + ext).Replace("\\", "/");
            return AssetDatabase.LoadAssetAtPath<Sprite>(fullPath);
        }
    }
}