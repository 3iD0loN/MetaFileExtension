using System;
using System.Collections.Generic;
using Newtonsoft.Json;

using UnityEngine;
using UnityEditor;

namespace USP.MetaFileExtension
{
    public class MetaFile
    {
        #region Static Methods
        public static T Read<T>(AssetImporter assetImporter, string key, Func<AssetImporter, T> factory = null)
        {
            var jsonObjectMap = GetUserDataMap(assetImporter);

            // Attempt to get the item associated with the key.
            bool found = jsonObjectMap.TryGetValue(key, out string userDataEntryJson);

            // If there is an item associated with the key and the item is valid, then:
            if (found && !string.IsNullOrEmpty(userDataEntryJson))
            {
                // Deserialize the user data entry from JSON.
                T userDataEntry = JsonUtility.FromJson<T>(userDataEntryJson);

                return userDataEntry;
            }

            // Otherwise, there are no items associated with the key.

            // If there is a valid factory creation, then:
            if (factory != null)
            {
                // Create a new instance of the user data based off the asset importer.
                T userDataEntry = factory(assetImporter);

                ChangeEntry(jsonObjectMap, key, userDataEntry);
                WriteWithoutSave(assetImporter, jsonObjectMap);

                // Return the new instance.
                return userDataEntry;
            }

            // Otherwise, there is no factory valid factory creation.

            // There is no item, so return a default value.
            return default;
        }

        public static void Clear(AssetImporter assetImporter, string key)
        {
            var jsonObjectMap = GetUserDataMap(assetImporter);

            RemoveEntry(jsonObjectMap, key);
            WriteAndSave(assetImporter, jsonObjectMap);
        }

        public static void Write<T>(AssetImporter assetImporter, string key, T userDataEntry)
        {
            var jsonObjectMap = GetUserDataMap(assetImporter);

            ChangeEntry(jsonObjectMap, key, userDataEntry);
            WriteAndSave(assetImporter, jsonObjectMap);
        }

        private static void ChangeEntry<T>(Dictionary<string, string> jsonObjectMap, string key, T userDataEntry)
        {
            jsonObjectMap[key] = JsonUtility.ToJson(userDataEntry);
        }

        private static void RemoveEntry(Dictionary<string, string> jsonObjectMap, string key)
        {
            jsonObjectMap.Remove(key);
        }

        private static void WriteAndSave(AssetImporter assetImporter, Dictionary<string, string> jsonObjectMap)
        {
            WriteWithoutSave(assetImporter, jsonObjectMap);

            assetImporter.SaveAndReimport();
        }

        private static void WriteWithoutSave(AssetImporter assetImporter, Dictionary<string, string> jsonObjectMap)
        {
            // Serialize the map to JSON.
            // Set the asset importer's user data.
            assetImporter.userData = JsonConvert.SerializeObject(jsonObjectMap);
        }

        private static Dictionary<string, string> GetUserDataMap(AssetImporter assetImporter)
        {
            // If there is no metafile user data, then assume that the json data is an empty json map.
            // Otherwise, use the metafile user data string as-is to represent the map.
            var userDataMapJson = string.IsNullOrEmpty(assetImporter.userData) ? "{}" : assetImporter.userData;

            // TODO: how do we handle if the metafile user data does not parse to the expercted type??
            // Attempt to deserialize the map into an actual map of object instances associated by keywords.
            var userDataMap = JsonConvert.DeserializeObject<Dictionary<string, string>>(userDataMapJson);

            return userDataMap;
        }
        #endregion
    }
}

