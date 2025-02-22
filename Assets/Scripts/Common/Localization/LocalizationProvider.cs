using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace Ulko
{
    [Serializable]
    public class LocalizationTable
    {
        public string tableId;
        public LocalizedStringTable table;
        [HideInInspector]
        public StringTable tableCache;
    }

    public class LocalizationProvider : MonoBehaviour, ILocalizationProvider
    {
        public List<LocalizationTable> tables = new List<LocalizationTable>();

        public bool Initialized { get; private set; }
        public event Action LocaleChanged;

        private void Start()
        {
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        }

        private void OnDestroy()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        }

        public void Init()
        {
            foreach (var table in tables)
            {
                table.tableCache = table.table.GetTable();
            }

            Initialized = true;
            LocaleChanged?.Invoke();
        }

        private void OnLocaleChanged(Locale loc)
        {
            Init();
        }

        public string Localize(string entryKey)
        {
            foreach(var table in tables)
            {
                if(Localize(table.tableId, entryKey, out string value))
                {
                    return value;
                }
            }

            return "$"+entryKey;
        }

        public string Localize(string tableId, string entryKey)
        {
            if(Localize(tableId, entryKey, out string value))
            {
                return value;
            }

            return "$"+entryKey;
        }

        private bool Localize(string tableId, string entryKey, out string value)
        {
            value = null;

            if (string.IsNullOrEmpty(entryKey))
                return false;

            var table = tables.Find(t => t.tableId == tableId);
            if (table == null)
                return false;

            if (table.tableCache == null || table.tableCache.GetEntry(entryKey) == null)
                return false;

            if (string.IsNullOrEmpty(table.tableCache[entryKey].LocalizedValue))
                return false;

            value = table.tableCache[entryKey].LocalizedValue;
            return true;
        }
    }
}
