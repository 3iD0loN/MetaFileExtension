using System;
using System.Collections.Generic;
using System.Text.Json;

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
            // If there is an item associated with the key, then:
            if (jsonObjectMap.TryGetValue(key, out string userDataEntryJson))
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

                // Serialize the user data to JSON.
                // Associate the JSON entry with the key.
                jsonObjectMap[key] = JsonUtility.ToJson(userDataEntry);

                // Serialize the map to JSON.
                // Set the asset importer's user data.
                assetImporter.userData = JsonSerializer.Serialize(jsonObjectMap);

                // Return the new instance.
                return userDataEntry;
            }

            // Otherwise, there is no factory valid factory creation.

            // There is no item, so return a default value.
            return default;
        }

        public static void Write<T>(AssetImporter assetImporter, string key, T userDataEntry)
        {
            var jsonObjectMap = GetUserDataMap(assetImporter);

            // Serialize the user data to JSON.
            // Associate the JSON entry with the key.
            jsonObjectMap[key] = JsonUtility.ToJson(userDataEntry);

            // Serialize the map to JSON.
            // Set the asset importer's user data.
            assetImporter.userData = JsonSerializer.Serialize(jsonObjectMap);

            assetImporter.SaveAndReimport();
        }

        private static Dictionary<string, string> GetUserDataMap(AssetImporter assetImporter)
        {
            var userDataMapJson = string.IsNullOrEmpty(assetImporter.userData) ? "{}" : assetImporter.userData;

            var userDataMap = JsonSerializer.Deserialize<Dictionary<string, string>>(userDataMapJson);

            return userDataMap; // ?? new Dictionary<string, string>();
        }
        #endregion
    }
}