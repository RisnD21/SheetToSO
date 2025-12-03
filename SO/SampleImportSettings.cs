using UnityEngine;

namespace SheetToSO
{
    [CreateAssetMenu(menuName = "SheetToSO/Sample Import Settings")]
    public class SampleImportSettings : ScriptableObject
    {
        [Header("CSV Settings")]
        public int headerToSkip = 1;

        [Header("Paths Settings")]
        public string csvFolderPath = "";
        public string[] imgFolderPaths;
        public string soFolderPath = "Assets/Resources/SampleItemSO";

        [Header("Column Names Settings")]
        public string idColumn          = "ID";
        public string imgColumn         = "Image";      // 第一個圖片欄位名稱
        public string nameColumn        = "Name";
        public string descriptionColumn = "Description";
    }
}
